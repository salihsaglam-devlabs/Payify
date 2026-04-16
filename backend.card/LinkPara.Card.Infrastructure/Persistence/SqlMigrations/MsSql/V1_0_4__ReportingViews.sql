-- =====================================================================================
-- REPORTING VIEWS - MSSQL Migration
-- Version: V1_0_4
-- Purpose: Operational reporting semantic layer for File Ingestion, Reconciliation, Archive
-- =====================================================================================

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Reporting')
BEGIN
EXEC('CREATE SCHEMA Reporting');
END
GO

-- =====================================================================================
-- A. FILE INGESTION
-- =====================================================================================

-- -------------------------------------------------------------------------------------
-- A1. Dosya bazında genel operasyon özeti (LIVE + ARCHIVE)
-- -------------------------------------------------------------------------------------
CREATE OR ALTER VIEW Reporting.vw_ingestion_file_overview
AS
SELECT * FROM (
    -- LIVE
    SELECT
        'LIVE' AS data_scope,
        f.id AS file_id,
        f.file_key,
        f.file_name,
        f.source_type,
        f.file_type,
        f.content_type,
        f.status AS file_status,
        f.message AS file_message,
        f.expected_line_count,
        f.processed_line_count,
        f.successful_line_count,
        f.failed_line_count,
        f.last_processed_line_number,
        f.last_processed_byte_offset,
        f.is_archived,
        f.create_date AS file_created_at,
        f.update_date AS file_updated_at,
        CASE
            WHEN f.processed_line_count > 0
                THEN ROUND(CAST(f.successful_line_count AS DECIMAL(18,4)) / f.processed_line_count * 100, 2)
            ELSE 0
            END AS line_success_rate_pct,
        CASE
            WHEN f.processed_line_count > 0
                THEN ROUND(CAST(f.failed_line_count AS DECIMAL(18,4)) / f.processed_line_count * 100, 2)
            ELSE 0
            END AS line_fail_rate_pct,
        CASE
            WHEN f.expected_line_count > 0
                THEN ROUND(CAST(f.processed_line_count AS DECIMAL(18,4)) / f.expected_line_count * 100, 2)
            ELSE 0
            END AS completeness_pct,
        ISNULL(fl.total_line_count, 0) AS actual_line_count,
        ISNULL(fl.success_line_count, 0) AS actual_success_line_count,
        ISNULL(fl.failed_line_count, 0) AS actual_failed_line_count,
        ISNULL(fl.processing_line_count, 0) AS actual_processing_line_count,
        ISNULL(fl.duplicate_line_count, 0) AS duplicate_line_count,
        ISNULL(fl.recon_ready_count, 0) AS recon_ready_count,
        ISNULL(fl.recon_success_count, 0) AS recon_success_count,
        ISNULL(fl.recon_failed_count, 0) AS recon_failed_count,
        DATEDIFF(SECOND, f.create_date, ISNULL(f.update_date, f.create_date)) AS processing_duration_seconds
    FROM Ingestion.[File] f
    OUTER APPLY (
        SELECT
            COUNT(*) AS total_line_count,
            SUM(CASE WHEN line.status = 'Success' THEN 1 ELSE 0 END) AS success_line_count,
            SUM(CASE WHEN line.status = 'Failed' THEN 1 ELSE 0 END) AS failed_line_count,
            SUM(CASE WHEN line.status = 'Processing' THEN 1 ELSE 0 END) AS processing_line_count,
            SUM(CASE WHEN line.duplicate_status IN ('Secondary', 'Conflict') THEN 1 ELSE 0 END) AS duplicate_line_count,
            SUM(CASE WHEN line.reconciliation_status = 'Ready' THEN 1 ELSE 0 END) AS recon_ready_count,
            SUM(CASE WHEN line.reconciliation_status = 'Success' THEN 1 ELSE 0 END) AS recon_success_count,
            SUM(CASE WHEN line.reconciliation_status = 'Failed' THEN 1 ELSE 0 END) AS recon_failed_count
        FROM Ingestion.FileLine line
        WHERE line.file_id = f.id
    ) fl

    UNION ALL

    -- ARCHIVE
    SELECT
        'ARCHIVE' AS data_scope,
        f.id AS file_id,
        f.file_key,
        f.file_name,
        f.source_type,
        f.file_type,
        f.content_type,
        f.status AS file_status,
        f.message AS file_message,
        f.expected_line_count,
        f.processed_line_count,
        f.successful_line_count,
        f.failed_line_count,
        f.last_processed_line_number,
        f.last_processed_byte_offset,
        f.is_archived,
        f.create_date AS file_created_at,
        f.update_date AS file_updated_at,
        CASE
            WHEN f.processed_line_count > 0
                THEN ROUND(CAST(f.successful_line_count AS DECIMAL(18,4)) / f.processed_line_count * 100, 2)
            ELSE 0
            END AS line_success_rate_pct,
        CASE
            WHEN f.processed_line_count > 0
                THEN ROUND(CAST(f.failed_line_count AS DECIMAL(18,4)) / f.processed_line_count * 100, 2)
            ELSE 0
            END AS line_fail_rate_pct,
        CASE
            WHEN f.expected_line_count > 0
                THEN ROUND(CAST(f.processed_line_count AS DECIMAL(18,4)) / f.expected_line_count * 100, 2)
            ELSE 0
            END AS completeness_pct,
        ISNULL(fl.total_line_count, 0) AS actual_line_count,
        ISNULL(fl.success_line_count, 0) AS actual_success_line_count,
        ISNULL(fl.failed_line_count, 0) AS actual_failed_line_count,
        ISNULL(fl.processing_line_count, 0) AS actual_processing_line_count,
        ISNULL(fl.duplicate_line_count, 0) AS duplicate_line_count,
        ISNULL(fl.recon_ready_count, 0) AS recon_ready_count,
        ISNULL(fl.recon_success_count, 0) AS recon_success_count,
        ISNULL(fl.recon_failed_count, 0) AS recon_failed_count,
        DATEDIFF(SECOND, f.create_date, ISNULL(f.update_date, f.create_date)) AS processing_duration_seconds
    FROM Archive.IngestionFile f
    OUTER APPLY (
        SELECT
            COUNT(*) AS total_line_count,
            SUM(CASE WHEN line.status = 'Success' THEN 1 ELSE 0 END) AS success_line_count,
            SUM(CASE WHEN line.status = 'Failed' THEN 1 ELSE 0 END) AS failed_line_count,
            SUM(CASE WHEN line.status = 'Processing' THEN 1 ELSE 0 END) AS processing_line_count,
            SUM(CASE WHEN line.duplicate_status IN ('Secondary', 'Conflict') THEN 1 ELSE 0 END) AS duplicate_line_count,
            SUM(CASE WHEN line.reconciliation_status = 'Ready' THEN 1 ELSE 0 END) AS recon_ready_count,
            SUM(CASE WHEN line.reconciliation_status = 'Success' THEN 1 ELSE 0 END) AS recon_success_count,
            SUM(CASE WHEN line.reconciliation_status = 'Failed' THEN 1 ELSE 0 END) AS recon_failed_count
        FROM Archive.IngestionFileLine line
        WHERE line.file_id = f.id
    ) fl
) combined;
GO

-- -------------------------------------------------------------------------------------
-- A2. Dosya kalitesi ve duplicate etkisi (LIVE + ARCHIVE)
-- -------------------------------------------------------------------------------------
CREATE OR ALTER VIEW Reporting.vw_ingestion_file_quality
AS
SELECT * FROM (
    -- LIVE
    SELECT
        'LIVE' AS data_scope,
        f.id AS file_id,
        f.file_name,
        f.file_type,
        f.content_type,
        f.status AS file_status,
        f.create_date AS file_created_at,
        ISNULL(x.total_line_count, 0) AS total_line_count,
        ISNULL(x.success_line_count, 0) AS success_line_count,
        ISNULL(x.failed_line_count, 0) AS failed_line_count,
        ISNULL(x.processing_line_count, 0) AS processing_line_count,
        ISNULL(x.unique_count, 0) AS duplicate_unique_count,
        ISNULL(x.primary_count, 0) AS duplicate_primary_count,
        ISNULL(x.secondary_count, 0) AS duplicate_secondary_count,
        ISNULL(x.conflict_count, 0) AS duplicate_conflict_count,
        ISNULL(x.total_retry_count, 0) AS total_retry_count,
        ISNULL(x.lines_with_retry_count, 0) AS lines_with_retry_count,
        CASE
            WHEN ISNULL(x.total_line_count, 0) > 0
                THEN ROUND(CAST(x.failed_line_count AS DECIMAL(18,4)) / x.total_line_count * 100, 2)
            ELSE 0
            END AS error_rate_pct,
        CASE
            WHEN ISNULL(x.total_line_count, 0) > 0
                THEN ROUND(CAST(ISNULL(x.secondary_count, 0) + ISNULL(x.conflict_count, 0) AS DECIMAL(18,4)) / x.total_line_count * 100, 2)
            ELSE 0
            END AS duplicate_impact_pct
    FROM Ingestion.[File] f
    OUTER APPLY (
        SELECT
            COUNT(*) AS total_line_count,
            SUM(CASE WHEN line.status = 'Success' THEN 1 ELSE 0 END) AS success_line_count,
            SUM(CASE WHEN line.status = 'Failed' THEN 1 ELSE 0 END) AS failed_line_count,
            SUM(CASE WHEN line.status = 'Processing' THEN 1 ELSE 0 END) AS processing_line_count,
            SUM(CASE WHEN line.duplicate_status = 'Unique' THEN 1 ELSE 0 END) AS unique_count,
            SUM(CASE WHEN line.duplicate_status = 'Primary' THEN 1 ELSE 0 END) AS primary_count,
            SUM(CASE WHEN line.duplicate_status = 'Secondary' THEN 1 ELSE 0 END) AS secondary_count,
            SUM(CASE WHEN line.duplicate_status = 'Conflict' THEN 1 ELSE 0 END) AS conflict_count,
            SUM(line.retry_count) AS total_retry_count,
            SUM(CASE WHEN line.retry_count > 0 THEN 1 ELSE 0 END) AS lines_with_retry_count
        FROM Ingestion.FileLine line
        WHERE line.file_id = f.id
    ) x

    UNION ALL

    -- ARCHIVE
    SELECT
        'ARCHIVE' AS data_scope,
        f.id AS file_id,
        f.file_name,
        f.file_type,
        f.content_type,
        f.status AS file_status,
        f.create_date AS file_created_at,
        ISNULL(x.total_line_count, 0) AS total_line_count,
        ISNULL(x.success_line_count, 0) AS success_line_count,
        ISNULL(x.failed_line_count, 0) AS failed_line_count,
        ISNULL(x.processing_line_count, 0) AS processing_line_count,
        ISNULL(x.unique_count, 0) AS duplicate_unique_count,
        ISNULL(x.primary_count, 0) AS duplicate_primary_count,
        ISNULL(x.secondary_count, 0) AS duplicate_secondary_count,
        ISNULL(x.conflict_count, 0) AS duplicate_conflict_count,
        ISNULL(x.total_retry_count, 0) AS total_retry_count,
        ISNULL(x.lines_with_retry_count, 0) AS lines_with_retry_count,
        CASE
            WHEN ISNULL(x.total_line_count, 0) > 0
                THEN ROUND(CAST(x.failed_line_count AS DECIMAL(18,4)) / x.total_line_count * 100, 2)
            ELSE 0
            END AS error_rate_pct,
        CASE
            WHEN ISNULL(x.total_line_count, 0) > 0
                THEN ROUND(CAST(ISNULL(x.secondary_count, 0) + ISNULL(x.conflict_count, 0) AS DECIMAL(18,4)) / x.total_line_count * 100, 2)
            ELSE 0
            END AS duplicate_impact_pct
    FROM Archive.IngestionFile f
    OUTER APPLY (
        SELECT
            COUNT(*) AS total_line_count,
            SUM(CASE WHEN line.status = 'Success' THEN 1 ELSE 0 END) AS success_line_count,
            SUM(CASE WHEN line.status = 'Failed' THEN 1 ELSE 0 END) AS failed_line_count,
            SUM(CASE WHEN line.status = 'Processing' THEN 1 ELSE 0 END) AS processing_line_count,
            SUM(CASE WHEN line.duplicate_status = 'Unique' THEN 1 ELSE 0 END) AS unique_count,
            SUM(CASE WHEN line.duplicate_status = 'Primary' THEN 1 ELSE 0 END) AS primary_count,
            SUM(CASE WHEN line.duplicate_status = 'Secondary' THEN 1 ELSE 0 END) AS secondary_count,
            SUM(CASE WHEN line.duplicate_status = 'Conflict' THEN 1 ELSE 0 END) AS conflict_count,
            SUM(line.retry_count) AS total_retry_count,
            SUM(CASE WHEN line.retry_count > 0 THEN 1 ELSE 0 END) AS lines_with_retry_count
        FROM Archive.IngestionFileLine line
        WHERE line.file_id = f.id
    ) x
) combined;
GO

-- -------------------------------------------------------------------------------------
-- A3. Günlük ingestion trendi (LIVE + ARCHIVE)
-- -------------------------------------------------------------------------------------
CREATE OR ALTER VIEW Reporting.vw_ingestion_daily_summary
AS
SELECT
    src.data_scope,
    src.report_date,
    src.content_type,
    src.file_type,
    SUM(src.file_count) AS file_count,
    SUM(src.success_file_count) AS success_file_count,
    SUM(src.failed_file_count) AS failed_file_count,
    SUM(src.processing_file_count) AS processing_file_count,
    SUM(src.expected_line_count) AS expected_line_count,
    SUM(src.processed_line_count) AS processed_line_count,
    SUM(src.successful_line_count) AS successful_line_count,
    SUM(src.failed_line_count) AS failed_line_count,
    CASE
        WHEN SUM(src.processed_line_count) > 0
            THEN ROUND(CAST(SUM(src.successful_line_count) AS DECIMAL(18,4)) / SUM(src.processed_line_count) * 100, 2)
        ELSE 0
        END AS processed_line_success_rate_pct
