using Elastic.Apm.Api;
using LinkPara.ApiGateway.Services.Identity.Models.Requests;
using LinkPara.ApiGateway.Services.Identity.Models.Responses;
using System.Text.Json;


namespace LinkPara.ApiGateway.Services.Identity.HttpClients
{
    public class AuthHttpClient : HttpClientBase, IAuthHttpClient
    {
        public AuthHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor)
            : base(client, httpContextAccessor)
        {
        }

        public async Task<TokenDto> LoginAsync(LoginRequest request)
        {
            var response = await PostAsJsonAsync("v1/auth/login", request);

            return await response.Content.ReadFromJsonAsync<TokenDto>();
        }

        public async Task<CheckPasswordResponse> CheckPasswordAsync(CheckPasswordRequest request)
        {
            var response = await PostAsJsonAsync("v1/auth/check-password", request);

            return await response.Content.ReadFromJsonAsync<CheckPasswordResponse>();
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

        public async Task MultifactorActivationAsync(MultifactorActivationRequest request)
        {
            await PostAsJsonAsync("v1/auth/multifactor-activation", request);
        }

        public async Task<StartClientTransactionResponse> StartClientTransaction(StartClientTransactionRequest request)
        {
            var response = await PostAsJsonAsync("v1/auth/start-client-transaction", request);

            return await response.Content.ReadFromJsonAsync<StartClientTransactionResponse>();

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
