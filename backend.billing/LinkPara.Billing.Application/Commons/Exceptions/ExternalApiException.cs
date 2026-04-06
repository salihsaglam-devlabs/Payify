using LinkPara.Billing.Domain.Entities;
using LinkPara.SharedModels.Exceptions;

namespace LinkPara.Billing.Application.Commons.Exceptions;

public class ExternalApiException : ApiException
{
    public ExternalApiException(string errorMessage)
        :base(ApiErrorCode.ExternalApiError, errorMessage)
    {

    }

    public ExternalApiException(Vendor vendor, string errorMessage) 
        : base(ApiErrorCode.ExternalApiError, $"ErrorInApi: {vendor.Name}, Error: {errorMessage}")
    {
    }
}