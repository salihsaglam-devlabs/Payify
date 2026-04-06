namespace LinkPara.ApiGateway.BackOffice.Services.Approval.Models;

public class RejectRequest : BaseRejectRequest
{
    public List<Guid> CheckerRoleIds { get; set; }
    public string CheckerFullName { get; set; }
}
