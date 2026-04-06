using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.HttpClients;

public class PfApiLogHttpClient : HttpClientBase, IPfApiLogHttpClient
{
    public PfApiLogHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor) 
        : base(client, httpContextAccessor)
    {
    }

    public async Task<PaginatedList<ApiLogDto>> GetAllAsync(GetAllApiLogRequest request)
    {
        var url = CreateUrlWithParams($"v1/Payments/logs", request, true);
        var response = await GetAsync(url);
        var apiLogs = await response.Content.ReadFromJsonAsync<PaginatedList<ApiLogDto>>();
        return apiLogs ?? throw new InvalidOperationException();
    }
}
