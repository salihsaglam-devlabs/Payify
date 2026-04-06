using LinkPara.ApiGateway.BackOffice.Services.Identity.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Identity.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.BackOffice.Services.Identity.HttpClients
{
    public interface IAuthHttpClient
    {
        Task<TokenDto> LoginAsync(LoginRequest request);
        Task LogoutAsync(LogoutRequest request);
        Task<UserRefreshTokenResponse> RefreshTokenAsync(UserRefreshTokenRequest request);
        Task RevokeRefreshTokenAsync(RevokeRefreshTokenRequest request);
        Task<ActionResult<PaginatedList<ActiveUserDto>>> GetAllActiveUsersAsync(AllActiveUserRequest query);
        Task<UserSessionDto> GetUserSessionAsync(string sessionId);
    }
}
