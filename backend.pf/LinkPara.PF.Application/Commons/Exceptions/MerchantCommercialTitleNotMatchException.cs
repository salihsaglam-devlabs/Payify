using LinkPara.SharedModels.Exceptions;

namespace LinkPara.PF.Application.Commons.Exceptions;

public class MerchantCommercialTitleNotMatchException : ApiException
{
    public MerchantCommercialTitleNotMatchException() : base(ApiErrorCode.MerchantCommercialTitleNotMatch, "Girilen Vkn'ye ait commercial title farkli")
    {
    }
}
