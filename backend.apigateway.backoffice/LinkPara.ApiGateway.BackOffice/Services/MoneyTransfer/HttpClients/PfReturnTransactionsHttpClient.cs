using LinkPara.HttpProviders.MoneyTransfer.Models;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.HttpClients;
public class PfReturnTransactionsHttpClient : HttpClientBase, IPfReturnTransactionsHttpClient
{
    public PfReturnTransactionsHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor) 
        : base(client, httpContextAccessor)
    {
    }

    public async Task<PaginatedList<PfReturnTransactionDto>> GetListAsync(GetPfReturnTransactionsRequest request)
    {
        var url = CreateUrlWithParams("v1/PfReturnTransactions", request, true);
        var response = await GetAsync(url);
        return await response.Content.ReadFromJsonAsync<PaginatedList<PfReturnTransactionDto>>();
    }

    public async Task<bool> ReturnPfTransactionAsync(ReturnPfTransactionRequest request)
    {
        var response = await PostAsJsonAsync($"v1/PfReturnTransactions/return", request);
        return await response.Content.ReadFromJsonAsync<bool>();
    }
}
