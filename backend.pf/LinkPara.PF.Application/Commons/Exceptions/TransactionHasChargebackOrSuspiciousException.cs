using LinkPara.SharedModels.Exceptions;

namespace LinkPara.PF.Application.Commons.Exceptions;

public class TransactionHasChargebackOrSuspiciousException : ApiException
{
    public TransactionHasChargebackOrSuspiciousException()
        : base(ApiErrorCode.TransactionHasChargeback, "TransactionHasChargeback")
    {
    }
}