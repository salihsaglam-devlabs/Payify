using LinkPara.ApiGateway.Services.DigitalKyc.Models.Requests.ScSoft;

namespace LinkPara.ApiGateway.Services.DigitalKyc.HttpClients;
public class ScSoftHttpClient : HttpClientBase, IScSoftHttpClient
{
    public ScSoftHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor) 
        : base(client, httpContextAccessor)
    {
    }
    public async Task<ScSoftAtapiResponse> CheckIdentityIsNewAsync(CheckIdentityIsNewRequest request)
    {
        var response = await PostAsJsonAsync($"v1/ScSoft/check-id", request);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException();
        }
        var session = await response.Content.ReadFromJsonAsync<ScSoftAtapiResponse>();
        return session ?? throw new InvalidOperationException();
    }
    public async Task<ScSoftAtapiResponse> CheckKpsInformationsAsync(CheckKpsInformationsRequest request)
    {
        var response = await PostAsJsonAsync($"v1/ScSoft/check-kps", request);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException();
        }
        var session = await response.Content.ReadFromJsonAsync<ScSoftAtapiResponse>();
        return session ?? throw new InvalidOperationException();
    }

    public async Task<ScSoftAtapiResponse> CheckFrontIdentityAsync(CheckFrontIdentityRequest request)
    {
        var response = await PostAsJsonAsync($"v1/ScSoft/check-front", request);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException();
        }
        var session = await response.Content.ReadFromJsonAsync<ScSoftAtapiResponse>();
        return session ?? throw new InvalidOperationException();
    }
    public async Task<ScSoftAtapiResponse> CheckRearIdentityAsync(CheckRearIdentityRequest request)
    {
        var response = await PostAsJsonAsync($"v1/ScSoft/check-rear", request);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException();
        }
        var session = await response.Content.ReadFromJsonAsync<ScSoftAtapiResponse>();
        return session ?? throw new InvalidOperationException();
    }

    public async Task<ScSoftAtapiResponse> CheckHeadPoseAsync(CheckHeadPoseRequest request)
    {
        var response = await PostAsJsonAsync($"v1/ScSoft/check-headpose", request);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException();
        }
        var session = await response.Content.ReadFromJsonAsync<ScSoftAtapiResponse>();
        return session ?? throw new InvalidOperationException();
    }

    public async Task<ScSoftAtapiResponse> CheckNfcAsync(CheckNfcRequest request)
    {
        var response = await PostAsJsonAsync($"v1/ScSoft/check-nfc", request);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException();
        }
        var session = await response.Content.ReadFromJsonAsync<ScSoftAtapiResponse>();
        return session ?? throw new InvalidOperationException();
    }


    public async Task<ScSoftAtapiResponse> CheckSimilarityRateAsync(CheckSimilarityRateRequest request)
    {
        var response = await PostAsJsonAsync($"v1/ScSoft/check-similarity", request);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException();
        }
        var session = await response.Content.ReadFromJsonAsync<ScSoftAtapiResponse>();
        return session ?? throw new InvalidOperationException();
    }

    public async Task<ScSoftAtapiResponse> CheckSpoofAsync(CheckSpoofRequest request)
    {
        var response = await PostAsJsonAsync($"v1/ScSoft/check-spoof", request);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException();
        }
        var session = await response.Content.ReadFromJsonAsync<ScSoftAtapiResponse>();
        return session ?? throw new InvalidOperationException();
    }
}
