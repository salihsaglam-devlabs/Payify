namespace LinkPara.SharedModels.Exceptions;

public class InvalidAuthorizationHeaderException : AuthorizationException
{
    public InvalidAuthorizationHeaderException()
        : base(ErrorCode.InvalidAuthorizationHeader, "InvalidAuthorizationHeader")
    {
        
    }
}