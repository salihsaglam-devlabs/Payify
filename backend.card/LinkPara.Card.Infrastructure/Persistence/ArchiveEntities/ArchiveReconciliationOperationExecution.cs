using LinkPara.SharedModels.Persistence;

namespace LinkPara.Card.Infrastructure.Persistence.ArchiveEntities;

public class ArchiveReconciliationOperationExecution : AuditEntity
{
    public Guid FileLineId { get; set; }
    public Guid GroupId { get; set; }
    public Guid EvaluationId { get; set; }
    public Guid OperationId { get; set; }
    public int AttemptNumber { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? FinishedAt { get; set; }
    public string Status { get; set; }
    public string RequestPayload { get; set; }
    public string ResponsePayload { get; set; }
    public string ResultCode { get; set; }
    public string ResultMessage { get; set; }
    public string ErrorCode { get; set; }
    public string ErrorMessage { get; set; }
    public Guid ArchiveRunId { get; set; }
    public DateTime ArchivedAt { get; set; }
    public string ArchivedBy { get; set; }
}
