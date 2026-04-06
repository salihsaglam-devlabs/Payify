using LinkPara.ApiGateway.BackOffice.Services.Identity.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Identity.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.Identity.HttpClients;

public interface IRoleHttpClient
{
    Task<PaginatedList<RoleDto>> GetAllRolesAsync(GetRolesRequest request);

    Task<RoleDetailDto> GetRoleAsync(string roleId);

    Task CreateRoleAsync(RoleRequest request);

    Task UpdateRoleAsync(UpdateRoleRequest request);

    Task DeleteRoleAsync(string roleId);

    Task<List<UserDto>> GetUsersByRoleIdAsync(Guid roleId);
}