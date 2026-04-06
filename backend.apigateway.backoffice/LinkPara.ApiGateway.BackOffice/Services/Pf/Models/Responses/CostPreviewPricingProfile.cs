using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Enums;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;

public class CostPreviewPricingProfile
{
    public string PricingProfileName { get; set; }
    public string PricingProfileNumber { get; set; }
    public ProfileCardType ProfileCardType { get; set; }
    public decimal CommissionRate { get; set; }
    public int BlockedDayNumber { get; set; }
}