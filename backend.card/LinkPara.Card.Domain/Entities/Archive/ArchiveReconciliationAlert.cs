using LinkPara.Card.Domain.Entities.Reconciliation;

namespace LinkPara.Card.Domain.Entities.Archive;

public class ArchiveReconciliationAlert : ReconciliationAlert
{
    public DateTime ArchivedAt { get; set; }
}

