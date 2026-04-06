using LinkPara.PF.Application.Commons.Models.PricingPreview;
using LinkPara.PF.Application.Features.VirtualPos;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace LinkPara.PF.Application.Features.CostProfiles.Command.UpdatePreviewCostProfile;

public class UpdatePreviewCostProfileCommand : IRequest<CostProfilePreviewResponse>
{
    public Guid Id { get; set; }
    public List<CostProfileItemDto> CostProfileItems { get; set; }
}

public class UpdatePreviewCostProfileCommandHandler : IRequestHandler<UpdatePreviewCostProfileCommand, CostProfilePreviewResponse>
{
    private readonly IGenericRepository<PricingProfileItem> _pricingProfileItemRepository;
    private readonly IGenericRepository<CostProfile> _costProfileRepository;
    private readonly IGenericRepository<Merchant> _merchantRepository;
    private readonly IStringLocalizer _localizer;
    
    public UpdatePreviewCostProfileCommandHandler(IGenericRepository<PricingProfileItem> pricingProfileItemRepository,
        IGenericRepository<CostProfile> costProfileRepository,
        IStringLocalizerFactory factory,
        IGenericRepository<Merchant> merchantRepository)
    {
        _pricingProfileItemRepository = pricingProfileItemRepository;
        _costProfileRepository = costProfileRepository;
        _merchantRepository = merchantRepository;
        _localizer = factory.Create("Exceptions", "LinkPara.PF.API");
    }

    public async Task<CostProfilePreviewResponse> Handle(UpdatePreviewCostProfileCommand command, CancellationToken cancellationToken)
    {
        var costProfile = await _costProfileRepository.GetByIdAsync(command.Id);

        if (costProfile is null)
        {
            throw new NotFoundException(nameof(CostProfile), command.Id);
        }

        var query = _merchantRepository.GetAll()
    .Where(s => s.MerchantStatus == MerchantStatus.Active);

        query = costProfile.PosType switch
        {
            PosType.Virtual => query.Where(s =>
                s.MerchantVposList.Any(v => v.VposId == costProfile.VposId)),

            PosType.Physical => query.Where(s =>
                s.MerchantPhysicalDevices.Any(d =>
                    d.MerchantPhysicalPosList.Any(p =>
                        p.PhysicalPosId == costProfile.PhysicalPosId))),

            _ => query
        };

        var relatedPricingProfileNumbers = await query
            .Select(s => s.PricingProfileNumber)
            .Distinct()
            .ToListAsync(cancellationToken);


        var pricingProfileItems = await _pricingProfileItemRepository.GetAll()
            .Include(s => s.PricingProfile)
            .Where(s => 
                relatedPricingProfileNumbers.Contains(s.PricingProfile.PricingProfileNumber) &&
                s.PricingProfile.ProfileStatus == ProfileStatus.InUse &&
                s.IsActive == true &&
                s.RecordStatus == RecordStatus.Active)
            .ToListAsync(cancellationToken: cancellationToken);

        var costProfilePreviews = new List<CostProfilePreview>();

        foreach (var costProfileItem in command.CostProfileItems.Where(s => s.IsActive).ToList())
        {
            var pricingProfileItemsAtLoss = pricingProfileItems.Where(s =>
                    (costProfileItem.ProfileCardType is ProfileCardType.Amex or ProfileCardType.Credit
                        ? s.ProfileCardType == ProfileCardType.Credit 
                        : s.ProfileCardType == costProfileItem.ProfileCardType) &&
                    s.InstallmentNumber == costProfileItem.InstallmentNumber &&
                    s.InstallmentNumberEnd == costProfileItem.InstallmentNumberEnd &&
                    (s.CommissionRate < costProfileItem.CommissionRate ||
                     s.BlockedDayNumber < costProfileItem.BlockedDayNumber))
                .ToList();

            if (pricingProfileItemsAtLoss.Count <= 0)
            {
                continue;
            }
            
            costProfilePreviews.Add(new CostProfilePreview
            {
                Description = string.Format(
                    _localizer.GetString("CostProfileItemAtLoss"), 
                    costProfileItem.CommissionRate, 
                    costProfileItem.BlockedDayNumber),
                CardTransactionType = costProfileItem.CardTransactionType,
                ProfileCardType = costProfileItem.ProfileCardType,
                InstallmentNumber = costProfileItem.InstallmentNumber,
                InstallmentNumberEnd = costProfileItem.InstallmentNumberEnd,
                CommissionRate = costProfileItem.CommissionRate,
                BlockedDayNumber = costProfileItem.BlockedDayNumber,
                PricingProfileItems = pricingProfileItemsAtLoss.Select(s => new CostPreviewPricingProfile
                {
                    PricingProfileName = s.PricingProfile.Name,
                    PricingProfileNumber = s.PricingProfile.PricingProfileNumber,
                    ProfileCardType = s.ProfileCardType,
                    CommissionRate = s.CommissionRate,
                    BlockedDayNumber = s.BlockedDayNumber
                }).ToList()
            });
        }
        
        return new CostProfilePreviewResponse
        {
            IsSucceed = true,
            CostProfileItemsAtLoss = costProfilePreviews
        };
    }
}