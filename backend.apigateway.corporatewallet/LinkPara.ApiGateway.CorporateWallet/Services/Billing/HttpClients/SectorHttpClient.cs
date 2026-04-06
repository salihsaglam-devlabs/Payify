using LinkPara.ApiGateway.CorporateWallet.Commons.Extensions;
using LinkPara.ApiGateway.CorporateWallet.Services.Billing.Models.Requests;
using LinkPara.ApiGateway.CorporateWallet.Services.Billing.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.CorporateWallet.Services.Billing.HttpClients;

public class SectorHttpClient : HttpClientBase, ISectorHttpClient
{
    public SectorHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor)
        :base(client, httpContextAccessor)
    {

    }

    public async Task<PaginatedList<SectorDto>> GetAllSectorAsync(SectorFilterRequest request)
    {
        var queryString = request.GetQueryString();
        var response = await GetAsync($"v1/Sectors" + queryString);

        return await response.Content.ReadFromJsonAsync<PaginatedList<SectorDto>>();
    }
}