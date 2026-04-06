using LinkPara.SharedModels.Exceptions;

namespace LinkPara.PF.Application.Commons.Exceptions;

public class TransactionNotReturnableException : ApiException
{
    public TransactionNotReturnableException()
        : base(ApiErrorCode.TransactionNotReturnable, "TransactionNotReturnable")
    {
    }
}