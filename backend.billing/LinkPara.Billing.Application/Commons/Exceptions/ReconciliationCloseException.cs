using LinkPara.SharedModels.Exceptions;

namespace LinkPara.Billing.Application.Commons.Exceptions;

public class ReconciliationCloseException : ApiException
{
    public ReconciliationCloseException(string message)
        : base(ApiErrorCode.ReconciliationCloseException, message)
    {

    }
}