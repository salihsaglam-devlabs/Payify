using LinkPara.ApiGateway.BackOffice.Services.Billing.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Billing.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.Billing.HttpClients;

public interface IReconciliationHttpClient
{
    Task<PaginatedList<InstitutionSummaryDto>> GetInstitutionSummaryAsync(InstitutionSummaryFilterRequest request);
    Task<PaginatedList<InstitutionDetailDto>> GetInstitutionDetailAsync(InstitutionDetailFilterRequest request);
    Task<RetryReconciliationInstitutionResponseDto> RetryInstitutionReconciliationAsync(ReconciliationInstitutionRetryRequest request);
    Task<PaginatedList<SummaryDto>> GetSummaryAsync(SummaryFilterRequest request);
    Task<ReconciliationJobResponseDto> DoReconciliationJobAsync(ReconciliationJobRequest request);
}