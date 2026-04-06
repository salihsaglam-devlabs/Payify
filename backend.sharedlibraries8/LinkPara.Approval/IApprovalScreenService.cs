using LinkPara.Approval.Models;

namespace LinkPara.Approval;

public interface IApprovalScreenService 
{
    Task<ApprovalScreenResponse> PostScreenFieldsAsync(ApprovalScreenRequest request);
    Task<ApprovalScreenResponse> PutScreenFieldsAsync(ApprovalScreenRequest request);
    Task<ApprovalScreenResponse> DeleteScreenFieldsAsync(ApprovalScreenRequest request);
    Task<ApprovalScreenResponse> PatchScreenFieldsAsync(ApprovalScreenRequest request);
}