using LinkPara.ApiGateway.BackOffice.Services.Approval.Models.Enums;

namespace LinkPara.ApiGateway.BackOffice.Services.Approval.Models;

public class PatchRequestDto
{
    public ApprovalStatus Status { get; set; }
    public string ExceptionMessage { get; set; }
    public string ExceptionDetails { get; set; }
}
