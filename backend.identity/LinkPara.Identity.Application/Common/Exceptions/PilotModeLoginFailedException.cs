using LinkPara.SharedModels.Exceptions;

namespace LinkPara.Identity.Application.Common.Exceptions;

public class PilotModeLoginFailedException : CustomApiException
{
    public PilotModeLoginFailedException(string message)
        : base(ApiErrorCode.PilotModeLoginFailed, message) { }
}