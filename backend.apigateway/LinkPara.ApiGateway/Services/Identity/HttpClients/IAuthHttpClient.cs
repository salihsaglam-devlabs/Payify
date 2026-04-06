using LinkPara.ApiGateway.Services.Identity.Models.Requests;
using LinkPara.ApiGateway.Services.Identity.Models.Responses;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.Services.Identity.HttpClients
{
    public interface IAuthHttpClient
    {
        Task<TokenDto> LoginAsync(LoginRequest request);
        Task LogoutAsync(LogoutRequest request);
        Task<UserRefreshTokenResponse> RefreshTokenAsync(UserRefreshTokenRequest request);
        public Task MultifactorActivationAsync(MultifactorActivationRequest request);
        Task<UserSessionDto> GetUserSessionAsync(string sessionId);
        Task<CheckPasswordResponse> CheckPasswordAsync(CheckPasswordRequest request);

    }
}
