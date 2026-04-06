using System.Runtime.Serialization;
using LinkPara.SharedModels.Exceptions;

namespace LinkPara.Emoney.Application.Commons.Exceptions;

[Serializable]
public class CommercialPricingStatusInvalidException : ApiException
{
    public CommercialPricingStatusInvalidException() 
        : base(ApiErrorCode.CommercialPricingStatusInvalid, "Pricing status is invalid!")
    {
    }
    
    protected CommercialPricingStatusInvalidException(SerializationInfo info, StreamingContext context) 
        : base(info, context)
    {
    }
}