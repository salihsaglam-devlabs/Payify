using LinkPara.SharedModels.Exceptions;

namespace LinkPara.Billing.Application.Commons.Exceptions;

public class NoBillException : ApiException
{
    public NoBillException() : base(ApiErrorCode.NoBillFoundError, "NoBillFound")
    {
    }
}