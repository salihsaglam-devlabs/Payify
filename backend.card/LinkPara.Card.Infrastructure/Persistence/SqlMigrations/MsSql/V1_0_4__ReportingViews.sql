-- ============================================================================
-- Payify Reporting Views — MSSQL (Consolidated)
-- Version: 1.0.4
-- Date: 2026-04-15
-- Description: ALL reporting views in a single migration file.
--              Includes: schema creation, file ingestion summary, reconciliation
--              file/line/operation/alert/aging/archive views, base transaction
--              views, matched pair view, business views, and summary views.
--
-- NOTE: Column names follow EF Core conventions for MSSQL:
--   - Columns with explicit HasColumnName() use snake_case
--   - AuditEntity base class columns (Id, CreateDate, UpdateDate, CreatedBy,
--     LastModifiedBy, RecordStatus) use PascalCase (no snake_case convention)
-- ============================================================================


-- ############################################################################
-- PART 1: SCHEMA + CORE REPORTING VIEWS
-- ############################################################################

-- Create reporting schema
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Reporting')
BEGIN
    EXEC('CREATE SCHEMA Reporting');
END
GO

-- ============================================================================
-- VIEW 1: File Ingestion Summary
-- ============================================================================
IF OBJECT_ID('Reporting.vw_FileIngestionSummary', 'V') IS NOT NULL
    DROP VIEW Reporting.vw_FileIngestionSummary;
GO
CREATE VIEW Reporting.vw_FileIngestionSummary AS
SELECT
    f.Id                                    AS FileId,
    f.file_name                             AS FileName,
    f.file_key                              AS FileKey,
    f.source_type                           AS SourceType,
    f.file_type                             AS FileType,
    f.content_type                          AS ContentType,
    f.status                                AS Status,
    f.expected_line_count                   AS ExpectedCount,
    f.processed_line_count                  AS ProcessedCount,
    f.successful_line_count                 AS SuccessCount,
    f.failed_line_count                     AS ErrorCount,
    CASE
        WHEN f.processed_line_count > 0
        THEN ROUND(CAST(f.successful_line_count AS DECIMAL(18,4)) / f.processed_line_count * 100, 2)
        ELSE 0
    END                                     AS SuccessRate,
    CASE
        WHEN f.expected_line_count > 0 AND f.processed_line_count <> f.expected_line_count
        THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
    END                                     AS HasCountMismatch,
    f.is_archived                           AS IsArchived,
    f.CreateDate                            AS IngestedAt,
    f.UpdateDate                            AS LastUpdatedAt,
    f.CreatedBy,
    f.RecordStatus
FROM Ingestion.[File] f;
GO

-- ============================================================================
-- VIEW 2: Reconciliation File Summary
-- ============================================================================
IF OBJECT_ID('Reporting.vw_ReconciliationFileSummary', 'V') IS NOT NULL
    DROP VIEW Reporting.vw_ReconciliationFileSummary;
GO
CREATE VIEW Reporting.vw_ReconciliationFileSummary AS
SELECT
    f.Id                                    AS FileId,
    f.file_name                             AS FileName,
    f.file_type                             AS FileType,
    f.content_type                          AS ContentType,
    f.status                                AS FileStatus,
    f.is_archived                           AS IsArchived,
    f.CreateDate                            AS IngestedAt,

    COUNT(CASE WHEN fl.line_type = 'D' THEN 1 END)
        AS TotalDetailLines,

    COUNT(CASE WHEN fl.line_type = 'D' AND fl.reconciliation_status = 'Ready' THEN 1 END)
        AS ReconReadyCount,
    COUNT(CASE WHEN fl.line_type = 'D' AND fl.reconciliation_status = 'Processing' THEN 1 END)
        AS ReconProcessingCount,
    COUNT(CASE WHEN fl.line_type = 'D' AND fl.reconciliation_status = 'Success' THEN 1 END)
        AS ReconSuccessCount,
    COUNT(CASE WHEN fl.line_type = 'D' AND fl.reconciliation_status = 'Failed' THEN 1 END)
        AS ReconFailedCount,
    COUNT(CASE WHEN fl.line_type = 'D' AND fl.reconciliation_status IS NULL THEN 1 END)
        AS ReconPendingCount,

    CASE
        WHEN COUNT(CASE WHEN fl.line_type = 'D' THEN 1 END) > 0
        THEN ROUND(
            CAST(COUNT(CASE WHEN fl.line_type = 'D' AND fl.reconciliation_status = 'Success' THEN 1 END) AS DECIMAL(18,4))
            / COUNT(CASE WHEN fl.line_type = 'D' THEN 1 END) * 100, 2)
        ELSE 0
    END AS ReconCompletionRate,

    COUNT(CASE WHEN fl.line_type = 'D' AND fl.matched_clearing_line_id IS NOT NULL THEN 1 END)
        AS MatchedCount,
    COUNT(CASE WHEN fl.line_type = 'D' AND fl.matched_clearing_line_id IS NULL
               AND f.file_type = 'Card' THEN 1 END)
        AS UnmatchedCardCount,

    COUNT(CASE WHEN fl.line_type = 'D' AND fl.duplicate_status IS NOT NULL
               AND fl.duplicate_status NOT IN ('Unique') THEN 1 END)
        AS DuplicateCount

FROM Ingestion.[File] f
LEFT JOIN Ingestion.FileLine fl ON fl.file_id = f.Id
GROUP BY f.Id, f.file_name, f.file_type, f.content_type, f.status, f.is_archived, f.CreateDate;
GO

-- ============================================================================
-- VIEW 3: Reconciliation Line Detail
-- ============================================================================
IF OBJECT_ID('Reporting.vw_ReconciliationLineDetail', 'V') IS NOT NULL
    DROP VIEW Reporting.vw_ReconciliationLineDetail;
GO
CREATE VIEW Reporting.vw_ReconciliationLineDetail AS
SELECT
    fl.Id                                   AS FileLineId,
    fl.file_id                              AS FileId,
    f.file_name                             AS FileName,
    f.file_type                             AS FileType,
    f.content_type                          AS ContentType,
    fl.line_number                          AS LineNumber,
    fl.line_type                            AS LineType,
    fl.status                               AS LineStatus,
    fl.reconciliation_status                AS ReconciliationStatus,
    fl.duplicate_status                     AS DuplicateStatus,
    fl.duplicate_group_id                   AS DuplicateGroupId,
    fl.matched_clearing_line_id             AS MatchedClearingLineId,
    CASE WHEN fl.matched_clearing_line_id IS NOT NULL THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END
        AS HasMatch,
    fl.correlation_key                      AS CorrelationKey,
    fl.correlation_value                    AS CorrelationValue,
    fl.retry_count                          AS RetryCount,
    fl.message                              AS LineMessage,
    f.is_archived                           AS IsArchived,

    eval_agg.EvaluationCount,
    eval_agg.LatestEvaluationStatus,

    op_agg.OperationCount,
    op_agg.PendingOperationCount,
    op_agg.FailedOperationCount,
    op_agg.CompletedOperationCount,
    op_agg.ManualOperationCount,

    fl.CreateDate                           AS LineCreatedAt,
    fl.UpdateDate                           AS LineUpdatedAt,
    DATEDIFF(DAY, fl.CreateDate, GETDATE()) AS AgeDays

