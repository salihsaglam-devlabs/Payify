using LinkPara.SharedModels.Exceptions;

namespace LinkPara.Epin.Application.Commons.Exceptions;

public class AuthorizationFailedException : ApiException
{
    public AuthorizationFailedException(string message)
        : base(ApiErrorCode.AuthorizationFailed, message)
    {
    }
}
