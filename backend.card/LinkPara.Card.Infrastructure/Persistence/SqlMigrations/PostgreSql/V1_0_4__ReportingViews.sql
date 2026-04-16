-- =====================================================================================
-- REPORTING VIEWS - PostgreSQL Migration
-- Version: V1_0_4
-- Purpose: Operational reporting semantic layer for File Ingestion, Reconciliation, Archive
-- =====================================================================================

DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_namespace WHERE nspname = 'reporting') THEN
CREATE SCHEMA reporting;
END IF;
END $$;

-- =====================================================================================
-- A. FILE INGESTION
-- =====================================================================================

-- -------------------------------------------------------------------------------------
-- A1. Dosya bazında genel operasyon özeti (LIVE + ARCHIVE)
-- -------------------------------------------------------------------------------------
CREATE OR REPLACE VIEW reporting.vw_ingestion_file_overview AS
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
                THEN ROUND((f.successful_line_count::numeric / f.processed_line_count) * 100, 2)
            ELSE 0
            END AS line_success_rate_pct,
        CASE
            WHEN f.processed_line_count > 0
                THEN ROUND((f.failed_line_count::numeric / f.processed_line_count) * 100, 2)
            ELSE 0
            END AS line_fail_rate_pct,
        CASE
            WHEN f.expected_line_count > 0
                THEN ROUND((f.processed_line_count::numeric / f.expected_line_count) * 100, 2)
            ELSE 0
            END AS completeness_pct,
        COALESCE(fl.total_line_count, 0) AS actual_line_count,
        COALESCE(fl.success_line_count, 0) AS actual_success_line_count,
        COALESCE(fl.failed_line_count, 0) AS actual_failed_line_count,
        COALESCE(fl.processing_line_count, 0) AS actual_processing_line_count,
        COALESCE(fl.duplicate_line_count, 0) AS duplicate_line_count,
        COALESCE(fl.recon_ready_count, 0) AS recon_ready_count,
        COALESCE(fl.recon_success_count, 0) AS recon_success_count,
        COALESCE(fl.recon_failed_count, 0) AS recon_failed_count,
        EXTRACT(EPOCH FROM (COALESCE(f.update_date, f.create_date) - f.create_date))::bigint AS processing_duration_seconds
    FROM ingestion.file f
             LEFT JOIN LATERAL (
        SELECT
            COUNT(*) AS total_line_count,
            COUNT(*) FILTER (WHERE line.status = 'Success') AS success_line_count,
            COUNT(*) FILTER (WHERE line.status = 'Failed') AS failed_line_count,
            COUNT(*) FILTER (WHERE line.status = 'Processing') AS processing_line_count,
            COUNT(*) FILTER (WHERE line.duplicate_status IN ('Secondary', 'Conflict')) AS duplicate_line_count,
            COUNT(*) FILTER (WHERE line.reconciliation_status = 'Ready') AS recon_ready_count,
            COUNT(*) FILTER (WHERE line.reconciliation_status = 'Success') AS recon_success_count,
            COUNT(*) FILTER (WHERE line.reconciliation_status = 'Failed') AS recon_failed_count
        FROM ingestion.file_line line
        WHERE line.file_id = f.id
            ) fl ON TRUE

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
                THEN ROUND((f.successful_line_count::numeric / f.processed_line_count) * 100, 2)
            ELSE 0
            END AS line_success_rate_pct,
        CASE
            WHEN f.processed_line_count > 0
                THEN ROUND((f.failed_line_count::numeric / f.processed_line_count) * 100, 2)
            ELSE 0
            END AS line_fail_rate_pct,
        CASE
            WHEN f.expected_line_count > 0
                THEN ROUND((f.processed_line_count::numeric / f.expected_line_count) * 100, 2)
            ELSE 0
            END AS completeness_pct,
        COALESCE(fl.total_line_count, 0) AS actual_line_count,
        COALESCE(fl.success_line_count, 0) AS actual_success_line_count,
        COALESCE(fl.failed_line_count, 0) AS actual_failed_line_count,
        COALESCE(fl.processing_line_count, 0) AS actual_processing_line_count,
        COALESCE(fl.duplicate_line_count, 0) AS duplicate_line_count,
        COALESCE(fl.recon_ready_count, 0) AS recon_ready_count,
        COALESCE(fl.recon_success_count, 0) AS recon_success_count,
        COALESCE(fl.recon_failed_count, 0) AS recon_failed_count,
        EXTRACT(EPOCH FROM (COALESCE(f.update_date, f.create_date) - f.create_date))::bigint AS processing_duration_seconds
    FROM archive.ingestion_file f
             LEFT JOIN LATERAL (
        SELECT
            COUNT(*) AS total_line_count,
            COUNT(*) FILTER (WHERE line.status = 'Success') AS success_line_count,
            COUNT(*) FILTER (WHERE line.status = 'Failed') AS failed_line_count,
            COUNT(*) FILTER (WHERE line.status = 'Processing') AS processing_line_count,
            COUNT(*) FILTER (WHERE line.duplicate_status IN ('Secondary', 'Conflict')) AS duplicate_line_count,
            COUNT(*) FILTER (WHERE line.reconciliation_status = 'Ready') AS recon_ready_count,
            COUNT(*) FILTER (WHERE line.reconciliation_status = 'Success') AS recon_success_count,
            COUNT(*) FILTER (WHERE line.reconciliation_status = 'Failed') AS recon_failed_count
        FROM archive.ingestion_file_line line
        WHERE line.file_id = f.id
            ) fl ON TRUE
) combined;

-- -------------------------------------------------------------------------------------
-- A2. Dosya kalitesi ve duplicate etkisi (LIVE + ARCHIVE)
-- -------------------------------------------------------------------------------------
CREATE OR REPLACE VIEW reporting.vw_ingestion_file_quality AS
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
        COALESCE(x.total_line_count, 0) AS total_line_count,
        COALESCE(x.success_line_count, 0) AS success_line_count,
        COALESCE(x.failed_line_count, 0) AS failed_line_count,
        COALESCE(x.processing_line_count, 0) AS processing_line_count,
        COALESCE(x.unique_count, 0) AS duplicate_unique_count,
        COALESCE(x.primary_count, 0) AS duplicate_primary_count,
        COALESCE(x.secondary_count, 0) AS duplicate_secondary_count,
        COALESCE(x.conflict_count, 0) AS duplicate_conflict_count,
        COALESCE(x.total_retry_count, 0) AS total_retry_count,
        COALESCE(x.lines_with_retry_count, 0) AS lines_with_retry_count,
        CASE
            WHEN COALESCE(x.total_line_count, 0) > 0
                THEN ROUND((x.failed_line_count::numeric / x.total_line_count) * 100, 2)
            ELSE 0
            END AS error_rate_pct,
        CASE
            WHEN COALESCE(x.total_line_count, 0) > 0
                THEN ROUND(((COALESCE(x.secondary_count, 0) + COALESCE(x.conflict_count, 0))::numeric / x.total_line_count) * 100, 2)
            ELSE 0
            END AS duplicate_impact_pct
    FROM ingestion.file f
             LEFT JOIN LATERAL (
        SELECT
            COUNT(*) AS total_line_count,
            COUNT(*) FILTER (WHERE line.status = 'Success') AS success_line_count,
            COUNT(*) FILTER (WHERE line.status = 'Failed') AS failed_line_count,
            COUNT(*) FILTER (WHERE line.status = 'Processing') AS processing_line_count,
            COUNT(*) FILTER (WHERE line.duplicate_status = 'Unique') AS unique_count,
            COUNT(*) FILTER (WHERE line.duplicate_status = 'Primary') AS primary_count,
            COUNT(*) FILTER (WHERE line.duplicate_status = 'Secondary') AS secondary_count,
            COUNT(*) FILTER (WHERE line.duplicate_status = 'Conflict') AS conflict_count,
            SUM(line.retry_count) AS total_retry_count,
            COUNT(*) FILTER (WHERE line.retry_count > 0) AS lines_with_retry_count
        FROM ingestion.file_line line
        WHERE line.file_id = f.id
            ) x ON TRUE

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
        COALESCE(x.total_line_count, 0) AS total_line_count,
        COALESCE(x.success_line_count, 0) AS success_line_count,
        COALESCE(x.failed_line_count, 0) AS failed_line_count,
        COALESCE(x.processing_line_count, 0) AS processing_line_count,
        COALESCE(x.unique_count, 0) AS duplicate_unique_count,
        COALESCE(x.primary_count, 0) AS duplicate_primary_count,
        COALESCE(x.secondary_count, 0) AS duplicate_secondary_count,
        COALESCE(x.conflict_count, 0) AS duplicate_conflict_count,
        COALESCE(x.total_retry_count, 0) AS total_retry_count,
        COALESCE(x.lines_with_retry_count, 0) AS lines_with_retry_count,
        CASE
            WHEN COALESCE(x.total_line_count, 0) > 0
                THEN ROUND((x.failed_line_count::numeric / x.total_line_count) * 100, 2)
            ELSE 0
            END AS error_rate_pct,
        CASE
            WHEN COALESCE(x.total_line_count, 0) > 0
                THEN ROUND(((COALESCE(x.secondary_count, 0) + COALESCE(x.conflict_count, 0))::numeric / x.total_line_count) * 100, 2)
            ELSE 0
            END AS duplicate_impact_pct
    FROM archive.ingestion_file f
             LEFT JOIN LATERAL (
        SELECT
            COUNT(*) AS total_line_count,
            COUNT(*) FILTER (WHERE line.status = 'Success') AS success_line_count,
            COUNT(*) FILTER (WHERE line.status = 'Failed') AS failed_line_count,
            COUNT(*) FILTER (WHERE line.status = 'Processing') AS processing_line_count,
            COUNT(*) FILTER (WHERE line.duplicate_status = 'Unique') AS unique_count,
            COUNT(*) FILTER (WHERE line.duplicate_status = 'Primary') AS primary_count,
            COUNT(*) FILTER (WHERE line.duplicate_status = 'Secondary') AS secondary_count,
            COUNT(*) FILTER (WHERE line.duplicate_status = 'Conflict') AS conflict_count,
            SUM(line.retry_count) AS total_retry_count,
            COUNT(*) FILTER (WHERE line.retry_count > 0) AS lines_with_retry_count
        FROM archive.ingestion_file_line line
        WHERE line.file_id = f.id
            ) x ON TRUE
) combined;

-- -------------------------------------------------------------------------------------
-- A3. Günlük ingestion trendi (LIVE + ARCHIVE)
-- -------------------------------------------------------------------------------------
CREATE OR REPLACE VIEW reporting.vw_ingestion_daily_summary AS
SELECT
    data_scope,
    report_date,
    content_type,
    file_type,
    SUM(file_count) AS file_count,
    SUM(success_file_count) AS success_file_count,
    SUM(failed_file_count) AS failed_file_count,
    SUM(processing_file_count) AS processing_file_count,
    SUM(expected_line_count) AS expected_line_count,
    SUM(processed_line_count) AS processed_line_count,
    SUM(successful_line_count) AS successful_line_count,
    SUM(failed_line_count) AS failed_line_count,
    CASE
        WHEN SUM(processed_line_count) > 0
            THEN ROUND((SUM(successful_line_count)::numeric / SUM(processed_line_count)) * 100, 2)
        ELSE 0
        END AS processed_line_success_rate_pct
