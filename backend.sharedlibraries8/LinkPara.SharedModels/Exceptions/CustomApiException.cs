namespace LinkPara.SharedModels.Exceptions;

public class CustomApiException : Exception
{
    public readonly string Code;

    public CustomApiException(string code, string message)
        : base(message)
    {
        Code = code;
    }
}