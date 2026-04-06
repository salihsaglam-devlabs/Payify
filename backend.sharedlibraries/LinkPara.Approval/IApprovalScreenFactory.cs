using LinkPara.Approval.Models.Enums;

namespace LinkPara.Approval;

public interface IApprovalScreenFactory
{
    IApprovalScreenService GetApprovalScreenService(string resource);
}