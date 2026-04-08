using LinkPara.SharedModels.Persistence;

namespace LinkPara.Card.Infrastructure.Persistence.ArchiveEntities;

public class ArchiveReconciliationReview : AuditEntity
{
    public Guid FileLineId { get; set; }
    public Guid GroupId { get; set; }
    public Guid EvaluationId { get; set; }
    public Guid OperationId { get; set; }
    public Guid? ReviewerId { get; set; }
    public string Decision { get; set; }
    public string Comment { get; set; }
    public DateTime? DecisionAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public string ExpirationAction { get; set; }
    public string ExpirationFlowAction { get; set; }
    public Guid ArchiveRunId { get; set; }
    public DateTime ArchivedAt { get; set; }
    public string ArchivedBy { get; set; }
}
