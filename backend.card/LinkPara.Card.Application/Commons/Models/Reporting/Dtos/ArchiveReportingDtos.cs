using LinkPara.Card.Domain.Enums.FileIngestion;
using LinkPara.Card.Domain.Enums.Reporting;

namespace LinkPara.Card.Application.Commons.Models.Reporting.Dtos;

public class ArchiveRunOverviewDto
{
    public Guid ArchiveLogId { get; set; }
    public Guid? IngestionFileId { get; set; }
    public string FileName { get; set; }
    public FileType FileType { get; set; }
    public FileContentType ContentType { get; set; }
    public string ArchiveStatus { get; set; }
    public string ArchiveMessage { get; set; }
    public string FailureReasonsJson { get; set; }
    public string FilterJson { get; set; }
    public DateTime ArchiveStartedAt { get; set; }
    public DateTime? ArchiveUpdatedAt { get; set; }
    public long ArchiveDurationSeconds { get; set; }
}

public class ArchiveEligibilityDto
{
    public Guid FileId { get; set; }
    public string FileName { get; set; }
    public FileType FileType { get; set; }
    public FileContentType ContentType { get; set; }
    public FileStatus FileStatus { get; set; }
    public bool IsArchived { get; set; }
    public DateTime FileCreatedAt { get; set; }
    public decimal AgeDays { get; set; }
    public long TotalReconLineCount { get; set; }
    public long ReconSuccessLineCount { get; set; }
    public long ReconOpenLineCount { get; set; }
    public ArchiveEligibilityStatus ArchiveEligibilityStatus { get; set; }
}

public class ArchiveBacklogTrendDto
{
    public DateTime ReportDate { get; set; }
    public long ArchiveRunCount { get; set; }
    public long SuccessRunCount { get; set; }
    public long FailedRunCount { get; set; }
    public long OtherRunCount { get; set; }
}

public class ArchiveRetentionSnapshotDto
{
    public long ActiveFileCount { get; set; }
    public long ArchivedMarkedFileCount { get; set; }
    public long ArchiveTableFileCount { get; set; }
    public long ArchiveTableFileLineCount { get; set; }
    public long ArchiveTableEvaluationCount { get; set; }
    public long ArchiveTableOperationCount { get; set; }
    public long ArchiveTableReviewCount { get; set; }
    public long ArchiveTableAlertCount { get; set; }
    public long ArchiveTableExecutionCount { get; set; }
    public DateTime? OldestUnarchivedFileDate { get; set; }
}