FROM (
    -- LIVE
    SELECT
        'LIVE' AS data_scope,
        DATE_TRUNC('day', f.create_date)::date AS report_date,
        f.content_type,
        f.file_type,
        COUNT(*) AS file_count,
        COUNT(*) FILTER (WHERE f.status = 'Success') AS success_file_count,
        COUNT(*) FILTER (WHERE f.status = 'Failed') AS failed_file_count,
        COUNT(*) FILTER (WHERE f.status = 'Processing') AS processing_file_count,
        SUM(f.expected_line_count) AS expected_line_count,
        SUM(f.processed_line_count) AS processed_line_count,
        SUM(f.successful_line_count) AS successful_line_count,
        SUM(f.failed_line_count) AS failed_line_count
    FROM ingestion.file f
    GROUP BY DATE_TRUNC('day', f.create_date)::date, f.content_type, f.file_type

    UNION ALL

    -- ARCHIVE
    SELECT
        'ARCHIVE' AS data_scope,
        DATE_TRUNC('day', f.create_date)::date AS report_date,
        f.content_type,
        f.file_type,
        COUNT(*) AS file_count,
        COUNT(*) FILTER (WHERE f.status = 'Success') AS success_file_count,
        COUNT(*) FILTER (WHERE f.status = 'Failed') AS failed_file_count,
        COUNT(*) FILTER (WHERE f.status = 'Processing') AS processing_file_count,
        SUM(f.expected_line_count) AS expected_line_count,
        SUM(f.processed_line_count) AS processed_line_count,
        SUM(f.successful_line_count) AS successful_line_count,
        SUM(f.failed_line_count) AS failed_line_count
    FROM archive.ingestion_file f
    GROUP BY DATE_TRUNC('day', f.create_date)::date, f.content_type, f.file_type
) src
GROUP BY data_scope, report_date, content_type, file_type;

-- -------------------------------------------------------------------------------------
-- A4. Network x file type matrisi (LIVE + ARCHIVE)
-- -------------------------------------------------------------------------------------
CREATE OR REPLACE VIEW reporting.vw_ingestion_network_matrix AS
SELECT
    data_scope,
    content_type,
    file_type,
    SUM(file_count) AS file_count,
    SUM(success_file_count) AS success_file_count,
    SUM(failed_file_count) AS failed_file_count,
    SUM(processed_line_count) AS processed_line_count,
    SUM(successful_line_count) AS successful_line_count,
    SUM(failed_line_count) AS failed_line_count,
    MIN(first_file_at) AS first_file_at,
    MAX(last_file_at) AS last_file_at
FROM (
    -- LIVE
    SELECT
        'LIVE' AS data_scope,
        f.content_type,
        f.file_type,
        COUNT(*) AS file_count,
        COUNT(*) FILTER (WHERE f.status = 'Success') AS success_file_count,
        COUNT(*) FILTER (WHERE f.status = 'Failed') AS failed_file_count,
        SUM(f.processed_line_count) AS processed_line_count,
        SUM(f.successful_line_count) AS successful_line_count,
        SUM(f.failed_line_count) AS failed_line_count,
        MIN(f.create_date) AS first_file_at,
        MAX(f.create_date) AS last_file_at
    FROM ingestion.file f
    GROUP BY f.content_type, f.file_type

    UNION ALL

    -- ARCHIVE
    SELECT
        'ARCHIVE' AS data_scope,
        f.content_type,
        f.file_type,
        COUNT(*) AS file_count,
        COUNT(*) FILTER (WHERE f.status = 'Success') AS success_file_count,
        COUNT(*) FILTER (WHERE f.status = 'Failed') AS failed_file_count,
        SUM(f.processed_line_count) AS processed_line_count,
        SUM(f.successful_line_count) AS successful_line_count,
        SUM(f.failed_line_count) AS failed_line_count,
        MIN(f.create_date) AS first_file_at,
        MAX(f.create_date) AS last_file_at
    FROM archive.ingestion_file f
    GROUP BY f.content_type, f.file_type
) src
GROUP BY data_scope, content_type, file_type;

-- -------------------------------------------------------------------------------------
-- A5. Problem hotspot dosyaları (LIVE + ARCHIVE)
-- -------------------------------------------------------------------------------------
CREATE OR REPLACE VIEW reporting.vw_ingestion_exception_hotspots AS
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
        COALESCE(x.total_retry_count, 0) AS total_retry_count,
        COALESCE(x.max_retry_count, 0) AS max_retry_count,
        COALESCE(x.error_message_count, 0) AS distinct_error_message_count,
        CASE
            WHEN f.status = 'Failed' THEN 'CRITICAL'
            WHEN f.processed_line_count > 0 AND (f.failed_line_count::numeric / f.processed_line_count) >= 0.20 THEN 'HIGH'
            WHEN f.failed_line_count > 0 THEN 'MEDIUM'
            ELSE 'LOW'
            END AS severity_level
    FROM ingestion.file f
             LEFT JOIN LATERAL (
        SELECT
            SUM(line.retry_count) AS total_retry_count,
            MAX(line.retry_count) AS max_retry_count,
            COUNT(DISTINCT line.message) FILTER (WHERE line.status = 'Failed') AS error_message_count
        FROM ingestion.file_line line
        WHERE line.file_id = f.id
            ) x ON TRUE
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
        COALESCE(x.total_retry_count, 0) AS total_retry_count,
        COALESCE(x.max_retry_count, 0) AS max_retry_count,
        COALESCE(x.error_message_count, 0) AS distinct_error_message_count,
        CASE
            WHEN f.status = 'Failed' THEN 'CRITICAL'
            WHEN f.processed_line_count > 0 AND (f.failed_line_count::numeric / f.processed_line_count) >= 0.20 THEN 'HIGH'
            WHEN f.failed_line_count > 0 THEN 'MEDIUM'
            ELSE 'LOW'
            END AS severity_level
    FROM archive.ingestion_file f
             LEFT JOIN LATERAL (
        SELECT
            SUM(line.retry_count) AS total_retry_count,
            MAX(line.retry_count) AS max_retry_count,
            COUNT(DISTINCT line.message) FILTER (WHERE line.status = 'Failed') AS error_message_count
        FROM archive.ingestion_file_line line
        WHERE line.file_id = f.id
            ) x ON TRUE
    WHERE f.status = 'Failed'
       OR f.failed_line_count > 0
) combined;

-- =====================================================================================
-- B. RECONCILIATION PROCESS
-- =====================================================================================

-- -------------------------------------------------------------------------------------
-- B1. Günlük mutabakat süreç özeti (LIVE + ARCHIVE)
-- -------------------------------------------------------------------------------------
CREATE OR REPLACE VIEW reporting.vw_recon_daily_overview AS
WITH dates AS (
    SELECT DISTINCT DATE_TRUNC('day', create_date)::date AS report_date, 'LIVE' AS data_scope FROM reconciliation.evaluation
    UNION
    SELECT DISTINCT DATE_TRUNC('day', create_date)::date, 'LIVE' FROM reconciliation.operation
    UNION
    SELECT DISTINCT DATE_TRUNC('day', create_date)::date, 'LIVE' FROM reconciliation.alert
    UNION
    SELECT DISTINCT DATE_TRUNC('day', create_date)::date, 'LIVE' FROM reconciliation.review
    UNION
    SELECT DISTINCT DATE_TRUNC('day', create_date)::date, 'ARCHIVE' FROM archive.reconciliation_evaluation
    UNION
    SELECT DISTINCT DATE_TRUNC('day', create_date)::date, 'ARCHIVE' FROM archive.reconciliation_operation
    UNION
    SELECT DISTINCT DATE_TRUNC('day', create_date)::date, 'ARCHIVE' FROM archive.reconciliation_alert
    UNION
    SELECT DISTINCT DATE_TRUNC('day', create_date)::date, 'ARCHIVE' FROM archive.reconciliation_review
)
SELECT
    d.data_scope,
    d.report_date,

    COALESCE(ev.total_evaluation_count, 0) AS total_evaluation_count,
    COALESCE(ev.completed_evaluation_count, 0) AS completed_evaluation_count,
    COALESCE(ev.failed_evaluation_count, 0) AS failed_evaluation_count,

    COALESCE(op.total_operation_count, 0) AS total_operation_count,
    COALESCE(op.completed_operation_count, 0) AS completed_operation_count,
    COALESCE(op.failed_operation_count, 0) AS failed_operation_count,
    COALESCE(op.blocked_operation_count, 0) AS blocked_operation_count,
    COALESCE(op.planned_operation_count, 0) AS planned_operation_count,
    COALESCE(op.manual_operation_count, 0) AS manual_operation_count,

    COALESCE(ex.total_execution_count, 0) AS total_execution_count,
    COALESCE(ex.completed_execution_count, 0) AS completed_execution_count,
    COALESCE(ex.failed_execution_count, 0) AS failed_execution_count,
    COALESCE(ex.avg_execution_duration_seconds, 0) AS avg_execution_duration_seconds,

    COALESCE(rv.pending_review_count, 0) AS pending_review_count,
    COALESCE(rv.approved_review_count, 0) AS approved_review_count,
    COALESCE(rv.rejected_review_count, 0) AS rejected_review_count,

    COALESCE(al.pending_alert_count, 0) AS pending_alert_count,
    COALESCE(al.failed_alert_count, 0) AS failed_alert_count,

    CASE
        WHEN COALESCE(op.total_operation_count, 0) > 0
            THEN ROUND((op.completed_operation_count::numeric / op.total_operation_count) * 100, 2)
        ELSE 0
        END AS operation_success_rate_pct
FROM dates d
         LEFT JOIN (
    SELECT 'LIVE' AS data_scope, DATE_TRUNC('day', create_date)::date AS report_date,
        COUNT(*) AS total_evaluation_count,
        COUNT(*) FILTER (WHERE status = 'Completed') AS completed_evaluation_count,
        COUNT(*) FILTER (WHERE status = 'Failed') AS failed_evaluation_count
    FROM reconciliation.evaluation
    GROUP BY DATE_TRUNC('day', create_date)::date
    UNION ALL
    SELECT 'ARCHIVE', DATE_TRUNC('day', create_date)::date,
        COUNT(*), COUNT(*) FILTER (WHERE status = 'Completed'), COUNT(*) FILTER (WHERE status = 'Failed')
    FROM archive.reconciliation_evaluation
    GROUP BY DATE_TRUNC('day', create_date)::date
) ev ON ev.report_date = d.report_date AND ev.data_scope = d.data_scope
         LEFT JOIN (
    SELECT 'LIVE' AS data_scope, DATE_TRUNC('day', create_date)::date AS report_date,
        COUNT(*) AS total_operation_count,
        COUNT(*) FILTER (WHERE status = 'Completed') AS completed_operation_count,
        COUNT(*) FILTER (WHERE status = 'Failed') AS failed_operation_count,
        COUNT(*) FILTER (WHERE status = 'Blocked') AS blocked_operation_count,
        COUNT(*) FILTER (WHERE status = 'Planned') AS planned_operation_count,
        COUNT(*) FILTER (WHERE is_manual = true) AS manual_operation_count
    FROM reconciliation.operation
    GROUP BY DATE_TRUNC('day', create_date)::date
    UNION ALL
    SELECT 'ARCHIVE', DATE_TRUNC('day', create_date)::date,
        COUNT(*), COUNT(*) FILTER (WHERE status = 'Completed'), COUNT(*) FILTER (WHERE status = 'Failed'),
        COUNT(*) FILTER (WHERE status = 'Blocked'), COUNT(*) FILTER (WHERE status = 'Planned'),
        COUNT(*) FILTER (WHERE is_manual = true)
    FROM archive.reconciliation_operation
    GROUP BY DATE_TRUNC('day', create_date)::date
) op ON op.report_date = d.report_date AND op.data_scope = d.data_scope
         LEFT JOIN (
    SELECT 'LIVE' AS data_scope, DATE_TRUNC('day', started_at)::date AS report_date,
        COUNT(*) AS total_execution_count,
        COUNT(*) FILTER (WHERE status = 'Completed') AS completed_execution_count,
        COUNT(*) FILTER (WHERE status = 'Failed') AS failed_execution_count,
        ROUND(AVG(EXTRACT(EPOCH FROM (finished_at - started_at))) FILTER (WHERE finished_at IS NOT NULL), 2) AS avg_execution_duration_seconds
    FROM reconciliation.operation_execution
    GROUP BY DATE_TRUNC('day', started_at)::date
    UNION ALL
    SELECT 'ARCHIVE', DATE_TRUNC('day', started_at)::date,
        COUNT(*), COUNT(*) FILTER (WHERE status = 'Completed'), COUNT(*) FILTER (WHERE status = 'Failed'),
        ROUND(AVG(EXTRACT(EPOCH FROM (finished_at - started_at))) FILTER (WHERE finished_at IS NOT NULL), 2)
    FROM archive.reconciliation_operation_execution
    GROUP BY DATE_TRUNC('day', started_at)::date
) ex ON ex.report_date = d.report_date AND ex.data_scope = d.data_scope
         LEFT JOIN (
    SELECT 'LIVE' AS data_scope, DATE_TRUNC('day', create_date)::date AS report_date,
        COUNT(*) FILTER (WHERE decision = 'Pending') AS pending_review_count,
        COUNT(*) FILTER (WHERE decision = 'Approved') AS approved_review_count,
        COUNT(*) FILTER (WHERE decision = 'Rejected') AS rejected_review_count
    FROM reconciliation.review
    GROUP BY DATE_TRUNC('day', create_date)::date
    UNION ALL
    SELECT 'ARCHIVE', DATE_TRUNC('day', create_date)::date,
        COUNT(*) FILTER (WHERE decision = 'Pending'), COUNT(*) FILTER (WHERE decision = 'Approved'),
        COUNT(*) FILTER (WHERE decision = 'Rejected')
    FROM archive.reconciliation_review
    GROUP BY DATE_TRUNC('day', create_date)::date
) rv ON rv.report_date = d.report_date AND rv.data_scope = d.data_scope
         LEFT JOIN (
    SELECT 'LIVE' AS data_scope, DATE_TRUNC('day', create_date)::date AS report_date,
        COUNT(*) FILTER (WHERE alert_status = 'Pending') AS pending_alert_count,
        COUNT(*) FILTER (WHERE alert_status = 'Failed') AS failed_alert_count
    FROM reconciliation.alert
    GROUP BY DATE_TRUNC('day', create_date)::date
    UNION ALL
    SELECT 'ARCHIVE', DATE_TRUNC('day', create_date)::date,
        COUNT(*) FILTER (WHERE alert_status = 'Pending'), COUNT(*) FILTER (WHERE alert_status = 'Failed')
    FROM archive.reconciliation_alert
    GROUP BY DATE_TRUNC('day', create_date)::date
) al ON al.report_date = d.report_date AND al.data_scope = d.data_scope;

