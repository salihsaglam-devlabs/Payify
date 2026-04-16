using LinkPara.Card.Domain.Enums.FileIngestion;
using LinkPara.Card.Domain.Enums.Reporting;

namespace LinkPara.Card.Application.Commons.Models.Reporting.Dtos;

public class IngestionFileOverviewDto
{
    public Guid FileId { get; set; }
    public string FileKey { get; set; }
    public string FileName { get; set; }
    public FileSourceType SourceType { get; set; }
    public FileType FileType { get; set; }
    public FileContentType ContentType { get; set; }
    public FileStatus FileStatus { get; set; }
    public string FileMessage { get; set; }
    public long ExpectedLineCount { get; set; }
    public long ProcessedLineCount { get; set; }
    public long SuccessfulLineCount { get; set; }
    public long FailedLineCount { get; set; }
    public long LastProcessedLineNumber { get; set; }
    public long LastProcessedByteOffset { get; set; }
    public bool IsArchived { get; set; }
    public DateTime FileCreatedAt { get; set; }
    public DateTime? FileUpdatedAt { get; set; }
    public decimal LineSuccessRatePct { get; set; }
    public decimal LineFailRatePct { get; set; }
    public decimal CompletenessPct { get; set; }
    public long ActualLineCount { get; set; }
    public long ActualSuccessLineCount { get; set; }
    public long ActualFailedLineCount { get; set; }
    public long ActualProcessingLineCount { get; set; }
    public long DuplicateLineCount { get; set; }
    public long ReconReadyCount { get; set; }
    public long ReconSuccessCount { get; set; }
    public long ReconFailedCount { get; set; }
    public long ProcessingDurationSeconds { get; set; }
    public DataScope DataScope { get; set; }
}

public class IngestionFileQualityDto
{
    public Guid FileId { get; set; }
    public string FileName { get; set; }
    public FileType FileType { get; set; }
    public FileContentType ContentType { get; set; }
    public FileStatus FileStatus { get; set; }
    public DateTime FileCreatedAt { get; set; }
    public long TotalLineCount { get; set; }
    public long SuccessLineCount { get; set; }
    public long FailedLineCount { get; set; }
    public long ProcessingLineCount { get; set; }
    public long DuplicateUniqueCount { get; set; }
    public long DuplicatePrimaryCount { get; set; }
    public long DuplicateSecondaryCount { get; set; }
    public long DuplicateConflictCount { get; set; }
    public long TotalRetryCount { get; set; }
    public long LinesWithRetryCount { get; set; }
    public decimal ErrorRatePct { get; set; }
    public decimal DuplicateImpactPct { get; set; }
    public DataScope DataScope { get; set; }
}

public class IngestionDailySummaryDto
{
    public DateTime ReportDate { get; set; }
    public FileContentType ContentType { get; set; }
    public FileType FileType { get; set; }
    public long FileCount { get; set; }
    public long SuccessFileCount { get; set; }
    public long FailedFileCount { get; set; }
    public long ProcessingFileCount { get; set; }
    public long? ExpectedLineCount { get; set; }
    public long? ProcessedLineCount { get; set; }
    public long? SuccessfulLineCount { get; set; }
    public long? FailedLineCount { get; set; }
    public decimal ProcessedLineSuccessRatePct { get; set; }
    public DataScope DataScope { get; set; }
}

public class IngestionNetworkMatrixDto
{
    public FileContentType ContentType { get; set; }
    public FileType FileType { get; set; }
    public long FileCount { get; set; }
    public long SuccessFileCount { get; set; }
    public long FailedFileCount { get; set; }
    public long? ProcessedLineCount { get; set; }
    public long? SuccessfulLineCount { get; set; }
    public long? FailedLineCount { get; set; }
    public DateTime FirstFileAt { get; set; }
    public DateTime LastFileAt { get; set; }
    public DataScope DataScope { get; set; }
}

public class IngestionExceptionHotspotDto
{
    public Guid FileId { get; set; }
    public string FileName { get; set; }
    public FileSourceType SourceType { get; set; }
    public FileType FileType { get; set; }
    public FileContentType ContentType { get; set; }
    public FileStatus FileStatus { get; set; }
    public string FileMessage { get; set; }
    public DateTime FileCreatedAt { get; set; }
    public long FailedLineCount { get; set; }
    public long ProcessedLineCount { get; set; }
    public long TotalRetryCount { get; set; }
    public long MaxRetryCount { get; set; }
    public long DistinctErrorMessageCount { get; set; }
    public SeverityLevel SeverityLevel { get; set; }
    public DataScope DataScope { get; set; }
}
