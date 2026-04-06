
using LinkPara.Epin.Application.Features.Reconciliations;
using LinkPara.Epin.Application.Features.Reconciliations.Queries.GetFilterReconciliationSummaries;
using LinkPara.Epin.Application.Features.Reconciliations.Queries.GetReconciliationSummaryById;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.Epin.Application.Commons.Interfaces;

public interface IReconciliationService
{
    Task<PaginatedList<ReconciliationSummaryDto>> GetFilterReconciliationSummariesAsync(GetFilterReconciliationSummariesQuery request);
    Task<ReconciliationSummaryDto> GetReconciliationSummaryByIdAsync(GetReconciliationSummaryByIdQuery request);
    Task ReconciliationAsync(DateTime reconciliationDate);
}
