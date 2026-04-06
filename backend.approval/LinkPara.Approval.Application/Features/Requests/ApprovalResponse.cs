
using LinkPara.Approval.Domain.Enums;

namespace LinkPara.Approval.Application.Features.Requests;

public class ApprovalResponse
{
    public Guid RequestId { get; set; }
    public ApprovalStatus Status { get; set; }
    public RequestDto Request { get; set; }
    public bool IsSuccess { get; set; }
    public string Message { get; set; }
}
