using LinkPara.ApiGateway.Commons.Extensions;
using LinkPara.ApiGateway.Commons.Helpers;
using LinkPara.ApiGateway.Services.Identity.Models.Requests;
using LinkPara.ApiGateway.Services.Identity.Models.Responses;
using System.Text.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace LinkPara.ApiGateway.Services.Identity.HttpClients;

public class AccountHttpClient : HttpClientBase, IAccountHttpClient
{
    private readonly IServiceRequestConverter _serviceRequestConverter;

    public AccountHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor, 
        IServiceRequestConverter serviceRequestConverter)
        : base(client, httpContextAccessor)
    {
        _serviceRequestConverter = serviceRequestConverter;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        var response = await PostAsJsonAsync("v1/account/login", request);

        return await response.Content.ReadFromJsonAsync<LoginResponse>();
    }

    public async Task<RegisterResponse> RegisterAsync(RegisterWithUserName request)
    {
        var response = await PostAsJsonAsync("v1/account/register", request);

        return await response.Content.ReadFromJsonAsync<RegisterResponse>();
    }

    public async Task ResetPasswordAsync(ResetPasswordRequest request)
    {
        await PostAsJsonAsync($"v1/account/reset-password", request);
    }

    public async Task<string> GetPasswordResetTokenAsync(SendForgotPasswordEmailRequest request)
    {
        request.UserName = request.UserName.Replace(" ", "");
        var response = await PostAsJsonAsync($"v1/account/forgot-password/email", request);
        return await response.Content.ReadAsStringAsync();
    }

    public async Task ForgotPasswordAsync(ForgotPasswordRequest request)
    {
        await PostAsJsonAsync($"v1/account/forgot-password", request);
    }

    public async Task<GetEmailUpdateTokenResponse> GetEmailUpdateTokenAsync(GetEmailUpdateTokenRequest request)
    {
        var clientRequest = _serviceRequestConverter.Convert<GetEmailUpdateTokenRequest, GetEmailUpdateTokenServiceRequest>(request);

        var queryString = clientRequest.GetQueryString();

        var response = await GetAsync($"v1/account/update-email/token" + queryString);

        var responseString = await response.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<GetEmailUpdateTokenResponse>(responseString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }

    public async Task UpdateEmailAsync(UpdateEmailRequest request)
    {
        var clientRequest = _serviceRequestConverter.Convert<UpdateEmailRequest, UpdateEmailServiceRequest>(request);

        await PostAsJsonAsync($"v1/account/update-email", clientRequest);
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

    public async Task<GetResetPasswordTokenResponse> GetPasswordResetTokenAndEmailAsync(GetPasswordResetTokenAndEmailRequest request)
    {
        request.UserName = request.UserName.Replace(" ", "");
        var response = await PostAsJsonAsync($"v1/account/forgot-password/email-otp", request);
        return await response.Content.ReadFromJsonAsync<GetResetPasswordTokenResponse>();
    }

    public async Task<GetPhoneNumberTokenResponse> GetPhoneNumberUpdateTokenAsync(GetPhoneNumberUpdateTokenRequest request)
    {
        var clientRequest = _serviceRequestConverter.Convert<GetPhoneNumberUpdateTokenRequest, GetPhoneNumberUpdateTokenServiceRequest>(request);

        var queryString = clientRequest.GetQueryString();

        var response = await GetAsync($"v1/account/update-phone/token" + queryString);

        var responseString = await response.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<GetPhoneNumberTokenResponse>(responseString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }

    public async Task UpdatePhoneNumberAsync(UpdatePhoneNumberRequest request)
    {
        var clientRequest = _serviceRequestConverter.Convert<UpdatePhoneNumberRequest, UpdatePhoneNumberServiceRequest>(request);

        await PostAsJsonAsync($"v1/account/update-phone-number", clientRequest);
    }
}