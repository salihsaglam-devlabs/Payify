using LinkPara.ApiGateway.Commons.Extensions;
using LinkPara.ApiGateway.Services.Emoney.Models.Requests;
using LinkPara.ApiGateway.Services.Emoney.Models.Responses;
using System.Text.Json;

namespace LinkPara.ApiGateway.Services.Emoney.HttpClients;

public class PricingProfileHttpClient : HttpClientBase, IPricingProfileHttpClient
{
    public PricingProfileHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor)
        : base(client, httpContextAccessor)
    {
    }

    public async Task<PricingProfileDto> GetCardTopupCommissionAsync()
    {
        var response = await GetAsync($"v1/PricingProfiles/card-commission");
        var responseString = await response.Content.ReadAsStringAsync();
        var topupCommissionOb = JsonSerializer.Deserialize<PricingProfileDto>(responseString, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        return topupCommissionOb ?? throw new InvalidOperationException();
    }
}

