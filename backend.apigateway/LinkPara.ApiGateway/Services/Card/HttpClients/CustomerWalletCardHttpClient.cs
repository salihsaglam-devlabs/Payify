using System.Text.Json;
using LinkPara.ApiGateway.Services.Card.Models.CustomerWalletCard.Request;
using LinkPara.ApiGateway.Services.Card.Models.CustomerWalletCard.Response;

namespace LinkPara.ApiGateway.Services.Card.HttpClients;

public class CustomerWalletCardHttpClient : HttpClientBase, ICustomerWalletCardHttpClient
{
    private ICustomerWalletCardHttpClient _customerWalletCardHttpClientImplementation;

    public CustomerWalletCardHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor)
        : base(client, httpContextAccessor)
    {
    }

    public async Task<GetCustomerWalletCardsResponse> GetCustomerWalletCardsAsync(GetCustomerWalletCardsRequest request)
    {
        var url = CreateUrlWithProperties("v1/CustomerWalletCard/customer-cards", request);
        var response = await GetAsync(url);
        var responseString = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<GetCustomerWalletCardsResponse>(responseString, JsonOptions())!;
    }

    private static JsonSerializerOptions JsonOptions()
    {
        return new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }
}