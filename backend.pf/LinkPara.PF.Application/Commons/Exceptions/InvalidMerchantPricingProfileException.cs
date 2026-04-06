using LinkPara.SharedModels.Exceptions;

namespace LinkPara.PF.Application.Commons.Exceptions;

public class InvalidMerchantPricingProfileException : ApiException
{
    public InvalidMerchantPricingProfileException() 
        : base(ApiErrorCode.InvalidMerchantPricingProfile, "InvalidMerchantPricingProfile")
    {
    }
}