using LinkPara.SharedModels.Exceptions;

namespace LinkPara.PF.Application.Commons.Exceptions
{
    public class MonthlyLimitAmountExceededException : ApiException
    {
        public MonthlyLimitAmountExceededException() : base(ApiErrorCode.MonthlyLimitAmountExceeded, "MonthlyLimitAmountExceeded")
        {

        }
    }
}
