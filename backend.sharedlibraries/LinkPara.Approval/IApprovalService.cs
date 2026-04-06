using LinkPara.Approval.Models;
using LinkPara.Approval.Models.Enums;

namespace LinkPara.Approval;

public interface IApprovalService
{
    Task<ApprovalScreenResponse> GetScreenFieldsAsync(ApprovalScreenRequest request);
}