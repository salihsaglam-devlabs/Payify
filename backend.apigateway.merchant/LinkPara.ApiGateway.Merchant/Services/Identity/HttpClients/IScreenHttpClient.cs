using LinkPara.ApiGateway.Merchant.Services.Identity.Models.Requests;
using LinkPara.ApiGateway.Merchant.Services.Identity.Models.Responses;

namespace LinkPara.ApiGateway.Merchant.Services.Identity.HttpClients
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
