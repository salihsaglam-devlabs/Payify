using AutoMapper;
using LinkPara.Audit;
using LinkPara.Audit.Models;
using LinkPara.PF.Application.Commons.Exceptions;
using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Commons.Models.PricingProfiles;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LinkPara.PF.Application.Features.PricingProfiles.Command.PatchPricingProfile;

public class PatchPricingProfileCommand : IRequest<UpdatePricingProfileRequest>
{
    public Guid Id { get; set; }
    public JsonPatchDocument<UpdatePricingProfileRequest> PricingProfile { get; set; }
}

public class PatchPricingProfileCommandHandler : IRequestHandler<PatchPricingProfileCommand, UpdatePricingProfileRequest>
{
    private readonly IGenericRepository<PricingProfile> _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<PricingProfile> _logger;
    private readonly IGenericRepository<Merchant> _merchantRepository;
    private readonly IAuditLogService _auditLogService;
    private readonly IPricingProfileService _pricingProfileService;

    public PatchPricingProfileCommandHandler(IGenericRepository<PricingProfile> repository, 
        IMapper mapper,
        ILogger<PricingProfile> logger,
        IGenericRepository<Merchant> merchantRepository,
        IAuditLogService auditLogService, 
        IPricingProfileService pricingProfileService)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
        _merchantRepository = merchantRepository;
        _auditLogService= auditLogService;
        _pricingProfileService = pricingProfileService;
    }
    public async Task<UpdatePricingProfileRequest> Handle(PatchPricingProfileCommand request, CancellationToken cancellationToken)
    {
        var pricingProfile = await _repository.GetAll()
                         .Include(b => b.Currency)
                         .Include(s => s.PricingProfileItems)
                         .ThenInclude(a => a.PricingProfileInstallments)
                         .FirstOrDefaultAsync(b => b.Id == request.Id, cancellationToken);

        if (pricingProfile is null)
        {
            throw new NotFoundException(nameof(PricingProfile), request.Id);
        }

        try
        {
            var pricingProfileMap = _mapper.Map<UpdatePricingProfileRequest>(pricingProfile);
            request.PricingProfile.ApplyTo(pricingProfileMap);
            _mapper.Map(pricingProfileMap, pricingProfile);
            
            var items = _mapper.Map<List<PricingProfileItemDto>>(pricingProfile.PricingProfileItems);
            _pricingProfileService.ValidateInstallment(items);
            
            if (pricingProfile.RecordStatus == RecordStatus.Passive)
            {
                var merchants = await _merchantRepository.GetAll()
                    .Where(b => b.PricingProfileNumber == pricingProfile.PricingProfileNumber 
                    && b.RecordStatus == RecordStatus.Active).ToListAsync(cancellationToken);

                if (merchants.Any())
                {
                    throw new AlreadyInUseException(nameof(PricingProfile));
                }

                pricingProfile.ProfileStatus = ProfileStatus.Deleted;
            }
            else
            {
                if (pricingProfile.ActivationDate <= DateTime.Now)
                {
                    throw new InvalidActivationDateException();
                }
                pricingProfile.ProfileStatus = ProfileStatus.Waiting;
            }
            
            if (pricingProfile.ProfileType == ProfileType.Standard &&
                pricingProfile.PricingProfileItems.Any(s => s.ParentMerchantCommissionRate > 0))
            {
                throw new ParentMerchantCommissionMustBeZeroException();
            }

            await _repository.UpdateAsync(pricingProfile);

            await _auditLogService.AuditLogAsync(
            new AuditLog
            {
                IsSuccess = true,
                LogDate = DateTime.Now,
                Operation = "UpdatePricingProfile",
                SourceApplication = "PF",
                Resource = "PricingProfile",
                Details = new Dictionary<string, string>
                {
                       {"Id", pricingProfile.Id.ToString() },
                       {"Name", pricingProfile.Name},
                       {"PricingProfileNumber", pricingProfile.PricingProfileNumber}
                }
            });

            return pricingProfileMap;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "PricingProfilePatchError : {Exception}", exception);
            throw;
        }
    }
}
