using LinkPara.Card.Domain.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.Card.Domain.Entities.Reconciliation;

public class ReconciliationManualReviewItem : AuditEntity
{
    public Guid ReconciliationOperationId { get; set; }
    public Guid RunId { get; set; }
    public Guid ExecutionGroupId { get; set; }
    public ManualReviewStatus ReviewStatus { get; set; }
    public string ReviewNote { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string ReviewedBy { get; set; }

    public ReconciliationOperation ReconciliationOperation { get; set; }
}