-- -------------------------------------------------------------------------------------
-- B2. Açık mutabakat işleri
-- -------------------------------------------------------------------------------------
CREATE OR REPLACE VIEW reporting.vw_recon_open_items AS
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
    ROUND(EXTRACT(EPOCH FROM (NOW() - op.create_date)) / 3600, 1) AS age_hours
FROM reconciliation.operation op
         JOIN reconciliation.evaluation ev ON ev.id = op.evaluation_id
WHERE op.status IN ('Planned', 'Blocked', 'Executing');

-- -------------------------------------------------------------------------------------
-- B3. Aging dağılımı
-- -------------------------------------------------------------------------------------
CREATE OR REPLACE VIEW reporting.vw_recon_open_item_aging AS
SELECT
    bucket_name,
    COUNT(*) AS item_count,
    COUNT(*) FILTER (WHERE operation_status = 'Planned') AS planned_count,
    COUNT(*) FILTER (WHERE operation_status = 'Blocked') AS blocked_count,
    COUNT(*) FILTER (WHERE operation_status = 'Executing') AS executing_count,
    COUNT(*) FILTER (WHERE is_manual = true) AS manual_count
FROM (
         SELECT
             op.status AS operation_status,
             op.is_manual,
             CASE
                 WHEN EXTRACT(EPOCH FROM (NOW() - op.create_date)) / 3600 < 1 THEN '0-1h'
                 WHEN EXTRACT(EPOCH FROM (NOW() - op.create_date)) / 3600 < 4 THEN '1-4h'
                 WHEN EXTRACT(EPOCH FROM (NOW() - op.create_date)) / 3600 < 24 THEN '4-24h'
                 WHEN EXTRACT(EPOCH FROM (NOW() - op.create_date)) / 3600 < 72 THEN '1-3d'
                 WHEN EXTRACT(EPOCH FROM (NOW() - op.create_date)) / 3600 < 168 THEN '3-7d'
                 ELSE '7d+'
                 END AS bucket_name
         FROM reconciliation.operation op
         WHERE op.status IN ('Planned', 'Blocked', 'Executing')
     ) t
GROUP BY bucket_name;

-- -------------------------------------------------------------------------------------
-- B4. Manuel review kuyruğu (enriched)
-- -------------------------------------------------------------------------------------
CREATE OR REPLACE VIEW reporting.vw_recon_manual_review_queue AS
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
    ROUND(EXTRACT(EPOCH FROM (NOW() - rv.create_date)) / 3600, 1) AS waiting_hours,
    CASE
        WHEN rv.expires_at IS NOT NULL AND rv.expires_at < NOW() THEN 'EXPIRED'
        WHEN rv.expires_at IS NOT NULL AND rv.expires_at < NOW() + INTERVAL '4 hours' THEN 'EXPIRING_SOON'
        WHEN EXTRACT(EPOCH FROM (NOW() - rv.create_date)) / 3600 > 24 THEN 'OVERDUE'
        ELSE 'NORMAL'
    END AS urgency_level,
    CASE
        WHEN le.last_execution_error_message IS NOT NULL THEN le.last_execution_error_message
        WHEN op.last_error IS NOT NULL THEN op.last_error
        ELSE NULL
    END AS effective_error
FROM reconciliation.review rv
    JOIN reconciliation.operation op ON op.id = rv.operation_id
    JOIN reconciliation.evaluation ev ON ev.id = rv.evaluation_id
    JOIN ingestion.file_line fl ON fl.id = rv.file_line_id
    JOIN ingestion.file f ON f.id = fl.file_id
    LEFT JOIN LATERAL (
        SELECT
            ex.id AS last_execution_id,
            ex.attempt_number AS last_attempt_number,
            ex.status AS last_execution_status,
            ex.started_at AS last_execution_started_at,
            ex.finished_at AS last_execution_finished_at,
            ex.result_code AS last_execution_result_code,
            ex.result_message AS last_execution_result_message,
            ex.error_code AS last_execution_error_code,
            ex.error_message AS last_execution_error_message,
            cnt.total_execution_count
        FROM reconciliation.operation_execution ex
        CROSS JOIN (
            SELECT COUNT(*) AS total_execution_count
            FROM reconciliation.operation_execution
            WHERE operation_id = rv.operation_id AND evaluation_id = rv.evaluation_id
        ) cnt
        WHERE ex.operation_id = rv.operation_id AND ex.evaluation_id = rv.evaluation_id
        ORDER BY ex.attempt_number DESC
        LIMIT 1
    ) le ON TRUE
    LEFT JOIN LATERAL (
        SELECT cv.transaction_date, cv.transaction_time,
               cv.original_amount, cv.original_currency, cv.settlement_amount, cv.billing_amount,
               cv.financial_type, cv.txn_effect, cv.response_code, cv.is_successful_txn, cv.rrn, cv.arn
        FROM ingestion.card_visa_detail cv WHERE cv.file_line_id = fl.id AND f.content_type = 'Visa' AND f.file_type = 'Card'
        UNION ALL
        SELECT cm.transaction_date, cm.transaction_time,
               cm.original_amount, cm.original_currency, cm.settlement_amount, cm.billing_amount,
               cm.financial_type, cm.txn_effect, cm.response_code, cm.is_successful_txn, cm.rrn, cm.arn
        FROM ingestion.card_msc_detail cm WHERE cm.file_line_id = fl.id AND f.content_type = 'Msc' AND f.file_type = 'Card'
        UNION ALL
        SELECT cb.transaction_date, cb.transaction_time,
               cb.original_amount, cb.original_currency, cb.settlement_amount, cb.billing_amount,
               cb.financial_type, cb.txn_effect, cb.response_code, cb.is_successful_txn, cb.rrn, cb.arn
        FROM ingestion.card_bkm_detail cb WHERE cb.file_line_id = fl.id AND f.content_type = 'Bkm' AND f.file_type = 'Card'
    ) cd ON TRUE
    LEFT JOIN LATERAL (
        SELECT cv.txn_date, cv.txn_time, cv.io_date,
               cv.source_amount, cv.source_currency, cv.destination_amount,
               cv.txn_type, cv.io_flag, cv.control_stat, cv.rrn, cv.arn
        FROM ingestion.clearing_visa_detail cv WHERE cv.file_line_id = fl.id AND f.content_type = 'Visa' AND f.file_type = 'Clearing'
        UNION ALL
        SELECT cm.txn_date, cm.txn_time, cm.io_date,
               cm.source_amount, cm.source_currency, cm.destination_amount,
               cm.txn_type, cm.io_flag, cm.control_stat, cm.rrn, cm.arn
        FROM ingestion.clearing_msc_detail cm WHERE cm.file_line_id = fl.id AND f.content_type = 'Msc' AND f.file_type = 'Clearing'
        UNION ALL
        SELECT cb.txn_date, cb.txn_time, cb.io_date,
               cb.source_amount, cb.source_currency, cb.destination_amount,
               cb.txn_type, cb.io_flag, cb.control_stat, cb.rrn, cb.arn
        FROM ingestion.clearing_bkm_detail cb WHERE cb.file_line_id = fl.id AND f.content_type = 'Bkm' AND f.file_type = 'Clearing'
    ) cld ON TRUE
WHERE rv.decision = 'Pending';

-- -------------------------------------------------------------------------------------
-- B5. Alert hotspot özeti (LIVE + ARCHIVE)
-- -------------------------------------------------------------------------------------
CREATE OR REPLACE VIEW reporting.vw_recon_alert_summary AS
SELECT
    data_scope,
    severity,
    alert_type,
    alert_status,
    SUM(alert_count) AS alert_count,
    SUM(distinct_group_count) AS distinct_group_count,
    SUM(distinct_operation_count) AS distinct_operation_count,
    MIN(first_alert_at) AS first_alert_at,
    MAX(last_alert_at) AS last_alert_at
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
    FROM reconciliation.alert alert
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
    FROM archive.reconciliation_alert alert
    GROUP BY alert.severity, alert.alert_type, alert.alert_status
) src
GROUP BY data_scope, severity, alert_type, alert_status;

-- =====================================================================================
-- C. RECONCILIATION CONTENT + FINANCIAL
-- =====================================================================================

-- -------------------------------------------------------------------------------------
-- C1. LIVE card içerik özeti
-- -------------------------------------------------------------------------------------
CREATE OR REPLACE VIEW reporting.vw_recon_live_card_content_daily AS
SELECT
    DATE_TRUNC('day', f.create_date)::date AS report_date,
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
FROM ingestion.file_line fl
         JOIN ingestion.file f
              ON f.id = fl.file_id
                  AND f.file_type = 'Card'
         JOIN LATERAL (
    SELECT
        cv.financial_type, cv.txn_effect, cv.txn_source, cv.txn_region,
        cv.terminal_type, cv.channel_code, cv.is_txn_settle, cv.txn_stat,
        cv.response_code, cv.is_successful_txn,
        cv.original_currency, cv.original_amount, cv.settlement_amount, cv.billing_amount
    FROM ingestion.card_visa_detail cv
    WHERE cv.file_line_id = fl.id AND f.content_type = 'Visa'
    UNION ALL
    SELECT
        cm.financial_type, cm.txn_effect, cm.txn_source, cm.txn_region,
        cm.terminal_type, cm.channel_code, cm.is_txn_settle, cm.txn_stat,
        cm.response_code, cm.is_successful_txn,
        cm.original_currency, cm.original_amount, cm.settlement_amount, cm.billing_amount
    FROM ingestion.card_msc_detail cm
    WHERE cm.file_line_id = fl.id AND f.content_type = 'Msc'
    UNION ALL
    SELECT
        cb.financial_type, cb.txn_effect, cb.txn_source, cb.txn_region,
        cb.terminal_type, cb.channel_code, cb.is_txn_settle, cb.txn_stat,
        cb.response_code, cb.is_successful_txn,
        cb.original_currency, cb.original_amount, cb.settlement_amount, cb.billing_amount
    FROM ingestion.card_bkm_detail cb
    WHERE cb.file_line_id = fl.id AND f.content_type = 'Bkm'
        ) d ON TRUE
