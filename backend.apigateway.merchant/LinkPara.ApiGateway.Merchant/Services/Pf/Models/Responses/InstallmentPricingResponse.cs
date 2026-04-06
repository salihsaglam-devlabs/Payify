namespace LinkPara.ApiGateway.Merchant.Services.Pf.Models.Responses;

public class InstallmentPricingResponse
{
    public string MerchantNumber { get; set; }
    public List<LoyaltyInstallmentPricing> LoyaltyInstallmentPricing { get; set; }
    public bool IsSucceed { get; set; }
}