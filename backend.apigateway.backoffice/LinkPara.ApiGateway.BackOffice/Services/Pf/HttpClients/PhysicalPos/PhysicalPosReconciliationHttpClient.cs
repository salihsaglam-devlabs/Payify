using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests.PhysicalPos;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses.PhysicalPos;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.HttpClients.PhysicalPos;

public class PhysicalPosReconciliationHttpClient : HttpClientBase, IPhysicalPosReconciliationHttpClient
{
    public PhysicalPosReconciliationHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor) : base(client, httpContextAccessor)
    {
    }
    public async Task<PaginatedList<PhysicalPosEndOfDayDto>> GetAllPhysicalPosEndOfDayAsync(GetAllPhysicalPosEndOfDayRequest request)
    {
        var url = CreateUrlWithParams($"v1/Reconciliation", request, true);
        var response = await GetAsync(url);
        var endOfDayList = await response.Content.ReadFromJsonAsync<PaginatedList<PhysicalPosEndOfDayDto>>();
        return endOfDayList ?? throw new InvalidOperationException();
    }

    public async Task<PhysicalPosEndOfDayDetailResponse> GetDetailsByIdAsync(Guid id)
    {
        var response = await GetAsync($"v1/Reconciliation/{id}");
        var endOfDay = await response.Content.ReadFromJsonAsync<PhysicalPosEndOfDayDetailResponse>();
        return endOfDay ?? throw new InvalidOperationException();
    }

    public async Task<HttpResponseMessage> DownloadReconciliationReportWithBytesAsync(Guid id)
    {
        var response = await GetAsync($"v1/Reconciliation/{id}/download");
        
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException();
        }

        return response;
    }

    public async Task BatchManualReconciliationAsync(BatchManualReconciliationRequest request)
    {
        var response = await PostAsJsonAsync($"v1/Reconciliation/batch-manual-reconciliation", request);
        if (!response.IsSuccessStatusCode) throw new InvalidOperationException();
    }

    public async Task SingleManualReconciliationAsync(SingleManualReconciliationRequest request)
    {
        var response = await PostAsJsonAsync($"v1/Reconciliation/single-manual-reconciliation", request);
        if (!response.IsSuccessStatusCode) throw new InvalidOperationException();
    }
}