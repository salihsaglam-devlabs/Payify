using LinkPara.SharedModels.Exceptions;

namespace LinkPara.Identity.Application.Common.Exceptions;

public class NewEmailAddressIsSameEmailException : ApiException
{
    public NewEmailAddressIsSameEmailException() 
        : base(ApiErrorCode.NewEmailAddressIsSameEmailException, "New Email Address Is Same Email!")
    {
    }
}