GROUP BY
    DATE_TRUNC('day', f.create_date)::date,
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

-- -------------------------------------------------------------------------------------
-- C2. LIVE clearing içerik özeti
-- -------------------------------------------------------------------------------------
CREATE OR REPLACE VIEW reporting.vw_recon_live_clearing_content_daily AS
SELECT
    DATE_TRUNC('day', f.create_date)::date AS report_date,
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
FROM ingestion.file_line fl
         JOIN ingestion.file f
              ON f.id = fl.file_id
                  AND f.file_type = 'Clearing'
         JOIN LATERAL (
    SELECT
        cv.txn_type, cv.io_flag, cv.control_stat,
        cv.source_currency, cv.source_amount, cv.destination_amount
    FROM ingestion.clearing_visa_detail cv
    WHERE cv.file_line_id = fl.id AND f.content_type = 'Visa'
    UNION ALL
    SELECT
        cm.txn_type, cm.io_flag, cm.control_stat,
        cm.source_currency, cm.source_amount, cm.destination_amount
    FROM ingestion.clearing_msc_detail cm
    WHERE cm.file_line_id = fl.id AND f.content_type = 'Msc'
    UNION ALL
    SELECT
        cb.txn_type, cb.io_flag, cb.control_stat,
        cb.source_currency, cb.source_amount, cb.destination_amount
    FROM ingestion.clearing_bkm_detail cb
    WHERE cb.file_line_id = fl.id AND f.content_type = 'Bkm'
        ) d ON TRUE
GROUP BY
    DATE_TRUNC('day', f.create_date)::date,
    f.content_type,
    fl.status,
    fl.reconciliation_status,
    d.txn_type,
    d.io_flag,
    d.control_stat,
    d.source_currency;

-- -------------------------------------------------------------------------------------
-- C3. ARCHIVE card içerik özeti
-- -------------------------------------------------------------------------------------
CREATE OR REPLACE VIEW reporting.vw_recon_archive_card_content_daily AS
SELECT
    DATE_TRUNC('day', f.create_date)::date AS report_date,
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
FROM archive.ingestion_file_line fl
         JOIN archive.ingestion_file f
              ON f.id = fl.file_id
                  AND f.file_type = 'Card'
         JOIN LATERAL (
        SELECT
            cv.financial_type, cv.txn_effect, cv.txn_source, cv.txn_region,
            cv.terminal_type, cv.channel_code, cv.is_txn_settle, cv.txn_stat,
            cv.response_code, cv.is_successful_txn,
            cv.original_currency, cv.original_amount, cv.settlement_amount, cv.billing_amount
        FROM archive.ingestion_card_visa_detail cv
        WHERE cv.file_line_id = fl.id AND f.content_type = 'Visa'
        UNION ALL
        SELECT
            cm.financial_type, cm.txn_effect, cm.txn_source, cm.txn_region,
            cm.terminal_type, cm.channel_code, cm.is_txn_settle, cm.txn_stat,
            cm.response_code, cm.is_successful_txn,
            cm.original_currency, cm.original_amount, cm.settlement_amount, cm.billing_amount
        FROM archive.ingestion_card_msc_detail cm
        WHERE cm.file_line_id = fl.id AND f.content_type = 'Msc'
        UNION ALL
        SELECT
            cb.financial_type, cb.txn_effect, cb.txn_source, cb.txn_region,
            cb.terminal_type, cb.channel_code, cb.is_txn_settle, cb.txn_stat,
            cb.response_code, cb.is_successful_txn,
            cb.original_currency, cb.original_amount, cb.settlement_amount, cb.billing_amount
        FROM archive.ingestion_card_bkm_detail cb
        WHERE cb.file_line_id = fl.id AND f.content_type = 'Bkm'
            ) d ON TRUE
GROUP BY
    DATE_TRUNC('day', f.create_date)::date,
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

-- -------------------------------------------------------------------------------------
-- C4. ARCHIVE clearing içerik özeti
-- -------------------------------------------------------------------------------------
CREATE OR REPLACE VIEW reporting.vw_recon_archive_clearing_content_daily AS
SELECT
    DATE_TRUNC('day', f.create_date)::date AS report_date,
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
FROM archive.ingestion_file_line fl
         JOIN archive.ingestion_file f
              ON f.id = fl.file_id
                  AND f.file_type = 'Clearing'
         JOIN LATERAL (
    SELECT
        cv.txn_type, cv.io_flag, cv.control_stat,
        cv.source_currency, cv.source_amount, cv.destination_amount
    FROM archive.ingestion_clearing_visa_detail cv
    WHERE cv.file_line_id = fl.id AND f.content_type = 'Visa'
    UNION ALL
    SELECT
        cm.txn_type, cm.io_flag, cm.control_stat,
        cm.source_currency, cm.source_amount, cm.destination_amount
    FROM archive.ingestion_clearing_msc_detail cm
    WHERE cm.file_line_id = fl.id AND f.content_type = 'Msc'
    UNION ALL
    SELECT
        cb.txn_type, cb.io_flag, cb.control_stat,
        cb.source_currency, cb.source_amount, cb.destination_amount
    FROM archive.ingestion_clearing_bkm_detail cb
    WHERE cb.file_line_id = fl.id AND f.content_type = 'Bkm'
        ) d ON TRUE
GROUP BY
    DATE_TRUNC('day', f.create_date)::date,
    f.content_type,
    fl.status,
    fl.reconciliation_status,
    d.txn_type,
    d.io_flag,
    d.control_stat,
    d.source_currency;

-- -------------------------------------------------------------------------------------
-- C5. Unified günlük reconciliation içerik özeti
-- Bu view başka reporting view'e bağımlı değildir.
-- -------------------------------------------------------------------------------------
CREATE OR REPLACE VIEW reporting.vw_recon_content_daily AS
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
             DATE_TRUNC('day', f.create_date)::date AS report_date,
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
             0::numeric AS total_clearing_source_amount,
             0::numeric AS total_clearing_destination_amount
         FROM ingestion.file_line fl
                  JOIN ingestion.file f ON f.id = fl.file_id AND f.file_type = 'Card'
                  JOIN LATERAL (
             SELECT cv.original_amount, cv.settlement_amount, cv.billing_amount
             FROM ingestion.card_visa_detail cv WHERE cv.file_line_id = fl.id AND f.content_type = 'Visa'
             UNION ALL
             SELECT cm.original_amount, cm.settlement_amount, cm.billing_amount
             FROM ingestion.card_msc_detail cm WHERE cm.file_line_id = fl.id AND f.content_type = 'Msc'
             UNION ALL
             SELECT cb.original_amount, cb.settlement_amount, cb.billing_amount
             FROM ingestion.card_bkm_detail cb WHERE cb.file_line_id = fl.id AND f.content_type = 'Bkm'
                 ) d ON TRUE
         GROUP BY DATE_TRUNC('day', f.create_date)::date, f.content_type, fl.status, fl.reconciliation_status

         UNION ALL

         -- live clearing
         SELECT
             DATE_TRUNC('day', f.create_date)::date,
             'LIVE',
             f.content_type,
             'Clearing',
             fl.status,
             fl.reconciliation_status,
             COUNT(*),
             COUNT(DISTINCT fl.file_id),
             SUM(CASE WHEN fl.matched_clearing_line_id IS NOT NULL THEN 1 ELSE 0 END),
             SUM(CASE WHEN fl.matched_clearing_line_id IS NULL THEN 1 ELSE 0 END),
             0::numeric,
             0::numeric,
             0::numeric,
             SUM(d.source_amount),
             SUM(d.destination_amount)
         FROM ingestion.file_line fl
             JOIN ingestion.file f ON f.id = fl.file_id AND f.file_type = 'Clearing'
             JOIN LATERAL (
             SELECT cv.source_amount, cv.destination_amount
             FROM ingestion.clearing_visa_detail cv WHERE cv.file_line_id = fl.id AND f.content_type = 'Visa'
             UNION ALL
             SELECT cm.source_amount, cm.destination_amount
             FROM ingestion.clearing_msc_detail cm WHERE cm.file_line_id = fl.id AND f.content_type = 'Msc'
             UNION ALL
             SELECT cb.source_amount, cb.destination_amount
             FROM ingestion.clearing_bkm_detail cb WHERE cb.file_line_id = fl.id AND f.content_type = 'Bkm'
             ) d ON TRUE
         GROUP BY DATE_TRUNC('day', f.create_date)::date, f.content_type, fl.status, fl.reconciliation_status

         UNION ALL

         -- archive card
         SELECT
             DATE_TRUNC('day', f.create_date)::date,
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
             0::numeric,
             0::numeric
         FROM archive.ingestion_file_line fl
             JOIN archive.ingestion_file f ON f.id = fl.file_id AND f.file_type = 'Card'
             JOIN LATERAL (
             SELECT cv.original_amount, cv.settlement_amount, cv.billing_amount
             FROM archive.ingestion_card_visa_detail cv WHERE cv.file_line_id = fl.id AND f.content_type = 'Visa'
             UNION ALL
             SELECT cm.original_amount, cm.settlement_amount, cm.billing_amount
             FROM archive.ingestion_card_msc_detail cm WHERE cm.file_line_id = fl.id AND f.content_type = 'Msc'
             UNION ALL
             SELECT cb.original_amount, cb.settlement_amount, cb.billing_amount
             FROM archive.ingestion_card_bkm_detail cb WHERE cb.file_line_id = fl.id AND f.content_type = 'Bkm'
             ) d ON TRUE
         GROUP BY DATE_TRUNC('day', f.create_date)::date, f.content_type, fl.status, fl.reconciliation_status

         UNION ALL

         -- archive clearing
         SELECT
             DATE_TRUNC('day', f.create_date)::date,
             'ARCHIVE',
             f.content_type,
             'Clearing',
             fl.status,
             fl.reconciliation_status,
             COUNT(*),
             COUNT(DISTINCT fl.file_id),
             SUM(CASE WHEN fl.matched_clearing_line_id IS NOT NULL THEN 1 ELSE 0 END),
             SUM(CASE WHEN fl.matched_clearing_line_id IS NULL THEN 1 ELSE 0 END),
             0::numeric,
             0::numeric,
             0::numeric,
             SUM(d.source_amount),
             SUM(d.destination_amount)
         FROM archive.ingestion_file_line fl
             JOIN archive.ingestion_file f ON f.id = fl.file_id AND f.file_type = 'Clearing'
             JOIN LATERAL (
             SELECT cv.source_amount, cv.destination_amount
             FROM archive.ingestion_clearing_visa_detail cv WHERE cv.file_line_id = fl.id AND f.content_type = 'Visa'
             UNION ALL
             SELECT cm.source_amount, cm.destination_amount
             FROM archive.ingestion_clearing_msc_detail cm WHERE cm.file_line_id = fl.id AND f.content_type = 'Msc'
             UNION ALL
             SELECT cb.source_amount, cb.destination_amount
             FROM archive.ingestion_clearing_bkm_detail cb WHERE cb.file_line_id = fl.id AND f.content_type = 'Bkm'
             ) d ON TRUE
         GROUP BY DATE_TRUNC('day', f.create_date)::date, f.content_type, fl.status, fl.reconciliation_status
     ) src
GROUP BY
    src.report_date,
    src.data_scope,
    src.network,
    src.side,
    src.line_status,
    src.reconciliation_status;

-- -------------------------------------------------------------------------------------
-- C6. Clearing control_stat analizi
-- -------------------------------------------------------------------------------------
CREATE OR REPLACE VIEW reporting.vw_recon_clearing_controlstat_analysis AS
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
            THEN ROUND((SUM(x.unmatched_count)::numeric / SUM(x.transaction_count)) * 100, 2)
        ELSE 0
        END AS unmatched_rate_pct
