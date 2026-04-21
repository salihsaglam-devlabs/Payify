-- =====================================================================================
-- REPORTING VIEWS - MS SQL SERVER (PascalCase Schema, View Names, and Column Names)
-- Version: V1_0_4 (SQL Server)
-- Purpose: Operational reporting semantic layer for File Ingestion, Reconciliation, Archive
-- =====================================================================================

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Reporting')
BEGIN
EXEC('CREATE SCHEMA [Reporting]');
END;
GO

-- =====================================================================================
-- A. FILE INGESTION
-- =====================================================================================

-- A1. Dosya bazında genel operasyon özeti (LIVE + ARCHIVE)
CREATE OR ALTER VIEW [Reporting].[VwIngestionFileOverview] AS
SELECT * FROM (
                  -- LIVE
                  SELECT
                      'LIVE' AS DataScope,
                      f.Id AS FileId,
                      f.FileKey AS FileKey,
                      f.FileName AS FileName,
                      f.SourceType AS SourceType,
                      f.FileType AS FileType,
                      f.ContentType AS ContentType,
                      f.Status AS FileStatus,
                      f.Message AS FileMessage,
                      f.ExpectedLineCount AS ExpectedLineCount,
                      f.ProcessedLineCount AS ProcessedLineCount,
                      f.SuccessfulLineCount AS SuccessfulLineCount,
                      f.FailedLineCount AS FailedLineCount,
                      f.LastProcessedLineNumber AS LastProcessedLineNumber,
                      f.LastProcessedByteOffset AS LastProcessedByteOffset,
                      f.IsArchived AS IsArchived,
                      f.CreateDate AS FileCreatedAt,
                      f.UpdateDate AS FileUpdatedAt,
                      CASE WHEN f.ProcessedLineCount > 0 THEN ROUND(CAST(f.SuccessfulLineCount AS DECIMAL(38,4)) / f.ProcessedLineCount * 100, 2) ELSE 0 END AS LineSuccessRatePct,
                      CASE WHEN f.ProcessedLineCount > 0 THEN ROUND(CAST(f.FailedLineCount AS DECIMAL(38,4)) / f.ProcessedLineCount * 100, 2) ELSE 0 END AS LineFailRatePct,
                      CASE WHEN f.ExpectedLineCount > 0 THEN ROUND(CAST(f.ProcessedLineCount AS DECIMAL(38,4)) / f.ExpectedLineCount * 100, 2) ELSE 0 END AS CompletenessPct,
                      ISNULL(fl.TotalLineCount, 0) AS ActualLineCount,
                      ISNULL(fl.SuccessLineCount, 0) AS ActualSuccessLineCount,
                      ISNULL(fl.FailedLineCount, 0) AS ActualFailedLineCount,
                      ISNULL(fl.ProcessingLineCount, 0) AS ActualProcessingLineCount,
                      ISNULL(fl.DuplicateLineCount, 0) AS DuplicateLineCount,
                      ISNULL(fl.ReconReadyCount, 0) AS ReconReadyCount,
                      ISNULL(fl.ReconSuccessCount, 0) AS ReconSuccessCount,
                      ISNULL(fl.ReconFailedCount, 0) AS ReconFailedCount,
                      DATEDIFF(SECOND, f.CreateDate, ISNULL(f.UpdateDate, f.CreateDate)) AS ProcessingDurationSeconds
                  FROM [Ingestion].[File] f
                      OUTER APPLY (
                      SELECT
                      COUNT(*) AS TotalLineCount,
                      SUM(CASE WHEN line.Status = 'Success' THEN 1 ELSE 0 END) AS SuccessLineCount,
                      SUM(CASE WHEN line.Status = 'Failed' THEN 1 ELSE 0 END) AS FailedLineCount,
                      SUM(CASE WHEN line.Status = 'Processing' THEN 1 ELSE 0 END) AS ProcessingLineCount,
                      SUM(CASE WHEN line.DuplicateStatus IN ('Secondary', 'Conflict') THEN 1 ELSE 0 END) AS DuplicateLineCount,
                      SUM(CASE WHEN line.ReconciliationStatus = 'Ready' THEN 1 ELSE 0 END) AS ReconReadyCount,
                      SUM(CASE WHEN line.ReconciliationStatus = 'Success' THEN 1 ELSE 0 END) AS ReconSuccessCount,
                      SUM(CASE WHEN line.ReconciliationStatus = 'Failed' THEN 1 ELSE 0 END) AS ReconFailedCount
                      FROM [Ingestion].[FileLine] line
                      WHERE line.FileId = f.Id
                      ) fl

                  UNION ALL

                  -- ARCHIVE
                  SELECT
                      'ARCHIVE' AS DataScope,
                      f.Id AS FileId,
                      f.FileKey AS FileKey,
                      f.FileName AS FileName,
                      f.SourceType AS SourceType,
                      f.FileType AS FileType,
                      f.ContentType AS ContentType,
                      f.Status AS FileStatus,
                      f.Message AS FileMessage,
                      f.ExpectedLineCount AS ExpectedLineCount,
                      f.ProcessedLineCount AS ProcessedLineCount,
                      f.SuccessfulLineCount AS SuccessfulLineCount,
                      f.FailedLineCount AS FailedLineCount,
                      f.LastProcessedLineNumber AS LastProcessedLineNumber,
                      f.LastProcessedByteOffset AS LastProcessedByteOffset,
                      f.IsArchived AS IsArchived,
                      f.CreateDate AS FileCreatedAt,
                      f.UpdateDate AS FileUpdatedAt,
                      CASE WHEN f.ProcessedLineCount > 0 THEN ROUND(CAST(f.SuccessfulLineCount AS DECIMAL(38,4)) / f.ProcessedLineCount * 100, 2) ELSE 0 END,
                      CASE WHEN f.ProcessedLineCount > 0 THEN ROUND(CAST(f.FailedLineCount AS DECIMAL(38,4)) / f.ProcessedLineCount * 100, 2) ELSE 0 END,
                      CASE WHEN f.ExpectedLineCount > 0 THEN ROUND(CAST(f.ProcessedLineCount AS DECIMAL(38,4)) / f.ExpectedLineCount * 100, 2) ELSE 0 END,
                      ISNULL(fl.TotalLineCount, 0),
                      ISNULL(fl.SuccessLineCount, 0),
                      ISNULL(fl.FailedLineCount, 0),
                      ISNULL(fl.ProcessingLineCount, 0),
                      ISNULL(fl.DuplicateLineCount, 0),
                      ISNULL(fl.ReconReadyCount, 0),
                      ISNULL(fl.ReconSuccessCount, 0),
                      ISNULL(fl.ReconFailedCount, 0),
                      DATEDIFF(SECOND, f.CreateDate, ISNULL(f.UpdateDate, f.CreateDate))
                  FROM [Archive].[IngestionFile] f
                      OUTER APPLY (
                      SELECT
                      COUNT(*) AS TotalLineCount,
                      SUM(CASE WHEN line.Status = 'Success' THEN 1 ELSE 0 END) AS SuccessLineCount,
                      SUM(CASE WHEN line.Status = 'Failed' THEN 1 ELSE 0 END) AS FailedLineCount,
                      SUM(CASE WHEN line.Status = 'Processing' THEN 1 ELSE 0 END) AS ProcessingLineCount,
                      SUM(CASE WHEN line.DuplicateStatus IN ('Secondary', 'Conflict') THEN 1 ELSE 0 END) AS DuplicateLineCount,
                      SUM(CASE WHEN line.ReconciliationStatus = 'Ready' THEN 1 ELSE 0 END) AS ReconReadyCount,
                      SUM(CASE WHEN line.ReconciliationStatus = 'Success' THEN 1 ELSE 0 END) AS ReconSuccessCount,
                      SUM(CASE WHEN line.ReconciliationStatus = 'Failed' THEN 1 ELSE 0 END) AS ReconFailedCount
                      FROM [Archive].[IngestionFileLine] line
                      WHERE line.FileId = f.Id
                      ) fl
              ) combined;
GO

-- A2. Dosya kalitesi ve duplicate etkisi
CREATE OR ALTER VIEW [Reporting].[VwIngestionFileQuality] AS
SELECT * FROM (
                  -- LIVE
                  SELECT
                      'LIVE' AS DataScope,
                      f.Id AS FileId,
                      f.FileName AS FileName,
                      f.FileType AS FileType,
                      f.ContentType AS ContentType,
                      f.Status AS FileStatus,
                      f.CreateDate AS FileCreatedAt,
                      ISNULL(x.TotalLineCount, 0) AS TotalLineCount,
                      ISNULL(x.SuccessLineCount, 0) AS SuccessLineCount,
                      ISNULL(x.FailedLineCount, 0) AS FailedLineCount,
                      ISNULL(x.ProcessingLineCount, 0) AS ProcessingLineCount,
                      ISNULL(x.UniqueCount, 0) AS DuplicateUniqueCount,
                      ISNULL(x.PrimaryCount, 0) AS DuplicatePrimaryCount,
                      ISNULL(x.SecondaryCount, 0) AS DuplicateSecondaryCount,
                      ISNULL(x.ConflictCount, 0) AS DuplicateConflictCount,
                      ISNULL(x.TotalRetryCount, 0) AS TotalRetryCount,
                      ISNULL(x.LinesWithRetryCount, 0) AS LinesWithRetryCount,
                      CASE WHEN ISNULL(x.TotalLineCount, 0) > 0 THEN ROUND(CAST(x.FailedLineCount AS DECIMAL(38,4)) / x.TotalLineCount * 100, 2) ELSE 0 END AS ErrorRatePct,
                      CASE WHEN ISNULL(x.TotalLineCount, 0) > 0 THEN ROUND((CAST(ISNULL(x.SecondaryCount,0) + ISNULL(x.ConflictCount,0) AS DECIMAL(38,4)) / x.TotalLineCount * 100), 2) ELSE 0 END AS DuplicateImpactPct
                  FROM [Ingestion].[File] f
                      OUTER APPLY (
                      SELECT
                      COUNT(*) AS TotalLineCount,
                      SUM(CASE WHEN line.Status = 'Success' THEN 1 ELSE 0 END) AS SuccessLineCount,
                      SUM(CASE WHEN line.Status = 'Failed' THEN 1 ELSE 0 END) AS FailedLineCount,
                      SUM(CASE WHEN line.Status = 'Processing' THEN 1 ELSE 0 END) AS ProcessingLineCount,
                      SUM(CASE WHEN line.DuplicateStatus = 'Unique' THEN 1 ELSE 0 END) AS UniqueCount,
                      SUM(CASE WHEN line.DuplicateStatus = 'Primary' THEN 1 ELSE 0 END) AS PrimaryCount,
                      SUM(CASE WHEN line.DuplicateStatus = 'Secondary' THEN 1 ELSE 0 END) AS SecondaryCount,
                      SUM(CASE WHEN line.DuplicateStatus = 'Conflict' THEN 1 ELSE 0 END) AS ConflictCount,
                      SUM(line.RetryCount) AS TotalRetryCount,
                      SUM(CASE WHEN line.RetryCount > 0 THEN 1 ELSE 0 END) AS LinesWithRetryCount
                      FROM [Ingestion].[FileLine] line
                      WHERE line.FileId = f.Id
                      ) x

                  UNION ALL

                  -- ARCHIVE
                  SELECT
                      'ARCHIVE',
                      f.Id,
                      f.FileName,
                      f.FileType,
                      f.ContentType,
                      f.Status,
                      f.CreateDate,
                      ISNULL(x.TotalLineCount, 0),
                      ISNULL(x.SuccessLineCount, 0),
                      ISNULL(x.FailedLineCount, 0),
                      ISNULL(x.ProcessingLineCount, 0),
                      ISNULL(x.UniqueCount, 0),
                      ISNULL(x.PrimaryCount, 0),
                      ISNULL(x.SecondaryCount, 0),
                      ISNULL(x.ConflictCount, 0),
                      ISNULL(x.TotalRetryCount, 0),
                      ISNULL(x.LinesWithRetryCount, 0),
                      CASE WHEN ISNULL(x.TotalLineCount, 0) > 0 THEN ROUND(CAST(x.FailedLineCount AS DECIMAL(38,4)) / x.TotalLineCount * 100, 2) ELSE 0 END,
                      CASE WHEN ISNULL(x.TotalLineCount, 0) > 0 THEN ROUND((CAST(ISNULL(x.SecondaryCount,0) + ISNULL(x.ConflictCount,0) AS DECIMAL(38,4)) / x.TotalLineCount * 100), 2) ELSE 0 END
                  FROM [Archive].[IngestionFile] f
                      OUTER APPLY (
                      SELECT
                      COUNT(*) AS TotalLineCount,
                      SUM(CASE WHEN line.Status = 'Success' THEN 1 ELSE 0 END) AS SuccessLineCount,
                      SUM(CASE WHEN line.Status = 'Failed' THEN 1 ELSE 0 END) AS FailedLineCount,
                      SUM(CASE WHEN line.Status = 'Processing' THEN 1 ELSE 0 END) AS ProcessingLineCount,
                      SUM(CASE WHEN line.DuplicateStatus = 'Unique' THEN 1 ELSE 0 END) AS UniqueCount,
                      SUM(CASE WHEN line.DuplicateStatus = 'Primary' THEN 1 ELSE 0 END) AS PrimaryCount,
                      SUM(CASE WHEN line.DuplicateStatus = 'Secondary' THEN 1 ELSE 0 END) AS SecondaryCount,
                      SUM(CASE WHEN line.DuplicateStatus = 'Conflict' THEN 1 ELSE 0 END) AS ConflictCount,
                      SUM(line.RetryCount) AS TotalRetryCount,
                      SUM(CASE WHEN line.RetryCount > 0 THEN 1 ELSE 0 END) AS LinesWithRetryCount
                      FROM [Archive].[IngestionFileLine] line
                      WHERE line.FileId = f.Id
                      ) x
              ) combined;
GO

-- A3. Günlük ingestion trendi
CREATE OR ALTER VIEW [Reporting].[VwIngestionDailySummary] AS
SELECT
    DataScope,
    ReportDate,
    ContentType,
    FileType,
    SUM(FileCount) AS FileCount,
    SUM(SuccessFileCount) AS SuccessFileCount,
    SUM(FailedFileCount) AS FailedFileCount,
    SUM(ProcessingFileCount) AS ProcessingFileCount,
    SUM(ExpectedLineCount) AS ExpectedLineCount,
    SUM(ProcessedLineCount) AS ProcessedLineCount,
    SUM(SuccessfulLineCount) AS SuccessfulLineCount,
    SUM(FailedLineCount) AS FailedLineCount,
    CASE WHEN SUM(ProcessedLineCount) > 0 THEN ROUND(CAST(SUM(SuccessfulLineCount) AS DECIMAL(38,4)) / SUM(ProcessedLineCount) * 100, 2) ELSE 0 END AS ProcessedLineSuccessRatePct
FROM (
         SELECT
             'LIVE' AS DataScope,
             CAST(f.CreateDate AS DATE) AS ReportDate,
             f.ContentType,
             f.FileType,
             COUNT(*) AS FileCount,
             SUM(CASE WHEN f.Status = 'Success' THEN 1 ELSE 0 END) AS SuccessFileCount,
             SUM(CASE WHEN f.Status = 'Failed' THEN 1 ELSE 0 END) AS FailedFileCount,
             SUM(CASE WHEN f.Status = 'Processing' THEN 1 ELSE 0 END) AS ProcessingFileCount,
             SUM(f.ExpectedLineCount) AS ExpectedLineCount,
             SUM(f.ProcessedLineCount) AS ProcessedLineCount,
             SUM(f.SuccessfulLineCount) AS SuccessfulLineCount,
             SUM(f.FailedLineCount) AS FailedLineCount
         FROM [Ingestion].[File] f
         GROUP BY CAST(f.CreateDate AS DATE), f.ContentType, f.FileType
         UNION ALL
         SELECT
             'ARCHIVE',
             CAST(f.CreateDate AS DATE),
             f.ContentType,
             f.FileType,
             COUNT(*),
             SUM(CASE WHEN f.Status = 'Success' THEN 1 ELSE 0 END),
             SUM(CASE WHEN f.Status = 'Failed' THEN 1 ELSE 0 END),
             SUM(CASE WHEN f.Status = 'Processing' THEN 1 ELSE 0 END),
             SUM(f.ExpectedLineCount),
             SUM(f.ProcessedLineCount),
             SUM(f.SuccessfulLineCount),
             SUM(f.FailedLineCount)
         FROM [Archive].[IngestionFile] f
         GROUP BY CAST(f.CreateDate AS DATE), f.ContentType, f.FileType
     ) src
GROUP BY DataScope, ReportDate, ContentType, FileType;
GO

-- A4. Network x file type matrisi
CREATE OR ALTER VIEW [Reporting].[VwIngestionNetworkMatrix] AS
SELECT
    DataScope,
    ContentType,
    FileType,
    SUM(FileCount) AS FileCount,
    SUM(SuccessFileCount) AS SuccessFileCount,
    SUM(FailedFileCount) AS FailedFileCount,
    SUM(ProcessedLineCount) AS ProcessedLineCount,
    SUM(SuccessfulLineCount) AS SuccessfulLineCount,
    SUM(FailedLineCount) AS FailedLineCount,
    MIN(FirstFileAt) AS FirstFileAt,
    MAX(LastFileAt) AS LastFileAt
FROM (
         SELECT
             'LIVE' AS DataScope,
             f.ContentType,
             f.FileType,
             COUNT(*) AS FileCount,
             SUM(CASE WHEN f.Status = 'Success' THEN 1 ELSE 0 END) AS SuccessFileCount,
             SUM(CASE WHEN f.Status = 'Failed' THEN 1 ELSE 0 END) AS FailedFileCount,
             SUM(f.ProcessedLineCount) AS ProcessedLineCount,
             SUM(f.SuccessfulLineCount) AS SuccessfulLineCount,
             SUM(f.FailedLineCount) AS FailedLineCount,
             MIN(f.CreateDate) AS FirstFileAt,
             MAX(f.CreateDate) AS LastFileAt
         FROM [Ingestion].[File] f
         GROUP BY f.ContentType, f.FileType
         UNION ALL
         SELECT
             'ARCHIVE',
             f.ContentType,
             f.FileType,
             COUNT(*),
             SUM(CASE WHEN f.Status = 'Success' THEN 1 ELSE 0 END),
             SUM(CASE WHEN f.Status = 'Failed' THEN 1 ELSE 0 END),
             SUM(f.ProcessedLineCount),
             SUM(f.SuccessfulLineCount),
             SUM(f.FailedLineCount),
             MIN(f.CreateDate),
             MAX(f.CreateDate)
         FROM [Archive].[IngestionFile] f
         GROUP BY f.ContentType, f.FileType
     ) src
GROUP BY DataScope, ContentType, FileType;
GO

-- A5. Problem hotspot dosyaları
CREATE OR ALTER VIEW [Reporting].[VwIngestionExceptionHotspots] AS
SELECT * FROM (
                  -- LIVE
                  SELECT
                      'LIVE' AS DataScope,
                      f.Id AS FileId,
                      f.FileName AS FileName,
                      f.SourceType AS SourceType,
                      f.FileType AS FileType,
                      f.ContentType AS ContentType,
                      f.Status AS FileStatus,
                      f.Message AS FileMessage,
                      f.CreateDate AS FileCreatedAt,
                      f.FailedLineCount AS FailedLineCount,
                      f.ProcessedLineCount AS ProcessedLineCount,
                      ISNULL(x.TotalRetryCount, 0) AS TotalRetryCount,
                      ISNULL(x.MaxRetryCount, 0) AS MaxRetryCount,
                      ISNULL(x.ErrorMessageCount, 0) AS DistinctErrorMessageCount,
                      CASE
                          WHEN f.Status = 'Failed' THEN 'CRITICAL'
                          WHEN f.ProcessedLineCount > 0 AND (CAST(f.FailedLineCount AS DECIMAL(38,4)) / f.ProcessedLineCount) >= 0.20 THEN 'HIGH'
                          WHEN f.FailedLineCount > 0 THEN 'MEDIUM'
                          ELSE 'LOW'
                          END AS SeverityLevel
                  FROM [Ingestion].[File] f
                      OUTER APPLY (
                      SELECT
                      SUM(line.RetryCount) AS TotalRetryCount,
                      MAX(line.RetryCount) AS MaxRetryCount,
                      COUNT(DISTINCT CASE WHEN line.Status = 'Failed' THEN line.Message END) AS ErrorMessageCount
                      FROM [Ingestion].[FileLine] line
                      WHERE line.FileId = f.Id
                      ) x
                  WHERE f.Status = 'Failed' OR f.FailedLineCount > 0

                  UNION ALL

                  -- ARCHIVE
                  SELECT
                      'ARCHIVE',
                      f.Id,
                      f.FileName,
                      f.SourceType,
                      f.FileType,
                      f.ContentType,
                      f.Status,
                      f.Message,
                      f.CreateDate,
                      f.FailedLineCount,
                      f.ProcessedLineCount,
                      ISNULL(x.TotalRetryCount, 0),
                      ISNULL(x.MaxRetryCount, 0),
                      ISNULL(x.ErrorMessageCount, 0),
                      CASE
                      WHEN f.Status = 'Failed' THEN 'CRITICAL'
                      WHEN f.ProcessedLineCount > 0 AND (CAST(f.FailedLineCount AS DECIMAL(38,4)) / f.ProcessedLineCount) >= 0.20 THEN 'HIGH'
                      WHEN f.FailedLineCount > 0 THEN 'MEDIUM'
                      ELSE 'LOW'
                      END
                  FROM [Archive].[IngestionFile] f
                      OUTER APPLY (
                      SELECT
                      SUM(line.RetryCount) AS TotalRetryCount,
                      MAX(line.RetryCount) AS MaxRetryCount,
                      COUNT(DISTINCT CASE WHEN line.Status = 'Failed' THEN line.Message END) AS ErrorMessageCount
                      FROM [Archive].[IngestionFileLine] line
                      WHERE line.FileId = f.Id
                      ) x
                  WHERE f.Status = 'Failed' OR f.FailedLineCount > 0
              ) combined;
GO

-- =====================================================================================
-- B. RECONCILIATION PROCESS
-- =====================================================================================

-- B1. Günlük mutabakat süreç özeti
CREATE OR ALTER VIEW [Reporting].[VwReconDailyOverview] AS
WITH dates AS (
    SELECT DISTINCT CAST(CreateDate AS DATE) AS ReportDate, 'LIVE' AS DataScope FROM [Reconciliation].[Evaluation]
    UNION
    SELECT DISTINCT CAST(CreateDate AS DATE), 'LIVE' FROM [Reconciliation].[Operation]
    UNION
    SELECT DISTINCT CAST(CreateDate AS DATE), 'LIVE' FROM [Reconciliation].[Alert]
    UNION
    SELECT DISTINCT CAST(CreateDate AS DATE), 'LIVE' FROM [Reconciliation].[Review]
    UNION
    SELECT DISTINCT CAST(CreateDate AS DATE), 'ARCHIVE' FROM [Archive].[ReconciliationEvaluation]
    UNION
    SELECT DISTINCT CAST(CreateDate AS DATE), 'ARCHIVE' FROM [Archive].[ReconciliationOperation]
    UNION
    SELECT DISTINCT CAST(CreateDate AS DATE), 'ARCHIVE' FROM [Archive].[ReconciliationAlert]
    UNION
    SELECT DISTINCT CAST(CreateDate AS DATE), 'ARCHIVE' FROM [Archive].[ReconciliationReview]
)
SELECT
    d.DataScope,
    d.ReportDate,
    ISNULL(ev.TotalEvaluationCount, 0) AS TotalEvaluationCount,
    ISNULL(ev.CompletedEvaluationCount, 0) AS CompletedEvaluationCount,
    ISNULL(ev.FailedEvaluationCount, 0) AS FailedEvaluationCount,
    ISNULL(op.TotalOperationCount, 0) AS TotalOperationCount,
    ISNULL(op.CompletedOperationCount, 0) AS CompletedOperationCount,
    ISNULL(op.FailedOperationCount, 0) AS FailedOperationCount,
    ISNULL(op.BlockedOperationCount, 0) AS BlockedOperationCount,
    ISNULL(op.PlannedOperationCount, 0) AS PlannedOperationCount,
    ISNULL(op.ManualOperationCount, 0) AS ManualOperationCount,
    ISNULL(ex.TotalExecutionCount, 0) AS TotalExecutionCount,
    ISNULL(ex.CompletedExecutionCount, 0) AS CompletedExecutionCount,
    ISNULL(ex.FailedExecutionCount, 0) AS FailedExecutionCount,
    ISNULL(ex.AvgExecutionDurationSeconds, 0) AS AvgExecutionDurationSeconds,
    ISNULL(rv.PendingReviewCount, 0) AS PendingReviewCount,
    ISNULL(rv.ApprovedReviewCount, 0) AS ApprovedReviewCount,
    ISNULL(rv.RejectedReviewCount, 0) AS RejectedReviewCount,
    ISNULL(al.PendingAlertCount, 0) AS PendingAlertCount,
    ISNULL(al.FailedAlertCount, 0) AS FailedAlertCount,
    CASE WHEN ISNULL(op.TotalOperationCount, 0) > 0 THEN ROUND(CAST(op.CompletedOperationCount AS DECIMAL(38,4)) / op.TotalOperationCount * 100, 2) ELSE 0 END AS OperationSuccessRatePct
FROM dates d
         LEFT JOIN (
    SELECT 'LIVE' AS DataScope, CAST(CreateDate AS DATE) AS ReportDate,
           COUNT(*) AS TotalEvaluationCount,
           SUM(CASE WHEN Status = 'Completed' THEN 1 ELSE 0 END) AS CompletedEvaluationCount,
           SUM(CASE WHEN Status = 'Failed' THEN 1 ELSE 0 END) AS FailedEvaluationCount
    FROM [Reconciliation].[Evaluation]
    GROUP BY CAST(CreateDate AS DATE)
    UNION ALL
    SELECT 'ARCHIVE', CAST(CreateDate AS DATE),
        COUNT(*), SUM(CASE WHEN Status = 'Completed' THEN 1 ELSE 0 END), SUM(CASE WHEN Status = 'Failed' THEN 1 ELSE 0 END)
    FROM [Archive].[ReconciliationEvaluation]
    GROUP BY CAST(CreateDate AS DATE)
) ev ON ev.ReportDate = d.ReportDate AND ev.DataScope = d.DataScope
         LEFT JOIN (
    SELECT 'LIVE' AS DataScope, CAST(CreateDate AS DATE) AS ReportDate,
           COUNT(*) AS TotalOperationCount,
           SUM(CASE WHEN Status = 'Completed' THEN 1 ELSE 0 END) AS CompletedOperationCount,
           SUM(CASE WHEN Status = 'Failed' THEN 1 ELSE 0 END) AS FailedOperationCount,
           SUM(CASE WHEN Status = 'Blocked' THEN 1 ELSE 0 END) AS BlockedOperationCount,
           SUM(CASE WHEN Status = 'Planned' THEN 1 ELSE 0 END) AS PlannedOperationCount,
           SUM(CASE WHEN IsManual = 1 THEN 1 ELSE 0 END) AS ManualOperationCount
    FROM [Reconciliation].[Operation]
    GROUP BY CAST(CreateDate AS DATE)
    UNION ALL
    SELECT 'ARCHIVE', CAST(CreateDate AS DATE),
        COUNT(*), SUM(CASE WHEN Status = 'Completed' THEN 1 ELSE 0 END), SUM(CASE WHEN Status = 'Failed' THEN 1 ELSE 0 END),
        SUM(CASE WHEN Status = 'Blocked' THEN 1 ELSE 0 END), SUM(CASE WHEN Status = 'Planned' THEN 1 ELSE 0 END),
        SUM(CASE WHEN IsManual = 1 THEN 1 ELSE 0 END)
    FROM [Archive].[ReconciliationOperation]
    GROUP BY CAST(CreateDate AS DATE)
) op ON op.ReportDate = d.ReportDate AND op.DataScope = d.DataScope
         LEFT JOIN (
    SELECT 'LIVE' AS DataScope, CAST(StartedAt AS DATE) AS ReportDate,
           COUNT(*) AS TotalExecutionCount,
           SUM(CASE WHEN Status = 'Completed' THEN 1 ELSE 0 END) AS CompletedExecutionCount,
           SUM(CASE WHEN Status = 'Failed' THEN 1 ELSE 0 END) AS FailedExecutionCount,
           AVG(CASE WHEN FinishedAt IS NOT NULL THEN DATEDIFF(SECOND, StartedAt, FinishedAt) END) AS AvgExecutionDurationSeconds
    FROM [Reconciliation].[OperationExecution]
    GROUP BY CAST(StartedAt AS DATE)
    UNION ALL
    SELECT 'ARCHIVE', CAST(StartedAt AS DATE),
        COUNT(*), SUM(CASE WHEN Status = 'Completed' THEN 1 ELSE 0 END), SUM(CASE WHEN Status = 'Failed' THEN 1 ELSE 0 END),
        AVG(CASE WHEN FinishedAt IS NOT NULL THEN DATEDIFF(SECOND, StartedAt, FinishedAt) END)
    FROM [Archive].[ReconciliationOperationExecution]
    GROUP BY CAST(StartedAt AS DATE)
) ex ON ex.ReportDate = d.ReportDate AND ex.DataScope = d.DataScope
         LEFT JOIN (
    SELECT 'LIVE' AS DataScope, CAST(CreateDate AS DATE) AS ReportDate,
           SUM(CASE WHEN Decision = 'Pending' THEN 1 ELSE 0 END) AS PendingReviewCount,
           SUM(CASE WHEN Decision = 'Approved' THEN 1 ELSE 0 END) AS ApprovedReviewCount,
           SUM(CASE WHEN Decision = 'Rejected' THEN 1 ELSE 0 END) AS RejectedReviewCount
    FROM [Reconciliation].[Review]
    GROUP BY CAST(CreateDate AS DATE)
    UNION ALL
    SELECT 'ARCHIVE', CAST(CreateDate AS DATE),
        SUM(CASE WHEN Decision = 'Pending' THEN 1 ELSE 0 END), SUM(CASE WHEN Decision = 'Approved' THEN 1 ELSE 0 END),
        SUM(CASE WHEN Decision = 'Rejected' THEN 1 ELSE 0 END)
    FROM [Archive].[ReconciliationReview]
    GROUP BY CAST(CreateDate AS DATE)
) rv ON rv.ReportDate = d.ReportDate AND rv.DataScope = d.DataScope
         LEFT JOIN (
    SELECT 'LIVE' AS DataScope, CAST(CreateDate AS DATE) AS ReportDate,
           SUM(CASE WHEN AlertStatus = 'Pending' THEN 1 ELSE 0 END) AS PendingAlertCount,
           SUM(CASE WHEN AlertStatus = 'Failed' THEN 1 ELSE 0 END) AS FailedAlertCount
    FROM [Reconciliation].[Alert]
    GROUP BY CAST(CreateDate AS DATE)
    UNION ALL
    SELECT 'ARCHIVE', CAST(CreateDate AS DATE),
        SUM(CASE WHEN AlertStatus = 'Pending' THEN 1 ELSE 0 END), SUM(CASE WHEN AlertStatus = 'Failed' THEN 1 ELSE 0 END)
    FROM [Archive].[ReconciliationAlert]
    GROUP BY CAST(CreateDate AS DATE)
) al ON al.ReportDate = d.ReportDate AND al.DataScope = d.DataScope;
GO