FROM Ingestion.FileLine fl
JOIN Ingestion.[File] f ON f.Id = fl.file_id
OUTER APPLY (
    SELECT
        COUNT(*) AS EvaluationCount,
        (SELECT TOP 1 e2.status FROM Reconciliation.Evaluation e2
         WHERE e2.file_line_id = fl.Id
         ORDER BY e2.CreateDate DESC) AS LatestEvaluationStatus
    FROM Reconciliation.Evaluation e
    WHERE e.file_line_id = fl.Id
) eval_agg
OUTER APPLY (
    SELECT
        COUNT(*) AS OperationCount,
        COUNT(CASE WHEN o.status IN ('Planned', 'Blocked', 'Executing') THEN 1 END)
            AS PendingOperationCount,
        COUNT(CASE WHEN o.status = 'Failed' THEN 1 END)
            AS FailedOperationCount,
        COUNT(CASE WHEN o.status = 'Completed' THEN 1 END)
            AS CompletedOperationCount,
        COUNT(CASE WHEN o.is_manual = 1 THEN 1 END)
            AS ManualOperationCount
    FROM Reconciliation.Operation o
    WHERE o.file_line_id = fl.Id
) op_agg
WHERE fl.line_type = 'D';
GO

-- ============================================================================
-- VIEW 4: Unmatched Card Records
-- ============================================================================
IF OBJECT_ID('Reporting.vw_ReconciliationUnmatchedCards', 'V') IS NOT NULL
    DROP VIEW Reporting.vw_ReconciliationUnmatchedCards;
GO
CREATE VIEW Reporting.vw_ReconciliationUnmatchedCards AS
SELECT
    fl.Id                                   AS FileLineId,
    fl.file_id                              AS FileId,
    f.file_name                             AS FileName,
    f.content_type                          AS ContentType,
    fl.line_number                          AS LineNumber,
    fl.status                               AS LineStatus,
    fl.reconciliation_status                AS ReconciliationStatus,
    fl.correlation_key                      AS CorrelationKey,
    fl.correlation_value                    AS CorrelationValue,
    fl.duplicate_status                     AS DuplicateStatus,
    fl.CreateDate                           AS LineCreatedAt,
    fl.UpdateDate                           AS LineUpdatedAt,
    DATEDIFF(DAY, fl.CreateDate, GETDATE()) AS AgeDays,
    f.is_archived                           AS IsArchived
FROM Ingestion.FileLine fl
JOIN Ingestion.[File] f ON f.Id = fl.file_id
WHERE f.file_type = 'Card'
  AND fl.line_type = 'D'
  AND fl.matched_clearing_line_id IS NULL
  AND fl.status = 'Success';
GO

-- ============================================================================
-- VIEW 5: Reconciliation Operation Tracker
-- ============================================================================
IF OBJECT_ID('Reporting.vw_ReconciliationOperationTracker', 'V') IS NOT NULL
    DROP VIEW Reporting.vw_ReconciliationOperationTracker;
GO
CREATE VIEW Reporting.vw_ReconciliationOperationTracker AS
SELECT
    o.Id                                    AS OperationId,
    o.file_line_id                          AS FileLineId,
    f.file_name                             AS FileName,
    f.content_type                          AS ContentType,
    o.evaluation_id                         AS EvaluationId,
    e.status                                AS EvaluationStatus,
    o.group_id                              AS GroupId,
    o.sequence_number                       AS SequenceNumber,
    o.code                                  AS OperationCode,
    o.note                                  AS OperationNote,
    o.status                                AS OperationStatus,
    o.is_manual                             AS IsManual,
    o.branch                                AS Branch,
    o.retry_count                           AS RetryCount,
    o.max_retry_count                       AS MaxRetries,
    o.last_error                            AS LastError,
    o.lease_owner                           AS LeaseOwner,
    o.next_attempt_at                       AS NextAttemptAt,

    latest_exec.attempt_number              AS LastAttemptNumber,
    latest_exec.status                      AS LastExecutionStatus,
    latest_exec.result_code                 AS LastResultCode,
    latest_exec.result_message              AS LastResultMessage,
    latest_exec.error_code                  AS LastErrorCode,
    latest_exec.error_message               AS LastErrorMessage,
    latest_exec.started_at                  AS LastExecutionStartedAt,
    latest_exec.finished_at                 AS LastExecutionFinishedAt,

    exec_count.TotalExecutions,

    o.CreateDate                            AS OperationCreatedAt,
    o.UpdateDate                            AS OperationUpdatedAt,
    DATEDIFF(HOUR, o.CreateDate, GETDATE()) AS AgeHours

FROM Reconciliation.Operation o
JOIN Reconciliation.Evaluation e ON e.Id = o.evaluation_id
JOIN Ingestion.FileLine fl ON fl.Id = o.file_line_id
JOIN Ingestion.[File] f ON f.Id = fl.file_id
OUTER APPLY (
    SELECT TOP 1 ex.attempt_number, ex.status, ex.result_code, ex.result_message,
                 ex.error_code, ex.error_message, ex.started_at, ex.finished_at
    FROM Reconciliation.OperationExecution ex
    WHERE ex.operation_id = o.Id
    ORDER BY ex.attempt_number DESC
) latest_exec
OUTER APPLY (
    SELECT COUNT(*) AS TotalExecutions
    FROM Reconciliation.OperationExecution ex2
    WHERE ex2.operation_id = o.Id
) exec_count;
GO

-- ============================================================================
-- VIEW 6: Pending Actions
-- ============================================================================
IF OBJECT_ID('Reporting.vw_ReconciliationPendingActions', 'V') IS NOT NULL
    DROP VIEW Reporting.vw_ReconciliationPendingActions;
GO
CREATE VIEW Reporting.vw_ReconciliationPendingActions AS
SELECT
    o.Id                                    AS OperationId,
    o.file_line_id                          AS FileLineId,
    f.file_name                             AS FileName,
    f.content_type                          AS ContentType,
    o.group_id                              AS GroupId,
    o.code                                  AS OperationCode,
    o.status                                AS OperationStatus,
    o.is_manual                             AS IsManual,
    o.last_error                            AS LastError,
    o.retry_count                           AS RetryCount,

    r.Id                                    AS ReviewId,
    r.decision                              AS ReviewDecision,
    r.reviewer_id                           AS ReviewerId,
    r.comment                               AS ReviewComment,
    r.expires_at                            AS ReviewExpiresAt,
    r.expiration_action                     AS ExpirationAction,
    r.expiration_flow_action                AS ExpirationFlowAction,

    CASE
        WHEN r.expires_at IS NOT NULL AND r.expires_at < GETDATE() AND r.decision = 'Pending'
        THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
    END                                     AS IsReviewExpired,

    o.CreateDate                            AS OperationCreatedAt,
    DATEDIFF(HOUR, o.CreateDate, GETDATE()) AS WaitingHours

FROM Reconciliation.Operation o
JOIN Ingestion.FileLine fl ON fl.Id = o.file_line_id
JOIN Ingestion.[File] f ON f.Id = fl.file_id
LEFT JOIN Reconciliation.Review r ON r.operation_id = o.Id AND r.decision = 'Pending'
WHERE o.status IN ('Planned', 'Blocked', 'Executing', 'Failed');
GO

-- ============================================================================
-- VIEW 7: Alert Dashboard
-- ============================================================================
IF OBJECT_ID('Reporting.vw_ReconciliationAlertDashboard', 'V') IS NOT NULL
    DROP VIEW Reporting.vw_ReconciliationAlertDashboard;
