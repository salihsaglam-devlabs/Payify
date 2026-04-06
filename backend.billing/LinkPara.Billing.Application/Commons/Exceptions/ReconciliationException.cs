using LinkPara.SharedModels.Exceptions;

namespace LinkPara.Billing.Application.Commons.Exceptions;

public class ReconciliationException : ApiException
{
    public ReconciliationException(string message) 
        : base(ApiErrorCode.ReconciliationJobError, message)
    {
    }
}