using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.Merchant.Services.Pf.HttpClients;

public class SubMerchantHttpClient : HttpClientBase, ISubMerchantHttpClient
{
    public SubMerchantHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor) 
        : base(client, httpContextAccessor)
    {
    }

    public async Task ApproveAsync(ApproveSubMerchantRequest request)
    {
        var response = await PutAsJsonAsync($"v1/SubMerchants/approve", request);
        if (!response.IsSuccessStatusCode) throw new InvalidOperationException();
    }

    public async Task<SubMerchantSummaryDto> GetSubMerchantSummary()
    {
        var response = await GetAsync($"v1/SubMerchants/summary");
        var subMerchantSummary = await response.Content.ReadFromJsonAsync<SubMerchantSummaryDto>();
        return subMerchantSummary ?? throw new InvalidOperationException();
    }

    public async Task DeleteAsync(Guid id)
    {
        var response = await DeleteAsync($"v1/SubMerchants/{id}");
        if (!response.IsSuccessStatusCode) throw new InvalidOperationException();
    }

    public async Task<PaginatedList<SubMerchantDto>> GetAllAsync(GetAllSubMerchantRequest request)
    {
        var url = CreateUrlWithParams($"v1/SubMerchants", request, true);
        var response = await GetAsync(url);
        var subMerchants = await response.Content.ReadFromJsonAsync<PaginatedList<SubMerchantDto>>();
        return subMerchants ?? throw new InvalidOperationException();
    }

    public async Task<SubMerchantDto> GetByIdAsync(Guid id)
    {
        var response = await GetAsync($"v1/SubMerchants/{id}");
        var subMerchant = await response.Content.ReadFromJsonAsync<SubMerchantDto>();
        return subMerchant ?? throw new InvalidOperationException();
    }

    public async Task<Guid> SaveAsync(SaveSubMerchantRequest request)
    {
        var response = await PostAsJsonAsync($"v1/SubMerchants", request);
        var subMerchantId = await response.Content.ReadFromJsonAsync<Guid>();
        return subMerchantId;
    }

    public async Task UpdateAsync(UpdateSubMerchantRequest request)
    {
        var response = await PutAsJsonAsync($"v1/SubMerchants", request);
        if (!response.IsSuccessStatusCode) throw new InvalidOperationException();
    }

    public async Task UpdateMultipleAsync(UpdateMultipleSubMerchantRequest request)
    {
        var response = await PutAsJsonAsync($"v1/SubMerchants/multiple", request);
        if (!response.IsSuccessStatusCode) throw new InvalidOperationException();
    }
}
