using AutoMapper;
using LinkPara.Audit;
using LinkPara.Audit.Models;
using LinkPara.ContextProvider;
using LinkPara.MappingExtensions.Mapping;
using LinkPara.PF.Application.Commons.Exceptions;
using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Features.CostProfiles;
using LinkPara.PF.Application.Features.CostProfiles.Command.SaveCostProfile;
using LinkPara.PF.Application.Features.CostProfiles.Queries.GetCostProfileById;
using LinkPara.PF.Application.Features.CostProfiles.Queries.GetFilterCostProfile;
using LinkPara.PF.Application.Features.VirtualPos;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;
using LinkPara.PF.Infrastructure.Persistence;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LinkPara.PF.Infrastructure.Services;

public class CostProfileService : ICostProfileService
{
    private readonly IGenericRepository<CostProfile> _repository;
    private readonly ILogger<CostProfileService> _logger;
    private readonly IMapper _mapper;
    private readonly IAuditLogService _auditLogService;
    private readonly IContextProvider _contextProvider;
    private readonly IGenericRepository<Vpos> _vposRepository;
    private readonly IGenericRepository<Domain.Entities.PhysicalPos.PhysicalPos> _physicalPosRepository;
    private readonly PfDbContext _context;

    public CostProfileService(IGenericRepository<CostProfile> repository,
        ILogger<CostProfileService> logger,
        IMapper mapper,
        IAuditLogService auditLogService,
        IContextProvider contextProvider,
        IGenericRepository<Vpos> vposRepository,
        PfDbContext context,
        IGenericRepository<Domain.Entities.PhysicalPos.PhysicalPos> physicalPosRepository)
    {
        _repository = repository;
        _logger = logger;
        _mapper = mapper;
        _auditLogService = auditLogService;
        _contextProvider = contextProvider;
        _vposRepository = vposRepository;
        _context = context;
        _physicalPosRepository = physicalPosRepository;
    }

    public async Task<CostProfilesDto> GetByIdAsync(GetCostProfileByIdQuery request)
    {
        var posType = await _repository.GetAll()
            .Where(b => b.Id == request.Id)
            .Select(b => (PosType?)b.PosType)
            .FirstOrDefaultAsync();
        
        if (posType is null)
        {
            throw new NotFoundException(nameof(CostProfile), request.Id);
        }
        
        var query = _repository.GetAll()
            .Include(b => b.CostProfileItems
                .OrderByDescending(b => b.CardTransactionType)
                .ThenByDescending(b => b.ProfileCardType)
                .ThenBy(b => b.InstallmentNumberEnd)).ThenInclude(a => a.CostProfileInstallments).AsQueryable();

        if (posType == PosType.Virtual)
        {
            query = query
                .Include(b => b.Vpos)
                .ThenInclude(v => v.AcquireBank.Bank);
        }
        else if (posType == PosType.Physical)
        {
            query = query
                .Include(b => b.PhysicalPos)
                .ThenInclude(p => p.AcquireBank.Bank);
        }

        var costProfile = await query.FirstOrDefaultAsync(b => b.Id == request.Id);
        return _mapper.Map<CostProfilesDto>(costProfile);
    }

    public async Task<PaginatedList<CostProfilesDto>> GetFilterListAsync(GetFilterCostProfileQuery request)
    {
        var posType = request.PosType ?? PosType.Virtual;
        IQueryable<CostProfile> costProfileList;
        
        if (posType == PosType.Virtual)
        {
            costProfileList = _repository.GetAll()
                .Include(b => b.Vpos)
                .ThenInclude(v => v.AcquireBank)
                .ThenInclude(a => a.Bank)
                .Where(b => b.PosType == PosType.Virtual);
        }
        else
        {
            costProfileList = _repository.GetAll()
                .Include(b => b.PhysicalPos)
                .ThenInclude(p => p.AcquireBank)
                .ThenInclude(a => a.Bank)
                .Where(b => b.PosType == PosType.Physical);
        }

        if (!string.IsNullOrEmpty(request.Q))
        {
            costProfileList = costProfileList.Where(b => b.Name.ToLower().Contains(request.Q.ToLower()));
        }

        if (request.ProfileStatus is not null)
        {
            costProfileList = costProfileList.Where(b => b.ProfileStatus
                               == request.ProfileStatus);
        }

        if (request.ProfileSettlementMode is not null)
        {
            costProfileList = costProfileList.Where(b => b.ProfileSettlementMode
                               == request.ProfileSettlementMode);
        }

        if (request.CreateDateStart is not null)
        {
            costProfileList = costProfileList.Where(b => b.CreateDate
                               >= request.CreateDateStart);
        }

        if (request.CreateDateEnd is not null)
        {
            costProfileList = costProfileList.Where(b => b.CreateDate
                               <= request.CreateDateEnd);
        }

        if (request.BankCode is not null)
        {
            costProfileList = 
                posType == PosType.Virtual ? 
                    costProfileList.Where(b => b.Vpos.AcquireBank.Bank.Code == request.BankCode) : 
                    costProfileList.Where(b => b.PhysicalPos.AcquireBank.Bank.Code == request.BankCode);
        }

        if (request.VposId is not null)
        {
            costProfileList = costProfileList.Where(b => b.Vpos.Id == request.VposId);
        }

        if (request.PhysicalPosId is not null)
        {
            costProfileList = costProfileList.Where(b => b.PhysicalPos.Id == request.PhysicalPosId);
        }
        
        var orderBy = posType == PosType.Virtual
            ? costProfileList.OrderBy(b => b.Vpos.AcquireBank.Bank.Name)
            : costProfileList.OrderBy(b => b.PhysicalPos.AcquireBank.Bank.Name);

        return await orderBy 
                   .PaginatedListWithMappingAsync<CostProfile, CostProfilesDto>(_mapper, request.Page, request.Size, request.OrderBy, request.SortBy);
    }

