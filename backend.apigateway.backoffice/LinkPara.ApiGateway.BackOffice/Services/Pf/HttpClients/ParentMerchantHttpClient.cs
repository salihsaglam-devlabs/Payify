using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.HttpClients;

public class ParentMerchantHttpClient : HttpClientBase, IParentMerchantHttpClient
{
    public ParentMerchantHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor)
        : base(client, httpContextAccessor)
    {
    }

    public async Task BulkUpdateIntegrationModeAsync(BulkIntegrationModeUpdateRequest request)
    {
        var response = await PutAsJsonAsync($"v1/ParentMerchants/bulkUpdateIntegrationModes", request);
        if (!response.IsSuccessStatusCode) throw new InvalidOperationException();
    }

    public async Task BulkUpdatePermissionAsync(BulkPermissionUpdateRequest request)
    {
        var response = await PutAsJsonAsync($"v1/ParentMerchants/bulkUpdatePermissions", request);
        if (!response.IsSuccessStatusCode) throw new InvalidOperationException();
    }

    public async Task BulkUpdatePricingProfileAsync(BulkPricingProfileRequest request)
    {
        var response = await PutAsJsonAsync($"v1/ParentMerchants/bulkUpdatePricingProfiles", request);
        if (!response.IsSuccessStatusCode) throw new InvalidOperationException();
    }

    public async Task<MerchantDto> GetByIdAsync(Guid id)
    {
        var response = await GetAsync($"v1/ParentMerchants/{id}");
        var merchant = await response.Content.ReadFromJsonAsync<MerchantDto>();
        return merchant ?? throw new InvalidOperationException();
    }

    public async Task<PaginatedList<MerchantDto>> GetFilterListAsync(GetAllParentMerchantRequest request)
    {
        var url = CreateUrlWithParams($"v1/ParentMerchants", request, true);
        var response = await GetAsync(url);
        var merchants = await response.Content.ReadFromJsonAsync<PaginatedList<MerchantDto>>();
        return merchants ?? throw new InvalidOperationException();
    }

    public async Task<PaginatedList<ParentMerchantResponse>> GetPermissionsAsync(GetParentMerchantPermissionsRequest request)
    {
        var url = CreateUrlWithParams($"v1/ParentMerchants/permissions", request, true);
        var response = await GetAsync(url);
        var merchants = await response.Content.ReadFromJsonAsync<PaginatedList<ParentMerchantResponse>>();
        return merchants ?? throw new InvalidOperationException();
    }

    public async Task UpdateMultipleIntegrationModeAsync(List<UpdateMultipleIntegrationModeRequest> request)
    {
        var response = await PutAsJsonAsync($"v1/ParentMerchants/multipleIntegrationMode", request);
        if (!response.IsSuccessStatusCode) throw new InvalidOperationException();
    }

    public async Task UpdateMultiplePermissionAsync(List<UpdateMultiplePermissionRequest> request)
    {
        var response = await PutAsJsonAsync($"v1/ParentMerchants/multiplePermission", request);
        if (!response.IsSuccessStatusCode) throw new InvalidOperationException();
    }

    public async Task UpdateMultiplePricingProfileAsync(UpdateMultiplePricingProfileRequest request)
    {
        var response = await PutAsJsonAsync($"v1/ParentMerchants/multiplePricingProfile", request);
        if (!response.IsSuccessStatusCode) throw new InvalidOperationException();
    }
}
