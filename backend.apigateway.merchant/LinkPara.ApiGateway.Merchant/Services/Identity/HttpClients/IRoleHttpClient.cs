using LinkPara.ApiGateway.Merchant.Services.Identity.Models.Requests;
using LinkPara.ApiGateway.Merchant.Services.Identity.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.Merchant.Services.Identity.HttpClients;

public interface IRoleHttpClient
{
    Task<PaginatedList<RoleDto>> GetAllRolesAsync(GetRolesRequest request);
}