FROM (
    -- LIVE
    SELECT
        'LIVE' AS data_scope,
        CAST(f.create_date AS DATE) AS report_date,
        f.content_type,
        f.file_type,
        COUNT(*) AS file_count,
        SUM(CASE WHEN f.status = 'Success' THEN 1 ELSE 0 END) AS success_file_count,
        SUM(CASE WHEN f.status = 'Failed' THEN 1 ELSE 0 END) AS failed_file_count,
        SUM(CASE WHEN f.status = 'Processing' THEN 1 ELSE 0 END) AS processing_file_count,
        SUM(f.expected_line_count) AS expected_line_count,
        SUM(f.processed_line_count) AS processed_line_count,
        SUM(f.successful_line_count) AS successful_line_count,
        SUM(f.failed_line_count) AS failed_line_count
    FROM Ingestion.[File] f
    GROUP BY CAST(f.create_date AS DATE), f.content_type, f.file_type

    UNION ALL

    -- ARCHIVE
    SELECT
        'ARCHIVE' AS data_scope,
        CAST(f.create_date AS DATE) AS report_date,
        f.content_type,
        f.file_type,
        COUNT(*) AS file_count,
        SUM(CASE WHEN f.status = 'Success' THEN 1 ELSE 0 END) AS success_file_count,
        SUM(CASE WHEN f.status = 'Failed' THEN 1 ELSE 0 END) AS failed_file_count,
        SUM(CASE WHEN f.status = 'Processing' THEN 1 ELSE 0 END) AS processing_file_count,
        SUM(f.expected_line_count) AS expected_line_count,
        SUM(f.processed_line_count) AS processed_line_count,
        SUM(f.successful_line_count) AS successful_line_count,
        SUM(f.failed_line_count) AS failed_line_count
    FROM Archive.IngestionFile f
    GROUP BY CAST(f.create_date AS DATE), f.content_type, f.file_type
) src
GROUP BY src.data_scope, src.report_date, src.content_type, src.file_type;
GO

-- -------------------------------------------------------------------------------------
-- A4. Network x file type matrisi (LIVE + ARCHIVE)
-- -------------------------------------------------------------------------------------
CREATE OR ALTER VIEW Reporting.vw_ingestion_network_matrix
AS
SELECT
    src.data_scope,
    src.content_type,
    src.file_type,
    SUM(src.file_count) AS file_count,
    SUM(src.success_file_count) AS success_file_count,
    SUM(src.failed_file_count) AS failed_file_count,
    SUM(src.processed_line_count) AS processed_line_count,
    SUM(src.successful_line_count) AS successful_line_count,
    SUM(src.failed_line_count) AS failed_line_count,
    MIN(src.first_file_at) AS first_file_at,
    MAX(src.last_file_at) AS last_file_at
FROM (
    -- LIVE
    SELECT
        'LIVE' AS data_scope,
        f.content_type,
        f.file_type,
        COUNT(*) AS file_count,
        SUM(CASE WHEN f.status = 'Success' THEN 1 ELSE 0 END) AS success_file_count,
        SUM(CASE WHEN f.status = 'Failed' THEN 1 ELSE 0 END) AS failed_file_count,
        SUM(f.processed_line_count) AS processed_line_count,
        SUM(f.successful_line_count) AS successful_line_count,
        SUM(f.failed_line_count) AS failed_line_count,
        MIN(f.create_date) AS first_file_at,
        MAX(f.create_date) AS last_file_at
    FROM Ingestion.[File] f
    GROUP BY f.content_type, f.file_type

    UNION ALL

    -- ARCHIVE
    SELECT
        'ARCHIVE' AS data_scope,
        f.content_type,
        f.file_type,
        COUNT(*) AS file_count,
        SUM(CASE WHEN f.status = 'Success' THEN 1 ELSE 0 END) AS success_file_count,
        SUM(CASE WHEN f.status = 'Failed' THEN 1 ELSE 0 END) AS failed_file_count,
        SUM(f.processed_line_count) AS processed_line_count,
        SUM(f.successful_line_count) AS successful_line_count,
        SUM(f.failed_line_count) AS failed_line_count,
        MIN(f.create_date) AS first_file_at,
        MAX(f.create_date) AS last_file_at
    FROM Archive.IngestionFile f
    GROUP BY f.content_type, f.file_type
) src
GROUP BY src.data_scope, src.content_type, src.file_type;
GO

-- -------------------------------------------------------------------------------------
-- A5. Problem hotspot dosyaları (LIVE + ARCHIVE)
-- -------------------------------------------------------------------------------------
CREATE OR ALTER VIEW Reporting.vw_ingestion_exception_hotspots
AS
SELECT * FROM (
    -- LIVE
    SELECT
        'LIVE' AS data_scope,
        f.id AS file_id,
        f.file_name,
        f.source_type,
        f.file_type,
        f.content_type,
        f.status AS file_status,
        f.message AS file_message,
        f.create_date AS file_created_at,
        f.failed_line_count,
        f.processed_line_count,
        ISNULL(x.total_retry_count, 0) AS total_retry_count,
        ISNULL(x.max_retry_count, 0) AS max_retry_count,
        ISNULL(x.error_message_count, 0) AS distinct_error_message_count,
        CASE
            WHEN f.status = 'Failed' THEN 'CRITICAL'
            WHEN f.processed_line_count > 0 AND CAST(f.failed_line_count AS DECIMAL(18,4)) / f.processed_line_count >= 0.20 THEN 'HIGH'
            WHEN f.failed_line_count > 0 THEN 'MEDIUM'
            ELSE 'LOW'
            END AS severity_level
    FROM Ingestion.[File] f
    OUTER APPLY (
        SELECT
            SUM(line.retry_count) AS total_retry_count,
            MAX(line.retry_count) AS max_retry_count,
            COUNT(DISTINCT CASE WHEN line.status = 'Failed' THEN line.message END) AS error_message_count
        FROM Ingestion.FileLine line
        WHERE line.file_id = f.id
    ) x
    WHERE f.status = 'Failed'
       OR f.failed_line_count > 0

    UNION ALL

    -- ARCHIVE
    SELECT
        'ARCHIVE' AS data_scope,
        f.id AS file_id,
        f.file_name,
        f.source_type,
        f.file_type,
        f.content_type,
        f.status AS file_status,
        f.message AS file_message,
        f.create_date AS file_created_at,
        f.failed_line_count,
        f.processed_line_count,
        ISNULL(x.total_retry_count, 0) AS total_retry_count,
        ISNULL(x.max_retry_count, 0) AS max_retry_count,
        ISNULL(x.error_message_count, 0) AS distinct_error_message_count,
        CASE
            WHEN f.status = 'Failed' THEN 'CRITICAL'
            WHEN f.processed_line_count > 0 AND CAST(f.failed_line_count AS DECIMAL(18,4)) / f.processed_line_count >= 0.20 THEN 'HIGH'
            WHEN f.failed_line_count > 0 THEN 'MEDIUM'
            ELSE 'LOW'
            END AS severity_level
    FROM Archive.IngestionFile f
    OUTER APPLY (
        SELECT
            SUM(line.retry_count) AS total_retry_count,
            MAX(line.retry_count) AS max_retry_count,
            COUNT(DISTINCT CASE WHEN line.status = 'Failed' THEN line.message END) AS error_message_count
        FROM Archive.IngestionFileLine line
        WHERE line.file_id = f.id
    ) x
    WHERE f.status = 'Failed'
       OR f.failed_line_count > 0
) combined;
GO

-- =====================================================================================
-- B. RECONCILIATION PROCESS
-- =====================================================================================

-- -------------------------------------------------------------------------------------
-- B1. Günlük mutabakat süreç özeti (LIVE + ARCHIVE)
-- -------------------------------------------------------------------------------------
CREATE OR ALTER VIEW Reporting.vw_recon_daily_overview
AS
WITH dates AS (
    SELECT DISTINCT CAST(create_date AS DATE) AS report_date, 'LIVE' AS data_scope FROM Reconciliation.Evaluation
    UNION
    SELECT DISTINCT CAST(create_date AS DATE), 'LIVE' FROM Reconciliation.Operation
    UNION
    SELECT DISTINCT CAST(create_date AS DATE), 'LIVE' FROM Reconciliation.Alert
    UNION
    SELECT DISTINCT CAST(create_date AS DATE), 'LIVE' FROM Reconciliation.Review
    UNION
    SELECT DISTINCT CAST(create_date AS DATE), 'ARCHIVE' FROM Archive.ReconciliationEvaluation
    UNION
    SELECT DISTINCT CAST(create_date AS DATE), 'ARCHIVE' FROM Archive.ReconciliationOperation
    UNION
    SELECT DISTINCT CAST(create_date AS DATE), 'ARCHIVE' FROM Archive.ReconciliationAlert
    UNION
    SELECT DISTINCT CAST(create_date AS DATE), 'ARCHIVE' FROM Archive.ReconciliationReview
)
SELECT
    d.data_scope,
    d.report_date,

    ISNULL(ev.total_evaluation_count, 0) AS total_evaluation_count,
    ISNULL(ev.completed_evaluation_count, 0) AS completed_evaluation_count,
    ISNULL(ev.failed_evaluation_count, 0) AS failed_evaluation_count,

    ISNULL(op.total_operation_count, 0) AS total_operation_count,
    ISNULL(op.completed_operation_count, 0) AS completed_operation_count,
    ISNULL(op.failed_operation_count, 0) AS failed_operation_count,
    ISNULL(op.blocked_operation_count, 0) AS blocked_operation_count,
    ISNULL(op.planned_operation_count, 0) AS planned_operation_count,
    ISNULL(op.manual_operation_count, 0) AS manual_operation_count,

    ISNULL(ex.total_execution_count, 0) AS total_execution_count,
    ISNULL(ex.completed_execution_count, 0) AS completed_execution_count,
    ISNULL(ex.failed_execution_count, 0) AS failed_execution_count,
    ISNULL(ex.avg_execution_duration_seconds, 0) AS avg_execution_duration_seconds,

    ISNULL(rv.pending_review_count, 0) AS pending_review_count,
    ISNULL(rv.approved_review_count, 0) AS approved_review_count,
    ISNULL(rv.rejected_review_count, 0) AS rejected_review_count,

    ISNULL(al.pending_alert_count, 0) AS pending_alert_count,
    ISNULL(al.failed_alert_count, 0) AS failed_alert_count,

    CASE
        WHEN ISNULL(op.total_operation_count, 0) > 0
            THEN ROUND(CAST(op.completed_operation_count AS DECIMAL(18,4)) / op.total_operation_count * 100, 2)
        ELSE 0
        END AS operation_success_rate_pct
FROM dates d
         LEFT JOIN (
    SELECT 'LIVE' AS data_scope, CAST(create_date AS DATE) AS report_date,
        COUNT(*) AS total_evaluation_count,
        SUM(CASE WHEN status = 'Completed' THEN 1 ELSE 0 END) AS completed_evaluation_count,
        SUM(CASE WHEN status = 'Failed' THEN 1 ELSE 0 END) AS failed_evaluation_count
    FROM Reconciliation.Evaluation
    GROUP BY CAST(create_date AS DATE)
    UNION ALL
    SELECT 'ARCHIVE', CAST(create_date AS DATE),
        COUNT(*), SUM(CASE WHEN status = 'Completed' THEN 1 ELSE 0 END), SUM(CASE WHEN status = 'Failed' THEN 1 ELSE 0 END)
    FROM Archive.ReconciliationEvaluation
    GROUP BY CAST(create_date AS DATE)
) ev ON ev.report_date = d.report_date AND ev.data_scope = d.data_scope
         LEFT JOIN (
    SELECT 'LIVE' AS data_scope, CAST(create_date AS DATE) AS report_date,
        COUNT(*) AS total_operation_count,
        SUM(CASE WHEN status = 'Completed' THEN 1 ELSE 0 END) AS completed_operation_count,
        SUM(CASE WHEN status = 'Failed' THEN 1 ELSE 0 END) AS failed_operation_count,
        SUM(CASE WHEN status = 'Blocked' THEN 1 ELSE 0 END) AS blocked_operation_count,
        SUM(CASE WHEN status = 'Planned' THEN 1 ELSE 0 END) AS planned_operation_count,
        SUM(CASE WHEN is_manual = 1 THEN 1 ELSE 0 END) AS manual_operation_count
    FROM Reconciliation.Operation
    GROUP BY CAST(create_date AS DATE)
    UNION ALL
    SELECT 'ARCHIVE', CAST(create_date AS DATE),
        COUNT(*), SUM(CASE WHEN status = 'Completed' THEN 1 ELSE 0 END), SUM(CASE WHEN status = 'Failed' THEN 1 ELSE 0 END),
        SUM(CASE WHEN status = 'Blocked' THEN 1 ELSE 0 END), SUM(CASE WHEN status = 'Planned' THEN 1 ELSE 0 END),
        SUM(CASE WHEN is_manual = 1 THEN 1 ELSE 0 END)
    FROM Archive.ReconciliationOperation
    GROUP BY CAST(create_date AS DATE)
) op ON op.report_date = d.report_date AND op.data_scope = d.data_scope
         LEFT JOIN (
    SELECT 'LIVE' AS data_scope, CAST(started_at AS DATE) AS report_date,
        COUNT(*) AS total_execution_count,
        SUM(CASE WHEN status = 'Completed' THEN 1 ELSE 0 END) AS completed_execution_count,
        SUM(CASE WHEN status = 'Failed' THEN 1 ELSE 0 END) AS failed_execution_count,
        ROUND(AVG(CASE WHEN finished_at IS NOT NULL THEN DATEDIFF(SECOND, started_at, finished_at) END), 2) AS avg_execution_duration_seconds
    FROM Reconciliation.OperationExecution
    GROUP BY CAST(started_at AS DATE)
    UNION ALL
    SELECT 'ARCHIVE', CAST(started_at AS DATE),
        COUNT(*), SUM(CASE WHEN status = 'Completed' THEN 1 ELSE 0 END), SUM(CASE WHEN status = 'Failed' THEN 1 ELSE 0 END),
        ROUND(AVG(CASE WHEN finished_at IS NOT NULL THEN DATEDIFF(SECOND, started_at, finished_at) END), 2)
    FROM Archive.ReconciliationOperationExecution
    GROUP BY CAST(started_at AS DATE)
) ex ON ex.report_date = d.report_date AND ex.data_scope = d.data_scope
         LEFT JOIN (
    SELECT 'LIVE' AS data_scope, CAST(create_date AS DATE) AS report_date,
        SUM(CASE WHEN decision = 'Pending' THEN 1 ELSE 0 END) AS pending_review_count,
        SUM(CASE WHEN decision = 'Approved' THEN 1 ELSE 0 END) AS approved_review_count,
        SUM(CASE WHEN decision = 'Rejected' THEN 1 ELSE 0 END) AS rejected_review_count
    FROM Reconciliation.Review
    GROUP BY CAST(create_date AS DATE)
    UNION ALL
    SELECT 'ARCHIVE', CAST(create_date AS DATE),
        SUM(CASE WHEN decision = 'Pending' THEN 1 ELSE 0 END), SUM(CASE WHEN decision = 'Approved' THEN 1 ELSE 0 END),
        SUM(CASE WHEN decision = 'Rejected' THEN 1 ELSE 0 END)
    FROM Archive.ReconciliationReview
    GROUP BY CAST(create_date AS DATE)
) rv ON rv.report_date = d.report_date AND rv.data_scope = d.data_scope
         LEFT JOIN (
    SELECT 'LIVE' AS data_scope, CAST(create_date AS DATE) AS report_date,
        SUM(CASE WHEN alert_status = 'Pending' THEN 1 ELSE 0 END) AS pending_alert_count,
        SUM(CASE WHEN alert_status = 'Failed' THEN 1 ELSE 0 END) AS failed_alert_count
    FROM Reconciliation.Alert
    GROUP BY CAST(create_date AS DATE)
    UNION ALL
    SELECT 'ARCHIVE', CAST(create_date AS DATE),
        SUM(CASE WHEN alert_status = 'Pending' THEN 1 ELSE 0 END), SUM(CASE WHEN alert_status = 'Failed' THEN 1 ELSE 0 END)
    FROM Archive.ReconciliationAlert
    GROUP BY CAST(create_date AS DATE)
) al ON al.report_date = d.report_date AND al.data_scope = d.data_scope;
GO