GO
CREATE VIEW Reporting.vw_ReconciliationAlertDashboard AS
SELECT
    a.Id                                    AS AlertId,
    a.file_line_id                          AS FileLineId,
    f.file_name                             AS FileName,
    f.content_type                          AS ContentType,
    a.group_id                              AS GroupId,
    a.evaluation_id                         AS EvaluationId,
    a.operation_id                          AS OperationId,
    o.code                                  AS OperationCode,
    o.status                                AS OperationStatus,
    a.severity                              AS Severity,
    a.alert_type                            AS AlertType,
    a.message                               AS AlertMessage,
    a.alert_status                          AS AlertStatus,
    a.CreateDate                            AS RaisedAt,
    a.UpdateDate                            AS LastUpdatedAt,
    DATEDIFF(HOUR, a.CreateDate, GETDATE()) AS AgeHours
FROM Reconciliation.Alert a
JOIN Reconciliation.Operation o ON o.Id = a.operation_id
JOIN Ingestion.FileLine fl ON fl.Id = a.file_line_id
JOIN Ingestion.[File] f ON f.Id = fl.file_id;
GO

-- ============================================================================
-- VIEW 8: Daily Reconciliation Summary
-- ============================================================================
IF OBJECT_ID('Reporting.vw_DailyReconciliationSummary', 'V') IS NOT NULL
    DROP VIEW Reporting.vw_DailyReconciliationSummary;
GO
CREATE VIEW Reporting.vw_DailyReconciliationSummary AS
SELECT
    CAST(f.CreateDate AS DATE)              AS ReportDate,
    f.content_type                          AS ContentType,
    f.file_type                             AS FileType,

    COUNT(DISTINCT f.Id)                    AS TotalFiles,

    COUNT(CASE WHEN fl.line_type = 'D' THEN 1 END)
        AS TotalDetailLines,

    COUNT(CASE WHEN fl.line_type = 'D' AND fl.reconciliation_status = 'Success' THEN 1 END)
        AS ReconSuccessCount,
    COUNT(CASE WHEN fl.line_type = 'D' AND fl.reconciliation_status = 'Failed' THEN 1 END)
        AS ReconFailedCount,
    COUNT(CASE WHEN fl.line_type = 'D' AND fl.reconciliation_status IN ('Ready', 'Processing') THEN 1 END)
        AS ReconPendingCount,
    COUNT(CASE WHEN fl.line_type = 'D' AND fl.reconciliation_status IS NULL THEN 1 END)
        AS ReconNotEvaluatedCount,

    COUNT(CASE WHEN fl.line_type = 'D' AND fl.matched_clearing_line_id IS NOT NULL THEN 1 END)
        AS MatchedCount,
    COUNT(CASE WHEN fl.line_type = 'D' AND fl.matched_clearing_line_id IS NULL
               AND f.file_type = 'Card' THEN 1 END)
        AS UnmatchedCardCount,

    CASE
        WHEN COUNT(CASE WHEN fl.line_type = 'D' THEN 1 END) > 0
        THEN ROUND(
            CAST(COUNT(CASE WHEN fl.line_type = 'D' AND fl.reconciliation_status = 'Success' THEN 1 END) AS DECIMAL(18,4))
            / COUNT(CASE WHEN fl.line_type = 'D' THEN 1 END) * 100, 2)
        ELSE 0
    END AS SuccessRate

FROM Ingestion.[File] f
LEFT JOIN Ingestion.FileLine fl ON fl.file_id = f.Id
GROUP BY CAST(f.CreateDate AS DATE), f.content_type, f.file_type;
GO

-- ============================================================================
-- VIEW 9: Reconciliation Aging
-- ============================================================================
IF OBJECT_ID('Reporting.vw_ReconciliationAging', 'V') IS NOT NULL
    DROP VIEW Reporting.vw_ReconciliationAging;
GO
CREATE VIEW Reporting.vw_ReconciliationAging AS
SELECT
    CASE
        WHEN DATEDIFF(DAY, fl.CreateDate, GETDATE()) <= 1  THEN '0-1 Gun'
        WHEN DATEDIFF(DAY, fl.CreateDate, GETDATE()) <= 3  THEN '2-3 Gun'
        WHEN DATEDIFF(DAY, fl.CreateDate, GETDATE()) <= 7  THEN '4-7 Gun'
        WHEN DATEDIFF(DAY, fl.CreateDate, GETDATE()) <= 14 THEN '8-14 Gun'
        WHEN DATEDIFF(DAY, fl.CreateDate, GETDATE()) <= 30 THEN '15-30 Gun'
        ELSE '30+ Gun'
    END                                     AS AgeBucket,
    CASE
        WHEN DATEDIFF(DAY, fl.CreateDate, GETDATE()) <= 1  THEN 1
        WHEN DATEDIFF(DAY, fl.CreateDate, GETDATE()) <= 3  THEN 2
        WHEN DATEDIFF(DAY, fl.CreateDate, GETDATE()) <= 7  THEN 3
        WHEN DATEDIFF(DAY, fl.CreateDate, GETDATE()) <= 14 THEN 4
        WHEN DATEDIFF(DAY, fl.CreateDate, GETDATE()) <= 30 THEN 5
        ELSE 6
    END                                     AS AgeBucketOrder,
    f.content_type                          AS ContentType,
    f.file_type                             AS FileType,
    COUNT(*)                                AS OpenCount,
    MIN(fl.CreateDate)                      AS OldestRecordDate,
    MAX(fl.CreateDate)                      AS NewestRecordDate
FROM Ingestion.FileLine fl
JOIN Ingestion.[File] f ON f.Id = fl.file_id
WHERE fl.line_type = 'D'
  AND fl.reconciliation_status IN ('Ready', 'Processing', 'Failed')
  AND f.is_archived = 0
GROUP BY
    CASE
        WHEN DATEDIFF(DAY, fl.CreateDate, GETDATE()) <= 1  THEN '0-1 Gun'
        WHEN DATEDIFF(DAY, fl.CreateDate, GETDATE()) <= 3  THEN '2-3 Gun'
        WHEN DATEDIFF(DAY, fl.CreateDate, GETDATE()) <= 7  THEN '4-7 Gun'
        WHEN DATEDIFF(DAY, fl.CreateDate, GETDATE()) <= 14 THEN '8-14 Gun'
        WHEN DATEDIFF(DAY, fl.CreateDate, GETDATE()) <= 30 THEN '15-30 Gun'
        ELSE '30+ Gun'
    END,
    CASE
        WHEN DATEDIFF(DAY, fl.CreateDate, GETDATE()) <= 1  THEN 1
        WHEN DATEDIFF(DAY, fl.CreateDate, GETDATE()) <= 3  THEN 2
        WHEN DATEDIFF(DAY, fl.CreateDate, GETDATE()) <= 7  THEN 3
        WHEN DATEDIFF(DAY, fl.CreateDate, GETDATE()) <= 14 THEN 4
        WHEN DATEDIFF(DAY, fl.CreateDate, GETDATE()) <= 30 THEN 5
        ELSE 6
    END,
    f.content_type,
    f.file_type;
GO

-- ============================================================================
-- VIEW 10: Archive Audit Trail
-- ============================================================================
IF OBJECT_ID('Reporting.vw_ArchiveAuditTrail', 'V') IS NOT NULL
    DROP VIEW Reporting.vw_ArchiveAuditTrail;
