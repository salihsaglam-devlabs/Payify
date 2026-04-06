using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;

public class GetParentMerchantPermissionsRequest : SearchQueryParams
{
    public Guid[] MainSubMerchantIdList { get; set; }
}
