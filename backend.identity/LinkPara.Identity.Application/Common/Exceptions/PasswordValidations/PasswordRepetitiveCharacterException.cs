using LinkPara.SharedModels.Exceptions;

namespace LinkPara.Identity.Application.Common.Exceptions.PasswordValidations;

public class PasswordRepetitiveCharacterException : ApiException
{
    public PasswordRepetitiveCharacterException()
        : base(ApiErrorCode.PasswordRepetitiveCharacter, "Password can not have repetitive characters!") { }
}