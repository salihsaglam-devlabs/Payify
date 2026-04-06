using LinkPara.ApiGateway.Boa.Services.Pf.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.Boa.Services.Pf.HttpClients;

public class MerchantIntegratorHttpClient : HttpClientBase, IMerchantIntegratorHttpClient
{
    public MerchantIntegratorHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor) : base(client, httpContextAccessor)
    {
    }
    
    public async Task<PaginatedList<MerchantIntegratorDto>> GetAllAsync(SearchQueryParams request)
    {
        var url = CreateUrlWithParams($"v1/BoaMerchantIntegrators", request, true);
        var response = await GetAsync(url);
        var merchantIntegrators = await response.Content.ReadFromJsonAsync<PaginatedList<MerchantIntegratorDto>>();
        return merchantIntegrators ?? throw new InvalidOperationException();
    }
}