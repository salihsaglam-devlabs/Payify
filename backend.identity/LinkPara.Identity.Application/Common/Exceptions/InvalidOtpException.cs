using LinkPara.SharedModels.Exceptions;

namespace LinkPara.Identity.Application.Common.Exceptions;

public class InvalidOtpException : ApiException
{
    public InvalidOtpException()
        : base(ApiErrorCode.InvalidOtp, "Invalid code!") { }
}