FROM (
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
         FROM ingestion.file_line fl
                  JOIN ingestion.file f ON f.id = fl.file_id AND f.file_type = 'Clearing'
                  JOIN LATERAL (
             SELECT cv.control_stat, cv.io_flag, cv.source_amount
             FROM ingestion.clearing_visa_detail cv WHERE cv.file_line_id = fl.id AND f.content_type = 'Visa'
             UNION ALL
             SELECT cm.control_stat, cm.io_flag, cm.source_amount
             FROM ingestion.clearing_msc_detail cm WHERE cm.file_line_id = fl.id AND f.content_type = 'Msc'
             UNION ALL
             SELECT cb.control_stat, cb.io_flag, cb.source_amount
             FROM ingestion.clearing_bkm_detail cb WHERE cb.file_line_id = fl.id AND f.content_type = 'Bkm'
                 ) d ON TRUE
         GROUP BY f.content_type, fl.status, d.control_stat, d.io_flag

         UNION ALL

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
         FROM archive.ingestion_file_line fl
                  JOIN archive.ingestion_file f ON f.id = fl.file_id AND f.file_type = 'Clearing'
                  JOIN LATERAL (
             SELECT cv.control_stat, cv.io_flag, cv.source_amount
             FROM archive.ingestion_clearing_visa_detail cv WHERE cv.file_line_id = fl.id AND f.content_type = 'Visa'
             UNION ALL
             SELECT cm.control_stat, cm.io_flag, cm.source_amount
             FROM archive.ingestion_clearing_msc_detail cm WHERE cm.file_line_id = fl.id AND f.content_type = 'Msc'
             UNION ALL
             SELECT cb.control_stat, cb.io_flag, cb.source_amount
             FROM archive.ingestion_clearing_bkm_detail cb WHERE cb.file_line_id = fl.id AND f.content_type = 'Bkm'
                 ) d ON TRUE
         GROUP BY f.content_type, fl.status, d.control_stat, d.io_flag
     ) x
GROUP BY
    x.data_scope,
    x.network,
    x.line_status,
    x.control_stat,
    x.io_flag;

-- -------------------------------------------------------------------------------------
-- C7. Finansal özet
-- -------------------------------------------------------------------------------------
CREATE OR REPLACE VIEW reporting.vw_recon_financial_summary AS
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
         FROM ingestion.file_line fl
                  JOIN ingestion.file f ON f.id = fl.file_id AND f.file_type = 'Card'
                  JOIN LATERAL (
             SELECT cv.financial_type, cv.txn_effect, cv.is_txn_settle, cv.original_currency, cv.original_amount, cv.settlement_amount, cv.billing_amount
             FROM ingestion.card_visa_detail cv WHERE cv.file_line_id = fl.id AND f.content_type = 'Visa'
             UNION ALL
             SELECT cm.financial_type, cm.txn_effect, cm.is_txn_settle, cm.original_currency, cm.original_amount, cm.settlement_amount, cm.billing_amount
             FROM ingestion.card_msc_detail cm WHERE cm.file_line_id = fl.id AND f.content_type = 'Msc'
             UNION ALL
             SELECT cb.financial_type, cb.txn_effect, cb.is_txn_settle, cb.original_currency, cb.original_amount, cb.settlement_amount, cb.billing_amount
             FROM ingestion.card_bkm_detail cb WHERE cb.file_line_id = fl.id AND f.content_type = 'Bkm'
                 ) d ON TRUE
         GROUP BY f.content_type, fl.status, d.financial_type, d.txn_effect, d.original_currency

         UNION ALL

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
         FROM archive.ingestion_file_line fl
                  JOIN archive.ingestion_file f ON f.id = fl.file_id AND f.file_type = 'Card'
                  JOIN LATERAL (
             SELECT cv.financial_type, cv.txn_effect, cv.is_txn_settle, cv.original_currency, cv.original_amount, cv.settlement_amount, cv.billing_amount
             FROM archive.ingestion_card_visa_detail cv WHERE cv.file_line_id = fl.id AND f.content_type = 'Visa'
             UNION ALL
             SELECT cm.financial_type, cm.txn_effect, cm.is_txn_settle, cm.original_currency, cm.original_amount, cm.settlement_amount, cm.billing_amount
             FROM archive.ingestion_card_msc_detail cm WHERE cm.file_line_id = fl.id AND f.content_type = 'Msc'
             UNION ALL
             SELECT cb.financial_type, cb.txn_effect, cb.is_txn_settle, cb.original_currency, cb.original_amount, cb.settlement_amount, cb.billing_amount
             FROM archive.ingestion_card_bkm_detail cb WHERE cb.file_line_id = fl.id AND f.content_type = 'Bkm'
                 ) d ON TRUE
         GROUP BY f.content_type, fl.status, d.financial_type, d.txn_effect, d.original_currency
     ) x
GROUP BY
    x.data_scope,
    x.network,
    x.line_status,
    x.financial_type,
    x.txn_effect,
    x.original_currency;

-- -------------------------------------------------------------------------------------
-- C8. Response / txn status analizi
-- -------------------------------------------------------------------------------------
CREATE OR REPLACE VIEW reporting.vw_recon_response_status_analysis AS
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
         FROM ingestion.file_line fl
                  JOIN ingestion.file f ON f.id = fl.file_id AND f.file_type = 'Card'
                  JOIN LATERAL (
             SELECT cv.response_code, cv.txn_stat, cv.is_successful_txn, cv.is_txn_settle, cv.original_amount
             FROM ingestion.card_visa_detail cv WHERE cv.file_line_id = fl.id AND f.content_type = 'Visa'
             UNION ALL
             SELECT cm.response_code, cm.txn_stat, cm.is_successful_txn, cm.is_txn_settle, cm.original_amount
             FROM ingestion.card_msc_detail cm WHERE cm.file_line_id = fl.id AND f.content_type = 'Msc'
             UNION ALL
             SELECT cb.response_code, cb.txn_stat, cb.is_successful_txn, cb.is_txn_settle, cb.original_amount
             FROM ingestion.card_bkm_detail cb WHERE cb.file_line_id = fl.id AND f.content_type = 'Bkm'
                 ) d ON TRUE
         GROUP BY f.content_type, fl.status, d.response_code, d.txn_stat, d.is_successful_txn, d.is_txn_settle, fl.reconciliation_status

         UNION ALL

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
         FROM archive.ingestion_file_line fl
                  JOIN archive.ingestion_file f ON f.id = fl.file_id AND f.file_type = 'Card'
                  JOIN LATERAL (
             SELECT cv.response_code, cv.txn_stat, cv.is_successful_txn, cv.is_txn_settle, cv.original_amount
             FROM archive.ingestion_card_visa_detail cv WHERE cv.file_line_id = fl.id AND f.content_type = 'Visa'
             UNION ALL
             SELECT cm.response_code, cm.txn_stat, cm.is_successful_txn, cm.is_txn_settle, cm.original_amount
             FROM archive.ingestion_card_msc_detail cm WHERE cm.file_line_id = fl.id AND f.content_type = 'Msc'
             UNION ALL
             SELECT cb.response_code, cb.txn_stat, cb.is_successful_txn, cb.is_txn_settle, cb.original_amount
             FROM archive.ingestion_card_bkm_detail cb WHERE cb.file_line_id = fl.id AND f.content_type = 'Bkm'
                 ) d ON TRUE
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

-- =====================================================================================
-- D. ARCHIVE
-- =====================================================================================

-- -------------------------------------------------------------------------------------
-- D1. Archive run overview
-- -------------------------------------------------------------------------------------
CREATE OR REPLACE VIEW reporting.vw_archive_run_overview AS
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
    EXTRACT(EPOCH FROM (COALESCE(log.update_date, log.create_date) - log.create_date))::bigint AS archive_duration_seconds
FROM archive.archive_log log
         LEFT JOIN ingestion.file lf ON lf.id = log.ingestion_file_id
         LEFT JOIN archive.ingestion_file af ON af.id = log.ingestion_file_id;

-- -------------------------------------------------------------------------------------
-- D2. Archive uygunluk görünümü
-- -------------------------------------------------------------------------------------
CREATE OR REPLACE VIEW reporting.vw_archive_eligibility AS
SELECT
    f.id AS file_id,
    f.file_name,
    f.file_type,
    f.content_type,
    f.status AS file_status,
    f.is_archived,
    f.create_date AS file_created_at,
    ROUND(EXTRACT(EPOCH FROM (NOW() - f.create_date)) / 86400, 1) AS age_days,
    COALESCE(x.total_recon_line_count, 0) AS total_recon_line_count,
    COALESCE(x.recon_success_line_count, 0) AS recon_success_line_count,
    COALESCE(x.recon_open_line_count, 0) AS recon_open_line_count,
    CASE
        WHEN af.id IS NOT NULL THEN 'ALREADY_ARCHIVED'
        WHEN f.status <> 'Success' THEN 'FILE_NOT_COMPLETE'
        WHEN COALESCE(x.recon_open_line_count, 0) > 0 THEN 'RECON_PENDING'
        ELSE 'ELIGIBLE'
        END AS archive_eligibility_status
FROM ingestion.file f
         LEFT JOIN archive.ingestion_file af ON af.id = f.id
         LEFT JOIN LATERAL (
    SELECT
        COUNT(*) FILTER (WHERE line.reconciliation_status IS NOT NULL) AS total_recon_line_count,
        COUNT(*) FILTER (WHERE line.reconciliation_status = 'Success') AS recon_success_line_count,
        COUNT(*) FILTER (WHERE line.reconciliation_status IN ('Ready', 'Processing', 'Failed')) AS recon_open_line_count
    FROM ingestion.file_line line
    WHERE line.file_id = f.id
        ) x ON TRUE;

-- -------------------------------------------------------------------------------------
-- D3. Archive backlog trend
-- -------------------------------------------------------------------------------------
CREATE OR REPLACE VIEW reporting.vw_archive_backlog_trend AS
SELECT
    DATE_TRUNC('day', log.create_date)::date AS report_date,
    COUNT(*) AS archive_run_count,
    COUNT(*) FILTER (WHERE log.status = 'Success') AS success_run_count,
    COUNT(*) FILTER (WHERE log.status = 'Failed') AS failed_run_count,
    COUNT(*) FILTER (WHERE log.status NOT IN ('Success', 'Failed')) AS other_run_count
FROM archive.archive_log log
GROUP BY DATE_TRUNC('day', log.create_date)::date;

-- -------------------------------------------------------------------------------------
-- D4. Retention snapshot
-- -------------------------------------------------------------------------------------
CREATE OR REPLACE VIEW reporting.vw_archive_retention_snapshot AS
SELECT
    (SELECT COUNT(*) FROM ingestion.file f WHERE NOT EXISTS (SELECT 1 FROM archive.ingestion_file af WHERE af.id = f.id)) AS active_file_count,
    (SELECT COUNT(*) FROM archive.ingestion_file) AS archived_marked_file_count,
    (SELECT COUNT(*) FROM archive.ingestion_file) AS archive_table_file_count,
    (SELECT COUNT(*) FROM archive.ingestion_file_line) AS archive_table_file_line_count,
    (SELECT COUNT(*) FROM archive.reconciliation_evaluation) AS archive_table_evaluation_count,
    (SELECT COUNT(*) FROM archive.reconciliation_operation) AS archive_table_operation_count,
    (SELECT COUNT(*) FROM archive.reconciliation_review) AS archive_table_review_count,
    (SELECT COUNT(*) FROM archive.reconciliation_alert) AS archive_table_alert_count,
    (SELECT COUNT(*) FROM archive.reconciliation_operation_execution) AS archive_table_execution_count,
    (SELECT MIN(f.create_date) FROM ingestion.file f WHERE NOT EXISTS (SELECT 1 FROM archive.ingestion_file af WHERE af.id = f.id) AND f.status = 'Success') AS oldest_unarchived_file_date;

-- =====================================================================================
-- E. ADVANCED RECONCILIATION REPORTS
-- =====================================================================================

