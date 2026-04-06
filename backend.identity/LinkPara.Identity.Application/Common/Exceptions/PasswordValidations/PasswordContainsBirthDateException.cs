using LinkPara.SharedModels.Exceptions;

namespace LinkPara.Identity.Application.Common.Exceptions.PasswordValidations;

public class PasswordContainsBirthDateException : ApiException
{
    public PasswordContainsBirthDateException()
        : base(ApiErrorCode.PasswordHistoryRequirement, "Password cannot contain birth date!") { }
}