namespace LinkPara.SharedModels.Exceptions;

public class AuthorizationSignatureMismatchException : AuthorizationException
{
    public AuthorizationSignatureMismatchException()
        : base(ErrorCode.AuthorizationSignatureMismatch, "HmacSignatureMismatch")
    {
        
    }
}