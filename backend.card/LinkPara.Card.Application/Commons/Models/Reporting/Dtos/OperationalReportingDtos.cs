using LinkPara.Card.Domain.Enums.Reporting;

namespace LinkPara.Card.Application.Commons.Models.Reporting.Dtos;

// O1 - rep_action_radar
public class ActionRadarDto
{
    public DataScope DataScope { get; set; }
    public string Category { get; set; }
    public string IssueType { get; set; }
    public long OpenCount { get; set; }
    public decimal? OldestAgeHours { get; set; }
    public string Urgency { get; set; }
    public string RecommendedAction { get; set; }
}

// O2 - rep_unhealthy_files
public class UnhealthyFileDto
{
    public DataScope DataScope { get; set; }
    public Guid FileId { get; set; }
    public string FileName { get; set; }
    public string Side { get; set; }
    public string Network { get; set; }
    public string FileStatus { get; set; }
    public int? ExpectedLineCount { get; set; }
    public int? ProcessedLineCount { get; set; }
    public int? FailedLineCount { get; set; }
    public decimal FailureRatePct { get; set; }
    public decimal? AgeHours { get; set; }
    public string IssueCategory { get; set; }
    public string Urgency { get; set; }
    public string RecommendedAction { get; set; }
    public string FileMessage { get; set; }
}

// O3 - rep_stuck_pipeline_items
public class StuckPipelineItemDto
{
    public DataScope DataScope { get; set; }
    public string Stage { get; set; }
    public string ItemId { get; set; }
    public string RelatedId { get; set; }
    public DateTime? StartedAt { get; set; }
    public decimal? StuckMinutes { get; set; }
    public string LeaseOwner { get; set; }
    public DateTime? LeaseExpiresAt { get; set; }
    public string StuckState { get; set; }
    public string Urgency { get; set; }
    public string RecommendedAction { get; set; }
}

// O4 - rep_recon_failure_categorization
public class ReconFailureCategorizationDto
{
    public DataScope DataScope { get; set; }
    public string OperationCode { get; set; }
    public string Branch { get; set; }
    public long FailedCount { get; set; }
    public long RetriesExhaustedCount { get; set; }
    public long ManualOperationCount { get; set; }
    public DateTime? OldestFailureAt { get; set; }
    public decimal? OldestAgeHours { get; set; }
    public string LikelyRootCause { get; set; }
    public string Urgency { get; set; }
    public string RecommendedAction { get; set; }
}

// O5 - rep_manual_review_pressure
public class ManualReviewPressureDto
{
    public DataScope DataScope { get; set; }
    public string SlaBucket { get; set; }
    public string DefaultOnExpiry { get; set; }
    public string Currency { get; set; }
    public long PendingReviewCount { get; set; }
    public decimal? OldestWaitingHours { get; set; }
    public decimal? ExposureAmount { get; set; }
    public string Urgency { get; set; }
    public string RecommendedAction { get; set; }
}

// O6 - rep_alert_delivery_health
public class AlertDeliveryHealthDto
{
    public DataScope DataScope { get; set; }
    public string Severity { get; set; }
    public string AlertType { get; set; }
    public long TotalCount { get; set; }
    public long PendingCount { get; set; }
    public long FailedCount { get; set; }
    public long SentCount { get; set; }
    public decimal FailureRatePct { get; set; }
    public decimal? OldestOpenAgeHours { get; set; }
    public string DeliveryHealthStatus { get; set; }
    public string Urgency { get; set; }
    public string RecommendedAction { get; set; }
}

// O7 - rep_unmatched_financial_exposure
public class UnmatchedFinancialExposureDto
{
    public DataScope DataScope { get; set; }
    public string Side { get; set; }
    public string Network { get; set; }
    public string Currency { get; set; }
    public string AgingBucket { get; set; }
    public long UnmatchedCount { get; set; }
    public decimal? ExposureAmount { get; set; }
    public DateTime? OldestUnmatchedAt { get; set; }
    public decimal? OldestAgeDays { get; set; }
    public string RiskFlag { get; set; }
    public string Urgency { get; set; }
    public string RecommendedAction { get; set; }
}

