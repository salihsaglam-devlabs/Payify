namespace LinkPara.SharedModels.Exceptions;

public class InvalidImageFormatException : CustomApiException
{
    public InvalidImageFormatException(string message)
        : base(ErrorCode.InvalidImageFormat, message)
    {
    }
}