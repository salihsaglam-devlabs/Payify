using LinkPara.SharedModels.Exceptions;

namespace LinkPara.PF.Application.Commons.Exceptions
{
    public class MerchantBusinessPartnerMaxCountException : ApiException
    {
        public MerchantBusinessPartnerMaxCountException()
            : base(ApiErrorCode.MerchantBusinessPartnerMaxCount, "MerchantBusinessPartnerMaxCount")
        {
        }
    }
}