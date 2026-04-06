using LinkPara.ApiGateway.CorporateWallet.Commons.Extensions;
using LinkPara.ApiGateway.CorporateWallet.Services.Emoney.Models.Requests;
using LinkPara.ApiGateway.CorporateWallet.Services.Emoney.Models.Responses;

namespace LinkPara.ApiGateway.CorporateWallet.Services.Emoney.HttpClients;

public class TierLevelHttpClient : HttpClientBase, ITierLevelHttpClient
{
    public TierLevelHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor)
        : base(client, httpContextAccessor)
    {
    }

    public async Task<List<TierLevelDto>> GetTierLevelsAsync()
    {
        var request = new GetTierLevelsRequest()
        {
            IncludeCustoms = false
        };
        var queryString = request.GetQueryString();
        var response = await GetAsync($"v1/TierLevels" + queryString);
        var tierLevels = await response.Content.ReadFromJsonAsync<List<TierLevelDto>>();
        return tierLevels ?? throw new InvalidOperationException();
    }
}