-- -------------------------------------------------------------------------------------
-- B2. Açık mutabakat işleri
-- -------------------------------------------------------------------------------------
CREATE OR ALTER VIEW Reporting.vw_recon_open_items
AS
SELECT
    op.id AS operation_id,
    op.file_line_id,
    op.evaluation_id,
    op.group_id,
    op.sequence_number,
    op.parent_sequence_number,
    op.code AS operation_code,
    op.branch,
    op.is_manual,
    op.status AS operation_status,
    op.retry_count,
    op.max_retry_count,
    op.next_attempt_at,
    op.lease_owner,
    op.lease_expires_at,
    op.last_error,
    op.create_date AS operation_created_at,
    op.update_date AS operation_updated_at,
    ev.status AS evaluation_status,
    ev.operation_count AS evaluation_operation_count,
    ROUND(CAST(DATEDIFF(MINUTE, op.create_date, GETUTCDATE()) AS DECIMAL(18,2)) / 60, 1) AS age_hours
FROM Reconciliation.Operation op
         JOIN Reconciliation.Evaluation ev ON ev.id = op.evaluation_id
WHERE op.status IN ('Planned', 'Blocked', 'Executing');
GO

-- -------------------------------------------------------------------------------------
-- B3. Aging dağılımı
-- -------------------------------------------------------------------------------------
CREATE OR ALTER VIEW Reporting.vw_recon_open_item_aging
AS
SELECT
    bucket_name,
    COUNT(*) AS item_count,
    SUM(CASE WHEN operation_status = 'Planned' THEN 1 ELSE 0 END) AS planned_count,
    SUM(CASE WHEN operation_status = 'Blocked' THEN 1 ELSE 0 END) AS blocked_count,
    SUM(CASE WHEN operation_status = 'Executing' THEN 1 ELSE 0 END) AS executing_count,
    SUM(CASE WHEN is_manual = 1 THEN 1 ELSE 0 END) AS manual_count
FROM (
         SELECT
             op.status AS operation_status,
             op.is_manual,
             CASE
                 WHEN DATEDIFF(HOUR, op.create_date, GETUTCDATE()) < 1 THEN '0-1h'
                 WHEN DATEDIFF(HOUR, op.create_date, GETUTCDATE()) < 4 THEN '1-4h'
                 WHEN DATEDIFF(HOUR, op.create_date, GETUTCDATE()) < 24 THEN '4-24h'
                 WHEN DATEDIFF(HOUR, op.create_date, GETUTCDATE()) < 72 THEN '1-3d'
                 WHEN DATEDIFF(HOUR, op.create_date, GETUTCDATE()) < 168 THEN '3-7d'
                 ELSE '7d+'
                 END AS bucket_name
         FROM Reconciliation.Operation op
         WHERE op.status IN ('Planned', 'Blocked', 'Executing')
     ) t
GROUP BY bucket_name;
GO

-- -------------------------------------------------------------------------------------
-- B4. Manuel review kuyruğu (enriched)
-- -------------------------------------------------------------------------------------
CREATE OR ALTER VIEW Reporting.vw_recon_manual_review_queue
AS
SELECT
    rv.id AS review_id,
    rv.file_line_id,
    rv.group_id,
    rv.evaluation_id,
    rv.operation_id,
    rv.reviewer_id,
    rv.decision,
    rv.comment,
    rv.decision_at,
    rv.expires_at,
    rv.expiration_action,
    rv.expiration_flow_action,
    rv.create_date AS review_created_at,
    op.code AS operation_code,
    op.branch AS operation_branch,
    op.status AS operation_status,
    op.is_manual AS operation_is_manual,
    op.note AS operation_note,
    op.retry_count AS operation_retry_count,
    op.max_retry_count AS operation_max_retries,
    op.next_attempt_at AS operation_next_attempt_at,
    op.lease_owner AS operation_lease_owner,
    op.lease_expires_at AS operation_lease_expires_at,
    op.last_error AS operation_last_error,
    op.payload AS operation_payload,
    op.create_date AS operation_created_at,
    op.update_date AS operation_updated_at,
    ev.status AS evaluation_status,
    ev.message AS evaluation_message,
    ev.operation_count AS evaluation_operation_count,
    ev.create_date AS evaluation_created_at,
    le.last_execution_id,
    le.last_attempt_number,
    le.last_execution_status,
    le.last_execution_started_at,
    le.last_execution_finished_at,
    le.last_execution_result_code,
    le.last_execution_result_message,
    le.last_execution_error_code,
    le.last_execution_error_message,
    le.total_execution_count,
    f.file_name,
    f.file_key,
    f.source_type AS file_source_type,
    f.file_type,
    f.content_type,
    f.status AS file_status,
    fl.line_number,
    fl.line_type AS line_record_type,
    fl.status AS line_status,
    fl.reconciliation_status AS line_reconciliation_status,
    fl.matched_clearing_line_id,
    fl.correlation_key,
    fl.correlation_value,
    fl.duplicate_status AS line_duplicate_status,
    fl.message AS line_message,
    cd.transaction_date AS card_transaction_date,
    cd.transaction_time AS card_transaction_time,
    cd.original_amount AS card_original_amount,
    cd.original_currency AS card_original_currency,
    cd.settlement_amount AS card_settlement_amount,
    cd.billing_amount AS card_billing_amount,
    cd.financial_type AS card_financial_type,
    cd.txn_effect AS card_txn_effect,
    cd.response_code AS card_response_code,
    cd.is_successful_txn AS card_is_successful_txn,
    cd.rrn AS card_rrn,
    cd.arn AS card_arn,
    cld.txn_date AS clearing_txn_date,
    cld.txn_time AS clearing_txn_time,
    cld.io_date AS clearing_io_date,
    cld.source_amount AS clearing_source_amount,
    cld.source_currency AS clearing_source_currency,
    cld.destination_amount AS clearing_destination_amount,
    cld.txn_type AS clearing_txn_type,
    cld.io_flag AS clearing_io_flag,
    cld.control_stat AS clearing_control_stat,
    cld.rrn AS clearing_rrn,
    cld.arn AS clearing_arn,
    ROUND(CAST(DATEDIFF(MINUTE, rv.create_date, GETUTCDATE()) AS DECIMAL(18,2)) / 60, 1) AS waiting_hours,
    CASE
        WHEN rv.expires_at IS NOT NULL AND rv.expires_at < GETUTCDATE() THEN 'EXPIRED'
        WHEN rv.expires_at IS NOT NULL AND rv.expires_at < DATEADD(HOUR, 4, GETUTCDATE()) THEN 'EXPIRING_SOON'
        WHEN DATEDIFF(HOUR, rv.create_date, GETUTCDATE()) > 24 THEN 'OVERDUE'
        ELSE 'NORMAL'
        END AS urgency_level,
    CASE
        WHEN le.last_execution_error_message IS NOT NULL THEN le.last_execution_error_message
        WHEN op.last_error IS NOT NULL THEN op.last_error
        ELSE NULL
    END AS effective_error
FROM Reconciliation.Review rv
    JOIN Reconciliation.Operation op ON op.id = rv.operation_id
    JOIN Reconciliation.Evaluation ev ON ev.id = rv.evaluation_id
    JOIN Ingestion.FileLine fl ON fl.id = rv.file_line_id
    JOIN Ingestion.[File] f ON f.id = fl.file_id
    OUTER APPLY (
        SELECT TOP 1
            ex.id AS last_execution_id,
            ex.attempt_number AS last_attempt_number,
            ex.status AS last_execution_status,
            ex.started_at AS last_execution_started_at,
            ex.finished_at AS last_execution_finished_at,
            ex.result_code AS last_execution_result_code,
            ex.result_message AS last_execution_result_message,
            ex.error_code AS last_execution_error_code,
            ex.error_message AS last_execution_error_message,
            (SELECT COUNT(*) FROM Reconciliation.OperationExecution
             WHERE operation_id = rv.operation_id AND evaluation_id = rv.evaluation_id) AS total_execution_count
        FROM Reconciliation.OperationExecution ex
        WHERE ex.operation_id = rv.operation_id AND ex.evaluation_id = rv.evaluation_id
        ORDER BY ex.attempt_number DESC
    ) le
    OUTER APPLY (
        SELECT TOP 1 cv.transaction_date, cv.transaction_time,
               cv.original_amount, cv.original_currency, cv.settlement_amount, cv.billing_amount,
               cv.financial_type, cv.txn_effect, cv.response_code, cv.is_successful_txn, cv.rrn, cv.arn
        FROM (
            SELECT cv2.transaction_date, cv2.transaction_time, cv2.original_amount, cv2.original_currency, cv2.settlement_amount, cv2.billing_amount, cv2.financial_type, cv2.txn_effect, cv2.response_code, cv2.is_successful_txn, cv2.rrn, cv2.arn
            FROM Ingestion.CardVisaDetail cv2 WHERE cv2.file_line_id = fl.id AND f.content_type = 'Visa' AND f.file_type = 'Card'
            UNION ALL
            SELECT cm.transaction_date, cm.transaction_time, cm.original_amount, cm.original_currency, cm.settlement_amount, cm.billing_amount, cm.financial_type, cm.txn_effect, cm.response_code, cm.is_successful_txn, cm.rrn, cm.arn
            FROM Ingestion.CardMscDetail cm WHERE cm.file_line_id = fl.id AND f.content_type = 'Msc' AND f.file_type = 'Card'
            UNION ALL
            SELECT cb.transaction_date, cb.transaction_time, cb.original_amount, cb.original_currency, cb.settlement_amount, cb.billing_amount, cb.financial_type, cb.txn_effect, cb.response_code, cb.is_successful_txn, cb.rrn, cb.arn
            FROM Ingestion.CardBkmDetail cb WHERE cb.file_line_id = fl.id AND f.content_type = 'Bkm' AND f.file_type = 'Card'
        ) cv
    ) cd
    OUTER APPLY (
        SELECT TOP 1 cv.txn_date, cv.txn_time, cv.io_date,
               cv.source_amount, cv.source_currency, cv.destination_amount,
               cv.txn_type, cv.io_flag, cv.control_stat, cv.rrn, cv.arn
        FROM (
            SELECT cv2.txn_date, cv2.txn_time, cv2.io_date, cv2.source_amount, cv2.source_currency, cv2.destination_amount, cv2.txn_type, cv2.io_flag, cv2.control_stat, cv2.rrn, cv2.arn
            FROM Ingestion.ClearingVisaDetail cv2 WHERE cv2.file_line_id = fl.id AND f.content_type = 'Visa' AND f.file_type = 'Clearing'
            UNION ALL
            SELECT cm.txn_date, cm.txn_time, cm.io_date, cm.source_amount, cm.source_currency, cm.destination_amount, cm.txn_type, cm.io_flag, cm.control_stat, cm.rrn, cm.arn
            FROM Ingestion.ClearingMscDetail cm WHERE cm.file_line_id = fl.id AND f.content_type = 'Msc' AND f.file_type = 'Clearing'
            UNION ALL
            SELECT cb.txn_date, cb.txn_time, cb.io_date, cb.source_amount, cb.source_currency, cb.destination_amount, cb.txn_type, cb.io_flag, cb.control_stat, cb.rrn, cb.arn
            FROM Ingestion.ClearingBkmDetail cb WHERE cb.file_line_id = fl.id AND f.content_type = 'Bkm' AND f.file_type = 'Clearing'
        ) cv
    ) cld
WHERE rv.decision = 'Pending';
GO

-- -------------------------------------------------------------------------------------
-- B5. Alert hotspot özeti (LIVE + ARCHIVE)
-- -------------------------------------------------------------------------------------
CREATE OR ALTER VIEW Reporting.vw_recon_alert_summary
AS
SELECT
    src.data_scope,
    src.severity,
    src.alert_type,
    src.alert_status,
    SUM(src.alert_count) AS alert_count,
    SUM(src.distinct_group_count) AS distinct_group_count,
    SUM(src.distinct_operation_count) AS distinct_operation_count,
    MIN(src.first_alert_at) AS first_alert_at,
    MAX(src.last_alert_at) AS last_alert_at
FROM (
    -- LIVE
    SELECT
        'LIVE' AS data_scope,
        alert.severity,
        alert.alert_type,
        alert.alert_status,
        COUNT(*) AS alert_count,
        COUNT(DISTINCT alert.group_id) AS distinct_group_count,
        COUNT(DISTINCT alert.operation_id) AS distinct_operation_count,
        MIN(alert.create_date) AS first_alert_at,
        MAX(alert.create_date) AS last_alert_at
    FROM Reconciliation.Alert alert
    GROUP BY alert.severity, alert.alert_type, alert.alert_status

    UNION ALL

    -- ARCHIVE
    SELECT
        'ARCHIVE' AS data_scope,
        alert.severity,
        alert.alert_type,
        alert.alert_status,
        COUNT(*) AS alert_count,
        COUNT(DISTINCT alert.group_id) AS distinct_group_count,
        COUNT(DISTINCT alert.operation_id) AS distinct_operation_count,
        MIN(alert.create_date) AS first_alert_at,
        MAX(alert.create_date) AS last_alert_at
    FROM Archive.ReconciliationAlert alert
    GROUP BY alert.severity, alert.alert_type, alert.alert_status
) src
GROUP BY src.data_scope, src.severity, src.alert_type, src.alert_status;
GO

-- =====================================================================================
-- C. RECONCILIATION CONTENT + FINANCIAL
-- =====================================================================================

-- -------------------------------------------------------------------------------------
-- C1. LIVE card içerik özeti
-- -------------------------------------------------------------------------------------
CREATE OR ALTER VIEW Reporting.vw_recon_live_card_content_daily
AS
SELECT
    CAST(f.create_date AS DATE) AS report_date,
    'LIVE' AS data_scope,
    f.content_type AS network,
    fl.status AS line_status,
    fl.reconciliation_status,
    d.financial_type,
    d.txn_effect,
    d.txn_source,
    d.txn_region,
    d.terminal_type,
    d.channel_code,
    d.is_txn_settle,
    d.txn_stat,
    d.response_code,
    d.is_successful_txn,
    d.original_currency,
    COUNT(*) AS transaction_count,
    COUNT(DISTINCT fl.file_id) AS distinct_file_count,
    SUM(d.original_amount) AS total_card_original_amount,
    SUM(d.settlement_amount) AS total_card_settlement_amount,
    SUM(d.billing_amount) AS total_card_billing_amount,
    AVG(d.original_amount) AS avg_card_original_amount,
    SUM(CASE WHEN fl.matched_clearing_line_id IS NOT NULL THEN 1 ELSE 0 END) AS matched_count,
    SUM(CASE WHEN fl.matched_clearing_line_id IS NULL THEN 1 ELSE 0 END) AS unmatched_count
