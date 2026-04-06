using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.ApiGateway.CorporateWallet.Services.Billing.Models.Requests;

public class SectorFilterRequest : SearchQueryParams
{
    public RecordStatus? RecordStatus { get; set; }
}