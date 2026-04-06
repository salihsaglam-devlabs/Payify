using LinkPara.SharedModels.Exceptions;


namespace LinkPara.Identity.Application.Common.Exceptions
{
    public class UserNotFoundException : ApiException
    {
        public UserNotFoundException()
            : base(ApiErrorCode.UserNotFound, "User not found for the entered information.") { }
    }
}