using LinkPara.HttpProviders.MoneyTransfer.Models;
using LinkPara.HttpProviders.Utility;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Json;

namespace LinkPara.HttpProviders.MoneyTransfer;
public class PfReturnTransactionService : HttpClientBase, IPfReturnTransactionService
{
    public PfReturnTransactionService(HttpClient client, IHttpContextAccessor httpContextAccessor) 
        : base(client, httpContextAccessor)
    {
    }
    public async Task<PaginatedList<PfReturnTransactionDto>> GetListAsync(GetPfReturnTransactionsRequest request)
    {
        var url = GetQueryString.CreateUrlWithSearchQueryParams($"v1/PfReturnTransactions", request, true);

        var response = await GetAsync(url);

        var transactions = await response.Content.ReadFromJsonAsync<PaginatedList<PfReturnTransactionDto>>();

        return transactions ?? throw new InvalidOperationException();
    }
    public async Task<bool> ReturnPfTransactionAsync(ReturnPfTransactionRequest request)
    {
        var response = await PostAsJsonAsync($"v1/PfReturnTransactions/return", request);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException();
        }

        return await response.Content.ReadFromJsonAsync<bool>();
    }
    public async Task<VerifyPfReturnTransactionResponse> VerifyPfReturnTransactionAsync(VerifyPfReturnTransactionRequest request)
    {
        var response = await PutAsJsonAsync($"v1/PfReturnTransactions/verify", request);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException();
        }

        var result  = await response.Content.ReadFromJsonAsync<VerifyPfReturnTransactionResponse>();

        return result ?? throw new InvalidOperationException();
    }
}
