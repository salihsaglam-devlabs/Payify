using LinkPara.SharedModels.Exceptions;

namespace LinkPara.Billing.Application.Commons.Exceptions;

public class InvalidDebtException : ApiException
{
    public InvalidDebtException() : base(ApiErrorCode.InvalidDebtError, "InvalidDebt") { }
}