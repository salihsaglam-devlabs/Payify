using LinkPara.ApiGateway.Commons.Helpers;
using LinkPara.ApiGateway.Services.Emoney.Models.Requests;
using LinkPara.ApiGateway.Services.Emoney.Models.Responses;
using System.Net;
using System.Text.Json;

namespace LinkPara.ApiGateway.Services.Emoney.HttpClients;

public class MasterpassHttpClient : HttpClientBase, IMasterpassHttpClient
{
    private readonly IServiceRequestConverter _serviceRequestConverter;

    public MasterpassHttpClient(HttpClient client,
        IHttpContextAccessor httpContextAccessor,
        IServiceRequestConverter serviceRequestConverter) 
        : base(client, httpContextAccessor)
    {
        _serviceRequestConverter = serviceRequestConverter;
    }

    public async Task<BaseResponse<AccountUnlinkResponse>> AccountUnlinkAccountAsync(AccountUnlinkRequest request)
    {
        var clientRequest = _serviceRequestConverter.Convert<AccountUnlinkRequest, AccountUnlinkServiceRequest>(request);
        var response = await PostAsJsonAsync("v1/Masterpass/account-unlink", clientRequest);

        var responseString = await response.Content.ReadAsStringAsync();
        var accountUnlinkResponse = JsonSerializer.Deserialize<BaseResponse<AccountUnlinkResponse>>(responseString,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return accountUnlinkResponse;
    }

    public async Task<BaseResponse<GenerateAccessTokenResponse>> GenerateAccessTokenAsync(GenerateAccessTokenRequest request)
    {
        var clientRequest = _serviceRequestConverter.Convert<GenerateAccessTokenRequest, GenerateAccessTokenServiceRequest>(request);
        var response = await PostAsJsonAsync("v1/Masterpass/access-token", clientRequest);
        var responseString = await response.Content.ReadAsStringAsync();
        var accessToken = JsonSerializer.Deserialize<BaseResponse<GenerateAccessTokenResponse>>(responseString,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return accessToken;
    }

    public async Task ThreedSecureAsync(ThreedSecureRequest request)
    {
        var response = await PostAsJsonAsync("v1/Masterpass/threed-secure", request);
        if (!response.IsSuccessStatusCode) throw new InvalidOperationException();
    }

    public async Task<TopupProcessResponse> TopupProcessAsync(MasterpassTopupProcessRequest request)
    {
        var clientRequest = _serviceRequestConverter.Convert<MasterpassTopupProcessRequest, MasterpassTopupProcessServiceRequest>(request);
        var response = await PostAsJsonAsync($"v1/Masterpass/process", clientRequest);
        var responseString = await response.Content.ReadAsStringAsync();
        var topupProcess = JsonSerializer.Deserialize<TopupProcessResponse>(responseString,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return topupProcess;
    }

    public async Task<ValidateThreedSecureResponse> ValidateThreedSecureAsync(string orderId)
    {
        var response = await GetAsync($"v1/Masterpass/validate-threed-secure?orderId={orderId}");
        var responseString = await response.Content.ReadAsStringAsync();
        var validateThreedSecureResponse = JsonSerializer.Deserialize<ValidateThreedSecureResponse>(responseString, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return validateThreedSecureResponse;
    }
}
