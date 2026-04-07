using System.Text.Json;
using LinkPara.ApiGateway.Services.Card.Models.PaycoreCustomer.Request;
using LinkPara.ApiGateway.Services.Card.Models.PaycoreCustomer.Response;
using LinkPara.ApiGateway.Services.Card.Models.Shared;

namespace LinkPara.ApiGateway.Services.Card.HttpClients;

public class PaycoreCustomerHttpClient : HttpClientBase, IPaycoreCustomerHttpClient
{
    private IPaycoreCustomerHttpClient _paycoreCustomerHttpClientImplementation;

    public PaycoreCustomerHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor)
        : base(client, httpContextAccessor)
    {
    }

    public async Task<PaycoreResponse> CreateCustomerAsync(CreateCustomerRequest request)
    {
        var response = await PostAsJsonAsync("v1/PaycoreCustomer", request);
        var responseString = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<PaycoreResponse>(responseString, JsonOptions())!;
    }

    public async Task<GetCustomerInformationResponse> GetCustomerInformationAsync(GetCustomerInformationRequest request)
    {
        var url = CreateUrlWithProperties("v1/PaycoreCustomer", request);
        var response = await GetAsync(url);
        var responseString = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<GetCustomerInformationResponse>(responseString, JsonOptions())!;
    }

    public async Task<List<GetCustomerCardsResponse>> GetCustomerCardsAsync(GetCustomerCardsRequest request)
    {
        var url = CreateUrlWithProperties("v1/PaycoreCustomer/cards", request);
        var response = await GetAsync(url);
        var responseString = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<List<GetCustomerCardsResponse>>(responseString, JsonOptions())!;
    }

    public async Task<List<GetCustomerLimitInfoResponse>> GetCustomerLimitInfoAsync(GetCustomerLimitInfoRequest request)
    {
        var url = CreateUrlWithProperties("v1/PaycoreCustomer/limit-info", request);
        var response = await GetAsync(url);
        var responseString = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<List<GetCustomerLimitInfoResponse>>(responseString, JsonOptions())!;
    }

    public async Task<PaycoreResponse> UpdateCustomerAsync(UpdateCustomerRequest request)
    {
        var response = await PutAsJsonAsync("v1/PaycoreCustomer/customer", request);
        var responseString = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<PaycoreResponse>(responseString, JsonOptions())!;
    }

    public async Task<PaycoreResponse> UpdateCustomerCommunicationAsync(UpdateCustomerCommunicationRequest request)
    {
        var response = await PutAsJsonAsync("v1/PaycoreCustomer/communication", request);
        var responseString = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<PaycoreResponse>(responseString, JsonOptions())!;
    }

    public async Task<PaycoreResponse> UpdateCustomerAddressAsync(UpdateCustomerAddressRequest request)
    {
        var response = await PutAsJsonAsync("v1/PaycoreCustomer/address", request);
        var responseString = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<PaycoreResponse>(responseString, JsonOptions())!;
    }

    public async Task<PaycoreResponse> UpdateCustomerLimitAsync(UpdateCustomerLimitRequest request)
    {
        var response = await PutAsJsonAsync("v1/PaycoreCustomer/limit", request);
        var responseString = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<PaycoreResponse>(responseString, JsonOptions())!;
    }

    private static JsonSerializerOptions JsonOptions()
    {
        return new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }
}