-- -------------------------------------------------------------------------------------
-- E1. Dosya bazlı mutabakat özeti (LIVE + ARCHIVE)
-- -------------------------------------------------------------------------------------
CREATE OR REPLACE VIEW reporting.vw_file_recon_summary AS
SELECT * FROM (
    SELECT
        f.id AS file_id, f.file_name, f.file_type, f.content_type, f.status AS file_status,
        f.create_date AS file_created_at, 'LIVE' AS data_scope,
        COALESCE(fl.total_line_count, 0) AS total_line_count,
        COALESCE(fl.matched_line_count, 0) AS matched_line_count,
        COALESCE(fl.unmatched_line_count, 0) AS unmatched_line_count,
        CASE WHEN COALESCE(fl.total_line_count, 0) > 0
            THEN ROUND((fl.matched_line_count::numeric / fl.total_line_count) * 100, 2) ELSE 0 END AS match_rate_pct,
        COALESCE(fl.total_original_amount, 0) AS total_original_amount,
        COALESCE(fl.matched_amount, 0) AS matched_amount,
        COALESCE(fl.unmatched_amount, 0) AS unmatched_amount,
        COALESCE(fl.total_settlement_amount, 0) AS total_settlement_amount,
        COALESCE(fl.recon_ready_count, 0) AS recon_ready_count,
        COALESCE(fl.recon_success_count, 0) AS recon_success_count,
        COALESCE(fl.recon_failed_count, 0) AS recon_failed_count,
        COALESCE(fl.recon_not_applicable_count, 0) AS recon_not_applicable_count
    FROM ingestion.file f
    LEFT JOIN LATERAL (
        SELECT
            COUNT(*) AS total_line_count,
            SUM(CASE WHEN line.matched_clearing_line_id IS NOT NULL THEN 1 ELSE 0 END) AS matched_line_count,
            SUM(CASE WHEN line.matched_clearing_line_id IS NULL THEN 1 ELSE 0 END) AS unmatched_line_count,
            SUM(COALESCE(d.amt, 0)) AS total_original_amount,
            SUM(CASE WHEN line.matched_clearing_line_id IS NOT NULL THEN COALESCE(d.amt, 0) ELSE 0 END) AS matched_amount,
            SUM(CASE WHEN line.matched_clearing_line_id IS NULL THEN COALESCE(d.amt, 0) ELSE 0 END) AS unmatched_amount,
            SUM(COALESCE(d.sett, 0)) AS total_settlement_amount,
            COUNT(*) FILTER (WHERE line.reconciliation_status = 'Ready') AS recon_ready_count,
            COUNT(*) FILTER (WHERE line.reconciliation_status = 'Success') AS recon_success_count,
            COUNT(*) FILTER (WHERE line.reconciliation_status = 'Failed') AS recon_failed_count,
            COUNT(*) FILTER (WHERE line.reconciliation_status IS NULL) AS recon_not_applicable_count
        FROM ingestion.file_line line
        LEFT JOIN LATERAL (
            SELECT cv.original_amount AS amt, cv.settlement_amount AS sett FROM ingestion.card_visa_detail cv WHERE cv.file_line_id = line.id AND f.content_type = 'Visa' AND f.file_type = 'Card'
            UNION ALL SELECT cm.original_amount, cm.settlement_amount FROM ingestion.card_msc_detail cm WHERE cm.file_line_id = line.id AND f.content_type = 'Msc' AND f.file_type = 'Card'
            UNION ALL SELECT cb.original_amount, cb.settlement_amount FROM ingestion.card_bkm_detail cb WHERE cb.file_line_id = line.id AND f.content_type = 'Bkm' AND f.file_type = 'Card'
            UNION ALL SELECT cv.source_amount, 0 FROM ingestion.clearing_visa_detail cv WHERE cv.file_line_id = line.id AND f.content_type = 'Visa' AND f.file_type = 'Clearing'
            UNION ALL SELECT cm.source_amount, 0 FROM ingestion.clearing_msc_detail cm WHERE cm.file_line_id = line.id AND f.content_type = 'Msc' AND f.file_type = 'Clearing'
            UNION ALL SELECT cb.source_amount, 0 FROM ingestion.clearing_bkm_detail cb WHERE cb.file_line_id = line.id AND f.content_type = 'Bkm' AND f.file_type = 'Clearing'
        ) d ON TRUE
        WHERE line.file_id = f.id
    ) fl ON TRUE

    UNION ALL

    SELECT
        f.id, f.file_name, f.file_type, f.content_type, f.status,
        f.create_date, 'ARCHIVE',
        COALESCE(fl.total_line_count, 0), COALESCE(fl.matched_line_count, 0), COALESCE(fl.unmatched_line_count, 0),
        CASE WHEN COALESCE(fl.total_line_count, 0) > 0 THEN ROUND((fl.matched_line_count::numeric / fl.total_line_count) * 100, 2) ELSE 0 END,
        COALESCE(fl.total_original_amount, 0), COALESCE(fl.matched_amount, 0), COALESCE(fl.unmatched_amount, 0),
        COALESCE(fl.total_settlement_amount, 0),
        COALESCE(fl.recon_ready_count, 0), COALESCE(fl.recon_success_count, 0), COALESCE(fl.recon_failed_count, 0), COALESCE(fl.recon_not_applicable_count, 0)
    FROM archive.ingestion_file f
    LEFT JOIN LATERAL (
        SELECT
            COUNT(*) AS total_line_count,
            SUM(CASE WHEN line.matched_clearing_line_id IS NOT NULL THEN 1 ELSE 0 END) AS matched_line_count,
            SUM(CASE WHEN line.matched_clearing_line_id IS NULL THEN 1 ELSE 0 END) AS unmatched_line_count,
            SUM(COALESCE(d.amt, 0)) AS total_original_amount,
            SUM(CASE WHEN line.matched_clearing_line_id IS NOT NULL THEN COALESCE(d.amt, 0) ELSE 0 END) AS matched_amount,
            SUM(CASE WHEN line.matched_clearing_line_id IS NULL THEN COALESCE(d.amt, 0) ELSE 0 END) AS unmatched_amount,
            SUM(COALESCE(d.sett, 0)) AS total_settlement_amount,
            COUNT(*) FILTER (WHERE line.reconciliation_status = 'Ready') AS recon_ready_count,
            COUNT(*) FILTER (WHERE line.reconciliation_status = 'Success') AS recon_success_count,
            COUNT(*) FILTER (WHERE line.reconciliation_status = 'Failed') AS recon_failed_count,
            COUNT(*) FILTER (WHERE line.reconciliation_status IS NULL) AS recon_not_applicable_count
        FROM archive.ingestion_file_line line
        LEFT JOIN LATERAL (
            SELECT cv.original_amount AS amt, cv.settlement_amount AS sett FROM archive.ingestion_card_visa_detail cv WHERE cv.file_line_id = line.id AND f.content_type = 'Visa' AND f.file_type = 'Card'
            UNION ALL SELECT cm.original_amount, cm.settlement_amount FROM archive.ingestion_card_msc_detail cm WHERE cm.file_line_id = line.id AND f.content_type = 'Msc' AND f.file_type = 'Card'
            UNION ALL SELECT cb.original_amount, cb.settlement_amount FROM archive.ingestion_card_bkm_detail cb WHERE cb.file_line_id = line.id AND f.content_type = 'Bkm' AND f.file_type = 'Card'
            UNION ALL SELECT cv.source_amount, 0 FROM archive.ingestion_clearing_visa_detail cv WHERE cv.file_line_id = line.id AND f.content_type = 'Visa' AND f.file_type = 'Clearing'
            UNION ALL SELECT cm.source_amount, 0 FROM archive.ingestion_clearing_msc_detail cm WHERE cm.file_line_id = line.id AND f.content_type = 'Msc' AND f.file_type = 'Clearing'
            UNION ALL SELECT cb.source_amount, 0 FROM archive.ingestion_clearing_bkm_detail cb WHERE cb.file_line_id = line.id AND f.content_type = 'Bkm' AND f.file_type = 'Clearing'
        ) d ON TRUE
        WHERE line.file_id = f.id
    ) fl ON TRUE
) combined;

-- -------------------------------------------------------------------------------------
-- E2. Günlük eşleşme oranı trendi (LIVE + ARCHIVE)
-- -------------------------------------------------------------------------------------
CREATE OR REPLACE VIEW reporting.vw_recon_match_rate_trend AS
SELECT
    src.report_date, src.data_scope, src.network, src.side,
    SUM(src.total_line_count) AS total_line_count,
    SUM(src.matched_count) AS matched_count,
    SUM(src.unmatched_count) AS unmatched_count,
    CASE WHEN SUM(src.total_line_count) > 0 THEN ROUND((SUM(src.matched_count)::numeric / SUM(src.total_line_count)) * 100, 2) ELSE 0 END AS match_rate_pct,
    SUM(src.total_amount) AS total_amount,
    SUM(src.matched_amount) AS matched_amount,
    SUM(src.unmatched_amount) AS unmatched_amount
FROM (
    SELECT DATE_TRUNC('day', f.create_date)::date AS report_date, 'LIVE' AS data_scope, f.content_type AS network, 'Card' AS side,
        COUNT(*) AS total_line_count,
        SUM(CASE WHEN fl.matched_clearing_line_id IS NOT NULL THEN 1 ELSE 0 END) AS matched_count,
        SUM(CASE WHEN fl.matched_clearing_line_id IS NULL THEN 1 ELSE 0 END) AS unmatched_count,
        SUM(d.original_amount) AS total_amount,
        SUM(CASE WHEN fl.matched_clearing_line_id IS NOT NULL THEN d.original_amount ELSE 0 END) AS matched_amount,
        SUM(CASE WHEN fl.matched_clearing_line_id IS NULL THEN d.original_amount ELSE 0 END) AS unmatched_amount
    FROM ingestion.file_line fl JOIN ingestion.file f ON f.id = fl.file_id AND f.file_type = 'Card'
    JOIN LATERAL (
        SELECT cv.original_amount FROM ingestion.card_visa_detail cv WHERE cv.file_line_id = fl.id AND f.content_type = 'Visa'
        UNION ALL SELECT cm.original_amount FROM ingestion.card_msc_detail cm WHERE cm.file_line_id = fl.id AND f.content_type = 'Msc'
        UNION ALL SELECT cb.original_amount FROM ingestion.card_bkm_detail cb WHERE cb.file_line_id = fl.id AND f.content_type = 'Bkm'
    ) d ON TRUE
    GROUP BY DATE_TRUNC('day', f.create_date)::date, f.content_type
    UNION ALL
    SELECT DATE_TRUNC('day', f.create_date)::date, 'LIVE', f.content_type, 'Clearing',
        COUNT(*), SUM(CASE WHEN fl.matched_clearing_line_id IS NOT NULL THEN 1 ELSE 0 END), SUM(CASE WHEN fl.matched_clearing_line_id IS NULL THEN 1 ELSE 0 END),
        SUM(d.source_amount), SUM(CASE WHEN fl.matched_clearing_line_id IS NOT NULL THEN d.source_amount ELSE 0 END), SUM(CASE WHEN fl.matched_clearing_line_id IS NULL THEN d.source_amount ELSE 0 END)
    FROM ingestion.file_line fl JOIN ingestion.file f ON f.id = fl.file_id AND f.file_type = 'Clearing'
    JOIN LATERAL (
        SELECT cv.source_amount FROM ingestion.clearing_visa_detail cv WHERE cv.file_line_id = fl.id AND f.content_type = 'Visa'
        UNION ALL SELECT cm.source_amount FROM ingestion.clearing_msc_detail cm WHERE cm.file_line_id = fl.id AND f.content_type = 'Msc'
        UNION ALL SELECT cb.source_amount FROM ingestion.clearing_bkm_detail cb WHERE cb.file_line_id = fl.id AND f.content_type = 'Bkm'
    ) d ON TRUE
    GROUP BY DATE_TRUNC('day', f.create_date)::date, f.content_type
    UNION ALL
    SELECT DATE_TRUNC('day', f.create_date)::date, 'ARCHIVE', f.content_type, 'Card',
        COUNT(*), SUM(CASE WHEN fl.matched_clearing_line_id IS NOT NULL THEN 1 ELSE 0 END), SUM(CASE WHEN fl.matched_clearing_line_id IS NULL THEN 1 ELSE 0 END),
        SUM(d.original_amount), SUM(CASE WHEN fl.matched_clearing_line_id IS NOT NULL THEN d.original_amount ELSE 0 END), SUM(CASE WHEN fl.matched_clearing_line_id IS NULL THEN d.original_amount ELSE 0 END)
    FROM archive.ingestion_file_line fl JOIN archive.ingestion_file f ON f.id = fl.file_id AND f.file_type = 'Card'
    JOIN LATERAL (
        SELECT cv.original_amount FROM archive.ingestion_card_visa_detail cv WHERE cv.file_line_id = fl.id AND f.content_type = 'Visa'
        UNION ALL SELECT cm.original_amount FROM archive.ingestion_card_msc_detail cm WHERE cm.file_line_id = fl.id AND f.content_type = 'Msc'
        UNION ALL SELECT cb.original_amount FROM archive.ingestion_card_bkm_detail cb WHERE cb.file_line_id = fl.id AND f.content_type = 'Bkm'
    ) d ON TRUE
    GROUP BY DATE_TRUNC('day', f.create_date)::date, f.content_type
    UNION ALL
    SELECT DATE_TRUNC('day', f.create_date)::date, 'ARCHIVE', f.content_type, 'Clearing',
        COUNT(*), SUM(CASE WHEN fl.matched_clearing_line_id IS NOT NULL THEN 1 ELSE 0 END), SUM(CASE WHEN fl.matched_clearing_line_id IS NULL THEN 1 ELSE 0 END),
        SUM(d.source_amount), SUM(CASE WHEN fl.matched_clearing_line_id IS NOT NULL THEN d.source_amount ELSE 0 END), SUM(CASE WHEN fl.matched_clearing_line_id IS NULL THEN d.source_amount ELSE 0 END)
    FROM archive.ingestion_file_line fl JOIN archive.ingestion_file f ON f.id = fl.file_id AND f.file_type = 'Clearing'
    JOIN LATERAL (
        SELECT cv.source_amount FROM archive.ingestion_clearing_visa_detail cv WHERE cv.file_line_id = fl.id AND f.content_type = 'Visa'
        UNION ALL SELECT cm.source_amount FROM archive.ingestion_clearing_msc_detail cm WHERE cm.file_line_id = fl.id AND f.content_type = 'Msc'
        UNION ALL SELECT cb.source_amount FROM archive.ingestion_clearing_bkm_detail cb WHERE cb.file_line_id = fl.id AND f.content_type = 'Bkm'
    ) d ON TRUE
    GROUP BY DATE_TRUNC('day', f.create_date)::date, f.content_type
) src
GROUP BY src.report_date, src.data_scope, src.network, src.side;

