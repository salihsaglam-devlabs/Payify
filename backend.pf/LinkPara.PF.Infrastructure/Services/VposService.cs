using AutoMapper;
using LinkPara.Audit;
using LinkPara.Audit.Models;
using LinkPara.ContextProvider;
using LinkPara.MappingExtensions.Mapping;
using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Features.VirtualPos;
using LinkPara.PF.Application.Features.VirtualPos.Command.DeleteVpos;
using LinkPara.PF.Application.Features.VirtualPos.Command.SaveVpos;
using LinkPara.PF.Application.Features.VirtualPos.Command.UpdateVpos;
using LinkPara.PF.Application.Features.VirtualPos.Queries.GetFilterVpos;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;
using LinkPara.PF.Infrastructure.Persistence;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Transactions;
using LinkPara.PF.Application.Commons.Exceptions;
using LinkPara.PF.Application.Commons.Models.VposModels.Request;
using LinkPara.PF.Infrastructure.Services.VposServices;

namespace LinkPara.PF.Infrastructure.Services;

public class VposService : IVposService
{
    private readonly ILogger<VposService> _logger;
    private readonly IGenericRepository<Vpos> _repository;
    private readonly IMapper _mapper;
    private readonly IGenericRepository<VposCurrency> _currencyRepository;
    private readonly IGenericRepository<AcquireBank> _acquireRepository;
    private readonly IGenericRepository<VposBankApiInfo> _apiRepository;
    private readonly IAuditLogService _auditLogService;
    private readonly IContextProvider _contextProvider;
    private readonly PfDbContext _dbContext;
    private readonly IGenericRepository<CostProfile> _costProfileRepository;

    public VposService(ILogger<VposService> logger, IGenericRepository<Vpos> repository,
        IMapper mapper,
        IGenericRepository<VposCurrency> currencyRepository,
        IGenericRepository<AcquireBank> acquireRepository,
        IGenericRepository<VposBankApiInfo> apiRepository,
        IAuditLogService auditLogService,
        IContextProvider contextProvider,
        PfDbContext dbContext,
        IGenericRepository<CostProfile> costProfileRepository)
    {
        _logger = logger;
        _repository = repository;
        _mapper = mapper;
        _currencyRepository = currencyRepository;
        _acquireRepository = acquireRepository;
        _apiRepository = apiRepository;
        _auditLogService = auditLogService;
        _contextProvider = contextProvider;
        _dbContext = dbContext;
        _costProfileRepository = costProfileRepository;
    }

    public async Task DeleteAsync(DeleteVposCommand command)
    {
        var vpos = await _repository.GetByIdAsync(command.Id);

        var userId = _contextProvider.CurrentContext.UserId;
        var parseUserId = userId != null ? Guid.Parse(userId) : Guid.Empty;

        if (vpos is null)
        {
            await _auditLogService.AuditLogAsync(
                new AuditLog
                {
                    IsSuccess = false,
                    LogDate = DateTime.Now,
                    Operation = "DeleteVpos",
                    SourceApplication = "PF",
                    Resource = "Vpos",
                    UserId = parseUserId,
                    Details = new Dictionary<string, string>
                    {
                        {"Id", command.Id.ToString()},
                        {"ErrorMessage" , "NotFoundException"}
                    }
                });

            throw new NotFoundException(nameof(Vpos), command.Id);
        }

        try
        {
            vpos.VposStatus = VposStatus.Cancelled;
            vpos.RecordStatus = RecordStatus.Passive;

            await _repository.UpdateAsync(vpos);

            await _auditLogService.AuditLogAsync(
                new AuditLog
                {
                    IsSuccess = true,
                    LogDate = DateTime.Now,
                    Operation = "DeleteVpos",
                    SourceApplication = "PF",
                    Resource = "Vpos",
                    UserId = parseUserId,
                    Details = new Dictionary<string, string>
                    {
                        {"Id", command.Id.ToString()}
                    }
                });

        }
        catch (Exception exception)
        {
            _logger.LogError($"VposDeleteError : {exception}");
        }
    }

