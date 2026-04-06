using LinkPara.SharedModels.Exceptions;

namespace LinkPara.PF.Application.Commons.Exceptions;

public class InvalidPricingProfileTypeException : ApiException
{
    public InvalidPricingProfileTypeException() 
        : base(ApiErrorCode.InvalidPricingProfileType, "InvalidPricingProfileType")
    {
    }
}