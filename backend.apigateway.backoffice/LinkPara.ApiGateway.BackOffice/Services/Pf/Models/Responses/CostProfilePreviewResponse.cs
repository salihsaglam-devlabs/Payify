namespace LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;

public class CostProfilePreviewResponse
{
    public bool IsSucceed { get; set; }
    public List<CostProfilePreview> CostProfileItemsAtLoss { get; set; }
}