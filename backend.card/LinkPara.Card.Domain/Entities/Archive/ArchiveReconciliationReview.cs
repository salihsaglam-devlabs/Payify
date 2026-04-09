using LinkPara.Card.Domain.Entities.Reconciliation;

namespace LinkPara.Card.Domain.Entities.Archive;

public class ArchiveReconciliationReview : ReconciliationReview
{
    public DateTime ArchivedAt { get; set; }
    public string ArchivedBy { get; set; } = string.Empty;
    public Guid ArchiveRunId { get; set; }
}

