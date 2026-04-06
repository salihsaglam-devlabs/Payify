using LinkPara.ApiGateway.BackOffice.Services.Approval.Models.Enums;

namespace LinkPara.ApiGateway.BackOffice.Services.Approval.Models;

public class ApprovalResponse
{
    public Guid RequestId { get; set; }
    public ApprovalStatus Status { get; set; }
    public RequestDto Request { get; set; }
    public string HttpResponseContent { get; set; }
    public bool IsSuccess { get; set; }
    public string Message { get; set; }
}

