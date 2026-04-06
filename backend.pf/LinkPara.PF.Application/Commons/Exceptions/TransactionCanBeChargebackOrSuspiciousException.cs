using LinkPara.SharedModels.Exceptions;

namespace LinkPara.PF.Application.Commons.Exceptions;

public class TransactionCanBeChargebackOrSuspiciousException : ApiException
{
    public TransactionCanBeChargebackOrSuspiciousException() 
        : base(ApiErrorCode.TransactionCanBeChargebackOrSuspicious, "TransactionCanBeChargebackOrSuspicious")
    {
    }
}