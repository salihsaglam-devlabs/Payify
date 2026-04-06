namespace LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Requests;

public class MasterpassCancelRequest
{
    public Guid CardTopupRequestId { get; set; }
    public string Description { get; set; }
}
