using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Enums;

namespace LinkPara.ApiGateway.Merchant.Services.Pf.Models.Responses;

public class LoyaltyInstallmentPricing
{
    public CardNetwork CardNetwork { get; set; }
    public List<InstallmentPricing> InstallmentPricings { get; set; }
}