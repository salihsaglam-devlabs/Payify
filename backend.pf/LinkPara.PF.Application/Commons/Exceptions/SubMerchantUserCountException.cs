using LinkPara.SharedModels.Exceptions;

namespace LinkPara.PF.Application.Commons.Exceptions
{
    public class SubMerchantUserCountException : CustomApiException
    {
        public SubMerchantUserCountException()
            : base(ApiErrorCode.SubMerchantUserCountError, "SubMerchantUserCountError")
        { }

        public SubMerchantUserCountException(string message)
            : base(ApiErrorCode.SubMerchantUserCountError, message) { }
    }
}
