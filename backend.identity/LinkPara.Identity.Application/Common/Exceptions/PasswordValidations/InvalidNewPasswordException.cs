using LinkPara.SharedModels.Exceptions;

namespace LinkPara.Identity.Application.Common.Exceptions.PasswordValidations;

public class InvalidNewPasswordException : ApiException
{
    public InvalidNewPasswordException()
        : base(ApiErrorCode.InvalidNewPassword, "Invalid new password!") { }
}