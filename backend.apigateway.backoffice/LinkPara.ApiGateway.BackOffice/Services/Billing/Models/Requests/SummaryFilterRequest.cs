using LinkPara.ApiGateway.BackOffice.Services.Billing.Enums;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.Billing.Models.Requests;

public class SummaryFilterRequest : SearchQueryParams
{
    public Guid? VendorId { get; set; }
    public DateTime ReconciliationStartDate { get; set; }
    public DateTime ReconciliationEndDate { get; set; }
    public ReconciliationStatus? ReconciliationStatus { get; set; }
}
