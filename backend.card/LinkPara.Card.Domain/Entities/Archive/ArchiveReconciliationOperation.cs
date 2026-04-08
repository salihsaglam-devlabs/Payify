using LinkPara.Card.Domain.Entities.Reconciliation;

namespace LinkPara.Card.Domain.Entities.Archive;

public class ArchiveReconciliationOperation : ReconciliationOperation
{
    public DateTime ArchivedAt { get; set; }
}

