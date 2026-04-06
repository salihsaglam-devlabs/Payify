using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Enums;
using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Responses;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LinkPara.ApiGateway.Merchant.Services.Pf.HttpClients;

public class PfPaymentHttpClient : HttpClientBase, IPfPaymentHttpClient
{
    private readonly HttpClient _client;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public PfPaymentHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor) :
        base(client, httpContextAccessor)
    {
        _client = client;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<GetThreeDSessionResponse> GetThreeDSessionAsync(GetThreeDSessionRequest request)
    {
        AddHeaders<GetThreeDSessionRequest>(request);

        var response = await PostAsJsonAsync("v1/ThreeDS/GetThreedSession", request);

        var responseString = await response.Content.ReadAsStringAsync();
        var sessionResult = JsonSerializer.Deserialize<GetThreeDSessionResponse>(responseString,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return sessionResult;
    }

    public async Task<GetThreeDSessionResultResponse> GetThreeDSessionResultAsync(GetThreeDSessionResultRequest request)
    {
        AddHeaders<GetThreeDSessionResultRequest>(request);

        var response = await PostAsJsonAsync("v1/ThreeDS/GetThreedSessionResult", request);

        var responseString = await response.Content.ReadAsStringAsync();
        var sessionResult = JsonSerializer.Deserialize<GetThreeDSessionResultResponse>(responseString,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return sessionResult;
    }

    public async Task<PfReturnResponse> ReturnPaymentAsync(ReturnRequest returnRequest)
    {
        AddHeaders<ReturnRequest>(returnRequest);

        var response = await PostAsJsonAsync("v1/Payments/return", returnRequest);

        var responseString = await response.Content.ReadAsStringAsync();
        var returnResponse = JsonSerializer.Deserialize<PfReturnResponse>(responseString,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return returnResponse;
    }

    public async Task<PfReverseResponse> ReversePaymentAsync(ReverseRequest reverse)
    {
        AddHeaders<ReverseRequest>(reverse);

        var response = await PostAsJsonAsync("v1/Payments/reverse", reverse);

        var responseString = await response.Content.ReadAsStringAsync();
        var reverseResponse = JsonSerializer.Deserialize<PfReverseResponse>(responseString,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return reverseResponse;
    }

    public async Task<PfProvisionResponse> SavePaymentAsync(ProvisionRequest provision)
    {
        AddHeaders<ProvisionRequest>(provision);

        var response = await PostAsJsonAsync("v1/Payments/provision", provision);

        var responseString = await response.Content.ReadAsStringAsync();
        var provisionResponse = JsonSerializer.Deserialize<PfProvisionResponse>(responseString,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return provisionResponse;
    }
    
    public async Task<InquireResponse> InquirePaymentAsync(InquireRequest request)
    {
        AddHeaders<InquireRequest>(request);

        var response = await PostAsJsonAsync("v1/Payments/inquire", request);

        var responseString = await response.Content.ReadAsStringAsync();
        var inquireResponse = JsonSerializer.Deserialize<InquireResponse>(responseString,
            new JsonSerializerOptions {
                PropertyNameCaseInsensitive = true,
                Converters =
                {
                    new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: false)
                }
            });
        return inquireResponse;
    }

    private void AddHeaders<T>(T request) where T : RequestHeaderInfo
    {
        _client.DefaultRequestHeaders.Add("PublicKey", request.PublicKey);
        _client.DefaultRequestHeaders.Add("Nonce", request.Nonce);
        _client.DefaultRequestHeaders.Add("Signature", request.Signature);
        _client.DefaultRequestHeaders.Add("ConversationId", request.ConversationId);
        _client.DefaultRequestHeaders.Add("ClientIpAddress", _httpContextAccessor.HttpContext.Connection?.RemoteIpAddress?.MapToIPv4().ToString().Trim());
        _client.DefaultRequestHeaders.Add("MerchantNumber", request.MerchantNumber);
        _client.DefaultRequestHeaders.Add("Gateway", Gateway.Merchant.ToString());
    }
}
