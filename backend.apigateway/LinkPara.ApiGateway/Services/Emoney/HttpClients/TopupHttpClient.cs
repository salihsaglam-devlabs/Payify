using LinkPara.ApiGateway.Commons.Helpers;
using LinkPara.ApiGateway.Services.Emoney.Models.Requests;
using LinkPara.ApiGateway.Services.Emoney.Models.Responses;
using System.Globalization;
using System.Text.Json;

namespace LinkPara.ApiGateway.Services.Emoney.HttpClients;

public class TopupHttpClient : HttpClientBase, ITopupHttpClient
{
    private readonly IServiceRequestConverter _serviceRequestConverter;

    public TopupHttpClient(HttpClient client,
        IHttpContextAccessor httpContextAccessor,
        IServiceRequestConverter serviceRequestConverter)
        : base(client, httpContextAccessor)
    {
        _serviceRequestConverter = serviceRequestConverter;
    }

    public async Task<TopupPreviewResponse> GetTopupPreviewAsync(TopupPreviewRequest request)
    {
        var queryString = $"CardNumber={request.CardNumber}" +
            $"&Amount={request.Amount.ToString(CultureInfo.InvariantCulture)}" +
            $"&UserId={request.UserId}" +
            $"&WalletNumber={request.WalletNumber}" +
            $"&Currency={request.Currency}";

        var response = await GetAsync($"v1/Topups/preview?{queryString}");
        var responseString = await response.Content.ReadAsStringAsync();
        var topupPreviewResponse = JsonSerializer.Deserialize<TopupPreviewResponse>(responseString, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return topupPreviewResponse;
    }

    public async Task<TopupProcessResponse> TopupProcessAsync(TopupProcessRequest request)
    {
        var clientRequest = _serviceRequestConverter.Convert<TopupProcessRequest, TopupProcessServiceRequest>(request);
        var response = await PostAsJsonAsync($"v1/Topups/process", clientRequest);
        var responseString = await response.Content.ReadAsStringAsync();
        var topupProcess = JsonSerializer.Deserialize<TopupProcessResponse>(responseString,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return topupProcess;
    }

    public async Task<CardTokenResponse> GenerateCardTokenAsync(GenerateCardTokenRequest request)
    {
        var response = await PostAsJsonAsync("v1/Topups/token", request);

        var responseString = await response.Content.ReadAsStringAsync();
        var cardToken = JsonSerializer.Deserialize<CardTokenResponse>(responseString,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return cardToken;
    }

    public async Task<GetThreeDSessionResponse> GetThreeDSessionAsync(GetThreeDSessionRequest request)
    {
        var response = await PostAsJsonAsync("v1/Topups/getthreedsession", request);

        var responseString = await response.Content.ReadAsStringAsync();
        var threeDSessionResponse = JsonSerializer.Deserialize<GetThreeDSessionResponse>(responseString,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return threeDSessionResponse;
    }

    public async Task<GetThreeDSessionResultResponse> GetThreeDSessionResultAsync(GetThreeDSessionResultRequest request)
    {
        var response = await PostAsJsonAsync("v1/Topups/getthreedsessionresult", request);

        var responseString = await response.Content.ReadAsStringAsync();
        var threeDSessionResponse = JsonSerializer.Deserialize<GetThreeDSessionResultResponse>(responseString,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return threeDSessionResponse;
    }

    public async Task<Init3dsResponse> Init3dsAsync(Init3dsRequest request)
    {
        var response = await PostAsJsonAsync("v1/Topups/init3ds", request);

        var responseString = await response.Content.ReadAsStringAsync();
        var init3dResponse = JsonSerializer.Deserialize<Init3dsResponse>(responseString,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return init3dResponse;
    }
}
