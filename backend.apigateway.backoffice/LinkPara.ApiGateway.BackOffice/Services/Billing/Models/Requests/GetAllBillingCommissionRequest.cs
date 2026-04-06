using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.Billing.Models.Requests;

public class GetAllBillingCommissionRequest : SearchQueryParams
{
    public Guid? InstitutionId { get; set; }
    public Guid? VendorId { get; set; }
    public PaymentSource? PaymentType { get; set; }
}