-- B2. Açık mutabakat işleri
CREATE OR ALTER VIEW [Reporting].[VwReconOpenItems] AS
SELECT
    op.Id AS OperationId,
    op.FileLineId AS FileLineId,
    op.EvaluationId AS EvaluationId,
    op.GroupId AS GroupId,
    op.SequenceNumber AS SequenceNumber,
    op.ParentSequenceNumber AS ParentSequenceNumber,
    op.Code AS OperationCode,
    op.Branch AS Branch,
    op.IsManual AS IsManual,
    op.Status AS OperationStatus,
    op.RetryCount AS RetryCount,
    op.MaxRetries AS MaxRetryCount,
    op.NextAttemptAt AS NextAttemptAt,
    op.LeaseOwner AS LeaseOwner,
    op.LeaseExpiresAt AS LeaseExpiresAt,
    op.LastError AS LastError,
    op.CreateDate AS OperationCreatedAt,
    op.UpdateDate AS OperationUpdatedAt,
    ev.Status AS EvaluationStatus,
    ev.OperationCount AS EvaluationOperationCount,
    ROUND(CAST(DATEDIFF(SECOND, op.CreateDate, GETDATE()) AS DECIMAL(38,4)) / 3600, 1) AS AgeHours
FROM [Reconciliation].[Operation] op
    JOIN [Reconciliation].[Evaluation] ev ON ev.Id = op.EvaluationId
WHERE op.Status IN ('Planned', 'Blocked', 'Executing');
GO

-- B3. Aging dağılımı
CREATE OR ALTER VIEW [Reporting].[VwReconOpenItemAging] AS
SELECT
    BucketName,
    COUNT(*) AS ItemCount,
    SUM(CASE WHEN OperationStatus = 'Planned' THEN 1 ELSE 0 END) AS PlannedCount,
    SUM(CASE WHEN OperationStatus = 'Blocked' THEN 1 ELSE 0 END) AS BlockedCount,
    SUM(CASE WHEN OperationStatus = 'Executing' THEN 1 ELSE 0 END) AS ExecutingCount,
    SUM(CASE WHEN IsManual = 1 THEN 1 ELSE 0 END) AS ManualCount
FROM (
         SELECT
             op.Status AS OperationStatus,
             op.IsManual AS IsManual,
             CASE
                 WHEN DATEDIFF(HOUR, op.CreateDate, GETDATE()) < 1 THEN '0-1h'
                 WHEN DATEDIFF(HOUR, op.CreateDate, GETDATE()) < 4 THEN '1-4h'
                 WHEN DATEDIFF(HOUR, op.CreateDate, GETDATE()) < 24 THEN '4-24h'
                 WHEN DATEDIFF(HOUR, op.CreateDate, GETDATE()) < 72 THEN '1-3d'
                 WHEN DATEDIFF(HOUR, op.CreateDate, GETDATE()) < 168 THEN '3-7d'
                 ELSE '7d+'
                 END AS BucketName
         FROM [Reconciliation].[Operation] op
         WHERE op.Status IN ('Planned', 'Blocked', 'Executing')
     ) t
GROUP BY BucketName;
GO

-- B4. Manuel review kuyruğu
CREATE OR ALTER VIEW [Reporting].[VwReconManualReviewQueue] AS
SELECT
    rv.Id AS ReviewId,
    rv.FileLineId AS FileLineId,
    rv.GroupId AS GroupId,
    rv.EvaluationId AS EvaluationId,
    rv.OperationId AS OperationId,
    rv.ReviewerId AS ReviewerId,
    rv.Decision AS Decision,
    rv.Comment AS Comment,
    rv.DecisionAt AS DecisionAt,
    rv.ExpiresAt AS ExpiresAt,
    rv.ExpirationAction AS ExpirationAction,
    rv.ExpirationFlowAction AS ExpirationFlowAction,
    rv.CreateDate AS ReviewCreatedAt,
    op.Code AS OperationCode,
    op.Branch AS OperationBranch,
    op.Status AS OperationStatus,
    op.IsManual AS OperationIsManual,
    op.Note AS OperationNote,
    op.RetryCount AS OperationRetryCount,
    op.MaxRetries AS OperationMaxRetries,
    op.NextAttemptAt AS OperationNextAttemptAt,
    op.LeaseOwner AS OperationLeaseOwner,
    op.LeaseExpiresAt AS OperationLeaseExpiresAt,
    op.LastError AS OperationLastError,
    op.Payload AS OperationPayload,
    op.CreateDate AS OperationCreatedAt,
    op.UpdateDate AS OperationUpdatedAt,
    ev.Status AS EvaluationStatus,
    ev.Message AS EvaluationMessage,
    ev.OperationCount AS EvaluationOperationCount,
    ev.CreateDate AS EvaluationCreatedAt,
    le.LastExecutionId AS LastExecutionId,
    le.LastAttemptNumber AS LastAttemptNumber,
    le.LastExecutionStatus AS LastExecutionStatus,
    le.LastExecutionStartedAt AS LastExecutionStartedAt,
    le.LastExecutionFinishedAt AS LastExecutionFinishedAt,
    le.LastExecutionResultCode AS LastExecutionResultCode,
    le.LastExecutionResultMessage AS LastExecutionResultMessage,
    le.LastExecutionErrorCode AS LastExecutionErrorCode,
    le.LastExecutionErrorMessage AS LastExecutionErrorMessage,
    le.TotalExecutionCount AS TotalExecutionCount,
    f.FileName AS FileName,
    f.FileKey AS FileKey,
    f.SourceType AS FileSourceType,
    f.FileType AS FileType,
    f.ContentType AS ContentType,
    f.Status AS FileStatus,
    fl.LineNumber AS LineNumber,
    fl.LineType AS LineRecordType,
    fl.Status AS LineStatus,
    fl.ReconciliationStatus AS LineReconciliationStatus,
    fl.MatchedClearingLineId AS MatchedClearingLineId,
    fl.CorrelationKey AS CorrelationKey,
    fl.CorrelationValue AS CorrelationValue,
    fl.DuplicateStatus AS LineDuplicateStatus,
    fl.Message AS LineMessage,
    cd.TransactionDate AS CardTransactionDate,
    cd.TransactionTime AS CardTransactionTime,
    cd.OriginalAmount AS CardOriginalAmount,
    cd.OriginalCurrency AS CardOriginalCurrency,
    cd.SettlementAmount AS CardSettlementAmount,
    cd.BillingAmount AS CardBillingAmount,
    cd.FinancialType AS CardFinancialType,
    cd.TxnEffect AS CardTxnEffect,
    cd.ResponseCode AS CardResponseCode,
    cd.IsSuccessfulTxn AS CardIsSuccessfulTxn,
    cd.Rrn AS CardRrn,
    cd.Arn AS CardArn,
    cld.TxnDate AS ClearingTxnDate,
    cld.TxnTime AS ClearingTxnTime,
    cld.IoDate AS ClearingIoDate,
    cld.SourceAmount AS ClearingSourceAmount,
    cld.SourceCurrency AS ClearingSourceCurrency,
    cld.DestinationAmount AS ClearingDestinationAmount,
    cld.TxnType AS ClearingTxnType,
    cld.IoFlag AS ClearingIoFlag,
    cld.ControlStat AS ClearingControlStat,
    cld.Rrn AS ClearingRrn,
    cld.Arn AS ClearingArn,
    ROUND(CAST(DATEDIFF(SECOND, rv.CreateDate, GETDATE()) AS DECIMAL(38,4)) / 3600, 1) AS WaitingHours,
    CASE
        WHEN rv.ExpiresAt IS NOT NULL AND rv.ExpiresAt < GETDATE() THEN 'EXPIRED'
        WHEN rv.ExpiresAt IS NOT NULL AND rv.ExpiresAt < DATEADD(HOUR, 4, GETDATE()) THEN 'EXPIRING_SOON'
        WHEN DATEDIFF(HOUR, rv.CreateDate, GETDATE()) > 24 THEN 'OVERDUE'
        ELSE 'NORMAL'
        END AS UrgencyLevel,
    ISNULL(le.LastExecutionErrorMessage, op.LastError) AS EffectiveError
FROM [Reconciliation].[Review] rv
    JOIN [Reconciliation].[Operation] op ON op.Id = rv.OperationId
    JOIN [Reconciliation].[Evaluation] ev ON ev.Id = rv.EvaluationId
    JOIN [Ingestion].[FileLine] fl ON fl.Id = rv.FileLineId
    JOIN [Ingestion].[File] f ON f.Id = fl.FileId
    OUTER APPLY (
    SELECT TOP 1
    ex.Id AS LastExecutionId,
    ex.AttemptNumber AS LastAttemptNumber,
    ex.Status AS LastExecutionStatus,
    ex.StartedAt AS LastExecutionStartedAt,
    ex.FinishedAt AS LastExecutionFinishedAt,
    ex.ResultCode AS LastExecutionResultCode,
    ex.ResultMessage AS LastExecutionResultMessage,
    ex.ErrorCode AS LastExecutionErrorCode,
    ex.ErrorMessage AS LastExecutionErrorMessage,
    (SELECT COUNT(*) FROM [Reconciliation].[OperationExecution] WHERE OperationId = rv.OperationId AND EvaluationId = rv.EvaluationId) AS TotalExecutionCount
    FROM [Reconciliation].[OperationExecution] ex
    WHERE ex.OperationId = rv.OperationId AND ex.EvaluationId = rv.EvaluationId
    ORDER BY ex.AttemptNumber DESC
    ) le
    OUTER APPLY (
    SELECT cv.TransactionDate, cv.TransactionTime,
    cv.OriginalAmount, cv.OriginalCurrency, cv.SettlementAmount, cv.BillingAmount,
    cv.FinancialType, cv.TxnEffect, cv.ResponseCode, cv.IsSuccessfulTxn, cv.Rrn, cv.Arn
    FROM [Ingestion].[CardVisaDetail] cv WHERE cv.FileLineId = fl.Id AND f.ContentType = 'Visa' AND f.FileType = 'Card'
    UNION ALL
    SELECT cm.TransactionDate, cm.TransactionTime,
    cm.OriginalAmount, cm.OriginalCurrency, cm.SettlementAmount, cm.BillingAmount,
    cm.FinancialType, cm.TxnEffect, cm.ResponseCode, cm.IsSuccessfulTxn, cm.Rrn, cm.Arn
    FROM [Ingestion].[CardMscDetail] cm WHERE cm.FileLineId = fl.Id AND f.ContentType = 'Msc' AND f.FileType = 'Card'
    UNION ALL
    SELECT cb.TransactionDate, cb.TransactionTime,
    cb.OriginalAmount, cb.OriginalCurrency, cb.SettlementAmount, cb.BillingAmount,
    cb.FinancialType, cb.TxnEffect, cb.ResponseCode, cb.IsSuccessfulTxn, cb.Rrn, cb.Arn
    FROM [Ingestion].[CardBkmDetail] cb WHERE cb.FileLineId = fl.Id AND f.ContentType = 'Bkm' AND f.FileType = 'Card'
    ) cd
    OUTER APPLY (
    SELECT cv.TxnDate, cv.TxnTime, cv.IoDate,
    cv.SourceAmount, cv.SourceCurrency, cv.DestinationAmount,
    cv.TxnType, cv.IoFlag, cv.ControlStat, cv.Rrn, cv.Arn
    FROM [Ingestion].[ClearingVisaDetail] cv WHERE cv.FileLineId = fl.Id AND f.ContentType = 'Visa' AND f.FileType = 'Clearing'
    UNION ALL
    SELECT cm.TxnDate, cm.TxnTime, cm.IoDate,
    cm.SourceAmount, cm.SourceCurrency, cm.DestinationAmount,
    cm.TxnType, cm.IoFlag, cm.ControlStat, cm.Rrn, cm.Arn
    FROM [Ingestion].[ClearingMscDetail] cm WHERE cm.FileLineId = fl.Id AND f.ContentType = 'Msc' AND f.FileType = 'Clearing'
    UNION ALL
    SELECT cb.TxnDate, cb.TxnTime, cb.IoDate,
    cb.SourceAmount, cb.SourceCurrency, cb.DestinationAmount,
    cb.TxnType, cb.IoFlag, cb.ControlStat, cb.Rrn, cb.Arn
    FROM [Ingestion].[ClearingBkmDetail] cb WHERE cb.FileLineId = fl.Id AND f.ContentType = 'Bkm' AND f.FileType = 'Clearing'
    ) cld
WHERE rv.Decision = 'Pending';
GO

-- B5. Alert hotspot özeti
CREATE OR ALTER VIEW [Reporting].[VwReconAlertSummary] AS
SELECT
    DataScope,
    Severity,
    AlertType,
    AlertStatus,
    SUM(AlertCount) AS AlertCount,
    SUM(DistinctGroupCount) AS DistinctGroupCount,
    SUM(DistinctOperationCount) AS DistinctOperationCount,
    MIN(FirstAlertAt) AS FirstAlertAt,
    MAX(LastAlertAt) AS LastAlertAt
FROM (
         SELECT
             'LIVE' AS DataScope,
             alert.Severity,
             alert.AlertType,
             alert.AlertStatus,
             COUNT(*) AS AlertCount,
             COUNT(DISTINCT alert.GroupId) AS DistinctGroupCount,
             COUNT(DISTINCT alert.OperationId) AS DistinctOperationCount,
             MIN(alert.CreateDate) AS FirstAlertAt,
             MAX(alert.CreateDate) AS LastAlertAt
         FROM [Reconciliation].[Alert] alert
         GROUP BY alert.Severity, alert.AlertType, alert.AlertStatus
         UNION ALL
         SELECT
             'ARCHIVE',
             alert.Severity,
             alert.AlertType,
             alert.AlertStatus,
             COUNT(*),
             COUNT(DISTINCT alert.GroupId),
             COUNT(DISTINCT alert.OperationId),
             MIN(alert.CreateDate),
             MAX(alert.CreateDate)
         FROM [Archive].[ReconciliationAlert] alert
         GROUP BY alert.Severity, alert.AlertType, alert.AlertStatus
     ) src
GROUP BY DataScope, Severity, AlertType, AlertStatus;
GO

-- =====================================================================================
-- C. RECONCILIATION CONTENT + FINANCIAL
-- =====================================================================================

-- C1. LIVE card içerik özeti
CREATE OR ALTER VIEW [Reporting].[VwReconLiveCardContentDaily] AS
SELECT
    CAST(f.CreateDate AS DATE) AS ReportDate,
    'LIVE' AS DataScope,
    f.ContentType AS Network,
    fl.Status AS LineStatus,
    fl.ReconciliationStatus AS ReconciliationStatus,
    d.FinancialType AS FinancialType,
    d.TxnEffect AS TxnEffect,
    d.TxnSource AS TxnSource,
    d.TxnRegion AS TxnRegion,
    d.TerminalType AS TerminalType,
    d.ChannelCode AS ChannelCode,
    d.IsTxnSettle AS IsTxnSettle,
    d.TxnStat AS TxnStat,
    d.ResponseCode AS ResponseCode,
    d.IsSuccessfulTxn AS IsSuccessfulTxn,
    d.OriginalCurrency AS OriginalCurrency,
    COUNT(*) AS TransactionCount,
    COUNT(DISTINCT fl.FileId) AS DistinctFileCount,
    SUM(d.OriginalAmount) AS TotalCardOriginalAmount,
    SUM(d.SettlementAmount) AS TotalCardSettlementAmount,
    SUM(d.BillingAmount) AS TotalCardBillingAmount,
    AVG(d.OriginalAmount) AS AvgCardOriginalAmount,
    SUM(CASE WHEN fl.MatchedClearingLineId IS NOT NULL THEN 1 ELSE 0 END) AS MatchedCount,
    SUM(CASE WHEN fl.MatchedClearingLineId IS NULL THEN 1 ELSE 0 END) AS UnmatchedCount
FROM [Ingestion].[FileLine] fl
    JOIN [Ingestion].[File] f ON f.Id = fl.FileId AND f.FileType = 'Card'
    CROSS APPLY (
    SELECT
    cv.FinancialType, cv.TxnEffect, cv.TxnSource, cv.TxnRegion,
    cv.TerminalType, cv.ChannelCode, cv.IsTxnSettle, cv.TxnStat,
    cv.ResponseCode, cv.IsSuccessfulTxn,
    cv.OriginalCurrency, cv.OriginalAmount, cv.SettlementAmount, cv.BillingAmount
    FROM [Ingestion].[CardVisaDetail] cv WHERE cv.FileLineId = fl.Id AND f.ContentType = 'Visa'
    UNION ALL
    SELECT
    cm.FinancialType, cm.TxnEffect, cm.TxnSource, cm.TxnRegion,
    cm.TerminalType, cm.ChannelCode, cm.IsTxnSettle, cm.TxnStat,
    cm.ResponseCode, cm.IsSuccessfulTxn,
    cm.OriginalCurrency, cm.OriginalAmount, cm.SettlementAmount, cm.BillingAmount
    FROM [Ingestion].[CardMscDetail] cm WHERE cm.FileLineId = fl.Id AND f.ContentType = 'Msc'
    UNION ALL
    SELECT
    cb.FinancialType, cb.TxnEffect, cb.TxnSource, cb.TxnRegion,
    cb.TerminalType, cb.ChannelCode, cb.IsTxnSettle, cb.TxnStat,
    cb.ResponseCode, cb.IsSuccessfulTxn,
    cb.OriginalCurrency, cb.OriginalAmount, cb.SettlementAmount, cb.BillingAmount
    FROM [Ingestion].[CardBkmDetail] cb WHERE cb.FileLineId = fl.Id AND f.ContentType = 'Bkm'
    ) d
GROUP BY
    CAST(f.CreateDate AS DATE),
    f.ContentType,
    fl.Status,
    fl.ReconciliationStatus,
    d.FinancialType,
    d.TxnEffect,
    d.TxnSource,
    d.TxnRegion,
    d.TerminalType,
    d.ChannelCode,
    d.IsTxnSettle,
    d.TxnStat,
    d.ResponseCode,
    d.IsSuccessfulTxn,
    d.OriginalCurrency;
GO

-- C2. LIVE clearing içerik özeti
CREATE OR ALTER VIEW [Reporting].[VwReconLiveClearingContentDaily] AS
SELECT
    CAST(f.CreateDate AS DATE) AS ReportDate,
    'LIVE' AS DataScope,
    f.ContentType AS Network,
    fl.Status AS LineStatus,
    fl.ReconciliationStatus AS ReconciliationStatus,
    d.TxnType AS TxnType,
    d.IoFlag AS IoFlag,
    d.ControlStat AS ControlStat,
    d.SourceCurrency AS SourceCurrency,
    COUNT(*) AS TransactionCount,
    COUNT(DISTINCT fl.FileId) AS DistinctFileCount,
    SUM(d.SourceAmount) AS TotalClearingSourceAmount,
    SUM(d.DestinationAmount) AS TotalClearingDestinationAmount,
    AVG(d.SourceAmount) AS AvgClearingSourceAmount,
    SUM(CASE WHEN fl.MatchedClearingLineId IS NOT NULL THEN 1 ELSE 0 END) AS MatchedCount,
    SUM(CASE WHEN fl.MatchedClearingLineId IS NULL THEN 1 ELSE 0 END) AS UnmatchedCount
FROM [Ingestion].[FileLine] fl
    JOIN [Ingestion].[File] f ON f.Id = fl.FileId AND f.FileType = 'Clearing'
    CROSS APPLY (
    SELECT
    cv.TxnType, cv.IoFlag, cv.ControlStat,
    cv.SourceCurrency, cv.SourceAmount, cv.DestinationAmount
    FROM [Ingestion].[ClearingVisaDetail] cv WHERE cv.FileLineId = fl.Id AND f.ContentType = 'Visa'
    UNION ALL
    SELECT
    cm.TxnType, cm.IoFlag, cm.ControlStat,
    cm.SourceCurrency, cm.SourceAmount, cm.DestinationAmount
    FROM [Ingestion].[ClearingMscDetail] cm WHERE cm.FileLineId = fl.Id AND f.ContentType = 'Msc'
    UNION ALL
    SELECT
    cb.TxnType, cb.IoFlag, cb.ControlStat,
    cb.SourceCurrency, cb.SourceAmount, cb.DestinationAmount
    FROM [Ingestion].[ClearingBkmDetail] cb WHERE cb.FileLineId = fl.Id AND f.ContentType = 'Bkm'
    ) d
GROUP BY
    CAST(f.CreateDate AS DATE),
    f.ContentType,
    fl.Status,
    fl.ReconciliationStatus,
    d.TxnType,
    d.IoFlag,
    d.ControlStat,
    d.SourceCurrency;
GO

-- C3. ARCHIVE card içerik özeti
CREATE OR ALTER VIEW [Reporting].[VwReconArchiveCardContentDaily] AS
SELECT
    CAST(f.CreateDate AS DATE) AS ReportDate,
    'ARCHIVE' AS DataScope,
    f.ContentType AS Network,
    fl.Status AS LineStatus,
    fl.ReconciliationStatus AS ReconciliationStatus,
    d.FinancialType AS FinancialType,
    d.TxnEffect AS TxnEffect,
    d.TxnSource AS TxnSource,
    d.TxnRegion AS TxnRegion,
    d.TerminalType AS TerminalType,
    d.ChannelCode AS ChannelCode,
    d.IsTxnSettle AS IsTxnSettle,
    d.TxnStat AS TxnStat,
    d.ResponseCode AS ResponseCode,
    d.IsSuccessfulTxn AS IsSuccessfulTxn,
    d.OriginalCurrency AS OriginalCurrency,
    COUNT(*) AS TransactionCount,
    COUNT(DISTINCT fl.FileId) AS DistinctFileCount,
    SUM(ISNULL(d.OriginalAmount, 0)) AS TotalCardOriginalAmount,
    SUM(ISNULL(d.SettlementAmount, 0)) AS TotalCardSettlementAmount,
    SUM(ISNULL(d.BillingAmount, 0)) AS TotalCardBillingAmount,
    AVG(ISNULL(d.OriginalAmount, 0)) AS AvgCardOriginalAmount,
    SUM(CASE WHEN fl.MatchedClearingLineId IS NOT NULL THEN 1 ELSE 0 END) AS MatchedCount,
    SUM(CASE WHEN fl.MatchedClearingLineId IS NULL THEN 1 ELSE 0 END) AS UnmatchedCount
FROM [Archive].[IngestionFileLine] fl
    JOIN [Archive].[IngestionFile] f ON f.Id = fl.FileId AND f.FileType = 'Card'
    OUTER APPLY (
    SELECT
    cv.FinancialType, cv.TxnEffect, cv.TxnSource, cv.TxnRegion,
    cv.TerminalType, cv.ChannelCode, cv.IsTxnSettle, cv.TxnStat,
    cv.ResponseCode, cv.IsSuccessfulTxn,
    cv.OriginalCurrency, cv.OriginalAmount, cv.SettlementAmount, cv.BillingAmount
    FROM [Archive].[IngestionCardVisaDetail] cv
    WHERE cv.Id = fl.CardVisaDetailId AND f.ContentType = 'Visa'
    UNION ALL
    SELECT
    cm.FinancialType, cm.TxnEffect, cm.TxnSource, cm.TxnRegion,
    cm.TerminalType, cm.ChannelCode, cm.IsTxnSettle, cm.TxnStat,
    cm.ResponseCode, cm.IsSuccessfulTxn,
    cm.OriginalCurrency, cm.OriginalAmount, cm.SettlementAmount, cm.BillingAmount
    FROM [Archive].[IngestionCardMscDetail] cm
    WHERE cm.Id = fl.CardMscDetailId AND f.ContentType = 'Msc'
    UNION ALL
    SELECT
    cb.FinancialType, cb.TxnEffect, cb.TxnSource, cb.TxnRegion,
    cb.TerminalType, cb.ChannelCode, cb.IsTxnSettle, cb.TxnStat,
    cb.ResponseCode, cb.IsSuccessfulTxn,
    cb.OriginalCurrency, cb.OriginalAmount, cb.SettlementAmount, cb.BillingAmount
    FROM [Archive].[IngestionCardBkmDetail] cb
    WHERE cb.Id = fl.CardBkmDetailId AND f.ContentType = 'Bkm'
    ) d
WHERE d.FinancialType IS NOT NULL
GROUP BY
    CAST(f.CreateDate AS DATE),
    f.ContentType,
    fl.Status,
    fl.ReconciliationStatus,
    d.FinancialType,
    d.TxnEffect,
    d.TxnSource,
    d.TxnRegion,
    d.TerminalType,
    d.ChannelCode,
    d.IsTxnSettle,
    d.TxnStat,
    d.ResponseCode,
    d.IsSuccessfulTxn,
    d.OriginalCurrency;
GO

-- C4. ARCHIVE clearing içerik özeti
CREATE OR ALTER VIEW [Reporting].[VwReconArchiveClearingContentDaily] AS
SELECT
    CAST(f.CreateDate AS DATE) AS ReportDate,
    'ARCHIVE' AS DataScope,
    f.ContentType AS Network,
    fl.Status AS LineStatus,
    fl.ReconciliationStatus AS ReconciliationStatus,
    d.TxnType AS TxnType,
    d.IoFlag AS IoFlag,
    d.ControlStat AS ControlStat,
    d.SourceCurrency AS SourceCurrency,
    COUNT(*) AS TransactionCount,
    COUNT(DISTINCT fl.FileId) AS DistinctFileCount,
    SUM(ISNULL(d.SourceAmount, 0)) AS TotalClearingSourceAmount,
    SUM(ISNULL(d.DestinationAmount, 0)) AS TotalClearingDestinationAmount,
    AVG(ISNULL(d.SourceAmount, 0)) AS AvgClearingSourceAmount,
    SUM(CASE WHEN fl.MatchedClearingLineId IS NOT NULL THEN 1 ELSE 0 END) AS MatchedCount,
    SUM(CASE WHEN fl.MatchedClearingLineId IS NULL THEN 1 ELSE 0 END) AS UnmatchedCount
FROM [Archive].[IngestionFileLine] fl
    JOIN [Archive].[IngestionFile] f ON f.Id = fl.FileId AND f.FileType = 'Clearing'
    OUTER APPLY (
    SELECT
    cv.TxnType, cv.IoFlag, cv.ControlStat,
    cv.SourceCurrency, cv.SourceAmount, cv.DestinationAmount
    FROM [Archive].[IngestionClearingVisaDetail] cv
    WHERE cv.Id = fl.ClearingVisaDetailId AND f.ContentType = 'Visa'
    UNION ALL
    SELECT
    cm.TxnType, cm.IoFlag, cm.ControlStat,
    cm.SourceCurrency, cm.SourceAmount, cm.DestinationAmount
    FROM [Archive].[IngestionClearingMscDetail] cm
    WHERE cm.Id = fl.ClearingMscDetailId AND f.ContentType = 'Msc'
    UNION ALL
    SELECT
    cb.TxnType, cb.IoFlag, cb.ControlStat,
    cb.SourceCurrency, cb.SourceAmount, cb.DestinationAmount
    FROM [Archive].[IngestionClearingBkmDetail] cb
    WHERE cb.Id = fl.ClearingBkmDetailId AND f.ContentType = 'Bkm'
    ) d
WHERE d.TxnType IS NOT NULL
GROUP BY
    CAST(f.CreateDate AS DATE),
    f.ContentType,
    fl.Status,
    fl.ReconciliationStatus,
    d.TxnType,
    d.IoFlag,
    d.ControlStat,
    d.SourceCurrency;
GO

-- C5. Unified günlük reconciliation içerik özeti
CREATE OR ALTER VIEW [Reporting].[VwReconContentDaily] AS
SELECT
    src.ReportDate,
    src.DataScope,
    src.Network,
    src.Side,
    src.LineStatus,
    src.ReconciliationStatus,
    SUM(src.TransactionCount) AS TransactionCount,
    SUM(src.DistinctFileCount) AS DistinctFileCount,
    SUM(src.MatchedCount) AS MatchedCount,
    SUM(src.UnmatchedCount) AS UnmatchedCount,
    SUM(src.TotalCardOriginalAmount) AS TotalCardOriginalAmount,
    SUM(src.TotalCardSettlementAmount) AS TotalCardSettlementAmount,
    SUM(src.TotalCardBillingAmount) AS TotalCardBillingAmount,
    SUM(src.TotalClearingSourceAmount) AS TotalClearingSourceAmount,
    SUM(src.TotalClearingDestinationAmount) AS TotalClearingDestinationAmount
