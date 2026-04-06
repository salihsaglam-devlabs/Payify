using LinkPara.SharedModels.Exceptions;

namespace LinkPara.Billing.Application.Commons.Exceptions;

public class ReconciliationSummaryException : ApiException
{
    public ReconciliationSummaryException(string message)
        : base(ApiErrorCode.ReconciliationSummaryException, message)
    {
    }
}