    public async Task SaveAsync(SaveCostProfileCommand command)
    {
        if (command.PosType == PosType.Virtual)
        {
            var vpos = await _vposRepository.GetAll()
                              .FirstOrDefaultAsync(b => b.Id == command.VposId);

            if (vpos is null)
            {
                throw new NotFoundException(nameof(Vpos), command.VposId);
            }
        }
        else if (command.PosType == PosType.Physical)
        {
            var physicalPos = await _physicalPosRepository.GetAll()
                  .FirstOrDefaultAsync(b => b.Id == command.PhysicalPosId);

            if (physicalPos is null)
            {
                throw new NotFoundException(nameof(PhysicalPos), command.PhysicalPosId);
            }
        }

        var userId = _contextProvider.CurrentContext.UserId;
        var parseUserId = userId != null ? Guid.Parse(userId) : Guid.Empty;

        ValidateInstallment(command.CostProfileItems, command.PosType, command.ProfileSettlementMode);
        if (command.ProfileSettlementMode == ProfileSettlementMode.SingleBlock)
        {
            command.CostProfileItems.ForEach(s =>
            {
                s.CostProfileInstallments = new List<CostProfileInstallmentDto>();
            });
        }

        await ValidateActivationDateAsync(command);

        var items = _mapper.Map<List<CostProfileItem>>(command.CostProfileItems);
        try
        {
            var costProfile = new CostProfile
            {
                Name = command.Name,
                ActivationDate = command.ActivationDate,
                PointCommission = command.PointCommission,
                ServiceCommission = command.ServiceCommission,
                ProfileStatus = ProfileStatus.Waiting,
                PosType = command.PosType,
                ProfileSettlementMode = command.ProfileSettlementMode,
                CurrencyCode = "TRY",
                CostProfileItems = items,
                VposId = command.VposId,
                PhysicalPosId = command.PhysicalPosId
            };

            await _repository.AddAsync(costProfile);

            await _auditLogService.AuditLogAsync(
                new AuditLog
                {
                    IsSuccess = true,
                    LogDate = DateTime.Now,
                    Operation = "SaveCostProfile",
                    SourceApplication = "PF",
                    Resource = "CostProfile",
                    UserId = parseUserId,
                    Details = new Dictionary<string, string>
                    {
                        {"VposId", command.VposId.ToString()},
                        {"PhysicalPosId", command.PhysicalPosId.ToString()},
                        {"Name", command.Name},
                        {"ActivationDate", command.ActivationDate.ToString()},
                        {"PointCommission", command.PointCommission.ToString()},
                        {"ServiceCommission", command.ServiceCommission.ToString()}
                    }
                });
        }
        catch (Exception exception)
        {
            _logger.LogError($"CostProfileSaveError : {exception}");
            throw;
        }
    }
    public async Task PatchCostProfile(CostProfile command)
    {
        _context.CostProfile.Update(command);

        await _context.SaveChangesAsync();
    }
    private async Task ValidateActivationDateAsync(SaveCostProfileCommand command)
    {
        var costProfiles = _repository.GetAll();

        if (command.PosType == PosType.Virtual)
        {
            costProfiles = costProfiles.Where(b => b.VposId == command.VposId);
        }
        else if (command.PosType == PosType.Physical)
        {
            costProfiles = costProfiles.Where(b => b.PhysicalPosId == command.PhysicalPosId);
        }

        if (costProfiles.Any())
        {
            foreach (var item in costProfiles)
            {
                if (item.ActivationDate == command.ActivationDate)
                {
                    throw new DuplicateRecordException(nameof(CostProfile), nameof(command.ActivationDate));
                }
            }
        }
    }

    public void ValidateInstallment(List<CostProfileItemDto> costProfileItems, PosType posType, ProfileSettlementMode settlementMode)
    {
        if (costProfileItems.All(b => !b.IsActive) || 
            posType == PosType.Physical && !costProfileItems.Any(b => b.ProfileCardType == ProfileCardType.International && b.IsActive))
        {
            throw new InvalidInstallmentException();
        }

        if (settlementMode != ProfileSettlementMode.PerInstallment)
        {
            return;
        }
        
        var activeInstallments = costProfileItems.Where(b => b.IsActive && b.InstallmentNumberEnd > 0).ToList();
        if (
            (
                from installment in activeInstallments 
                let sequences = installment.CostProfileInstallments.Select(s => s.InstallmentSequence).OrderBy(x => x).ToList() 
                where !sequences.SequenceEqual(Enumerable.Range(1, installment.InstallmentNumberEnd - 1)) 
                select installment
            )
            .Any())
        {
            throw new InvalidInstallmentException();
        }
    }
}
