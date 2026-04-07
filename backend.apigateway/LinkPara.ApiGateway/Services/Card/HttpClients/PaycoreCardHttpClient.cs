using System.Text.Json;
using LinkPara.ApiGateway.Services.Card.Models.PaycoreCard.Request;
using LinkPara.ApiGateway.Services.Card.Models.PaycoreCard.Response;
using LinkPara.ApiGateway.Services.Card.Models.Shared;

namespace LinkPara.ApiGateway.Services.Card.HttpClients;

public class PaycoreCardHttpClient : HttpClientBase, IPaycoreCardHttpClient
{
    public PaycoreCardHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor)
        : base(client, httpContextAccessor)
    {
    }

    public async Task<PaycoreResponse> CreateCardAsync(CreateCardRequest request)
    {
        var response = await PostAsJsonAsync("v1/PaycoreCard", request);
        var responseString = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<PaycoreResponse>(responseString, JsonOptions())!;
    }

    public async Task<UpdateCardStatusResponse> UpdateCardStatusAsync(UpdateCardStatusRequest request)
    {
        var response = await PutAsJsonAsync("v1/PaycoreCard/status", request);
        var responseString = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<UpdateCardStatusResponse>(responseString, JsonOptions())!;
    }

    public async Task<GetCardAuthorizationsResponse> GetCardAuthorizationsAsync(GetCardAuthorizationsRequest request)
    {
        var url = CreateUrlWithProperties("v1/PaycoreCard/card-authorizations", request);
        var response = await GetAsync(url);
        var responseString = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<GetCardAuthorizationsResponse>(responseString, JsonOptions())!;
    }

    public async Task<List<GetCardInformationsResponse>> GetCardInformationsAsync(GetCardInformationsRequest request)
    {
        var url = CreateUrlWithProperties("v1/PaycoreCard/card-info", request);
        var response = await GetAsync(url);
        var responseString = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<List<GetCardInformationsResponse>>(responseString, JsonOptions())!;
    }

    public async Task<PaycoreResponse> UpdateCardAuthorizationsAsync(UpdateCardAuthorizationRequest request)
    {
        var response = await PutAsJsonAsync("v1/PaycoreCard/card-authorization", request);
        var responseString = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<PaycoreResponse>(responseString, JsonOptions())!;
    }

    public async Task<GetCardTransactionsResponse> GetCardTransactionsAsync(GetCardTransactionsRequest request)
    {
        var url = CreateUrlWithProperties("v1/PaycoreCard/transactions", request);
        var response = await GetAsync(url);
        var responseString = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<GetCardTransactionsResponse>(responseString, JsonOptions())!;
    }

    public async Task<GetCardLastCourierActivityResponse> GetCardLastCourierActivityAsync(GetCardLastCourierActivityRequest request)
    {
        var url = CreateUrlWithProperties("v1/PaycoreCard/card-last-courier-activity", request);
        var response = await GetAsync(url);
        var responseString = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<GetCardLastCourierActivityResponse>(responseString, JsonOptions())!;
    }

    public async Task<AddAdditionalLimitRestrictionResponse> AddAdditionalLimitRestrictionAsync(AddAdditionalLimitRestrictionRequest request)
    {
        var response = await PutAsJsonAsync("v1/PaycoreCard/additional-limit-restriction", request);
        var responseString = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<AddAdditionalLimitRestrictionResponse>(responseString, JsonOptions())!;
    }

    public async Task<GetCardSensitiveDataResponse> GetCardSensitiveDataAsync(GetCardSensitiveDataRequest request)
    {
        var url = CreateUrlWithProperties("v1/PaycoreCard/card-sensitive-data", request);
        var response = await GetAsync(url);
        var responseString = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<GetCardSensitiveDataResponse>(responseString, JsonOptions())!;
    }

    private static JsonSerializerOptions JsonOptions()
    {
        return new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }
}