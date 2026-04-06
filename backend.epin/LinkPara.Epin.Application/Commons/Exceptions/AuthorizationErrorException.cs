using LinkPara.SharedModels.Exceptions;

namespace LinkPara.Epin.Application.Commons.Exceptions;

public class AuthorizationErrorException : ApiException
{
    public AuthorizationErrorException(string message)
        : base(ApiErrorCode.AuthorizationError, message)
    {
    }
}
