using LinkPara.SharedModels.Exceptions;

namespace LinkPara.PF.Application.Commons.Exceptions
{
    public class BankLimitExceededException : ApiException
    {
        public BankLimitExceededException() : base(ApiErrorCode.BankLimitExceeded, "BankLimitExceeded")
        {
        }
    }
}
