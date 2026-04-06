using LinkPara.PF.Domain.Enums;

namespace LinkPara.PF.Application.Commons.Models.PricingPreview;

public class CostPreviewPricingProfile
{
    public string PricingProfileName { get; set; }
    public string PricingProfileNumber { get; set; }
    public ProfileCardType ProfileCardType { get; set; }
    public decimal CommissionRate { get; set; }
    public int BlockedDayNumber { get; set; }
}