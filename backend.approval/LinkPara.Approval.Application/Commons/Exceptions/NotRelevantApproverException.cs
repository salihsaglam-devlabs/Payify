using LinkPara.SharedModels.Exceptions;

namespace LinkPara.Approval.Application.Commons.Exceptions;

public class NotRelevantApproverException : ApiException
{
    public NotRelevantApproverException() : base(ApiErrorCode.NotRelevantApprover, "NotRelevantApprover")
    {
    }
}
