using LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Requests.RepresentativeTransactions;
using LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.HttpClients;

public class RepresentativeTransactionHttpClient : HttpClientBase, IRepresentativeTransactionHttpClient
{
    public RepresentativeTransactionHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor)
        : base(client, httpContextAccessor)
    {

    }

    public async Task<RepresentativeTransactionDto> GetByIdAsync(Guid id)
    {
        var response = await GetAsync($"v1/RepresentativeTransactions/{id}");

        return await response.Content.ReadFromJsonAsync<RepresentativeTransactionDto>();
    }

    public async Task<PaginatedList<RepresentativeTransactionDto>> GetListAsync(GetRepresentativeTransactionsRequest request)
    {
        var url = CreateUrlWithParams("v1/RepresentativeTransactions", request, true);
        var response = await GetAsync(url);

        return await response.Content.ReadFromJsonAsync<PaginatedList<RepresentativeTransactionDto>>();
    }

    public async Task SaveAsync(SaveRepresentativeTransactionRequest request)
    {
        await PostAsJsonAsync("v1/RepresentativeTransactions", request);
    }

    public async Task UpdateAsync(UpdateRepresentativeTransactionRequest request)
    {
        await PutAsJsonAsync("v1/RepresentativeTransactions", request);
    }
}