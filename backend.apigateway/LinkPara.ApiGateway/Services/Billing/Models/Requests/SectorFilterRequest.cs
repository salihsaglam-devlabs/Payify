using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.ApiGateway.Services.Billing.Models.Requests;

public class SectorFilterRequest : SearchQueryParams
{
    public RecordStatus? RecordStatus { get; set; }
}