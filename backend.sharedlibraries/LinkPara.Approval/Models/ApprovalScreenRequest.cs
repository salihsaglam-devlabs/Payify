using LinkPara.Approval.Models.Enums;

namespace LinkPara.Approval.Models;

public class ApprovalScreenRequest
{
    public string Url { get; set; }
    public string Resource { get; set; }
    public string QueryParameters { get; set; }
    public string Body { get; set; }
    public ActionType Action { get; set; }
}
