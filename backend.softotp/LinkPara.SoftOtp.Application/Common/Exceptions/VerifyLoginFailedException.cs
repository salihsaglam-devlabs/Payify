using System.Runtime.Serialization;
using LinkPara.SharedModels.Exceptions;

namespace LinkPara.SoftOtp.Application.Common.Exceptions;

public class VerifyLoginFailedException : ApiException
{
    public VerifyLoginFailedException() 
        : base(ApiErrorCode.VerifyLoginFailed, "Verify login failed!")
    {
    }

    protected VerifyLoginFailedException(SerializationInfo info, StreamingContext context) 
        : base(info, context)
    {
    }
}