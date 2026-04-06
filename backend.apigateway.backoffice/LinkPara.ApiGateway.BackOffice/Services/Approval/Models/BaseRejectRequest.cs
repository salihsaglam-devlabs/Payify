namespace LinkPara.ApiGateway.BackOffice.Services.Approval.Models;

public class BaseRejectRequest
{
    public Guid UserId { get; set; }
    public Guid RequestId { get; set; }
    public string Reason { get; set; }
}
