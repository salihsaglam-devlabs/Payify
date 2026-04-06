using LinkPara.SharedModels.Exceptions;

namespace LinkPara.Approval.Application.Commons.Exceptions;

public class SameRecordException : ApiException
{
    public SameRecordException() : base(ApiErrorCode.SameRecord, "SameRecord")
    {
    }
}
