using LinkPara.SharedModels.Exceptions;

namespace LinkPara.Identity.Application.Common.Exceptions;
public class InvalidNewPhoneNumberException : CustomApiException
{
    public InvalidNewPhoneNumberException() 
        : base(ApiErrorCode.InvalidNewPhoneNumber, "Invalid new phone number!") { }
}