FROM Ingestion.FileLine fl
         JOIN Ingestion.[File] f
ON f.id = fl.file_id
    AND f.file_type = 'Card'
    CROSS APPLY (
    SELECT
    cv.financial_type, cv.txn_effect, cv.txn_source, cv.txn_region,
    cv.terminal_type, cv.channel_code, cv.is_txn_settle, cv.txn_stat,
    cv.response_code, cv.is_successful_txn,
    cv.original_currency, cv.original_amount, cv.settlement_amount, cv.billing_amount
    FROM Ingestion.CardVisaDetail cv
    WHERE cv.file_line_id = fl.id AND f.content_type = 'Visa'
    UNION ALL
    SELECT
    cm.financial_type, cm.txn_effect, cm.txn_source, cm.txn_region,
    cm.terminal_type, cm.channel_code, cm.is_txn_settle, cm.txn_stat,
    cm.response_code, cm.is_successful_txn,
    cm.original_currency, cm.original_amount, cm.settlement_amount, cm.billing_amount
    FROM Ingestion.CardMscDetail cm
    WHERE cm.file_line_id = fl.id AND f.content_type = 'Msc'
    UNION ALL
    SELECT
    cb.financial_type, cb.txn_effect, cb.txn_source, cb.txn_region,
    cb.terminal_type, cb.channel_code, cb.is_txn_settle, cb.txn_stat,
    cb.response_code, cb.is_successful_txn,
    cb.original_currency, cb.original_amount, cb.settlement_amount, cb.billing_amount
    FROM Ingestion.CardBkmDetail cb
    WHERE cb.file_line_id = fl.id AND f.content_type = 'Bkm'
    ) d
GROUP BY
    CAST(f.create_date AS DATE),
    f.content_type,
    fl.status,
    fl.reconciliation_status,
    d.financial_type,
    d.txn_effect,
    d.txn_source,
    d.txn_region,
    d.terminal_type,
    d.channel_code,
    d.is_txn_settle,
    d.txn_stat,
    d.response_code,
    d.is_successful_txn,
    d.original_currency;
GO

-- -------------------------------------------------------------------------------------
-- C2. LIVE clearing içerik özeti
-- -------------------------------------------------------------------------------------
CREATE OR ALTER VIEW Reporting.vw_recon_live_clearing_content_daily
AS
SELECT
    CAST(f.create_date AS DATE) AS report_date,
    'LIVE' AS data_scope,
    f.content_type AS network,
    fl.status AS line_status,
    fl.reconciliation_status,
    d.txn_type,
    d.io_flag,
    d.control_stat,
    d.source_currency,
    COUNT(*) AS transaction_count,
    COUNT(DISTINCT fl.file_id) AS distinct_file_count,
    SUM(d.source_amount) AS total_clearing_source_amount,
    SUM(d.destination_amount) AS total_clearing_destination_amount,
    AVG(d.source_amount) AS avg_clearing_source_amount,
    SUM(CASE WHEN fl.matched_clearing_line_id IS NOT NULL THEN 1 ELSE 0 END) AS matched_count,
    SUM(CASE WHEN fl.matched_clearing_line_id IS NULL THEN 1 ELSE 0 END) AS unmatched_count
FROM Ingestion.FileLine fl
         JOIN Ingestion.[File] f
ON f.id = fl.file_id
    AND f.file_type = 'Clearing'
    CROSS APPLY (
    SELECT
    cv.txn_type, cv.io_flag, cv.control_stat,
    cv.source_currency, cv.source_amount, cv.destination_amount
    FROM Ingestion.ClearingVisaDetail cv
    WHERE cv.file_line_id = fl.id AND f.content_type = 'Visa'
    UNION ALL
    SELECT
    cm.txn_type, cm.io_flag, cm.control_stat,
    cm.source_currency, cm.source_amount, cm.destination_amount
    FROM Ingestion.ClearingMscDetail cm
    WHERE cm.file_line_id = fl.id AND f.content_type = 'Msc'
    UNION ALL
    SELECT
    cb.txn_type, cb.io_flag, cb.control_stat,
    cb.source_currency, cb.source_amount, cb.destination_amount
    FROM Ingestion.ClearingBkmDetail cb
    WHERE cb.file_line_id = fl.id AND f.content_type = 'Bkm'
    ) d
GROUP BY
    CAST(f.create_date AS DATE),
    f.content_type,
    fl.status,
    fl.reconciliation_status,
    d.txn_type,
    d.io_flag,
    d.control_stat,
    d.source_currency;
GO

-- -------------------------------------------------------------------------------------
-- C3. ARCHIVE card içerik özeti
-- -------------------------------------------------------------------------------------
CREATE OR ALTER VIEW Reporting.vw_recon_archive_card_content_daily
AS
SELECT
    CAST(f.create_date AS DATE) AS report_date,
    'ARCHIVE' AS data_scope,
    f.content_type AS network,
    fl.status AS line_status,
    fl.reconciliation_status,
    d.financial_type,
    d.txn_effect,
    d.txn_source,
    d.txn_region,
    d.terminal_type,
    d.channel_code,
    d.is_txn_settle,
    d.txn_stat,
    d.response_code,
    d.is_successful_txn,
    d.original_currency,
    COUNT(*) AS transaction_count,
    COUNT(DISTINCT fl.file_id) AS distinct_file_count,
    SUM(d.original_amount) AS total_card_original_amount,
    SUM(d.settlement_amount) AS total_card_settlement_amount,
    SUM(d.billing_amount) AS total_card_billing_amount,
    AVG(d.original_amount) AS avg_card_original_amount,
    SUM(CASE WHEN fl.matched_clearing_line_id IS NOT NULL THEN 1 ELSE 0 END) AS matched_count,
    SUM(CASE WHEN fl.matched_clearing_line_id IS NULL THEN 1 ELSE 0 END) AS unmatched_count
FROM Archive.IngestionFileLine fl
         JOIN Archive.IngestionFile f
              ON f.id = fl.file_id
                  AND f.file_type = 'Card'
    CROSS APPLY (
    SELECT
        cv.financial_type, cv.txn_effect, cv.txn_source, cv.txn_region,
        cv.terminal_type, cv.channel_code, cv.is_txn_settle, cv.txn_stat,
        cv.response_code, cv.is_successful_txn,
        cv.original_currency, cv.original_amount, cv.settlement_amount, cv.billing_amount
    FROM Archive.IngestionCardVisaDetail cv
    WHERE cv.file_line_id = fl.id AND f.content_type = 'Visa'
    UNION ALL
    SELECT
        cm.financial_type, cm.txn_effect, cm.txn_source, cm.txn_region,
        cm.terminal_type, cm.channel_code, cm.is_txn_settle, cm.txn_stat,
        cm.response_code, cm.is_successful_txn,
        cm.original_currency, cm.original_amount, cm.settlement_amount, cm.billing_amount
    FROM Archive.IngestionCardMscDetail cm
    WHERE cm.file_line_id = fl.id AND f.content_type = 'Msc'
    UNION ALL
    SELECT
        cb.financial_type, cb.txn_effect, cb.txn_source, cb.txn_region,
        cb.terminal_type, cb.channel_code, cb.is_txn_settle, cb.txn_stat,
        cb.response_code, cb.is_successful_txn,
        cb.original_currency, cb.original_amount, cb.settlement_amount, cb.billing_amount
    FROM Archive.IngestionCardBkmDetail cb
    WHERE cb.file_line_id = fl.id AND f.content_type = 'Bkm'
) d
GROUP BY
    CAST(f.create_date AS DATE),
    f.content_type,
    fl.status,
    fl.reconciliation_status,
    d.financial_type,
    d.txn_effect,
    d.txn_source,
    d.txn_region,
    d.terminal_type,
    d.channel_code,
    d.is_txn_settle,
    d.txn_stat,
    d.response_code,
    d.is_successful_txn,
    d.original_currency;
GO

-- -------------------------------------------------------------------------------------
-- C4. ARCHIVE clearing içerik özeti
-- -------------------------------------------------------------------------------------
CREATE OR ALTER VIEW Reporting.vw_recon_archive_clearing_content_daily
AS
SELECT
    CAST(f.create_date AS DATE) AS report_date,
    'ARCHIVE' AS data_scope,
    f.content_type AS network,
    fl.status AS line_status,
    fl.reconciliation_status,
    d.txn_type,
    d.io_flag,
    d.control_stat,
    d.source_currency,
    COUNT(*) AS transaction_count,
    COUNT(DISTINCT fl.file_id) AS distinct_file_count,
    SUM(d.source_amount) AS total_clearing_source_amount,
    SUM(d.destination_amount) AS total_clearing_destination_amount,
    AVG(d.source_amount) AS avg_clearing_source_amount,
    SUM(CASE WHEN fl.matched_clearing_line_id IS NOT NULL THEN 1 ELSE 0 END) AS matched_count,
    SUM(CASE WHEN fl.matched_clearing_line_id IS NULL THEN 1 ELSE 0 END) AS unmatched_count
FROM Archive.IngestionFileLine fl
         JOIN Archive.IngestionFile f
              ON f.id = fl.file_id
                  AND f.file_type = 'Clearing'
    CROSS APPLY (
    SELECT
        cv.txn_type, cv.io_flag, cv.control_stat,
        cv.source_currency, cv.source_amount, cv.destination_amount
    FROM Archive.IngestionClearingVisaDetail cv
    WHERE cv.file_line_id = fl.id AND f.content_type = 'Visa'
    UNION ALL
    SELECT
        cm.txn_type, cm.io_flag, cm.control_stat,
        cm.source_currency, cm.source_amount, cm.destination_amount
    FROM Archive.IngestionClearingMscDetail cm
    WHERE cm.file_line_id = fl.id AND f.content_type = 'Msc'
    UNION ALL
    SELECT
        cb.txn_type, cb.io_flag, cb.control_stat,
        cb.source_currency, cb.source_amount, cb.destination_amount
    FROM Archive.IngestionClearingBkmDetail cb
    WHERE cb.file_line_id = fl.id AND f.content_type = 'Bkm'
) d
GROUP BY
    CAST(f.create_date AS DATE),
    f.content_type,
    fl.status,
    fl.reconciliation_status,
    d.txn_type,
    d.io_flag,
    d.control_stat,
    d.source_currency;
GO

-- -------------------------------------------------------------------------------------
-- C5. Unified günlük reconciliation içerik özeti
-- -------------------------------------------------------------------------------------
CREATE OR ALTER VIEW Reporting.vw_recon_content_daily
AS
SELECT
    src.report_date,
    src.data_scope,
    src.network,
    src.side,
    src.line_status,
    src.reconciliation_status,
    SUM(src.transaction_count) AS transaction_count,
    SUM(src.distinct_file_count) AS distinct_file_count,
    SUM(src.matched_count) AS matched_count,
    SUM(src.unmatched_count) AS unmatched_count,
    SUM(src.total_card_original_amount) AS total_card_original_amount,
    SUM(src.total_card_settlement_amount) AS total_card_settlement_amount,
    SUM(src.total_card_billing_amount) AS total_card_billing_amount,
    SUM(src.total_clearing_source_amount) AS total_clearing_source_amount,
    SUM(src.total_clearing_destination_amount) AS total_clearing_destination_amount
FROM (
         -- live card
         SELECT
             CAST(f.create_date AS DATE) AS report_date,
             'LIVE' AS data_scope,
             f.content_type AS network,
             'Card' AS side,
             fl.status AS line_status,
             fl.reconciliation_status,
             COUNT(*) AS transaction_count,
             COUNT(DISTINCT fl.file_id) AS distinct_file_count,
             SUM(CASE WHEN fl.matched_clearing_line_id IS NOT NULL THEN 1 ELSE 0 END) AS matched_count,
             SUM(CASE WHEN fl.matched_clearing_line_id IS NULL THEN 1 ELSE 0 END) AS unmatched_count,
             SUM(d.original_amount) AS total_card_original_amount,
             SUM(d.settlement_amount) AS total_card_settlement_amount,
             SUM(d.billing_amount) AS total_card_billing_amount,
             CAST(0 AS DECIMAL(18,4)) AS total_clearing_source_amount,
             CAST(0 AS DECIMAL(18,4)) AS total_clearing_destination_amount
         FROM Ingestion.FileLine fl
                  JOIN Ingestion.[File] f ON f.id = fl.file_id AND f.file_type = 'Card'
             CROSS APPLY (
             SELECT cv.original_amount, cv.settlement_amount, cv.billing_amount
             FROM Ingestion.CardVisaDetail cv WHERE cv.file_line_id = fl.id AND f.content_type = 'Visa'
             UNION ALL
             SELECT cm.original_amount, cm.settlement_amount, cm.billing_amount
             FROM Ingestion.CardMscDetail cm WHERE cm.file_line_id = fl.id AND f.content_type = 'Msc'
             UNION ALL
             SELECT cb.original_amount, cb.settlement_amount, cb.billing_amount
             FROM Ingestion.CardBkmDetail cb WHERE cb.file_line_id = fl.id AND f.content_type = 'Bkm'
             ) d
         GROUP BY CAST(f.create_date AS DATE), f.content_type, fl.status, fl.reconciliation_status

         UNION ALL

         -- live clearing
         SELECT
             CAST(f.create_date AS DATE),
             'LIVE',
             f.content_type,
             'Clearing',
             fl.status,
             fl.reconciliation_status,
             COUNT(*),
             COUNT(DISTINCT fl.file_id),
             SUM(CASE WHEN fl.matched_clearing_line_id IS NOT NULL THEN 1 ELSE 0 END),
             SUM(CASE WHEN fl.matched_clearing_line_id IS NULL THEN 1 ELSE 0 END),
             CAST(0 AS DECIMAL(18,4)),
             CAST(0 AS DECIMAL(18,4)),
             CAST(0 AS DECIMAL(18,4)),
             SUM(d.source_amount),
             SUM(d.destination_amount)
         FROM Ingestion.FileLine fl
             JOIN Ingestion.[File] f ON f.id = fl.file_id AND f.file_type = 'Clearing'
             CROSS APPLY (
             SELECT cv.source_amount, cv.destination_amount
             FROM Ingestion.ClearingVisaDetail cv WHERE cv.file_line_id = fl.id AND f.content_type = 'Visa'
             UNION ALL
             SELECT cm.source_amount, cm.destination_amount
             FROM Ingestion.ClearingMscDetail cm WHERE cm.file_line_id = fl.id AND f.content_type = 'Msc'
             UNION ALL
             SELECT cb.source_amount, cb.destination_amount
             FROM Ingestion.ClearingBkmDetail cb WHERE cb.file_line_id = fl.id AND f.content_type = 'Bkm'
             ) d
         GROUP BY CAST(f.create_date AS DATE), f.content_type, fl.status, fl.reconciliation_status

         UNION ALL

         -- archive card
         SELECT
             CAST(f.create_date AS DATE),
             'ARCHIVE',
             f.content_type,
             'Card',
             fl.status,
             fl.reconciliation_status,
             COUNT(*),
             COUNT(DISTINCT fl.file_id),
             SUM(CASE WHEN fl.matched_clearing_line_id IS NOT NULL THEN 1 ELSE 0 END),
             SUM(CASE WHEN fl.matched_clearing_line_id IS NULL THEN 1 ELSE 0 END),
             SUM(d.original_amount),
             SUM(d.settlement_amount),
             SUM(d.billing_amount),
             CAST(0 AS DECIMAL(18,4)),
             CAST(0 AS DECIMAL(18,4))
         FROM Archive.IngestionFileLine fl
             JOIN Archive.IngestionFile f ON f.id = fl.file_id AND f.file_type = 'Card'
             CROSS APPLY (
             SELECT cv.original_amount, cv.settlement_amount, cv.billing_amount
             FROM Archive.IngestionCardVisaDetail cv WHERE cv.file_line_id = fl.id AND f.content_type = 'Visa'
             UNION ALL
             SELECT cm.original_amount, cm.settlement_amount, cm.billing_amount
             FROM Archive.IngestionCardMscDetail cm WHERE cm.file_line_id = fl.id AND f.content_type = 'Msc'
             UNION ALL
             SELECT cb.original_amount, cb.settlement_amount, cb.billing_amount
             FROM Archive.IngestionCardBkmDetail cb WHERE cb.file_line_id = fl.id AND f.content_type = 'Bkm'
             ) d
         GROUP BY CAST(f.create_date AS DATE), f.content_type, fl.status, fl.reconciliation_status

         UNION ALL

         -- archive clearing
         SELECT
             CAST(f.create_date AS DATE),
             'ARCHIVE',
             f.content_type,
             'Clearing',
             fl.status,
             fl.reconciliation_status,
             COUNT(*),
             COUNT(DISTINCT fl.file_id),
             SUM(CASE WHEN fl.matched_clearing_line_id IS NOT NULL THEN 1 ELSE 0 END),
             SUM(CASE WHEN fl.matched_clearing_line_id IS NULL THEN 1 ELSE 0 END),
             CAST(0 AS DECIMAL(18,4)),
             CAST(0 AS DECIMAL(18,4)),
             CAST(0 AS DECIMAL(18,4)),
             SUM(d.source_amount),
             SUM(d.destination_amount)
         FROM Archive.IngestionFileLine fl
             JOIN Archive.IngestionFile f ON f.id = fl.file_id AND f.file_type = 'Clearing'
             CROSS APPLY (
             SELECT cv.source_amount, cv.destination_amount
             FROM Archive.IngestionClearingVisaDetail cv WHERE cv.file_line_id = fl.id AND f.content_type = 'Visa'
             UNION ALL
             SELECT cm.source_amount, cm.destination_amount
             FROM Archive.IngestionClearingMscDetail cm WHERE cm.file_line_id = fl.id AND f.content_type = 'Msc'
             UNION ALL
             SELECT cb.source_amount, cb.destination_amount
             FROM Archive.IngestionClearingBkmDetail cb WHERE cb.file_line_id = fl.id AND f.content_type = 'Bkm'
             ) d
         GROUP BY CAST(f.create_date AS DATE), f.content_type, fl.status, fl.reconciliation_status
     ) src
