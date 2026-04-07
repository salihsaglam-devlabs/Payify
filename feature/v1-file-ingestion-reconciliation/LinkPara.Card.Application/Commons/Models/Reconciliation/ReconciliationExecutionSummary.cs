namespace LinkPara.Card.Application.Commons.Models.Reconciliation;

public class ReconciliationExecutionSummary
{
    public int RequestedCount { get; set; }
    public int ProcessedCount { get; set; }
    public int SucceededCount { get; set; }
    public int FailedCount { get; set; }
}
