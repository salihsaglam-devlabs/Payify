using LinkPara.SharedModels.Persistence;

namespace LinkPara.Card.Domain.Entities.Archive;

public class ArchiveReconciliationOperation : AuditEntity
{
    public Guid FileLineId { get; set; }
    public Guid EvaluationId { get; set; }
    public Guid GroupId { get; set; }
    public int SequenceIndex { get; set; }
    public int? ParentSequenceIndex { get; set; }
    public string Code { get; set; }
    public string Note { get; set; }
    public string Payload { get; set; }
    public bool IsManual { get; set; }
    public string Branch { get; set; }
    public string Status { get; set; }
    public string LeaseOwner { get; set; }
    public DateTime? LeaseExpiresAt { get; set; }
    public int RetryCount { get; set; }
    public int MaxRetries { get; set; }
    public DateTime? NextAttemptAt { get; set; }
    public string IdempotencyKey { get; set; }
    public string LastError { get; set; }
    public Guid ArchiveRunId { get; set; }
    public DateTime ArchivedAt { get; set; }
    public string ArchivedBy { get; set; }
}

