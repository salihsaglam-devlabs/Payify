using LinkPara.SharedModels.Exceptions;

namespace LinkPara.PF.Application.Commons.Exceptions
{
    public class MonthlyLimitCountExceededException : ApiException
    {
        public MonthlyLimitCountExceededException() : base(ApiErrorCode.MonthlyLimitCountExceeded, "MonthlyLimitCountExceeded")
        {

        }
    }
}
