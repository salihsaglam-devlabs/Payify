using LinkPara.ApiGateway.CorporateWallet.Services.Identity.Models.Requests;
using LinkPara.ApiGateway.CorporateWallet.Services.Identity.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.CorporateWallet.Services.Identity.HttpClients;

public interface IRoleHttpClient
{
    Task<PaginatedList<RoleDto>> GetAllRolesAsync(GetRolesRequest request);
}