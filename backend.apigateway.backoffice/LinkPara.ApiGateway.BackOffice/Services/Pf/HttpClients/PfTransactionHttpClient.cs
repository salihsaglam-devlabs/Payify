using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.HttpClients;

public class PfTransactionHttpClient : HttpClientBase, IPfTransactionHttpClient
{
    public PfTransactionHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor)
        : base(client, httpContextAccessor)
    {
    }

    public async Task<PaginatedList<PfTransactionDto>> GetAllAsync(GetAllTransactionRequest request)
    {
        var url = CreateUrlWithParams($"v1/Transactions", request, true);
        var response = await GetAsync(url);
        var transactions = await response.Content.ReadFromJsonAsync<PaginatedList<PfTransactionDto>>();
        return transactions ?? throw new InvalidOperationException();
    }
}