GO
CREATE VIEW Reporting.vw_ArchiveAuditTrail AS
SELECT
    al.Id                                   AS ArchiveLogId,
    al.ingestion_file_id                    AS IngestionFileId,
    af.file_name                            AS FileName,
    af.file_type                            AS FileType,
    af.content_type                         AS ContentType,
    af.status                               AS FileStatus,
    af.processed_line_count                 AS ProcessedLineCount,
    af.successful_line_count                AS SuccessfulLineCount,
    af.failed_line_count                    AS FailedLineCount,
    al.status                               AS ArchiveStatus,
    al.message                              AS ArchiveMessage,
    al.failure_reasons_json                 AS FailureReasonsJson,
    al.filter_json                          AS FilterJson,
    al.CreateDate                           AS ArchivedAt,
    al.CreatedBy                            AS ArchivedBy,
    af.CreateDate                           AS FileIngestedAt,
    DATEDIFF(DAY, af.CreateDate, al.CreateDate) AS DaysToArchive
FROM Archive.ArchiveLog al
LEFT JOIN Archive.IngestionFile af ON af.Id = al.ingestion_file_id;
GO

-- ============================================================================
-- VIEW 11: Clearing Dispute Monitor
-- ============================================================================
IF OBJECT_ID('Reporting.vw_ClearingDisputeMonitor', 'V') IS NOT NULL
    DROP VIEW Reporting.vw_ClearingDisputeMonitor;
GO
CREATE VIEW Reporting.vw_ClearingDisputeMonitor AS

-- VISA
SELECT
    cvd.Id                                  AS ClearingDetailId,
    fl.Id                                   AS FileLineId,
    f.Id                                    AS FileId,
    f.file_name                             AS FileName,
    'Visa'                                  AS Network,
    cvd.txn_type                            AS TxnType,
    cvd.io_flag                             AS IoFlag,
    cvd.ocean_txn_guid                      AS OceanTxnGuid,
    cvd.rrn                                 AS Rrn,
    cvd.arn                                 AS Arn,
    cvd.card_no                             AS CardNo,
    cvd.card_dci                            AS CardDci,
    cvd.dispute_code                        AS DisputeCode,
    cvd.control_stat                        AS ControlStat,
    cvd.source_amount                       AS SourceAmount,
    cvd.source_currency                     AS SourceCurrency,
    cvd.destination_amount                  AS DestinationAmount,
    cvd.destination_currency                AS DestinationCurrency,
    cvd.merchant_name                       AS MerchantName,
    cvd.txn_date                            AS TxnDate,
    cvd.txn_time                            AS TxnTime,
    fl.reconciliation_status                AS ReconciliationStatus,
    fl.matched_clearing_line_id             AS MatchedClearingLineId,
    fl.CreateDate                           AS IngestedAt,
    f.is_archived                           AS IsArchived
FROM Ingestion.ClearingVisaDetail cvd
JOIN Ingestion.FileLine fl ON fl.Id = cvd.file_line_id
JOIN Ingestion.[File] f ON f.Id = fl.file_id
WHERE cvd.control_stat <> 'Normal' OR cvd.dispute_code <> 'None'

UNION ALL

-- MSC
SELECT
    cmd.Id, fl.Id, f.Id,
    f.file_name, 'Mastercard',
    cmd.txn_type, cmd.io_flag, cmd.ocean_txn_guid,
    cmd.rrn, cmd.arn, cmd.card_no, cmd.card_dci,
    cmd.dispute_code, cmd.control_stat,
    cmd.source_amount, cmd.source_currency,
    cmd.destination_amount, cmd.destination_currency,
    cmd.merchant_name, cmd.txn_date, cmd.txn_time,
    fl.reconciliation_status, fl.matched_clearing_line_id,
    fl.CreateDate, f.is_archived
FROM Ingestion.ClearingMscDetail cmd
JOIN Ingestion.FileLine fl ON fl.Id = cmd.file_line_id
JOIN Ingestion.[File] f ON f.Id = fl.file_id
WHERE cmd.control_stat <> 'Normal' OR cmd.dispute_code <> 'None'

UNION ALL

-- BKM
SELECT
    cbd.Id, fl.Id, f.Id,
    f.file_name, 'BKM',
    cbd.txn_type, cbd.io_flag, cbd.ocean_txn_guid,
    cbd.rrn, cbd.arn, cbd.card_no, cbd.card_dci,
    cbd.dispute_code, cbd.control_stat,
    cbd.source_amount, cbd.source_currency,
    cbd.destination_amount, cbd.destination_currency,
    cbd.merchant_name, cbd.txn_date, cbd.txn_time,
    fl.reconciliation_status, fl.matched_clearing_line_id,
    fl.CreateDate, f.is_archived
FROM Ingestion.ClearingBkmDetail cbd
JOIN Ingestion.FileLine fl ON fl.Id = cbd.file_line_id
JOIN Ingestion.[File] f ON f.Id = fl.file_id
WHERE cbd.control_stat <> 'Normal' OR cbd.dispute_code <> 'None';
GO


-- ############################################################################
-- PART 2: BASE TRANSACTION VIEWS
-- ############################################################################

-- ============================================================================
-- VIEW 12: Reporting.vw_BaseCardTransaction
-- Purpose: Normalized union of BKM/VISA/MSC card detail tables.
-- ============================================================================
IF OBJECT_ID('Reporting.vw_BaseCardTransaction', 'V') IS NOT NULL
    DROP VIEW Reporting.vw_BaseCardTransaction;
GO
CREATE VIEW Reporting.vw_BaseCardTransaction AS

-- ── BKM Card Transactions ───────────────────────────────────────────────────
SELECT
    f.Id                                        AS FileId,
    f.file_name                                 AS FileName,
    fl.Id                                       AS FileLineId,
    'BKM'                                       AS Network,

    cbd.ocean_txn_guid                          AS OceanTxnGuid,
    cbd.ocean_main_txn_guid                     AS OceanMainTxnGuid,
    cbd.rrn                                     AS Rrn,
    cbd.arn                                     AS Arn,
    cbd.provision_code                          AS ProvisionCode,
    cbd.card_no                                 AS CardNo,

    cbd.merchant_name                           AS MerchantName,
    cbd.merchant_city                           AS MerchantCity,
    cbd.merchant_country                        AS MerchantCountry,
    cbd.mcc                                     AS Mcc,

    cbd.transaction_date                        AS TransactionDate,
    cbd.transaction_time                        AS TransactionTime,
    cbd.value_date                              AS ValueDate,
    cbd.end_of_day_date                         AS EndOfDayDate,

    cbd.original_amount                         AS OriginalAmount,
    cbd.original_currency                       AS OriginalCurrency,
    cbd.settlement_amount                       AS SettlementAmount,
    cbd.settlement_currency                     AS SettlementCurrency,
    cbd.billing_amount                          AS BillingAmount,
    cbd.billing_currency                        AS BillingCurrency,
    cbd.tax1                                    AS Tax1,
    cbd.tax2                                    AS Tax2,
    cbd.cashback_amount                         AS CashbackAmount,
    cbd.surcharge_amount                        AS SurchargeAmount,

    cbd.txn_stat                                AS TxnStat,
    cbd.response_code                           AS ResponseCode,
    cbd.is_successful_txn                       AS IsSuccessfulTxn,
    cbd.is_txn_settle                           AS IsTxnSettle,

    fl.reconciliation_status                    AS ReconciliationStatus,
    fl.duplicate_status                         AS DuplicateStatus,
    fl.duplicate_group_id                       AS DuplicateGroupId,
    fl.matched_clearing_line_id                 AS MatchedClearingLineId,

    cbd.CreateDate                              AS CreateDate,
    cbd.RecordStatus                            AS RecordStatus

