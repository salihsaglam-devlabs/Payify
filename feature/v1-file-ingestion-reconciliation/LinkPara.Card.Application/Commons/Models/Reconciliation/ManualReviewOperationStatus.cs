namespace LinkPara.Card.Application.Commons.Models.Reconciliation;

public enum ManualReviewOperationStatus
{
    Success = 0,
    NotFound = 1,
    AlreadyReviewed = 2,
    OperationExecutionFailed = 3,
    StaleRun = 4
}
