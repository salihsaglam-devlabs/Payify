using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;

public class UpdatePreviewPricingProfileRequest
{
    public Guid Id { get; set; }
    public List<PricingProfileItemDto> PricingProfileItems { get; set; }
}