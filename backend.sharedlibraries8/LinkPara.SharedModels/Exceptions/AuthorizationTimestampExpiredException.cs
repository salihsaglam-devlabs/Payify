namespace LinkPara.SharedModels.Exceptions;

public class AuthorizationTimestampExpiredException : AuthorizationException 
{
    public AuthorizationTimestampExpiredException()
        : base(ErrorCode.AuthorizationTimestampExpired, "AuthorizationTimestampExpired")
    {
        
    }
}