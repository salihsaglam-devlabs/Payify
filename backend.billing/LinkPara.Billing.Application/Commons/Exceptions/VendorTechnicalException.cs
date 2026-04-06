using LinkPara.SharedModels.Exceptions;

namespace LinkPara.Billing.Application.Commons.Exceptions;

public class VendorTechnicalException : ApiException
{
    public VendorTechnicalException(string errorMessage) 
        : base(ApiErrorCode.TechnicalError, errorMessage)
    {
    }
}