GROUP BY
    src.report_date,
    src.data_scope,
    src.network,
    src.side,
    src.line_status,
    src.reconciliation_status;
GO

-- -------------------------------------------------------------------------------------
-- C6. Clearing control_stat analizi
-- -------------------------------------------------------------------------------------
CREATE OR ALTER VIEW Reporting.vw_recon_clearing_controlstat_analysis
AS
SELECT
    x.data_scope,
    x.network,
    x.line_status,
    x.control_stat,
    x.io_flag,
    SUM(x.transaction_count) AS transaction_count,
    SUM(x.total_clearing_source_amount) AS total_clearing_source_amount,
    SUM(x.matched_count) AS matched_count,
    SUM(x.unmatched_count) AS unmatched_count,
    CASE
        WHEN SUM(x.transaction_count) > 0
            THEN ROUND(CAST(SUM(x.unmatched_count) AS DECIMAL(18,4)) / SUM(x.transaction_count) * 100, 2)
        ELSE 0
        END AS unmatched_rate_pct
FROM (
         -- live
         SELECT
             'LIVE' AS data_scope,
             f.content_type AS network,
             fl.status AS line_status,
             d.control_stat,
             d.io_flag,
             COUNT(*) AS transaction_count,
             SUM(d.source_amount) AS total_clearing_source_amount,
             SUM(CASE WHEN fl.matched_clearing_line_id IS NOT NULL THEN 1 ELSE 0 END) AS matched_count,
             SUM(CASE WHEN fl.matched_clearing_line_id IS NULL THEN 1 ELSE 0 END) AS unmatched_count
         FROM Ingestion.FileLine fl
                  JOIN Ingestion.[File] f ON f.id = fl.file_id AND f.file_type = 'Clearing'
             CROSS APPLY (
             SELECT cv.control_stat, cv.io_flag, cv.source_amount
             FROM Ingestion.ClearingVisaDetail cv WHERE cv.file_line_id = fl.id AND f.content_type = 'Visa'
             UNION ALL
             SELECT cm.control_stat, cm.io_flag, cm.source_amount
             FROM Ingestion.ClearingMscDetail cm WHERE cm.file_line_id = fl.id AND f.content_type = 'Msc'
             UNION ALL
             SELECT cb.control_stat, cb.io_flag, cb.source_amount
             FROM Ingestion.ClearingBkmDetail cb WHERE cb.file_line_id = fl.id AND f.content_type = 'Bkm'
             ) d
         GROUP BY f.content_type, fl.status, d.control_stat, d.io_flag

         UNION ALL

         -- archive
         SELECT
             'ARCHIVE',
             f.content_type,
             fl.status,
             d.control_stat,
             d.io_flag,
             COUNT(*),
             SUM(d.source_amount),
             SUM(CASE WHEN fl.matched_clearing_line_id IS NOT NULL THEN 1 ELSE 0 END),
             SUM(CASE WHEN fl.matched_clearing_line_id IS NULL THEN 1 ELSE 0 END)
         FROM Archive.IngestionFileLine fl
             JOIN Archive.IngestionFile f ON f.id = fl.file_id AND f.file_type = 'Clearing'
             CROSS APPLY (
             SELECT cv.control_stat, cv.io_flag, cv.source_amount
             FROM Archive.IngestionClearingVisaDetail cv WHERE cv.file_line_id = fl.id AND f.content_type = 'Visa'
             UNION ALL
             SELECT cm.control_stat, cm.io_flag, cm.source_amount
             FROM Archive.IngestionClearingMscDetail cm WHERE cm.file_line_id = fl.id AND f.content_type = 'Msc'
             UNION ALL
             SELECT cb.control_stat, cb.io_flag, cb.source_amount
             FROM Archive.IngestionClearingBkmDetail cb WHERE cb.file_line_id = fl.id AND f.content_type = 'Bkm'
             ) d
         GROUP BY f.content_type, fl.status, d.control_stat, d.io_flag
     ) x
GROUP BY
    x.data_scope,
    x.network,
    x.line_status,
    x.control_stat,
    x.io_flag;
GO

-- -------------------------------------------------------------------------------------
-- C7. Finansal özet
-- -------------------------------------------------------------------------------------
CREATE OR ALTER VIEW Reporting.vw_recon_financial_summary
AS
SELECT
    x.data_scope,
    x.network,
    x.line_status,
    x.financial_type,
    x.txn_effect,
    x.original_currency,
    SUM(x.transaction_count) AS transaction_count,
    SUM(x.total_card_original_amount) AS total_card_original_amount,
    SUM(x.total_card_settlement_amount) AS total_card_settlement_amount,
    SUM(x.total_card_billing_amount) AS total_card_billing_amount,
    SUM(x.settled_count) AS settled_count,
    SUM(x.unsettled_count) AS unsettled_count,
    SUM(x.debit_amount) AS debit_amount,
    SUM(x.credit_amount) AS credit_amount,
    SUM(x.matched_count) AS matched_count,
    SUM(x.unmatched_count) AS unmatched_count
FROM (
         -- live
         SELECT
             'LIVE' AS data_scope,
             f.content_type AS network,
             fl.status AS line_status,
             d.financial_type,
             d.txn_effect,
             d.original_currency,
             COUNT(*) AS transaction_count,
             SUM(d.original_amount) AS total_card_original_amount,
             SUM(d.settlement_amount) AS total_card_settlement_amount,
             SUM(d.billing_amount) AS total_card_billing_amount,
             SUM(CASE WHEN d.is_txn_settle = 'Y' THEN 1 ELSE 0 END) AS settled_count,
             SUM(CASE WHEN d.is_txn_settle = 'N' THEN 1 ELSE 0 END) AS unsettled_count,
             SUM(CASE WHEN d.txn_effect = 'D' THEN d.original_amount ELSE 0 END) AS debit_amount,
             SUM(CASE WHEN d.txn_effect = 'C' THEN d.original_amount ELSE 0 END) AS credit_amount,
             SUM(CASE WHEN fl.matched_clearing_line_id IS NOT NULL THEN 1 ELSE 0 END) AS matched_count,
             SUM(CASE WHEN fl.matched_clearing_line_id IS NULL THEN 1 ELSE 0 END) AS unmatched_count
         FROM Ingestion.FileLine fl
                  JOIN Ingestion.[File] f ON f.id = fl.file_id AND f.file_type = 'Card'
             CROSS APPLY (
             SELECT cv.financial_type, cv.txn_effect, cv.is_txn_settle, cv.original_currency, cv.original_amount, cv.settlement_amount, cv.billing_amount
             FROM Ingestion.CardVisaDetail cv WHERE cv.file_line_id = fl.id AND f.content_type = 'Visa'
             UNION ALL
             SELECT cm.financial_type, cm.txn_effect, cm.is_txn_settle, cm.original_currency, cm.original_amount, cm.settlement_amount, cm.billing_amount
             FROM Ingestion.CardMscDetail cm WHERE cm.file_line_id = fl.id AND f.content_type = 'Msc'
             UNION ALL
             SELECT cb.financial_type, cb.txn_effect, cb.is_txn_settle, cb.original_currency, cb.original_amount, cb.settlement_amount, cb.billing_amount
             FROM Ingestion.CardBkmDetail cb WHERE cb.file_line_id = fl.id AND f.content_type = 'Bkm'
             ) d
         GROUP BY f.content_type, fl.status, d.financial_type, d.txn_effect, d.original_currency

         UNION ALL

         -- archive
         SELECT
             'ARCHIVE',
             f.content_type,
             fl.status,
             d.financial_type,
             d.txn_effect,
             d.original_currency,
             COUNT(*),
             SUM(d.original_amount),
             SUM(d.settlement_amount),
             SUM(d.billing_amount),
             SUM(CASE WHEN d.is_txn_settle = 'Y' THEN 1 ELSE 0 END),
             SUM(CASE WHEN d.is_txn_settle = 'N' THEN 1 ELSE 0 END),
             SUM(CASE WHEN d.txn_effect = 'D' THEN d.original_amount ELSE 0 END),
             SUM(CASE WHEN d.txn_effect = 'C' THEN d.original_amount ELSE 0 END),
             SUM(CASE WHEN fl.matched_clearing_line_id IS NOT NULL THEN 1 ELSE 0 END),
             SUM(CASE WHEN fl.matched_clearing_line_id IS NULL THEN 1 ELSE 0 END)
         FROM Archive.IngestionFileLine fl
             JOIN Archive.IngestionFile f ON f.id = fl.file_id AND f.file_type = 'Card'
             CROSS APPLY (
              SELECT cv.financial_type, cv.txn_effect, cv.is_txn_settle, cv.original_currency, cv.original_amount, cv.settlement_amount, cv.billing_amount
             FROM Archive.IngestionCardVisaDetail cv WHERE cv.file_line_id = fl.id AND f.content_type = 'Visa'
             UNION ALL
             SELECT cm.financial_type, cm.txn_effect, cm.is_txn_settle, cm.original_currency, cm.original_amount, cm.settlement_amount, cm.billing_amount
             FROM Archive.IngestionCardMscDetail cm WHERE cm.file_line_id = fl.id AND f.content_type = 'Msc'
             UNION ALL
             SELECT cb.financial_type, cb.txn_effect, cb.is_txn_settle, cb.original_currency, cb.original_amount, cb.settlement_amount, cb.billing_amount
             FROM Archive.IngestionCardBkmDetail cb WHERE cb.file_line_id = fl.id AND f.content_type = 'Bkm'
             ) d
         GROUP BY f.content_type, fl.status, d.financial_type, d.txn_effect, d.original_currency
     ) x
GROUP BY
    x.data_scope,
    x.network,
    x.line_status,
    x.financial_type,
    x.txn_effect,
    x.original_currency;
GO

-- -------------------------------------------------------------------------------------
-- C8. Response / txn status analizi
-- -------------------------------------------------------------------------------------
CREATE OR ALTER VIEW Reporting.vw_recon_response_status_analysis
AS
SELECT
    x.data_scope,
    x.network,
    x.line_status,
    x.response_code,
    x.txn_stat,
    x.is_successful_txn,
    x.is_txn_settle,
    x.reconciliation_status,
    SUM(x.transaction_count) AS transaction_count,
    SUM(x.total_card_original_amount) AS total_card_original_amount,
    SUM(x.matched_count) AS matched_count,
    SUM(x.unmatched_count) AS unmatched_count