FROM (
         -- live card
         SELECT
             CAST(f.CreateDate AS DATE) AS ReportDate,
             'LIVE' AS DataScope,
             f.ContentType AS Network,
             'Card' AS Side,
             fl.Status AS LineStatus,
             fl.ReconciliationStatus AS ReconciliationStatus,
             COUNT(*) AS TransactionCount,
             COUNT(DISTINCT fl.FileId) AS DistinctFileCount,
             SUM(CASE WHEN fl.MatchedClearingLineId IS NOT NULL THEN 1 ELSE 0 END) AS MatchedCount,
             SUM(CASE WHEN fl.MatchedClearingLineId IS NULL THEN 1 ELSE 0 END) AS UnmatchedCount,
             SUM(d.OriginalAmount) AS TotalCardOriginalAmount,
             SUM(d.SettlementAmount) AS TotalCardSettlementAmount,
             SUM(d.BillingAmount) AS TotalCardBillingAmount,
             0 AS TotalClearingSourceAmount,
             0 AS TotalClearingDestinationAmount
         FROM [Ingestion].[FileLine] fl
             JOIN [Ingestion].[File] f ON f.Id = fl.FileId AND f.FileType = 'Card'
             CROSS APPLY (
             SELECT cv.OriginalAmount, cv.SettlementAmount, cv.BillingAmount
             FROM [Ingestion].[CardVisaDetail] cv WHERE cv.FileLineId = fl.Id AND f.ContentType = 'Visa'
             UNION ALL
             SELECT cm.OriginalAmount, cm.SettlementAmount, cm.BillingAmount
             FROM [Ingestion].[CardMscDetail] cm WHERE cm.FileLineId = fl.Id AND f.ContentType = 'Msc'
             UNION ALL
             SELECT cb.OriginalAmount, cb.SettlementAmount, cb.BillingAmount
             FROM [Ingestion].[CardBkmDetail] cb WHERE cb.FileLineId = fl.Id AND f.ContentType = 'Bkm'
             ) d
         GROUP BY CAST(f.CreateDate AS DATE), f.ContentType, fl.Status, fl.ReconciliationStatus

         UNION ALL

         -- live clearing
         SELECT
             CAST(f.CreateDate AS DATE),
             'LIVE',
             f.ContentType,
             'Clearing',
             fl.Status,
             fl.ReconciliationStatus,
             COUNT(*),
             COUNT(DISTINCT fl.FileId),
             SUM(CASE WHEN fl.MatchedClearingLineId IS NOT NULL THEN 1 ELSE 0 END),
             SUM(CASE WHEN fl.MatchedClearingLineId IS NULL THEN 1 ELSE 0 END),
             0,0,0,
             SUM(d.SourceAmount),
             SUM(d.DestinationAmount)
         FROM [Ingestion].[FileLine] fl
             JOIN [Ingestion].[File] f ON f.Id = fl.FileId AND f.FileType = 'Clearing'
             CROSS APPLY (
             SELECT cv.SourceAmount, cv.DestinationAmount
             FROM [Ingestion].[ClearingVisaDetail] cv WHERE cv.FileLineId = fl.Id AND f.ContentType = 'Visa'
             UNION ALL
             SELECT cm.SourceAmount, cm.DestinationAmount
             FROM [Ingestion].[ClearingMscDetail] cm WHERE cm.FileLineId = fl.Id AND f.ContentType = 'Msc'
             UNION ALL
             SELECT cb.SourceAmount, cb.DestinationAmount
             FROM [Ingestion].[ClearingBkmDetail] cb WHERE cb.FileLineId = fl.Id AND f.ContentType = 'Bkm'
             ) d
         GROUP BY CAST(f.CreateDate AS DATE), f.ContentType, fl.Status, fl.ReconciliationStatus

         UNION ALL

         -- archive card
         SELECT
             CAST(f.CreateDate AS DATE),
             'ARCHIVE',
             f.ContentType,
             'Card',
             fl.Status,
             fl.ReconciliationStatus,
             COUNT(*),
             COUNT(DISTINCT fl.FileId),
             SUM(CASE WHEN fl.MatchedClearingLineId IS NOT NULL THEN 1 ELSE 0 END),
             SUM(CASE WHEN fl.MatchedClearingLineId IS NULL THEN 1 ELSE 0 END),
             SUM(ISNULL(d.OriginalAmount, 0)),
             SUM(ISNULL(d.SettlementAmount, 0)),
             SUM(ISNULL(d.BillingAmount, 0)),
             0,0
         FROM [Archive].[IngestionFileLine] fl
             JOIN [Archive].[IngestionFile] f ON f.Id = fl.FileId AND f.FileType = 'Card'
             OUTER APPLY (
             SELECT cv.OriginalAmount, cv.SettlementAmount, cv.BillingAmount
             FROM [Archive].[IngestionCardVisaDetail] cv WHERE cv.Id = fl.CardVisaDetailId AND f.ContentType = 'Visa'
             UNION ALL
             SELECT cm.OriginalAmount, cm.SettlementAmount, cm.BillingAmount
             FROM [Archive].[IngestionCardMscDetail] cm WHERE cm.Id = fl.CardMscDetailId AND f.ContentType = 'Msc'
             UNION ALL
             SELECT cb.OriginalAmount, cb.SettlementAmount, cb.BillingAmount
             FROM [Archive].[IngestionCardBkmDetail] cb WHERE cb.Id = fl.CardBkmDetailId AND f.ContentType = 'Bkm'
             ) d
         WHERE d.OriginalAmount IS NOT NULL
         GROUP BY CAST(f.CreateDate AS DATE), f.ContentType, fl.Status, fl.ReconciliationStatus

         UNION ALL

         -- archive clearing
         SELECT
             CAST(f.CreateDate AS DATE),
             'ARCHIVE',
             f.ContentType,
             'Clearing',
             fl.Status,
             fl.ReconciliationStatus,
             COUNT(*),
             COUNT(DISTINCT fl.FileId),
             SUM(CASE WHEN fl.MatchedClearingLineId IS NOT NULL THEN 1 ELSE 0 END),
             SUM(CASE WHEN fl.MatchedClearingLineId IS NULL THEN 1 ELSE 0 END),
             0,0,0,
             SUM(ISNULL(d.SourceAmount, 0)),
             SUM(ISNULL(d.DestinationAmount, 0))
         FROM [Archive].[IngestionFileLine] fl
             JOIN [Archive].[IngestionFile] f ON f.Id = fl.FileId AND f.FileType = 'Clearing'
             OUTER APPLY (
             SELECT cv.SourceAmount, cv.DestinationAmount
             FROM [Archive].[IngestionClearingVisaDetail] cv WHERE cv.Id = fl.ClearingVisaDetailId AND f.ContentType = 'Visa'
             UNION ALL
             SELECT cm.SourceAmount, cm.DestinationAmount
             FROM [Archive].[IngestionClearingMscDetail] cm WHERE cm.Id = fl.ClearingMscDetailId AND f.ContentType = 'Msc'
             UNION ALL
             SELECT cb.SourceAmount, cb.DestinationAmount
             FROM [Archive].[IngestionClearingBkmDetail] cb WHERE cb.Id = fl.ClearingBkmDetailId AND f.ContentType = 'Bkm'
             ) d
         WHERE d.SourceAmount IS NOT NULL
         GROUP BY CAST(f.CreateDate AS DATE), f.ContentType, fl.Status, fl.ReconciliationStatus
     ) src
GROUP BY src.ReportDate, src.DataScope, src.Network, src.Side, src.LineStatus, src.ReconciliationStatus;
GO

-- C6. Clearing control_stat analizi
CREATE OR ALTER VIEW [Reporting].[VwReconClearingControlstatAnalysis] AS
SELECT
    x.DataScope,
    x.Network,
    x.LineStatus,
    x.ControlStat,
    x.IoFlag,
    SUM(x.TransactionCount) AS TransactionCount,
    SUM(x.TotalClearingSourceAmount) AS TotalClearingSourceAmount,
    SUM(x.MatchedCount) AS MatchedCount,
    SUM(x.UnmatchedCount) AS UnmatchedCount,
    CASE WHEN SUM(x.TransactionCount) > 0 THEN ROUND(CAST(SUM(x.UnmatchedCount) AS DECIMAL(38,4)) / SUM(x.TransactionCount) * 100, 2) ELSE 0 END AS UnmatchedRatePct
FROM (
         -- LIVE
         SELECT
             'LIVE' AS DataScope,
             f.ContentType AS Network,
             fl.Status AS LineStatus,
             d.ControlStat,
             d.IoFlag,
             COUNT(*) AS TransactionCount,
             SUM(d.SourceAmount) AS TotalClearingSourceAmount,
             SUM(CASE WHEN fl.MatchedClearingLineId IS NOT NULL THEN 1 ELSE 0 END) AS MatchedCount,
             SUM(CASE WHEN fl.MatchedClearingLineId IS NULL THEN 1 ELSE 0 END) AS UnmatchedCount
         FROM [Ingestion].[FileLine] fl
             JOIN [Ingestion].[File] f ON f.Id = fl.FileId AND f.FileType = 'Clearing'
             CROSS APPLY (
             SELECT cv.ControlStat, cv.IoFlag, cv.SourceAmount
             FROM [Ingestion].[ClearingVisaDetail] cv WHERE cv.FileLineId = fl.Id AND f.ContentType = 'Visa'
             UNION ALL
             SELECT cm.ControlStat, cm.IoFlag, cm.SourceAmount
             FROM [Ingestion].[ClearingMscDetail] cm WHERE cm.FileLineId = fl.Id AND f.ContentType = 'Msc'
             UNION ALL
             SELECT cb.ControlStat, cb.IoFlag, cb.SourceAmount
             FROM [Ingestion].[ClearingBkmDetail] cb WHERE cb.FileLineId = fl.Id AND f.ContentType = 'Bkm'
             ) d
         GROUP BY f.ContentType, fl.Status, d.ControlStat, d.IoFlag

         UNION ALL

         -- ARCHIVE
         SELECT
             'ARCHIVE',
             f.ContentType,
             fl.Status,
             d.ControlStat,
             d.IoFlag,
             COUNT(*),
             SUM(ISNULL(d.SourceAmount, 0)),
             SUM(CASE WHEN fl.MatchedClearingLineId IS NOT NULL THEN 1 ELSE 0 END),
             SUM(CASE WHEN fl.MatchedClearingLineId IS NULL THEN 1 ELSE 0 END)
         FROM [Archive].[IngestionFileLine] fl
             JOIN [Archive].[IngestionFile] f ON f.Id = fl.FileId AND f.FileType = 'Clearing'
             OUTER APPLY (
             SELECT cv.ControlStat, cv.IoFlag, cv.SourceAmount
             FROM [Archive].[IngestionClearingVisaDetail] cv WHERE cv.Id = fl.ClearingVisaDetailId AND f.ContentType = 'Visa'
             UNION ALL
             SELECT cm.ControlStat, cm.IoFlag, cm.SourceAmount
             FROM [Archive].[IngestionClearingMscDetail] cm WHERE cm.Id = fl.ClearingMscDetailId AND f.ContentType = 'Msc'
             UNION ALL
             SELECT cb.ControlStat, cb.IoFlag, cb.SourceAmount
             FROM [Archive].[IngestionClearingBkmDetail] cb WHERE cb.Id = fl.ClearingBkmDetailId AND f.ContentType = 'Bkm'
             ) d
         WHERE d.ControlStat IS NOT NULL
         GROUP BY f.ContentType, fl.Status, d.ControlStat, d.IoFlag
     ) x
GROUP BY x.DataScope, x.Network, x.LineStatus, x.ControlStat, x.IoFlag;
GO

-- C7. Finansal özet
CREATE OR ALTER VIEW [Reporting].[VwReconFinancialSummary] AS
SELECT
    x.DataScope,
    x.Network,
    x.LineStatus,
    x.FinancialType,
    x.TxnEffect,
    x.OriginalCurrency,
    SUM(x.TransactionCount) AS TransactionCount,
    SUM(x.TotalCardOriginalAmount) AS TotalCardOriginalAmount,
    SUM(x.TotalCardSettlementAmount) AS TotalCardSettlementAmount,
    SUM(x.TotalCardBillingAmount) AS TotalCardBillingAmount,
    SUM(x.SettledCount) AS SettledCount,
    SUM(x.UnsettledCount) AS UnsettledCount,
    SUM(x.DebitAmount) AS DebitAmount,
    SUM(x.CreditAmount) AS CreditAmount,
    SUM(x.MatchedCount) AS MatchedCount,
    SUM(x.UnmatchedCount) AS UnmatchedCount
FROM (
         -- LIVE
         SELECT
             'LIVE' AS DataScope,
             f.ContentType AS Network,
             fl.Status AS LineStatus,
             d.FinancialType,
             d.TxnEffect,
             d.OriginalCurrency,
             COUNT(*) AS TransactionCount,
             SUM(d.OriginalAmount) AS TotalCardOriginalAmount,
             SUM(d.SettlementAmount) AS TotalCardSettlementAmount,
             SUM(d.BillingAmount) AS TotalCardBillingAmount,
             SUM(CASE WHEN d.IsTxnSettle = 'Y' THEN 1 ELSE 0 END) AS SettledCount,
             SUM(CASE WHEN d.IsTxnSettle = 'N' THEN 1 ELSE 0 END) AS UnsettledCount,
             SUM(CASE WHEN d.TxnEffect = 'D' THEN d.OriginalAmount ELSE 0 END) AS DebitAmount,
             SUM(CASE WHEN d.TxnEffect = 'C' THEN d.OriginalAmount ELSE 0 END) AS CreditAmount,
             SUM(CASE WHEN fl.MatchedClearingLineId IS NOT NULL THEN 1 ELSE 0 END) AS MatchedCount,
             SUM(CASE WHEN fl.MatchedClearingLineId IS NULL THEN 1 ELSE 0 END) AS UnmatchedCount
         FROM [Ingestion].[FileLine] fl
             JOIN [Ingestion].[File] f ON f.Id = fl.FileId AND f.FileType = 'Card'
             CROSS APPLY (
             SELECT cv.FinancialType, cv.TxnEffect, cv.IsTxnSettle, cv.OriginalCurrency, cv.OriginalAmount, cv.SettlementAmount, cv.BillingAmount
             FROM [Ingestion].[CardVisaDetail] cv WHERE cv.FileLineId = fl.Id AND f.ContentType = 'Visa'
             UNION ALL
             SELECT cm.FinancialType, cm.TxnEffect, cm.IsTxnSettle, cm.OriginalCurrency, cm.OriginalAmount, cm.SettlementAmount, cm.BillingAmount
             FROM [Ingestion].[CardMscDetail] cm WHERE cm.FileLineId = fl.Id AND f.ContentType = 'Msc'
             UNION ALL
             SELECT cb.FinancialType, cb.TxnEffect, cb.IsTxnSettle, cb.OriginalCurrency, cb.OriginalAmount, cb.SettlementAmount, cb.BillingAmount
             FROM [Ingestion].[CardBkmDetail] cb WHERE cb.FileLineId = fl.Id AND f.ContentType = 'Bkm'
             ) d
         GROUP BY f.ContentType, fl.Status, d.FinancialType, d.TxnEffect, d.OriginalCurrency

         UNION ALL

         -- ARCHIVE
         SELECT
             'ARCHIVE',
             f.ContentType,
             fl.Status,
             d.FinancialType,
             d.TxnEffect,
             d.OriginalCurrency,
             COUNT(*),
             SUM(ISNULL(d.OriginalAmount, 0)),
             SUM(ISNULL(d.SettlementAmount, 0)),
             SUM(ISNULL(d.BillingAmount, 0)),
             SUM(CASE WHEN d.IsTxnSettle = 'Y' THEN 1 ELSE 0 END),
             SUM(CASE WHEN d.IsTxnSettle = 'N' THEN 1 ELSE 0 END),
             SUM(CASE WHEN d.TxnEffect = 'D' THEN ISNULL(d.OriginalAmount, 0) ELSE 0 END),
             SUM(CASE WHEN d.TxnEffect = 'C' THEN ISNULL(d.OriginalAmount, 0) ELSE 0 END),
             SUM(CASE WHEN fl.MatchedClearingLineId IS NOT NULL THEN 1 ELSE 0 END),
             SUM(CASE WHEN fl.MatchedClearingLineId IS NULL THEN 1 ELSE 0 END)
         FROM [Archive].[IngestionFileLine] fl
             JOIN [Archive].[IngestionFile] f ON f.Id = fl.FileId AND f.FileType = 'Card'
             OUTER APPLY (
             SELECT cv.FinancialType, cv.TxnEffect, cv.IsTxnSettle, cv.OriginalCurrency, cv.OriginalAmount, cv.SettlementAmount, cv.BillingAmount
             FROM [Archive].[IngestionCardVisaDetail] cv WHERE cv.Id = fl.CardVisaDetailId AND f.ContentType = 'Visa'
             UNION ALL
             SELECT cm.FinancialType, cm.TxnEffect, cm.IsTxnSettle, cm.OriginalCurrency, cm.OriginalAmount, cm.SettlementAmount, cm.BillingAmount
             FROM [Archive].[IngestionCardMscDetail] cm WHERE cm.Id = fl.CardMscDetailId AND f.ContentType = 'Msc'
             UNION ALL
             SELECT cb.FinancialType, cb.TxnEffect, cb.IsTxnSettle, cb.OriginalCurrency, cb.OriginalAmount, cb.SettlementAmount, cb.BillingAmount
             FROM [Archive].[IngestionCardBkmDetail] cb WHERE cb.Id = fl.CardBkmDetailId AND f.ContentType = 'Bkm'
             ) d
         WHERE d.FinancialType IS NOT NULL
         GROUP BY f.ContentType, fl.Status, d.FinancialType, d.TxnEffect, d.OriginalCurrency
     ) x
GROUP BY x.DataScope, x.Network, x.LineStatus, x.FinancialType, x.TxnEffect, x.OriginalCurrency;
GO

-- C8. Response / txn status analizi
CREATE OR ALTER VIEW [Reporting].[VwReconResponseStatusAnalysis] AS
SELECT
    x.DataScope,
    x.Network,
    x.LineStatus,
    x.ResponseCode,
    x.TxnStat,
    x.IsSuccessfulTxn,
    x.IsTxnSettle,
    x.ReconciliationStatus,
    SUM(x.TransactionCount) AS TransactionCount,
    SUM(x.TotalCardOriginalAmount) AS TotalCardOriginalAmount,
    SUM(x.MatchedCount) AS MatchedCount,
    SUM(x.UnmatchedCount) AS UnmatchedCount
FROM (
         -- LIVE
         SELECT
             'LIVE' AS DataScope,
             f.ContentType AS Network,
             fl.Status AS LineStatus,
             d.ResponseCode,
             d.TxnStat,
             d.IsSuccessfulTxn,
             d.IsTxnSettle,
             fl.ReconciliationStatus,
             COUNT(*) AS TransactionCount,
             SUM(d.OriginalAmount) AS TotalCardOriginalAmount,
             SUM(CASE WHEN fl.MatchedClearingLineId IS NOT NULL THEN 1 ELSE 0 END) AS MatchedCount,
             SUM(CASE WHEN fl.MatchedClearingLineId IS NULL THEN 1 ELSE 0 END) AS UnmatchedCount
         FROM [Ingestion].[FileLine] fl
             JOIN [Ingestion].[File] f ON f.Id = fl.FileId AND f.FileType = 'Card'
             CROSS APPLY (
             SELECT cv.ResponseCode, cv.TxnStat, cv.IsSuccessfulTxn, cv.IsTxnSettle, cv.OriginalAmount
             FROM [Ingestion].[CardVisaDetail] cv WHERE cv.FileLineId = fl.Id AND f.ContentType = 'Visa'
             UNION ALL
             SELECT cm.ResponseCode, cm.TxnStat, cm.IsSuccessfulTxn, cm.IsTxnSettle, cm.OriginalAmount
             FROM [Ingestion].[CardMscDetail] cm WHERE cm.FileLineId = fl.Id AND f.ContentType = 'Msc'
             UNION ALL
             SELECT cb.ResponseCode, cb.TxnStat, cb.IsSuccessfulTxn, cb.IsTxnSettle, cb.OriginalAmount
             FROM [Ingestion].[CardBkmDetail] cb WHERE cb.FileLineId = fl.Id AND f.ContentType = 'Bkm'
             ) d
         GROUP BY f.ContentType, fl.Status, d.ResponseCode, d.TxnStat, d.IsSuccessfulTxn, d.IsTxnSettle, fl.ReconciliationStatus

         UNION ALL

         -- ARCHIVE
         SELECT
             'ARCHIVE',
             f.ContentType,
             fl.Status,
             d.ResponseCode,
             d.TxnStat,
             d.IsSuccessfulTxn,
             d.IsTxnSettle,
             fl.ReconciliationStatus,
             COUNT(*),
             SUM(ISNULL(d.OriginalAmount, 0)),
             SUM(CASE WHEN fl.MatchedClearingLineId IS NOT NULL THEN 1 ELSE 0 END),
             SUM(CASE WHEN fl.MatchedClearingLineId IS NULL THEN 1 ELSE 0 END)
         FROM [Archive].[IngestionFileLine] fl
             JOIN [Archive].[IngestionFile] f ON f.Id = fl.FileId AND f.FileType = 'Card'
             OUTER APPLY (
             SELECT cv.ResponseCode, cv.TxnStat, cv.IsSuccessfulTxn, cv.IsTxnSettle, cv.OriginalAmount
             FROM [Archive].[IngestionCardVisaDetail] cv WHERE cv.Id = fl.CardVisaDetailId AND f.ContentType = 'Visa'
             UNION ALL
             SELECT cm.ResponseCode, cm.TxnStat, cm.IsSuccessfulTxn, cm.IsTxnSettle, cm.OriginalAmount
             FROM [Archive].[IngestionCardMscDetail] cm WHERE cm.Id = fl.CardMscDetailId AND f.ContentType = 'Msc'
             UNION ALL
             SELECT cb.ResponseCode, cb.TxnStat, cb.IsSuccessfulTxn, cb.IsTxnSettle, cb.OriginalAmount
             FROM [Archive].[IngestionCardBkmDetail] cb WHERE cb.Id = fl.CardBkmDetailId AND f.ContentType = 'Bkm'
             ) d
         WHERE d.ResponseCode IS NOT NULL
         GROUP BY f.ContentType, fl.Status, d.ResponseCode, d.TxnStat, d.IsSuccessfulTxn, d.IsTxnSettle, fl.ReconciliationStatus
     ) x
GROUP BY x.DataScope, x.Network, x.LineStatus, x.ResponseCode, x.TxnStat, x.IsSuccessfulTxn, x.IsTxnSettle, x.ReconciliationStatus;
GO

-- =====================================================================================
-- D. ARCHIVE
-- =====================================================================================

-- D1. Archive run overview
CREATE OR ALTER VIEW [Reporting].[VwArchiveRunOverview] AS
SELECT
    log.Id AS ArchiveLogId,
    log.IngestionFileId AS IngestionFileId,
    ISNULL(lf.FileName, af.FileName) AS FileName,
    ISNULL(lf.FileType, af.FileType) AS FileType,
    ISNULL(lf.ContentType, af.ContentType) AS ContentType,
    log.Status AS ArchiveStatus,
    log.Message AS ArchiveMessage,
    log.FailureReasonsJson AS FailureReasonsJson,
    log.FilterJson AS FilterJson,
    log.CreateDate AS ArchiveStartedAt,
    log.UpdateDate AS ArchiveUpdatedAt,
    DATEDIFF(SECOND, log.CreateDate, ISNULL(log.UpdateDate, log.CreateDate)) AS ArchiveDurationSeconds
FROM [Archive].[ArchiveLog] log
    LEFT JOIN [Ingestion].[File] lf ON lf.Id = log.IngestionFileId
    LEFT JOIN [Archive].[IngestionFile] af ON af.Id = log.IngestionFileId;
GO

-- D2. Archive uygunluk görünümü
CREATE OR ALTER VIEW [Reporting].[VwArchiveEligibility] AS
SELECT
    f.Id AS FileId,
    f.FileName AS FileName,
    f.FileType AS FileType,
    f.ContentType AS ContentType,
    f.Status AS FileStatus,
    f.IsArchived AS IsArchived,
    f.CreateDate AS FileCreatedAt,
    ROUND(CAST(DATEDIFF(SECOND, f.CreateDate, GETDATE()) AS DECIMAL(38,4)) / 86400, 1) AS AgeDays,
    ISNULL(x.TotalReconLineCount, 0) AS TotalReconLineCount,
    ISNULL(x.ReconSuccessLineCount, 0) AS ReconSuccessLineCount,
    ISNULL(x.ReconOpenLineCount, 0) AS ReconOpenLineCount,
    CASE
        WHEN af.Id IS NOT NULL THEN 'ALREADY_ARCHIVED'
        WHEN f.Status <> 'Success' THEN 'FILE_NOT_COMPLETE'
        WHEN ISNULL(x.ReconOpenLineCount, 0) > 0 THEN 'RECON_PENDING'
        ELSE 'ELIGIBLE'
        END AS ArchiveEligibilityStatus
FROM [Ingestion].[File] f
    LEFT JOIN [Archive].[IngestionFile] af ON af.Id = f.Id
    OUTER APPLY (
    SELECT
    SUM(CASE WHEN line.ReconciliationStatus IS NOT NULL THEN 1 ELSE 0 END) AS TotalReconLineCount,
    SUM(CASE WHEN line.ReconciliationStatus = 'Success' THEN 1 ELSE 0 END) AS ReconSuccessLineCount,
    SUM(CASE WHEN line.ReconciliationStatus IN ('Ready', 'Processing', 'Failed') THEN 1 ELSE 0 END) AS ReconOpenLineCount
    FROM [Ingestion].[FileLine] line
    WHERE line.FileId = f.Id
    ) x;
GO

-- D3. Archive backlog trend
CREATE OR ALTER VIEW [Reporting].[VwArchiveBacklogTrend] AS
SELECT
    CAST(log.CreateDate AS DATE) AS ReportDate,
    COUNT(*) AS ArchiveRunCount,
    SUM(CASE WHEN log.Status = 'Success' THEN 1 ELSE 0 END) AS SuccessRunCount,
    SUM(CASE WHEN log.Status = 'Failed' THEN 1 ELSE 0 END) AS FailedRunCount,
    SUM(CASE WHEN log.Status NOT IN ('Success', 'Failed') THEN 1 ELSE 0 END) AS OtherRunCount
FROM [Archive].[ArchiveLog] log
GROUP BY CAST(log.CreateDate AS DATE);
GO

-- D4. Retention snapshot
CREATE OR ALTER VIEW [Reporting].[VwArchiveRetentionSnapshot] AS
SELECT
    (SELECT COUNT(*) FROM [Ingestion].[File] f WHERE NOT EXISTS (SELECT 1 FROM [Archive].[IngestionFile] af WHERE af.Id = f.Id)) AS ActiveFileCount,
                                                   (SELECT COUNT(*) FROM [Archive].[IngestionFile]) AS ArchivedMarkedFileCount,
    (SELECT COUNT(*) FROM [Archive].[IngestionFile]) AS ArchiveTableFileCount,
    (SELECT COUNT(*) FROM [Archive].[IngestionFileLine]) AS ArchiveTableFileLineCount,
    (SELECT COUNT(*) FROM [Archive].[ReconciliationEvaluation]) AS ArchiveTableEvaluationCount,
    (SELECT COUNT(*) FROM [Archive].[ReconciliationOperation]) AS ArchiveTableOperationCount,
    (SELECT COUNT(*) FROM [Archive].[ReconciliationReview]) AS ArchiveTableReviewCount,
    (SELECT COUNT(*) FROM [Archive].[ReconciliationAlert]) AS ArchiveTableAlertCount,
    (SELECT COUNT(*) FROM [Archive].[ReconciliationOperationExecution]) AS ArchiveTableExecutionCount,
    (SELECT MIN(f.CreateDate) FROM [Ingestion].[File] f WHERE NOT EXISTS (SELECT 1 FROM [Archive].[IngestionFile] af WHERE af.Id = f.Id) AND f.Status = 'Success') AS OldestUnarchivedFileDate;
GO

-- =====================================================================================
-- E. ADVANCED RECONCILIATION REPORTS
-- =====================================================================================

-- E1. Dosya bazlı mutabakat özeti
CREATE OR ALTER VIEW [Reporting].[VwFileReconSummary] AS
SELECT * FROM (
                  SELECT
                      f.Id AS FileId, f.FileName AS FileName, f.FileType AS FileType, f.ContentType AS ContentType, f.Status AS FileStatus,
                      f.CreateDate AS FileCreatedAt, 'LIVE' AS DataScope,
                      ISNULL(fl.TotalLineCount, 0) AS TotalLineCount,
                      ISNULL(fl.MatchedLineCount, 0) AS MatchedLineCount,
                      ISNULL(fl.UnmatchedLineCount, 0) AS UnmatchedLineCount,
                      CASE WHEN ISNULL(fl.TotalLineCount, 0) > 0 THEN ROUND(CAST(fl.MatchedLineCount AS DECIMAL(38,4)) / fl.TotalLineCount * 100, 2) ELSE 0 END AS MatchRatePct,
                      ISNULL(fl.TotalOriginalAmount, 0) AS TotalOriginalAmount,
                      ISNULL(fl.MatchedAmount, 0) AS MatchedAmount,
                      ISNULL(fl.UnmatchedAmount, 0) AS UnmatchedAmount,
                      ISNULL(fl.TotalSettlementAmount, 0) AS TotalSettlementAmount,
                      ISNULL(fl.ReconReadyCount, 0) AS ReconReadyCount,
                      ISNULL(fl.ReconSuccessCount, 0) AS ReconSuccessCount,
                      ISNULL(fl.ReconFailedCount, 0) AS ReconFailedCount,
                      ISNULL(fl.ReconNotApplicableCount, 0) AS ReconNotApplicableCount
                  FROM [Ingestion].[File] f
                      OUTER APPLY (
                      SELECT
                      COUNT(*) AS TotalLineCount,
                      SUM(CASE WHEN line.MatchedClearingLineId IS NOT NULL THEN 1 ELSE 0 END) AS MatchedLineCount,
                      SUM(CASE WHEN line.MatchedClearingLineId IS NULL THEN 1 ELSE 0 END) AS UnmatchedLineCount,
                      SUM(ISNULL(d.Amt, 0)) AS TotalOriginalAmount,
                      SUM(CASE WHEN line.MatchedClearingLineId IS NOT NULL THEN ISNULL(d.Amt, 0) ELSE 0 END) AS MatchedAmount,
                      SUM(CASE WHEN line.MatchedClearingLineId IS NULL THEN ISNULL(d.Amt, 0) ELSE 0 END) AS UnmatchedAmount,
                      SUM(ISNULL(d.Sett, 0)) AS TotalSettlementAmount,
                      SUM(CASE WHEN line.ReconciliationStatus = 'Ready' THEN 1 ELSE 0 END) AS ReconReadyCount,
                      SUM(CASE WHEN line.ReconciliationStatus = 'Success' THEN 1 ELSE 0 END) AS ReconSuccessCount,
                      SUM(CASE WHEN line.ReconciliationStatus = 'Failed' THEN 1 ELSE 0 END) AS ReconFailedCount,
                      SUM(CASE WHEN line.ReconciliationStatus IS NULL THEN 1 ELSE 0 END) AS ReconNotApplicableCount
                      FROM [Ingestion].[FileLine] line
                      OUTER APPLY (
                      SELECT cv.OriginalAmount AS Amt, cv.SettlementAmount AS Sett FROM [Ingestion].[CardVisaDetail] cv WHERE cv.FileLineId = line.Id AND f.ContentType = 'Visa' AND f.FileType = 'Card'
                      UNION ALL SELECT cm.OriginalAmount, cm.SettlementAmount FROM [Ingestion].[CardMscDetail] cm WHERE cm.FileLineId = line.Id AND f.ContentType = 'Msc' AND f.FileType = 'Card'
                      UNION ALL SELECT cb.OriginalAmount, cb.SettlementAmount FROM [Ingestion].[CardBkmDetail] cb WHERE cb.FileLineId = line.Id AND f.ContentType = 'Bkm' AND f.FileType = 'Card'
                      UNION ALL SELECT cv.SourceAmount, 0 FROM [Ingestion].[ClearingVisaDetail] cv WHERE cv.FileLineId = line.Id AND f.ContentType = 'Visa' AND f.FileType = 'Clearing'
                      UNION ALL SELECT cm.SourceAmount, 0 FROM [Ingestion].[ClearingMscDetail] cm WHERE cm.FileLineId = line.Id AND f.ContentType = 'Msc' AND f.FileType = 'Clearing'
                      UNION ALL SELECT cb.SourceAmount, 0 FROM [Ingestion].[ClearingBkmDetail] cb WHERE cb.FileLineId = line.Id AND f.ContentType = 'Bkm' AND f.FileType = 'Clearing'
                      ) d
                      WHERE line.FileId = f.Id
                      ) fl

                  UNION ALL

                  SELECT
                      f.Id, f.FileName, f.FileType, f.ContentType, f.Status, f.CreateDate, 'ARCHIVE',
                      ISNULL(fl.TotalLineCount, 0), ISNULL(fl.MatchedLineCount, 0), ISNULL(fl.UnmatchedLineCount, 0),
                      CASE WHEN ISNULL(fl.TotalLineCount, 0) > 0 THEN ROUND(CAST(fl.MatchedLineCount AS DECIMAL(38,4)) / fl.TotalLineCount * 100, 2) ELSE 0 END,
                      ISNULL(fl.TotalOriginalAmount, 0), ISNULL(fl.MatchedAmount, 0), ISNULL(fl.UnmatchedAmount, 0),
                      ISNULL(fl.TotalSettlementAmount, 0),
                      ISNULL(fl.ReconReadyCount, 0), ISNULL(fl.ReconSuccessCount, 0), ISNULL(fl.ReconFailedCount, 0), ISNULL(fl.ReconNotApplicableCount, 0)
                  FROM [Archive].[IngestionFile] f
                      OUTER APPLY (
                      SELECT
                      COUNT(*) AS TotalLineCount,
                      SUM(CASE WHEN line.MatchedClearingLineId IS NOT NULL THEN 1 ELSE 0 END) AS MatchedLineCount,
                      SUM(CASE WHEN line.MatchedClearingLineId IS NULL THEN 1 ELSE 0 END) AS UnmatchedLineCount,
                      SUM(ISNULL(d.Amt, 0)) AS TotalOriginalAmount,
                      SUM(CASE WHEN line.MatchedClearingLineId IS NOT NULL THEN ISNULL(d.Amt, 0) ELSE 0 END) AS MatchedAmount,
                      SUM(CASE WHEN line.MatchedClearingLineId IS NULL THEN ISNULL(d.Amt, 0) ELSE 0 END) AS UnmatchedAmount,
                      SUM(ISNULL(d.Sett, 0)) AS TotalSettlementAmount,
                      SUM(CASE WHEN line.ReconciliationStatus = 'Ready' THEN 1 ELSE 0 END) AS ReconReadyCount,
                      SUM(CASE WHEN line.ReconciliationStatus = 'Success' THEN 1 ELSE 0 END) AS ReconSuccessCount,
                      SUM(CASE WHEN line.ReconciliationStatus = 'Failed' THEN 1 ELSE 0 END) AS ReconFailedCount,
                      SUM(CASE WHEN line.ReconciliationStatus IS NULL THEN 1 ELSE 0 END) AS ReconNotApplicableCount
                      FROM [Archive].[IngestionFileLine] line
                      OUTER APPLY (
                      SELECT cv.OriginalAmount AS Amt, cv.SettlementAmount AS Sett FROM [Archive].[IngestionCardVisaDetail] cv WHERE cv.Id = line.CardVisaDetailId AND f.ContentType = 'Visa' AND f.FileType = 'Card'
                      UNION ALL SELECT cm.OriginalAmount, cm.SettlementAmount FROM [Archive].[IngestionCardMscDetail] cm WHERE cm.Id = line.CardMscDetailId AND f.ContentType = 'Msc' AND f.FileType = 'Card'
                      UNION ALL SELECT cb.OriginalAmount, cb.SettlementAmount FROM [Archive].[IngestionCardBkmDetail] cb WHERE cb.Id = line.CardBkmDetailId AND f.ContentType = 'Bkm' AND f.FileType = 'Card'
                      UNION ALL SELECT cv.SourceAmount, 0 FROM [Archive].[IngestionClearingVisaDetail] cv WHERE cv.Id = line.ClearingVisaDetailId AND f.ContentType = 'Visa' AND f.FileType = 'Clearing'
                      UNION ALL SELECT cm.SourceAmount, 0 FROM [Archive].[IngestionClearingMscDetail] cm WHERE cm.Id = line.ClearingMscDetailId AND f.ContentType = 'Msc' AND f.FileType = 'Clearing'
                      UNION ALL SELECT cb.SourceAmount, 0 FROM [Archive].[IngestionClearingBkmDetail] cb WHERE cb.Id = line.ClearingBkmDetailId AND f.ContentType = 'Bkm' AND f.FileType = 'Clearing'
                      ) d
                      WHERE line.FileId = f.Id
                      ) fl
              ) combined;
