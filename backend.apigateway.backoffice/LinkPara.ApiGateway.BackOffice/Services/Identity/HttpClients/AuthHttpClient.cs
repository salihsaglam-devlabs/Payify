using LinkPara.ApiGateway.BackOffice.Services.Identity.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Identity.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace LinkPara.ApiGateway.BackOffice.Services.Identity.HttpClients
{
    public class AuthHttpClient : HttpClientBase, IAuthHttpClient
    {
        public AuthHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor)
            : base(client, httpContextAccessor)
        {
        }

        public async Task<ActionResult<PaginatedList<ActiveUserDto>>> GetAllActiveUsersAsync(AllActiveUserRequest query)
        {
            var url = CreateUrlWithParams($"v1/auth", query, true);
            var response = await GetAsync(url);
            var activeUsers = await response.Content.ReadFromJsonAsync<PaginatedList<ActiveUserDto>>();
            return activeUsers ?? throw new InvalidOperationException();
        }

        public async Task<TokenDto> LoginAsync(LoginRequest request)
        {
            var response = await PostAsJsonAsync("v1/auth/login", request);

            return await response.Content.ReadFromJsonAsync<TokenDto>();
        }
        public async Task LogoutAsync(LogoutRequest request)
        {
            var response = await PostAsJsonAsync("v1/auth/logout", request);

            if (!response.IsSuccessStatusCode) throw new InvalidOperationException();
        }
        public async Task<UserRefreshTokenResponse> RefreshTokenAsync(UserRefreshTokenRequest request)
        {
            var response = await PostAsJsonAsync("v1/auth/refresh-token", request);

            return await response.Content.ReadFromJsonAsync<UserRefreshTokenResponse>();
        }

        public async Task RevokeRefreshTokenAsync(RevokeRefreshTokenRequest request)
        {
            var response = await PostAsJsonAsync("v1/auth/revoke-refresh-token", request);

            if (!response.IsSuccessStatusCode) throw new InvalidOperationException();
        }
        public async Task<UserSessionDto> GetUserSessionAsync(string sessionId)
        {
            var response = await GetAsync($"v1/auth/session/{sessionId}");
            var responseString = await response.Content.ReadAsStringAsync();

            var userSession = JsonSerializer.Deserialize<UserSessionDto>(responseString, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return userSession ?? throw new InvalidOperationException();
        }

    }
}
