namespace LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;

public class MerchantPricingPreviewRequest
{
    public string PricingProfileNumber { get; set; }
    public List<Guid> MerchantVposIdList { get; set; }
}