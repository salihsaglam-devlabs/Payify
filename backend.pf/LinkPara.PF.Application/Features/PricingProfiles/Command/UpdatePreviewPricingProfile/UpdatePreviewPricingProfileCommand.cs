using LinkPara.PF.Application.Commons.Models.PricingPreview;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace LinkPara.PF.Application.Features.PricingProfiles.Command.UpdatePreviewPricingProfile;

public class UpdatePreviewPricingProfileCommand : IRequest<PricingProfilePreviewResponse>
{
    public Guid Id { get; set; }
    public List<PricingProfileItemDto> PricingProfileItems { get; set; }
}

public class UpdatePreviewPricingProfileCommandHandler : IRequestHandler<UpdatePreviewPricingProfileCommand,
    PricingProfilePreviewResponse>
{
    private readonly IGenericRepository<PricingProfile> _pricingProfileRepository;
    private readonly IGenericRepository<CostProfileItem> _costProfileItemRepository;
    private readonly IGenericRepository<Merchant> _merchantRepository;
    private readonly IStringLocalizer _localizer;

    public UpdatePreviewPricingProfileCommandHandler(IGenericRepository<PricingProfile> pricingProfileRepository,
        IGenericRepository<CostProfileItem> costProfileItemRepository,
        IStringLocalizerFactory factory,
        IGenericRepository<Merchant> merchantRepository)
    {
        _pricingProfileRepository = pricingProfileRepository;
        _costProfileItemRepository = costProfileItemRepository;
        _merchantRepository = merchantRepository;
        _localizer = factory.Create("Exceptions", "LinkPara.PF.API");
    }

    public async Task<PricingProfilePreviewResponse> Handle(UpdatePreviewPricingProfileCommand command,
        CancellationToken cancellationToken)
    {
        var pricingProfile = await _pricingProfileRepository.GetByIdAsync(command.Id);

        if (pricingProfile is null)
        {
            throw new NotFoundException(nameof(PricingProfile), command.Id);
        }

        var relatedVposIds = await _merchantRepository.GetAll()
            .Where(s => s.PricingProfileNumber == pricingProfile.PricingProfileNumber &&
                        s.MerchantStatus == MerchantStatus.Active)
            .SelectMany(s => s.MerchantVposList
                .Where(v => v.RecordStatus == RecordStatus.Active)
                .Select(v => v.VposId))
            .Distinct()
            .ToListAsync(cancellationToken: cancellationToken);

        var costProfileItems = await _costProfileItemRepository.GetAll()
            .Include(s => s.CostProfile)
            .Where(s => 
                        relatedVposIds.Contains(s.CostProfile.VposId.Value) && 
                        s.CostProfile.ProfileStatus == ProfileStatus.InUse && 
                        s.CostProfile.PosType == PosType.Virtual && 
                        s.IsActive == true && 
                        s.RecordStatus == RecordStatus.Active)
            .ToListAsync(cancellationToken: cancellationToken);

        var pricingProfilePreviews = new List<PricingProfilePreview>();
        
        foreach (var pricingProfileItem in command.PricingProfileItems.Where(s => s.IsActive).ToList())
        {
            var costProfileItemsAtLoss = costProfileItems.Where(s =>
                    (pricingProfileItem.ProfileCardType == ProfileCardType.Credit
                        ? s.ProfileCardType is ProfileCardType.Amex or ProfileCardType.Credit
                        : s.ProfileCardType == pricingProfileItem.ProfileCardType) &&
                    s.InstallmentNumber == pricingProfileItem.InstallmentNumber &&
                    s.InstallmentNumberEnd == pricingProfileItem.InstallmentNumberEnd &&
                    (s.CommissionRate > pricingProfileItem.CommissionRate ||
                     s.BlockedDayNumber > pricingProfileItem.BlockedDayNumber))
                .ToList();

            if (costProfileItemsAtLoss.Count <= 0)
            {
                continue;
            }

            pricingProfilePreviews.Add(new PricingProfilePreview
            {
                Description = string.Format(
                    _localizer.GetString("PricingProfileItemAtLoss"), 
                    costProfileItemsAtLoss.Max(s => s.CommissionRate), 
                    costProfileItemsAtLoss.Max(s => s.BlockedDayNumber)),
                ProfileCardType = pricingProfileItem.ProfileCardType,
                InstallmentNumber = pricingProfileItem.InstallmentNumber,
                InstallmentNumberEnd = pricingProfileItem.InstallmentNumberEnd,
                CommissionRate = pricingProfileItem.CommissionRate,
                BlockedDayNumber = pricingProfileItem.BlockedDayNumber,
                CostProfileItems = costProfileItemsAtLoss.Select(s => new PricingPreviewCostProfile
                {
                    Name = s.CostProfile.Name,
                    ProfileCardType = s.ProfileCardType,
                    CardTransactionType = s.CardTransactionType,
                    CommissionRate = s.CommissionRate,
                    BlockedDayNumber = s.BlockedDayNumber
                }).ToList()
            });
        }

        return new PricingProfilePreviewResponse
        {
            IsSucceed = true,
            PricingProfileItemsAtLoss = pricingProfilePreviews
        };
    }
}