using LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Requests.MoneyTransferReconciliation;
using LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Responses.MoneyTransferReconciliation;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.HttpClients;

public class MoneyTransferReconciliationHttpClient : HttpClientBase, IMoneyTransferReconciliationHttpClient
{
    public MoneyTransferReconciliationHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor)
        : base(client, httpContextAccessor)
    {

    }

    public async Task<PaginatedList<ReconciliationDetailDto>> GetReconciliationDetailAsync(ReconciliationDetailRequest request)
    {
        var url = CreateUrlWithParams($"v1/Reconciliation/summary/details", request, true);
        var response = await GetAsync(url);
        var details = await response.Content.ReadFromJsonAsync<PaginatedList<ReconciliationDetailDto>>();
        return details ?? throw new InvalidOperationException();
    }

    public async Task<PaginatedList<ReconciliationSummaryDto>> GetReconciliationSummaryAsync(ReconciliationSummaryRequest request)
    {
        var url = CreateUrlWithParams($"v1/Reconciliation/summaries", request, true);
        var response = await GetAsync(url);
        var summaries = await response.Content.ReadFromJsonAsync<PaginatedList<ReconciliationSummaryDto>>();
        return summaries ?? throw new InvalidOperationException();
    }

    public async Task RunReconciliationAsync(RunReconciliationRequest request)
    {
        await PostAsJsonAsync($"v1/Reconciliation", request);
    }

    public async Task CancelReconciliationDetailAsync(CancelReconciliationDetailRequest request)
    {
        await PutAsJsonAsync($"v1/Reconciliation/summary/details/{request.Id}/cancel", request);
    }
}
