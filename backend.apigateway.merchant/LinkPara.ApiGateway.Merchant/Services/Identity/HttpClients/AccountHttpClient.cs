using LinkPara.ApiGateway.Merchant.Services.Identity.Models.Requests;

namespace LinkPara.ApiGateway.Merchant.Services.Identity.HttpClients;

public class AccountHttpClient : HttpClientBase, IAccountHttpClient
{
    public AccountHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor)
        : base(client, httpContextAccessor)
    {
    }

    public async Task ForgotPasswordAsync(ForgotPasswordRequest request)
    {
        await PostAsJsonAsync($"v1/Account/forgot-password", request);
    }

    public async Task<string> GetPasswordResetTokenAsync(SendForgotPasswordEmailRequest request)
    {
        var response = await PostAsJsonAsync($"v1/Account/forgot-password/email", request);
        return await response.Content.ReadAsStringAsync();
    }

    public async Task ResetPasswordAsync(ResetPasswordRequest request)
    {
        await PostAsJsonAsync($"v1/Account/reset-password", request);
    }
}
