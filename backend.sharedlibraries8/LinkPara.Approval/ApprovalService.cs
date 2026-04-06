using LinkPara.Approval.Models;
using LinkPara.Approval.Models.Enums;

namespace LinkPara.Approval;

public class ApprovalService : IApprovalService
{
    private readonly IApprovalScreenFactory _approvalScreenFactory;
    
    public ApprovalService(IApprovalScreenFactory approvalScreenFactory)
    {
        _approvalScreenFactory = approvalScreenFactory;
    }
    
    public async Task<ApprovalScreenResponse> GetScreenFieldsAsync(ApprovalScreenRequest request)
    {
        var service = _approvalScreenFactory.GetApprovalScreenService(request.Resource);
        if (service is null)
        {
            throw new ArgumentException("service is null");
        }
        switch (request.Action)
        {
            case ActionType.Post:
                return await service.PostScreenFieldsAsync(request);
            case ActionType.Put:
                return await service.PutScreenFieldsAsync(request);
            case ActionType.Delete:
                return await service.DeleteScreenFieldsAsync(request);
            case ActionType.Patch:
                return await service.PatchScreenFieldsAsync(request);
            default:
                throw new InvalidOperationException();
        }
    }
}