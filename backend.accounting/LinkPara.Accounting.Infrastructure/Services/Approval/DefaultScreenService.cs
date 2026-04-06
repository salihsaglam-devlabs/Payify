using LinkPara.Approval.Models;
using LinkPara.Approval;

namespace LinkPara.Accounting.Infrastructure.Services.Approval;

public class DefaultScreenService : IApprovalScreenService
{
    public async Task<ApprovalScreenResponse> DeleteScreenFieldsAsync(ApprovalScreenRequest request)
    {
        return await Task.FromResult(new ApprovalScreenResponse());
    }

    public async Task<ApprovalScreenResponse> PatchScreenFieldsAsync(ApprovalScreenRequest request)
    {
        return await Task.FromResult(new ApprovalScreenResponse());
    }

    public async Task<ApprovalScreenResponse> PostScreenFieldsAsync(ApprovalScreenRequest request)
    {
        return await Task.FromResult(new ApprovalScreenResponse());
    }

    public async Task<ApprovalScreenResponse> PutScreenFieldsAsync(ApprovalScreenRequest request)
    {
        return await Task.FromResult(new ApprovalScreenResponse());
    }
}
