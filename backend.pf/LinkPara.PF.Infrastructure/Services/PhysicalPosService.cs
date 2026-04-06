using AutoMapper;
using LinkPara.Audit;
using LinkPara.Audit.Models;
using LinkPara.ContextProvider;
using LinkPara.MappingExtensions.Mapping;
using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Features.PhysicalPoses;
using LinkPara.PF.Application.Features.PhysicalPoses.Command.DeletePhysicalPos;
using LinkPara.PF.Application.Features.PhysicalPoses.Command.SavePhysicalPos;
using LinkPara.PF.Application.Features.PhysicalPoses.Command.UpdatePhysicalPos;
using LinkPara.PF.Application.Features.PhysicalPoses.Queries.GetAllPhysicalPos;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Entities.PhysicalPos;
using LinkPara.PF.Domain.Enums;
using LinkPara.PF.Infrastructure.Persistence;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Transactions;

namespace LinkPara.PF.Infrastructure.Services;

public class PhysicalPosService : IPhysicalPosService
{
    private readonly ILogger<PhysicalPosService> _logger;
    private readonly IGenericRepository<Domain.Entities.PhysicalPos.PhysicalPos> _repository;
    private readonly IMapper _mapper;
    private readonly IGenericRepository<PhysicalPosCurrency> _currencyRepository;
    private readonly IGenericRepository<AcquireBank> _acquireRepository;
    private readonly IAuditLogService _auditLogService;
    private readonly IContextProvider _contextProvider;
    private readonly PfDbContext _dbContext;
    private readonly IGenericRepository<CostProfile> _costProfileRepository;
    private readonly IGenericRepository<MerchantPhysicalPos> _merchantPhysicalPosRepository;
    public PhysicalPosService(ILogger<PhysicalPosService> logger,
        IGenericRepository<Domain.Entities.PhysicalPos.PhysicalPos> repository,
        IMapper mapper,
        IGenericRepository<AcquireBank> acquireRepository,
        IAuditLogService auditLogService,
        IContextProvider contextProvider,
        PfDbContext dbContext,
        IGenericRepository<CostProfile> costProfileRepository,
        IGenericRepository<PhysicalPosCurrency> currencyRepository,
        IGenericRepository<MerchantPhysicalPos> merchantPhysicalPosRepository)
    {
        _logger = logger;
        _repository = repository;
        _mapper = mapper;
        _acquireRepository = acquireRepository;
        _auditLogService = auditLogService;
        _contextProvider = contextProvider;
        _dbContext = dbContext;
        _costProfileRepository = costProfileRepository;
        _currencyRepository = currencyRepository;
        _merchantPhysicalPosRepository = merchantPhysicalPosRepository;
    }
    public async Task DeleteAsync(DeletePhysicalPosCommand command)
    {
        var physicalPos = await _repository.GetByIdAsync(command.Id);

        var userId = _contextProvider.CurrentContext.UserId;
        var parseUserId = userId != null ? Guid.Parse(userId) : Guid.Empty;

        if (physicalPos is null)
        {
            await _auditLogService.AuditLogAsync(
                new AuditLog
                {
                    IsSuccess = false,
                    LogDate = DateTime.Now,
                    Operation = "DeletePhysicalPos",
                    SourceApplication = "PF",
                    Resource = "PhysicalPos",
                    UserId = parseUserId,
                    Details = new Dictionary<string, string>
                    {
                        {"Id", command.Id.ToString()},
                        {"ErrorMessage" , "NotFoundException"}
                    }
                });

            throw new NotFoundException(nameof(PhysicalPos), command.Id);
        }

        var merchantPhysicalPos = await _merchantPhysicalPosRepository
                            .GetAll()
                            .Where(b => b.PhysicalPosId == physicalPos.Id && b.RecordStatus == RecordStatus.Active)
                            .ToListAsync();
        if (merchantPhysicalPos.Any())
        {
            throw new AlreadyInUseException(physicalPos.Name);
        }

        try
        {
            physicalPos.VposStatus = VposStatus.Cancelled;
            physicalPos.RecordStatus = RecordStatus.Passive;

            await _repository.UpdateAsync(physicalPos);

            await _auditLogService.AuditLogAsync(
                new AuditLog
                {
                    IsSuccess = true,
                    LogDate = DateTime.Now,
                    Operation = "DeletePhysicalPos",
                    SourceApplication = "PF",
                    Resource = "PhysicalPos",
                    UserId = parseUserId,
                    Details = new Dictionary<string, string>
                    {
                        {"Id", command.Id.ToString()}
                    }
                });

        }
        catch (Exception exception)
        {
            _logger.LogError($"PhysicalPosDeleteError : {exception}");
        }
    }