FROM Ingestion.CardBkmDetail cbd
INNER JOIN Ingestion.FileLine fl ON fl.Id = cbd.file_line_id
INNER JOIN Ingestion.[File] f   ON f.Id  = fl.file_id

UNION ALL

-- ── VISA Card Transactions ──────────────────────────────────────────────────
SELECT
    f.Id, f.file_name, fl.Id, 'VISA',
    cvd.ocean_txn_guid, cvd.ocean_main_txn_guid,
    cvd.rrn, cvd.arn, cvd.provision_code, cvd.card_no,
    cvd.merchant_name, cvd.merchant_city, cvd.merchant_country, cvd.mcc,
    cvd.transaction_date, cvd.transaction_time, cvd.value_date, cvd.end_of_day_date,
    cvd.original_amount, cvd.original_currency,
    cvd.settlement_amount, cvd.settlement_currency,
    cvd.billing_amount, cvd.billing_currency,
    cvd.tax1, cvd.tax2, cvd.cashback_amount, cvd.surcharge_amount,
    cvd.txn_stat, cvd.response_code, cvd.is_successful_txn, cvd.is_txn_settle,
    fl.reconciliation_status, fl.duplicate_status, fl.duplicate_group_id, fl.matched_clearing_line_id,
    cvd.CreateDate, cvd.RecordStatus
FROM Ingestion.CardVisaDetail cvd
INNER JOIN Ingestion.FileLine fl ON fl.Id = cvd.file_line_id
INNER JOIN Ingestion.[File] f   ON f.Id  = fl.file_id

UNION ALL

-- ── MSC (Mastercard) Card Transactions ──────────────────────────────────────
SELECT
    f.Id, f.file_name, fl.Id, 'MSC',
    cmd.ocean_txn_guid, cmd.ocean_main_txn_guid,
    cmd.rrn, cmd.arn, cmd.provision_code, cmd.card_no,
    cmd.merchant_name, cmd.merchant_city, cmd.merchant_country, cmd.mcc,
    cmd.transaction_date, cmd.transaction_time, cmd.value_date, cmd.end_of_day_date,
    cmd.original_amount, cmd.original_currency,
    cmd.settlement_amount, cmd.settlement_currency,
    cmd.billing_amount, cmd.billing_currency,
    cmd.tax1, cmd.tax2, cmd.cashback_amount, cmd.surcharge_amount,
    cmd.txn_stat, cmd.response_code, cmd.is_successful_txn, cmd.is_txn_settle,
    fl.reconciliation_status, fl.duplicate_status, fl.duplicate_group_id, fl.matched_clearing_line_id,
    cmd.CreateDate, cmd.RecordStatus
FROM Ingestion.CardMscDetail cmd
INNER JOIN Ingestion.FileLine fl ON fl.Id = cmd.file_line_id
INNER JOIN Ingestion.[File] f   ON f.Id  = fl.file_id;
GO


-- ============================================================================
-- VIEW 13: Reporting.vw_BaseClearingTransaction
-- Purpose: Normalized union of BKM/VISA/MSC clearing detail tables.
-- ============================================================================
IF OBJECT_ID('Reporting.vw_BaseClearingTransaction', 'V') IS NOT NULL
    DROP VIEW Reporting.vw_BaseClearingTransaction;
GO
CREATE VIEW Reporting.vw_BaseClearingTransaction AS

-- ── BKM Clearing Transactions ───────────────────────────────────────────────
SELECT
    f.Id                                        AS FileId,
    f.file_name                                 AS FileName,
    fl.Id                                       AS FileLineId,
    'BKM'                                       AS Network,

    cbd.ocean_txn_guid                          AS OceanTxnGuid,
    cbd.rrn                                     AS Rrn,
    cbd.arn                                     AS Arn,
    cbd.provision_code                          AS ProvisionCode,
    cbd.card_no                                 AS CardNo,

    cbd.txn_type                                AS TxnType,
    cbd.io_flag                                 AS IoFlag,
    cbd.control_stat                            AS ControlStat,
    cbd.dispute_code                            AS DisputeCode,

    cbd.source_amount                           AS SourceAmount,
    cbd.source_currency                         AS SourceCurrency,
    cbd.destination_amount                      AS DestinationAmount,
    cbd.destination_currency                    AS DestinationCurrency,
    cbd.cashback_amount                         AS CashbackAmount,
    cbd.reimbursement_amount                    AS ReimbursementAmount,

    cbd.merchant_name                           AS MerchantName,
    cbd.merchant_city                           AS MerchantCity,

    cbd.txn_date                                AS TxnDate,
    cbd.txn_time                                AS TxnTime,

    fl.reconciliation_status                    AS ReconciliationStatus,
    fl.matched_clearing_line_id                 AS MatchedClearingLineId,

    cbd.CreateDate                              AS CreateDate,
    cbd.RecordStatus                            AS RecordStatus

FROM Ingestion.ClearingBkmDetail cbd
INNER JOIN Ingestion.FileLine fl ON fl.Id = cbd.file_line_id
INNER JOIN Ingestion.[File] f   ON f.Id  = fl.file_id

UNION ALL

-- ── VISA Clearing Transactions ──────────────────────────────────────────────
SELECT
    f.Id, f.file_name, fl.Id, 'VISA',
    cvd.ocean_txn_guid, cvd.rrn, cvd.arn, cvd.provision_code, cvd.card_no,
    cvd.txn_type, cvd.io_flag, cvd.control_stat, cvd.dispute_code,
    cvd.source_amount, cvd.source_currency, cvd.destination_amount, cvd.destination_currency,
    cvd.cashback_amount, cvd.reimbursement_amount,
    cvd.merchant_name, cvd.merchant_city,
    cvd.txn_date, cvd.txn_time,
    fl.reconciliation_status, fl.matched_clearing_line_id,
    cvd.CreateDate, cvd.RecordStatus
FROM Ingestion.ClearingVisaDetail cvd
INNER JOIN Ingestion.FileLine fl ON fl.Id = cvd.file_line_id
INNER JOIN Ingestion.[File] f   ON f.Id  = fl.file_id

UNION ALL

-- ── MSC (Mastercard) Clearing Transactions ──────────────────────────────────
SELECT
    f.Id, f.file_name, fl.Id, 'MSC',
    cmd.ocean_txn_guid, cmd.rrn, cmd.arn, cmd.provision_code, cmd.card_no,
    cmd.txn_type, cmd.io_flag, cmd.control_stat, cmd.dispute_code,
    cmd.source_amount, cmd.source_currency, cmd.destination_amount, cmd.destination_currency,
    cmd.cashback_amount, cmd.reimbursement_amount,
    cmd.merchant_name, cmd.merchant_city,
    cmd.txn_date, cmd.txn_time,
    fl.reconciliation_status, fl.matched_clearing_line_id,
    cmd.CreateDate, cmd.RecordStatus
