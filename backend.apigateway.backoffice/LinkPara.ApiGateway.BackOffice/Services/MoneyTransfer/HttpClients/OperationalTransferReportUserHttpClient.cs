using LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Responses;

namespace LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.HttpClients;
public class OperationalTransferReportUserHttpClient : HttpClientBase, IOperationalTransferReportUserHttpClient
{
    public OperationalTransferReportUserHttpClient(
        HttpClient client,
        IHttpContextAccessor httpContextAccessor) : base(client, httpContextAccessor) { }

    public async Task DeleteAsync(Guid id)
    {
        await DeleteAsync($"v1/OperationalTransferReportUsers/{id}");
    }

    public async Task<List<OperationalTransferReportUserDto>> GetListAsync()
    {
        var response = await GetAsync("v1/OperationalTransferReportUsers");
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException();
        }
        return await response.Content.ReadFromJsonAsync<List<OperationalTransferReportUserDto>>();
    }

    public async Task SyncAsync(SyncOperationalTransferReportUserRequest request)
    {
        await PostAsJsonAsync("v1/OperationalTransferReportUsers", request);
    }
}
