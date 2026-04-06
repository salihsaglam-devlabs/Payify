using LinkPara.ApiGateway.CorporateWallet.Commons.Extensions;
using LinkPara.ApiGateway.CorporateWallet.Services.Identity.Models.Requests;

namespace LinkPara.ApiGateway.CorporateWallet.Services.Identity.HttpClients;

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

    public async Task<bool> CheckBirthdateAllowedRange(CheckBirthDateRequest request)
    {
        var queryString = request.GetQueryString();

        var response = await GetAsync($"v1/Account/check-birthdate" + queryString);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException();
        }
        var result = await response.Content.ReadFromJsonAsync<bool>();
        return result;
    }
}
