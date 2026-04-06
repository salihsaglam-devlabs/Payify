using LinkPara.SharedModels.Exceptions;

namespace LinkPara.Identity.Application.Common.Exceptions.PasswordValidations;

public class PasswordSuccessiveCharacterException : ApiException
{
    public PasswordSuccessiveCharacterException()
        : base(ApiErrorCode.PasswordSuccessiveCharacter, "Password can not have successive characters!") { }
}