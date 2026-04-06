using LinkPara.SharedModels.Exceptions;

namespace LinkPara.PF.Application.Commons.Exceptions;

public class PricingProfileItemNotFoundException : ApiException
{
    public PricingProfileItemNotFoundException() 
        : base(ApiErrorCode.PricingProfileItemNotFound, "PricingProfileItemNotFound")
    {
    }
}