GO

-- E2. Günlük eşleşme oranı trendi
CREATE OR ALTER VIEW [Reporting].[VwReconMatchRateTrend] AS
SELECT
    src.ReportDate, src.DataScope, src.Network, src.Side,
    SUM(src.TotalLineCount) AS TotalLineCount,
    SUM(src.MatchedCount) AS MatchedCount,
    SUM(src.UnmatchedCount) AS UnmatchedCount,
    CASE WHEN SUM(src.TotalLineCount) > 0 THEN ROUND(CAST(SUM(src.MatchedCount) AS DECIMAL(38,4)) / SUM(src.TotalLineCount) * 100, 2) ELSE 0 END AS MatchRatePct,
    SUM(src.TotalAmount) AS TotalAmount,
    SUM(src.MatchedAmount) AS MatchedAmount,
    SUM(src.UnmatchedAmount) AS UnmatchedAmount
FROM (
         SELECT CAST(f.CreateDate AS DATE) AS ReportDate, 'LIVE' AS DataScope, f.ContentType AS Network, 'Card' AS Side,
                COUNT(*) AS TotalLineCount,
                SUM(CASE WHEN fl.MatchedClearingLineId IS NOT NULL THEN 1 ELSE 0 END) AS MatchedCount,
                SUM(CASE WHEN fl.MatchedClearingLineId IS NULL THEN 1 ELSE 0 END) AS UnmatchedCount,
                SUM(d.OriginalAmount) AS TotalAmount,
                SUM(CASE WHEN fl.MatchedClearingLineId IS NOT NULL THEN d.OriginalAmount ELSE 0 END) AS MatchedAmount,
                SUM(CASE WHEN fl.MatchedClearingLineId IS NULL THEN d.OriginalAmount ELSE 0 END) AS UnmatchedAmount
         FROM [Ingestion].[FileLine] fl JOIN [Ingestion].[File] f ON f.Id = fl.FileId AND f.FileType = 'Card'
             CROSS APPLY (
             SELECT cv.OriginalAmount FROM [Ingestion].[CardVisaDetail] cv WHERE cv.FileLineId = fl.Id AND f.ContentType = 'Visa'
             UNION ALL SELECT cm.OriginalAmount FROM [Ingestion].[CardMscDetail] cm WHERE cm.FileLineId = fl.Id AND f.ContentType = 'Msc'
             UNION ALL SELECT cb.OriginalAmount FROM [Ingestion].[CardBkmDetail] cb WHERE cb.FileLineId = fl.Id AND f.ContentType = 'Bkm'
             ) d
         GROUP BY CAST(f.CreateDate AS DATE), f.ContentType
         UNION ALL
         SELECT CAST(f.CreateDate AS DATE), 'LIVE', f.ContentType, 'Clearing',
             COUNT(*), SUM(CASE WHEN fl.MatchedClearingLineId IS NOT NULL THEN 1 ELSE 0 END), SUM(CASE WHEN fl.MatchedClearingLineId IS NULL THEN 1 ELSE 0 END),
             SUM(d.SourceAmount), SUM(CASE WHEN fl.MatchedClearingLineId IS NOT NULL THEN d.SourceAmount ELSE 0 END), SUM(CASE WHEN fl.MatchedClearingLineId IS NULL THEN d.SourceAmount ELSE 0 END)
         FROM [Ingestion].[FileLine] fl JOIN [Ingestion].[File] f ON f.Id = fl.FileId AND f.FileType = 'Clearing'
             CROSS APPLY (
             SELECT cv.SourceAmount FROM [Ingestion].[ClearingVisaDetail] cv WHERE cv.FileLineId = fl.Id AND f.ContentType = 'Visa'
             UNION ALL SELECT cm.SourceAmount FROM [Ingestion].[ClearingMscDetail] cm WHERE cm.FileLineId = fl.Id AND f.ContentType = 'Msc'
             UNION ALL SELECT cb.SourceAmount FROM [Ingestion].[ClearingBkmDetail] cb WHERE cb.FileLineId = fl.Id AND f.ContentType = 'Bkm'
             ) d
         GROUP BY CAST(f.CreateDate AS DATE), f.ContentType
         UNION ALL
         SELECT CAST(f.CreateDate AS DATE), 'ARCHIVE', f.ContentType, 'Card',
             COUNT(*), SUM(CASE WHEN fl.MatchedClearingLineId IS NOT NULL THEN 1 ELSE 0 END), SUM(CASE WHEN fl.MatchedClearingLineId IS NULL THEN 1 ELSE 0 END),
             SUM(ISNULL(d.OriginalAmount, 0)), SUM(CASE WHEN fl.MatchedClearingLineId IS NOT NULL THEN ISNULL(d.OriginalAmount, 0) ELSE 0 END), SUM(CASE WHEN fl.MatchedClearingLineId IS NULL THEN ISNULL(d.OriginalAmount, 0) ELSE 0 END)
         FROM [Archive].[IngestionFileLine] fl JOIN [Archive].[IngestionFile] f ON f.Id = fl.FileId AND f.FileType = 'Card'
             OUTER APPLY (
             SELECT cv.OriginalAmount FROM [Archive].[IngestionCardVisaDetail] cv WHERE cv.Id = fl.CardVisaDetailId AND f.ContentType = 'Visa'
             UNION ALL SELECT cm.OriginalAmount FROM [Archive].[IngestionCardMscDetail] cm WHERE cm.Id = fl.CardMscDetailId AND f.ContentType = 'Msc'
             UNION ALL SELECT cb.OriginalAmount FROM [Archive].[IngestionCardBkmDetail] cb WHERE cb.Id = fl.CardBkmDetailId AND f.ContentType = 'Bkm'
             ) d
         WHERE d.OriginalAmount IS NOT NULL
         GROUP BY CAST(f.CreateDate AS DATE), f.ContentType
         UNION ALL
         SELECT CAST(f.CreateDate AS DATE), 'ARCHIVE', f.ContentType, 'Clearing',
             COUNT(*), SUM(CASE WHEN fl.MatchedClearingLineId IS NOT NULL THEN 1 ELSE 0 END), SUM(CASE WHEN fl.MatchedClearingLineId IS NULL THEN 1 ELSE 0 END),
             SUM(ISNULL(d.SourceAmount, 0)), SUM(CASE WHEN fl.MatchedClearingLineId IS NOT NULL THEN ISNULL(d.SourceAmount, 0) ELSE 0 END), SUM(CASE WHEN fl.MatchedClearingLineId IS NULL THEN ISNULL(d.SourceAmount, 0) ELSE 0 END)
         FROM [Archive].[IngestionFileLine] fl JOIN [Archive].[IngestionFile] f ON f.Id = fl.FileId AND f.FileType = 'Clearing'
             OUTER APPLY (
             SELECT cv.SourceAmount FROM [Archive].[IngestionClearingVisaDetail] cv WHERE cv.Id = fl.ClearingVisaDetailId AND f.ContentType = 'Visa'
             UNION ALL SELECT cm.SourceAmount FROM [Archive].[IngestionClearingMscDetail] cm WHERE cm.Id = fl.ClearingMscDetailId AND f.ContentType = 'Msc'
             UNION ALL SELECT cb.SourceAmount FROM [Archive].[IngestionClearingBkmDetail] cb WHERE cb.Id = fl.ClearingBkmDetailId AND f.ContentType = 'Bkm'
             ) d
         WHERE d.SourceAmount IS NOT NULL
         GROUP BY CAST(f.CreateDate AS DATE), f.ContentType
     ) src
GROUP BY src.ReportDate, src.DataScope, src.Network, src.Side;
GO

-- E3. Card vs Clearing fark analizi
CREATE OR ALTER VIEW [Reporting].[VwReconGapAnalysis] AS
SELECT
    ISNULL(c.ReportDate, cl.ReportDate) AS ReportDate,
    ISNULL(c.DataScope, cl.DataScope) AS DataScope,
    ISNULL(c.Network, cl.Network) AS Network,
    ISNULL(c.CardLineCount, 0) AS CardLineCount,
    ISNULL(cl.ClearingLineCount, 0) AS ClearingLineCount,
    ISNULL(c.CardLineCount, 0) - ISNULL(cl.ClearingLineCount, 0) AS LineCountDifference,
    ISNULL(c.CardMatchedCount, 0) AS CardMatchedCount,
    ISNULL(cl.ClearingMatchedCount, 0) AS ClearingMatchedCount,
    ISNULL(c.CardTotalAmount, 0) AS CardTotalAmount,
    ISNULL(cl.ClearingTotalAmount, 0) AS ClearingTotalAmount,
    ISNULL(c.CardTotalAmount, 0) - ISNULL(cl.ClearingTotalAmount, 0) AS AmountDifference,
    CASE WHEN ISNULL(c.CardLineCount, 0) > 0 THEN ROUND(CAST(c.CardMatchedCount AS DECIMAL(38,4)) / c.CardLineCount * 100, 2) ELSE 0 END AS CardMatchRatePct,
    CASE WHEN ISNULL(cl.ClearingLineCount, 0) > 0 THEN ROUND(CAST(cl.ClearingMatchedCount AS DECIMAL(38,4)) / cl.ClearingLineCount * 100, 2) ELSE 0 END AS ClearingMatchRatePct
FROM (
         SELECT CAST(f.CreateDate AS DATE) AS ReportDate, src.ds AS DataScope, f.ContentType AS Network,
                COUNT(*) AS CardLineCount,
                SUM(CASE WHEN fl.MatchedClearingLineId IS NOT NULL THEN 1 ELSE 0 END) AS CardMatchedCount,
                SUM(d.OriginalAmount) AS CardTotalAmount
         FROM (SELECT 'LIVE' AS ds UNION ALL SELECT 'ARCHIVE') src
             CROSS APPLY (
        SELECT fl2.Id, fl2.FileId, fl2.MatchedClearingLineId FROM [Ingestion].[FileLine] fl2 WHERE src.ds = 'LIVE'
        UNION ALL SELECT fl2.Id, fl2.FileId, fl2.MatchedClearingLineId FROM [Archive].[IngestionFileLine] fl2 WHERE src.ds = 'ARCHIVE'
    ) fl
    CROSS APPLY (
        SELECT f2.CreateDate, f2.ContentType FROM [Ingestion].[File] f2 WHERE src.ds = 'LIVE' AND f2.Id = fl.FileId AND f2.FileType = 'Card'
        UNION ALL SELECT f2.CreateDate, f2.ContentType FROM [Archive].[IngestionFile] f2 WHERE src.ds = 'ARCHIVE' AND f2.Id = fl.FileId AND f2.FileType = 'Card'
    ) f
    CROSS APPLY (
        SELECT cv.OriginalAmount FROM [Ingestion].[CardVisaDetail] cv WHERE cv.FileLineId = fl.Id AND f.ContentType = 'Visa' AND src.ds = 'LIVE'
        UNION ALL SELECT cm.OriginalAmount FROM [Ingestion].[CardMscDetail] cm WHERE cm.FileLineId = fl.Id AND f.ContentType = 'Msc' AND src.ds = 'LIVE'
        UNION ALL SELECT cb.OriginalAmount FROM [Ingestion].[CardBkmDetail] cb WHERE cb.FileLineId = fl.Id AND f.ContentType = 'Bkm' AND src.ds = 'LIVE'
        UNION ALL 
        SELECT cv.OriginalAmount FROM [Archive].[IngestionCardVisaDetail] cv, [Archive].[IngestionFileLine] aifl 
            WHERE aifl.Id = fl.Id AND cv.Id = aifl.CardVisaDetailId AND f.ContentType = 'Visa' AND src.ds = 'ARCHIVE'
        UNION ALL 
        SELECT cm.OriginalAmount FROM [Archive].[IngestionCardMscDetail] cm, [Archive].[IngestionFileLine] aifl 
            WHERE aifl.Id = fl.Id AND cm.Id = aifl.CardMscDetailId AND f.ContentType = 'Msc' AND src.ds = 'ARCHIVE'
        UNION ALL 
        SELECT cb.OriginalAmount FROM [Archive].[IngestionCardBkmDetail] cb, [Archive].[IngestionFileLine] aifl 
            WHERE aifl.Id = fl.Id AND cb.Id = aifl.CardBkmDetailId AND f.ContentType = 'Bkm' AND src.ds = 'ARCHIVE'
    ) d
         GROUP BY CAST(f.CreateDate AS DATE), src.ds, f.ContentType
     ) c
         FULL OUTER JOIN (
    SELECT CAST(f.CreateDate AS DATE) AS ReportDate, src.ds AS DataScope, f.ContentType AS Network,
           COUNT(*) AS ClearingLineCount,
           SUM(CASE WHEN fl.MatchedClearingLineId IS NOT NULL THEN 1 ELSE 0 END) AS ClearingMatchedCount,
           SUM(d.SourceAmount) AS ClearingTotalAmount
    FROM (SELECT 'LIVE' AS ds UNION ALL SELECT 'ARCHIVE') src
        CROSS APPLY (
        SELECT fl2.Id, fl2.FileId, fl2.MatchedClearingLineId FROM [Ingestion].[FileLine] fl2 WHERE src.ds = 'LIVE'
        UNION ALL SELECT fl2.Id, fl2.FileId, fl2.MatchedClearingLineId FROM [Archive].[IngestionFileLine] fl2 WHERE src.ds = 'ARCHIVE'
    ) fl
    CROSS APPLY (
        SELECT f2.CreateDate, f2.ContentType FROM [Ingestion].[File] f2 WHERE src.ds = 'LIVE' AND f2.Id = fl.FileId AND f2.FileType = 'Clearing'
        UNION ALL SELECT f2.CreateDate, f2.ContentType FROM [Archive].[IngestionFile] f2 WHERE src.ds = 'ARCHIVE' AND f2.Id = fl.FileId AND f2.FileType = 'Clearing'
    ) f
    CROSS APPLY (
        SELECT cv.SourceAmount FROM [Ingestion].[ClearingVisaDetail] cv WHERE cv.FileLineId = fl.Id AND f.ContentType = 'Visa' AND src.ds = 'LIVE'
        UNION ALL SELECT cm.SourceAmount FROM [Ingestion].[ClearingMscDetail] cm WHERE cm.FileLineId = fl.Id AND f.ContentType = 'Msc' AND src.ds = 'LIVE'
        UNION ALL SELECT cb.SourceAmount FROM [Ingestion].[ClearingBkmDetail] cb WHERE cb.FileLineId = fl.Id AND f.ContentType = 'Bkm' AND src.ds = 'LIVE'
        UNION ALL 
        SELECT cv.SourceAmount FROM [Archive].[IngestionClearingVisaDetail] cv, [Archive].[IngestionFileLine] aifl 
            WHERE aifl.Id = fl.Id AND cv.Id = aifl.ClearingVisaDetailId AND f.ContentType = 'Visa' AND src.ds = 'ARCHIVE'
        UNION ALL 
        SELECT cm.SourceAmount FROM [Archive].[IngestionClearingMscDetail] cm, [Archive].[IngestionFileLine] aifl 
            WHERE aifl.Id = fl.Id AND cm.Id = aifl.ClearingMscDetailId AND f.ContentType = 'Msc' AND src.ds = 'ARCHIVE'
        UNION ALL 
        SELECT cb.SourceAmount FROM [Archive].[IngestionClearingBkmDetail] cb, [Archive].[IngestionFileLine] aifl 
            WHERE aifl.Id = fl.Id AND cb.Id = aifl.ClearingBkmDetailId AND f.ContentType = 'Bkm' AND src.ds = 'ARCHIVE'
    ) d
    GROUP BY CAST(f.CreateDate AS DATE), src.ds, f.ContentType
) cl ON c.ReportDate = cl.ReportDate AND c.DataScope = cl.DataScope AND c.Network = cl.Network;
GO

-- E4. Eşleşmemiş işlem yaş dağılımı
CREATE OR ALTER VIEW [Reporting].[VwUnmatchedTransactionAging] AS
SELECT
    x.AgeBucket,
    x.DataScope,
    x.Network,
    x.Side,
    x.UnmatchedCount,
    x.UnmatchedAmount,
    CASE WHEN t.TotalUnmatched > 0 THEN ROUND(CAST(x.UnmatchedCount AS DECIMAL(38,4)) / t.TotalUnmatched * 100, 2) ELSE 0 END AS PctOfTotalUnmatched
FROM (
         SELECT
             CASE
                 WHEN DATEDIFF(DAY, f.CreateDate, GETDATE()) < 1 THEN '0-1d'
                 WHEN DATEDIFF(DAY, f.CreateDate, GETDATE()) < 3 THEN '1-3d'
                 WHEN DATEDIFF(DAY, f.CreateDate, GETDATE()) < 7 THEN '3-7d'
                 WHEN DATEDIFF(DAY, f.CreateDate, GETDATE()) < 14 THEN '7-14d'
                 WHEN DATEDIFF(DAY, f.CreateDate, GETDATE()) < 30 THEN '14-30d'
                 ELSE '30d+'
                 END AS AgeBucket,
             'LIVE' AS DataScope,
             f.ContentType AS Network,
             f.FileType AS Side,
             COUNT(*) AS UnmatchedCount,
             SUM(ISNULL(d.Amount, 0)) AS UnmatchedAmount
         FROM [Ingestion].[FileLine] fl
             JOIN [Ingestion].[File] f ON f.Id = fl.FileId
             OUTER APPLY (
             SELECT cv.OriginalAmount AS Amount FROM [Ingestion].[CardVisaDetail] cv WHERE cv.FileLineId = fl.Id AND f.ContentType = 'Visa' AND f.FileType = 'Card'
             UNION ALL
             SELECT cm.OriginalAmount FROM [Ingestion].[CardMscDetail] cm WHERE cm.FileLineId = fl.Id AND f.ContentType = 'Msc' AND f.FileType = 'Card'
             UNION ALL
             SELECT cb.OriginalAmount FROM [Ingestion].[CardBkmDetail] cb WHERE cb.FileLineId = fl.Id AND f.ContentType = 'Bkm' AND f.FileType = 'Card'
             UNION ALL
             SELECT cv.SourceAmount FROM [Ingestion].[ClearingVisaDetail] cv WHERE cv.FileLineId = fl.Id AND f.ContentType = 'Visa' AND f.FileType = 'Clearing'
             UNION ALL
             SELECT cm.SourceAmount FROM [Ingestion].[ClearingMscDetail] cm WHERE cm.FileLineId = fl.Id AND f.ContentType = 'Msc' AND f.FileType = 'Clearing'
             UNION ALL
             SELECT cb.SourceAmount FROM [Ingestion].[ClearingBkmDetail] cb WHERE cb.FileLineId = fl.Id AND f.ContentType = 'Bkm' AND f.FileType = 'Clearing'
             ) d
         WHERE fl.MatchedClearingLineId IS NULL
         GROUP BY AgeBucket, f.ContentType, f.FileType

         UNION ALL

         SELECT
             CASE
             WHEN DATEDIFF(DAY, f.CreateDate, GETDATE()) < 1 THEN '0-1d'
             WHEN DATEDIFF(DAY, f.CreateDate, GETDATE()) < 3 THEN '1-3d'
             WHEN DATEDIFF(DAY, f.CreateDate, GETDATE()) < 7 THEN '3-7d'
             WHEN DATEDIFF(DAY, f.CreateDate, GETDATE()) < 14 THEN '7-14d'
             WHEN DATEDIFF(DAY, f.CreateDate, GETDATE()) < 30 THEN '14-30d'
             ELSE '30d+'
             END,
             'ARCHIVE',
             f.ContentType,
             f.FileType,
             COUNT(*),
             SUM(ISNULL(d.Amount, 0))
         FROM [Archive].[IngestionFileLine] fl
             JOIN [Archive].[IngestionFile] f ON f.Id = fl.FileId
             OUTER APPLY (
             SELECT cv.OriginalAmount AS Amount FROM [Archive].[IngestionCardVisaDetail] cv WHERE cv.Id = fl.CardVisaDetailId AND f.ContentType = 'Visa' AND f.FileType = 'Card'
             UNION ALL
             SELECT cm.OriginalAmount FROM [Archive].[IngestionCardMscDetail] cm WHERE cm.Id = fl.CardMscDetailId AND f.ContentType = 'Msc' AND f.FileType = 'Card'
             UNION ALL
             SELECT cb.OriginalAmount FROM [Archive].[IngestionCardBkmDetail] cb WHERE cb.Id = fl.CardBkmDetailId AND f.ContentType = 'Bkm' AND f.FileType = 'Card'
             UNION ALL
             SELECT cv.SourceAmount FROM [Archive].[IngestionClearingVisaDetail] cv WHERE cv.Id = fl.ClearingVisaDetailId AND f.ContentType = 'Visa' AND f.FileType = 'Clearing'
             UNION ALL
             SELECT cm.SourceAmount FROM [Archive].[IngestionClearingMscDetail] cm WHERE cm.Id = fl.ClearingMscDetailId AND f.ContentType = 'Msc' AND f.FileType = 'Clearing'
             UNION ALL
             SELECT cb.SourceAmount FROM [Archive].[IngestionClearingBkmDetail] cb WHERE cb.Id = fl.ClearingBkmDetailId AND f.ContentType = 'Bkm' AND f.FileType = 'Clearing'
             ) d
         WHERE fl.MatchedClearingLineId IS NULL
         GROUP BY AgeBucket, f.ContentType, f.FileType
     ) x
         CROSS JOIN (
    SELECT COUNT(*) AS TotalUnmatched
    FROM (
             SELECT 1 FROM [Ingestion].[FileLine] WHERE MatchedClearingLineId IS NULL
             UNION ALL
             SELECT 1 FROM [Archive].[IngestionFileLine] WHERE MatchedClearingLineId IS NULL
         ) u
) t;
GO

-- E5. Network bazlı mutabakat skorkartı
CREATE OR ALTER VIEW [Reporting].[VwNetworkReconScorecard] AS
SELECT
    x.DataScope,
    x.Network,
    x.TotalFileCount,
    x.TotalCardLineCount,
    x.TotalClearingLineCount,
    x.TotalMatchedCount,
    x.TotalUnmatchedCount,
    CASE WHEN (x.TotalCardLineCount + x.TotalClearingLineCount) > 0 THEN ROUND(CAST(x.TotalMatchedCount AS DECIMAL(38,4)) / (x.TotalCardLineCount + x.TotalClearingLineCount) * 100, 2) ELSE 0 END AS OverallMatchRatePct,
    x.TotalCardAmount,
    x.TotalClearingAmount,
    ISNULL(x.TotalCardAmount, 0) - ISNULL(x.TotalClearingAmount, 0) AS NetAmountDifference,
    x.AvgCardOriginalAmount,
    x.AvgClearingSourceAmount,
    x.ReconSuccessLineCount,
    x.ReconFailedLineCount,
    x.ReconPendingLineCount,
    CASE WHEN (x.ReconSuccessLineCount + x.ReconFailedLineCount + x.ReconPendingLineCount) > 0 THEN ROUND(CAST(x.ReconSuccessLineCount AS DECIMAL(38,4)) / (x.ReconSuccessLineCount + x.ReconFailedLineCount + x.ReconPendingLineCount) * 100, 2) ELSE 0 END AS ReconSuccessRatePct,
    x.FirstFileDate,
    x.LastFileDate
