using LinkPara.PF.Application.Commons.Models.Merchants;
using LinkPara.PF.Application.Commons.Models.PricingPreview;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace LinkPara.PF.Application.Features.Merchants.Command.MerchantPricingPreview;

public class MerchantPricingPreviewCommand : IRequest<PricingProfilePreviewResponse>
{
    public string PricingProfileNumber { get; set; }
    public List<Guid> MerchantVposIdList { get; set; }
}

public class MerchantPricingPreviewCommandHandler : IRequestHandler<MerchantPricingPreviewCommand, PricingProfilePreviewResponse>
{
    private readonly IGenericRepository<PricingProfile> _pricingProfileRepository;
    private readonly IGenericRepository<CostProfileItem> _costProfileItemRepository;
    private readonly IStringLocalizer _localizer;
    
    public MerchantPricingPreviewCommandHandler(IGenericRepository<PricingProfile> pricingProfileRepository,
        IGenericRepository<CostProfileItem> costProfileItemRepository,
        IStringLocalizerFactory factory)
    {
        _pricingProfileRepository = pricingProfileRepository;
        _costProfileItemRepository = costProfileItemRepository;
        _localizer = factory.Create("Exceptions", "LinkPara.PF.API");
    }

    public async Task<PricingProfilePreviewResponse> Handle(MerchantPricingPreviewCommand command, CancellationToken cancellationToken)
    {
        var pricingProfile = await _pricingProfileRepository.GetAll()
            .Include(s => s.PricingProfileItems.Where(a => a.IsActive == true && a.RecordStatus == RecordStatus.Active))
            .Where(s => s.PricingProfileNumber == command.PricingProfileNumber &&
                        s.ProfileStatus == ProfileStatus.InUse)
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);
        
        if (pricingProfile is null)
        {
            throw new NotFoundException(nameof(PricingProfile), command.PricingProfileNumber);
        }
        
        var costProfileItems = await _costProfileItemRepository.GetAll()
            .Include(s => s.CostProfile)
            .Where(s => 
                command.MerchantVposIdList.Contains(s.CostProfile.VposId.Value) && 
                s.CostProfile.ProfileStatus == ProfileStatus.InUse && 
                s.IsActive == true && 
                s.CostProfile.PosType == PosType.Virtual && 
                s.RecordStatus == RecordStatus.Active)
            .ToListAsync(cancellationToken: cancellationToken);
        
        var pricingProfilePreviews = new List<PricingProfilePreview>();
        
        foreach (var pricingProfileItem in pricingProfile.PricingProfileItems)
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
                    _localizer.GetString("MerchantPricingAtLoss"), 
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