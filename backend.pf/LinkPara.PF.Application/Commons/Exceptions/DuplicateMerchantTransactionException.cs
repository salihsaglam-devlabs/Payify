using LinkPara.SharedModels.Exceptions;

namespace LinkPara.PF.Application.Commons.Exceptions;

public class DuplicateMerchantTransactionException : ApiException
{
    public DuplicateMerchantTransactionException()
        : base(ApiErrorCode.DuplicateMerchantTransaction, "DuplicateMerchantTransaction")
    {
    }
}