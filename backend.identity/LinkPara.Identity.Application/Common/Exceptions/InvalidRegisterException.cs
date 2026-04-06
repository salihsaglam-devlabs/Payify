using LinkPara.SharedModels.Exceptions;

namespace LinkPara.Identity.Application.Common.Exceptions;

public class InvalidRegisterException : ApiException
{
    public InvalidRegisterException()
        : base(ApiErrorCode.InvalidRegister, "One or more register failures have occured.") { }
}