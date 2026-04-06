using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.HttpClients;

public class MerchantIntegratorHttpClient : HttpClientBase, IMerchantIntegratorHttpClient
{
    public MerchantIntegratorHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor) : base(client, httpContextAccessor)
    {
    }

    public async Task DeleteMerchantIntegratorAsync(Guid id)
    {
        var response = await DeleteAsync($"v1/MerchantIntegrators/{id}");
        if (!response.IsSuccessStatusCode) throw new InvalidOperationException();
    }

    public async Task<PaginatedList<MerchantIntegratorDto>> GetAllAsync(SearchQueryParams request)
    {
        var url = CreateUrlWithParams($"v1/MerchantIntegrators", request, true);
        var response = await GetAsync(url);
        var merchantIntegrators = await response.Content.ReadFromJsonAsync<PaginatedList<MerchantIntegratorDto>>();
        return merchantIntegrators ?? throw new InvalidOperationException();
    }

    public async Task<MerchantIntegratorDto> GetByIdAsync(Guid id)
    {
        var response = await GetAsync($"v1/MerchantIntegrators/{id}");
        var merchantIntegrator = await response.Content.ReadFromJsonAsync<MerchantIntegratorDto>();
        return merchantIntegrator ?? throw new InvalidOperationException();
    }

    public async Task SaveAsync(SaveMerchantIntegratorRequest request)
    {
        var response = await PostAsJsonAsync($"v1/MerchantIntegrators", request);
        if (!response.IsSuccessStatusCode) throw new InvalidOperationException();
    }

    public async Task UpdateAsync(UpdateMerchantIntegratorRequest request)
    {
        var response = await PutAsJsonAsync($"v1/MerchantIntegrators", request);
        if (!response.IsSuccessStatusCode) throw new InvalidOperationException();
    }
}