FROM (
         SELECT
             ds.DataScope,
             f.ContentType AS Network,
             COUNT(DISTINCT f.Id) AS TotalFileCount,
             SUM(CASE WHEN f.FileType = 'Card' THEN lc.LineCount ELSE 0 END) AS TotalCardLineCount,
             SUM(CASE WHEN f.FileType = 'Clearing' THEN lc.LineCount ELSE 0 END) AS TotalClearingLineCount,
             SUM(lc.MatchedCount) AS TotalMatchedCount,
             SUM(lc.UnmatchedCount) AS TotalUnmatchedCount,
             SUM(CASE WHEN f.FileType = 'Card' THEN lc.TotalAmount ELSE 0 END) AS TotalCardAmount,
             SUM(CASE WHEN f.FileType = 'Clearing' THEN lc.TotalAmount ELSE 0 END) AS TotalClearingAmount,
             CASE WHEN SUM(CASE WHEN f.FileType = 'Card' THEN lc.LineCount ELSE 0 END) > 0 THEN ROUND(SUM(CASE WHEN f.FileType = 'Card' THEN lc.TotalAmount ELSE 0 END) / SUM(CASE WHEN f.FileType = 'Card' THEN lc.LineCount ELSE 0 END), 2) ELSE 0 END AS AvgCardOriginalAmount,
             CASE WHEN SUM(CASE WHEN f.FileType = 'Clearing' THEN lc.LineCount ELSE 0 END) > 0 THEN ROUND(SUM(CASE WHEN f.FileType = 'Clearing' THEN lc.TotalAmount ELSE 0 END) / SUM(CASE WHEN f.FileType = 'Clearing' THEN lc.LineCount ELSE 0 END), 2) ELSE 0 END AS AvgClearingSourceAmount,
             SUM(lc.ReconSuccessCount) AS ReconSuccessLineCount,
             SUM(lc.ReconFailedCount) AS ReconFailedLineCount,
             SUM(lc.ReconPendingCount) AS ReconPendingLineCount,
             MIN(f.CreateDate) AS FirstFileDate,
             MAX(f.CreateDate) AS LastFileDate
         FROM (SELECT 'LIVE' AS DataScope UNION ALL SELECT 'ARCHIVE') ds
             CROSS APPLY (
        SELECT f2.Id, f2.ContentType, f2.FileType, f2.CreateDate
        FROM [Ingestion].[File] f2 WHERE ds.DataScope = 'LIVE'
        UNION ALL
        SELECT f2.Id, f2.ContentType, f2.FileType, f2.CreateDate
        FROM [Archive].[IngestionFile] f2 WHERE ds.DataScope = 'ARCHIVE'
    ) f
    CROSS APPLY (
        SELECT
            COUNT(*) AS LineCount,
            SUM(CASE WHEN fl.MatchedClearingLineId IS NOT NULL THEN 1 ELSE 0 END) AS MatchedCount,
            SUM(CASE WHEN fl.MatchedClearingLineId IS NULL THEN 1 ELSE 0 END) AS UnmatchedCount,
            SUM(CASE WHEN fl.ReconciliationStatus = 'Success' THEN 1 ELSE 0 END) AS ReconSuccessCount,
            SUM(CASE WHEN fl.ReconciliationStatus = 'Failed' THEN 1 ELSE 0 END) AS ReconFailedCount,
            SUM(CASE WHEN fl.ReconciliationStatus IN ('Ready', 'Processing') THEN 1 ELSE 0 END) AS ReconPendingCount,
            SUM(ISNULL(
                (SELECT TOP 1 cv.OriginalAmount FROM [Ingestion].[CardVisaDetail] cv WHERE cv.FileLineId = fl.Id AND f.ContentType = 'Visa' AND f.FileType = 'Card' AND ds.DataScope = 'LIVE'),
                (SELECT TOP 1 cm.OriginalAmount FROM [Ingestion].[CardMscDetail] cm WHERE cm.FileLineId = fl.Id AND f.ContentType = 'Msc' AND f.FileType = 'Card' AND ds.DataScope = 'LIVE'),
                (SELECT TOP 1 cb.OriginalAmount FROM [Ingestion].[CardBkmDetail] cb WHERE cb.FileLineId = fl.Id AND f.ContentType = 'Bkm' AND f.FileType = 'Card' AND ds.DataScope = 'LIVE'),
                (SELECT TOP 1 cv.SourceAmount FROM [Ingestion].[ClearingVisaDetail] cv WHERE cv.FileLineId = fl.Id AND f.ContentType = 'Visa' AND f.FileType = 'Clearing' AND ds.DataScope = 'LIVE'),
                (SELECT TOP 1 cm.SourceAmount FROM [Ingestion].[ClearingMscDetail] cm WHERE cm.FileLineId = fl.Id AND f.ContentType = 'Msc' AND f.FileType = 'Clearing' AND ds.DataScope = 'LIVE'),
                (SELECT TOP 1 cb.SourceAmount FROM [Ingestion].[ClearingBkmDetail] cb WHERE cb.FileLineId = fl.Id AND f.ContentType = 'Bkm' AND f.FileType = 'Clearing' AND ds.DataScope = 'LIVE'),
                (SELECT TOP 1 cv.OriginalAmount FROM [Archive].[IngestionCardVisaDetail] cv, [Archive].[IngestionFileLine] aifl 
                    WHERE aifl.Id = fl.Id AND cv.Id = aifl.CardVisaDetailId AND f.ContentType = 'Visa' AND f.FileType = 'Card' AND ds.DataScope = 'ARCHIVE'),
                (SELECT TOP 1 cm.OriginalAmount FROM [Archive].[IngestionCardMscDetail] cm, [Archive].[IngestionFileLine] aifl 
                    WHERE aifl.Id = fl.Id AND cm.Id = aifl.CardMscDetailId AND f.ContentType = 'Msc' AND f.FileType = 'Card' AND ds.DataScope = 'ARCHIVE'),
                (SELECT TOP 1 cb.OriginalAmount FROM [Archive].[IngestionCardBkmDetail] cb, [Archive].[IngestionFileLine] aifl 
                    WHERE aifl.Id = fl.Id AND cb.Id = aifl.CardBkmDetailId AND f.ContentType = 'Bkm' AND f.FileType = 'Card' AND ds.DataScope = 'ARCHIVE'),
                (SELECT TOP 1 cv.SourceAmount FROM [Archive].[IngestionClearingVisaDetail] cv, [Archive].[IngestionFileLine] aifl 
                    WHERE aifl.Id = fl.Id AND cv.Id = aifl.ClearingVisaDetailId AND f.ContentType = 'Visa' AND f.FileType = 'Clearing' AND ds.DataScope = 'ARCHIVE'),
                (SELECT TOP 1 cm.SourceAmount FROM [Archive].[IngestionClearingMscDetail] cm, [Archive].[IngestionFileLine] aifl 
                    WHERE aifl.Id = fl.Id AND cm.Id = aifl.ClearingMscDetailId AND f.ContentType = 'Msc' AND f.FileType = 'Clearing' AND ds.DataScope = 'ARCHIVE'),
                (SELECT TOP 1 cb.SourceAmount FROM [Archive].[IngestionClearingBkmDetail] cb, [Archive].[IngestionFileLine] aifl 
                    WHERE aifl.Id = fl.Id AND cb.Id = aifl.ClearingBkmDetailId AND f.ContentType = 'Bkm' AND f.FileType = 'Clearing' AND ds.DataScope = 'ARCHIVE'),
                0
            )) AS TotalAmount
        FROM (
            SELECT fl2.Id, fl2.MatchedClearingLineId, fl2.ReconciliationStatus
            FROM [Ingestion].[FileLine] fl2 WHERE fl2.FileId = f.Id AND ds.DataScope = 'LIVE'
            UNION ALL
            SELECT fl2.Id, fl2.MatchedClearingLineId, fl2.ReconciliationStatus
            FROM [Archive].[IngestionFileLine] fl2 WHERE fl2.FileId = f.Id AND ds.DataScope = 'ARCHIVE'
        ) fl
    ) lc
         GROUP BY ds.DataScope, f.ContentType
     ) x;
GO

-- =====================================================================================
-- F. CARD <-> CLEARING CORRELATION
-- =====================================================================================

-- F1. Card kayıtlarını LIVE + ARCHIVE clearing kayıtlarıyla eşleştirir
--     (OceanTxnGuid, Rrn ya da Arn üzerinden öncelikli arama)
CREATE OR ALTER VIEW [Reporting].[VwCardClearingCorrelation] AS
WITH AllCard AS (
    -- LIVE
    SELECT
        CAST('LIVE' AS VARCHAR(16)) AS DataScope,
        CAST('CardBkmDetail' AS VARCHAR(64)) AS CardTable,
        c.Id AS CardId,
        c.FileLineId,
        fl.FileId,
        c.OceanTxnGuid,
        c.Rrn,
        c.Arn
    FROM [Ingestion].[CardBkmDetail] c
    JOIN [Ingestion].[FileLine] fl ON fl.Id = c.FileLineId

    UNION ALL

    SELECT
        'LIVE',
        'CardVisaDetail',
        c.Id,
        c.FileLineId,
        fl.FileId,
        c.OceanTxnGuid,
        c.Rrn,
        c.Arn
    FROM [Ingestion].[CardVisaDetail] c
    JOIN [Ingestion].[FileLine] fl ON fl.Id = c.FileLineId

    UNION ALL

    SELECT
        'LIVE',
        'CardMscDetail',
        c.Id,
        c.FileLineId,
        fl.FileId,
        c.OceanTxnGuid,
        c.Rrn,
        c.Arn
    FROM [Ingestion].[CardMscDetail] c
    JOIN [Ingestion].[FileLine] fl ON fl.Id = c.FileLineId

    -- ARCHIVE
    UNION ALL

    SELECT
        'ARCHIVE',
        'Archive.IngestionCardBkmDetail',
        c.Id,
        c.FileLineId,
        fl.FileId,
        c.OceanTxnGuid,
        c.Rrn,
        c.Arn
    FROM [Archive].[IngestionCardBkmDetail] c
    JOIN [Archive].[IngestionFileLine] fl ON fl.Id = c.FileLineId

    UNION ALL

    SELECT
        'ARCHIVE',
        'Archive.IngestionCardVisaDetail',
        c.Id,
        c.FileLineId,
        fl.FileId,
        c.OceanTxnGuid,
        c.Rrn,
        c.Arn
    FROM [Archive].[IngestionCardVisaDetail] c
    JOIN [Archive].[IngestionFileLine] fl ON fl.Id = c.FileLineId

    UNION ALL

    SELECT
        'ARCHIVE',
        'Archive.IngestionCardMscDetail',
        c.Id,
        c.FileLineId,
        fl.FileId,
        c.OceanTxnGuid,
        c.Rrn,
        c.Arn
    FROM [Archive].[IngestionCardMscDetail] c
    JOIN [Archive].[IngestionFileLine] fl ON fl.Id = c.FileLineId
)
SELECT
    ROW_NUMBER() OVER (ORDER BY c.DataScope, c.CardTable, c.CardId) AS RowNumber,

    c.DataScope                  AS DataScope,
    c.CardTable                  AS CardTable,

    c.CardId                     AS CardId,
    c.FileLineId                 AS FileLineId,
    c.FileId                     AS FileId,

    bkm_live.Id                  AS ClearingBkmLiveId,
    bkm_arc.Id                   AS ClearingBkmArchiveId,

    visa_live.Id                 AS ClearingVisaLiveId,
    visa_arc.Id                  AS ClearingVisaArchiveId,

    msc_live.Id                  AS ClearingMastercardLiveId,
    msc_arc.Id                   AS ClearingMastercardArchiveId
FROM AllCard c

-- BKM LIVE
OUTER APPLY (
    SELECT TOP 1 x.Id
    FROM [Ingestion].[ClearingBkmDetail] x
    WHERE x.OceanTxnGuid = c.OceanTxnGuid
       OR (c.Rrn IS NOT NULL AND x.Rrn = c.Rrn)
       OR (c.Arn IS NOT NULL AND x.Arn = c.Arn)
    ORDER BY
        CASE
            WHEN x.OceanTxnGuid = c.OceanTxnGuid THEN 1
            WHEN x.Rrn = c.Rrn THEN 2
            WHEN x.Arn = c.Arn THEN 3
            ELSE 99
        END
) bkm_live

-- BKM ARCHIVE
OUTER APPLY (
    SELECT TOP 1 x.Id
    FROM [Archive].[IngestionClearingBkmDetail] x
    WHERE x.OceanTxnGuid = c.OceanTxnGuid
       OR (c.Rrn IS NOT NULL AND x.Rrn = c.Rrn)
       OR (c.Arn IS NOT NULL AND x.Arn = c.Arn)
    ORDER BY
        CASE
            WHEN x.OceanTxnGuid = c.OceanTxnGuid THEN 1
            WHEN x.Rrn = c.Rrn THEN 2
            WHEN x.Arn = c.Arn THEN 3
            ELSE 99
        END
) bkm_arc

-- VISA LIVE
OUTER APPLY (
    SELECT TOP 1 x.Id
    FROM [Ingestion].[ClearingVisaDetail] x
    WHERE x.OceanTxnGuid = c.OceanTxnGuid
       OR (c.Rrn IS NOT NULL AND x.Rrn = c.Rrn)
       OR (c.Arn IS NOT NULL AND x.Arn = c.Arn)
    ORDER BY
        CASE
            WHEN x.OceanTxnGuid = c.OceanTxnGuid THEN 1
            WHEN x.Rrn = c.Rrn THEN 2
            WHEN x.Arn = c.Arn THEN 3
            ELSE 99
        END
) visa_live

-- VISA ARCHIVE
OUTER APPLY (
    SELECT TOP 1 x.Id
    FROM [Archive].[IngestionClearingVisaDetail] x
    WHERE x.OceanTxnGuid = c.OceanTxnGuid
       OR (c.Rrn IS NOT NULL AND x.Rrn = c.Rrn)
       OR (c.Arn IS NOT NULL AND x.Arn = c.Arn)
    ORDER BY
        CASE
            WHEN x.OceanTxnGuid = c.OceanTxnGuid THEN 1
            WHEN x.Rrn = c.Rrn THEN 2
            WHEN x.Arn = c.Arn THEN 3
            ELSE 99
        END
) visa_arc

-- MSC LIVE
OUTER APPLY (
    SELECT TOP 1 x.Id
    FROM [Ingestion].[ClearingMscDetail] x
    WHERE x.OceanTxnGuid = c.OceanTxnGuid
       OR (c.Rrn IS NOT NULL AND x.Rrn = c.Rrn)
       OR (c.Arn IS NOT NULL AND x.Arn = c.Arn)
    ORDER BY
        CASE
            WHEN x.OceanTxnGuid = c.OceanTxnGuid THEN 1
            WHEN x.Rrn = c.Rrn THEN 2
            WHEN x.Arn = c.Arn THEN 3
            ELSE 99
        END
) msc_live

-- MSC ARCHIVE
OUTER APPLY (
    SELECT TOP 1 x.Id
    FROM [Archive].[IngestionClearingMscDetail] x
    WHERE x.OceanTxnGuid = c.OceanTxnGuid
       OR (c.Rrn IS NOT NULL AND x.Rrn = c.Rrn)
       OR (c.Arn IS NOT NULL AND x.Arn = c.Arn)
    ORDER BY
        CASE
            WHEN x.OceanTxnGuid = c.OceanTxnGuid THEN 1
            WHEN x.Rrn = c.Rrn THEN 2
            WHEN x.Arn = c.Arn THEN 3
            ELSE 99
        END
) msc_arc;
GO


-- =====================================================================================
-- G. FINANCIAL ANALYTICS  (MSSQL conversion of PG rep_* extension views)
-- LIVE + ARCHIVE union, network breakdown, flag/urgency/recommended_action triplet.
-- =====================================================================================

-- G1. Daily transaction volume by network (real TransactionDate)
CREATE OR ALTER VIEW [Reporting].[VwDailyTransactionVolume] AS
WITH Src AS (
    SELECT 'LIVE' AS Scope, 'BKM' AS Network, TransactionDate, OriginalAmount, OriginalCurrency, FinancialType, TxnEffect, Tax1, Tax2, SurchargeAmount, CashbackAmount FROM [Ingestion].[CardBkmDetail]
    UNION ALL SELECT 'LIVE','VISA', TransactionDate, OriginalAmount, OriginalCurrency, FinancialType, TxnEffect, Tax1, Tax2, SurchargeAmount, CashbackAmount FROM [Ingestion].[CardVisaDetail]
    UNION ALL SELECT 'LIVE','MSC',  TransactionDate, OriginalAmount, OriginalCurrency, FinancialType, TxnEffect, Tax1, Tax2, SurchargeAmount, CashbackAmount FROM [Ingestion].[CardMscDetail]
    UNION ALL SELECT 'ARCHIVE','BKM', TransactionDate, OriginalAmount, OriginalCurrency, FinancialType, TxnEffect, Tax1, Tax2, SurchargeAmount, CashbackAmount FROM [Archive].[IngestionCardBkmDetail]
    UNION ALL SELECT 'ARCHIVE','VISA',TransactionDate, OriginalAmount, OriginalCurrency, FinancialType, TxnEffect, Tax1, Tax2, SurchargeAmount, CashbackAmount FROM [Archive].[IngestionCardVisaDetail]
    UNION ALL SELECT 'ARCHIVE','MSC', TransactionDate, OriginalAmount, OriginalCurrency, FinancialType, TxnEffect, Tax1, Tax2, SurchargeAmount, CashbackAmount FROM [Archive].[IngestionCardMscDetail]
)
SELECT
    Scope AS DataScope, Network,
    TRY_CONVERT(DATE, CAST(TransactionDate AS VARCHAR(8)), 112) AS TransactionDate,
    CAST(OriginalCurrency AS VARCHAR(8))                        AS OriginalCurrency,
    COUNT(*)                                                    AS TransactionCount,
    SUM(OriginalAmount)                                         AS GrossAmount,
    SUM(CASE WHEN TxnEffect = 'Debit'  THEN OriginalAmount ELSE 0 END)          AS DebitAmount,
    SUM(CASE WHEN TxnEffect = 'Credit' THEN OriginalAmount ELSE 0 END)          AS CreditAmount,
    SUM(CASE WHEN TxnEffect = 'Debit'  THEN OriginalAmount ELSE 0 END)
        - SUM(CASE WHEN TxnEffect = 'Credit' THEN OriginalAmount ELSE 0 END)    AS NetFlowAmount,
    SUM(ISNULL(Tax1,0) + ISNULL(Tax2,0))                                        AS TotalTaxAmount,
    SUM(ISNULL(SurchargeAmount,0))                                              AS TotalSurchargeAmount,
    SUM(ISNULL(CashbackAmount,0))                                               AS TotalCashbackAmount,
    CASE WHEN Scope = 'ARCHIVE' THEN 'HISTORICAL'
         WHEN SUM(OriginalAmount) >= 10000000 THEN 'PEAK_VOLUME_DAY'
         WHEN SUM(OriginalAmount) >= 1000000  THEN 'HIGH_VOLUME_DAY'
         ELSE 'NORMAL' END AS VolumeFlag,
    CASE WHEN Scope = 'ARCHIVE' THEN 'P5'
         WHEN SUM(OriginalAmount) >= 10000000 THEN 'P2'
         WHEN SUM(OriginalAmount) >= 1000000  THEN 'P3'
         ELSE 'P5' END AS Urgency,
    CASE WHEN Scope = 'ARCHIVE' THEN 'HISTORICAL_TREND_ANALYSIS_ONLY'
         WHEN SUM(OriginalAmount) >= 10000000 THEN 'CAPACITY_PLANNING_REVIEW'
         WHEN SUM(OriginalAmount) >= 1000000  THEN 'MONITOR_VOLUME_TREND'
         ELSE 'NONE' END AS RecommendedAction
FROM Src
WHERE TransactionDate BETWEEN 19000101 AND 99991231
GROUP BY Scope, Network, TransactionDate, OriginalCurrency;
GO


-- G2. MCC revenue concentration
CREATE OR ALTER VIEW [Reporting].[VwMccRevenueConcentration] AS
WITH Src AS (
    SELECT 'LIVE' AS Scope, 'BKM' AS Network, Mcc, OriginalAmount FROM [Ingestion].[CardBkmDetail]
    UNION ALL SELECT 'LIVE','VISA', Mcc, OriginalAmount FROM [Ingestion].[CardVisaDetail]
    UNION ALL SELECT 'LIVE','MSC',  Mcc, OriginalAmount FROM [Ingestion].[CardMscDetail]
    UNION ALL SELECT 'ARCHIVE','BKM', Mcc, OriginalAmount FROM [Archive].[IngestionCardBkmDetail]
    UNION ALL SELECT 'ARCHIVE','VISA',Mcc, OriginalAmount FROM [Archive].[IngestionCardVisaDetail]
    UNION ALL SELECT 'ARCHIVE','MSC', Mcc, OriginalAmount FROM [Archive].[IngestionCardMscDetail]
), Agg AS (
    SELECT Scope, Network, Mcc,
           COUNT(*)            AS TransactionCount,
           SUM(OriginalAmount) AS TotalAmount
    FROM Src
    GROUP BY Scope, Network, Mcc
)
SELECT
    Scope AS DataScope, Network, Mcc, TransactionCount, TotalAmount,
    ROUND(CAST(TotalAmount AS DECIMAL(38,4)) * 100
          / NULLIF(SUM(TotalAmount) OVER (PARTITION BY Scope, Network), 0), 2) AS NetworkSharePct,
    CASE WHEN Scope = 'ARCHIVE' THEN 'HISTORICAL'
         WHEN CAST(TotalAmount AS DECIMAL(38,4)) / NULLIF(SUM(TotalAmount) OVER (PARTITION BY Scope, Network), 0) >= 0.30 THEN 'HIGH_CONCENTRATION'
         WHEN CAST(TotalAmount AS DECIMAL(38,4)) / NULLIF(SUM(TotalAmount) OVER (PARTITION BY Scope, Network), 0) >= 0.15 THEN 'MEDIUM_CONCENTRATION'
         ELSE 'DIVERSIFIED' END AS ConcentrationFlag,
    CASE WHEN Scope = 'ARCHIVE' THEN 'P5'
         WHEN CAST(TotalAmount AS DECIMAL(38,4)) / NULLIF(SUM(TotalAmount) OVER (PARTITION BY Scope, Network), 0) >= 0.30 THEN 'P2'
         WHEN CAST(TotalAmount AS DECIMAL(38,4)) / NULLIF(SUM(TotalAmount) OVER (PARTITION BY Scope, Network), 0) >= 0.15 THEN 'P3'
         ELSE 'P5' END AS Urgency,
    CASE WHEN Scope = 'ARCHIVE' THEN 'HISTORICAL_TREND_ANALYSIS_ONLY'
         WHEN CAST(TotalAmount AS DECIMAL(38,4)) / NULLIF(SUM(TotalAmount) OVER (PARTITION BY Scope, Network), 0) >= 0.30 THEN 'REVIEW_MCC_CONCENTRATION_RISK'
         WHEN CAST(TotalAmount AS DECIMAL(38,4)) / NULLIF(SUM(TotalAmount) OVER (PARTITION BY Scope, Network), 0) >= 0.15 THEN 'MONITOR_MCC_DEPENDENCY'
         ELSE 'NONE' END AS RecommendedAction
FROM Agg;
GO


-- G3. Merchant risk hotspots
CREATE OR ALTER VIEW [Reporting].[VwMerchantRiskHotspots] AS
WITH Src AS (
    SELECT 'LIVE' AS Scope, 'BKM' AS Network, d.MerchantId, d.MerchantName, d.MerchantCountry, d.OriginalAmount, d.IsSuccessfulTxn, d.ResponseCode,
           CASE WHEN fl.MatchedClearingLineId IS NULL THEN 1 ELSE 0 END AS IsUnmatched
    FROM [Ingestion].[CardBkmDetail] d JOIN [Ingestion].[FileLine] fl ON fl.Id = d.FileLineId
    UNION ALL
    SELECT 'LIVE','VISA', d.MerchantId, d.MerchantName, d.MerchantCountry, d.OriginalAmount, d.IsSuccessfulTxn, d.ResponseCode,
           CASE WHEN fl.MatchedClearingLineId IS NULL THEN 1 ELSE 0 END
    FROM [Ingestion].[CardVisaDetail] d JOIN [Ingestion].[FileLine] fl ON fl.Id = d.FileLineId
    UNION ALL
    SELECT 'LIVE','MSC', d.MerchantId, d.MerchantName, d.MerchantCountry, d.OriginalAmount, d.IsSuccessfulTxn, d.ResponseCode,
           CASE WHEN fl.MatchedClearingLineId IS NULL THEN 1 ELSE 0 END
    FROM [Ingestion].[CardMscDetail] d JOIN [Ingestion].[FileLine] fl ON fl.Id = d.FileLineId
    UNION ALL
    SELECT 'ARCHIVE','BKM', d.MerchantId, d.MerchantName, d.MerchantCountry, d.OriginalAmount, d.IsSuccessfulTxn, d.ResponseCode,
           CASE WHEN fl.MatchedClearingLineId IS NULL THEN 1 ELSE 0 END
    FROM [Archive].[IngestionCardBkmDetail] d JOIN [Archive].[IngestionFileLine] fl ON fl.Id = d.FileLineId
    UNION ALL
    SELECT 'ARCHIVE','VISA', d.MerchantId, d.MerchantName, d.MerchantCountry, d.OriginalAmount, d.IsSuccessfulTxn, d.ResponseCode,
           CASE WHEN fl.MatchedClearingLineId IS NULL THEN 1 ELSE 0 END
    FROM [Archive].[IngestionCardVisaDetail] d JOIN [Archive].[IngestionFileLine] fl ON fl.Id = d.FileLineId
    UNION ALL
    SELECT 'ARCHIVE','MSC', d.MerchantId, d.MerchantName, d.MerchantCountry, d.OriginalAmount, d.IsSuccessfulTxn, d.ResponseCode,
           CASE WHEN fl.MatchedClearingLineId IS NULL THEN 1 ELSE 0 END
    FROM [Archive].[IngestionCardMscDetail] d JOIN [Archive].[IngestionFileLine] fl ON fl.Id = d.FileLineId
), Agg AS (
    SELECT Scope, Network,
           ISNULL(MerchantId, 'UNKNOWN')      AS MerchantId,
           ISNULL(MerchantName, 'UNKNOWN')    AS MerchantName,
           ISNULL(MerchantCountry, 'UNKNOWN') AS MerchantCountry,
           COUNT(*)                                                                                                  AS TransactionCount,
           SUM(CASE WHEN IsSuccessfulTxn = 'N' OR (ResponseCode IS NOT NULL AND ResponseCode <> '00') THEN 1 ELSE 0 END) AS DeclinedCount,
           SUM(IsUnmatched)                                                                                            AS UnmatchedCount,
           SUM(OriginalAmount)                                                                                          AS TotalAmount,
           SUM(CASE WHEN IsUnmatched = 1 THEN OriginalAmount ELSE 0 END)                                                AS UnmatchedAmount
    FROM Src
    GROUP BY Scope, Network, ISNULL(MerchantId, 'UNKNOWN'), ISNULL(MerchantName, 'UNKNOWN'), ISNULL(MerchantCountry, 'UNKNOWN')
)
SELECT
    Scope AS DataScope, Network, MerchantId, MerchantName, MerchantCountry,
    TransactionCount, DeclinedCount, UnmatchedCount, TotalAmount, UnmatchedAmount,
    CASE WHEN TransactionCount > 0 THEN ROUND(CAST(DeclinedCount  AS DECIMAL(38,4)) / TransactionCount * 100, 2) ELSE 0 END AS DeclineRatePct,
    CASE WHEN TransactionCount > 0 THEN ROUND(CAST(UnmatchedCount AS DECIMAL(38,4)) / TransactionCount * 100, 2) ELSE 0 END AS UnmatchedRatePct,
    CASE WHEN Scope = 'ARCHIVE' THEN 'HISTORICAL'
         WHEN UnmatchedCount > 0 AND CAST(UnmatchedCount AS DECIMAL(38,4)) / NULLIF(TransactionCount, 0) >= 0.20 AND UnmatchedAmount >= 100000 THEN 'HIGH_RISK_MERCHANT'
         WHEN CAST(DeclinedCount AS DECIMAL(38,4)) / NULLIF(TransactionCount, 0) >= 0.30 THEN 'HIGH_DECLINE_MERCHANT'
         WHEN UnmatchedCount > 0 THEN 'NEEDS_INVESTIGATION'
         ELSE 'HEALTHY' END AS RiskFlag,
    CASE WHEN Scope = 'ARCHIVE' THEN 'P5'
         WHEN UnmatchedCount > 0 AND CAST(UnmatchedCount AS DECIMAL(38,4)) / NULLIF(TransactionCount, 0) >= 0.20 AND UnmatchedAmount >= 100000 THEN 'P1'
         WHEN CAST(DeclinedCount AS DECIMAL(38,4)) / NULLIF(TransactionCount, 0) >= 0.30 THEN 'P2'
         WHEN UnmatchedCount > 0 THEN 'P3'
         ELSE 'P5' END AS Urgency,
    CASE WHEN Scope = 'ARCHIVE' THEN 'HISTORICAL_TREND_ANALYSIS_ONLY'
         WHEN UnmatchedCount > 0 AND CAST(UnmatchedCount AS DECIMAL(38,4)) / NULLIF(TransactionCount, 0) >= 0.20 AND UnmatchedAmount >= 100000 THEN 'ESCALATE_TO_RISK_TEAM'
         WHEN CAST(DeclinedCount AS DECIMAL(38,4)) / NULLIF(TransactionCount, 0) >= 0.30 THEN 'INVESTIGATE_DECLINE_PATTERN'
         WHEN UnmatchedCount > 0 THEN 'INVESTIGATE_UNMATCHED'
         ELSE 'NONE' END AS RecommendedAction
FROM Agg;
GO


-- G4. Country / cross-border FX exposure
CREATE OR ALTER VIEW [Reporting].[VwCountryCrossBorderExposure] AS
WITH Src AS (
    SELECT 'LIVE' AS Scope, 'BKM' AS Network, MerchantCountry, OriginalAmount, OriginalCurrency, SettlementCurrency FROM [Ingestion].[CardBkmDetail]
    UNION ALL SELECT 'LIVE','VISA', MerchantCountry, OriginalAmount, OriginalCurrency, SettlementCurrency FROM [Ingestion].[CardVisaDetail]
    UNION ALL SELECT 'LIVE','MSC',  MerchantCountry, OriginalAmount, OriginalCurrency, SettlementCurrency FROM [Ingestion].[CardMscDetail]
    UNION ALL SELECT 'ARCHIVE','BKM', MerchantCountry, OriginalAmount, OriginalCurrency, SettlementCurrency FROM [Archive].[IngestionCardBkmDetail]
    UNION ALL SELECT 'ARCHIVE','VISA',MerchantCountry, OriginalAmount, OriginalCurrency, SettlementCurrency FROM [Archive].[IngestionCardVisaDetail]
    UNION ALL SELECT 'ARCHIVE','MSC', MerchantCountry, OriginalAmount, OriginalCurrency, SettlementCurrency FROM [Archive].[IngestionCardMscDetail]
), Agg AS (
    SELECT Scope, Network,
           ISNULL(MerchantCountry, 'UNKNOWN') AS MerchantCountry,
           CASE WHEN OriginalCurrency <> SettlementCurrency THEN 'CROSS_CURRENCY' ELSE 'SAME_CURRENCY' END AS FxPattern,
           CAST(OriginalCurrency AS VARCHAR(8))   AS OriginalCurrency,
           CAST(SettlementCurrency AS VARCHAR(8)) AS SettlementCurrency,
           COUNT(*)                  AS TransactionCount,
           SUM(OriginalAmount)       AS TotalOriginalAmount
    FROM Src
    GROUP BY Scope, Network,
             ISNULL(MerchantCountry, 'UNKNOWN'),
             CASE WHEN OriginalCurrency <> SettlementCurrency THEN 'CROSS_CURRENCY' ELSE 'SAME_CURRENCY' END,
             OriginalCurrency, SettlementCurrency
)
SELECT
    Scope AS DataScope, Network, MerchantCountry, FxPattern, OriginalCurrency, SettlementCurrency,
    TransactionCount, TotalOriginalAmount,
    CASE WHEN Scope = 'ARCHIVE' THEN 'HISTORICAL'
         WHEN FxPattern = 'CROSS_CURRENCY' AND TotalOriginalAmount >= 1000000 THEN 'HIGH_FX_EXPOSURE'
         WHEN FxPattern = 'CROSS_CURRENCY'                                     THEN 'FX_EXPOSURE'
         ELSE 'DOMESTIC' END AS ExposureFlag,
    CASE WHEN Scope = 'ARCHIVE' THEN 'P5'
         WHEN FxPattern = 'CROSS_CURRENCY' AND TotalOriginalAmount >= 1000000 THEN 'P2'
         WHEN FxPattern = 'CROSS_CURRENCY'                                     THEN 'P3'
         ELSE 'P5' END AS Urgency,
    CASE WHEN Scope = 'ARCHIVE' THEN 'HISTORICAL_TREND_ANALYSIS_ONLY'
         WHEN FxPattern = 'CROSS_CURRENCY' AND TotalOriginalAmount >= 1000000 THEN 'HEDGE_OR_REVIEW_FX_EXPOSURE'
         WHEN FxPattern = 'CROSS_CURRENCY'                                     THEN 'MONITOR_FX_EXPOSURE'
         ELSE 'NONE' END AS RecommendedAction
FROM Agg;
GO


