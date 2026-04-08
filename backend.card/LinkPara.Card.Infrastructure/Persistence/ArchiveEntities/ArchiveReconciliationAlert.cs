using LinkPara.SharedModels.Persistence;

namespace LinkPara.Card.Infrastructure.Persistence.ArchiveEntities;

public class ArchiveReconciliationAlert : AuditEntity
{
    public Guid FileLineId { get; set; }
    public Guid GroupId { get; set; }
    public Guid EvaluationId { get; set; }
    public Guid OperationId { get; set; }
    public string Severity { get; set; }
    public string AlertType { get; set; }
    public string Message { get; set; }
    public string AlertStatus { get; set; }
    public Guid ArchiveRunId { get; set; }
    public DateTime ArchivedAt { get; set; }
    public string ArchivedBy { get; set; }
}
