using LinkPara.SharedModels.Exceptions;

namespace LinkPara.Accounting.Application.Commons.Exceptions;

public class CanNotRetrySuccessCustomerException : ApiException
{
    public CanNotRetrySuccessCustomerException()
        : base(ApiErrorCode.CanNotRetrySuccessCustomer, "CustomerAlreadySucceeded")
    {
    }
}