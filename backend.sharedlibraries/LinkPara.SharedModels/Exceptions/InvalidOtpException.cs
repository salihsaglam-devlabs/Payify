namespace LinkPara.SharedModels.Exceptions;

public class InvalidOtpException : GenericException
{
    public InvalidOtpException()
        : base(ErrorCode.InvalidOtp)
    {
        
    }
}