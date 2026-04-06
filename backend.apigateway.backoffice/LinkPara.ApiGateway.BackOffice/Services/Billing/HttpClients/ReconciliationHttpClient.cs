using LinkPara.ApiGateway.BackOffice.Services.Billing.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Billing.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.Billing.HttpClients;

public class ReconciliationHttpClient : HttpClientBase, IReconciliationHttpClient
{
    public ReconciliationHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor) 
        : base(client, httpContextAccessor)
    {
    }

    public async Task<ReconciliationJobResponseDto> DoReconciliationJobAsync(ReconciliationJobRequest request)
    {
        var response = await PostAsJsonAsync("v1/Reconciliations/reconciliation-job", request);

        return await response.Content.ReadFromJsonAsync<ReconciliationJobResponseDto>();
    }

    public async Task<PaginatedList<InstitutionDetailDto>> GetInstitutionDetailAsync(InstitutionDetailFilterRequest request)
    {
        var url = CreateUrlWithParams($"v1/Reconciliations/institution-detail", request, true);
        var response = await GetAsync(url);

        return await response.Content.ReadFromJsonAsync<PaginatedList<InstitutionDetailDto>>();
    }

    public async Task<PaginatedList<InstitutionSummaryDto>> GetInstitutionSummaryAsync(InstitutionSummaryFilterRequest request)
    {
        var url = CreateUrlWithParams($"v1/Reconciliations/institution-summary", request, true);
        var response = await GetAsync(url);

        return await response.Content.ReadFromJsonAsync<PaginatedList<InstitutionSummaryDto>>();
    }

    public async Task<PaginatedList<SummaryDto>> GetSummaryAsync(SummaryFilterRequest request)
    {
        var url = CreateUrlWithParams($"v1/Reconciliations/summary", request, true);
        var response = await GetAsync(url);

        return await response.Content.ReadFromJsonAsync<PaginatedList<SummaryDto>>();
    }

    public async Task<RetryReconciliationInstitutionResponseDto> RetryInstitutionReconciliationAsync(ReconciliationInstitutionRetryRequest request)
    {
        var response = await PostAsJsonAsync("v1/Reconciliations/retry-institution", request);

        return  await response.Content.ReadFromJsonAsync<RetryReconciliationInstitutionResponseDto>();
    }
}