FROM (
         -- live
         SELECT
             'LIVE' AS data_scope,
             f.content_type AS network,
             fl.status AS line_status,
             d.response_code,
             d.txn_stat,
             d.is_successful_txn,
             d.is_txn_settle,
             fl.reconciliation_status,
             COUNT(*) AS transaction_count,
             SUM(d.original_amount) AS total_card_original_amount,
             SUM(CASE WHEN fl.matched_clearing_line_id IS NOT NULL THEN 1 ELSE 0 END) AS matched_count,
             SUM(CASE WHEN fl.matched_clearing_line_id IS NULL THEN 1 ELSE 0 END) AS unmatched_count
         FROM Ingestion.FileLine fl
                  JOIN Ingestion.[File] f ON f.id = fl.file_id AND f.file_type = 'Card'
             CROSS APPLY (
             SELECT cv.response_code, cv.txn_stat, cv.is_successful_txn, cv.is_txn_settle, cv.original_amount
             FROM Ingestion.CardVisaDetail cv WHERE cv.file_line_id = fl.id AND f.content_type = 'Visa'
             UNION ALL
             SELECT cm.response_code, cm.txn_stat, cm.is_successful_txn, cm.is_txn_settle, cm.original_amount
             FROM Ingestion.CardMscDetail cm WHERE cm.file_line_id = fl.id AND f.content_type = 'Msc'
             UNION ALL
             SELECT cb.response_code, cb.txn_stat, cb.is_successful_txn, cb.is_txn_settle, cb.original_amount
             FROM Ingestion.CardBkmDetail cb WHERE cb.file_line_id = fl.id AND f.content_type = 'Bkm'
             ) d
         GROUP BY f.content_type, fl.status, d.response_code, d.txn_stat, d.is_successful_txn, d.is_txn_settle, fl.reconciliation_status

         UNION ALL

         -- archive
         SELECT
             'ARCHIVE',
             f.content_type,
             fl.status,
             d.response_code,
             d.txn_stat,
             d.is_successful_txn,
             d.is_txn_settle,
             fl.reconciliation_status,
             COUNT(*),
             SUM(d.original_amount),
             SUM(CASE WHEN fl.matched_clearing_line_id IS NOT NULL THEN 1 ELSE 0 END),
             SUM(CASE WHEN fl.matched_clearing_line_id IS NULL THEN 1 ELSE 0 END)
         FROM Archive.IngestionFileLine fl
             JOIN Archive.IngestionFile f ON f.id = fl.file_id AND f.file_type = 'Card'
             CROSS APPLY (
             SELECT cv.response_code, cv.txn_stat, cv.is_successful_txn, cv.is_txn_settle, cv.original_amount
             FROM Archive.IngestionCardVisaDetail cv WHERE cv.file_line_id = fl.id AND f.content_type = 'Visa'
             UNION ALL
             SELECT cm.response_code, cm.txn_stat, cm.is_successful_txn, cm.is_txn_settle, cm.original_amount
             FROM Archive.IngestionCardMscDetail cm WHERE cm.file_line_id = fl.id AND f.content_type = 'Msc'
             UNION ALL
             SELECT cb.response_code, cb.txn_stat, cb.is_successful_txn, cb.is_txn_settle, cb.original_amount
             FROM Archive.IngestionCardBkmDetail cb WHERE cb.file_line_id = fl.id AND f.content_type = 'Bkm'
             ) d
         GROUP BY f.content_type, fl.status, d.response_code, d.txn_stat, d.is_successful_txn, d.is_txn_settle, fl.reconciliation_status
     ) x
GROUP BY
    x.data_scope,
    x.network,
    x.line_status,
    x.response_code,
    x.txn_stat,
    x.is_successful_txn,
    x.is_txn_settle,
    x.reconciliation_status;
GO

-- =====================================================================================
-- D. ARCHIVE
-- =====================================================================================

-- -------------------------------------------------------------------------------------
-- D1. Archive run overview
-- -------------------------------------------------------------------------------------
CREATE OR ALTER VIEW Reporting.vw_archive_run_overview
AS
SELECT
    log.id AS archive_log_id,
    log.ingestion_file_id,
    COALESCE(lf.file_name, af.file_name) AS file_name,
    COALESCE(lf.file_type, af.file_type) AS file_type,
    COALESCE(lf.content_type, af.content_type) AS content_type,
    log.status AS archive_status,
    log.message AS archive_message,
    log.failure_reasons_json,
    log.filter_json,
    log.create_date AS archive_started_at,
    log.update_date AS archive_updated_at,
    DATEDIFF(SECOND, log.create_date, ISNULL(log.update_date, log.create_date)) AS archive_duration_seconds
FROM Archive.ArchiveLog log
         LEFT JOIN Ingestion.[File] lf ON lf.id = log.ingestion_file_id
         LEFT JOIN Archive.IngestionFile af ON af.id = log.ingestion_file_id;
GO

-- -------------------------------------------------------------------------------------
-- D2. Archive uygunluk görünümü
-- -------------------------------------------------------------------------------------
CREATE OR ALTER VIEW Reporting.vw_archive_eligibility
AS
SELECT
    f.id AS file_id,
    f.file_name,
    f.file_type,
    f.content_type,
    f.status AS file_status,
    f.is_archived,
    f.create_date AS file_created_at,
    ROUND(CAST(DATEDIFF(MINUTE, f.create_date, GETUTCDATE()) AS DECIMAL(18,2)) / 1440, 1) AS age_days,
    ISNULL(x.total_recon_line_count, 0) AS total_recon_line_count,
    ISNULL(x.recon_success_line_count, 0) AS recon_success_line_count,
    ISNULL(x.recon_open_line_count, 0) AS recon_open_line_count,
    CASE
        WHEN af.id IS NOT NULL THEN 'ALREADY_ARCHIVED'
        WHEN f.status <> 'Success' THEN 'FILE_NOT_COMPLETE'
        WHEN ISNULL(x.recon_open_line_count, 0) > 0 THEN 'RECON_PENDING'
        ELSE 'ELIGIBLE'
        END AS archive_eligibility_status
FROM Ingestion.[File] f
LEFT JOIN Archive.IngestionFile af ON af.id = f.id
OUTER APPLY (
    SELECT
        SUM(CASE WHEN line.reconciliation_status IS NOT NULL THEN 1 ELSE 0 END) AS total_recon_line_count,
        SUM(CASE WHEN line.reconciliation_status = 'Success' THEN 1 ELSE 0 END) AS recon_success_line_count,
        SUM(CASE WHEN line.reconciliation_status IN ('Ready', 'Processing', 'Failed') THEN 1 ELSE 0 END) AS recon_open_line_count
    FROM Ingestion.FileLine line
    WHERE line.file_id = f.id
) x;
GO

-- -------------------------------------------------------------------------------------
-- D3. Archive backlog trend
-- -------------------------------------------------------------------------------------
CREATE OR ALTER VIEW Reporting.vw_archive_backlog_trend
AS
SELECT
    CAST(log.create_date AS DATE) AS report_date,
    COUNT(*) AS archive_run_count,
    SUM(CASE WHEN log.status = 'Success' THEN 1 ELSE 0 END) AS success_run_count,
    SUM(CASE WHEN log.status = 'Failed' THEN 1 ELSE 0 END) AS failed_run_count,
    SUM(CASE WHEN log.status NOT IN ('Success', 'Failed') THEN 1 ELSE 0 END) AS other_run_count
FROM Archive.ArchiveLog log
GROUP BY CAST(log.create_date AS DATE);
GO

-- -------------------------------------------------------------------------------------
-- D4. Retention snapshot
-- -------------------------------------------------------------------------------------
CREATE OR ALTER VIEW Reporting.vw_archive_retention_snapshot
AS
SELECT
    (SELECT COUNT(*) FROM Ingestion.[File] f WHERE NOT EXISTS (SELECT 1 FROM Archive.IngestionFile af WHERE af.id = f.id)) AS active_file_count,
    (SELECT COUNT(*) FROM Archive.IngestionFile) AS archived_marked_file_count,
    (SELECT COUNT(*) FROM Archive.IngestionFile) AS archive_table_file_count,
    (SELECT COUNT(*) FROM Archive.IngestionFileLine) AS archive_table_file_line_count,
    (SELECT COUNT(*) FROM Archive.ReconciliationEvaluation) AS archive_table_evaluation_count,
    (SELECT COUNT(*) FROM Archive.ReconciliationOperation) AS archive_table_operation_count,
    (SELECT COUNT(*) FROM Archive.ReconciliationReview) AS archive_table_review_count,
    (SELECT COUNT(*) FROM Archive.ReconciliationAlert) AS archive_table_alert_count,
    (SELECT COUNT(*) FROM Archive.ReconciliationOperationExecution) AS archive_table_execution_count,
    (SELECT MIN(f.create_date) FROM Ingestion.[File] f WHERE NOT EXISTS (SELECT 1 FROM Archive.IngestionFile af WHERE af.id = f.id) AND f.status = 'Success') AS oldest_unarchived_file_date;
GO

-- =====================================================================================
-- E. ADVANCED RECONCILIATION REPORTS
-- =====================================================================================

-- -------------------------------------------------------------------------------------
-- E1. Dosya bazlı mutabakat özeti (LIVE + ARCHIVE)
-- -------------------------------------------------------------------------------------
CREATE OR ALTER VIEW Reporting.vw_file_recon_summary
AS
SELECT * FROM (
    SELECT
        f.id AS file_id, f.file_name, f.file_type, f.content_type, f.status AS file_status,
        f.create_date AS file_created_at, 'LIVE' AS data_scope,
        ISNULL(fl.total_line_count, 0) AS total_line_count,
        ISNULL(fl.matched_line_count, 0) AS matched_line_count,
        ISNULL(fl.unmatched_line_count, 0) AS unmatched_line_count,
        CASE WHEN ISNULL(fl.total_line_count, 0) > 0
            THEN ROUND(CAST(fl.matched_line_count AS DECIMAL(18,4)) / fl.total_line_count * 100, 2) ELSE 0 END AS match_rate_pct,
        ISNULL(fl.total_original_amount, 0) AS total_original_amount,
        ISNULL(fl.matched_amount, 0) AS matched_amount,
        ISNULL(fl.unmatched_amount, 0) AS unmatched_amount,
        ISNULL(fl.total_settlement_amount, 0) AS total_settlement_amount,
        ISNULL(fl.recon_ready_count, 0) AS recon_ready_count,
        ISNULL(fl.recon_success_count, 0) AS recon_success_count,
        ISNULL(fl.recon_failed_count, 0) AS recon_failed_count,
        ISNULL(fl.recon_not_applicable_count, 0) AS recon_not_applicable_count
    FROM Ingestion.[File] f
    OUTER APPLY (
        SELECT
            COUNT(*) AS total_line_count,
            SUM(CASE WHEN line.matched_clearing_line_id IS NOT NULL THEN 1 ELSE 0 END) AS matched_line_count,
            SUM(CASE WHEN line.matched_clearing_line_id IS NULL THEN 1 ELSE 0 END) AS unmatched_line_count,
            SUM(ISNULL(d.amt, 0)) AS total_original_amount,
            SUM(CASE WHEN line.matched_clearing_line_id IS NOT NULL THEN ISNULL(d.amt, 0) ELSE 0 END) AS matched_amount,
            SUM(CASE WHEN line.matched_clearing_line_id IS NULL THEN ISNULL(d.amt, 0) ELSE 0 END) AS unmatched_amount,
            SUM(ISNULL(d.sett, 0)) AS total_settlement_amount,
            SUM(CASE WHEN line.reconciliation_status = 'Ready' THEN 1 ELSE 0 END) AS recon_ready_count,
            SUM(CASE WHEN line.reconciliation_status = 'Success' THEN 1 ELSE 0 END) AS recon_success_count,
            SUM(CASE WHEN line.reconciliation_status = 'Failed' THEN 1 ELSE 0 END) AS recon_failed_count,
            SUM(CASE WHEN line.reconciliation_status IS NULL THEN 1 ELSE 0 END) AS recon_not_applicable_count
        FROM Ingestion.FileLine line
        OUTER APPLY (
            SELECT TOP 1 cv.original_amount AS amt, cv.settlement_amount AS sett FROM (
                SELECT cv2.original_amount, cv2.settlement_amount FROM Ingestion.CardVisaDetail cv2 WHERE cv2.file_line_id = line.id AND f.content_type = 'Visa' AND f.file_type = 'Card'
                UNION ALL SELECT cm.original_amount, cm.settlement_amount FROM Ingestion.CardMscDetail cm WHERE cm.file_line_id = line.id AND f.content_type = 'Msc' AND f.file_type = 'Card'
                UNION ALL SELECT cb.original_amount, cb.settlement_amount FROM Ingestion.CardBkmDetail cb WHERE cb.file_line_id = line.id AND f.content_type = 'Bkm' AND f.file_type = 'Card'
                UNION ALL SELECT cv2.source_amount, 0 FROM Ingestion.ClearingVisaDetail cv2 WHERE cv2.file_line_id = line.id AND f.content_type = 'Visa' AND f.file_type = 'Clearing'
                UNION ALL SELECT cm.source_amount, 0 FROM Ingestion.ClearingMscDetail cm WHERE cm.file_line_id = line.id AND f.content_type = 'Msc' AND f.file_type = 'Clearing'
                UNION ALL SELECT cb.source_amount, 0 FROM Ingestion.ClearingBkmDetail cb WHERE cb.file_line_id = line.id AND f.content_type = 'Bkm' AND f.file_type = 'Clearing'
            ) cv
        ) d
        WHERE line.file_id = f.id
    ) fl

    UNION ALL

    SELECT
        f.id, f.file_name, f.file_type, f.content_type, f.status,
        f.create_date, 'ARCHIVE',
        ISNULL(fl.total_line_count, 0), ISNULL(fl.matched_line_count, 0), ISNULL(fl.unmatched_line_count, 0),
        CASE WHEN ISNULL(fl.total_line_count, 0) > 0 THEN ROUND(CAST(fl.matched_line_count AS DECIMAL(18,4)) / fl.total_line_count * 100, 2) ELSE 0 END,
        ISNULL(fl.total_original_amount, 0), ISNULL(fl.matched_amount, 0), ISNULL(fl.unmatched_amount, 0),
        ISNULL(fl.total_settlement_amount, 0),
        ISNULL(fl.recon_ready_count, 0), ISNULL(fl.recon_success_count, 0), ISNULL(fl.recon_failed_count, 0), ISNULL(fl.recon_not_applicable_count, 0)
    FROM Archive.IngestionFile f
    OUTER APPLY (
        SELECT
            COUNT(*) AS total_line_count,
            SUM(CASE WHEN line.matched_clearing_line_id IS NOT NULL THEN 1 ELSE 0 END) AS matched_line_count,
            SUM(CASE WHEN line.matched_clearing_line_id IS NULL THEN 1 ELSE 0 END) AS unmatched_line_count,
            SUM(ISNULL(d.amt, 0)) AS total_original_amount,
            SUM(CASE WHEN line.matched_clearing_line_id IS NOT NULL THEN ISNULL(d.amt, 0) ELSE 0 END) AS matched_amount,
            SUM(CASE WHEN line.matched_clearing_line_id IS NULL THEN ISNULL(d.amt, 0) ELSE 0 END) AS unmatched_amount,
            SUM(ISNULL(d.sett, 0)) AS total_settlement_amount,
            SUM(CASE WHEN line.reconciliation_status = 'Ready' THEN 1 ELSE 0 END) AS recon_ready_count,
            SUM(CASE WHEN line.reconciliation_status = 'Success' THEN 1 ELSE 0 END) AS recon_success_count,
            SUM(CASE WHEN line.reconciliation_status = 'Failed' THEN 1 ELSE 0 END) AS recon_failed_count,
            SUM(CASE WHEN line.reconciliation_status IS NULL THEN 1 ELSE 0 END) AS recon_not_applicable_count
        FROM Archive.IngestionFileLine line
        OUTER APPLY (
            SELECT TOP 1 cv.original_amount AS amt, cv.settlement_amount AS sett FROM (
                SELECT cv2.original_amount, cv2.settlement_amount FROM Archive.IngestionCardVisaDetail cv2 WHERE cv2.file_line_id = line.id AND f.content_type = 'Visa' AND f.file_type = 'Card'
                UNION ALL SELECT cm.original_amount, cm.settlement_amount FROM Archive.IngestionCardMscDetail cm WHERE cm.file_line_id = line.id AND f.content_type = 'Msc' AND f.file_type = 'Card'
                UNION ALL SELECT cb.original_amount, cb.settlement_amount FROM Archive.IngestionCardBkmDetail cb WHERE cb.file_line_id = line.id AND f.content_type = 'Bkm' AND f.file_type = 'Card'
                UNION ALL SELECT cv2.source_amount, 0 FROM Archive.IngestionClearingVisaDetail cv2 WHERE cv2.file_line_id = line.id AND f.content_type = 'Visa' AND f.file_type = 'Clearing'
                UNION ALL SELECT cm.source_amount, 0 FROM Archive.IngestionClearingMscDetail cm WHERE cm.file_line_id = line.id AND f.content_type = 'Msc' AND f.file_type = 'Clearing'
                UNION ALL SELECT cb.source_amount, 0 FROM Archive.IngestionClearingBkmDetail cb WHERE cb.file_line_id = line.id AND f.content_type = 'Bkm' AND f.file_type = 'Clearing'
            ) cv
        ) d
        WHERE line.file_id = f.id
    ) fl
) combined;
GO

