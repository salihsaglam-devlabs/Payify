namespace LinkPara.SharedModels.Exceptions;

public class InternalServiceException : Exception
{
    public readonly string Code;

    public InternalServiceException(string code, string message)
        : base(message)
    {
        Code = code;
    }
}