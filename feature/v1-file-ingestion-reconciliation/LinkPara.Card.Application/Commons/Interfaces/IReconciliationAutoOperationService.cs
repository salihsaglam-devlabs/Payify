using LinkPara.Card.Application.Commons.Models.Reconciliation;

namespace LinkPara.Card.Application.Commons.Interfaces;

public interface IReconciliationAutoOperationService
{
    Task<ReconciliationExecutionSummary> ExecutePendingOperationsAsync(
        int take = 100,
        string actor = null,
        CancellationToken cancellationToken = default);
}
