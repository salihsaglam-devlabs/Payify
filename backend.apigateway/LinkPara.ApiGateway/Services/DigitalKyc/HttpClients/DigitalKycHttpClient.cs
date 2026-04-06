using LinkPara.ApiGateway.Services.DigitalKyc.Models.Requests;
using LinkPara.ApiGateway.Services.DigitalKyc.Models.Responses;

namespace LinkPara.ApiGateway.Services.DigitalKyc.HttpClients;
public class DigitalKycHttpClient : HttpClientBase, IDigitalKycHttpClient
{
    public DigitalKycHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor)
        : base(client, httpContextAccessor)
    {
    }

    public async Task DigitalKycEndAsync(DigitalKycEndRequest request)
    {
        var response = await PostAsJsonAsync($"v1/DigitalKyc/integration-send", request);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException();
        }
    }

    public async Task<DigitalKycStartResponse> DigitalKycStartAsync(DigitalKycStartRequest request)
    {
        var response = await PostAsJsonAsync($"v1/DigitalKyc/integration-add", request);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException();
        }
        var integration = await response.Content.ReadFromJsonAsync<DigitalKycStartResponse>();
        return integration ?? throw new InvalidOperationException();
    }

    public async Task<bool> GetKycStateByUserId(string userId)
    {
        var response = await GetAsync($"v1/DigitalKyc/kyc-state/{userId}");
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException();
        }
        var result = await response.Content.ReadFromJsonAsync<bool>();
        return result;
    }

    public async Task<IntegrationGetResponse> IntegrationGetAsync(IntegrationGetRequest request)
    {
        var response = await PostAsJsonAsync($"v1/DigitalKyc/integration-get", request);
        var integration = await response.Content.ReadFromJsonAsync<IntegrationGetResponse>();
        return integration ?? throw new InvalidOperationException();
    }
}

