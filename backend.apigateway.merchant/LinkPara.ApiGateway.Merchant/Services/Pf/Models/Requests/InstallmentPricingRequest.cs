namespace LinkPara.ApiGateway.Merchant.Services.Pf.Models.Requests;

public class InstallmentPricingRequest
{
    public string MerchantNumber { get; set; }
    public string BinNumber { get; set; }
    public decimal Amount { get; set; }
}