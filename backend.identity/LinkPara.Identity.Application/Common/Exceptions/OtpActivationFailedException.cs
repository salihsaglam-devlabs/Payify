using LinkPara.SharedModels.Exceptions;

namespace LinkPara.Identity.Application.Common.Exceptions;

public class OtpActivationFailedException: ApiException
{
    public OtpActivationFailedException() 
        : base(ApiErrorCode.OtpActivationFailed, "Error occured while activating otp!")
    {
    }
}