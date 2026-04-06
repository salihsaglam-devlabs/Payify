using LinkPara.SharedModels.Exceptions;

namespace LinkPara.PF.Application.Commons.Exceptions;

public class IdentityValidationFailedException : ApiException
{
    public IdentityValidationFailedException()
    : base(ApiErrorCode.IdentityValidationFailed, "IdentityValidationFailed")
    {
    }
}