-- -------------------------------------------------------------------------------------
-- E2. Günlük eşleşme oranı trendi (LIVE + ARCHIVE)
-- -------------------------------------------------------------------------------------
CREATE OR ALTER VIEW Reporting.vw_recon_match_rate_trend
AS
SELECT
    src.report_date, src.data_scope, src.network, src.side,
    SUM(src.total_line_count) AS total_line_count,
    SUM(src.matched_count) AS matched_count,
    SUM(src.unmatched_count) AS unmatched_count,
    CASE WHEN SUM(src.total_line_count) > 0 THEN ROUND(CAST(SUM(src.matched_count) AS DECIMAL(18,4)) / SUM(src.total_line_count) * 100, 2) ELSE 0 END AS match_rate_pct,
    SUM(src.total_amount) AS total_amount,
    SUM(src.matched_amount) AS matched_amount,
    SUM(src.unmatched_amount) AS unmatched_amount
FROM (
    SELECT CAST(f.create_date AS DATE) AS report_date, 'LIVE' AS data_scope, f.content_type AS network, 'Card' AS side,
        COUNT(*) AS total_line_count,
        SUM(CASE WHEN fl.matched_clearing_line_id IS NOT NULL THEN 1 ELSE 0 END) AS matched_count,
        SUM(CASE WHEN fl.matched_clearing_line_id IS NULL THEN 1 ELSE 0 END) AS unmatched_count,
        SUM(d.original_amount) AS total_amount,
        SUM(CASE WHEN fl.matched_clearing_line_id IS NOT NULL THEN d.original_amount ELSE 0 END) AS matched_amount,
        SUM(CASE WHEN fl.matched_clearing_line_id IS NULL THEN d.original_amount ELSE 0 END) AS unmatched_amount
    FROM Ingestion.FileLine fl JOIN Ingestion.[File] f ON f.id = fl.file_id AND f.file_type = 'Card'
    CROSS APPLY (
        SELECT cv.original_amount FROM Ingestion.CardVisaDetail cv WHERE cv.file_line_id = fl.id AND f.content_type = 'Visa'
        UNION ALL SELECT cm.original_amount FROM Ingestion.CardMscDetail cm WHERE cm.file_line_id = fl.id AND f.content_type = 'Msc'
        UNION ALL SELECT cb.original_amount FROM Ingestion.CardBkmDetail cb WHERE cb.file_line_id = fl.id AND f.content_type = 'Bkm'
    ) d
    GROUP BY CAST(f.create_date AS DATE), f.content_type
    UNION ALL
    SELECT CAST(f.create_date AS DATE), 'LIVE', f.content_type, 'Clearing',
        COUNT(*), SUM(CASE WHEN fl.matched_clearing_line_id IS NOT NULL THEN 1 ELSE 0 END), SUM(CASE WHEN fl.matched_clearing_line_id IS NULL THEN 1 ELSE 0 END),
        SUM(d.source_amount), SUM(CASE WHEN fl.matched_clearing_line_id IS NOT NULL THEN d.source_amount ELSE 0 END), SUM(CASE WHEN fl.matched_clearing_line_id IS NULL THEN d.source_amount ELSE 0 END)
    FROM Ingestion.FileLine fl JOIN Ingestion.[File] f ON f.id = fl.file_id AND f.file_type = 'Clearing'
    CROSS APPLY (
        SELECT cv.source_amount FROM Ingestion.ClearingVisaDetail cv WHERE cv.file_line_id = fl.id AND f.content_type = 'Visa'
        UNION ALL SELECT cm.source_amount FROM Ingestion.ClearingMscDetail cm WHERE cm.file_line_id = fl.id AND f.content_type = 'Msc'
        UNION ALL SELECT cb.source_amount FROM Ingestion.ClearingBkmDetail cb WHERE cb.file_line_id = fl.id AND f.content_type = 'Bkm'
    ) d
    GROUP BY CAST(f.create_date AS DATE), f.content_type
    UNION ALL
    SELECT CAST(f.create_date AS DATE), 'ARCHIVE', f.content_type, 'Card',
        COUNT(*), SUM(CASE WHEN fl.matched_clearing_line_id IS NOT NULL THEN 1 ELSE 0 END), SUM(CASE WHEN fl.matched_clearing_line_id IS NULL THEN 1 ELSE 0 END),
        SUM(d.original_amount), SUM(CASE WHEN fl.matched_clearing_line_id IS NOT NULL THEN d.original_amount ELSE 0 END), SUM(CASE WHEN fl.matched_clearing_line_id IS NULL THEN d.original_amount ELSE 0 END)
    FROM Archive.IngestionFileLine fl JOIN Archive.IngestionFile f ON f.id = fl.file_id AND f.file_type = 'Card'
    CROSS APPLY (
        SELECT cv.original_amount FROM Archive.IngestionCardVisaDetail cv WHERE cv.file_line_id = fl.id AND f.content_type = 'Visa'
        UNION ALL SELECT cm.original_amount FROM Archive.IngestionCardMscDetail cm WHERE cm.file_line_id = fl.id AND f.content_type = 'Msc'
        UNION ALL SELECT cb.original_amount FROM Archive.IngestionCardBkmDetail cb WHERE cb.file_line_id = fl.id AND f.content_type = 'Bkm'
    ) d
    GROUP BY CAST(f.create_date AS DATE), f.content_type
    UNION ALL
    SELECT CAST(f.create_date AS DATE), 'ARCHIVE', f.content_type, 'Clearing',
        COUNT(*), SUM(CASE WHEN fl.matched_clearing_line_id IS NOT NULL THEN 1 ELSE 0 END), SUM(CASE WHEN fl.matched_clearing_line_id IS NULL THEN 1 ELSE 0 END),
        SUM(d.source_amount), SUM(CASE WHEN fl.matched_clearing_line_id IS NOT NULL THEN d.source_amount ELSE 0 END), SUM(CASE WHEN fl.matched_clearing_line_id IS NULL THEN d.source_amount ELSE 0 END)
    FROM Archive.IngestionFileLine fl JOIN Archive.IngestionFile f ON f.id = fl.file_id AND f.file_type = 'Clearing'
    CROSS APPLY (
        SELECT cv.source_amount FROM Archive.IngestionClearingVisaDetail cv WHERE cv.file_line_id = fl.id AND f.content_type = 'Visa'
        UNION ALL SELECT cm.source_amount FROM Archive.IngestionClearingMscDetail cm WHERE cm.file_line_id = fl.id AND f.content_type = 'Msc'
        UNION ALL SELECT cb.source_amount FROM Archive.IngestionClearingBkmDetail cb WHERE cb.file_line_id = fl.id AND f.content_type = 'Bkm'
    ) d
    GROUP BY CAST(f.create_date AS DATE), f.content_type
) src
GROUP BY src.report_date, src.data_scope, src.network, src.side;
GO

-- -------------------------------------------------------------------------------------
-- E3. Card vs Clearing fark analizi (LIVE + ARCHIVE)
-- -------------------------------------------------------------------------------------
CREATE OR ALTER VIEW Reporting.vw_recon_gap_analysis
AS
SELECT
    ISNULL(c.report_date, cl.report_date) AS report_date,
    ISNULL(c.data_scope, cl.data_scope) AS data_scope,
    ISNULL(c.network, cl.network) AS network,
    ISNULL(c.card_line_count, 0) AS card_line_count,
    ISNULL(cl.clearing_line_count, 0) AS clearing_line_count,
    ISNULL(c.card_line_count, 0) - ISNULL(cl.clearing_line_count, 0) AS line_count_difference,
    ISNULL(c.card_matched_count, 0) AS card_matched_count,
    ISNULL(cl.clearing_matched_count, 0) AS clearing_matched_count,
    ISNULL(c.card_total_amount, 0) AS card_total_amount,
    ISNULL(cl.clearing_total_amount, 0) AS clearing_total_amount,
    ISNULL(c.card_total_amount, 0) - ISNULL(cl.clearing_total_amount, 0) AS amount_difference,
    CASE WHEN ISNULL(c.card_line_count, 0) > 0 THEN ROUND(CAST(c.card_matched_count AS DECIMAL(18,4)) / c.card_line_count * 100, 2) ELSE 0 END AS card_match_rate_pct,
    CASE WHEN ISNULL(cl.clearing_line_count, 0) > 0 THEN ROUND(CAST(cl.clearing_matched_count AS DECIMAL(18,4)) / cl.clearing_line_count * 100, 2) ELSE 0 END AS clearing_match_rate_pct
FROM (
    SELECT CAST(f.create_date AS DATE) AS report_date, 'LIVE' AS data_scope, f.content_type AS network,
        COUNT(*) AS card_line_count,
        SUM(CASE WHEN fl.matched_clearing_line_id IS NOT NULL THEN 1 ELSE 0 END) AS card_matched_count,
        SUM(d.original_amount) AS card_total_amount
    FROM Ingestion.FileLine fl JOIN Ingestion.[File] f ON f.id = fl.file_id AND f.file_type = 'Card'
    CROSS APPLY (
        SELECT cv.original_amount FROM Ingestion.CardVisaDetail cv WHERE cv.file_line_id = fl.id AND f.content_type = 'Visa'
        UNION ALL SELECT cm.original_amount FROM Ingestion.CardMscDetail cm WHERE cm.file_line_id = fl.id AND f.content_type = 'Msc'
        UNION ALL SELECT cb.original_amount FROM Ingestion.CardBkmDetail cb WHERE cb.file_line_id = fl.id AND f.content_type = 'Bkm'
    ) d
    GROUP BY CAST(f.create_date AS DATE), f.content_type
    UNION ALL
    SELECT CAST(f.create_date AS DATE), 'ARCHIVE', f.content_type,
        COUNT(*), SUM(CASE WHEN fl.matched_clearing_line_id IS NOT NULL THEN 1 ELSE 0 END), SUM(d.original_amount)
    FROM Archive.IngestionFileLine fl JOIN Archive.IngestionFile f ON f.id = fl.file_id AND f.file_type = 'Card'
    CROSS APPLY (
        SELECT cv.original_amount FROM Archive.IngestionCardVisaDetail cv WHERE cv.file_line_id = fl.id AND f.content_type = 'Visa'
        UNION ALL SELECT cm.original_amount FROM Archive.IngestionCardMscDetail cm WHERE cm.file_line_id = fl.id AND f.content_type = 'Msc'
        UNION ALL SELECT cb.original_amount FROM Archive.IngestionCardBkmDetail cb WHERE cb.file_line_id = fl.id AND f.content_type = 'Bkm'
    ) d
    GROUP BY CAST(f.create_date AS DATE), f.content_type
) c
FULL OUTER JOIN (
    SELECT CAST(f.create_date AS DATE) AS report_date, 'LIVE' AS data_scope, f.content_type AS network,
        COUNT(*) AS clearing_line_count,
        SUM(CASE WHEN fl.matched_clearing_line_id IS NOT NULL THEN 1 ELSE 0 END) AS clearing_matched_count,
        SUM(d.source_amount) AS clearing_total_amount
    FROM Ingestion.FileLine fl JOIN Ingestion.[File] f ON f.id = fl.file_id AND f.file_type = 'Clearing'
    CROSS APPLY (
        SELECT cv.source_amount FROM Ingestion.ClearingVisaDetail cv WHERE cv.file_line_id = fl.id AND f.content_type = 'Visa'
        UNION ALL SELECT cm.source_amount FROM Ingestion.ClearingMscDetail cm WHERE cm.file_line_id = fl.id AND f.content_type = 'Msc'
        UNION ALL SELECT cb.source_amount FROM Ingestion.ClearingBkmDetail cb WHERE cb.file_line_id = fl.id AND f.content_type = 'Bkm'
    ) d
    GROUP BY CAST(f.create_date AS DATE), f.content_type
    UNION ALL
    SELECT CAST(f.create_date AS DATE), 'ARCHIVE', f.content_type,
        COUNT(*), SUM(CASE WHEN fl.matched_clearing_line_id IS NOT NULL THEN 1 ELSE 0 END), SUM(d.source_amount)
    FROM Archive.IngestionFileLine fl JOIN Archive.IngestionFile f ON f.id = fl.file_id AND f.file_type = 'Clearing'
    CROSS APPLY (
        SELECT cv.source_amount FROM Archive.IngestionClearingVisaDetail cv WHERE cv.file_line_id = fl.id AND f.content_type = 'Visa'
        UNION ALL SELECT cm.source_amount FROM Archive.IngestionClearingMscDetail cm WHERE cm.file_line_id = fl.id AND f.content_type = 'Msc'
        UNION ALL SELECT cb.source_amount FROM Archive.IngestionClearingBkmDetail cb WHERE cb.file_line_id = fl.id AND f.content_type = 'Bkm'
    ) d
    GROUP BY CAST(f.create_date AS DATE), f.content_type
) cl ON c.report_date = cl.report_date AND c.data_scope = cl.data_scope AND c.network = cl.network;
GO

-- -------------------------------------------------------------------------------------
-- E4. Eşleşmemiş işlem yaş dağılımı (LIVE + ARCHIVE)
-- -------------------------------------------------------------------------------------
CREATE OR ALTER VIEW Reporting.vw_unmatched_transaction_aging
AS
SELECT
    x.age_bucket, x.data_scope, x.network, x.side,
    x.unmatched_count, x.unmatched_amount,
    CASE WHEN t.total_unmatched > 0 THEN ROUND(CAST(x.unmatched_count AS DECIMAL(18,4)) / t.total_unmatched * 100, 2) ELSE 0 END AS pct_of_total_unmatched
