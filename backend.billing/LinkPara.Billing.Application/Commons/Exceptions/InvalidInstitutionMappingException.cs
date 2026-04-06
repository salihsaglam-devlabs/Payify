using LinkPara.Billing.Domain.Entities;
using LinkPara.SharedModels.Exceptions;

namespace LinkPara.Billing.Application.Commons.Exceptions;

public class InvalidInstitutionMappingException : ApiException
{
    public InvalidInstitutionMappingException(Vendor vendor, Guid InstitutionId) 
        : base(ApiErrorCode.InvalidInstitutionMapping, $"InvalidInstitutionMapping: Vendor: {vendor.Name}, Id: {InstitutionId}") 
    { 
    
    }
}
