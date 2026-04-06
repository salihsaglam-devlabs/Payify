using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.Merchant.Services.Pf.HttpClients;

public class ParentMerchantHttpClient : HttpClientBase, IParentMerchantHttpClient
{
    public ParentMerchantHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor)
        : base(client, httpContextAccessor)
    {
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
}
