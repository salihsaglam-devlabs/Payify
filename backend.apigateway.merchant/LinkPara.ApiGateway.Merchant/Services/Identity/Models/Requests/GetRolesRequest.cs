using LinkPara.ApiGateway.Merchant.Services.Identity.Models.Enums;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.ApiGateway.Merchant.Services.Identity.Models.Requests;

public class GetRolesRequest : SearchQueryParams
{
    public string Name { get; set; }
    public RoleScope[] RoleScopes { get; set; }
    public RecordStatus? RecordStatus { get; set; }

}