FROM (
    SELECT
        CASE
            WHEN DATEDIFF(DAY, f.create_date, GETUTCDATE()) < 1 THEN '0-1d'
            WHEN DATEDIFF(DAY, f.create_date, GETUTCDATE()) < 3 THEN '1-3d'
            WHEN DATEDIFF(DAY, f.create_date, GETUTCDATE()) < 7 THEN '3-7d'
            WHEN DATEDIFF(DAY, f.create_date, GETUTCDATE()) < 14 THEN '7-14d'
            WHEN DATEDIFF(DAY, f.create_date, GETUTCDATE()) < 30 THEN '14-30d'
            ELSE '30d+'
        END AS age_bucket,
        'LIVE' AS data_scope, f.content_type AS network, f.file_type AS side,
        COUNT(*) AS unmatched_count, SUM(ISNULL(d.amount, 0)) AS unmatched_amount
    FROM Ingestion.FileLine fl JOIN Ingestion.[File] f ON f.id = fl.file_id
    OUTER APPLY (
        SELECT TOP 1 cv.amount FROM (
            SELECT cv2.original_amount AS amount FROM Ingestion.CardVisaDetail cv2 WHERE cv2.file_line_id = fl.id AND f.content_type = 'Visa' AND f.file_type = 'Card'
            UNION ALL SELECT cm.original_amount FROM Ingestion.CardMscDetail cm WHERE cm.file_line_id = fl.id AND f.content_type = 'Msc' AND f.file_type = 'Card'
            UNION ALL SELECT cb.original_amount FROM Ingestion.CardBkmDetail cb WHERE cb.file_line_id = fl.id AND f.content_type = 'Bkm' AND f.file_type = 'Card'
            UNION ALL SELECT cv2.source_amount FROM Ingestion.ClearingVisaDetail cv2 WHERE cv2.file_line_id = fl.id AND f.content_type = 'Visa' AND f.file_type = 'Clearing'
            UNION ALL SELECT cm.source_amount FROM Ingestion.ClearingMscDetail cm WHERE cm.file_line_id = fl.id AND f.content_type = 'Msc' AND f.file_type = 'Clearing'
            UNION ALL SELECT cb.source_amount FROM Ingestion.ClearingBkmDetail cb WHERE cb.file_line_id = fl.id AND f.content_type = 'Bkm' AND f.file_type = 'Clearing'
        ) cv
    ) d
    WHERE fl.matched_clearing_line_id IS NULL
    GROUP BY CASE
            WHEN DATEDIFF(DAY, f.create_date, GETUTCDATE()) < 1 THEN '0-1d'
            WHEN DATEDIFF(DAY, f.create_date, GETUTCDATE()) < 3 THEN '1-3d'
            WHEN DATEDIFF(DAY, f.create_date, GETUTCDATE()) < 7 THEN '3-7d'
            WHEN DATEDIFF(DAY, f.create_date, GETUTCDATE()) < 14 THEN '7-14d'
            WHEN DATEDIFF(DAY, f.create_date, GETUTCDATE()) < 30 THEN '14-30d'
            ELSE '30d+'
        END, f.content_type, f.file_type
    UNION ALL
    SELECT
        CASE
            WHEN DATEDIFF(DAY, f.create_date, GETUTCDATE()) < 1 THEN '0-1d'
            WHEN DATEDIFF(DAY, f.create_date, GETUTCDATE()) < 3 THEN '1-3d'
            WHEN DATEDIFF(DAY, f.create_date, GETUTCDATE()) < 7 THEN '3-7d'
            WHEN DATEDIFF(DAY, f.create_date, GETUTCDATE()) < 14 THEN '7-14d'
            WHEN DATEDIFF(DAY, f.create_date, GETUTCDATE()) < 30 THEN '14-30d'
            ELSE '30d+'
        END,
        'ARCHIVE', f.content_type, f.file_type,
        COUNT(*), SUM(ISNULL(d.amount, 0))
    FROM Archive.IngestionFileLine fl JOIN Archive.IngestionFile f ON f.id = fl.file_id
    OUTER APPLY (
        SELECT TOP 1 cv.amount FROM (
            SELECT cv2.original_amount AS amount FROM Archive.IngestionCardVisaDetail cv2 WHERE cv2.file_line_id = fl.id AND f.content_type = 'Visa' AND f.file_type = 'Card'
            UNION ALL SELECT cm.original_amount FROM Archive.IngestionCardMscDetail cm WHERE cm.file_line_id = fl.id AND f.content_type = 'Msc' AND f.file_type = 'Card'
            UNION ALL SELECT cb.original_amount FROM Archive.IngestionCardBkmDetail cb WHERE cb.file_line_id = fl.id AND f.content_type = 'Bkm' AND f.file_type = 'Card'
            UNION ALL SELECT cv2.source_amount FROM Archive.IngestionClearingVisaDetail cv2 WHERE cv2.file_line_id = fl.id AND f.content_type = 'Visa' AND f.file_type = 'Clearing'
            UNION ALL SELECT cm.source_amount FROM Archive.IngestionClearingMscDetail cm WHERE cm.file_line_id = fl.id AND f.content_type = 'Msc' AND f.file_type = 'Clearing'
            UNION ALL SELECT cb.source_amount FROM Archive.IngestionClearingBkmDetail cb WHERE cb.file_line_id = fl.id AND f.content_type = 'Bkm' AND f.file_type = 'Clearing'
        ) cv
    ) d
    WHERE fl.matched_clearing_line_id IS NULL
    GROUP BY CASE
            WHEN DATEDIFF(DAY, f.create_date, GETUTCDATE()) < 1 THEN '0-1d'
            WHEN DATEDIFF(DAY, f.create_date, GETUTCDATE()) < 3 THEN '1-3d'
            WHEN DATEDIFF(DAY, f.create_date, GETUTCDATE()) < 7 THEN '3-7d'
            WHEN DATEDIFF(DAY, f.create_date, GETUTCDATE()) < 14 THEN '7-14d'
            WHEN DATEDIFF(DAY, f.create_date, GETUTCDATE()) < 30 THEN '14-30d'
            ELSE '30d+'
        END, f.content_type, f.file_type
) x
CROSS JOIN (
    SELECT COUNT(*) AS total_unmatched FROM (
        SELECT 1 AS v FROM Ingestion.FileLine WHERE matched_clearing_line_id IS NULL
        UNION ALL
        SELECT 1 FROM Archive.IngestionFileLine WHERE matched_clearing_line_id IS NULL
    ) u
) t;
GO

-- -------------------------------------------------------------------------------------
-- E5. Network bazlı mutabakat skorkartı (LIVE + ARCHIVE)
-- -------------------------------------------------------------------------------------
CREATE OR ALTER VIEW Reporting.vw_network_recon_scorecard
AS
SELECT
    x.data_scope, x.network, x.total_file_count,
    x.total_card_line_count, x.total_clearing_line_count,
    x.total_matched_count, x.total_unmatched_count,
    CASE WHEN (x.total_card_line_count + x.total_clearing_line_count) > 0
        THEN ROUND(CAST(x.total_matched_count AS DECIMAL(18,4)) / (x.total_card_line_count + x.total_clearing_line_count) * 100, 2) ELSE 0 END AS overall_match_rate_pct,
    x.total_card_amount, x.total_clearing_amount,
    ISNULL(x.total_card_amount, 0) - ISNULL(x.total_clearing_amount, 0) AS net_amount_difference,
    x.avg_card_original_amount, x.avg_clearing_source_amount,
    x.recon_success_line_count, x.recon_failed_line_count, x.recon_pending_line_count,
    CASE WHEN (x.recon_success_line_count + x.recon_failed_line_count + x.recon_pending_line_count) > 0
        THEN ROUND(CAST(x.recon_success_line_count AS DECIMAL(18,4)) / (x.recon_success_line_count + x.recon_failed_line_count + x.recon_pending_line_count) * 100, 2) ELSE 0 END AS recon_success_rate_pct,
    x.first_file_date, x.last_file_date
FROM (
    -- LIVE
    SELECT
        'LIVE' AS data_scope, f.content_type AS network,
        COUNT(DISTINCT f.id) AS total_file_count,
        SUM(CASE WHEN f.file_type = 'Card' THEN 1 ELSE 0 END) AS total_card_line_count,
        SUM(CASE WHEN f.file_type = 'Clearing' THEN 1 ELSE 0 END) AS total_clearing_line_count,
        SUM(CASE WHEN fl.matched_clearing_line_id IS NOT NULL THEN 1 ELSE 0 END) AS total_matched_count,
        SUM(CASE WHEN fl.matched_clearing_line_id IS NULL THEN 1 ELSE 0 END) AS total_unmatched_count,
        SUM(CASE WHEN f.file_type = 'Card' THEN ISNULL(d.amt, 0) ELSE 0 END) AS total_card_amount,
        SUM(CASE WHEN f.file_type = 'Clearing' THEN ISNULL(d.amt, 0) ELSE 0 END) AS total_clearing_amount,
        CASE WHEN SUM(CASE WHEN f.file_type = 'Card' THEN 1 ELSE 0 END) > 0
            THEN ROUND(CAST(SUM(CASE WHEN f.file_type = 'Card' THEN ISNULL(d.amt, 0) ELSE 0 END) AS DECIMAL(18,4)) / SUM(CASE WHEN f.file_type = 'Card' THEN 1 ELSE 0 END), 2) ELSE 0 END AS avg_card_original_amount,
        CASE WHEN SUM(CASE WHEN f.file_type = 'Clearing' THEN 1 ELSE 0 END) > 0
            THEN ROUND(CAST(SUM(CASE WHEN f.file_type = 'Clearing' THEN ISNULL(d.amt, 0) ELSE 0 END) AS DECIMAL(18,4)) / SUM(CASE WHEN f.file_type = 'Clearing' THEN 1 ELSE 0 END), 2) ELSE 0 END AS avg_clearing_source_amount,
        SUM(CASE WHEN fl.reconciliation_status = 'Success' THEN 1 ELSE 0 END) AS recon_success_line_count,
        SUM(CASE WHEN fl.reconciliation_status = 'Failed' THEN 1 ELSE 0 END) AS recon_failed_line_count,
        SUM(CASE WHEN fl.reconciliation_status IN ('Ready', 'Processing') THEN 1 ELSE 0 END) AS recon_pending_line_count,
        MIN(f.create_date) AS first_file_date,
        MAX(f.create_date) AS last_file_date
    FROM Ingestion.FileLine fl
    JOIN Ingestion.[File] f ON f.id = fl.file_id
    OUTER APPLY (
        SELECT TOP 1 cv.amount AS amt FROM (
            SELECT cv2.original_amount AS amount FROM Ingestion.CardVisaDetail cv2 WHERE cv2.file_line_id = fl.id AND f.content_type = 'Visa' AND f.file_type = 'Card'
            UNION ALL SELECT cm.original_amount FROM Ingestion.CardMscDetail cm WHERE cm.file_line_id = fl.id AND f.content_type = 'Msc' AND f.file_type = 'Card'
            UNION ALL SELECT cb.original_amount FROM Ingestion.CardBkmDetail cb WHERE cb.file_line_id = fl.id AND f.content_type = 'Bkm' AND f.file_type = 'Card'
            UNION ALL SELECT cv2.source_amount FROM Ingestion.ClearingVisaDetail cv2 WHERE cv2.file_line_id = fl.id AND f.content_type = 'Visa' AND f.file_type = 'Clearing'
            UNION ALL SELECT cm.source_amount FROM Ingestion.ClearingMscDetail cm WHERE cm.file_line_id = fl.id AND f.content_type = 'Msc' AND f.file_type = 'Clearing'
            UNION ALL SELECT cb.source_amount FROM Ingestion.ClearingBkmDetail cb WHERE cb.file_line_id = fl.id AND f.content_type = 'Bkm' AND f.file_type = 'Clearing'
        ) cv
    ) d
    GROUP BY f.content_type
    UNION ALL
    -- ARCHIVE
    SELECT
        'ARCHIVE', f.content_type,
        COUNT(DISTINCT f.id),
        SUM(CASE WHEN f.file_type = 'Card' THEN 1 ELSE 0 END),
        SUM(CASE WHEN f.file_type = 'Clearing' THEN 1 ELSE 0 END),
        SUM(CASE WHEN fl.matched_clearing_line_id IS NOT NULL THEN 1 ELSE 0 END),
        SUM(CASE WHEN fl.matched_clearing_line_id IS NULL THEN 1 ELSE 0 END),
        SUM(CASE WHEN f.file_type = 'Card' THEN ISNULL(d.amt, 0) ELSE 0 END),
        SUM(CASE WHEN f.file_type = 'Clearing' THEN ISNULL(d.amt, 0) ELSE 0 END),
        CASE WHEN SUM(CASE WHEN f.file_type = 'Card' THEN 1 ELSE 0 END) > 0
            THEN ROUND(CAST(SUM(CASE WHEN f.file_type = 'Card' THEN ISNULL(d.amt, 0) ELSE 0 END) AS DECIMAL(18,4)) / SUM(CASE WHEN f.file_type = 'Card' THEN 1 ELSE 0 END), 2) ELSE 0 END,
        CASE WHEN SUM(CASE WHEN f.file_type = 'Clearing' THEN 1 ELSE 0 END) > 0
            THEN ROUND(CAST(SUM(CASE WHEN f.file_type = 'Clearing' THEN ISNULL(d.amt, 0) ELSE 0 END) AS DECIMAL(18,4)) / SUM(CASE WHEN f.file_type = 'Clearing' THEN 1 ELSE 0 END), 2) ELSE 0 END,
        SUM(CASE WHEN fl.reconciliation_status = 'Success' THEN 1 ELSE 0 END),
        SUM(CASE WHEN fl.reconciliation_status = 'Failed' THEN 1 ELSE 0 END),
        SUM(CASE WHEN fl.reconciliation_status IN ('Ready', 'Processing') THEN 1 ELSE 0 END),
        MIN(f.create_date), MAX(f.create_date)
    FROM Archive.IngestionFileLine fl
    JOIN Archive.IngestionFile f ON f.id = fl.file_id
    OUTER APPLY (
        SELECT TOP 1 cv.amount AS amt FROM (
            SELECT cv2.original_amount AS amount FROM Archive.IngestionCardVisaDetail cv2 WHERE cv2.file_line_id = fl.id AND f.content_type = 'Visa' AND f.file_type = 'Card'
            UNION ALL SELECT cm.original_amount FROM Archive.IngestionCardMscDetail cm WHERE cm.file_line_id = fl.id AND f.content_type = 'Msc' AND f.file_type = 'Card'
            UNION ALL SELECT cb.original_amount FROM Archive.IngestionCardBkmDetail cb WHERE cb.file_line_id = fl.id AND f.content_type = 'Bkm' AND f.file_type = 'Card'
            UNION ALL SELECT cv2.source_amount FROM Archive.IngestionClearingVisaDetail cv2 WHERE cv2.file_line_id = fl.id AND f.content_type = 'Visa' AND f.file_type = 'Clearing'
            UNION ALL SELECT cm.source_amount FROM Archive.IngestionClearingMscDetail cm WHERE cm.file_line_id = fl.id AND f.content_type = 'Msc' AND f.file_type = 'Clearing'
            UNION ALL SELECT cb.source_amount FROM Archive.IngestionClearingBkmDetail cb WHERE cb.file_line_id = fl.id AND f.content_type = 'Bkm' AND f.file_type = 'Clearing'
        ) cv
    ) d
    GROUP BY f.content_type
) x;
GO