-- -------------------------------------------------------------------------------------
-- E3. Card vs Clearing fark analizi (LIVE + ARCHIVE)
-- -------------------------------------------------------------------------------------
CREATE OR REPLACE VIEW reporting.vw_recon_gap_analysis AS
SELECT
    COALESCE(c.report_date, cl.report_date) AS report_date,
    COALESCE(c.data_scope, cl.data_scope) AS data_scope,
    COALESCE(c.network, cl.network) AS network,
    COALESCE(c.card_line_count, 0) AS card_line_count,
    COALESCE(cl.clearing_line_count, 0) AS clearing_line_count,
    COALESCE(c.card_line_count, 0) - COALESCE(cl.clearing_line_count, 0) AS line_count_difference,
    COALESCE(c.card_matched_count, 0) AS card_matched_count,
    COALESCE(cl.clearing_matched_count, 0) AS clearing_matched_count,
    COALESCE(c.card_total_amount, 0) AS card_total_amount,
    COALESCE(cl.clearing_total_amount, 0) AS clearing_total_amount,
    COALESCE(c.card_total_amount, 0) - COALESCE(cl.clearing_total_amount, 0) AS amount_difference,
    CASE WHEN COALESCE(c.card_line_count, 0) > 0 THEN ROUND((c.card_matched_count::numeric / c.card_line_count) * 100, 2) ELSE 0 END AS card_match_rate_pct,
    CASE WHEN COALESCE(cl.clearing_line_count, 0) > 0 THEN ROUND((cl.clearing_matched_count::numeric / cl.clearing_line_count) * 100, 2) ELSE 0 END AS clearing_match_rate_pct
FROM (
    SELECT DATE_TRUNC('day', f.create_date)::date AS report_date, src.ds AS data_scope, f.content_type AS network,
        COUNT(*) AS card_line_count,
        SUM(CASE WHEN fl.matched_clearing_line_id IS NOT NULL THEN 1 ELSE 0 END) AS card_matched_count,
        SUM(d.original_amount) AS card_total_amount
    FROM (SELECT 'LIVE' AS ds UNION ALL SELECT 'ARCHIVE') src
    CROSS JOIN LATERAL (
        SELECT fl2.id, fl2.file_id, fl2.matched_clearing_line_id FROM ingestion.file_line fl2 WHERE src.ds = 'LIVE'
        UNION ALL SELECT fl2.id, fl2.file_id, fl2.matched_clearing_line_id FROM archive.ingestion_file_line fl2 WHERE src.ds = 'ARCHIVE'
    ) fl
    CROSS JOIN LATERAL (
        SELECT f2.create_date, f2.content_type FROM ingestion.file f2 WHERE src.ds = 'LIVE' AND f2.id = fl.file_id AND f2.file_type = 'Card'
        UNION ALL SELECT f2.create_date, f2.content_type FROM archive.ingestion_file f2 WHERE src.ds = 'ARCHIVE' AND f2.id = fl.file_id AND f2.file_type = 'Card'
    ) f
    JOIN LATERAL (
        SELECT cv.original_amount FROM ingestion.card_visa_detail cv WHERE cv.file_line_id = fl.id AND f.content_type = 'Visa' AND src.ds = 'LIVE'
        UNION ALL SELECT cm.original_amount FROM ingestion.card_msc_detail cm WHERE cm.file_line_id = fl.id AND f.content_type = 'Msc' AND src.ds = 'LIVE'
        UNION ALL SELECT cb.original_amount FROM ingestion.card_bkm_detail cb WHERE cb.file_line_id = fl.id AND f.content_type = 'Bkm' AND src.ds = 'LIVE'
        UNION ALL SELECT cv.original_amount FROM archive.ingestion_card_visa_detail cv WHERE cv.file_line_id = fl.id AND f.content_type = 'Visa' AND src.ds = 'ARCHIVE'
        UNION ALL SELECT cm.original_amount FROM archive.ingestion_card_msc_detail cm WHERE cm.file_line_id = fl.id AND f.content_type = 'Msc' AND src.ds = 'ARCHIVE'
        UNION ALL SELECT cb.original_amount FROM archive.ingestion_card_bkm_detail cb WHERE cb.file_line_id = fl.id AND f.content_type = 'Bkm' AND src.ds = 'ARCHIVE'
    ) d ON TRUE
    GROUP BY DATE_TRUNC('day', f.create_date)::date, src.ds, f.content_type
) c
FULL OUTER JOIN (
    SELECT DATE_TRUNC('day', f.create_date)::date AS report_date, src.ds AS data_scope, f.content_type AS network,
        COUNT(*) AS clearing_line_count,
        SUM(CASE WHEN fl.matched_clearing_line_id IS NOT NULL THEN 1 ELSE 0 END) AS clearing_matched_count,
        SUM(d.source_amount) AS clearing_total_amount
    FROM (SELECT 'LIVE' AS ds UNION ALL SELECT 'ARCHIVE') src
    CROSS JOIN LATERAL (
        SELECT fl2.id, fl2.file_id, fl2.matched_clearing_line_id FROM ingestion.file_line fl2 WHERE src.ds = 'LIVE'
        UNION ALL SELECT fl2.id, fl2.file_id, fl2.matched_clearing_line_id FROM archive.ingestion_file_line fl2 WHERE src.ds = 'ARCHIVE'
    ) fl
    CROSS JOIN LATERAL (
        SELECT f2.create_date, f2.content_type FROM ingestion.file f2 WHERE src.ds = 'LIVE' AND f2.id = fl.file_id AND f2.file_type = 'Clearing'
        UNION ALL SELECT f2.create_date, f2.content_type FROM archive.ingestion_file f2 WHERE src.ds = 'ARCHIVE' AND f2.id = fl.file_id AND f2.file_type = 'Clearing'
    ) f
    JOIN LATERAL (
        SELECT cv.source_amount FROM ingestion.clearing_visa_detail cv WHERE cv.file_line_id = fl.id AND f.content_type = 'Visa' AND src.ds = 'LIVE'
        UNION ALL SELECT cm.source_amount FROM ingestion.clearing_msc_detail cm WHERE cm.file_line_id = fl.id AND f.content_type = 'Msc' AND src.ds = 'LIVE'
        UNION ALL SELECT cb.source_amount FROM ingestion.clearing_bkm_detail cb WHERE cb.file_line_id = fl.id AND f.content_type = 'Bkm' AND src.ds = 'LIVE'
        UNION ALL SELECT cv.source_amount FROM archive.ingestion_clearing_visa_detail cv WHERE cv.file_line_id = fl.id AND f.content_type = 'Visa' AND src.ds = 'ARCHIVE'
        UNION ALL SELECT cm.source_amount FROM archive.ingestion_clearing_msc_detail cm WHERE cm.file_line_id = fl.id AND f.content_type = 'Msc' AND src.ds = 'ARCHIVE'
        UNION ALL SELECT cb.source_amount FROM archive.ingestion_clearing_bkm_detail cb WHERE cb.file_line_id = fl.id AND f.content_type = 'Bkm' AND src.ds = 'ARCHIVE'
    ) d ON TRUE
    GROUP BY DATE_TRUNC('day', f.create_date)::date, src.ds, f.content_type
) cl ON c.report_date = cl.report_date AND c.data_scope = cl.data_scope AND c.network = cl.network;

-- -------------------------------------------------------------------------------------
-- E4. Eşleşmemiş işlem yaş dağılımı (LIVE + ARCHIVE)
-- -------------------------------------------------------------------------------------
CREATE OR REPLACE VIEW reporting.vw_unmatched_transaction_aging AS
SELECT
    x.age_bucket,
    x.data_scope,
    x.network,
    x.side,
    x.unmatched_count,
    x.unmatched_amount,
    CASE
        WHEN t.total_unmatched > 0
            THEN ROUND((x.unmatched_count::numeric / t.total_unmatched) * 100, 2)
        ELSE 0
        END AS pct_of_total_unmatched
