using LinkPara.Card.Application.Commons.Constants;
using LinkPara.Card.Domain.Entities.Reconciliation;
using LinkPara.Card.Domain.Enums;

namespace LinkPara.Card.Infrastructure.Services.Reconciliation.Orchestration;

internal static class CardReconciliationStateResolver
{
    public static (CardReconciliationState State, string Reason) Resolve(
        IReadOnlyCollection<ReconciliationOperation> operations,
        bool hasPendingManualReview)
    {
        if (hasPendingManualReview)
        {
            return (CardReconciliationState.ManualReviewRequired, ReconciliationStateReasons.ManualReviewPending);
        }

        if (operations.Count == 0)
        {
            return (CardReconciliationState.ReadyForReconcile, ReconciliationStateReasons.NoOperationsFound);
        }

        if (operations.Any(x => x.Status is ReconciliationOperationStatus.Pending or ReconciliationOperationStatus.Blocked or ReconciliationOperationStatus.Processing))
        {
            return (CardReconciliationState.ReadyForReconcile, ReconciliationStateReasons.RunNotFinished);
        }

        if (operations.Any(x => x.Status == ReconciliationOperationStatus.Failed))
        {
            return (CardReconciliationState.ReconcileFailed, ReconciliationStateReasons.RunFailed);
        }

        if (operations.Any(x => x.Status == ReconciliationOperationStatus.Rejected))
        {
            return (CardReconciliationState.ReconcileFailed, ReconciliationStateReasons.RunRejected);
        }

        return (CardReconciliationState.ReconcileCompleted, ReconciliationStateReasons.RunDone);
    }
}