-- G5. Response code decline health
CREATE OR ALTER VIEW [Reporting].[VwResponseCodeDeclineHealth] AS
WITH Src AS (
    SELECT 'LIVE' AS Scope, 'BKM' AS Network, ResponseCode, IsSuccessfulTxn, OriginalAmount FROM [Ingestion].[CardBkmDetail]
    UNION ALL SELECT 'LIVE','VISA', ResponseCode, IsSuccessfulTxn, OriginalAmount FROM [Ingestion].[CardVisaDetail]
    UNION ALL SELECT 'LIVE','MSC',  ResponseCode, IsSuccessfulTxn, OriginalAmount FROM [Ingestion].[CardMscDetail]
    UNION ALL SELECT 'ARCHIVE','BKM', ResponseCode, IsSuccessfulTxn, OriginalAmount FROM [Archive].[IngestionCardBkmDetail]
    UNION ALL SELECT 'ARCHIVE','VISA',ResponseCode, IsSuccessfulTxn, OriginalAmount FROM [Archive].[IngestionCardVisaDetail]
    UNION ALL SELECT 'ARCHIVE','MSC', ResponseCode, IsSuccessfulTxn, OriginalAmount FROM [Archive].[IngestionCardMscDetail]
), Agg AS (
    SELECT Scope, Network, ISNULL(ResponseCode, 'NONE') AS ResponseCode,
           COUNT(*)                                                  AS TransactionCount,
           SUM(OriginalAmount)                                       AS TotalAmount,
           SUM(CASE WHEN IsSuccessfulTxn = 'Y' THEN 1 ELSE 0 END)    AS SuccessfulCount,
           SUM(CASE WHEN IsSuccessfulTxn = 'N' THEN 1 ELSE 0 END)    AS FailedCount
    FROM Src
    GROUP BY Scope, Network, ISNULL(ResponseCode, 'NONE')
)
SELECT
    Scope AS DataScope, Network, ResponseCode,
    TransactionCount, TotalAmount, SuccessfulCount, FailedCount,
    CASE WHEN TransactionCount > 0 THEN ROUND(CAST(FailedCount AS DECIMAL(38,4)) / TransactionCount * 100, 2) ELSE 0 END AS FailureRatePct,
    ROUND(CAST(TransactionCount AS DECIMAL(38,4)) * 100 / NULLIF(SUM(TransactionCount) OVER (PARTITION BY Scope, Network), 0), 2) AS NetworkSharePct,
    CASE WHEN Scope = 'ARCHIVE' THEN 'HISTORICAL'
         WHEN ResponseCode NOT IN ('00','NONE')
              AND CAST(TransactionCount AS DECIMAL(38,4)) / NULLIF(SUM(TransactionCount) OVER (PARTITION BY Scope, Network), 0) >= 0.05 THEN 'DOMINANT_FAILURE_REASON'
         WHEN ResponseCode NOT IN ('00','NONE') THEN 'NORMAL_FAILURE'
         ELSE 'SUCCESS_OR_UNKNOWN' END AS HealthFlag,
    CASE WHEN Scope = 'ARCHIVE' THEN 'P5'
         WHEN ResponseCode NOT IN ('00','NONE')
              AND CAST(TransactionCount AS DECIMAL(38,4)) / NULLIF(SUM(TransactionCount) OVER (PARTITION BY Scope, Network), 0) >= 0.05 THEN 'P2'
         WHEN ResponseCode NOT IN ('00','NONE') THEN 'P4'
         ELSE 'P5' END AS Urgency,
    CASE WHEN Scope = 'ARCHIVE' THEN 'HISTORICAL_TREND_ANALYSIS_ONLY'
         WHEN ResponseCode NOT IN ('00','NONE')
              AND CAST(TransactionCount AS DECIMAL(38,4)) / NULLIF(SUM(TransactionCount) OVER (PARTITION BY Scope, Network), 0) >= 0.05 THEN 'INVESTIGATE_DOMINANT_DECLINE_REASON'
         WHEN ResponseCode NOT IN ('00','NONE') THEN 'TRACK_DECLINE_REASON'
         ELSE 'NONE' END AS RecommendedAction
FROM Agg;
GO


-- G6. Settlement lag analysis
CREATE OR ALTER VIEW [Reporting].[VwSettlementLagAnalysis] AS
WITH Src AS (
    SELECT 'LIVE' AS Scope, 'BKM' AS Network, TransactionDate, ValueDate, EndOfDayDate, CreateDate, OriginalAmount FROM [Ingestion].[CardBkmDetail]
    UNION ALL SELECT 'LIVE','VISA', TransactionDate, ValueDate, EndOfDayDate, CreateDate, OriginalAmount FROM [Ingestion].[CardVisaDetail]
    UNION ALL SELECT 'LIVE','MSC',  TransactionDate, ValueDate, EndOfDayDate, CreateDate, OriginalAmount FROM [Ingestion].[CardMscDetail]
    UNION ALL SELECT 'ARCHIVE','BKM', TransactionDate, ValueDate, EndOfDayDate, CreateDate, OriginalAmount FROM [Archive].[IngestionCardBkmDetail]
    UNION ALL SELECT 'ARCHIVE','VISA',TransactionDate, ValueDate, EndOfDayDate, CreateDate, OriginalAmount FROM [Archive].[IngestionCardVisaDetail]
    UNION ALL SELECT 'ARCHIVE','MSC', TransactionDate, ValueDate, EndOfDayDate, CreateDate, OriginalAmount FROM [Archive].[IngestionCardMscDetail]
), Calc AS (
    SELECT Scope, Network, OriginalAmount,
           DATEDIFF(DAY, TRY_CONVERT(DATE, CAST(TransactionDate AS VARCHAR(8)), 112), CAST(CreateDate AS DATE))                                          AS IngestLagDays,
           DATEDIFF(DAY, TRY_CONVERT(DATE, CAST(TransactionDate AS VARCHAR(8)), 112), TRY_CONVERT(DATE, CAST(ValueDate AS VARCHAR(8)), 112))             AS ValueLagDays,
           DATEDIFF(DAY, TRY_CONVERT(DATE, CAST(TransactionDate AS VARCHAR(8)), 112), TRY_CONVERT(DATE, CAST(EndOfDayDate AS VARCHAR(8)), 112))          AS EodLagDays
    FROM Src
    WHERE TransactionDate BETWEEN 19000101 AND 99991231
)
SELECT
    Scope AS DataScope, Network,
    COUNT(*)                                                            AS TransactionCount,
    SUM(OriginalAmount)                                                 AS TotalAmount,
    AVG(CAST(IngestLagDays AS DECIMAL(38,4)))                           AS AvgIngestLagDays,
    AVG(CAST(ValueLagDays  AS DECIMAL(38,4)))                           AS AvgValueLagDays,
    AVG(CAST(EodLagDays    AS DECIMAL(38,4)))                           AS AvgEodLagDays,
    MAX(IngestLagDays)                                                  AS MaxIngestLagDays,
    MAX(ValueLagDays)                                                   AS MaxValueLagDays,
    SUM(CASE WHEN IngestLagDays > 3 THEN 1 ELSE 0 END)                  AS LateIngestCount,
    SUM(CASE WHEN ValueLagDays  > 3 THEN 1 ELSE 0 END)                  AS LateValueCount,
    CASE WHEN Scope = 'ARCHIVE' THEN 'HISTORICAL'
         WHEN AVG(CAST(IngestLagDays AS DECIMAL(38,4))) >= 5 OR AVG(CAST(ValueLagDays AS DECIMAL(38,4))) >= 5 THEN 'CRITICAL_SETTLEMENT_DELAY'
         WHEN AVG(CAST(IngestLagDays AS DECIMAL(38,4))) >= 2 OR AVG(CAST(ValueLagDays AS DECIMAL(38,4))) >= 2 THEN 'ELEVATED_LAG'
         ELSE 'NORMAL' END AS LagFlag,
    CASE WHEN Scope = 'ARCHIVE' THEN 'P5'
         WHEN AVG(CAST(IngestLagDays AS DECIMAL(38,4))) >= 5 OR AVG(CAST(ValueLagDays AS DECIMAL(38,4))) >= 5 THEN 'P1'
         WHEN AVG(CAST(IngestLagDays AS DECIMAL(38,4))) >= 2 OR AVG(CAST(ValueLagDays AS DECIMAL(38,4))) >= 2 THEN 'P3'
         ELSE 'P5' END AS Urgency,
    CASE WHEN Scope = 'ARCHIVE' THEN 'HISTORICAL_TREND_ANALYSIS_ONLY'
         WHEN AVG(CAST(IngestLagDays AS DECIMAL(38,4))) >= 5 OR AVG(CAST(ValueLagDays AS DECIMAL(38,4))) >= 5 THEN 'ESCALATE_SETTLEMENT_PIPELINE_BREACH'
         WHEN AVG(CAST(IngestLagDays AS DECIMAL(38,4))) >= 2 OR AVG(CAST(ValueLagDays AS DECIMAL(38,4))) >= 2 THEN 'REVIEW_INGEST_OR_VALUE_LAG_TREND'
         ELSE 'NONE' END AS RecommendedAction
FROM Calc
GROUP BY Scope, Network;
GO


-- G7. Currency / FX drift
CREATE OR ALTER VIEW [Reporting].[VwCurrencyFxDrift] AS
WITH Src AS (
    SELECT 'LIVE' AS Scope, 'BKM' AS Network, OriginalCurrency, SettlementCurrency, OriginalAmount, SettlementAmount FROM [Ingestion].[CardBkmDetail]
    UNION ALL SELECT 'LIVE','VISA', OriginalCurrency, SettlementCurrency, OriginalAmount, SettlementAmount FROM [Ingestion].[CardVisaDetail]
    UNION ALL SELECT 'LIVE','MSC',  OriginalCurrency, SettlementCurrency, OriginalAmount, SettlementAmount FROM [Ingestion].[CardMscDetail]
    UNION ALL SELECT 'ARCHIVE','BKM', OriginalCurrency, SettlementCurrency, OriginalAmount, SettlementAmount FROM [Archive].[IngestionCardBkmDetail]
    UNION ALL SELECT 'ARCHIVE','VISA',OriginalCurrency, SettlementCurrency, OriginalAmount, SettlementAmount FROM [Archive].[IngestionCardVisaDetail]
    UNION ALL SELECT 'ARCHIVE','MSC', OriginalCurrency, SettlementCurrency, OriginalAmount, SettlementAmount FROM [Archive].[IngestionCardMscDetail]
)
SELECT
    Scope AS DataScope, Network,
    CAST(OriginalCurrency AS VARCHAR(8))    AS OriginalCurrency,
    CAST(SettlementCurrency AS VARCHAR(8))  AS SettlementCurrency,
    COUNT(*)                                                  AS TransactionCount,
    SUM(OriginalAmount)                                       AS TotalOriginalAmount,
    SUM(SettlementAmount)                                     AS TotalSettlementAmount,
    SUM(SettlementAmount) - SUM(OriginalAmount)               AS DriftAmount,
    CASE WHEN SUM(OriginalAmount) <> 0
         THEN ROUND((SUM(SettlementAmount) - SUM(OriginalAmount)) * 100.0 / NULLIF(SUM(OriginalAmount), 0), 4) ELSE 0 END AS DriftPct,
    CASE WHEN Scope = 'ARCHIVE' THEN 'HISTORICAL'
         WHEN OriginalCurrency = SettlementCurrency THEN 'NO_FX'
         WHEN ABS(SUM(SettlementAmount) - SUM(OriginalAmount)) >= 100000 THEN 'HIGH_FX_DRIFT'
         WHEN ABS(SUM(SettlementAmount) - SUM(OriginalAmount)) >= 10000  THEN 'MODERATE_FX_DRIFT'
         ELSE 'STABLE' END AS DriftFlag,
    CASE WHEN Scope = 'ARCHIVE' THEN 'P5'
         WHEN OriginalCurrency = SettlementCurrency THEN 'P5'
         WHEN ABS(SUM(SettlementAmount) - SUM(OriginalAmount)) >= 100000 THEN 'P2'
         WHEN ABS(SUM(SettlementAmount) - SUM(OriginalAmount)) >= 10000  THEN 'P3'
         ELSE 'P5' END AS Urgency,
    CASE WHEN Scope = 'ARCHIVE' THEN 'HISTORICAL_TREND_ANALYSIS_ONLY'
         WHEN OriginalCurrency = SettlementCurrency THEN 'NONE'
         WHEN ABS(SUM(SettlementAmount) - SUM(OriginalAmount)) >= 100000 THEN 'INVESTIGATE_FX_RATE_OR_HEDGING'
         WHEN ABS(SUM(SettlementAmount) - SUM(OriginalAmount)) >= 10000  THEN 'MONITOR_FX_DRIFT'
         ELSE 'NONE' END AS RecommendedAction
FROM Src
GROUP BY Scope, Network, OriginalCurrency, SettlementCurrency;
GO


-- G8. Installment portfolio summary
CREATE OR ALTER VIEW [Reporting].[VwInstallmentPortfolioSummary] AS
WITH Src AS (
    SELECT 'LIVE' AS Scope, 'BKM' AS Network, InstallCount, OriginalAmount FROM [Ingestion].[CardBkmDetail]
    UNION ALL SELECT 'LIVE','VISA', InstallCount, OriginalAmount FROM [Ingestion].[CardVisaDetail]
    UNION ALL SELECT 'LIVE','MSC',  InstallCount, OriginalAmount FROM [Ingestion].[CardMscDetail]
    UNION ALL SELECT 'ARCHIVE','BKM', InstallCount, OriginalAmount FROM [Archive].[IngestionCardBkmDetail]
    UNION ALL SELECT 'ARCHIVE','VISA',InstallCount, OriginalAmount FROM [Archive].[IngestionCardVisaDetail]
    UNION ALL SELECT 'ARCHIVE','MSC', InstallCount, OriginalAmount FROM [Archive].[IngestionCardMscDetail]
)
SELECT
    Scope AS DataScope, Network,
    CASE WHEN InstallCount IS NULL OR InstallCount <= 1 THEN 'SINGLE_PAYMENT'
         WHEN InstallCount BETWEEN 2 AND 3              THEN 'SHORT_TERM_2_3'
         WHEN InstallCount BETWEEN 4 AND 6              THEN 'MEDIUM_TERM_4_6'
         WHEN InstallCount BETWEEN 7 AND 12             THEN 'LONG_TERM_7_12'
         ELSE 'EXTENDED_13_PLUS' END AS InstallmentBucket,
    COUNT(*)                AS TransactionCount,
    SUM(OriginalAmount)     AS TotalAmount,
    AVG(CAST(OriginalAmount AS DECIMAL(38,4))) AS AvgAmount,
    CASE WHEN Scope = 'ARCHIVE' THEN 'HISTORICAL'
         WHEN MAX(InstallCount) >= 13 AND SUM(OriginalAmount) >= 1000000 THEN 'HIGH_EXTENDED_INSTALLMENT_RISK'
         WHEN MAX(InstallCount) >= 13 THEN 'EXTENDED_INSTALLMENT_PRESENT'
         WHEN MAX(InstallCount) >= 7  THEN 'LONG_TERM_PRESENT'
         ELSE 'SHORT_TERM_PORTFOLIO' END AS PortfolioFlag,
    CASE WHEN Scope = 'ARCHIVE' THEN 'P5'
         WHEN MAX(InstallCount) >= 13 AND SUM(OriginalAmount) >= 1000000 THEN 'P2'
         WHEN MAX(InstallCount) >= 13 THEN 'P3'
         WHEN MAX(InstallCount) >= 7  THEN 'P4'
         ELSE 'P5' END AS Urgency,
    CASE WHEN Scope = 'ARCHIVE' THEN 'HISTORICAL_TREND_ANALYSIS_ONLY'
         WHEN MAX(InstallCount) >= 13 AND SUM(OriginalAmount) >= 1000000 THEN 'REVIEW_EXTENDED_INSTALLMENT_RISK'
         WHEN MAX(InstallCount) >= 13 THEN 'MONITOR_EXTENDED_INSTALLMENT_PORTFOLIO'
         WHEN MAX(InstallCount) >= 7  THEN 'TRACK_LONG_TERM_INSTALLMENT_GROWTH'
         ELSE 'NONE' END AS RecommendedAction
FROM Src
GROUP BY Scope, Network,
         CASE WHEN InstallCount IS NULL OR InstallCount <= 1 THEN 'SINGLE_PAYMENT'
              WHEN InstallCount BETWEEN 2 AND 3              THEN 'SHORT_TERM_2_3'
              WHEN InstallCount BETWEEN 4 AND 6              THEN 'MEDIUM_TERM_4_6'
              WHEN InstallCount BETWEEN 7 AND 12             THEN 'LONG_TERM_7_12'
              ELSE 'EXTENDED_13_PLUS' END;
GO


-- G9. Loyalty points economy
CREATE OR ALTER VIEW [Reporting].[VwLoyaltyPointsEconomy] AS
WITH Src AS (
    SELECT 'LIVE' AS Scope, 'BKM' AS Network, BcPointAmount, McPointAmount, CcPointAmount, OriginalAmount FROM [Ingestion].[CardBkmDetail]
    UNION ALL SELECT 'LIVE','VISA', BcPointAmount, McPointAmount, CcPointAmount, OriginalAmount FROM [Ingestion].[CardVisaDetail]
    UNION ALL SELECT 'LIVE','MSC',  BcPointAmount, McPointAmount, CcPointAmount, OriginalAmount FROM [Ingestion].[CardMscDetail]
    UNION ALL SELECT 'ARCHIVE','BKM', BcPointAmount, McPointAmount, CcPointAmount, OriginalAmount FROM [Archive].[IngestionCardBkmDetail]
    UNION ALL SELECT 'ARCHIVE','VISA',BcPointAmount, McPointAmount, CcPointAmount, OriginalAmount FROM [Archive].[IngestionCardVisaDetail]
    UNION ALL SELECT 'ARCHIVE','MSC', BcPointAmount, McPointAmount, CcPointAmount, OriginalAmount FROM [Archive].[IngestionCardMscDetail]
)
SELECT
    Scope AS DataScope, Network,
    COUNT(*)                                                                            AS TransactionCount,
    SUM(ISNULL(BcPointAmount, 0))                                                       AS TotalBcPoints,
    SUM(ISNULL(McPointAmount, 0))                                                       AS TotalMcPoints,
    SUM(ISNULL(CcPointAmount, 0))                                                       AS TotalCcPoints,
    SUM(ISNULL(BcPointAmount, 0) + ISNULL(McPointAmount, 0) + ISNULL(CcPointAmount, 0)) AS TotalPointAmount,
    SUM(OriginalAmount)                                                                 AS TotalSpendAmount,
    CASE WHEN SUM(OriginalAmount) > 0
         THEN ROUND(SUM(ISNULL(BcPointAmount, 0) + ISNULL(McPointAmount, 0) + ISNULL(CcPointAmount, 0)) * 100.0
                    / NULLIF(SUM(OriginalAmount), 0), 4)
         ELSE 0 END AS PointToSpendRatioPct,
    CASE WHEN Scope = 'ARCHIVE' THEN 'HISTORICAL'
         WHEN SUM(ISNULL(BcPointAmount, 0) + ISNULL(McPointAmount, 0) + ISNULL(CcPointAmount, 0))
              / NULLIF(SUM(OriginalAmount), 0) >= 0.05 THEN 'HIGH_POINT_LIABILITY'
         WHEN SUM(ISNULL(BcPointAmount, 0) + ISNULL(McPointAmount, 0) + ISNULL(CcPointAmount, 0)) > 0 THEN 'ACTIVE_LOYALTY_PROGRAM'
         ELSE 'NO_POINT_ACCRUAL' END AS LoyaltyFlag,
    CASE WHEN Scope = 'ARCHIVE' THEN 'P5'
         WHEN SUM(ISNULL(BcPointAmount, 0) + ISNULL(McPointAmount, 0) + ISNULL(CcPointAmount, 0))
              / NULLIF(SUM(OriginalAmount), 0) >= 0.05 THEN 'P2'
         WHEN SUM(ISNULL(BcPointAmount, 0) + ISNULL(McPointAmount, 0) + ISNULL(CcPointAmount, 0)) > 0 THEN 'P4'
         ELSE 'P5' END AS Urgency,
    CASE WHEN Scope = 'ARCHIVE' THEN 'HISTORICAL_TREND_ANALYSIS_ONLY'
         WHEN SUM(ISNULL(BcPointAmount, 0) + ISNULL(McPointAmount, 0) + ISNULL(CcPointAmount, 0))
              / NULLIF(SUM(OriginalAmount), 0) >= 0.05 THEN 'REVIEW_LOYALTY_LIABILITY_AND_BURN_RATE'
         WHEN SUM(ISNULL(BcPointAmount, 0) + ISNULL(McPointAmount, 0) + ISNULL(CcPointAmount, 0)) > 0 THEN 'TRACK_LOYALTY_PROGRAM_HEALTH'
         ELSE 'NONE' END AS RecommendedAction
FROM Src
GROUP BY Scope, Network;
GO


-- G10. Clearing dispute summary
CREATE OR ALTER VIEW [Reporting].[VwClearingDisputeSummary] AS
WITH Src AS (
    SELECT 'LIVE' AS Scope, 'BKM' AS Network, DisputeCode, ReasonCode, ControlStat, SourceAmount, ReimbursementAmount, TxnDate FROM [Ingestion].[ClearingBkmDetail]
    UNION ALL SELECT 'LIVE','VISA', DisputeCode, ReasonCode, ControlStat, SourceAmount, ReimbursementAmount, TxnDate FROM [Ingestion].[ClearingVisaDetail]
    UNION ALL SELECT 'LIVE','MSC',  DisputeCode, ReasonCode, ControlStat, SourceAmount, ReimbursementAmount, TxnDate FROM [Ingestion].[ClearingMscDetail]
    UNION ALL SELECT 'ARCHIVE','BKM', DisputeCode, ReasonCode, ControlStat, SourceAmount, ReimbursementAmount, TxnDate FROM [Archive].[IngestionClearingBkmDetail]
    UNION ALL SELECT 'ARCHIVE','VISA',DisputeCode, ReasonCode, ControlStat, SourceAmount, ReimbursementAmount, TxnDate FROM [Archive].[IngestionClearingVisaDetail]
    UNION ALL SELECT 'ARCHIVE','MSC', DisputeCode, ReasonCode, ControlStat, SourceAmount, ReimbursementAmount, TxnDate FROM [Archive].[IngestionClearingMscDetail]
), Agg AS (
    SELECT Scope, Network,
           ISNULL(CAST(DisputeCode AS VARCHAR(100)), 'NONE') AS DisputeCode,
           ISNULL(ReasonCode, 'NONE')                        AS ReasonCode,
           CAST(ControlStat AS VARCHAR(64))                  AS ControlStat,
           COUNT(*)                                          AS TransactionCount,
           SUM(SourceAmount)                                 AS TotalSourceAmount,
           SUM(ReimbursementAmount)                          AS TotalReimbursementAmount,
           MIN(CASE WHEN TxnDate BETWEEN 19000101 AND 99991231 THEN TRY_CONVERT(DATE, CAST(TxnDate AS VARCHAR(8)), 112) END) AS FirstTxnDate,
           MAX(CASE WHEN TxnDate BETWEEN 19000101 AND 99991231 THEN TRY_CONVERT(DATE, CAST(TxnDate AS VARCHAR(8)), 112) END) AS LastTxnDate
    FROM Src
    GROUP BY Scope, Network,
             ISNULL(CAST(DisputeCode AS VARCHAR(100)), 'NONE'),
             ISNULL(ReasonCode, 'NONE'),
             CAST(ControlStat AS VARCHAR(64))
)
SELECT
    Scope AS DataScope, Network, DisputeCode, ReasonCode, ControlStat,
    TransactionCount, TotalSourceAmount, TotalReimbursementAmount, FirstTxnDate, LastTxnDate,
    CASE WHEN Scope = 'ARCHIVE' THEN 'HISTORICAL'
         WHEN DisputeCode <> 'NONE' AND TotalReimbursementAmount >= 100000 THEN 'HIGH_DISPUTE_EXPOSURE'
         WHEN DisputeCode <> 'NONE'                                        THEN 'ACTIVE_DISPUTE'
         ELSE 'CLEAN' END AS DisputeFlag,
    CASE WHEN Scope = 'ARCHIVE' THEN 'P5'
         WHEN DisputeCode <> 'NONE' AND TotalReimbursementAmount >= 100000 THEN 'P1'
         WHEN DisputeCode <> 'NONE'                                        THEN 'P3'
         ELSE 'P5' END AS Urgency,
    CASE WHEN Scope = 'ARCHIVE' THEN 'HISTORICAL_TREND_ANALYSIS_ONLY'
         WHEN DisputeCode <> 'NONE' AND TotalReimbursementAmount >= 100000 THEN 'ESCALATE_DISPUTE_RESOLUTION'
         WHEN DisputeCode <> 'NONE'                                        THEN 'WORK_DISPUTE_QUEUE'
         ELSE 'NONE' END AS RecommendedAction
FROM Agg;
GO


-- G11. Daily clearing incoming vs outgoing imbalance
CREATE OR ALTER VIEW [Reporting].[VwClearingIoImbalance] AS
WITH Src AS (
    SELECT 'LIVE' AS Scope, 'BKM' AS Network, TxnDate, IoFlag, SourceAmount FROM [Ingestion].[ClearingBkmDetail]
    UNION ALL SELECT 'LIVE','VISA', TxnDate, IoFlag, SourceAmount FROM [Ingestion].[ClearingVisaDetail]
    UNION ALL SELECT 'LIVE','MSC',  TxnDate, IoFlag, SourceAmount FROM [Ingestion].[ClearingMscDetail]
    UNION ALL SELECT 'ARCHIVE','BKM', TxnDate, IoFlag, SourceAmount FROM [Archive].[IngestionClearingBkmDetail]
    UNION ALL SELECT 'ARCHIVE','VISA',TxnDate, IoFlag, SourceAmount FROM [Archive].[IngestionClearingVisaDetail]
    UNION ALL SELECT 'ARCHIVE','MSC', TxnDate, IoFlag, SourceAmount FROM [Archive].[IngestionClearingMscDetail]
)
SELECT
    Scope AS DataScope, Network,
    TRY_CONVERT(DATE, CAST(TxnDate AS VARCHAR(8)), 112) AS TxnDate,
    SUM(CASE WHEN IoFlag = 'In'  THEN 1 ELSE 0 END)                  AS IncomingCount,
    SUM(CASE WHEN IoFlag = 'Out' THEN 1 ELSE 0 END)                  AS OutgoingCount,
    SUM(CASE WHEN IoFlag = 'In'  THEN SourceAmount ELSE 0 END)       AS IncomingAmount,
    SUM(CASE WHEN IoFlag = 'Out' THEN SourceAmount ELSE 0 END)       AS OutgoingAmount,
    SUM(CASE WHEN IoFlag = 'In'  THEN SourceAmount ELSE 0 END)
        - SUM(CASE WHEN IoFlag = 'Out' THEN SourceAmount ELSE 0 END) AS NetImbalanceAmount,
    CASE WHEN Scope = 'ARCHIVE' THEN 'HISTORICAL'
         WHEN ABS(SUM(CASE WHEN IoFlag = 'In' THEN SourceAmount ELSE 0 END)
                  - SUM(CASE WHEN IoFlag = 'Out' THEN SourceAmount ELSE 0 END)) >= 1000000 THEN 'CRITICAL_IMBALANCE'
         WHEN ABS(SUM(CASE WHEN IoFlag = 'In' THEN SourceAmount ELSE 0 END)
                  - SUM(CASE WHEN IoFlag = 'Out' THEN SourceAmount ELSE 0 END)) >= 100000  THEN 'NOTABLE_IMBALANCE'
         ELSE 'BALANCED' END AS ImbalanceFlag,
    CASE WHEN Scope = 'ARCHIVE' THEN 'P5'
         WHEN ABS(SUM(CASE WHEN IoFlag = 'In' THEN SourceAmount ELSE 0 END)
                  - SUM(CASE WHEN IoFlag = 'Out' THEN SourceAmount ELSE 0 END)) >= 1000000 THEN 'P1'
         WHEN ABS(SUM(CASE WHEN IoFlag = 'In' THEN SourceAmount ELSE 0 END)
                  - SUM(CASE WHEN IoFlag = 'Out' THEN SourceAmount ELSE 0 END)) >= 100000  THEN 'P3'
         ELSE 'P5' END AS Urgency,
    CASE WHEN Scope = 'ARCHIVE' THEN 'HISTORICAL_TREND_ANALYSIS_ONLY'
         WHEN ABS(SUM(CASE WHEN IoFlag = 'In' THEN SourceAmount ELSE 0 END)
                  - SUM(CASE WHEN IoFlag = 'Out' THEN SourceAmount ELSE 0 END)) >= 1000000 THEN 'ESCALATE_CLEARING_RECONCILIATION'
         WHEN ABS(SUM(CASE WHEN IoFlag = 'In' THEN SourceAmount ELSE 0 END)
                  - SUM(CASE WHEN IoFlag = 'Out' THEN SourceAmount ELSE 0 END)) >= 100000  THEN 'INVESTIGATE_DAILY_IMBALANCE'
         ELSE 'NONE' END AS RecommendedAction
FROM Src
WHERE TxnDate BETWEEN 19000101 AND 99991231
GROUP BY Scope, Network, TRY_CONVERT(DATE, CAST(TxnDate AS VARCHAR(8)), 112);
GO


-- G12. High-value unmatched transactions (>= 100k) - row-level alarms
CREATE OR ALTER VIEW [Reporting].[VwHighValueUnmatchedTransactions] AS
WITH Src AS (
    SELECT 'LIVE' AS Scope, 'BKM' AS Network, d.Id AS DetailId, d.OriginalAmount, d.OriginalCurrency, d.MerchantId, d.MerchantName, d.MerchantCountry, d.TransactionDate, d.CardNo, d.CreateDate
    FROM [Ingestion].[CardBkmDetail] d JOIN [Ingestion].[FileLine] fl ON fl.Id = d.FileLineId
    WHERE fl.MatchedClearingLineId IS NULL AND d.OriginalAmount >= 100000
    UNION ALL
    SELECT 'LIVE','VISA', d.Id, d.OriginalAmount, d.OriginalCurrency, d.MerchantId, d.MerchantName, d.MerchantCountry, d.TransactionDate, d.CardNo, d.CreateDate
    FROM [Ingestion].[CardVisaDetail] d JOIN [Ingestion].[FileLine] fl ON fl.Id = d.FileLineId
    WHERE fl.MatchedClearingLineId IS NULL AND d.OriginalAmount >= 100000
    UNION ALL
    SELECT 'LIVE','MSC', d.Id, d.OriginalAmount, d.OriginalCurrency, d.MerchantId, d.MerchantName, d.MerchantCountry, d.TransactionDate, d.CardNo, d.CreateDate
    FROM [Ingestion].[CardMscDetail] d JOIN [Ingestion].[FileLine] fl ON fl.Id = d.FileLineId
    WHERE fl.MatchedClearingLineId IS NULL AND d.OriginalAmount >= 100000
    UNION ALL
    SELECT 'ARCHIVE','BKM', d.Id, d.OriginalAmount, d.OriginalCurrency, d.MerchantId, d.MerchantName, d.MerchantCountry, d.TransactionDate, d.CardNo, d.CreateDate
    FROM [Archive].[IngestionCardBkmDetail] d JOIN [Archive].[IngestionFileLine] fl ON fl.Id = d.FileLineId
    WHERE fl.MatchedClearingLineId IS NULL AND d.OriginalAmount >= 100000
    UNION ALL
    SELECT 'ARCHIVE','VISA', d.Id, d.OriginalAmount, d.OriginalCurrency, d.MerchantId, d.MerchantName, d.MerchantCountry, d.TransactionDate, d.CardNo, d.CreateDate
    FROM [Archive].[IngestionCardVisaDetail] d JOIN [Archive].[IngestionFileLine] fl ON fl.Id = d.FileLineId
    WHERE fl.MatchedClearingLineId IS NULL AND d.OriginalAmount >= 100000
    UNION ALL
    SELECT 'ARCHIVE','MSC', d.Id, d.OriginalAmount, d.OriginalCurrency, d.MerchantId, d.MerchantName, d.MerchantCountry, d.TransactionDate, d.CardNo, d.CreateDate
    FROM [Archive].[IngestionCardMscDetail] d JOIN [Archive].[IngestionFileLine] fl ON fl.Id = d.FileLineId
    WHERE fl.MatchedClearingLineId IS NULL AND d.OriginalAmount >= 100000
)
SELECT
    Scope AS DataScope, Network, DetailId,
    OriginalAmount, CAST(OriginalCurrency AS VARCHAR(8)) AS OriginalCurrency,
    ISNULL(MerchantId, 'UNKNOWN')      AS MerchantId,
    ISNULL(MerchantName, 'UNKNOWN')    AS MerchantName,
    ISNULL(MerchantCountry, 'UNKNOWN') AS MerchantCountry,
    TRY_CONVERT(DATE, CAST(TransactionDate AS VARCHAR(8)), 112) AS TransactionDate,
    CASE WHEN CardNo IS NOT NULL AND LEN(CardNo) >= 4 THEN '****' + RIGHT(CardNo, 4) ELSE NULL END AS CardLast4,
    CreateDate AS IngestedAt,
    DATEDIFF(DAY,
             TRY_CONVERT(DATE, CAST(TransactionDate AS VARCHAR(8)), 112),
             CAST(CreateDate AS DATE)) AS AgeDays,
    CASE WHEN Scope = 'ARCHIVE'                         THEN 'HISTORICAL'
         WHEN OriginalAmount >= 1000000                 THEN 'CRITICAL_UNMATCHED'
         WHEN OriginalAmount >= 500000                  THEN 'HIGH_VALUE_UNMATCHED'
         ELSE 'ELEVATED_UNMATCHED' END AS AlarmFlag,
    CASE WHEN Scope = 'ARCHIVE'         THEN 'P5'
         WHEN OriginalAmount >= 1000000 THEN 'P1'
         WHEN OriginalAmount >= 500000  THEN 'P2'
         ELSE 'P3' END AS Urgency,
    CASE WHEN Scope = 'ARCHIVE'         THEN 'HISTORICAL_TREND_ANALYSIS_ONLY'
         WHEN OriginalAmount >= 1000000 THEN 'IMMEDIATE_RECONCILIATION_REQUIRED'
         WHEN OriginalAmount >= 500000  THEN 'PRIORITY_RECONCILIATION_REQUIRED'
         ELSE 'INVESTIGATE_HIGH_VALUE_UNMATCHED' END AS RecommendedAction
