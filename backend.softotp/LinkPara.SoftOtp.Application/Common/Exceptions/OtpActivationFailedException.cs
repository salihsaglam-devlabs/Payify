using LinkPara.SharedModels.Exceptions;

namespace LinkPara.SoftOtp.Application.Common.Exceptions;

public class OtpActivationFailedException : ApiException
{
    public OtpActivationFailedException() 
        : base(ApiErrorCode.OtpActivationFailed, "Error occured while activating otp!")
    {
    }
}