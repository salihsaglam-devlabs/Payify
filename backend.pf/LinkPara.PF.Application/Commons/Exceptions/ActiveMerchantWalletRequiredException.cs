using LinkPara.SharedModels.Exceptions;

namespace LinkPara.PF.Application.Commons.Exceptions;

public class ActiveMerchantWalletRequiredException : ApiException
{
    public ActiveMerchantWalletRequiredException() : base(ApiErrorCode.ActiveMerchantWalletRequired, "ActiveMerchantWalletRequired")
    {
    }
}

