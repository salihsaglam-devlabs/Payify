namespace LinkPara.SharedModels.Exceptions;

public class InvalidFileExtensionException : CustomApiException
{
    public InvalidFileExtensionException(string message)
        : base(ErrorCode.InvalidFileExtension, message)
    {
    }
}