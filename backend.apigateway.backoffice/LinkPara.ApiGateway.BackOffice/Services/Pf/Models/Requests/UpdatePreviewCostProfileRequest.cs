using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;

public class UpdatePreviewCostProfileRequest
{
    public Guid Id { get; set; }
    public List<CostProfileItemDto> CostProfileItems { get; set; }
}