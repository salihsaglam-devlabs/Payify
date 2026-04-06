using LinkPara.ApiGateway.BackOffice.Services.Epin.Models.Enums;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.Epin.Models.Requests;

public class GetFilterReconciliationSummariesRequest : SearchQueryParams
{
    public DateTime? ReconciliationDateStart { get; set; }
    public DateTime? ReconciliationDateEnd { get; set; }
    public EpinReconciliationStatus? ReconciliationStatus { get; set; }
    public Organization? Organisation { get; set; }
}