FROM Ingestion.ClearingMscDetail cmd
INNER JOIN Ingestion.FileLine fl ON fl.Id = cmd.file_line_id
INNER JOIN Ingestion.[File] f   ON f.Id  = fl.file_id;
GO


-- ############################################################################
-- PART 3: RECONCILIATION MATCHED PAIR VIEW
-- ############################################################################

-- ============================================================================
-- VIEW 14: Reporting.vw_ReconciliationMatchedPair
-- Purpose: Side-by-side card vs clearing comparison with mismatch flags.
-- ============================================================================
IF OBJECT_ID('Reporting.vw_ReconciliationMatchedPair', 'V') IS NOT NULL
    DROP VIEW Reporting.vw_ReconciliationMatchedPair;
GO
CREATE VIEW Reporting.vw_ReconciliationMatchedPair AS
SELECT
    -- ── IDENTIFIERS ─────────────────────────────────────────────────────────
    c.FileId                                        AS FileId,
    c.FileName                                      AS FileName,
    c.FileLineId                                    AS CardFileLineId,
    clr.FileLineId                                  AS ClearingFileLineId,
    c.Network                                       AS Network,

    -- ── CARD SIDE ───────────────────────────────────────────────────────────
    c.OceanTxnGuid                                  AS CardOceanTxnGuid,
    c.Rrn                                           AS CardRrn,
    c.Arn                                           AS CardArn,
    c.CardNo                                        AS CardCardNo,
    c.MerchantName                                  AS CardMerchantName,
    c.TransactionDate                               AS CardTransactionDate,
    c.OriginalAmount                                AS CardOriginalAmount,
    c.OriginalCurrency                              AS CardOriginalCurrency,
    c.SettlementAmount                              AS CardSettlementAmount,
    c.SettlementCurrency                            AS CardSettlementCurrency,
    c.BillingAmount                                 AS CardBillingAmount,
    c.IsSuccessfulTxn                               AS CardIsSuccessfulTxn,

    -- ── CLEARING SIDE ───────────────────────────────────────────────────────
    clr.OceanTxnGuid                                AS ClearingOceanTxnGuid,
    clr.Rrn                                         AS ClearingRrn,
    clr.Arn                                         AS ClearingArn,
    clr.CardNo                                      AS ClearingCardNo,
    clr.MerchantName                                AS ClearingMerchantName,
    clr.TxnDate                                     AS ClearingTxnDate,
    clr.SourceAmount                                AS ClearingSourceAmount,
    clr.SourceCurrency                              AS ClearingSourceCurrency,
    clr.DestinationAmount                           AS ClearingDestinationAmount,
    clr.DestinationCurrency                         AS ClearingDestinationCurrency,
    clr.ControlStat                                 AS ClearingControlStat,

    -- ── MATCH STATUS ────────────────────────────────────────────────────────
    CASE
        WHEN clr.FileLineId IS NULL THEN 'UNMATCHED_CARD'
        ELSE 'MATCHED'
    END                                             AS MatchStatus,

    -- ── COMPARISON FIELDS ───────────────────────────────────────────────────
    c.OriginalAmount - clr.SourceAmount             AS AmountDifference,

    CASE
        WHEN clr.FileLineId IS NULL THEN NULL
        WHEN ABS(ISNULL(c.OriginalAmount, 0) - ISNULL(clr.SourceAmount, 0)) > 0.01
            THEN CAST(1 AS BIT)
        ELSE CAST(0 AS BIT)
    END                                             AS HasAmountMismatch,

    CASE
        WHEN clr.FileLineId IS NULL THEN NULL
        WHEN ISNULL(c.OriginalCurrency, -1) <> ISNULL(clr.SourceCurrency, -1)
            THEN CAST(1 AS BIT)
        ELSE CAST(0 AS BIT)
    END                                             AS HasCurrencyMismatch,

    CASE
        WHEN clr.FileLineId IS NULL THEN NULL
        WHEN CAST(c.TransactionDate AS VARCHAR(20)) <> CAST(clr.TxnDate AS VARCHAR(20))
            THEN CAST(1 AS BIT)
        ELSE CAST(0 AS BIT)
    END                                             AS HasDateMismatch,

    CASE
        WHEN clr.FileLineId IS NULL THEN NULL
        WHEN c.IsSuccessfulTxn = 'Successful'
             AND ISNULL(clr.ControlStat, 'Normal') <> 'Normal'
            THEN CAST(1 AS BIT)
        WHEN ISNULL(c.IsSuccessfulTxn, 'Unsuccessful') = 'Unsuccessful'
             AND ISNULL(clr.ControlStat, 'Normal') = 'Normal'
            THEN CAST(1 AS BIT)
        ELSE CAST(0 AS BIT)
    END                                             AS HasStatusMismatch,

    -- ── RECON META ──────────────────────────────────────────────────────────
    c.ReconciliationStatus                          AS ReconciliationStatus,
    c.DuplicateStatus                               AS DuplicateStatus,

    -- ── AUDIT ───────────────────────────────────────────────────────────────
    c.CreateDate                                    AS CardCreateDate

FROM Reporting.vw_BaseCardTransaction c
LEFT JOIN Reporting.vw_BaseClearingTransaction clr
    ON c.MatchedClearingLineId = clr.FileLineId;
GO


-- ############################################################################
-- PART 4: RECONCILIATION BUSINESS VIEWS
-- ############################################################################

-- ============================================================================
-- VIEW 15: Reporting.vw_ReconciliationUnmatchedCard
-- Purpose: Card transactions with NO corresponding clearing record.
-- ============================================================================
IF OBJECT_ID('Reporting.vw_ReconciliationUnmatchedCard', 'V') IS NOT NULL
    DROP VIEW Reporting.vw_ReconciliationUnmatchedCard;
GO
CREATE VIEW Reporting.vw_ReconciliationUnmatchedCard AS
SELECT
    FileId, FileName, CardFileLineId, ClearingFileLineId, Network,
    CardOceanTxnGuid, CardRrn, CardArn, CardCardNo, CardMerchantName,
    CardTransactionDate, CardOriginalAmount, CardOriginalCurrency,
    CardSettlementAmount, CardSettlementCurrency, CardBillingAmount,
    CardIsSuccessfulTxn,
    ClearingOceanTxnGuid, ClearingRrn, ClearingArn, ClearingCardNo,
    ClearingMerchantName, ClearingTxnDate, ClearingSourceAmount,
    ClearingSourceCurrency, ClearingDestinationAmount,
    ClearingDestinationCurrency, ClearingControlStat,
    MatchStatus, AmountDifference, HasAmountMismatch, HasCurrencyMismatch,
    HasDateMismatch, HasStatusMismatch,
    ReconciliationStatus, DuplicateStatus, CardCreateDate
FROM Reporting.vw_ReconciliationMatchedPair
WHERE MatchStatus = 'UNMATCHED_CARD';
GO


-- ============================================================================
-- VIEW 16: Reporting.vw_ReconciliationAmountMismatch
-- Purpose: Matched pairs where card amount and clearing amount differ > 0.01
-- ============================================================================
IF OBJECT_ID('Reporting.vw_ReconciliationAmountMismatch', 'V') IS NOT NULL
    DROP VIEW Reporting.vw_ReconciliationAmountMismatch;