// O8 - rep_card_clearing_imbalance
public class CardClearingImbalanceDto
{
    public DataScope DataScope { get; set; }
    public DateTime? ReportDate { get; set; }
    public string Network { get; set; }
    public string Currency { get; set; }
    public long CardLineCount { get; set; }
    public long ClearingLineCount { get; set; }
    public decimal? CardTotalAmount { get; set; }
    public decimal? ClearingTotalAmount { get; set; }
    public decimal? AmountGap { get; set; }
    public decimal? AbsGap { get; set; }
    public decimal? GapRatioPct { get; set; }
    public string ImbalanceSeverity { get; set; }
    public string Urgency { get; set; }
    public string RecommendedAction { get; set; }
}

// O9 - rep_reconciliation_quality_score
public class ReconciliationQualityScoreDto
{
    public DataScope DataScope { get; set; }
    public DateTime? ReportDate { get; set; }
    public string Network { get; set; }
    public long TotalLines { get; set; }
    public long MatchedLines { get; set; }
    public long ReconFailedLines { get; set; }
    public long TotalRetries { get; set; }
    public long ReviewedLines { get; set; }
    public decimal MatchRatePct { get; set; }
    public decimal ReconFailureRatePct { get; set; }
    public decimal AvgRetriesPerLine { get; set; }
    public decimal ManualReviewRatePct { get; set; }
    public string QualityGrade { get; set; }
    public string WeakestDimension { get; set; }
    public string Urgency { get; set; }
    public string RecommendedAction { get; set; }
}

// O10 - rep_misleading_success_cases
public class MisleadingSuccessCaseDto
{
    public DataScope DataScope { get; set; }
    public DateTime? ReportDate { get; set; }
    public string Network { get; set; }
    public string Side { get; set; }
    public string Currency { get; set; }
    public long LineCount { get; set; }
    public long MatchedCount { get; set; }
    public decimal? TotalAmount { get; set; }
    public decimal? MatchedAmount { get; set; }
    public decimal? UnmatchedAmount { get; set; }
    public decimal CountMatchRatePct { get; set; }
    public decimal AmountMatchRatePct { get; set; }
    public string MisleadingPattern { get; set; }
    public string Urgency { get; set; }
    public string RecommendedAction { get; set; }
}

// O11 - rep_archive_pipeline_health
public class ArchivePipelineHealthDto
{
    public DataScope DataScope { get; set; }
    public string Perspective { get; set; }
    public string ReferenceId { get; set; }
    public string FileName { get; set; }
    public string Side { get; set; }
    public string Network { get; set; }
    public DateTime? ReferenceDate { get; set; }
    public decimal? AgeDays { get; set; }
    public string ArchiveStatus { get; set; }
    public string ArchiveMessage { get; set; }
    public string PipelineHealth { get; set; }
    public string Urgency { get; set; }
    public string RecommendedAction { get; set; }
}

// D - rep_documentation
public class ReportingDocumentationDto
{
    public string ViewName { get; set; }
    public string ReportGroup { get; set; }
    public string PurposeTr { get; set; }
    public string PurposeEn { get; set; }
    public string BusinessQuestionTr { get; set; }
    public string BusinessQuestionEn { get; set; }
    public string InterpretationTr { get; set; }
    public string InterpretationEn { get; set; }
    public string UsageTimeTr { get; set; }
    public string UsageTimeEn { get; set; }
    public string TargetUserTr { get; set; }
    public string TargetUserEn { get; set; }
    public string ActionGuidanceTr { get; set; }
    public string ActionGuidanceEn { get; set; }
    public string ImportantColumnsTr { get; set; }
    public string ImportantColumnsEn { get; set; }
    public string LiveArchiveInterpretationTr { get; set; }
    public string LiveArchiveInterpretationEn { get; set; }
    public string NotesTr { get; set; }
    public string NotesEn { get; set; }
}

