using LinkPara.Billing.Domain.Entities;
using LinkPara.SharedModels.Exceptions;

namespace LinkPara.Billing.Application.Commons.Exceptions;

public class InvalidExternalSectorException : ApiException
{
    public InvalidExternalSectorException(Vendor vendor, string externalSectorId)
        : base(ApiErrorCode.InvalidExternalSector, $"InvalidExternalSector: Vendor: {vendor.Name}, Id: {externalSectorId}")
    {

    }
}