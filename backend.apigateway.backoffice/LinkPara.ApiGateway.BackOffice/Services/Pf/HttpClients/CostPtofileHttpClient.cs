using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.JsonPatch;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.HttpClients;

public class CostPtofileHttpClient : HttpClientBase, ICostProfileHttpClient
{
    public CostPtofileHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor) : base(client, httpContextAccessor)
    {
    }

    public async Task<CostProfilesDto> GetByIdAsync(Guid id)
    {
        var response = await GetAsync($"v1/CostProfiles/{id}");
        var costProfile = await response.Content.ReadFromJsonAsync<CostProfilesDto>();
        return costProfile ?? throw new InvalidOperationException();
    }

    public async Task<PaginatedList<CostProfilesDto>> GetFilterListAsync(GetFilterCostProfileRequest request)
    {
        var url = CreateUrlWithParams($"v1/CostProfiles", request, true);
        var response = await GetAsync(url);
        var costProfiles = await response.Content.ReadFromJsonAsync<PaginatedList<CostProfilesDto>>();
        return costProfiles ?? throw new InvalidOperationException();
    }

    public async Task<UpdateCostProfileRequest> PatchAsync(Guid id, JsonPatchDocument<UpdateCostProfileRequest> costProfilePatch)
    {
        var response = await PatchAsync($"v1/CostProfiles/{id}", costProfilePatch);
        var costProfile = await response.Content.ReadFromJsonAsync<UpdateCostProfileRequest>();
        return costProfile ?? throw new InvalidOperationException();
    }

    public async Task SaveAsync(SaveCostProfileRequest request)
    {
        var response = await PostAsJsonAsync($"v1/CostProfiles", request);
        if (!response.IsSuccessStatusCode) throw new InvalidOperationException();
    }
    
    public async Task<CostProfilePreviewResponse> PreviewCostProfileUpdateAsync(UpdatePreviewCostProfileRequest request)
    {
        var response = await PostAsJsonAsync($"v1/CostProfiles/update-preview", request);
        var pricingProfilePreview = await response.Content.ReadFromJsonAsync<CostProfilePreviewResponse>();
        return pricingProfilePreview ?? throw new InvalidOperationException();
    }
}
