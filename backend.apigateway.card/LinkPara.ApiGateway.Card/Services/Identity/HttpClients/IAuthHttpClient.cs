using LinkPara.ApiGateway.Card.Services.Identity.Models.Requests;
using LinkPara.ApiGateway.Card.Services.Identity.Models.Responses;

namespace LinkPara.ApiGateway.Card.Services.Identity.HttpClients
{
    public interface IAuthHttpClient
    {
        Task<UserRefreshTokenResponse> RefreshTokenAsync(UserRefreshTokenRequest request);
        Task<UserSessionDto> GetUserSessionAsync(string sessionId);
        Task<GenerateTokenResponse> GenerateTokenAsync(GenerateTokenRequest request);
    }
}
