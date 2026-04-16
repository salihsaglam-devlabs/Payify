using LinkPara.Card.Domain.Enums.FileIngestion;
using LinkPara.Card.Domain.Enums.Reconciliation;
using LinkPara.Card.Domain.Enums.Reporting;

namespace LinkPara.Card.Application.Commons.Models.Reporting.Dtos;

public class ReconDailyOverviewDto
{
    public DateTime ReportDate { get; set; }
    public long TotalEvaluationCount { get; set; }
    public long CompletedEvaluationCount { get; set; }
    public long FailedEvaluationCount { get; set; }
    public long TotalOperationCount { get; set; }
    public long CompletedOperationCount { get; set; }
    public long FailedOperationCount { get; set; }
    public long BlockedOperationCount { get; set; }
    public long PlannedOperationCount { get; set; }
    public long ManualOperationCount { get; set; }
    public long TotalExecutionCount { get; set; }
    public long CompletedExecutionCount { get; set; }
    public long FailedExecutionCount { get; set; }
    public decimal AvgExecutionDurationSeconds { get; set; }
    public long PendingReviewCount { get; set; }
    public long ApprovedReviewCount { get; set; }
    public long RejectedReviewCount { get; set; }
    public long PendingAlertCount { get; set; }
    public long FailedAlertCount { get; set; }
    public decimal OperationSuccessRatePct { get; set; }
    public DataScope DataScope { get; set; }
}

public class ReconOpenItemDto
{
    public Guid OperationId { get; set; }
    public Guid FileLineId { get; set; }
    public Guid EvaluationId { get; set; }
    public Guid GroupId { get; set; }
    public int SequenceNumber { get; set; }
    public int? ParentSequenceNumber { get; set; }
    public string OperationCode { get; set; }
    public string Branch { get; set; }
    public bool IsManual { get; set; }
    public OperationStatus OperationStatus { get; set; }
    public int RetryCount { get; set; }
    public int MaxRetryCount { get; set; }
    public DateTime? NextAttemptAt { get; set; }
    public string LeaseOwner { get; set; }
    public DateTime? LeaseExpiresAt { get; set; }
    public string LastError { get; set; }
    public DateTime OperationCreatedAt { get; set; }
    public DateTime? OperationUpdatedAt { get; set; }
    public EvaluationStatus EvaluationStatus { get; set; }
    public int EvaluationOperationCount { get; set; }
    public decimal AgeHours { get; set; }
}

public class ReconOpenItemAgingDto
{
    public string BucketName { get; set; }
    public long ItemCount { get; set; }
    public long PlannedCount { get; set; }
    public long BlockedCount { get; set; }
    public long ExecutingCount { get; set; }
    public long ManualCount { get; set; }
}

public class ReconManualReviewQueueDto
{
    public Guid ReviewId { get; set; }
    public Guid FileLineId { get; set; }
    public Guid GroupId { get; set; }
    public Guid EvaluationId { get; set; }
    public Guid OperationId { get; set; }
    public Guid? ReviewerId { get; set; }
    public ReviewDecision Decision { get; set; }
    public string Comment { get; set; }
    public DateTime? DecisionAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public ReviewExpirationAction ExpirationAction { get; set; }
    public ReviewExpirationFlowAction ExpirationFlowAction { get; set; }
    public DateTime ReviewCreatedAt { get; set; }
    
    public string OperationCode { get; set; }
    public string OperationBranch { get; set; }
    public OperationStatus OperationStatus { get; set; }
    public bool OperationIsManual { get; set; }
    public string OperationNote { get; set; }
    public int OperationRetryCount { get; set; }
    public int OperationMaxRetries { get; set; }
    public DateTime? OperationNextAttemptAt { get; set; }
    public string OperationLeaseOwner { get; set; }
    public DateTime? OperationLeaseExpiresAt { get; set; }
    public string OperationLastError { get; set; }
    public string OperationPayload { get; set; }
    public DateTime OperationCreatedAt { get; set; }
    public DateTime? OperationUpdatedAt { get; set; }
    
    public EvaluationStatus EvaluationStatus { get; set; }
    public string EvaluationMessage { get; set; }
    public int EvaluationOperationCount { get; set; }
    public DateTime EvaluationCreatedAt { get; set; }
    
    public Guid? LastExecutionId { get; set; }
    public int? LastAttemptNumber { get; set; }
    public string LastExecutionStatus { get; set; }
    public DateTime? LastExecutionStartedAt { get; set; }
    public DateTime? LastExecutionFinishedAt { get; set; }
    public string LastExecutionResultCode { get; set; }
    public string LastExecutionResultMessage { get; set; }
    public string LastExecutionErrorCode { get; set; }
    public string LastExecutionErrorMessage { get; set; }
    public long? TotalExecutionCount { get; set; }
    
    public string FileName { get; set; }
    public string FileKey { get; set; }
    public FileSourceType? FileSourceType { get; set; }
    public FileType? FileType { get; set; }
    public FileContentType? ContentType { get; set; }
    public FileStatus? FileStatus { get; set; }
    
    public int? LineNumber { get; set; }
    public string LineRecordType { get; set; }
    public FileRowStatus? LineStatus { get; set; }
    public ReconciliationStatus? LineReconciliationStatus { get; set; }
    public Guid? MatchedClearingLineId { get; set; }
    public string CorrelationKey { get; set; }
    public string CorrelationValue { get; set; }
    public DuplicateStatus? LineDuplicateStatus { get; set; }
    public string LineMessage { get; set; }
    
    public int? CardTransactionDate { get; set; }
    public int? CardTransactionTime { get; set; }
    public decimal? CardOriginalAmount { get; set; }
    public int? CardOriginalCurrency { get; set; }
    public decimal? CardSettlementAmount { get; set; }
    public decimal? CardBillingAmount { get; set; }
    public string CardFinancialType { get; set; }
    public string CardTxnEffect { get; set; }
    public string CardResponseCode { get; set; }
    public string CardIsSuccessfulTxn { get; set; }
    public string CardRrn { get; set; }
    public string CardArn { get; set; }
    
    public int? ClearingTxnDate { get; set; }
    public int? ClearingTxnTime { get; set; }
    public string ClearingIoDate { get; set; }
    public decimal? ClearingSourceAmount { get; set; }
    public int? ClearingSourceCurrency { get; set; }
    public decimal? ClearingDestinationAmount { get; set; }
    public string ClearingTxnType { get; set; }
    public string ClearingIoFlag { get; set; }
    public string ClearingControlStat { get; set; }
    public string ClearingRrn { get; set; }
    public string ClearingArn { get; set; }
    
    public decimal WaitingHours { get; set; }
    public UrgencyLevel UrgencyLevel { get; set; }
    public string EffectiveError { get; set; }
}

public class ReconAlertSummaryDto
{
    public string Severity { get; set; }
    public string AlertType { get; set; }
    public AlertStatus AlertStatus { get; set; }
    public long AlertCount { get; set; }
    public long DistinctGroupCount { get; set; }
    public long DistinctOperationCount { get; set; }
    public DateTime FirstAlertAt { get; set; }
    public DateTime LastAlertAt { get; set; }
    public DataScope DataScope { get; set; }
}
