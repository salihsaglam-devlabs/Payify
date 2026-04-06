using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.HttpClients;

public class SubMerchantHttpClient : HttpClientBase, ISubMerchantHttpClient
{
    public SubMerchantHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor)
        : base(client, httpContextAccessor)
    {
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
}
