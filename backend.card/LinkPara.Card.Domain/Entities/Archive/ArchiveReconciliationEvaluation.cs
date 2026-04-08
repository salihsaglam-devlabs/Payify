using LinkPara.Card.Domain.Entities.Reconciliation;

namespace LinkPara.Card.Domain.Entities.Archive;

public class ArchiveReconciliationEvaluation : ReconciliationEvaluation
{
    public DateTime ArchivedAt { get; set; }
}

