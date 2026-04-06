using LinkPara.Approval.Models.Enums;

namespace LinkPara.ApiGateway.BackOffice.Services.Approval.Models;

public class ApprovalRequest
{
    public Guid UserId { get; set; }
    public List<Guid> MakerRoleIds { get; set; }
    public Guid CaseId { get; set; }
    public string Body { get; set; }
    public ActionType ActionType { get; set; }
    public string Resource { get; set; }
    public string Url { get; set; }
    public string QueryParameters { get; set; }
    public string DisplayName { get; set; }
    public string MakerFullName { get; set; }
}
