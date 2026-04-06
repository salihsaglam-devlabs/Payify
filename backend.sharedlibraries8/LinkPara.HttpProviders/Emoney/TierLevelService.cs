using System.Net.Http.Json;
using LinkPara.HttpProviders.Emoney.Models;
using LinkPara.HttpProviders.Utility;
using Microsoft.AspNetCore.Http;

namespace LinkPara.HttpProviders.Emoney;

public class TierLevelService : HttpClientBase, ITierLevelService
{
    public TierLevelService(HttpClient client, IHttpContextAccessor httpContextAccessor) :
        base(client, httpContextAccessor)
    {

    }
    public async Task<List<TierLevelDto>> GetTierLevelsAsync(GetTierLevelsQuery query)
    {
        var url = GetQueryString.CreateUrlWithSearchQueryParams($"v1/TierLevels", query, true);

        var response = await GetAsync(url);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException();
        }

        var limits = await response.Content.ReadFromJsonAsync<List<TierLevelDto>>();

        return limits ?? throw new InvalidOperationException();
    }
    
    public async Task CheckOrUpgradeAccountTierAsync(CheckOrUpgradeAccountTierRequest request)
    {
        var response = await PostAsJsonAsync("v1/TierLevels/upgrade-account-tier", request);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException();
        }
    }
}