    public async Task<VposDto> GetByIdAsync(Guid id)
    {
        var vpos = await _repository.GetAll().Include(b => b.VposBankApiInfos
            .Where(b => b.RecordStatus == RecordStatus.Active && b.Key.RecordStatus == RecordStatus.Active))
            .ThenInclude(c => c.Key)
            .Include(b => b.AcquireBank.Bank)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (vpos is null)
        {
            throw new NotFoundException(nameof(Vpos), id);
        }

        return _mapper.Map<VposDto>(vpos);
    }

    public async Task<PaginatedList<VposDto>> GetFilterListAsync(GetFilterVposQuery request)
    {
        var vposList = _repository.GetAll().Include(b => b.AcquireBank.Bank)
            .Include(b => b.VposBankApiInfos.Where(b => b.RecordStatus == RecordStatus.Active)).AsQueryable();

        if (!string.IsNullOrEmpty(request.Q))
        {
            vposList = vposList.Where(b => b.Name.ToLower().Contains(request.Q.ToLower()));
        }

        if (request.BankCode is not null)
        {
            vposList = vposList.Where(b => b.AcquireBank.Bank.Code == request.BankCode);
        }

        if (request.VposStatus is not null)
        {
            vposList = vposList.Where(b => b.VposStatus == request.VposStatus);
        }

        if (request.VposType is not null)
        {
            vposList = vposList.Where(b => b.VposType == request.VposType);
        }

        if (request.RecordStatus is not null)
        {
            vposList = vposList.Where(b => b.RecordStatus == request.RecordStatus);
        }

        if (request.CreateDateStart is not null)
        {
            vposList = vposList.Where(b => b.CreateDate
                               >= request.CreateDateStart);
        }

        if (request.CreateDateEnd is not null)
        {
            vposList = vposList.Where(b => b.CreateDate
                               <= request.CreateDateEnd);
        }

        if (request.IsInsuranceVpos is not null)
        {
            vposList = vposList.Where(b => b.IsInsuranceVpos == request.IsInsuranceVpos);
        }
        
        if (request.IsTopUpVpos is not null)
        {
            vposList = vposList.Where(b => b.IsTopUpVpos == request.IsTopUpVpos);
        }
        
        if (request.IsOnUsVpos is not null)
        {
            vposList = vposList.Where(b => b.IsOnUsVpos == request.IsOnUsVpos);
        }

        var paginatedVposList = await vposList.OrderBy(b => b.AcquireBank.Bank.Name)
            .PaginatedListWithMappingAsync<Vpos, VposDto>(_mapper, request.Page, request.Size, request.OrderBy, request.SortBy);

        var hasActiveCostProfile = await _costProfileRepository.GetAll().Where(s =>
                s.RecordStatus == RecordStatus.Active &&
                s.ProfileStatus == ProfileStatus.InUse &&
                paginatedVposList.Items.Select(a => a.Id).ToList().Contains(s.VposId.Value))
            .Select(s => s.VposId)
            .ToListAsync();

        paginatedVposList.Items.ForEach(s =>
        {
            s.HasActiveCostProfile = hasActiveCostProfile.Contains(s.Id);
        });

        return paginatedVposList;
    }

