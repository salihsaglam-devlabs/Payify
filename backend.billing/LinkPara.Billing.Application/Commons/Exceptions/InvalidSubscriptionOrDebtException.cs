using LinkPara.SharedModels.Exceptions;

namespace LinkPara.Billing.Application.Commons.Exceptions;

public class InvalidSubscriptionOrDebtException : ApiException
{
    public InvalidSubscriptionOrDebtException() : base(ApiErrorCode.InvalidDSubscriptionOrDebtException, "InvalidSubscriptionOrDebt") { }
}
