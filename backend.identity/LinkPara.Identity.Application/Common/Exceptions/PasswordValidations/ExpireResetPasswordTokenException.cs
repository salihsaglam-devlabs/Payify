using LinkPara.SharedModels.Exceptions;

namespace LinkPara.Identity.Application.Common.Exceptions.PasswordValidations;

public class ExpireResetPasswordTokenException : ApiException
{
    public ExpireResetPasswordTokenException()
        : base(ApiErrorCode.InvalidNewPassword, "Expired reset password token!") { }
}
