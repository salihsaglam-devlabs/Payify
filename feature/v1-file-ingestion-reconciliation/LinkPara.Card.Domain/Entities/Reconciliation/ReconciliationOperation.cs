using LinkPara.Card.Domain.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.Card.Domain.Entities.Reconciliation;

public class ReconciliationOperation : AuditEntity
{
    public Guid CardTransactionRecordId { get; set; }
    public Guid? ClearingRecordId { get; set; }
    public Guid RunId { get; set; }
    public Guid ExecutionGroupId { get; set; }
    public int OperationIndex { get; set; }
    public int? DependsOnIndex { get; set; }
    public bool IsApprovalGate { get; set; }
    public string OperationCode { get; set; }
    public ReconciliationOperationMode Mode { get; set; }
    public ReconciliationOperationStatus Status { get; set; }
    public string Fingerprint { get; set; }
    public string IdempotencyKey { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string ErrorCode { get; set; }
    public string ErrorText { get; set; }
    public string Payload { get; set; }
}
