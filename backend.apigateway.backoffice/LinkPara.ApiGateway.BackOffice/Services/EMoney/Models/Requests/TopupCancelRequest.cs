namespace LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Requests;

public class TopupCancelRequest
{
    public Guid CardTopupRequestId { get; set; }
    public string Description { get; set; }
}