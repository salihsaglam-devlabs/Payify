namespace LinkPara.SharedModels.Exceptions;

public class BirthdateOutOfRangeException : CustomApiException
{
    public BirthdateOutOfRangeException(string message)
        : base(ErrorCode.BirthdateOutOfRange, message)
    {
    }
}