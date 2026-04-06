using LinkPara.SharedModels.Exceptions;

namespace LinkPara.PF.Application.Commons.Exceptions
{
    public class MerchantLimitMonthlyMaxAmountExceededException : ApiException
    {
        public MerchantLimitMonthlyMaxAmountExceededException() : base(ApiErrorCode.MerchantLimitMonthlyMaxAmountExceeded, "MerchantLimitMonthlyMaxAmountExceeded")
        {

        }
    }
}

