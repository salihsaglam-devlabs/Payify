namespace LinkPara.ApiGateway.BackOffice.Services.Approval.Models;

public class ApproveRequest : BaseApproveRequest
{
    public List<Guid> CheckerRoleIds { get; set; }
    public string CheckerFullName { get; set; }
}
