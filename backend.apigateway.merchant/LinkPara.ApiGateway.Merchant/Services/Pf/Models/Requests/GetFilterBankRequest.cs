using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.ApiGateway.Merchant.Services.Pf.Models.Requests;

public class GetFilterBankRequest : SearchQueryParams
{
    public RecordStatus? RecordStatus { get; set; }
}