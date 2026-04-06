using LinkPara.SharedModels.Exceptions;

namespace LinkPara.Accounting.Application.Commons.Exceptions;

public class CanNotRetrySuccessPaymentException : ApiException
{
    public CanNotRetrySuccessPaymentException() 
        : base(ApiErrorCode.CanNotRetrySuccessPayment, "CanNotRetrySuccessPayment")
    {
    }
}