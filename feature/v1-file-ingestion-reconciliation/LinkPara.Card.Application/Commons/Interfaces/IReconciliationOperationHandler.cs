using LinkPara.Card.Application.Commons.Models.Reconciliation;

namespace LinkPara.Card.Application.Commons.Interfaces;

public interface IReconciliationOperationHandler
{
    string OperationName { get; }

    Task<bool> ExecuteAsync(
        ReconciliationOperationPlan plan,
        ReconciliationOperationScope scope,
        string actor,
        CancellationToken cancellationToken = default);
}
