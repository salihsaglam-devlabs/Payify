using LinkPara.Card.Domain.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.Card.Domain.Entities.Reconciliation;

public class ReconciliationEvaluation : AuditEntity
{
    public Guid CardTransactionRecordId { get; set; }
    public Guid? RunId { get; set; }
    public Guid ExecutionGroupId { get; set; }
    public ReconciliationDecisionType DecisionType { get; set; }
    public string DecisionCode { get; set; }
    public string DecisionReason { get; set; }
    public bool HasClearing { get; set; }
    public Guid? ClearingRecordId { get; set; }
    public int PlannedOperationCount { get; set; }
    public string PlannedOperationCodes { get; set; }
}
