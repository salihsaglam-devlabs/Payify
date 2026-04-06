using LinkPara.ApiGateway.Services.DigitalKyc.Models.Requests;
using LinkPara.ApiGateway.Services.DigitalKyc.Models.Responses;

namespace LinkPara.ApiGateway.Services.DigitalKyc.HttpClients;

public class SodecHttpClient : HttpClientBase, ISodecHttpClient
{
    public SodecHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor) : base(client, httpContextAccessor)
    {
    }

    public async Task<SodecCreateSessionResponse> SodecCreateSessionAsync(SodecCreateSessionRequest request)
    {
        var response = await PostAsJsonAsync($"v1/Sodec/create-session", request);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException();
        }
        var session = await response.Content.ReadFromJsonAsync<SodecCreateSessionResponse>();
        return session ?? throw new InvalidOperationException();
    }

    public async Task<SodecCompleteSessionResponse> SodecCompleteSessionAsync(SodecCompleteSessionRequest request)
    {
        var response = await PostAsJsonAsync($"v1/Sodec/complete-session", request);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException();
        }
        var result = await response.Content.ReadFromJsonAsync<SodecCompleteSessionResponse>();
        return result ?? throw new InvalidOperationException();
    }
}
