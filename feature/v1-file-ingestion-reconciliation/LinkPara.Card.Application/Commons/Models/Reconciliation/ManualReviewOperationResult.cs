namespace LinkPara.Card.Application.Commons.Models.Reconciliation;

public class ManualReviewOperationResult
{
    public ManualReviewOperationStatus Status { get; set; }
    public string Message { get; set; }

    public static ManualReviewOperationResult Success(string message = null) => new()
    {
        Status = ManualReviewOperationStatus.Success,
        Message = message
    };

    public static ManualReviewOperationResult NotFound(string message = null) => new()
    {
        Status = ManualReviewOperationStatus.NotFound,
        Message = message
    };

    public static ManualReviewOperationResult AlreadyReviewed(string message = null) => new()
    {
        Status = ManualReviewOperationStatus.AlreadyReviewed,
        Message = message
    };

    public static ManualReviewOperationResult OperationExecutionFailed(string message = null) => new()
    {
        Status = ManualReviewOperationStatus.OperationExecutionFailed,
        Message = message
    };

    public static ManualReviewOperationResult StaleRun(string message = null) => new()
    {
        Status = ManualReviewOperationStatus.StaleRun,
        Message = message
    };
}
