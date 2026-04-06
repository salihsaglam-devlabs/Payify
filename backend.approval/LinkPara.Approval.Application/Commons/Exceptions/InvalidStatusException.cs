using LinkPara.SharedModels.Exceptions;

namespace LinkPara.Approval.Application.Commons.Exceptions;

public class InvalidStatusException : ApiException
{
    public InvalidStatusException(string status)
       : base(ApiErrorCode.InvalidStatus, $"InvalidStatus:{status}")
    {
    }
}
