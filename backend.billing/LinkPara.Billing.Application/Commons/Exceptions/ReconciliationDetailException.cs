using LinkPara.SharedModels.Exceptions;

namespace LinkPara.Billing.Application.Commons.Exceptions;

public class ReconciliationDetailException : ApiException
{
    public ReconciliationDetailException(string message)
        : base(ApiErrorCode.ReconciliationDetailException, message)
    {

    }
}