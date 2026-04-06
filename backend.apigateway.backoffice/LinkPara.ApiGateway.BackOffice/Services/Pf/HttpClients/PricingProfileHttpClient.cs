using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.JsonPatch;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.HttpClients;

public class PricingProfileHttpClient : HttpClientBase, IPricingProfileHttpClient
{
    public PricingProfileHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor)
: base(client, httpContextAccessor)
    {

    }

    public async Task DeleteProfileAsync(Guid id)
    {
        var response = await DeleteAsync($"v1/PricingProfiles/{id}");
        if (!response.IsSuccessStatusCode) throw new InvalidOperationException();
    }

    public async Task<PricingProfileDto> GetByIdAsync(Guid id)
    {
        var response = await GetAsync($"v1/PricingProfiles/{id}");
        var pricingProfile = await response.Content.ReadFromJsonAsync<PricingProfileDto>();
        return pricingProfile ?? throw new InvalidOperationException();
    }

    public async Task<PaginatedList<PricingProfileDto>> GetFilterListAsync(GetFilterPricingProfileRequest request)
    {
        var url = CreateUrlWithParams($"v1/PricingProfiles", request, true);
        var response = await GetAsync(url);
        var pricingProfiles = await response.Content.ReadFromJsonAsync<PaginatedList<PricingProfileDto>>();
        return pricingProfiles ?? throw new InvalidOperationException();
    }

    public async Task<UpdatePricingProfileRequest> PatchAsync(Guid id, JsonPatchDocument<UpdatePricingProfileRequest> acquireBank)
    {
        var response = await PatchAsync($"v1/PricingProfiles/{id}", acquireBank);
        var pricingProfile = await response.Content.ReadFromJsonAsync<UpdatePricingProfileRequest>();
        return pricingProfile ?? throw new InvalidOperationException();
    }

    public async Task SaveAsync(SavePricingProfileRequest request)
    {
        var response = await PostAsJsonAsync($"v1/PricingProfiles", request);
        if (!response.IsSuccessStatusCode) throw new InvalidOperationException();
    }

    public async Task UpdateAsync(UpdatePricingProfileRequest request)
    {
        var response = await PutAsJsonAsync($"v1/PricingProfiles", request);
        if (!response.IsSuccessStatusCode) throw new InvalidOperationException();
    }
    
    public async Task<PricingProfilePreviewResponse> PreviewPricingProfileUpdateAsync(UpdatePreviewPricingProfileRequest request)
    {
        var response = await PostAsJsonAsync($"v1/PricingProfiles/update-preview", request);
        var pricingProfilePreview = await response.Content.ReadFromJsonAsync<PricingProfilePreviewResponse>();
        return pricingProfilePreview ?? throw new InvalidOperationException();
    }
}
