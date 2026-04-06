using LinkPara.SharedModels.Exceptions;

namespace LinkPara.Billing.Application.Commons.Exceptions;

public class BillAccountingCancelException : ApiException
{
    public BillAccountingCancelException(string message) 
        : base(ApiErrorCode.BillAccountingCancelException, message) { }
}