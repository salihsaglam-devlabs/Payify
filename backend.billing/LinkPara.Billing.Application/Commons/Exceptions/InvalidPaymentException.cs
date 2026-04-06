using LinkPara.SharedModels.Exceptions;

namespace LinkPara.Billing.Application.Commons.Exceptions;

public class InvalidPaymentException : ApiException
{
    public InvalidPaymentException() : base(ApiErrorCode.InvalidPaymentError, "InvalidPayment") { }
}
