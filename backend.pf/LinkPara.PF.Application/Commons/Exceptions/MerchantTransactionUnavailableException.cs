using LinkPara.SharedModels.Exceptions;

namespace LinkPara.PF.Application.Commons.Exceptions;

public class MerchantTransactionUnavailableException : ApiException
{
    public MerchantTransactionUnavailableException()
        : base(ApiErrorCode.MerchantTransactionUnavailable, "MerchantTransactionUnavailable")
    {
    }
}