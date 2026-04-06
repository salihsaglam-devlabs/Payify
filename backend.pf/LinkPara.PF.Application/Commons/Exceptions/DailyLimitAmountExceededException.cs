using LinkPara.SharedModels.Exceptions;

namespace LinkPara.PF.Application.Commons.Exceptions
{
    public class DailyLimitAmountExceededException : ApiException
    {
        public DailyLimitAmountExceededException() : base(ApiErrorCode.DailyLimitAmountExceeded, "DailyLimitAmountExceeded")
        {

        }
    }
}
