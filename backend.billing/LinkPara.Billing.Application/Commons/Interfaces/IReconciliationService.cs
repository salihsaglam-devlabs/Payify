using LinkPara.Billing.Application.Features.Reconciliations;
using LinkPara.Billing.Application.Features.Reconciliations.Queries.GetInstitutionDetail;
using LinkPara.Billing.Application.Features.Reconciliations.Queries.GetInstitutionSummary;
using LinkPara.Billing.Application.Features.Reconciliations.Queries.GetSummary;
using LinkPara.Billing.Domain.Entities;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.Billing.Application.Commons.Interfaces;

public interface IReconciliationService
{
    Task<PaginatedList<InstitutionSummaryDto>> GetInstitutionSummaryAsync(GetInstitutionSummaryQuery request);
    Task<PaginatedList<InstitutionDetailDto>> GetInstitutionDetailAsync(GetInstitutionDetailQuery request);
    Task<InstitutionSummary> GetInstitutionSummaryByIdAsync(Guid institutionSummaryId);
    Task<PaginatedList<SummaryDto>> GetSummaryAsync(GetSummaryQuery request);
    Task CancelInstitutionPaymentByTransactionIdAsync(Guid transactionId);
}