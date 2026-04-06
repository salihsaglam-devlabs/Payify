using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.Merchant.Services.Pf.HttpClients;

public class SubMerchantLimitsHttpClient : HttpClientBase, ISubMerchantLimitsHttpClient
{
    public SubMerchantLimitsHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor)
        : base(client, httpContextAccessor)
    {
    }
    
    public async Task<PaginatedList<SubMerchantLimitDto>> GetAllAsync(GetAllSubMerchantLimitsRequest request)
    {
        var url = CreateUrlWithParams($"v1/SubMerchantLimits", request, true);
        var response = await GetAsync(url);
        var subMerchantLimits = await response.Content.ReadFromJsonAsync<PaginatedList<SubMerchantLimitDto>>();
        return subMerchantLimits ?? throw new InvalidOperationException();
    }

    public async Task<SubMerchantLimitDto> GetByIdAsync(Guid id)
    {
        var response = await GetAsync($"v1/SubMerchantLimits/{id}");
        var subMerchantLimit = await response.Content.ReadFromJsonAsync<SubMerchantLimitDto>();
        return subMerchantLimit ?? throw new InvalidOperationException();
    }

    public async Task DeleteAsync(Guid id)
    {
        var response = await DeleteAsync($"v1/SubMerchantLimits/{id}");
        if (!response.IsSuccessStatusCode) throw new InvalidOperationException();
    }

    public async Task SaveSubMerchantLimit(SaveSubMerchantLimitRequest request)
    {
        var response = await PostAsJsonAsync($"v1/SubMerchantLimits", request);
        if (!response.IsSuccessStatusCode) throw new InvalidOperationException();
    }

    public async Task UpdateSubMerchantLimit(SubMerchantLimitDto request)
    {
        var response = await PutAsJsonAsync($"v1/SubMerchantLimits", request);
        if (!response.IsSuccessStatusCode) throw new InvalidOperationException();
    }
}