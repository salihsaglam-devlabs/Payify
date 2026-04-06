using System.Text.Json;
using LinkPara.ApiGateway.BackOffice.Commons.Extensions;
using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models;
using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Responses;
using Microsoft.AspNetCore.JsonPatch;

namespace LinkPara.ApiGateway.BackOffice.Services.EMoney.HttpClients;

public class TierLevelHttpClient : HttpClientBase, ITierLevelHttpClient
{
    public TierLevelHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor)
        : base(client, httpContextAccessor)
    {
    }
    
    public async Task<List<TierLevelResponse>> GetTierLevelsAsync(GetTierLevelsRequest request)
    {
        var queryString = request.GetQueryString();
        var response = await GetAsync($"v1/TierLevels" + queryString);
        var responseString = await response.Content.ReadAsStringAsync();
        var levelList = JsonSerializer.Deserialize<List<TierLevelResponse>>(responseString,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        return levelList ?? throw new InvalidOperationException();
    }
    
    public async Task<TierLevelResponse> GetTierLevelByIdAsync(Guid id)
    {
        var response = await GetAsync($"v1/TierLevels/{id}");
        var responseString = await response.Content.ReadAsStringAsync();
        var tierLevel = JsonSerializer.Deserialize<TierLevelResponse>(responseString,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        return tierLevel ?? throw new InvalidOperationException();
    }

    public async Task CreateTierLevelsAsync(CustomTierLevelDto request)
    {
        await PostAsJsonAsync("v1/TierLevels", request);
    }

    public async Task DisableCustomTierLevelAsync(Guid id)
    {
        await DeleteAsync($"v1/TierLevels/{id}");
    }

    public async Task PatchCustomTierLevelAsync(Guid id, JsonPatchDocument<CustomTierLevelDto> request)
    {
        await PatchAsync($"v1/TierLevels/{id}", request);
    }
    
    public async Task CreateAccountCustomTierAsync(AccountCustomTierDto request)
    {
        await PostAsJsonAsync("v1/TierLevels/account-custom-tier", request);
    }

    public async Task DeleteAccountCustomTierAsync(Guid id)
    {
        await DeleteAsync($"v1/TierLevels/account-custom-tier/{id}");
    }
}