FROM Src
WHERE TransactionDate BETWEEN 19000101 AND 99991231;
GO



-- =====================================================================================
-- [Reporting].[VwReportingDocumentation] source (parallel to reporting.rep_documentation).
-- Contains entries for every [Reporting].[Vw*] view present in this migration; column
-- names are PascalCase per project convention.
-- =====================================================================================
IF OBJECT_ID('[Reporting].[VwReportingDocumentation]', 'V') IS NOT NULL
    DROP VIEW [Reporting].[VwReportingDocumentation];
GO

CREATE VIEW [Reporting].[VwReportingDocumentation] AS
SELECT d.ViewName, d.ReportGroup,
       d.PurposeTr, d.PurposeEn,
       d.BusinessQuestionTr, d.BusinessQuestionEn,
       d.InterpretationTr, d.InterpretationEn,
       d.UsageTimeTr, d.UsageTimeEn,
       d.TargetUserTr, d.TargetUserEn,
       d.ActionGuidanceTr, d.ActionGuidanceEn,
       d.ImportantColumnsTr, d.ImportantColumnsEn,
       d.LiveArchiveInterpretationTr, d.LiveArchiveInterpretationEn,
       d.NotesTr, d.NotesEn
FROM (VALUES
    ('[Reporting].[VwHighValueUnmatchedTransactions]', 'FINANCIAL_RISK', 'Tek tek 100k+ tutarli eslesmemis islemleri merchant adi, kanal ve PAN-mask edilmis kart bilgisi ile listeler; tek-noktali yuksek riski hedefler.', 'Lists individual unmatched transactions of 100k+ with merchant name, channel and PAN-masked card; targets single-item high risk.', 'Yuksek tutarli hangi tek tek islemler hala beklemede?', 'Which individual high-value transactions are still pending match?', 'risk_flag CRITICAL_HIGH_VALUE_UNMATCHED 1M+ islemdir, derhal incelenmelidir; HIGH_VALUE_UNMATCHED ikinci sira riskidir.', 'CRITICAL_HIGH_VALUE_UNMATCHED risk_flag is a 1M+ transaction; inspect immediately. HIGH_VALUE_UNMATCHED is the second-tier risk.', 'Gunluk finansal kapanis sonrasi.', 'After the daily financial close.', 'Recon, finans, risk, KYC ekibi.', 'Recon, finance, risk, KYC team.', 'CRITICAL_HIGH_VALUE_UNMATCHED satirlari risk komitesine eskale, gerekirse manuel hareketle deniklestir; HIGH_VALUE_UNMATCHED operasyon ekibinde isleme alinir.', 'Escalate CRITICAL_HIGH_VALUE_UNMATCHED to the risk committee, balance manually if required; HIGH_VALUE_UNMATCHED is processed by operations.', 'detail_id, transaction_date, original_amount, merchant_name, card_mask, risk_flag', 'detail_id, transaction_date, original_amount, merchant_name, card_mask, risk_flag', 'LIVE: aktif yuksek tutarli risk. ARCHIVE: tarihi yuksek tutarli profil.', 'LIVE: active high-value risk. ARCHIVE: historical high-value profile.', 'Kart numarasi PAN-mask edilir (ilk6 + **** + son4); 100k esiginin altindaki islemler dahil edilmez.', 'Card number is PAN-masked (first6 + **** + last4); transactions below the 100k threshold are excluded.'),
    ('[Reporting].[VwDailyTransactionVolume]', 'FINANCIAL_VOLUME', 'Islem tarihine (transaction_date) gore network/financial_type/txn_effect/currency bazinda gunluk hacim, debit/credit ve net akis.', 'Daily volume per network/financial_type/txn_effect/currency using real business transaction_date with debit, credit and net flow.', 'Hangi guneki finansal hacim ne kadardi, anormal bir tepe ya da cukur var mi?', 'What was the daily financial volume, are there abnormal peaks or troughs?', 'volume_flag MATERIAL_NET_FLOW net akis 1M esigini astigini gosterir, denetim gerekir; NORMAL_VOLUME ise olagan dagilimdir.', 'MATERIAL_NET_FLOW volume_flag means net flow exceeds the 1M threshold and warrants review; NORMAL_VOLUME indicates ordinary distribution.', 'Gunluk finansal kapanis ve haftalik trend incelemesi.', 'Daily financial close and weekly trend review.', 'Finans, recon, yonetim.', 'Finance, recon, management.', 'Anormal net akis tespit edilirse o gunun yuksek tutarli islemleri (rep_high_value_unmatched_transactions) ve cesidi (network/currency) ile incele.', 'On abnormal net flow drill into that day high-value transactions (rep_high_value_unmatched_transactions) and breakdown (network/currency).', 'transaction_date, network, currency, debit_amount, credit_amount, net_flow_amount, volume_flag', 'transaction_date, network, currency, debit_amount, credit_amount, net_flow_amount, volume_flag', 'LIVE: bugunku/dunku gercek tarihler. ARCHIVE: tarihsel hacim profili ve trend.', 'LIVE: today/yesterday actuals. ARCHIVE: historical volume profile and trend.', 'transaction_date int4 YYYYMMDD formatindadir; out-of-range degerler atlanir (1900-9999).', 'transaction_date is int4 YYYYMMDD; out-of-range values are skipped (1900-9999).'),
    ('[Reporting].[VwMccRevenueConcentration]', 'FINANCIAL_CONCENTRATION', 'MCC bazinda hacim payi, vergi/komisyon, surcharge ve cashback ekonomisi; konsantrasyon riski etiketi.', 'Per-MCC volume share, tax/commission, surcharge and cashback economics with concentration risk flag.', 'Hacmimiz hangi MCC lere bagli, tek bir MCC ye asiri bagimliyiz?', 'Which MCCs drive our volume, are we over-dependent on a single one?', 'concentration_risk HIGH_CONCENTRATION MCC payinin %30 unu astigini gosterir; is surekliligi ve gelir cesitlendirme riskidir.', 'HIGH_CONCENTRATION concentration_risk means an MCC exceeds 30% share; a business-continuity and revenue diversification risk.', 'Aylik portfoy degerlendirmesi.', 'Monthly portfolio review.', 'Risk, finans, ticari ekipler.', 'Risk, finance, commercial teams.', 'HIGH_CONCENTRATION MCC ler portfoy diversifikasyonu calismasina alinir; ticari ekiple yeni MCC kazanimi planlanir.', 'HIGH_CONCENTRATION MCCs are sent to portfolio diversification work; new MCC acquisition is planned with the commercial team.', 'mcc, volume_share_pct, total_tax_amount, total_cashback_amount, concentration_risk', 'mcc, volume_share_pct, total_tax_amount, total_cashback_amount, concentration_risk', 'LIVE: guncel portfoy. ARCHIVE: tarihsel konsantrasyon trendi.', 'LIVE: current portfolio. ARCHIVE: historical concentration trend.', 'Vergi geliri (tax1+tax2) ve cashback gideri ayni satirda goruldugu icin net katki hesaplanabilir.', 'Tax revenue (tax1+tax2) and cashback cost are visible side by side; net contribution is computable.'),
    ('[Reporting].[VwMerchantRiskHotspots]', 'FINANCIAL_RISK', 'Merchant bazinda decline orani, eslesmemis is orani ve eslesmesim is finansal etki ile risk siniflandirmasi (HIGH_RISK / MEDIUM_RISK / LOW_RISK / HIGH_DECLINE).', 'Per-merchant decline rate, unmatched rate and unmatched financial impact with risk classification (HIGH_RISK / MEDIUM_RISK / LOW_RISK / HIGH_DECLINE).', 'Hangi merchant lar yuksek risk tasiyor, hangi merchant da decline patlamasi var?', 'Which merchants carry high risk, where is decline spiking?', 'HIGH_RISK_MERCHANT %20+ unmatched ve 100k+ tutar; HIGH_DECLINE_MERCHANT decline ozellikle yogun.', 'HIGH_RISK_MERCHANT means 20%+ unmatched and 100k+ amount; HIGH_DECLINE_MERCHANT means decline is particularly intense.', 'Haftalik merchant risk degerlendirmesi.', 'Weekly merchant risk review.', 'Risk, ticari, finans.', 'Risk, commercial, finance.', 'HIGH_RISK_MERCHANT ler risk ekibine eskale; HIGH_DECLINE_MERCHANT lar icin decline pattern (issuer/limit/3DS) incelenir.', 'Escalate HIGH_RISK_MERCHANT to the risk team; for HIGH_DECLINE_MERCHANT investigate decline pattern (issuer/limit/3DS).', 'merchant_id, decline_rate_pct, unmatched_rate_pct, unmatched_amount, risk_flag', 'merchant_id, decline_rate_pct, unmatched_rate_pct, unmatched_amount, risk_flag', 'LIVE: aktif merchant riski. ARCHIVE: gecmis profili.', 'LIVE: active merchant risk. ARCHIVE: historical profile.', 'Decline orani response_code <> 00 veya is_successful_txn = N kombinasyonuyla hesaplanir.', 'Decline rate is computed from response_code <> 00 or is_successful_txn = N.'),
    ('[Reporting].[VwCountryCrossBorderExposure]', 'FINANCIAL_RISK', 'Merchant ulkesi ve original/settlement currency esitsizligine gore yurt disi ve FX maruziyetini gosterir.', 'Surfaces cross-border and FX exposure by merchant_country and original-vs-settlement currency.', 'Yurt disi ve farkli para birimi islemlerine ne kadar maruziz?', 'How exposed are we to cross-border and cross-currency transactions?', 'exposure_flag HIGH_FX_EXPOSURE yuksek tutarli FX maruziyetidir, hedge degerlendirilmelidir.', 'HIGH_FX_EXPOSURE exposure_flag indicates large FX exposure; consider hedging.', 'Aylik FX risk degerlendirmesi.', 'Monthly FX risk review.', 'Hazine, finans, risk.', 'Treasury, finance, risk.', 'HIGH_FX_EXPOSURE icin hedging politikasi gozden gecirilir; ulke bazinda diversifikasyon planlanir.', 'Review hedging policy on HIGH_FX_EXPOSURE; plan country-level diversification.', 'merchant_country, fx_pattern, total_original_amount, exposure_flag', 'merchant_country, fx_pattern, total_original_amount, exposure_flag', 'LIVE: guncel exposure. ARCHIVE: tarihsel FX riski.', 'LIVE: current exposure. ARCHIVE: historical FX risk.', 'Currency kodlari ISO 4217 numerik (orn 949 = TRY) olabilir.', 'Currency codes are likely ISO 4217 numeric (e.g. 949 = TRY).'),
    ('[Reporting].[VwResponseCodeDeclineHealth]', 'AUTHORIZATION_HEALTH', 'Network bazinda response_code dagilimi, basarisizlik orani ve baskin red sebepleri.', 'Per-network response_code distribution with failure rate and dominant decline reasons.', 'Iadeler ve redler hangi response_code dan geliyor?', 'Which response_codes are driving declines and refusals?', 'DOMINANT_FAILURE_REASON %5 ustu paya sahip bir red sebebi vardir, kaynak arastirilmalidir; HEALTHY normaldir.', 'DOMINANT_FAILURE_REASON means a decline reason exceeds 5% share; investigate the root cause; HEALTHY is normal.', 'Gunluk operasyonel takip.', 'Daily operational monitoring.', 'Operasyon, urun, risk.', 'Operations, product, risk.', 'DOMINANT_FAILURE_REASON satirlari icin kaynak (issuer/network/limit) arastirilir; gerekirse 3DS/limit ayarlari guncellenir.', 'Investigate the source (issuer/network/limit) for DOMINANT_FAILURE_REASON rows; update 3DS/limit settings if needed.', 'network, response_code, failure_rate_pct, network_share_pct, health_flag', 'network, response_code, failure_rate_pct, network_share_pct, health_flag', 'LIVE: anlik authorization sagligi. ARCHIVE: tarihsel red profili.', 'LIVE: real-time authorization health. ARCHIVE: historical decline profile.', 'response_code = 00 basari kabul edilir; NONE veri eksikligini gosterir.', 'response_code = 00 is treated as success; NONE indicates missing data.'),
    ('[Reporting].[VwSettlementLagAnalysis]', 'OPERATIONS_KPI', 'Islem tarihi ile end_of_day_date, value_date ve dosya alim tarihi arasindaki gecikmeleri olcer.', 'Measures lag between transaction_date and end_of_day_date, value_date, and file ingestion date.', 'Islemler ne kadar zamaninda sisteme dusuyor, hangi noktada gecikme olusuyor?', 'How timely are transactions arriving in the system, where does the delay accumulate?', 'lag_health CHRONIC_INGEST_DELAY yapisal bir alim gecikmesidir; ON_TARGET SLA icindedir.', 'CHRONIC_INGEST_DELAY lag_health means structurally lagging ingestion; ON_TARGET means within SLA.', 'Gunluk SLA takibi.', 'Daily SLA monitoring.', 'Operasyon, SRE, recon.', 'Operations, SRE, recon.', 'CHRONIC_INGEST_DELAY de upstream provider/kanal ile temas kurulur; transport ya da batch zamanlamasi degistirilir.', 'On CHRONIC_INGEST_DELAY contact upstream provider/channel; change transport or batch schedule.', 'channel, avg_lag_to_ingest_days, max_lag_to_ingest_days, late_ingest_count, lag_health', 'channel, avg_lag_to_ingest_days, max_lag_to_ingest_days, late_ingest_count, lag_health', 'LIVE: guncel SLA durumu. ARCHIVE: tarihsel SLA profili.', 'LIVE: current SLA status. ARCHIVE: historical SLA profile.', 'transaction_date, end_of_day_date, value_date YYYYMMDD int4 olarak tutulur.', 'transaction_date, end_of_day_date, value_date are stored as YYYYMMDD int4.'),
    ('[Reporting].[VwCurrencyFxDrift]', 'FINANCIAL_RISK', 'Cross-currency islemlerde original ve settlement tutar farklarini toplayarak FX drift i (kazanc/kayip) gosterir.', 'For cross-currency transactions aggregates the original-vs-settlement amount drift to surface FX gain/loss.', 'FX donusumlerinde sistematik bir kayip ya da kazanc var mi?', 'Is there systematic FX gain/loss in our currency conversions?', 'MATERIAL_DRIFT 100k+ kumulatif drift birikmistir; settlement mantigi ve kur kaynagi denetlenmelidir.', 'MATERIAL_DRIFT means cumulative drift exceeds 100k; review settlement logic and FX rate source.', 'Aylik FX denetimi.', 'Monthly FX audit.', 'Hazine, finans, denetim.', 'Treasury, finance, audit.', 'MATERIAL_DRIFT te kur kaynagi (Reuters/issuer rate) ve settlement formulu denetlenir; gerekirse rezerv ayrilir.', 'On MATERIAL_DRIFT audit the FX rate source (Reuters/issuer rate) and settlement formula; reserve if needed.', 'currency_pair, settlement_drift, billing_drift, fx_drift_severity', 'currency_pair, settlement_drift, billing_drift, fx_drift_severity', 'LIVE: guncel FX drift. ARCHIVE: tarihsel FX drift profili.', 'LIVE: current FX drift. ARCHIVE: historical FX drift profile.', 'Sadece original_currency <> settlement_currency olan satirlar ele alinir.', 'Only original_currency <> settlement_currency rows are considered.'),
    ('[Reporting].[VwInstallmentPortfolioSummary]', 'PORTFOLIO_RISK', 'Network bazinda taksit kovasi (1, 2-3, 4-6, 7-12, 13+) ile portfoy dagilimi ve uzun vadeli maruziyet bayragi.', 'Per-network installment buckets (1, 2-3, 4-6, 7-12, 13+) with portfolio share and long-term exposure flag.', 'Taksitli portfoyumuz ne kadar uzun vadeye yayilmis?', 'How much of our portfolio is in long-term installments?', 'HIGH_LONG_TERM_INSTALLMENT_EXPOSURE 13+ taksit %10 unu astigini gosterir, kredi riski artmistir.', 'HIGH_LONG_TERM_INSTALLMENT_EXPOSURE means 13+ installment exceeds 10%, increasing credit risk.', 'Aylik portfoy degerlendirmesi.', 'Monthly portfolio review.', 'Risk, kredi, finans.', 'Risk, credit, finance.', 'HIGH_LONG_TERM_INSTALLMENT_EXPOSURE durumunda taksit politikasi/skorlama incelenmelidir.', 'Review installment policy/scoring on HIGH_LONG_TERM_INSTALLMENT_EXPOSURE.', 'network, installment_bucket, volume_share_pct, amount_share_pct, portfolio_flag', 'network, installment_bucket, volume_share_pct, amount_share_pct, portfolio_flag', 'LIVE: guncel portfoy. ARCHIVE: tarihsel taksit egilimi.', 'LIVE: current portfolio. ARCHIVE: historical installment trend.', 'install_count detail tabloundaki gercek taksit sayisidir; install_order taksit sirasidir.', 'install_count is the real installment count from detail; install_order is the order index.'),
    ('[Reporting].[VwLoyaltyPointsEconomy]', 'PROGRAM_ECONOMICS', 'Gunluk bazda BC/MC/CC puan tutarlarinin orijinal islem tutarina oranini gosterir; sadakat program maliyetini olcer.', 'Daily BC/MC/CC point amounts vs original transaction amount; measures loyalty program cost.', 'Sadakat programi gunluk olarak ne kadara mal oluyor?', 'How much does the loyalty program cost daily?', 'HIGH_LOYALTY_USAGE %10+ subvansiyon var; program maliyeti gozden gecirilmelidir.', 'HIGH_LOYALTY_USAGE means subsidy exceeds 10%; review program cost.', 'Aylik program degerlendirmesi.', 'Monthly program review.', 'Sadakat, urun, finans.', 'Loyalty, product, finance.', 'HIGH_LOYALTY_USAGE gunlerinde puan kazanim/harcama kurallari ve kampanyalar gozden gecirilir.', 'On HIGH_LOYALTY_USAGE days review point earning/burning rules and campaigns.', 'business_date, total_loyalty_amount, loyalty_to_amount_ratio_pct, loyalty_intensity', 'business_date, total_loyalty_amount, loyalty_to_amount_ratio_pct, loyalty_intensity', 'LIVE: gunluk program maliyeti. ARCHIVE: tarihsel maliyet trendi.', 'LIVE: daily program cost. ARCHIVE: historical cost trend.', 'BC/MC/CC point_amount alanlari TL bazinda tutar olarak saklanir.', 'BC/MC/CC point_amount fields are stored as currency amounts.'),
    ('[Reporting].[VwClearingDisputeSummary]', 'DISPUTES', 'Clearing tarafindaki dispute_code/reason_code/control_stat bazinda islem ve reimbursement tutarlari.', 'Clearing dispute aggregations by dispute_code/reason_code/control_stat with reimbursement amount.', 'Itiraz/chargeback yuku ne kadar, hangi reason_code lar baskin?', 'What is our chargeback load, which reason codes dominate?', 'HIGH_DISPUTE_EXPOSURE reimbursement tutarinin 100k yi astigini gosterir, eskale edilmelidir.', 'HIGH_DISPUTE_EXPOSURE means reimbursement exceeds 100k; escalate.', 'Haftalik dispute degerlendirmesi.', 'Weekly dispute review.', 'Dispute ekibi, finans, denetim.', 'Dispute team, finance, audit.', 'HIGH_DISPUTE_EXPOSURE icin musteri/issuer ile temas plani olusturulur; reason_code bazinda root cause incelenir.', 'For HIGH_DISPUTE_EXPOSURE create a contact plan with customer/issuer; investigate root cause per reason_code.', 'dispute_code, reason_code, control_stat, total_reimbursement_amount, dispute_flag', 'dispute_code, reason_code, control_stat, total_reimbursement_amount, dispute_flag', 'LIVE: aktif disputes. ARCHIVE: tarihsel dispute profili.', 'LIVE: active disputes. ARCHIVE: historical dispute profile.', 'Sadece clearing detay tablolari kullanilir; card detail tarafinda dispute alani yoktur.', 'Only clearing detail tables are used; card detail does not have a dispute field.'),
    ('[Reporting].[VwClearingIoImbalance]', 'CLEARING_FLOW', 'Gunluk clearing incoming/outgoing tutarlari ve net akis dengesizligi.', 'Daily clearing incoming/outgoing amounts and net flow imbalance.', 'Clearing incoming/outgoing dengemiz nasil, net akis ne yonde?', 'How is our incoming/outgoing clearing balance, in which direction is the net flow?', 'MATERIAL_NET_IMBALANCE 1M yi asan net akis demektir; recon veya reconciliation gecikmesi gostergesidir.', 'MATERIAL_NET_IMBALANCE means net flow exceeds 1M; indicates recon delay or imbalance.', 'Gunluk T+1 takibi.', 'Daily T+1 monitoring.', 'Recon, finans.', 'Recon, finance.', 'MATERIAL_NET_IMBALANCE icin gunluk reconciliation kapanis akisi denetlenir.', 'On MATERIAL_NET_IMBALANCE check the daily reconciliation close flow.', 'business_date, incoming_amount, outgoing_amount, net_flow_amount, imbalance_flag', 'business_date, incoming_amount, outgoing_amount, net_flow_amount, imbalance_flag', 'LIVE: guncel akis dengesi. ARCHIVE: tarihsel akis profili.', 'LIVE: current flow balance. ARCHIVE: historical flow profile.', 'io_flag I = incoming, O = outgoing kabul edilir.', 'io_flag I = incoming, O = outgoing.'),
    ('[Reporting].[VwIngestionFileOverview]', 'FILE_PROCESSING', 'Tek dosya icin ozet operasyonel rapor: dosya kimligi, durum, mesaj ve kaynak/tip ozellikleri.', 'Per-file operational summary: file id, status, message, source/type.', 'Bu rapor ne diyor?', 'What does this report tell us?', 'Operasyonel ve finansal anlik durum gostergesidir; trend ile birlikte degerlendirilmelidir.', 'Operational and financial point-in-time indicator; evaluate together with the trend.', 'Gunluk ve haftalik takip.', 'Daily and weekly monitoring.', 'Operasyon, recon, finans.', 'Operations, recon, finance.', 'Anormal sapmalarda ilgili detay raporlarina dril edin; kalici sorunlarda runbook taki adimi izleyin.', 'On abnormal deviations drill into the related detail reports; for persistent issues follow the runbook step.', 'Bkz. view tanimi.', 'See view definition.', 'LIVE: anlik durum; ARCHIVE: tarihsel trend.', 'LIVE: point-in-time; ARCHIVE: historical trend.', 'MS SQL e ozgu eski (legacy) operasyonel view dur; PostgreSQL tarafinda karsiligi yoktur.', 'Legacy SQL Server-only operational view; no PostgreSQL counterpart.'),
    ('[Reporting].[VwIngestionFileQuality]', 'FILE_PROCESSING', 'Dosya bazinda parse/validation hata orani, eksik satir, tekrar eden satir gibi kalite gostergeleri.', 'Per-file parse/validation failure rate, missing lines, duplicates.', 'Bu rapor ne diyor?', 'What does this report tell us?', 'Operasyonel ve finansal anlik durum gostergesidir; trend ile birlikte degerlendirilmelidir.', 'Operational and financial point-in-time indicator; evaluate together with the trend.', 'Gunluk ve haftalik takip.', 'Daily and weekly monitoring.', 'Operasyon, recon, finans.', 'Operations, recon, finance.', 'Anormal sapmalarda ilgili detay raporlarina dril edin; kalici sorunlarda runbook taki adimi izleyin.', 'On abnormal deviations drill into the related detail reports; for persistent issues follow the runbook step.', 'Bkz. view tanimi.', 'See view definition.', 'LIVE: anlik durum; ARCHIVE: tarihsel trend.', 'LIVE: point-in-time; ARCHIVE: historical trend.', 'MS SQL e ozgu eski (legacy) operasyonel view dur; PostgreSQL tarafinda karsiligi yoktur.', 'Legacy SQL Server-only operational view; no PostgreSQL counterpart.'),
    ('[Reporting].[VwIngestionDailySummary]', 'FILE_PROCESSING', 'Gunluk dosya alim hacmi, basari/basarisiz dosya sayisi ve hata orani.', 'Daily ingestion volume, success/failure file counts, failure rate.', 'Bu rapor ne diyor?', 'What does this report tell us?', 'Operasyonel ve finansal anlik durum gostergesidir; trend ile birlikte degerlendirilmelidir.', 'Operational and financial point-in-time indicator; evaluate together with the trend.', 'Gunluk ve haftalik takip.', 'Daily and weekly monitoring.', 'Operasyon, recon, finans.', 'Operations, recon, finance.', 'Anormal sapmalarda ilgili detay raporlarina dril edin; kalici sorunlarda runbook taki adimi izleyin.', 'On abnormal deviations drill into the related detail reports; for persistent issues follow the runbook step.', 'Bkz. view tanimi.', 'See view definition.', 'LIVE: anlik durum; ARCHIVE: tarihsel trend.', 'LIVE: point-in-time; ARCHIVE: historical trend.', 'MS SQL e ozgu eski (legacy) operasyonel view dur; PostgreSQL tarafinda karsiligi yoktur.', 'Legacy SQL Server-only operational view; no PostgreSQL counterpart.'),
    ('[Reporting].[VwIngestionNetworkMatrix]', 'FILE_PROCESSING', 'Network x dosya tipi matrisinde alim sayim ve tutarlari.', 'Counts and amounts in a network x file-type matrix.', 'Bu rapor ne diyor?', 'What does this report tell us?', 'Operasyonel ve finansal anlik durum gostergesidir; trend ile birlikte degerlendirilmelidir.', 'Operational and financial point-in-time indicator; evaluate together with the trend.', 'Gunluk ve haftalik takip.', 'Daily and weekly monitoring.', 'Operasyon, recon, finans.', 'Operations, recon, finance.', 'Anormal sapmalarda ilgili detay raporlarina dril edin; kalici sorunlarda runbook taki adimi izleyin.', 'On abnormal deviations drill into the related detail reports; for persistent issues follow the runbook step.', 'Bkz. view tanimi.', 'See view definition.', 'LIVE: anlik durum; ARCHIVE: tarihsel trend.', 'LIVE: point-in-time; ARCHIVE: historical trend.', 'MS SQL e ozgu eski (legacy) operasyonel view dur; PostgreSQL tarafinda karsiligi yoktur.', 'Legacy SQL Server-only operational view; no PostgreSQL counterpart.'),
    ('[Reporting].[VwIngestionExceptionHotspots]', 'FILE_PROCESSING', 'Alim sirasinda en cok karsilasilan hata tipleri ve sayilari.', 'Top ingestion exception types with counts.', 'Bu rapor ne diyor?', 'What does this report tell us?', 'Operasyonel ve finansal anlik durum gostergesidir; trend ile birlikte degerlendirilmelidir.', 'Operational and financial point-in-time indicator; evaluate together with the trend.', 'Gunluk ve haftalik takip.', 'Daily and weekly monitoring.', 'Operasyon, recon, finans.', 'Operations, recon, finance.', 'Anormal sapmalarda ilgili detay raporlarina dril edin; kalici sorunlarda runbook taki adimi izleyin.', 'On abnormal deviations drill into the related detail reports; for persistent issues follow the runbook step.', 'Bkz. view tanimi.', 'See view definition.', 'LIVE: anlik durum; ARCHIVE: tarihsel trend.', 'LIVE: point-in-time; ARCHIVE: historical trend.', 'MS SQL e ozgu eski (legacy) operasyonel view dur; PostgreSQL tarafinda karsiligi yoktur.', 'Legacy SQL Server-only operational view; no PostgreSQL counterpart.'),
    ('[Reporting].[VwReconDailyOverview]', 'RECONCILIATION', 'Gunluk reconciliation high-level: islenen, eslesen, eslesmemis, manuel inceleme gereken.', 'Daily reconciliation high-level: processed, matched, unmatched, manual-review.', 'Bu rapor ne diyor?', 'What does this report tell us?', 'Operasyonel ve finansal anlik durum gostergesidir; trend ile birlikte degerlendirilmelidir.', 'Operational and financial point-in-time indicator; evaluate together with the trend.', 'Gunluk ve haftalik takip.', 'Daily and weekly monitoring.', 'Operasyon, recon, finans.', 'Operations, recon, finance.', 'Anormal sapmalarda ilgili detay raporlarina dril edin; kalici sorunlarda runbook taki adimi izleyin.', 'On abnormal deviations drill into the related detail reports; for persistent issues follow the runbook step.', 'Bkz. view tanimi.', 'See view definition.', 'LIVE: anlik durum; ARCHIVE: tarihsel trend.', 'LIVE: point-in-time; ARCHIVE: historical trend.', 'MS SQL e ozgu eski (legacy) operasyonel view dur; PostgreSQL tarafinda karsiligi yoktur.', 'Legacy SQL Server-only operational view; no PostgreSQL counterpart.'),
    ('[Reporting].[VwReconOpenItems]', 'RECONCILIATION', 'Acik kalan reconciliation kayitlari (ozet halinde).', 'Open reconciliation items (summary).', 'Bu rapor ne diyor?', 'What does this report tell us?', 'Operasyonel ve finansal anlik durum gostergesidir; trend ile birlikte degerlendirilmelidir.', 'Operational and financial point-in-time indicator; evaluate together with the trend.', 'Gunluk ve haftalik takip.', 'Daily and weekly monitoring.', 'Operasyon, recon, finans.', 'Operations, recon, finance.', 'Anormal sapmalarda ilgili detay raporlarina dril edin; kalici sorunlarda runbook taki adimi izleyin.', 'On abnormal deviations drill into the related detail reports; for persistent issues follow the runbook step.', 'Bkz. view tanimi.', 'See view definition.', 'LIVE: anlik durum; ARCHIVE: tarihsel trend.', 'LIVE: point-in-time; ARCHIVE: historical trend.', 'MS SQL e ozgu eski (legacy) operasyonel view dur; PostgreSQL tarafinda karsiligi yoktur.', 'Legacy SQL Server-only operational view; no PostgreSQL counterpart.'),
    ('[Reporting].[VwReconOpenItemAging]', 'RECONCILIATION', 'Acik reconciliation kayitlarinin yas dagilimi.', 'Aging distribution of open reconciliation items.', 'Bu rapor ne diyor?', 'What does this report tell us?', 'Operasyonel ve finansal anlik durum gostergesidir; trend ile birlikte degerlendirilmelidir.', 'Operational and financial point-in-time indicator; evaluate together with the trend.', 'Gunluk ve haftalik takip.', 'Daily and weekly monitoring.', 'Operasyon, recon, finans.', 'Operations, recon, finance.', 'Anormal sapmalarda ilgili detay raporlarina dril edin; kalici sorunlarda runbook taki adimi izleyin.', 'On abnormal deviations drill into the related detail reports; for persistent issues follow the runbook step.', 'Bkz. view tanimi.', 'See view definition.', 'LIVE: anlik durum; ARCHIVE: tarihsel trend.', 'LIVE: point-in-time; ARCHIVE: historical trend.', 'MS SQL e ozgu eski (legacy) operasyonel view dur; PostgreSQL tarafinda karsiligi yoktur.', 'Legacy SQL Server-only operational view; no PostgreSQL counterpart.'),
    ('[Reporting].[VwReconManualReviewQueue]', 'MANUAL_REVIEW', 'Manuel inceleme bekleyen kayitlarin kuyruk goruntusu.', 'Queue view of items awaiting manual review.', 'Bu rapor ne diyor?', 'What does this report tell us?', 'Operasyonel ve finansal anlik durum gostergesidir; trend ile birlikte degerlendirilmelidir.', 'Operational and financial point-in-time indicator; evaluate together with the trend.', 'Gunluk ve haftalik takip.', 'Daily and weekly monitoring.', 'Operasyon, recon, finans.', 'Operations, recon, finance.', 'Anormal sapmalarda ilgili detay raporlarina dril edin; kalici sorunlarda runbook taki adimi izleyin.', 'On abnormal deviations drill into the related detail reports; for persistent issues follow the runbook step.', 'Bkz. view tanimi.', 'See view definition.', 'LIVE: anlik durum; ARCHIVE: tarihsel trend.', 'LIVE: point-in-time; ARCHIVE: historical trend.', 'MS SQL e ozgu eski (legacy) operasyonel view dur; PostgreSQL tarafinda karsiligi yoktur.', 'Legacy SQL Server-only operational view; no PostgreSQL counterpart.'),
    ('[Reporting].[VwReconAlertSummary]', 'ALERTS', 'Reconciliation kaynakli alarm gruplarinin ozeti.', 'Summary of reconciliation-originated alert groups.', 'Bu rapor ne diyor?', 'What does this report tell us?', 'Operasyonel ve finansal anlik durum gostergesidir; trend ile birlikte degerlendirilmelidir.', 'Operational and financial point-in-time indicator; evaluate together with the trend.', 'Gunluk ve haftalik takip.', 'Daily and weekly monitoring.', 'Operasyon, recon, finans.', 'Operations, recon, finance.', 'Anormal sapmalarda ilgili detay raporlarina dril edin; kalici sorunlarda runbook taki adimi izleyin.', 'On abnormal deviations drill into the related detail reports; for persistent issues follow the runbook step.', 'Bkz. view tanimi.', 'See view definition.', 'LIVE: anlik durum; ARCHIVE: tarihsel trend.', 'LIVE: point-in-time; ARCHIVE: historical trend.', 'MS SQL e ozgu eski (legacy) operasyonel view dur; PostgreSQL tarafinda karsiligi yoktur.', 'Legacy SQL Server-only operational view; no PostgreSQL counterpart.'),
    ('[Reporting].[VwReconLiveCardContentDaily]', 'RECONCILIATION_CONTENT', 'LIVE card detay tablolari icin gunluk icerik metrikleri.', 'Daily content metrics for the LIVE card detail tables.', 'Bu rapor ne diyor?', 'What does this report tell us?', 'Operasyonel ve finansal anlik durum gostergesidir; trend ile birlikte degerlendirilmelidir.', 'Operational and financial point-in-time indicator; evaluate together with the trend.', 'Gunluk ve haftalik takip.', 'Daily and weekly monitoring.', 'Operasyon, recon, finans.', 'Operations, recon, finance.', 'Anormal sapmalarda ilgili detay raporlarina dril edin; kalici sorunlarda runbook taki adimi izleyin.', 'On abnormal deviations drill into the related detail reports; for persistent issues follow the runbook step.', 'Bkz. view tanimi.', 'See view definition.', 'LIVE: anlik durum; ARCHIVE: tarihsel trend.', 'LIVE: point-in-time; ARCHIVE: historical trend.', 'MS SQL e ozgu eski (legacy) operasyonel view dur; PostgreSQL tarafinda karsiligi yoktur.', 'Legacy SQL Server-only operational view; no PostgreSQL counterpart.'),
    ('[Reporting].[VwReconLiveClearingContentDaily]', 'RECONCILIATION_CONTENT', 'LIVE clearing detay tablolari icin gunluk icerik metrikleri.', 'Daily content metrics for the LIVE clearing detail tables.', 'Bu rapor ne diyor?', 'What does this report tell us?', 'Operasyonel ve finansal anlik durum gostergesidir; trend ile birlikte degerlendirilmelidir.', 'Operational and financial point-in-time indicator; evaluate together with the trend.', 'Gunluk ve haftalik takip.', 'Daily and weekly monitoring.', 'Operasyon, recon, finans.', 'Operations, recon, finance.', 'Anormal sapmalarda ilgili detay raporlarina dril edin; kalici sorunlarda runbook taki adimi izleyin.', 'On abnormal deviations drill into the related detail reports; for persistent issues follow the runbook step.', 'Bkz. view tanimi.', 'See view definition.', 'LIVE: anlik durum; ARCHIVE: tarihsel trend.', 'LIVE: point-in-time; ARCHIVE: historical trend.', 'MS SQL e ozgu eski (legacy) operasyonel view dur; PostgreSQL tarafinda karsiligi yoktur.', 'Legacy SQL Server-only operational view; no PostgreSQL counterpart.'),
    ('[Reporting].[VwReconArchiveCardContentDaily]', 'RECONCILIATION_CONTENT', 'ARCHIVE card detay tablolari icin gunluk icerik metrikleri.', 'Daily content metrics for the ARCHIVE card detail tables.', 'Bu rapor ne diyor?', 'What does this report tell us?', 'Operasyonel ve finansal anlik durum gostergesidir; trend ile birlikte degerlendirilmelidir.', 'Operational and financial point-in-time indicator; evaluate together with the trend.', 'Gunluk ve haftalik takip.', 'Daily and weekly monitoring.', 'Operasyon, recon, finans.', 'Operations, recon, finance.', 'Anormal sapmalarda ilgili detay raporlarina dril edin; kalici sorunlarda runbook taki adimi izleyin.', 'On abnormal deviations drill into the related detail reports; for persistent issues follow the runbook step.', 'Bkz. view tanimi.', 'See view definition.', 'LIVE: anlik durum; ARCHIVE: tarihsel trend.', 'LIVE: point-in-time; ARCHIVE: historical trend.', 'MS SQL e ozgu eski (legacy) operasyonel view dur; PostgreSQL tarafinda karsiligi yoktur.', 'Legacy SQL Server-only operational view; no PostgreSQL counterpart.'),
    ('[Reporting].[VwReconArchiveClearingContentDaily]', 'RECONCILIATION_CONTENT', 'ARCHIVE clearing detay tablolari icin gunluk icerik metrikleri.', 'Daily content metrics for the ARCHIVE clearing detail tables.', 'Bu rapor ne diyor?', 'What does this report tell us?', 'Operasyonel ve finansal anlik durum gostergesidir; trend ile birlikte degerlendirilmelidir.', 'Operational and financial point-in-time indicator; evaluate together with the trend.', 'Gunluk ve haftalik takip.', 'Daily and weekly monitoring.', 'Operasyon, recon, finans.', 'Operations, recon, finance.', 'Anormal sapmalarda ilgili detay raporlarina dril edin; kalici sorunlarda runbook taki adimi izleyin.', 'On abnormal deviations drill into the related detail reports; for persistent issues follow the runbook step.', 'Bkz. view tanimi.', 'See view definition.', 'LIVE: anlik durum; ARCHIVE: tarihsel trend.', 'LIVE: point-in-time; ARCHIVE: historical trend.', 'MS SQL e ozgu eski (legacy) operasyonel view dur; PostgreSQL tarafinda karsiligi yoktur.', 'Legacy SQL Server-only operational view; no PostgreSQL counterpart.'),
    ('[Reporting].[VwReconContentDaily]', 'RECONCILIATION_CONTENT', 'LIVE+ARCHIVE birlestirilmis gunluk icerik metrikleri.', 'Combined LIVE+ARCHIVE daily content metrics.', 'Bu rapor ne diyor?', 'What does this report tell us?', 'Operasyonel ve finansal anlik durum gostergesidir; trend ile birlikte degerlendirilmelidir.', 'Operational and financial point-in-time indicator; evaluate together with the trend.', 'Gunluk ve haftalik takip.', 'Daily and weekly monitoring.', 'Operasyon, recon, finans.', 'Operations, recon, finance.', 'Anormal sapmalarda ilgili detay raporlarina dril edin; kalici sorunlarda runbook taki adimi izleyin.', 'On abnormal deviations drill into the related detail reports; for persistent issues follow the runbook step.', 'Bkz. view tanimi.', 'See view definition.', 'LIVE: anlik durum; ARCHIVE: tarihsel trend.', 'LIVE: point-in-time; ARCHIVE: historical trend.', 'MS SQL e ozgu eski (legacy) operasyonel view dur; PostgreSQL tarafinda karsiligi yoktur.', 'Legacy SQL Server-only operational view; no PostgreSQL counterpart.'),
    ('[Reporting].[VwReconClearingControlstatAnalysis]', 'RECONCILIATION', 'Clearing control_stat dagilimi ve etkileri.', 'Clearing control_stat distribution and impacts.', 'Bu rapor ne diyor?', 'What does this report tell us?', 'Operasyonel ve finansal anlik durum gostergesidir; trend ile birlikte degerlendirilmelidir.', 'Operational and financial point-in-time indicator; evaluate together with the trend.', 'Gunluk ve haftalik takip.', 'Daily and weekly monitoring.', 'Operasyon, recon, finans.', 'Operations, recon, finance.', 'Anormal sapmalarda ilgili detay raporlarina dril edin; kalici sorunlarda runbook taki adimi izleyin.', 'On abnormal deviations drill into the related detail reports; for persistent issues follow the runbook step.', 'Bkz. view tanimi.', 'See view definition.', 'LIVE: anlik durum; ARCHIVE: tarihsel trend.', 'LIVE: point-in-time; ARCHIVE: historical trend.', 'MS SQL e ozgu eski (legacy) operasyonel view dur; PostgreSQL tarafinda karsiligi yoktur.', 'Legacy SQL Server-only operational view; no PostgreSQL counterpart.'),
    ('[Reporting].[VwReconFinancialSummary]', 'FINANCIAL_RECONCILIATION', 'Reconciliation in finansal ozetidir; matched/unmatched tutarlar.', 'Financial summary of reconciliation; matched/unmatched amounts.', 'Bu rapor ne diyor?', 'What does this report tell us?', 'Operasyonel ve finansal anlik durum gostergesidir; trend ile birlikte degerlendirilmelidir.', 'Operational and financial point-in-time indicator; evaluate together with the trend.', 'Gunluk ve haftalik takip.', 'Daily and weekly monitoring.', 'Operasyon, recon, finans.', 'Operations, recon, finance.', 'Anormal sapmalarda ilgili detay raporlarina dril edin; kalici sorunlarda runbook taki adimi izleyin.', 'On abnormal deviations drill into the related detail reports; for persistent issues follow the runbook step.', 'Bkz. view tanimi.', 'See view definition.', 'LIVE: anlik durum; ARCHIVE: tarihsel trend.', 'LIVE: point-in-time; ARCHIVE: historical trend.', 'MS SQL e ozgu eski (legacy) operasyonel view dur; PostgreSQL tarafinda karsiligi yoktur.', 'Legacy SQL Server-only operational view; no PostgreSQL counterpart.'),
    ('[Reporting].[VwReconResponseStatusAnalysis]', 'AUTHORIZATION_HEALTH', 'Response/status kombinasyonlarinin reconciliation uzerine etkisi.', 'Effect of response/status combinations on reconciliation.', 'Bu rapor ne diyor?', 'What does this report tell us?', 'Operasyonel ve finansal anlik durum gostergesidir; trend ile birlikte degerlendirilmelidir.', 'Operational and financial point-in-time indicator; evaluate together with the trend.', 'Gunluk ve haftalik takip.', 'Daily and weekly monitoring.', 'Operasyon, recon, finans.', 'Operations, recon, finance.', 'Anormal sapmalarda ilgili detay raporlarina dril edin; kalici sorunlarda runbook taki adimi izleyin.', 'On abnormal deviations drill into the related detail reports; for persistent issues follow the runbook step.', 'Bkz. view tanimi.', 'See view definition.', 'LIVE: anlik durum; ARCHIVE: tarihsel trend.', 'LIVE: point-in-time; ARCHIVE: historical trend.', 'MS SQL e ozgu eski (legacy) operasyonel view dur; PostgreSQL tarafinda karsiligi yoktur.', 'Legacy SQL Server-only operational view; no PostgreSQL counterpart.'),
    ('[Reporting].[VwArchiveRunOverview]', 'ARCHIVE', 'Arsiv kosumlarinin yuksek seviye ozeti.', 'High-level overview of archive runs.', 'Bu rapor ne diyor?', 'What does this report tell us?', 'Operasyonel ve finansal anlik durum gostergesidir; trend ile birlikte degerlendirilmelidir.', 'Operational and financial point-in-time indicator; evaluate together with the trend.', 'Gunluk ve haftalik takip.', 'Daily and weekly monitoring.', 'Operasyon, recon, finans.', 'Operations, recon, finance.', 'Anormal sapmalarda ilgili detay raporlarina dril edin; kalici sorunlarda runbook taki adimi izleyin.', 'On abnormal deviations drill into the related detail reports; for persistent issues follow the runbook step.', 'Bkz. view tanimi.', 'See view definition.', 'LIVE: anlik durum; ARCHIVE: tarihsel trend.', 'LIVE: point-in-time; ARCHIVE: historical trend.', 'MS SQL e ozgu eski (legacy) operasyonel view dur; PostgreSQL tarafinda karsiligi yoktur.', 'Legacy SQL Server-only operational view; no PostgreSQL counterpart.'),
    ('[Reporting].[VwArchiveEligibility]', 'ARCHIVE', 'Arsivlenmeye uygun ama henuz arsivlenmemis dosyalar.', 'Files eligible for archiving but not yet archived.', 'Bu rapor ne diyor?', 'What does this report tell us?', 'Operasyonel ve finansal anlik durum gostergesidir; trend ile birlikte degerlendirilmelidir.', 'Operational and financial point-in-time indicator; evaluate together with the trend.', 'Gunluk ve haftalik takip.', 'Daily and weekly monitoring.', 'Operasyon, recon, finans.', 'Operations, recon, finance.', 'Anormal sapmalarda ilgili detay raporlarina dril edin; kalici sorunlarda runbook taki adimi izleyin.', 'On abnormal deviations drill into the related detail reports; for persistent issues follow the runbook step.', 'Bkz. view tanimi.', 'See view definition.', 'LIVE: anlik durum; ARCHIVE: tarihsel trend.', 'LIVE: point-in-time; ARCHIVE: historical trend.', 'MS SQL e ozgu eski (legacy) operasyonel view dur; PostgreSQL tarafinda karsiligi yoktur.', 'Legacy SQL Server-only operational view; no PostgreSQL counterpart.'),
    ('[Reporting].[VwArchiveBacklogTrend]', 'ARCHIVE', 'Arsiv backlog trendi (gun bazinda).', 'Daily archive backlog trend.', 'Bu rapor ne diyor?', 'What does this report tell us?', 'Operasyonel ve finansal anlik durum gostergesidir; trend ile birlikte degerlendirilmelidir.', 'Operational and financial point-in-time indicator; evaluate together with the trend.', 'Gunluk ve haftalik takip.', 'Daily and weekly monitoring.', 'Operasyon, recon, finans.', 'Operations, recon, finance.', 'Anormal sapmalarda ilgili detay raporlarina dril edin; kalici sorunlarda runbook taki adimi izleyin.', 'On abnormal deviations drill into the related detail reports; for persistent issues follow the runbook step.', 'Bkz. view tanimi.', 'See view definition.', 'LIVE: anlik durum; ARCHIVE: tarihsel trend.', 'LIVE: point-in-time; ARCHIVE: historical trend.', 'MS SQL e ozgu eski (legacy) operasyonel view dur; PostgreSQL tarafinda karsiligi yoktur.', 'Legacy SQL Server-only operational view; no PostgreSQL counterpart.'),
    ('[Reporting].[VwArchiveRetentionSnapshot]', 'ARCHIVE', 'Veri saklama politikasi anlik goruntusu (yas/kategori).', 'Retention policy snapshot (age/category).', 'Bu rapor ne diyor?', 'What does this report tell us?', 'Operasyonel ve finansal anlik durum gostergesidir; trend ile birlikte degerlendirilmelidir.', 'Operational and financial point-in-time indicator; evaluate together with the trend.', 'Gunluk ve haftalik takip.', 'Daily and weekly monitoring.', 'Operasyon, recon, finans.', 'Operations, recon, finance.', 'Anormal sapmalarda ilgili detay raporlarina dril edin; kalici sorunlarda runbook taki adimi izleyin.', 'On abnormal deviations drill into the related detail reports; for persistent issues follow the runbook step.', 'Bkz. view tanimi.', 'See view definition.', 'LIVE: anlik durum; ARCHIVE: tarihsel trend.', 'LIVE: point-in-time; ARCHIVE: historical trend.', 'MS SQL e ozgu eski (legacy) operasyonel view dur; PostgreSQL tarafinda karsiligi yoktur.', 'Legacy SQL Server-only operational view; no PostgreSQL counterpart.'),
    ('[Reporting].[VwFileReconSummary]', 'FILE_PROCESSING', 'Dosya bazinda reconciliation sonucu (matched/unmatched/manual).', 'Per-file reconciliation outcome (matched/unmatched/manual).', 'Bu rapor ne diyor?', 'What does this report tell us?', 'Operasyonel ve finansal anlik durum gostergesidir; trend ile birlikte degerlendirilmelidir.', 'Operational and financial point-in-time indicator; evaluate together with the trend.', 'Gunluk ve haftalik takip.', 'Daily and weekly monitoring.', 'Operasyon, recon, finans.', 'Operations, recon, finance.', 'Anormal sapmalarda ilgili detay raporlarina dril edin; kalici sorunlarda runbook taki adimi izleyin.', 'On abnormal deviations drill into the related detail reports; for persistent issues follow the runbook step.', 'Bkz. view tanimi.', 'See view definition.', 'LIVE: anlik durum; ARCHIVE: tarihsel trend.', 'LIVE: point-in-time; ARCHIVE: historical trend.', 'MS SQL e ozgu eski (legacy) operasyonel view dur; PostgreSQL tarafinda karsiligi yoktur.', 'Legacy SQL Server-only operational view; no PostgreSQL counterpart.'),
    ('[Reporting].[VwReconMatchRateTrend]', 'RECONCILIATION_KPI', 'Reconciliation match-rate trend grafigi (gunluk/haftalik).', 'Reconciliation match-rate trend chart (daily/weekly).', 'Bu rapor ne diyor?', 'What does this report tell us?', 'Operasyonel ve finansal anlik durum gostergesidir; trend ile birlikte degerlendirilmelidir.', 'Operational and financial point-in-time indicator; evaluate together with the trend.', 'Gunluk ve haftalik takip.', 'Daily and weekly monitoring.', 'Operasyon, recon, finans.', 'Operations, recon, finance.', 'Anormal sapmalarda ilgili detay raporlarina dril edin; kalici sorunlarda runbook taki adimi izleyin.', 'On abnormal deviations drill into the related detail reports; for persistent issues follow the runbook step.', 'Bkz. view tanimi.', 'See view definition.', 'LIVE: anlik durum; ARCHIVE: tarihsel trend.', 'LIVE: point-in-time; ARCHIVE: historical trend.', 'MS SQL e ozgu eski (legacy) operasyonel view dur; PostgreSQL tarafinda karsiligi yoktur.', 'Legacy SQL Server-only operational view; no PostgreSQL counterpart.'),
    ('[Reporting].[VwReconGapAnalysis]', 'RECONCILIATION', 'Reconciliation gap (eksik/fazla) analizi.', 'Reconciliation gap (missing/extra) analysis.', 'Bu rapor ne diyor?', 'What does this report tell us?', 'Operasyonel ve finansal anlik durum gostergesidir; trend ile birlikte degerlendirilmelidir.', 'Operational and financial point-in-time indicator; evaluate together with the trend.', 'Gunluk ve haftalik takip.', 'Daily and weekly monitoring.', 'Operasyon, recon, finans.', 'Operations, recon, finance.', 'Anormal sapmalarda ilgili detay raporlarina dril edin; kalici sorunlarda runbook taki adimi izleyin.', 'On abnormal deviations drill into the related detail reports; for persistent issues follow the runbook step.', 'Bkz. view tanimi.', 'See view definition.', 'LIVE: anlik durum; ARCHIVE: tarihsel trend.', 'LIVE: point-in-time; ARCHIVE: historical trend.', 'MS SQL e ozgu eski (legacy) operasyonel view dur; PostgreSQL tarafinda karsiligi yoktur.', 'Legacy SQL Server-only operational view; no PostgreSQL counterpart.'),
    ('[Reporting].[VwUnmatchedTransactionAging]', 'FINANCIAL_RISK', 'Eslesmesim is islemlerin yas dagilimi.', 'Aging distribution of unmatched transactions.', 'Bu rapor ne diyor?', 'What does this report tell us?', 'Operasyonel ve finansal anlik durum gostergesidir; trend ile birlikte degerlendirilmelidir.', 'Operational and financial point-in-time indicator; evaluate together with the trend.', 'Gunluk ve haftalik takip.', 'Daily and weekly monitoring.', 'Operasyon, recon, finans.', 'Operations, recon, finance.', 'Anormal sapmalarda ilgili detay raporlarina dril edin; kalici sorunlarda runbook taki adimi izleyin.', 'On abnormal deviations drill into the related detail reports; for persistent issues follow the runbook step.', 'Bkz. view tanimi.', 'See view definition.', 'LIVE: anlik durum; ARCHIVE: tarihsel trend.', 'LIVE: point-in-time; ARCHIVE: historical trend.', 'MS SQL e ozgu eski (legacy) operasyonel view dur; PostgreSQL tarafinda karsiligi yoktur.', 'Legacy SQL Server-only operational view; no PostgreSQL counterpart.'),
    ('[Reporting].[VwNetworkReconScorecard]', 'RECONCILIATION_KPI', 'Network bazinda reconciliation puan karti.', 'Per-network reconciliation scorecard.', 'Bu rapor ne diyor?', 'What does this report tell us?', 'Operasyonel ve finansal anlik durum gostergesidir; trend ile birlikte degerlendirilmelidir.', 'Operational and financial point-in-time indicator; evaluate together with the trend.', 'Gunluk ve haftalik takip.', 'Daily and weekly monitoring.', 'Operasyon, recon, finans.', 'Operations, recon, finance.', 'Anormal sapmalarda ilgili detay raporlarina dril edin; kalici sorunlarda runbook taki adimi izleyin.', 'On abnormal deviations drill into the related detail reports; for persistent issues follow the runbook step.', 'Bkz. view tanimi.', 'See view definition.', 'LIVE: anlik durum; ARCHIVE: tarihsel trend.', 'LIVE: point-in-time; ARCHIVE: historical trend.', 'MS SQL e ozgu eski (legacy) operasyonel view dur; PostgreSQL tarafinda karsiligi yoktur.', 'Legacy SQL Server-only operational view; no PostgreSQL counterpart.'),
    ('[Reporting].[VwCardClearingCorrelation]', 'FINANCIAL_RECONCILIATION', 'Card-Clearing korelasyon ve eslesme oranlari.', 'Card-Clearing correlation and match ratios.', 'Bu rapor ne diyor?', 'What does this report tell us?', 'Operasyonel ve finansal anlik durum gostergesidir; trend ile birlikte degerlendirilmelidir.', 'Operational and financial point-in-time indicator; evaluate together with the trend.', 'Gunluk ve haftalik takip.', 'Daily and weekly monitoring.', 'Operasyon, recon, finans.', 'Operations, recon, finance.', 'Anormal sapmalarda ilgili detay raporlarina dril edin; kalici sorunlarda runbook taki adimi izleyin.', 'On abnormal deviations drill into the related detail reports; for persistent issues follow the runbook step.', 'Bkz. view tanimi.', 'See view definition.', 'LIVE: anlik durum; ARCHIVE: tarihsel trend.', 'LIVE: point-in-time; ARCHIVE: historical trend.', 'MS SQL e ozgu eski (legacy) operasyonel view dur; PostgreSQL tarafinda karsiligi yoktur.', 'Legacy SQL Server-only operational view; no PostgreSQL counterpart.')
) AS d(
    ViewName, ReportGroup,
    PurposeTr, PurposeEn,
    BusinessQuestionTr, BusinessQuestionEn,
    InterpretationTr, InterpretationEn,
    UsageTimeTr, UsageTimeEn,
    TargetUserTr, TargetUserEn,
    ActionGuidanceTr, ActionGuidanceEn,
    ImportantColumnsTr, ImportantColumnsEn,
    LiveArchiveInterpretationTr, LiveArchiveInterpretationEn,
    NotesTr, NotesEn
);
GO

