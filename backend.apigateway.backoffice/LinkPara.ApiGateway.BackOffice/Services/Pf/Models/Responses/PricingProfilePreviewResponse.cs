namespace LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;

public class PricingProfilePreviewResponse
{
    public bool IsSucceed { get; set; }
    public List<PricingProfilePreview> PricingProfileItemsAtLoss { get; set; }
}