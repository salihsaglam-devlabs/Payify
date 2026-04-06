using LinkPara.ApiGateway.BackOffice.Commons.Extensions;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.HttpClients;

public class MerchantStatementHttpClient : HttpClientBase, IMerchantStatementHttpClient
{
    public MerchantStatementHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor)
        : base(client, httpContextAccessor)
    {
    }

    public async Task<PaginatedList<MerchantStatementDto>> GetMerchantStatementsAsync(GetFilterMerchantStatementRequest request)
    {
        var url = CreateUrlWithParams($"v1/MerchantStatement", request, true);
        var response = await GetAsync(url);
        var merchantStatements = await response.Content.ReadFromJsonAsync<PaginatedList<MerchantStatementDto>>();

        return merchantStatements ?? throw new InvalidOperationException();
    }

    public async Task<HttpResponseMessage> GetMerchantStatementWithBytesAsync(DownloadMerchantStatementRequest request)
    {
        var queryString = request.GetQueryString();
        var response = await GetAsync($"v1/MerchantStatement/download" + queryString);
        
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException();
        }

        return response;
    }
}
