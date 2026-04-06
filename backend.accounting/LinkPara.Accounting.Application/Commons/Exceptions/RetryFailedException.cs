using LinkPara.SharedModels.Exceptions;

namespace LinkPara.Accounting.Application.Commons.Exceptions;

public class RetryFailedException : ApiException
{
    public RetryFailedException()
    : base(ApiErrorCode.RetryFailed, "RetryFailed")
    {
    }
}
