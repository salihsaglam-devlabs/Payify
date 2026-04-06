using LinkPara.SharedModels.Exceptions;

namespace LinkPara.Billing.Application.Commons.Exceptions;

public class NotPaidException : ApiException
{
    public NotPaidException() : base(ApiErrorCode.NotPaidException, "NotPaid") { }
}