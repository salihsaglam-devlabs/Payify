using LinkPara.SharedModels.Exceptions;

namespace LinkPara.Identity.Application.Common.Exceptions
{
    public class SuspendedUserLoginException : ApiException
    {
        public SuspendedUserLoginException() 
            : base(ApiErrorCode.SuspendedUserLogin, $"SuspendedUserLogin")
        {
        }
    }
}
