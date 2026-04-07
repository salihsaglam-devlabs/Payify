namespace LinkPara.Card.Application.Commons.Models.Reconciliation;

public class ReconciliationProcessSummary
{
    public int ReconciledCards { get; set; }
    public int ReconciliationOperations { get; set; }
    public int ReconciliationManualReviewItems { get; set; }
    public ReconciliationProcessSummaryDetails Details { get; set; }
}
