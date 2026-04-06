using LinkPara.SharedModels.Exceptions;

namespace LinkPara.PF.Application.Commons.Exceptions
{
    public class MerchantLimitDailyMaxAmountExceededException : ApiException
    {
        public MerchantLimitDailyMaxAmountExceededException() : base(ApiErrorCode.MerchantLimitDailyMaxAmountExceeded, "MerchantLimitDailyMaxAmountExceeded")
        {

        }
    }
}
