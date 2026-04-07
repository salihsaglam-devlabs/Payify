using LinkPara.SharedModels.Persistence;
using LinkPara.Card.Domain.Enums;

namespace LinkPara.Card.Domain.Entities.Reconciliation;

public class ReconciliationOperationExecution : AuditEntity
{
    public Guid ReconciliationOperationId { get; set; }
    public Guid ExecutionGroupId { get; set; }
    public int AttemptNo { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public ReconciliationOperationExecutionOutcome Outcome { get; set; }
    public string ErrorCode { get; set; }
    public string ErrorText { get; set; }
    public string RequestPayload { get; set; }
    public string ResponsePayload { get; set; }

    public ReconciliationOperation ReconciliationOperation { get; set; }
}
