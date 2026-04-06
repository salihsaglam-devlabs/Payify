using LinkPara.ApiGateway.BackOffice.Commons.Extensions;
using LinkPara.ApiGateway.BackOffice.Commons.Helpers;
using LinkPara.ApiGateway.BackOffice.Services.Identity.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Identity.Models.Responses;
using System.Text.Json;

namespace LinkPara.ApiGateway.BackOffice.Services.Identity.HttpClients;

public class AccountHttpClient : HttpClientBase, IAccountHttpClient
{
    private readonly IServiceRequestConverter _serviceRequestConverter;

    public AccountHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor,
        IServiceRequestConverter serviceRequestConverter)
        : base(client, httpContextAccessor)
    {
        _serviceRequestConverter = serviceRequestConverter;
    }

    public async Task ForgotPasswordAsync(ForgotPasswordRequest request)
    {
        await PostAsJsonAsync($"v1/Account/forgot-password", request);
    }

    public async Task<GetEmailUpdateTokenResponse> GetEmailUpdateTokenAsync(GetEmailUpdateTokenRequest request)
    {
        var clientRequest = _serviceRequestConverter.Convert<GetEmailUpdateTokenRequest, GetEmailUpdateTokenServiceRequest>(request);

        var queryString = clientRequest.GetQueryString();

        var response = await GetAsync($"v1/Account/update-email/token" + queryString);

        var responseString = await response.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<GetEmailUpdateTokenResponse>(responseString,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }

    public async Task<string> GetPasswordResetTokenAsync(SendForgotPasswordEmailRequest request)
    {
        var response = await PostAsJsonAsync($"v1/Account/forgot-password/email", request);
        return await response.Content.ReadAsStringAsync();
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        var response = await PostAsJsonAsync("v1/Account/login", request);

        return await response.Content.ReadFromJsonAsync<LoginResponse>();
    }

    public async Task ResetPasswordAsync(ResetPasswordRequest request)
    {
        await PostAsJsonAsync($"v1/Account/reset-password", request);
    }

    public async Task UpdateEmailAsync(UpdateEmailRequest request)
    {
        var clientRequest = _serviceRequestConverter.Convert<UpdateEmailRequest, UpdateEmailServiceRequest>(request);

        await PostAsJsonAsync($"v1/Account/update-email", clientRequest);
    }
}