-- =====================================================================================
-- Helper view: dynamic reporting catalog (SQL Server)
-- Auto-discovers every view under the [Reporting] schema and produces JSON contracts
-- consumed by the generic /api/reporting/dynamic endpoint.
-- View name and column names mirror the PostgreSQL counterpart so the same Dapper
-- query (`reporting.rep_contract_catalog`) works on both providers.
-- =====================================================================================
IF OBJECT_ID('[Reporting].[rep_contract_catalog]', 'V') IS NOT NULL
    DROP VIEW [Reporting].[rep_contract_catalog];
GO

CREATE VIEW [Reporting].[rep_contract_catalog] AS
WITH rep_views AS (
    SELECT
        v.name                                            AS report_name,
        '[Reporting].[' + v.name + ']'                    AS full_view_name,
        v.object_id                                       AS view_object_id
    FROM sys.views v
    JOIN sys.schemas s ON s.schema_id = v.schema_id
    WHERE s.name = 'Reporting'
      AND v.name <> 'rep_contract_catalog'
),
cols AS (
    SELECT
        v.report_name,
        v.full_view_name,
        c.name                                            AS column_name,
        c.column_id                                       AS ordinal,
        CAST(c.is_nullable AS bit)                        AS is_nullable,
        t.name                                            AS sql_type,
        CASE
            WHEN t.name IN ('char','varchar','nchar','nvarchar','text','ntext','sysname','uniqueidentifier','xml')
                THEN 'string'
            WHEN t.name IN ('tinyint','smallint','int','bigint','decimal','numeric','smallmoney','money','real','float')
                THEN 'number'
            WHEN t.name = 'bit'
                THEN 'boolean'
            WHEN t.name IN ('date','datetime','datetime2','smalldatetime','datetimeoffset','time')
                THEN 'datetime'
            ELSE 'string'
        END                                               AS api_type
    FROM rep_views v
    JOIN sys.columns c ON c.object_id    = v.view_object_id
    JOIN sys.types   t ON t.user_type_id = c.user_type_id
),
cols_with_ops AS (
    SELECT
        c.*,
        CASE c.api_type
            WHEN 'string'   THEN N'["eq","neq","contains","startsWith","endsWith","in","isNull","isNotNull"]'
            WHEN 'number'   THEN N'["eq","neq","gt","gte","lt","lte","between","in","isNull","isNotNull"]'
            WHEN 'datetime' THEN N'["eq","neq","gt","gte","lt","lte","between","isNull","isNotNull"]'
            WHEN 'boolean'  THEN N'["eq","neq","isNull","isNotNull"]'
            ELSE                 N'["eq","neq","isNull","isNotNull"]'
        END AS operators_json
    FROM cols c
)
SELECT
    v.report_name,
    v.full_view_name,
    -- request_contract_json: { "filters": [ { field, type, nullable, operators[] }, ... ] }
    JSON_QUERY((
        SELECT
            JSON_QUERY(ISNULL((
                SELECT
                    c.column_name                       AS field,
                    c.api_type                          AS [type],
                    c.is_nullable                       AS nullable,
                    JSON_QUERY(c.operators_json)        AS operators
                FROM cols_with_ops c
                WHERE c.report_name = v.report_name
                ORDER BY c.ordinal
                FOR JSON PATH
            ), N'[]')) AS filters
        FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
    )) AS request_contract_json,
    -- response_contract_json: { "type": "Dictionary<string, object?>", "columns": [ { field, type, nullable, ordinal }, ... ] }
    JSON_QUERY((
        SELECT
            'Dictionary<string, object?>' AS [type],
            JSON_QUERY(ISNULL((
                SELECT
                    c.column_name                       AS field,
                    c.api_type                          AS [type],
                    c.is_nullable                       AS nullable,
                    c.ordinal                           AS ordinal
                FROM cols_with_ops c
                WHERE c.report_name = v.report_name
                ORDER BY c.ordinal
                FOR JSON PATH
            ), N'[]')) AS columns
        FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
    )) AS response_contract_json
FROM rep_views v;
GO

