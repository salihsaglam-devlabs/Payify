using LinkPara.Card.Application.Commons.Models.Reconciliation;

namespace LinkPara.Card.Application.Commons.Interfaces;

public interface IReconciliationOperationExecutor
{
    Task<ReconciliationOperationPlan> PrepareAsync(
        ReconciliationOperationScope scope,
        string actor,
        CancellationToken cancellationToken = default);

    Task<bool> ExecuteAsync(
        ReconciliationOperationPlan plan,
        ReconciliationOperationScope scope,
        string actor,
        CancellationToken cancellationToken = default);
}
