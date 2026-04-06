using LinkPara.Approval.Models;

namespace LinkPara.ApiGateway.BackOffice.Services.Approval.Models;

public class RequestScreenFields
{
    public RequestDto Request { get; set; }
    public ApprovalScreenResponse DisplayScreenFields { get; set; }
}
