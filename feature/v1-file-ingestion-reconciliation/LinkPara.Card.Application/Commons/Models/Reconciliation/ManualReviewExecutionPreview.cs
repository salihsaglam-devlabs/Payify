namespace LinkPara.Card.Application.Commons.Models.Reconciliation;

public class ManualReviewExecutionPreview
{
    public Guid ManualReviewItemId { get; set; }
    public Guid ReconciliationOperationId { get; set; }
    public Guid CardTransactionRecordId { get; set; }
    public Guid RunId { get; set; }
    public string[] PlannedOperations { get; set; } = [];
    public string[] ApproveOperationPath { get; set; } = [];
    public string[] RejectOperationPath { get; set; } = [];
    public string OperationPreview { get; set; }
    public IReadOnlyCollection<ManualReviewExecutionLogItem> LastExecutionLogs { get; set; } = [];
}

public class ManualReviewExecutionLogItem
{
    public int OperationOrder { get; set; }
    public string OperationName { get; set; }
    public string Status { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public string ErrorMessage { get; set; }
}