FROM (
    SELECT
        CASE
            WHEN EXTRACT(EPOCH FROM (NOW() - f.create_date)) / 86400 < 1 THEN '0-1d'
            WHEN EXTRACT(EPOCH FROM (NOW() - f.create_date)) / 86400 < 3 THEN '1-3d'
            WHEN EXTRACT(EPOCH FROM (NOW() - f.create_date)) / 86400 < 7 THEN '3-7d'
            WHEN EXTRACT(EPOCH FROM (NOW() - f.create_date)) / 86400 < 14 THEN '7-14d'
            WHEN EXTRACT(EPOCH FROM (NOW() - f.create_date)) / 86400 < 30 THEN '14-30d'
            ELSE '30d+'
        END AS age_bucket,
        'LIVE' AS data_scope,
        f.content_type AS network,
        f.file_type AS side,
        COUNT(*) AS unmatched_count,
        SUM(COALESCE(d.amount, 0)) AS unmatched_amount
    FROM ingestion.file_line fl
    JOIN ingestion.file f ON f.id = fl.file_id
    LEFT JOIN LATERAL (
        SELECT cv.original_amount AS amount FROM ingestion.card_visa_detail cv WHERE cv.file_line_id = fl.id AND f.content_type = 'Visa' AND f.file_type = 'Card'
        UNION ALL
        SELECT cm.original_amount FROM ingestion.card_msc_detail cm WHERE cm.file_line_id = fl.id AND f.content_type = 'Msc' AND f.file_type = 'Card'
        UNION ALL
        SELECT cb.original_amount FROM ingestion.card_bkm_detail cb WHERE cb.file_line_id = fl.id AND f.content_type = 'Bkm' AND f.file_type = 'Card'
        UNION ALL
        SELECT cv.source_amount FROM ingestion.clearing_visa_detail cv WHERE cv.file_line_id = fl.id AND f.content_type = 'Visa' AND f.file_type = 'Clearing'
        UNION ALL
        SELECT cm.source_amount FROM ingestion.clearing_msc_detail cm WHERE cm.file_line_id = fl.id AND f.content_type = 'Msc' AND f.file_type = 'Clearing'
        UNION ALL
        SELECT cb.source_amount FROM ingestion.clearing_bkm_detail cb WHERE cb.file_line_id = fl.id AND f.content_type = 'Bkm' AND f.file_type = 'Clearing'
    ) d ON TRUE
    WHERE fl.matched_clearing_line_id IS NULL
    GROUP BY age_bucket, f.content_type, f.file_type

    UNION ALL

    SELECT
        CASE
            WHEN EXTRACT(EPOCH FROM (NOW() - f.create_date)) / 86400 < 1 THEN '0-1d'
            WHEN EXTRACT(EPOCH FROM (NOW() - f.create_date)) / 86400 < 3 THEN '1-3d'
            WHEN EXTRACT(EPOCH FROM (NOW() - f.create_date)) / 86400 < 7 THEN '3-7d'
            WHEN EXTRACT(EPOCH FROM (NOW() - f.create_date)) / 86400 < 14 THEN '7-14d'
            WHEN EXTRACT(EPOCH FROM (NOW() - f.create_date)) / 86400 < 30 THEN '14-30d'
            ELSE '30d+'
        END AS age_bucket,
        'ARCHIVE',
        f.content_type,
        f.file_type,
        COUNT(*),
        SUM(COALESCE(d.amount, 0))
    FROM archive.ingestion_file_line fl
    JOIN archive.ingestion_file f ON f.id = fl.file_id
    LEFT JOIN LATERAL (
        SELECT cv.original_amount AS amount FROM archive.ingestion_card_visa_detail cv WHERE cv.file_line_id = fl.id AND f.content_type = 'Visa' AND f.file_type = 'Card'
        UNION ALL
        SELECT cm.original_amount FROM archive.ingestion_card_msc_detail cm WHERE cm.file_line_id = fl.id AND f.content_type = 'Msc' AND f.file_type = 'Card'
        UNION ALL
        SELECT cb.original_amount FROM archive.ingestion_card_bkm_detail cb WHERE cb.file_line_id = fl.id AND f.content_type = 'Bkm' AND f.file_type = 'Card'
        UNION ALL
        SELECT cv.source_amount FROM archive.ingestion_clearing_visa_detail cv WHERE cv.file_line_id = fl.id AND f.content_type = 'Visa' AND f.file_type = 'Clearing'
        UNION ALL
        SELECT cm.source_amount FROM archive.ingestion_clearing_msc_detail cm WHERE cm.file_line_id = fl.id AND f.content_type = 'Msc' AND f.file_type = 'Clearing'
        UNION ALL
        SELECT cb.source_amount FROM archive.ingestion_clearing_bkm_detail cb WHERE cb.file_line_id = fl.id AND f.content_type = 'Bkm' AND f.file_type = 'Clearing'
    ) d ON TRUE
    WHERE fl.matched_clearing_line_id IS NULL
    GROUP BY age_bucket, f.content_type, f.file_type
) x
CROSS JOIN (
    SELECT COUNT(*) AS total_unmatched
    FROM (
        SELECT 1 FROM ingestion.file_line WHERE matched_clearing_line_id IS NULL
        UNION ALL
        SELECT 1 FROM archive.ingestion_file_line WHERE matched_clearing_line_id IS NULL
    ) u
) t;

-- -------------------------------------------------------------------------------------
-- E5. Network bazlı mutabakat skorkartı (LIVE + ARCHIVE)
-- -------------------------------------------------------------------------------------
CREATE OR REPLACE VIEW reporting.vw_network_recon_scorecard AS
SELECT
    x.data_scope,
    x.network,
    x.total_file_count,
    x.total_card_line_count,
    x.total_clearing_line_count,
    x.total_matched_count,
    x.total_unmatched_count,
    CASE
        WHEN (x.total_card_line_count + x.total_clearing_line_count) > 0
            THEN ROUND((x.total_matched_count::numeric / (x.total_card_line_count + x.total_clearing_line_count)) * 100, 2)
        ELSE 0
        END AS overall_match_rate_pct,
    x.total_card_amount,
    x.total_clearing_amount,
    COALESCE(x.total_card_amount, 0) - COALESCE(x.total_clearing_amount, 0) AS net_amount_difference,
    x.avg_card_original_amount,
    x.avg_clearing_source_amount,
    x.recon_success_line_count,
    x.recon_failed_line_count,
    x.recon_pending_line_count,
    CASE
        WHEN (x.recon_success_line_count + x.recon_failed_line_count + x.recon_pending_line_count) > 0
            THEN ROUND((x.recon_success_line_count::numeric / (x.recon_success_line_count + x.recon_failed_line_count + x.recon_pending_line_count)) * 100, 2)
        ELSE 0
        END AS recon_success_rate_pct,
    x.first_file_date,
    x.last_file_date
FROM (
    SELECT
        ds.data_scope,
        f.content_type AS network,
        COUNT(DISTINCT f.id) AS total_file_count,
        SUM(CASE WHEN f.file_type = 'Card' THEN lc.line_count ELSE 0 END) AS total_card_line_count,
        SUM(CASE WHEN f.file_type = 'Clearing' THEN lc.line_count ELSE 0 END) AS total_clearing_line_count,
        SUM(lc.matched_count) AS total_matched_count,
        SUM(lc.unmatched_count) AS total_unmatched_count,
        SUM(CASE WHEN f.file_type = 'Card' THEN lc.total_amount ELSE 0 END) AS total_card_amount,
        SUM(CASE WHEN f.file_type = 'Clearing' THEN lc.total_amount ELSE 0 END) AS total_clearing_amount,
        CASE WHEN SUM(CASE WHEN f.file_type = 'Card' THEN lc.line_count ELSE 0 END) > 0
            THEN ROUND(SUM(CASE WHEN f.file_type = 'Card' THEN lc.total_amount ELSE 0 END)::numeric / SUM(CASE WHEN f.file_type = 'Card' THEN lc.line_count ELSE 0 END), 2)
            ELSE 0 END AS avg_card_original_amount,
        CASE WHEN SUM(CASE WHEN f.file_type = 'Clearing' THEN lc.line_count ELSE 0 END) > 0
            THEN ROUND(SUM(CASE WHEN f.file_type = 'Clearing' THEN lc.total_amount ELSE 0 END)::numeric / SUM(CASE WHEN f.file_type = 'Clearing' THEN lc.line_count ELSE 0 END), 2)
            ELSE 0 END AS avg_clearing_source_amount,
        SUM(lc.recon_success_count) AS recon_success_line_count,
        SUM(lc.recon_failed_count) AS recon_failed_line_count,
        SUM(lc.recon_pending_count) AS recon_pending_line_count,
        MIN(f.create_date) AS first_file_date,
        MAX(f.create_date) AS last_file_date
    FROM (SELECT 'LIVE' AS data_scope UNION ALL SELECT 'ARCHIVE') ds
    CROSS JOIN LATERAL (
        SELECT f2.id, f2.content_type, f2.file_type, f2.create_date
        FROM ingestion.file f2 WHERE ds.data_scope = 'LIVE'
        UNION ALL
        SELECT f2.id, f2.content_type, f2.file_type, f2.create_date
        FROM archive.ingestion_file f2 WHERE ds.data_scope = 'ARCHIVE'
    ) f
    CROSS JOIN LATERAL (
        SELECT
            COUNT(*) AS line_count,
            SUM(CASE WHEN fl.matched_clearing_line_id IS NOT NULL THEN 1 ELSE 0 END) AS matched_count,
            SUM(CASE WHEN fl.matched_clearing_line_id IS NULL THEN 1 ELSE 0 END) AS unmatched_count,
            COUNT(*) FILTER (WHERE fl.reconciliation_status = 'Success') AS recon_success_count,
            COUNT(*) FILTER (WHERE fl.reconciliation_status = 'Failed') AS recon_failed_count,
            COUNT(*) FILTER (WHERE fl.reconciliation_status IN ('Ready', 'Processing')) AS recon_pending_count,
            SUM(COALESCE(
                (SELECT cv.original_amount FROM ingestion.card_visa_detail cv WHERE cv.file_line_id = fl.id AND f.content_type = 'Visa' AND f.file_type = 'Card' AND ds.data_scope = 'LIVE' LIMIT 1),
                (SELECT cm.original_amount FROM ingestion.card_msc_detail cm WHERE cm.file_line_id = fl.id AND f.content_type = 'Msc' AND f.file_type = 'Card' AND ds.data_scope = 'LIVE' LIMIT 1),
                (SELECT cb.original_amount FROM ingestion.card_bkm_detail cb WHERE cb.file_line_id = fl.id AND f.content_type = 'Bkm' AND f.file_type = 'Card' AND ds.data_scope = 'LIVE' LIMIT 1),
                (SELECT cv.source_amount FROM ingestion.clearing_visa_detail cv WHERE cv.file_line_id = fl.id AND f.content_type = 'Visa' AND f.file_type = 'Clearing' AND ds.data_scope = 'LIVE' LIMIT 1),
                (SELECT cm.source_amount FROM ingestion.clearing_msc_detail cm WHERE cm.file_line_id = fl.id AND f.content_type = 'Msc' AND f.file_type = 'Clearing' AND ds.data_scope = 'LIVE' LIMIT 1),
                (SELECT cb.source_amount FROM ingestion.clearing_bkm_detail cb WHERE cb.file_line_id = fl.id AND f.content_type = 'Bkm' AND f.file_type = 'Clearing' AND ds.data_scope = 'LIVE' LIMIT 1),
                (SELECT cv.original_amount FROM archive.ingestion_card_visa_detail cv WHERE cv.file_line_id = fl.id AND f.content_type = 'Visa' AND f.file_type = 'Card' AND ds.data_scope = 'ARCHIVE' LIMIT 1),
                (SELECT cm.original_amount FROM archive.ingestion_card_msc_detail cm WHERE cm.file_line_id = fl.id AND f.content_type = 'Msc' AND f.file_type = 'Card' AND ds.data_scope = 'ARCHIVE' LIMIT 1),
                (SELECT cb.original_amount FROM archive.ingestion_card_bkm_detail cb WHERE cb.file_line_id = fl.id AND f.content_type = 'Bkm' AND f.file_type = 'Card' AND ds.data_scope = 'ARCHIVE' LIMIT 1),
                (SELECT cv.source_amount FROM archive.ingestion_clearing_visa_detail cv WHERE cv.file_line_id = fl.id AND f.content_type = 'Visa' AND f.file_type = 'Clearing' AND ds.data_scope = 'ARCHIVE' LIMIT 1),
                (SELECT cm.source_amount FROM archive.ingestion_clearing_msc_detail cm WHERE cm.file_line_id = fl.id AND f.content_type = 'Msc' AND f.file_type = 'Clearing' AND ds.data_scope = 'ARCHIVE' LIMIT 1),
                (SELECT cb.source_amount FROM archive.ingestion_clearing_bkm_detail cb WHERE cb.file_line_id = fl.id AND f.content_type = 'Bkm' AND f.file_type = 'Clearing' AND ds.data_scope = 'ARCHIVE' LIMIT 1),
                0
            )) AS total_amount
        FROM (
            SELECT fl2.id, fl2.matched_clearing_line_id, fl2.reconciliation_status
            FROM ingestion.file_line fl2 WHERE fl2.file_id = f.id AND ds.data_scope = 'LIVE'
            UNION ALL
            SELECT fl2.id, fl2.matched_clearing_line_id, fl2.reconciliation_status
            FROM archive.ingestion_file_line fl2 WHERE fl2.file_id = f.id AND ds.data_scope = 'ARCHIVE'
        ) fl
    ) lc
    GROUP BY ds.data_scope, f.content_type
) x;
