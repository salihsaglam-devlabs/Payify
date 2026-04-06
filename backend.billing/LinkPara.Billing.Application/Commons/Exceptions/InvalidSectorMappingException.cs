using LinkPara.Billing.Domain.Entities;
using LinkPara.SharedModels.Exceptions;

namespace LinkPara.Billing.Application.Commons.Exceptions;

public class InvalidSectorMappingException : ApiException
{
    public InvalidSectorMappingException(Vendor vendor, Guid sectorId) 
        : base(ApiErrorCode.InvalidSectorMapping, $"InvalidSectorMapping: Vendor: {vendor.Name}, Id: {sectorId}")
    {

    }
}