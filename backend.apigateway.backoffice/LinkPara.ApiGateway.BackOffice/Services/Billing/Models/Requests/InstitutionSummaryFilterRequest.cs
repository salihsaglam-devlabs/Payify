using LinkPara.ApiGateway.BackOffice.Services.Billing.Enums;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.Billing.Models.Requests;

public class InstitutionSummaryFilterRequest : SearchQueryParams
{
    public Guid? VendorId { get; set; }
    public Guid? InstitutionId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public ReconciliationStatus? ReconciliationStatus { get; set; }
}