GO
CREATE VIEW Reporting.vw_ReconciliationAmountMismatch AS
SELECT
    FileId, FileName, CardFileLineId, ClearingFileLineId, Network,
    CardOceanTxnGuid, CardRrn, CardArn, CardCardNo, CardMerchantName,
    CardTransactionDate, CardOriginalAmount, CardOriginalCurrency,
    CardSettlementAmount, CardSettlementCurrency, CardBillingAmount,
    CardIsSuccessfulTxn,
    ClearingOceanTxnGuid, ClearingRrn, ClearingArn, ClearingCardNo,
    ClearingMerchantName, ClearingTxnDate, ClearingSourceAmount,
    ClearingSourceCurrency, ClearingDestinationAmount,
    ClearingDestinationCurrency, ClearingControlStat,
    MatchStatus, AmountDifference, HasAmountMismatch, HasCurrencyMismatch,
    HasDateMismatch, HasStatusMismatch,
    ReconciliationStatus, DuplicateStatus, CardCreateDate
FROM Reporting.vw_ReconciliationMatchedPair
WHERE MatchStatus = 'MATCHED'
  AND HasAmountMismatch = 1;
GO


-- ============================================================================
-- VIEW 17: Reporting.vw_ReconciliationStatusMismatch
-- Purpose: Matched pairs where success indicators disagree.
-- ============================================================================
IF OBJECT_ID('Reporting.vw_ReconciliationStatusMismatch', 'V') IS NOT NULL
    DROP VIEW Reporting.vw_ReconciliationStatusMismatch;
GO
CREATE VIEW Reporting.vw_ReconciliationStatusMismatch AS
SELECT
    FileId, FileName, CardFileLineId, ClearingFileLineId, Network,
    CardOceanTxnGuid, CardRrn, CardArn, CardCardNo, CardMerchantName,
    CardTransactionDate, CardOriginalAmount, CardOriginalCurrency,
    CardSettlementAmount, CardSettlementCurrency, CardBillingAmount,
    CardIsSuccessfulTxn,
    ClearingOceanTxnGuid, ClearingRrn, ClearingArn, ClearingCardNo,
    ClearingMerchantName, ClearingTxnDate, ClearingSourceAmount,
    ClearingSourceCurrency, ClearingDestinationAmount,
    ClearingDestinationCurrency, ClearingControlStat,
    MatchStatus, AmountDifference, HasAmountMismatch, HasCurrencyMismatch,
    HasDateMismatch, HasStatusMismatch,
    ReconciliationStatus, DuplicateStatus, CardCreateDate
FROM Reporting.vw_ReconciliationMatchedPair
WHERE MatchStatus = 'MATCHED'
  AND HasStatusMismatch = 1;
GO


-- ============================================================================
-- VIEW 18: Reporting.vw_ReconciliationCleanMatched
-- Purpose: Matched pairs that pass ALL comparison checks — fully reconciled.
-- ============================================================================
IF OBJECT_ID('Reporting.vw_ReconciliationCleanMatched', 'V') IS NOT NULL
    DROP VIEW Reporting.vw_ReconciliationCleanMatched;
GO
CREATE VIEW Reporting.vw_ReconciliationCleanMatched AS
SELECT
    FileId, FileName, CardFileLineId, ClearingFileLineId, Network,
    CardOceanTxnGuid, CardRrn, CardArn, CardCardNo, CardMerchantName,
    CardTransactionDate, CardOriginalAmount, CardOriginalCurrency,
    CardSettlementAmount, CardSettlementCurrency, CardBillingAmount,
    CardIsSuccessfulTxn,
    ClearingOceanTxnGuid, ClearingRrn, ClearingArn, ClearingCardNo,
    ClearingMerchantName, ClearingTxnDate, ClearingSourceAmount,
    ClearingSourceCurrency, ClearingDestinationAmount,
    ClearingDestinationCurrency, ClearingControlStat,
    MatchStatus, AmountDifference, HasAmountMismatch, HasCurrencyMismatch,
    HasDateMismatch, HasStatusMismatch,
    ReconciliationStatus, DuplicateStatus, CardCreateDate
FROM Reporting.vw_ReconciliationMatchedPair
WHERE MatchStatus = 'MATCHED'
  AND HasAmountMismatch   = 0
  AND HasCurrencyMismatch = 0
  AND HasDateMismatch     = 0;
GO


-- ============================================================================
-- VIEW 19: Reporting.vw_ReconciliationProblemRecords
-- Purpose: ALL records with at least one reconciliation issue.
-- ============================================================================
IF OBJECT_ID('Reporting.vw_ReconciliationProblemRecords', 'V') IS NOT NULL
    DROP VIEW Reporting.vw_ReconciliationProblemRecords;
GO
CREATE VIEW Reporting.vw_ReconciliationProblemRecords AS
SELECT
    FileId, FileName, CardFileLineId, ClearingFileLineId, Network,
    CardOceanTxnGuid, CardRrn, CardArn, CardCardNo, CardMerchantName,
    CardTransactionDate, CardOriginalAmount, CardOriginalCurrency,
    CardSettlementAmount, CardSettlementCurrency, CardBillingAmount,
    CardIsSuccessfulTxn,
    ClearingOceanTxnGuid, ClearingRrn, ClearingArn, ClearingCardNo,
    ClearingMerchantName, ClearingTxnDate, ClearingSourceAmount,
    ClearingSourceCurrency, ClearingDestinationAmount,
    ClearingDestinationCurrency, ClearingControlStat,
    MatchStatus, AmountDifference, HasAmountMismatch, HasCurrencyMismatch,
    HasDateMismatch, HasStatusMismatch,
    ReconciliationStatus, DuplicateStatus, CardCreateDate
FROM Reporting.vw_ReconciliationMatchedPair
WHERE MatchStatus = 'UNMATCHED_CARD'
   OR HasAmountMismatch   = 1
   OR HasCurrencyMismatch = 1
   OR HasDateMismatch     = 1
   OR HasStatusMismatch   = 1
   OR (DuplicateStatus IS NOT NULL AND DuplicateStatus <> 'Unique');
GO


-- ############################################################################
-- PART 5: RECONCILIATION SUMMARY VIEWS
-- ############################################################################

-- ============================================================================
-- VIEW 20: Reporting.vw_ReconciliationSummaryDaily
-- Purpose: One row per CardTransactionDate with reconciliation KPIs.
-- ============================================================================
IF OBJECT_ID('Reporting.vw_ReconciliationSummaryDaily', 'V') IS NOT NULL
    DROP VIEW Reporting.vw_ReconciliationSummaryDaily;
GO
CREATE VIEW Reporting.vw_ReconciliationSummaryDaily AS
SELECT
    CardTransactionDate                             AS TransactionDate,

    COUNT(*)                                        AS TotalCount,

    COUNT(CASE WHEN MatchStatus = 'MATCHED' THEN 1 END)
                                                    AS MatchedCount,
    COUNT(CASE WHEN MatchStatus = 'UNMATCHED_CARD' THEN 1 END)
                                                    AS UnmatchedCount,
    COUNT(CASE WHEN HasAmountMismatch = 1 THEN 1 END)
                                                    AS AmountMismatchCount,
    COUNT(CASE WHEN HasCurrencyMismatch = 1 THEN 1 END)
                                                    AS CurrencyMismatchCount,
    COUNT(CASE WHEN HasDateMismatch = 1 THEN 1 END)
                                                    AS DateMismatchCount,
    COUNT(CASE WHEN HasStatusMismatch = 1 THEN 1 END)
                                                    AS StatusMismatchCount,

    COUNT(CASE WHEN MatchStatus = 'UNMATCHED_CARD'
                 OR HasAmountMismatch   = 1
                 OR HasCurrencyMismatch = 1
                 OR HasDateMismatch     = 1
                 OR HasStatusMismatch   = 1
                 OR ISNULL(DuplicateStatus, 'Unique') <> 'Unique'
            THEN 1 END)                             AS ProblemCount,

    COUNT(CASE WHEN MatchStatus = 'MATCHED'
                AND HasAmountMismatch   = 0
                AND HasCurrencyMismatch = 0
                AND HasDateMismatch     = 0
                AND HasStatusMismatch   = 0
            THEN 1 END)                             AS CleanCount

