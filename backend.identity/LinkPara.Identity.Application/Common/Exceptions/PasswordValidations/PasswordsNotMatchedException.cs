using LinkPara.SharedModels.Exceptions;

namespace LinkPara.Identity.Application.Common.Exceptions.PasswordValidations;

public class PasswordsNotMatchedException : ApiException
{
    public PasswordsNotMatchedException()
        : base(ApiErrorCode.PasswordsNotMatched, "Passwords not matched!") { }
}