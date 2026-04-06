using LinkPara.Billing.Domain.Entities;
using LinkPara.SharedModels.Exceptions;
namespace LinkPara.Billing.Application.Commons.Exceptions;

public class InvalidExternalInstitutionException : ApiException
{
    public InvalidExternalInstitutionException(Vendor vendor, string ExternalCode)
        : base(ApiErrorCode.InvalidExternalInstitution, $"InvalidExternalInstitution: Vendor: {vendor.Name}, Id: {ExternalCode}")
    {

    }
}