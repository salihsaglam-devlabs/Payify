using LinkPara.ApiGateway.BackOffice.Services.Identity.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Identity.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.Identity.HttpClients
{
    public interface IScreenHttpClient
    {
        Task<List<ScreenDto>> GetAllScreensAsync(GetRoleScreensRequest request);

        Task<List<ScreenDto>> GetUserRoleScreenAsync(string userId);
        Task<RoleScreenDto> GetRoleScreenAsync(string roleId);
        Task UpdateRoleScreenAsync(UpdateRoleScreenRequest request);
        Task CreateRoleScreenAsync(RoleScreenRequest request);
        Task DeleteRoleScreenAsync(string roleId);

    }
}
