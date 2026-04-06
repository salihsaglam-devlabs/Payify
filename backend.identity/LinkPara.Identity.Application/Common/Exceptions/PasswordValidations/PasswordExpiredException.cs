using LinkPara.SharedModels.Exceptions;

namespace LinkPara.Identity.Application.Common.Exceptions.PasswordValidations;

public class PasswordExpiredException : ApiException
{
    public PasswordExpiredException()
        : base(ApiErrorCode.PasswordExpired, "Password has expired!") { }
}