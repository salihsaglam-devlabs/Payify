using LinkPara.SharedModels.Exceptions;

namespace LinkPara.PF.Application.Commons.Exceptions
{
    public class DailyLimitCountExceededException : ApiException
    {
        public DailyLimitCountExceededException() : base(ApiErrorCode.DailyLimitCountExceeded, "DailyLimitCountExceeded")
        {
        }
    }
}
