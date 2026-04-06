using System.Text.Json;
using LinkPara.ApiGateway.BackOffice.Commons.Extensions;
using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Responses;
using Microsoft.AspNetCore.JsonPatch;

namespace LinkPara.ApiGateway.BackOffice.Services.EMoney.HttpClients;

public class TierPermissionHttpClient : HttpClientBase, ITierPermissionHttpClient
{
    public TierPermissionHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor)
        : base(client, httpContextAccessor)
    {
    }
    
    public async Task<List<TierPermissionResponse>> GetTierPermissionsAsync(GetTierPermissionsRequest request)
    {
        var queryString = request.GetQueryString();
        var response = await GetAsync($"v1/TierPermissions" + queryString);
        var responseString = await response.Content.ReadAsStringAsync();
        var levelList = JsonSerializer.Deserialize<List<TierPermissionResponse>>(responseString,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        return levelList ?? throw new InvalidOperationException();
    }

    public async Task<TierPermissionResponse> GetTierPermissionByIdAsync(Guid id)
    {
        var response = await GetAsync($"v1/TierPermissions/{id}");
        var responseString = await response.Content.ReadAsStringAsync();
        var tierLevel = JsonSerializer.Deserialize<TierPermissionResponse>(responseString,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        return tierLevel ?? throw new InvalidOperationException();
    }

    public async Task CreateTierPermissionAsync(CreateTierPermissionRequest request)
    {
        await PostAsJsonAsync("v1/TierPermissions", request);
    }
    
    public async Task PutTierPermissionAsync(UpdateTierPermissionRequest request)
    {
        var response = await PutAsJsonAsync($"v1/TierPermissions", request);
        if (!response.IsSuccessStatusCode) throw new InvalidOperationException();
    }

    public async Task PatchTierPermissionAsync(Guid id, JsonPatchDocument<TierPermissionResponse> request)
    {
        await PatchAsync($"v1/TierPermissions/{id}", request);
    }

    public async Task DisableTierPermissionAsync(Guid id)
    {
        await DeleteAsync($"v1/TierPermissions/{id}");
    }
}