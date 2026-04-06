using AutoMapper;
using LinkPara.Audit;
using LinkPara.Audit.Models;
using LinkPara.ContextProvider;
using LinkPara.PF.Application.Commons.Exceptions;
using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Commons.Models.CostProfiles;
using LinkPara.PF.Application.Features.VirtualPos;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LinkPara.PF.Application.Features.CostProfiles.Command.PatchCostProfile;

public class PatchCostProfileCommand : IRequest<UpdateCostProfileRequest>
{
    public Guid Id { get; set; }
    public JsonPatchDocument<UpdateCostProfileRequest> CostProfile { get; set; }
}

public class PatchCostProfileCommandHandler : IRequestHandler<PatchCostProfileCommand, UpdateCostProfileRequest>
{
    private readonly IGenericRepository<CostProfile> _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<CostProfile> _logger;
    private readonly ICostProfileService _costProfileService;
    private readonly IAuditLogService _auditLogService;
    private readonly IContextProvider _contextProvider;
    private readonly IGenericRepository<Vpos> _vposRepository;
    private readonly IGenericRepository<Domain.Entities.PhysicalPos.PhysicalPos> _physicalPosRepository;

    public PatchCostProfileCommandHandler(IGenericRepository<CostProfile> repository, IMapper mapper,
        ILogger<CostProfile> logger, ICostProfileService costProfileService,
        IAuditLogService auditLogService,
        IContextProvider contextProvider,
        IGenericRepository<Vpos> vposRepository, IGenericRepository<Domain.Entities.PhysicalPos.PhysicalPos> physicalPosRepository)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
        _costProfileService = costProfileService;
        _auditLogService = auditLogService;
        _contextProvider = contextProvider;
        _vposRepository = vposRepository;
        _physicalPosRepository = physicalPosRepository;
    }
    public async Task<UpdateCostProfileRequest> Handle(PatchCostProfileCommand request, CancellationToken cancellationToken)
    {
        var costProfile = await _repository.GetAll()
            .Include(b => b.CostProfileItems
                .OrderByDescending(b => b.CardTransactionType)
                .ThenByDescending(b=> b.ProfileCardType)
                .ThenBy(b=> b.InstallmentNumberEnd))
            .ThenInclude(a => a.CostProfileInstallments)
            .FirstOrDefaultAsync(b => b.Id == request.Id, cancellationToken);

        if (costProfile is null)
        {
            throw new NotFoundException(nameof(CostProfile), request.Id);
        }

        try
        {
            var oldActivationDate = costProfile.ActivationDate;

            var costProfileMap = _mapper.Map<UpdateCostProfileRequest>(costProfile);

            request.CostProfile.ApplyTo(costProfileMap);

            _mapper.Map(costProfileMap, costProfile);
            
            var items = _mapper.Map<List<CostProfileItemDto>>(costProfile.CostProfileItems);
            _costProfileService.ValidateInstallment(items, costProfile.PosType, costProfile.ProfileSettlementMode);

            if (costProfile.PosType == PosType.Physical)
            {
                if (costProfile.PhysicalPosId == null)
                {
                    throw new NotFoundException(nameof(PhysicalPos));
                }
            }

            if (costProfile.PosType == PosType.Virtual)
            {
                if (costProfile.VposId == null)
                {
                    throw new NotFoundException(nameof(Vpos));
                }
            }

            if (costProfile.RecordStatus == RecordStatus.Active)
            {
                if (costProfile.ActivationDate < DateTime.Now)
                {
                    throw new InvalidActivationDateException();
                }

                var costProfiles = _repository.GetAll()
                    .Where(b => b.Id != request.Id);

                if (costProfile.PosType == PosType.Virtual)
                {
                    costProfiles = costProfiles
                    .Where(b => b.VposId == costProfile.VposId);
                }
                else if (costProfile.PosType == PosType.Physical)
                {
                    costProfiles = costProfiles
                    .Where(b => b.PhysicalPosId == costProfile.PhysicalPosId);
                }

                if (costProfiles.Any())
                {
                    foreach (var item in costProfiles)
                    {
                        if (item.ActivationDate == costProfile.ActivationDate)
                        {
                            throw new DuplicateRecordException(nameof(CostProfile), nameof(costProfile.ActivationDate));
                        }
                    }
                }

                if (costProfile.ProfileStatus == ProfileStatus.InUse && costProfile.ActivationDate > oldActivationDate)
                {
                    await NewCostProfileAsync(costProfile, items);
                    return costProfileMap;
                }

                if (costProfile.ProfileStatus == ProfileStatus.Deleted)
                {
                    costProfile.ProfileStatus = ProfileStatus.Waiting;
                }
                else
                {
                    if (costProfile.ProfileStatus != ProfileStatus.Waiting)
                    {
                        costProfile.ProfileStatus = ProfileStatus.InUse;
                    }
                }

            }

            if (costProfile.RecordStatus == RecordStatus.Passive)
            {
                if (costProfile.ProfileStatus == ProfileStatus.InUse)
                {
                    if (costProfile.PosType  == PosType.Virtual)
                    {
                        var vpos = await _vposRepository
                                                   .GetAll()
                                                   .FirstOrDefaultAsync(b => b.Id == costProfile.VposId);

                        if (vpos is not null && vpos.VposStatus == VposStatus.Active)
                        {
                            throw new CostProfileHasActivePosException();
                        }
                    }
                    else if (costProfile.PosType == PosType.Physical)
                    {
                        var physicalPos = await _physicalPosRepository
                                                   .GetAll()
                                                   .FirstOrDefaultAsync(b => b.Id == costProfile.PhysicalPosId);

                        if (physicalPos is not null && physicalPos.VposStatus == VposStatus.Active)
                        {
                            throw new CostProfileHasActivePosException();
                        }
                    }
                       
                }
                costProfile.ProfileStatus = ProfileStatus.Deleted;
            }

            await _costProfileService.PatchCostProfile(costProfile);

            return costProfileMap;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "CostProfilePatchError : {Exception}", exception);
            throw;
        }
    }
    private async Task NewCostProfileAsync(CostProfile costProfile, List<CostProfileItemDto> itemsDto)
    {
        var userId = _contextProvider.CurrentContext.UserId;
        var parseUserId = userId != null ? Guid.Parse(userId) : Guid.Empty;

        var items = _mapper.Map<List<CostProfileItem>>(itemsDto);
        items.ForEach(b =>
        {
            b.Id = Guid.Empty;
            b.CostProfileId = Guid.Empty;
            b.CostProfileInstallments.ForEach(s =>
            {
                s.Id = Guid.Empty;
                s.CostProfileItemId = Guid.Empty;
            });
        });

        var newCostProfile = new CostProfile
        {
            Name = costProfile.Name,
            ActivationDate = costProfile.ActivationDate,
            PointCommission = costProfile.PointCommission,
            ServiceCommission = costProfile.ServiceCommission,
            ProfileStatus = ProfileStatus.Waiting,
            CurrencyCode = "TRY",
            CostProfileItems = items,
            VposId = costProfile.VposId,
            PhysicalPosId = costProfile.PhysicalPosId,
            PosType = costProfile.PosType,
            ProfileSettlementMode = costProfile.ProfileSettlementMode
        };

        await _repository.AddAsync(newCostProfile);

        await _auditLogService.AuditLogAsync(
       new AuditLog
       {
           IsSuccess = true,
           LogDate = DateTime.Now,
           Operation = "PatchCostProfile",
           SourceApplication = "PF",
           Resource = "CostProfile",
           UserId = parseUserId,
           Details = new Dictionary<string, string>
           {
                 {"VposId", costProfile.VposId.ToString()},
                 {"Name", costProfile.Name},
                 {"ActivationDate", costProfile.ActivationDate.ToString()},
                 {"PointCommission", costProfile.PointCommission.ToString()},
                 {"ServiceCommission", costProfile.ServiceCommission.ToString()}
           }
       });
    }
}
