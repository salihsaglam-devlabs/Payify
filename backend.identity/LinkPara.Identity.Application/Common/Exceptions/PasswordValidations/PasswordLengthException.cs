using LinkPara.SharedModels.Exceptions;

namespace LinkPara.Identity.Application.Common.Exceptions.PasswordValidations;

public class PasswordLengthException : ApiException
{
    public PasswordLengthException()
        : base(ApiErrorCode.PasswordLength, "Password must have 6 characters!") { }
}