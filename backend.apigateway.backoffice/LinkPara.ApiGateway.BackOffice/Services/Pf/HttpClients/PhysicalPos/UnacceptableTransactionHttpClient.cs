using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests.PhysicalPos;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses.PhysicalPos;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.JsonPatch;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.HttpClients.PhysicalPos;

public class UnacceptableTransactionHttpClient : HttpClientBase, IUnacceptableTransactionHttpClient
{
    public UnacceptableTransactionHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor) : base(client, httpContextAccessor)
    {
    }

    public async Task RetryUnacceptableAsync(RetryUnacceptableTransactionRequest request)
    {
        var response = await PostAsJsonAsync($"v1/UnacceptableTransactions/retry", request);
        if (!response.IsSuccessStatusCode) throw new InvalidOperationException();
    }

    public async Task<PaginatedList<PhysicalPosUnacceptableTransactionDto>> GetAllUnacceptableTransactionsAsync(GetAllUnacceptableTransactionRequest request)
    {
        var url = CreateUrlWithParams($"v1/UnacceptableTransactions", request, true);
        var response = await GetAsync(url);
        var unacceptableTransactionList = await response.Content.ReadFromJsonAsync<PaginatedList<PhysicalPosUnacceptableTransactionDto>>();
        return unacceptableTransactionList ?? throw new InvalidOperationException();
    }

    public async Task<UnacceptableTransactionDetailResponse> GetDetailsByIdAsync(Guid id)
    {
        var response = await GetAsync($"v1/UnacceptableTransactions/{id}");
        var unacceptableTransaction = await response.Content.ReadFromJsonAsync<UnacceptableTransactionDetailResponse>();
        return unacceptableTransaction ?? throw new InvalidOperationException();
    }

    public async Task<PhysicalPosUnacceptableTransactionDto> UpdateStatusAsync(Guid id, JsonPatchDocument<UpdateUnacceptableTransactionRequest> unacceptableTransaction)
    {
        var response = await PatchAsync($"v1/UnacceptableTransactions/{id}", unacceptableTransaction);
        var unacceptableTransactionDto = await response.Content.ReadFromJsonAsync<PhysicalPosUnacceptableTransactionDto>();
        return unacceptableTransactionDto ?? throw new InvalidOperationException();
    }
}