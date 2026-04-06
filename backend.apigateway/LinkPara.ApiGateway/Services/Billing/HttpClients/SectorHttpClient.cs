using LinkPara.ApiGateway.Commons.Extensions;
using LinkPara.ApiGateway.Services.Billing.Models.Requests;
using LinkPara.ApiGateway.Services.Billing.Models.Responses;
using LinkPara.SharedModels.Pagination;
using System.Text.Json;

namespace LinkPara.ApiGateway.Services.Billing.HttpClients;

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