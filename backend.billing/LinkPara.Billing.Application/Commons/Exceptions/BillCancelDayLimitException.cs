using LinkPara.SharedModels.Exceptions;

namespace LinkPara.Billing.Application.Commons.Exceptions;

public class BillCancelDayLimitException : ApiException
{
    public BillCancelDayLimitException() : base(ApiErrorCode.InvalidCancelRequest, "CancelDayLimitError") { }
}