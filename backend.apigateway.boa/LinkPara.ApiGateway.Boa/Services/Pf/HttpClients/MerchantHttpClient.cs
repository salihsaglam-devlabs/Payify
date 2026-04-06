using LinkPara.ApiGateway.Boa.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.Boa.Services.Pf.Models.Responses;

namespace LinkPara.ApiGateway.Boa.Services.Pf.HttpClients;

public class MerchantHttpClient : HttpClientBase, IMerchantHttpClient
{
    public MerchantHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor)
        : base(client, httpContextAccessor)
    {

    }
    
    public async Task<CreateBoaMerchantResponse> CreateBoaMerchantAsync(CreateBoaMerchantRequest request)
    {
        var response = await PostAsJsonAsync($"v1/BoaMerchants", request);
        var result = await response.Content.ReadFromJsonAsync<CreateBoaMerchantResponse>();
        return result ?? throw new InvalidOperationException();
    }

    public async Task<BoaMerchantDto> GetBoaMerchantAsync(string merchantNumber)
    {
        var response = await GetAsync($"v1/BoaMerchants/{merchantNumber}");
        var merchant = await response.Content.ReadFromJsonAsync<BoaMerchantDto>();
        return merchant ?? throw new InvalidOperationException();
    }
}