FROM Reporting.vw_ReconciliationMatchedPair
GROUP BY CardTransactionDate;
GO


-- ============================================================================
-- VIEW 21: Reporting.vw_ReconciliationSummaryByNetwork
-- Purpose: One row per network (BKM / VISA / MSC) with reconciliation KPIs.
-- ============================================================================
IF OBJECT_ID('Reporting.vw_ReconciliationSummaryByNetwork', 'V') IS NOT NULL
    DROP VIEW Reporting.vw_ReconciliationSummaryByNetwork;
GO
CREATE VIEW Reporting.vw_ReconciliationSummaryByNetwork AS
SELECT
    Network                                         AS Network,

    COUNT(*)                                        AS TotalCount,

    COUNT(CASE WHEN MatchStatus = 'MATCHED' THEN 1 END)
                                                    AS MatchedCount,
    COUNT(CASE WHEN MatchStatus = 'UNMATCHED_CARD' THEN 1 END)
                                                    AS UnmatchedCount,
    COUNT(CASE WHEN HasAmountMismatch = 1 THEN 1 END)
                                                    AS AmountMismatchCount,
    COUNT(CASE WHEN HasCurrencyMismatch = 1 THEN 1 END)
                                                    AS CurrencyMismatchCount,
    COUNT(CASE WHEN HasDateMismatch = 1 THEN 1 END)
                                                    AS DateMismatchCount,
    COUNT(CASE WHEN HasStatusMismatch = 1 THEN 1 END)
                                                    AS StatusMismatchCount,

    COUNT(CASE WHEN MatchStatus = 'UNMATCHED_CARD'
                 OR HasAmountMismatch   = 1
                 OR HasCurrencyMismatch = 1
                 OR HasDateMismatch     = 1
                 OR HasStatusMismatch   = 1
                 OR ISNULL(DuplicateStatus, 'Unique') <> 'Unique'
            THEN 1 END)                             AS ProblemCount,

    COUNT(CASE WHEN MatchStatus = 'MATCHED'
                AND HasAmountMismatch   = 0
                AND HasCurrencyMismatch = 0
                AND HasDateMismatch     = 0
                AND HasStatusMismatch   = 0
            THEN 1 END)                             AS CleanCount

FROM Reporting.vw_ReconciliationMatchedPair
GROUP BY Network;
GO


-- ============================================================================
-- VIEW 22: Reporting.vw_ReconciliationSummaryByFile
-- Purpose: One row per ingestion file with reconciliation KPIs.
-- ============================================================================
IF OBJECT_ID('Reporting.vw_ReconciliationSummaryByFile', 'V') IS NOT NULL
    DROP VIEW Reporting.vw_ReconciliationSummaryByFile;
GO
CREATE VIEW Reporting.vw_ReconciliationSummaryByFile AS
SELECT
    FileId                                          AS FileId,
    FileName                                        AS FileName,

    COUNT(*)                                        AS TotalCount,

    COUNT(CASE WHEN MatchStatus = 'MATCHED' THEN 1 END)
                                                    AS MatchedCount,
    COUNT(CASE WHEN MatchStatus = 'UNMATCHED_CARD' THEN 1 END)
                                                    AS UnmatchedCount,
    COUNT(CASE WHEN HasAmountMismatch = 1 THEN 1 END)
                                                    AS AmountMismatchCount,
    COUNT(CASE WHEN HasCurrencyMismatch = 1 THEN 1 END)
                                                    AS CurrencyMismatchCount,
    COUNT(CASE WHEN HasDateMismatch = 1 THEN 1 END)
                                                    AS DateMismatchCount,
    COUNT(CASE WHEN HasStatusMismatch = 1 THEN 1 END)
                                                    AS StatusMismatchCount,

    COUNT(CASE WHEN MatchStatus = 'UNMATCHED_CARD'
                 OR HasAmountMismatch   = 1
                 OR HasCurrencyMismatch = 1
                 OR HasDateMismatch     = 1
                 OR HasStatusMismatch   = 1
                 OR ISNULL(DuplicateStatus, 'Unique') <> 'Unique'
            THEN 1 END)                             AS ProblemCount,

    COUNT(CASE WHEN MatchStatus = 'MATCHED'
                AND HasAmountMismatch   = 0
                AND HasCurrencyMismatch = 0
                AND HasDateMismatch     = 0
                AND HasStatusMismatch   = 0
            THEN 1 END)                             AS CleanCount

FROM Reporting.vw_ReconciliationMatchedPair
GROUP BY FileId, FileName;
GO


-- ============================================================================
-- VIEW 23: Reporting.vw_ReconciliationSummaryOverall
-- Purpose: Single-row grand total of all reconciliation KPIs.
-- ============================================================================
IF OBJECT_ID('Reporting.vw_ReconciliationSummaryOverall', 'V') IS NOT NULL
    DROP VIEW Reporting.vw_ReconciliationSummaryOverall;
GO
CREATE VIEW Reporting.vw_ReconciliationSummaryOverall AS
SELECT
    COUNT(*)                                        AS TotalCount,

    COUNT(CASE WHEN MatchStatus = 'MATCHED' THEN 1 END)
                                                    AS MatchedCount,
    COUNT(CASE WHEN MatchStatus = 'UNMATCHED_CARD' THEN 1 END)
                                                    AS UnmatchedCount,
    COUNT(CASE WHEN HasAmountMismatch = 1 THEN 1 END)
                                                    AS AmountMismatchCount,
    COUNT(CASE WHEN HasCurrencyMismatch = 1 THEN 1 END)
                                                    AS CurrencyMismatchCount,
    COUNT(CASE WHEN HasDateMismatch = 1 THEN 1 END)
                                                    AS DateMismatchCount,
    COUNT(CASE WHEN HasStatusMismatch = 1 THEN 1 END)
                                                    AS StatusMismatchCount,

    COUNT(CASE WHEN MatchStatus = 'UNMATCHED_CARD'
                 OR HasAmountMismatch   = 1
                 OR HasCurrencyMismatch = 1
                 OR HasDateMismatch     = 1
                 OR HasStatusMismatch   = 1
                 OR ISNULL(DuplicateStatus, 'Unique') <> 'Unique'
            THEN 1 END)                             AS ProblemCount,

    COUNT(CASE WHEN MatchStatus = 'MATCHED'
                AND HasAmountMismatch   = 0
                AND HasCurrencyMismatch = 0
                AND HasDateMismatch     = 0
                AND HasStatusMismatch   = 0
            THEN 1 END)                             AS CleanCount

FROM Reporting.vw_ReconciliationMatchedPair;
GO