    public async Task<PaginatedList<PhysicalPosDto>> GetAllAsync(GetAllPhysicalPosQuery request)
    {
        var physicalPosList = _repository.GetAll().Include(b => b.AcquireBank.Bank).AsQueryable();

        if (!string.IsNullOrEmpty(request.Q))
        {
            physicalPosList = physicalPosList.Where(b => b.Name.ToLower().Contains(request.Q.ToLower()));
        }

        if (request.BankCode is not null)
        {
            physicalPosList = physicalPosList.Where(b => b.AcquireBank.Bank.Code == request.BankCode);
        }

        if (request.VposStatus is not null)
        {
            physicalPosList = physicalPosList.Where(b => b.VposStatus == request.VposStatus);
        }

        if (request.VposType is not null)
        {
            physicalPosList = physicalPosList.Where(b => b.VposType == request.VposType);
        }

        if (request.RecordStatus is not null)
        {
            physicalPosList = physicalPosList.Where(b => b.RecordStatus == request.RecordStatus);
        }

        if (request.CreateDateStart is not null)
        {
            physicalPosList = physicalPosList.Where(b => b.CreateDate
                               >= request.CreateDateStart);
        }

        if (request.CreateDateEnd is not null)
        {
            physicalPosList = physicalPosList.Where(b => b.CreateDate
                               <= request.CreateDateEnd);
        }

        var paginatedphysicalPosList = await physicalPosList.OrderBy(b => b.AcquireBank.Bank.Name)
            .PaginatedListWithMappingAsync<Domain.Entities.PhysicalPos.PhysicalPos, PhysicalPosDto>(_mapper, request.Page, request.Size, request.OrderBy, request.SortBy);

        var hasActiveCostProfile = await _costProfileRepository.GetAll().Where(s =>
                s.RecordStatus == RecordStatus.Active &&
                s.ProfileStatus == ProfileStatus.InUse &&
                paginatedphysicalPosList.Items.Select(a => a.Id).ToList().Contains(s.PhysicalPosId.Value))
            .Select(s => s.VposId)
            .ToListAsync();

        paginatedphysicalPosList.Items.ForEach(s =>
        {
            s.HasActiveCostProfile = hasActiveCostProfile.Contains(s.Id);
        });

        return paginatedphysicalPosList;
    }

    public async Task<PhysicalPosDto> GetByIdAsync(Guid id)
    {
        var physicalPos = await _repository.GetAll()
            .Include(b => b.AcquireBank.Bank)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (physicalPos is null)
        {
            throw new NotFoundException(nameof(PhysicalPos), id);
        }

        return _mapper.Map<PhysicalPosDto>(physicalPos);
    }

    public async Task SaveAsync(SavePhysicalPosCommand command)
    {
        var activePhysicalPos = await _repository.GetAll()
           .FirstOrDefaultAsync(b => b.Name.Equals(command.Name)
           && b.AcquireBankId == command.AcquireBankId
           && b.RecordStatus == RecordStatus.Active);

        if (activePhysicalPos is not null)
        {
            throw new DuplicateRecordException(nameof(PhysicalPos), command.Name);
        }

        var acquireBank = await _acquireRepository.GetByIdAsync(command.AcquireBankId);

        var userId = _contextProvider.CurrentContext.UserId;
        var parseUserId = userId != null ? Guid.Parse(userId) : Guid.Empty;

        if (acquireBank is null)
        {
            throw new NotFoundException(nameof(AcquireBank), command.AcquireBankId);
        }

        try
        {
            var strategy = _dbContext.Database.CreateExecutionStrategy();

            await strategy.ExecuteAsync(async () =>
            {
                using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

                var physicalPos = new Domain.Entities.PhysicalPos.PhysicalPos
                {
                    AcquireBankId = command.AcquireBankId,
                    Name = command.Name,
                    VposType = command.VposType,
                    VposStatus = VposStatus.Active,
                    PfMainMerchantId = command.PfMainMerchantId,
                };

                await _repository.AddAsync(physicalPos);

                var physicalPosCurrency = new PhysicalPosCurrency
                {
                    CurrencyCode = "TRY",
                    PhysicalPosId = physicalPos.Id
                };

                await _currencyRepository.AddAsync(physicalPosCurrency);

                await _auditLogService.AuditLogAsync(
                    new AuditLog
                    {
                        IsSuccess = true,
                        LogDate = DateTime.Now,
                        Operation = "SavePhysicalPos",
                        SourceApplication = "PF",
                        Resource = "PhysicalPos",
                        UserId = parseUserId,
                        Details = new Dictionary<string, string>
                        {
                        {"Name", command.Name},
                        {"AcquireBankId", command.AcquireBankId.ToString()},
                        }
                    });
                scope.Complete();
            });
        }
        catch (Exception exception)
        {
            _logger.LogError($"PhysicalPosSaveError : {exception}");
            throw;
        }
    }

    public async Task UpdateAsync(UpdatePhysicalPosCommand command)
    {
        var physicalPos = await _repository.GetAll()
               .FirstOrDefaultAsync(b => b.Id == command.Id);

        var userId = _contextProvider.CurrentContext.UserId;
        var parseUserId = userId != null ? Guid.Parse(userId) : Guid.Empty;

        if (physicalPos is null)
        {
            throw new NotFoundException(nameof(PhysicalPos), command.Id);
        }

        var acquireBank = await _acquireRepository.GetByIdAsync(command.AcquireBankId);

        if (acquireBank is null)
        {
            throw new NotFoundException(nameof(AcquireBank), command.AcquireBankId);
        }

        try
        {

            physicalPos.AcquireBankId = command.AcquireBankId;
            physicalPos.PfMainMerchantId = command.PfMainMerchantId;
            physicalPos.Name = command.Name;
            physicalPos.VposType = command.VposType;
            physicalPos.VposStatus = VposStatus.Active;

            await _repository.UpdateAsync(physicalPos);

            await _auditLogService.AuditLogAsync(
                new AuditLog
                {
                    IsSuccess = true,
                    LogDate = DateTime.Now,
                    Operation = "UpdatePhysicalPos",
                    SourceApplication = "PF",
                    Resource = "PhysicalPos",
                    UserId = parseUserId,
                    Details = new Dictionary<string, string>
                    {
                        {"Id", command.Id.ToString()},
                        {"Name", command.Name},
                        {"AcquireBankId", command.AcquireBankId.ToString()},
                    }
                });
        }
        catch (Exception exception)
        {
            _logger.LogError($"PhysicalPosUpdateError : {exception}");
            throw;
        }
    }
}
