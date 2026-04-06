using LinkPara.HttpProviders.Emoney.Models;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Json;

namespace LinkPara.HttpProviders.Emoney;

public class FraudService : HttpClientBase, IFraudService
{

    public FraudService(HttpClient client, IHttpContextAccessor httpContextAccessor)
        : base(client, httpContextAccessor)
    {

    }

    public async Task<FraudCheckResponse> Resume(ResumeRequest request)
    {
        var response = await PostAsJsonAsync("v1/Fraud", request);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException();
        }

        var fraudCheckResponse = await response.Content.ReadFromJsonAsync<FraudCheckResponse>();

        return fraudCheckResponse ?? throw new InvalidOperationException();
    }
}
