using System;

namespace LinkPara.Card.Application.Commons.Models.Reconciliation;

public class ReconciliationProcessSummaryDetails
{
    public Guid? ExecutionGroupId { get; set; }
    public Guid[] GeneratedRunIds { get; set; } = [];
    public ReconciliationSummaryCounts InMemoryCounts { get; set; }
    public ReconciliationSummaryCounts DbAuditCounts { get; set; }
}
