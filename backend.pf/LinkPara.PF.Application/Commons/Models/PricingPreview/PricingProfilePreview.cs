using LinkPara.PF.Domain.Enums;

namespace LinkPara.PF.Application.Commons.Models.PricingPreview;

public class PricingProfilePreview
{
    public string Description { get; set; }
    public ProfileCardType ProfileCardType { get; set; }
    public int InstallmentNumber { get; set; }
    public int InstallmentNumberEnd { get; set; }
    public decimal CommissionRate { get; set; }
    public int BlockedDayNumber { get; set; }
    public List<PricingPreviewCostProfile> CostProfileItems { get; set; }
}