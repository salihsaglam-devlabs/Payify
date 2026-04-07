using LinkPara.Card.Application.Commons.Models.Reconciliation;

namespace LinkPara.Card.Application.Commons.Interfaces;

public interface IReconciliationManualOperationService
{
    Task<IReadOnlyCollection<ReconciliationRunListItem>> GetPendingManualReviewsAsync(int take = 100, CancellationToken cancellationToken = default);
    Task<ManualReviewExecutionPreview> GetManualReviewDecisionPreviewAsync(Guid manualReviewItemId, CancellationToken cancellationToken = default);
    Task<ManualReviewOperationResult> ApproveManualReviewAsync(Guid manualReviewItemId, string note, string actor, CancellationToken cancellationToken = default);
    Task<ManualReviewOperationResult> RejectManualReviewAsync(Guid manualReviewItemId, string note, string actor, CancellationToken cancellationToken = default);
}
