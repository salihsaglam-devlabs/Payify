using LinkPara.SharedModels.Exceptions;

namespace LinkPara.Identity.Application.Common.Exceptions;

public class InvalidNewEmailException : ApiException
{
    public InvalidNewEmailException() 
        : base(ApiErrorCode.InvalidNewEmail, "Invalid new email!") { }

}