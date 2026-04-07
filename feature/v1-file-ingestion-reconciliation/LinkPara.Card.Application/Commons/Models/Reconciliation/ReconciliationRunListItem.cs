using LinkPara.Card.Domain.Enums;

namespace LinkPara.Card.Application.Commons.Models.Reconciliation;

public class ReconciliationRunListItem
{
    public Guid CardTransactionRecordId { get; set; }
    public Guid? ClearingRecordId { get; set; }
    public Guid RunId { get; set; }
    public Guid ExecutionGroupId { get; set; }
    public ReconciliationOperationMode OperationMode { get; set; }
    public ReconciliationOperationStatus RunStatus { get; set; }
    public DateTime CreateDate { get; set; }
    public IReadOnlyCollection<Guid> ManualReviewItemIds { get; set; } = [];
    public IReadOnlyCollection<ManualReviewListItem> ManualReviews { get; set; } = [];
    public IReadOnlyCollection<ReconciliationRunOperationListItem> Operations { get; set; } = [];
    public string[] PlannedOperations { get; set; } = [];
    public string OperationPreview { get; set; } = string.Empty;
}

public class ReconciliationRunOperationListItem
{
    public Guid Id { get; set; }
    public int OperationIndex { get; set; }
    public string OperationShortCode { get; set; }
    public string OperationCode { get; set; }
    public string OperationText { get; set; }
    public string OperationHandlerName { get; set; }
    public ReconciliationOperationMode Mode { get; set; }
    public ReconciliationOperationStatus Status { get; set; }
}

public class ManualReviewListItem
{
    public Guid Id { get; set; }
    public ManualReviewStatus ReviewStatus { get; set; }
    public string ReviewNote { get; set; }
}