    public async Task SaveAsync(SaveVposCommand command)
    {
        var activeVpos = await _repository.GetAll()
            .FirstOrDefaultAsync(b => b.Name.Equals(command.Name)
            && b.AcquireBankId == command.AcquireBankId
            && b.SecurityType == command.SecurityType
            && b.RecordStatus == RecordStatus.Active);

        if (activeVpos is not null)
        {
            throw new DuplicateRecordException(nameof(Vpos), command.Name);
        }

        if (command.IsOnUsVpos)
        {
            var activeOnUsVpos = await _repository.GetAll()
                .AnyAsync(s => s.IsOnUsVpos == true && s.RecordStatus == RecordStatus.Active);

            if (activeOnUsVpos)
            {
                throw new OnUsVposAlreadyActiveException();
            }
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

                var vposBankApiInfoKeys = _mapper.Map<List<VposBankApiInfo>>(command.VposBankApiInfos);

                var vpos = new Vpos
                {
                    AcquireBankId = command.AcquireBankId,
                    SecurityType = command.SecurityType,
                    Name = command.Name,
                    VposType = command.VposType,
                    VposStatus = VposStatus.Active,
                    BlockageCode = command.BlockageCode,
                    IsOnUsVpos = command.IsOnUsVpos,
                    IsInsuranceVpos = command.IsInsuranceVpos,
                    IsTopUpVpos = command.IsTopUpVpos,
                    VposBankApiInfos = vposBankApiInfoKeys,
                };

                await _repository.AddAsync(vpos);

                var vposCurrency = new VposCurrency
                {
                    CurrencyCode = "TRY",
                    VposId = vpos.Id
                };

                await _currencyRepository.AddAsync(vposCurrency);

                await _auditLogService.AuditLogAsync(
                    new AuditLog
                    {
                        IsSuccess = true,
                        LogDate = DateTime.Now,
                        Operation = "SaveVpos",
                        SourceApplication = "PF",
                        Resource = "Vpos",
                        UserId = parseUserId,
                        Details = new Dictionary<string, string>
                        {
                        {"Name", command.Name},
                        {"AcquireBankId", command.AcquireBankId.ToString()},
                        {"SecurityType", command.SecurityType.ToString()}
                        }
                    });
                scope.Complete();
            });
        }
        catch (Exception exception)
        {
            _logger.LogError($"VposSaveError : {exception}");
            throw;
        }

    }

    public async Task UpdateAsync(UpdateVposCommand command)
    {
        var vpos = await _repository.GetAll().Include(b => b.VposBankApiInfos)
                .FirstOrDefaultAsync(b => b.Id == command.Id);

        var userId = _contextProvider.CurrentContext.UserId;
        var parseUserId = userId != null ? Guid.Parse(userId) : Guid.Empty;

        if (vpos is null)
        {
            throw new NotFoundException(nameof(Vpos), command.Id);
        }

        var acquireBank = await _acquireRepository.GetByIdAsync(command.AcquireBankId);

        if (acquireBank is null)
        {
            throw new NotFoundException(nameof(AcquireBank), command.AcquireBankId);
        }
        
        if (command.IsOnUsVpos)
        {
            var activeOnUsVpos = await _repository.GetAll()
                .AnyAsync(s => s.IsOnUsVpos == true && s.RecordStatus == RecordStatus.Active && s.Id != vpos.Id);
            if (activeOnUsVpos)
            {
                throw new OnUsVposAlreadyActiveException();
            }
        }

        try
        {
            if (vpos.VposBankApiInfos.Any())
            {
                await _apiRepository.RemoveRangeAsync(vpos.VposBankApiInfos);
            }

            var vposBankApiInfo = command.VposBankApiInfos.Select(b => new VposBankApiInfo
            {
                KeyId = b.KeyId,
                Value = b.Value,
                VposId = vpos.Id

            }).ToList();

            await _apiRepository.AddRangeAsync(vposBankApiInfo);

            vpos.AcquireBankId = command.AcquireBankId;
            vpos.SecurityType = command.SecurityType;
            vpos.Name = command.Name;
            vpos.VposType = command.VposType;
            vpos.BlockageCode = command.BlockageCode;
            vpos.IsOnUsVpos = command.IsOnUsVpos;
            vpos.IsTopUpVpos = command.IsTopUpVpos;
            vpos.IsInsuranceVpos = command.IsInsuranceVpos;
            vpos.VposStatus = VposStatus.Active;

            await _repository.UpdateAsync(vpos);

            await _auditLogService.AuditLogAsync(
                new AuditLog
                {
                    IsSuccess = true,
                    LogDate = DateTime.Now,
                    Operation = "UpdateVpos",
                    SourceApplication = "PF",
                    Resource = "Vpos",
                    UserId = parseUserId,
                    Details = new Dictionary<string, string>
                    {
                        {"Id", command.Id.ToString()},
                        {"Name", command.Name},
                        {"AcquireBankId", command.AcquireBankId.ToString()},
                        {"SecurityType", command.SecurityType.ToString()}
                    }
                });
        }
        catch (Exception exception)
        {
            _logger.LogError($"VposUpdateError : {exception}");
            throw;
        }

    }
}
