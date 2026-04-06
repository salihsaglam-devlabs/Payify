using LinkPara.SharedModels.Exceptions;

namespace LinkPara.Billing.Application.Commons.Exceptions;

public class InsufficientBalanceException : ApiException
{
    public InsufficientBalanceException() 
        : base(ApiErrorCode.InsufficientBalanceError, "InsufficientBalanceError")
    {
    }
}