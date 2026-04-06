using LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.HttpClients;

public class OperationalTransferHttpClient : HttpClientBase, IOperationalTransferHttpClient
{
    public OperationalTransferHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor) : base(client, httpContextAccessor)
    {
    }

    public async Task<OperationalTransferDto> GetOperationalTransferByIdAsync(Guid id)
    {
        var response = await GetAsync($"v1/OperationalTransfers/{id}");
        return await response.Content.ReadFromJsonAsync<OperationalTransferDto>();
    }

    public async Task<PaginatedList<OperationalTransferDto>> GetOperationalTransferListAsync(GetOperationalTransferListRequest request)
    {
        var url = CreateUrlWithParams("v1/OperationalTransfers", request, true);
        var response = await GetAsync(url);

        return await response.Content.ReadFromJsonAsync<PaginatedList<OperationalTransferDto>>();
    }
}
