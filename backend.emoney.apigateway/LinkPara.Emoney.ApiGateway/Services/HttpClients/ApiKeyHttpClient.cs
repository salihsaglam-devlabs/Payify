using LinkPara.Emoney.ApiGateway.Models.Responses;
using System.Text.Json;

namespace LinkPara.Emoney.ApiGateway.Services.HttpClients;

public class ApiKeyHttpClient : HttpClientBase, IApiKeyHttpClient
{
    public ApiKeyHttpClient(HttpClient client) : base(client)
    {
    }

    public async Task<ApiKeyDto> GetApiKeyAsync(string publicKey)
    {
        var publicKeyBytes = System.Text.Encoding.UTF8.GetBytes(publicKey);
        var publicKeyEncoded = Convert.ToBase64String(publicKeyBytes);

        var response = await GetAsync($"v1/ApiKeys?PublicKeyEncoded={publicKeyEncoded}");
        var responseString = await response.Content.ReadAsStringAsync();
        var apiKey = JsonSerializer.Deserialize<ApiKeyDto>(responseString,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return apiKey ?? throw new InvalidOperationException();
    }
}
