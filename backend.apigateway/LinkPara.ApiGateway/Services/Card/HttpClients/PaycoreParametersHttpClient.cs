using System.Text.Json;
using LinkPara.ApiGateway.Services.Card.Models.PaycoreParameters.Response;

namespace LinkPara.ApiGateway.Services.Card.HttpClients;

public class PaycoreParametersHttpClient : HttpClientBase, IPaycoreParametersHttpClient
{
    public PaycoreParametersHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor)
        : base(client, httpContextAccessor)
    {
    }

    public async Task<GetProductsResponse> GetProductsAsync()
    {
        var response = await GetAsync("v1/PaycoreParameters");
        var responseString = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<GetProductsResponse>(responseString, JsonOptions())!;
    }

    private static JsonSerializerOptions JsonOptions()
    {
        return new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }
}