using LinkPara.Billing.Domain.Entities;
using LinkPara.SharedModels.Exceptions;

namespace LinkPara.Billing.Application.Commons.Exceptions;

public class InvalidBillStatusException : ApiException
{
    public InvalidBillStatusException(Vendor vendor, string errorMessage) 
        : base(ApiErrorCode.InvalidBillStatus, $"ErrorMappingBillStatus: Vendor: {vendor.Name}, Error: {errorMessage}")
    {
    }
}