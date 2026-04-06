namespace LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;

public class UpdateMultiplePricingProfileRequest
{
    public List<Guid> MainSubMerchantIds { get; set; }
    public string PricingProfileNumber { get; set; }
}
