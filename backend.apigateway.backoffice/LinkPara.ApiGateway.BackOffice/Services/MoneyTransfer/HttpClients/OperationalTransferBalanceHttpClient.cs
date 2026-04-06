using LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.JsonPatch;

namespace LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.HttpClients;

public class OperationalTransferBalanceHttpClient : HttpClientBase, IOperationalTransferBalanceHttpClient
{
    public OperationalTransferBalanceHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor) : base(client, httpContextAccessor)
    {
    }

    public async Task<OperationalTransferBalanceDto> GetOperationalTransferBalanceByIdAsync(Guid id)
    {
        var response = await GetAsync($"v1/OperationalTransferBalances/{id}");
        return await response.Content.ReadFromJsonAsync<OperationalTransferBalanceDto>();
    }

    public async Task<PaginatedList<OperationalTransferBalanceDto>> GetOperationalTransferBalanceListAsync(GetOperationalTransferBalanceListRequest request)
    {
        var url = CreateUrlWithParams("v1/OperationalTransferBalances", request, true);
        var response = await GetAsync(url);

        return await response.Content.ReadFromJsonAsync<PaginatedList<OperationalTransferBalanceDto>>();
    }

    public async Task PatchOperationalTransferBalanceAsync(Guid id, JsonPatchDocument<PatchOperationalTransferBalanceRequest> request)
    {
        var response = await PatchAsync($"v1/OperationalTransferBalances/{id}", request);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException();
        }
    }

    public async Task SaveOperationalTransferBalanceAsync(SaveOperationalTransferBalanceRequest request)
    {
        await PostAsJsonAsync("v1/OperationalTransferBalances", request);
    }
}
