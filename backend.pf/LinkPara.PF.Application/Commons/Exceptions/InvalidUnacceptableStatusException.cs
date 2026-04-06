using LinkPara.SharedModels.Exceptions;

namespace LinkPara.PF.Application.Commons.Exceptions;

public class InvalidUnacceptableStatusException : ApiException
{
    public InvalidUnacceptableStatusException()
        : base(ApiErrorCode.InvalidUnacceptableTransactionStatus, "InvalidUnacceptableTransactionStatus")
    {
    }
}