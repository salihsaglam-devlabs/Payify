using LinkPara.SharedModels.Exceptions;

namespace LinkPara.Identity.Application.Common.Exceptions;

public class LoginFailedException : CustomApiException
{
    public LoginFailedException()
        : base(ApiErrorCode.LoginFailed, "Login failed! Check your username and password.") { }
    
    public LoginFailedException(string code, string message) 
        : base(code, message) { }
}