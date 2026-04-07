namespace LinkPara.Card.API.Constants;

public static class EndpointRoutes
{
    public static class FileIngestionService
    {
        public const string ImportCardTransactionsFromLocal = "CardTransactions/Import/Local";
        public const string ImportCardTransactionsFromFtp = "CardTransactions/Import/Ftp";
        public const string ImportClearingFromLocal = "Clearing/Import/Local";
        public const string ImportClearingFromFtp = "Clearing/Import/Ftp";
    }

    public static class ReconciliationManualReviewService
    {
        public const string GetPendingManualReviews = "ManualOperations/Reviews/Pending";
        public const string GetManualReviewDecisionPreview = "ManualOperations/Reviews/{manualReviewItemId:guid}/DecisionPreview";
        public const string ApproveManualReview = "ManualOperations/Reviews/{manualReviewItemId:guid}/Approve";
        public const string RejectManualReview = "ManualOperations/Reviews/{manualReviewItemId:guid}/Reject";
    }

    public static class ReconciliationAutoOperationService
    {
        public const string ExecutePendingOperations = "AutoOperations/Pending/Execute";
    }

    public static class ReconciliationService
    {
        public const string RegenerateOperations = "Operations/Regenerate";
    }
}
