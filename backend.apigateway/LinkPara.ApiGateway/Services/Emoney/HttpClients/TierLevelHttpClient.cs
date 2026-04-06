using LinkPara.ApiGateway.Commons.Extensions;
using LinkPara.ApiGateway.Services.Emoney.Models.Requests;
using LinkPara.ApiGateway.Services.Emoney.Models.Responses;

namespace LinkPara.ApiGateway.Services.Emoney.HttpClients;

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