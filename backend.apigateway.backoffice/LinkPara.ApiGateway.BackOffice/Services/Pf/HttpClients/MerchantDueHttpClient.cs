using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.HttpClients;

public class MerchantDueHttpClient : HttpClientBase, IMerchantDueHttpClient
{
    public MerchantDueHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor)
        : base(client, httpContextAccessor)
    {
    }
    
    public async Task<MerchantDueDto> GetByIdAsync(Guid id)
    {
        var response = await GetAsync($"v1/MerchantDue/{id}");
        var merchantDue = await response.Content.ReadFromJsonAsync<MerchantDueDto>();
        return merchantDue ?? throw new InvalidOperationException();
    }

    public async Task<PaginatedList<MerchantDueDto>> GetAllMerchantDuesAsync(GetAllMerchantDueRequest request)
    {
        var url = CreateUrlWithParams($"v1/MerchantDue", request, true);
        var response = await GetAsync(url);
        var merchantDues = await response.Content.ReadFromJsonAsync<PaginatedList<MerchantDueDto>>();
        return merchantDues ?? throw new InvalidOperationException();
    }
    
    public async Task SaveMerchantDueAsync(SaveMerchantDueRequest request)
    {
        var response = await PostAsJsonAsync($"v1/MerchantDue", request);
        if (!response.IsSuccessStatusCode) throw new InvalidOperationException();
    }
    public async Task DeleteMerchantDueAsync(Guid id)
    {
        var response = await DeleteAsync($"v1/MerchantDue/{id}");
        if (!response.IsSuccessStatusCode) throw new InvalidOperationException();
    }
}