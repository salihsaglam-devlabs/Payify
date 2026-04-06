using LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Requests.BranchTransactions;
using LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.HttpClients;

public class BranchTransactionHttpClient : HttpClientBase, IBranchTransactionHttpClient
{
    public BranchTransactionHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor)
        : base(client, httpContextAccessor)
    {

    }

    public async Task<BranchTransactionDto> GetByIdAsync(Guid id)
    {
        var response = await GetAsync($"v1/BranchTransactions/{id}");

        return await response.Content.ReadFromJsonAsync<BranchTransactionDto>();
    }

    public async Task<PaginatedList<BranchTransactionDto>> GetListAsync(GetBranchTransactionsRequest request)
    {
        var url = CreateUrlWithParams("v1/BranchTransactions", request, true);
        var response = await GetAsync(url);

        return await response.Content.ReadFromJsonAsync<PaginatedList<BranchTransactionDto>>();
    }

    public async Task SaveAsync(SaveBranchTransactionRequest request)
    {
        await PostAsJsonAsync("v1/BranchTransactions", request);
    }

    public async Task UpdateAsync(UpdateBranchTransactionRequest request)
    {
        await PutAsJsonAsync("v1/BranchTransactions", request);
    }
}