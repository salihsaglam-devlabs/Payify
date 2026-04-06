using LinkPara.SharedModels.Exceptions;

namespace LinkPara.Identity.Application.Common.Exceptions.PasswordValidations;

public class PasswordHistoryRequirementException : ApiException
{
    public PasswordHistoryRequirementException()
        : base(ApiErrorCode.PasswordHistoryRequirement, "Password must be different then the old passwords!") { }
}