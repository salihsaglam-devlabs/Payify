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

