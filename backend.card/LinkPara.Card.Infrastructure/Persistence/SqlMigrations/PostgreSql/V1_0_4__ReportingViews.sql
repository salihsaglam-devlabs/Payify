-- ============================================================================
-- Payify Reporting Views — PostgreSQL (Consolidated)
-- Version: 1.0.4
-- Date: 2026-04-15
-- Description: ALL reporting views in a single migration file.
--              Includes: schema creation, file ingestion summary, reconciliation
--              file/line/operation/alert/aging/archive views, base transaction
--              views, matched pair view, business views, and summary views.
-- ============================================================================


-- ############################################################################
-- PART 1: SCHEMA + CORE REPORTING VIEWS
-- ############################################################################

-- Create reporting schema
DO $$ BEGIN
    IF NOT EXISTS(SELECT 1 FROM pg_namespace WHERE nspname = 'reporting') THEN
        CREATE SCHEMA reporting;
    END IF;
END $$;

-- ============================================================================
-- VIEW 1: File Ingestion Summary
-- ============================================================================
CREATE OR REPLACE VIEW reporting.vw_file_ingestion_summary AS
SELECT
    f.id                                    AS file_id,
    f.file_name,
    f.file_key,
    f.source_type,
    f.file_type,
    f.content_type,
    f.status,
    f.expected_line_count                   AS expected_count,
    f.processed_line_count                  AS processed_count,
    f.successful_line_count                 AS success_count,
    f.failed_line_count                     AS error_count,
    CASE
        WHEN f.processed_line_count > 0
        THEN ROUND((f.successful_line_count::numeric / f.processed_line_count) * 100, 2)
        ELSE 0
    END                                     AS success_rate,
    CASE
        WHEN f.expected_line_count > 0 AND f.processed_line_count <> f.expected_line_count
        THEN true ELSE false
    END                                     AS has_count_mismatch,
    f.is_archived,
    f.create_date                           AS ingested_at,
    f.update_date                           AS last_updated_at,
    f.created_by,
    f.record_status
FROM ingestion.file f;

-- ============================================================================
-- VIEW 2: Reconciliation File Summary
-- ============================================================================
CREATE OR REPLACE VIEW reporting.vw_reconciliation_file_summary AS
SELECT
    f.id                                        AS file_id,
    f.file_name,
    f.file_type,
    f.content_type,
    f.status                                    AS file_status,
    f.is_archived,
    f.create_date                               AS ingested_at,

    COUNT(fl.id) FILTER (WHERE fl.line_type = 'D')
        AS total_detail_lines,

    COUNT(fl.id) FILTER (WHERE fl.line_type = 'D' AND fl.reconciliation_status = 'Ready')
        AS recon_ready_count,
    COUNT(fl.id) FILTER (WHERE fl.line_type = 'D' AND fl.reconciliation_status = 'Processing')
        AS recon_processing_count,
    COUNT(fl.id) FILTER (WHERE fl.line_type = 'D' AND fl.reconciliation_status = 'Success')
        AS recon_success_count,
    COUNT(fl.id) FILTER (WHERE fl.line_type = 'D' AND fl.reconciliation_status = 'Failed')
        AS recon_failed_count,
    COUNT(fl.id) FILTER (WHERE fl.line_type = 'D' AND fl.reconciliation_status IS NULL)
        AS recon_pending_count,

    CASE
        WHEN COUNT(fl.id) FILTER (WHERE fl.line_type = 'D') > 0
        THEN ROUND(
            (COUNT(fl.id) FILTER (WHERE fl.line_type = 'D' AND fl.reconciliation_status = 'Success')::numeric
             / COUNT(fl.id) FILTER (WHERE fl.line_type = 'D')) * 100, 2)
        ELSE 0
    END AS recon_completion_rate,

    COUNT(fl.id) FILTER (WHERE fl.line_type = 'D' AND fl.matched_clearing_line_id IS NOT NULL)
        AS matched_count,
    COUNT(fl.id) FILTER (WHERE fl.line_type = 'D' AND fl.matched_clearing_line_id IS NULL
                          AND f.file_type = 'Card')
        AS unmatched_card_count,

    COUNT(fl.id) FILTER (WHERE fl.line_type = 'D' AND fl.duplicate_status IS NOT NULL
                          AND fl.duplicate_status NOT IN ('Unique'))
        AS duplicate_count

FROM ingestion.file f
LEFT JOIN ingestion.file_line fl ON fl.file_id = f.id
GROUP BY f.id, f.file_name, f.file_type, f.content_type, f.status, f.is_archived, f.create_date;

-- ============================================================================
-- VIEW 3: Reconciliation Line Detail
-- ============================================================================
CREATE OR REPLACE VIEW reporting.vw_reconciliation_line_detail AS
SELECT
    fl.id                                   AS file_line_id,
    fl.file_id,
    f.file_name,
    f.file_type,
    f.content_type,
    fl.line_number,
    fl.line_type,
    fl.status                               AS line_status,
    fl.reconciliation_status,
    fl.duplicate_status,
    fl.duplicate_group_id,
    fl.matched_clearing_line_id,
    CASE WHEN fl.matched_clearing_line_id IS NOT NULL THEN true ELSE false END
        AS has_match,
    fl.correlation_key,
    fl.correlation_value,
    fl.retry_count,
    fl.message                              AS line_message,
    f.is_archived,

    -- Evaluation aggregates (subquery for latest)
    eval_agg.evaluation_count,
    eval_agg.latest_evaluation_status,
    eval_agg.latest_evaluation_message,

    -- Operation aggregates
    op_agg.operation_count,
    op_agg.pending_operation_count,
    op_agg.failed_operation_count,
    op_agg.completed_operation_count,
    op_agg.manual_operation_count,

    fl.create_date                          AS line_created_at,
    fl.update_date                          AS line_updated_at,
    EXTRACT(DAY FROM (NOW() - fl.create_date))::int AS age_days

FROM ingestion.file_line fl
JOIN ingestion.file f ON f.id = fl.file_id
LEFT JOIN LATERAL (
    SELECT
        COUNT(*)                            AS evaluation_count,
        (SELECT e2.status FROM reconciliation.evaluation e2
         WHERE e2.file_line_id = fl.id
         ORDER BY e2.create_date DESC LIMIT 1)  AS latest_evaluation_status,
        (SELECT e2.message FROM reconciliation.evaluation e2
         WHERE e2.file_line_id = fl.id
         ORDER BY e2.create_date DESC LIMIT 1)  AS latest_evaluation_message
    FROM reconciliation.evaluation e
    WHERE e.file_line_id = fl.id
) eval_agg ON true
LEFT JOIN LATERAL (
    SELECT
        COUNT(*)                            AS operation_count,
        COUNT(*) FILTER (WHERE o.status IN ('Planned', 'Blocked', 'Executing'))
            AS pending_operation_count,
        COUNT(*) FILTER (WHERE o.status = 'Failed')
            AS failed_operation_count,
        COUNT(*) FILTER (WHERE o.status = 'Completed')
            AS completed_operation_count,
        COUNT(*) FILTER (WHERE o.is_manual = true)
            AS manual_operation_count
    FROM reconciliation.operation o
    WHERE o.file_line_id = fl.id
) op_agg ON true
WHERE fl.line_type = 'D';

-- ============================================================================
-- VIEW 4: Unmatched Card Records
-- ============================================================================
CREATE OR REPLACE VIEW reporting.vw_reconciliation_unmatched_cards AS
SELECT
    fl.id                                   AS file_line_id,
    fl.file_id,
    f.file_name,
    f.content_type,
    fl.line_number,
    fl.status                               AS line_status,
    fl.reconciliation_status,
    fl.correlation_key,
    fl.correlation_value,
    fl.duplicate_status,
    fl.create_date                          AS line_created_at,
    fl.update_date                          AS line_updated_at,
    EXTRACT(DAY FROM (NOW() - fl.create_date))::int AS age_days,
    f.is_archived
FROM ingestion.file_line fl
JOIN ingestion.file f ON f.id = fl.file_id
WHERE f.file_type = 'Card'
  AND fl.line_type = 'D'
  AND fl.matched_clearing_line_id IS NULL
  AND fl.status = 'Success';

-- ============================================================================
-- VIEW 5: Reconciliation Operation Tracker
-- ============================================================================
CREATE OR REPLACE VIEW reporting.vw_reconciliation_operation_tracker AS
SELECT
    o.id                                    AS operation_id,
    o.file_line_id,
    f.file_name,
    f.content_type,
    o.evaluation_id,
    e.status                                AS evaluation_status,
    o.group_id,
    o.sequence_number,
    o.code                                  AS operation_code,
    o.note                                  AS operation_note,
    o.status                                AS operation_status,
    o.is_manual,
    o.branch,
    o.retry_count,
    o.max_retry_count                       AS max_retries,
    o.last_error,
    o.lease_owner,
    o.next_attempt_at,

    -- Latest execution info
    latest_exec.attempt_number              AS last_attempt_number,
    latest_exec.status                      AS last_execution_status,
    latest_exec.result_code                 AS last_result_code,
    latest_exec.result_message              AS last_result_message,
    latest_exec.error_code                  AS last_error_code,
    latest_exec.error_message               AS last_error_message,
    latest_exec.started_at                  AS last_execution_started_at,
    latest_exec.finished_at                 AS last_execution_finished_at,

    exec_count.total_executions,

    o.create_date                           AS operation_created_at,
    o.update_date                           AS operation_updated_at,
    EXTRACT(EPOCH FROM (NOW() - o.create_date)) / 3600 AS age_hours

FROM reconciliation.operation o
JOIN reconciliation.evaluation e ON e.id = o.evaluation_id
JOIN ingestion.file_line fl ON fl.id = o.file_line_id
JOIN ingestion.file f ON f.id = fl.file_id
LEFT JOIN LATERAL (
    SELECT ex.attempt_number, ex.status, ex.result_code, ex.result_message,
           ex.error_code, ex.error_message, ex.started_at, ex.finished_at
    FROM reconciliation.operation_execution ex
    WHERE ex.operation_id = o.id
    ORDER BY ex.attempt_number DESC
    LIMIT 1
) latest_exec ON true
LEFT JOIN LATERAL (
    SELECT COUNT(*) AS total_executions
    FROM reconciliation.operation_execution ex2
    WHERE ex2.operation_id = o.id
) exec_count ON true;

-- ============================================================================
-- VIEW 6: Pending Actions
-- ============================================================================
CREATE OR REPLACE VIEW reporting.vw_reconciliation_pending_actions AS
SELECT
    o.id                                    AS operation_id,
    o.file_line_id,
    f.file_name,
    f.content_type,
    o.group_id,
    o.code                                  AS operation_code,
    o.status                                AS operation_status,
    o.is_manual,
    o.last_error,
    o.retry_count,

    -- Review info (if manual)
    r.id                                    AS review_id,
    r.decision                              AS review_decision,
    r.reviewer_id,
    r.comment                               AS review_comment,
    r.expires_at                            AS review_expires_at,
    r.expiration_action,
    r.expiration_flow_action,

    CASE
        WHEN r.expires_at IS NOT NULL AND r.expires_at < NOW() AND r.decision = 'Pending'
        THEN true ELSE false
    END                                     AS is_review_expired,

    o.create_date                           AS operation_created_at,
    EXTRACT(EPOCH FROM (NOW() - o.create_date)) / 3600 AS waiting_hours

FROM reconciliation.operation o
JOIN ingestion.file_line fl ON fl.id = o.file_line_id
JOIN ingestion.file f ON f.id = fl.file_id
LEFT JOIN reconciliation.review r ON r.operation_id = o.id AND r.decision = 'Pending'
WHERE o.status IN ('Planned', 'Blocked', 'Executing', 'Failed');

-- ============================================================================
-- VIEW 7: Alert Dashboard
-- ============================================================================
CREATE OR REPLACE VIEW reporting.vw_reconciliation_alert_dashboard AS
SELECT
    a.id                                    AS alert_id,
    a.file_line_id,
    f.file_name,
    f.content_type,
    a.group_id,
    a.evaluation_id,
    a.operation_id,
    o.code                                  AS operation_code,
    o.status                                AS operation_status,
    a.severity,
    a.alert_type,
    a.message                               AS alert_message,
    a.alert_status,
    a.create_date                           AS raised_at,
    a.update_date                           AS last_updated_at,
    EXTRACT(EPOCH FROM (NOW() - a.create_date)) / 3600 AS age_hours
FROM reconciliation.alert a
JOIN reconciliation.operation o ON o.id = a.operation_id
JOIN ingestion.file_line fl ON fl.id = a.file_line_id
JOIN ingestion.file f ON f.id = fl.file_id;

-- ============================================================================
-- VIEW 8: Daily Reconciliation Summary
-- ============================================================================
CREATE OR REPLACE VIEW reporting.vw_daily_reconciliation_summary AS
SELECT
    f.create_date::date                     AS report_date,
    f.content_type,
    f.file_type,

    COUNT(DISTINCT f.id)                    AS total_files,

    COUNT(fl.id) FILTER (WHERE fl.line_type = 'D')
        AS total_detail_lines,

    COUNT(fl.id) FILTER (WHERE fl.line_type = 'D' AND fl.reconciliation_status = 'Success')
        AS recon_success_count,
    COUNT(fl.id) FILTER (WHERE fl.line_type = 'D' AND fl.reconciliation_status = 'Failed')
        AS recon_failed_count,
    COUNT(fl.id) FILTER (WHERE fl.line_type = 'D' AND fl.reconciliation_status IN ('Ready', 'Processing'))
        AS recon_pending_count,
    COUNT(fl.id) FILTER (WHERE fl.line_type = 'D' AND fl.reconciliation_status IS NULL)
        AS recon_not_evaluated_count,

    COUNT(fl.id) FILTER (WHERE fl.line_type = 'D' AND fl.matched_clearing_line_id IS NOT NULL)
        AS matched_count,
    COUNT(fl.id) FILTER (WHERE fl.line_type = 'D' AND fl.matched_clearing_line_id IS NULL
                          AND f.file_type = 'Card')
        AS unmatched_card_count,

    CASE
        WHEN COUNT(fl.id) FILTER (WHERE fl.line_type = 'D') > 0
        THEN ROUND(
            (COUNT(fl.id) FILTER (WHERE fl.line_type = 'D' AND fl.reconciliation_status = 'Success')::numeric
             / COUNT(fl.id) FILTER (WHERE fl.line_type = 'D')) * 100, 2)
        ELSE 0
    END AS success_rate

FROM ingestion.file f
LEFT JOIN ingestion.file_line fl ON fl.file_id = f.id
GROUP BY f.create_date::date, f.content_type, f.file_type;

-- ============================================================================
-- VIEW 9: Reconciliation Aging
-- ============================================================================
CREATE OR REPLACE VIEW reporting.vw_reconciliation_aging AS
SELECT
    CASE
        WHEN EXTRACT(DAY FROM (NOW() - fl.create_date)) <= 1  THEN '0-1 Gün'
        WHEN EXTRACT(DAY FROM (NOW() - fl.create_date)) <= 3  THEN '2-3 Gün'
        WHEN EXTRACT(DAY FROM (NOW() - fl.create_date)) <= 7  THEN '4-7 Gün'
        WHEN EXTRACT(DAY FROM (NOW() - fl.create_date)) <= 14 THEN '8-14 Gün'
        WHEN EXTRACT(DAY FROM (NOW() - fl.create_date)) <= 30 THEN '15-30 Gün'
        ELSE '30+ Gün'
    END                                     AS age_bucket,
    CASE
        WHEN EXTRACT(DAY FROM (NOW() - fl.create_date)) <= 1  THEN 1
        WHEN EXTRACT(DAY FROM (NOW() - fl.create_date)) <= 3  THEN 2
        WHEN EXTRACT(DAY FROM (NOW() - fl.create_date)) <= 7  THEN 3
        WHEN EXTRACT(DAY FROM (NOW() - fl.create_date)) <= 14 THEN 4
        WHEN EXTRACT(DAY FROM (NOW() - fl.create_date)) <= 30 THEN 5
        ELSE 6
    END                                     AS age_bucket_order,
    f.content_type,
    f.file_type,
    COUNT(*)                                AS open_count,
    MIN(fl.create_date)                     AS oldest_record_date,
    MAX(fl.create_date)                     AS newest_record_date
FROM ingestion.file_line fl
JOIN ingestion.file f ON f.id = fl.file_id
WHERE fl.line_type = 'D'
  AND fl.reconciliation_status IN ('Ready', 'Processing', 'Failed')
  AND f.is_archived = false
GROUP BY age_bucket, age_bucket_order, f.content_type, f.file_type;

-- ============================================================================
-- VIEW 10: Archive Audit Trail
-- ============================================================================
CREATE OR REPLACE VIEW reporting.vw_archive_audit_trail AS
SELECT
    al.id                                   AS archive_log_id,
    al.ingestion_file_id,
    af.file_name,
    af.file_type,
    af.content_type,
    af.status                               AS file_status,
    af.processed_line_count,
    af.successful_line_count,
    af.failed_line_count,
    al.status                               AS archive_status,
    al.message                              AS archive_message,
    al.failure_reasons_json,
    al.filter_json,
    al.create_date                          AS archived_at,
    al.created_by                           AS archived_by,
    af.create_date                          AS file_ingested_at,
    EXTRACT(DAY FROM (al.create_date - af.create_date))::int AS days_to_archive
FROM archive.archive_log al
LEFT JOIN archive.ingestion_file af ON af.id = al.ingestion_file_id;

-- ============================================================================
-- VIEW 11: Clearing Dispute Monitor
-- ============================================================================
CREATE OR REPLACE VIEW reporting.vw_clearing_dispute_monitor AS

-- VISA clearing disputes
SELECT
    cvd.id                                  AS clearing_detail_id,
    fl.id                                   AS file_line_id,
    f.id                                    AS file_id,
    f.file_name,
    'Visa'                                  AS network,
    cvd.txn_type,
    cvd.io_flag,
    cvd.ocean_txn_guid,
    cvd.rrn,
    cvd.arn,
    cvd.card_no,
    cvd.card_dci,
    cvd.dispute_code,
    cvd.control_stat,
    cvd.source_amount,
    cvd.source_currency,
    cvd.destination_amount,
    cvd.destination_currency,
    cvd.merchant_name,
    cvd.txn_date,
    cvd.txn_time,
    fl.reconciliation_status,
    fl.matched_clearing_line_id,
    fl.create_date                          AS ingested_at,
    f.is_archived
FROM ingestion.clearing_visa_detail cvd
JOIN ingestion.file_line fl ON fl.id = cvd.file_line_id
JOIN ingestion.file f ON f.id = fl.file_id
WHERE cvd.control_stat <> 'Normal' OR cvd.dispute_code <> 'None'

UNION ALL

-- MSC clearing disputes
SELECT
    cmd.id,
    fl.id,
    f.id,
    f.file_name,
    'Mastercard',
    cmd.txn_type,
    cmd.io_flag,
    cmd.ocean_txn_guid,
    cmd.rrn,
    cmd.arn,
    cmd.card_no,
    cmd.card_dci,
    cmd.dispute_code,
    cmd.control_stat,
    cmd.source_amount,
    cmd.source_currency,
    cmd.destination_amount,
    cmd.destination_currency,
    cmd.merchant_name,
    cmd.txn_date,
    cmd.txn_time,
    fl.reconciliation_status,
    fl.matched_clearing_line_id,
    fl.create_date,
    f.is_archived
FROM ingestion.clearing_msc_detail cmd
JOIN ingestion.file_line fl ON fl.id = cmd.file_line_id
JOIN ingestion.file f ON f.id = fl.file_id
WHERE cmd.control_stat <> 'Normal' OR cmd.dispute_code <> 'None'

UNION ALL

-- BKM clearing disputes
SELECT
    cbd.id,
    fl.id,
    f.id,
    f.file_name,
    'BKM',
    cbd.txn_type,
    cbd.io_flag,
    cbd.ocean_txn_guid,
    cbd.rrn,
    cbd.arn,
    cbd.card_no,
    cbd.card_dci,
    cbd.dispute_code,
    cbd.control_stat,
    cbd.source_amount,
    cbd.source_currency,
    cbd.destination_amount,
    cbd.destination_currency,
    cbd.merchant_name,
    cbd.txn_date,
    cbd.txn_time,
    fl.reconciliation_status,
    fl.matched_clearing_line_id,
    fl.create_date,
    f.is_archived
FROM ingestion.clearing_bkm_detail cbd
JOIN ingestion.file_line fl ON fl.id = cbd.file_line_id
JOIN ingestion.file f ON f.id = fl.file_id
WHERE cbd.control_stat <> 'Normal' OR cbd.dispute_code <> 'None';


-- ############################################################################
-- PART 2: BASE TRANSACTION VIEWS
-- ############################################################################

-- ============================================================================
-- VIEW 12: reporting.vw_base_card_transaction
-- Purpose: Normalized union of BKM/VISA/MSC card detail tables.
-- ============================================================================
CREATE OR REPLACE VIEW reporting.vw_base_card_transaction AS

-- ── BKM Card Transactions ───────────────────────────────────────────────────
SELECT
    f.id                                        AS file_id,
    f.file_name                                 AS file_name,
    fl.id                                       AS file_line_id,
    'BKM'::text                                 AS network,

    cbd.ocean_txn_guid                          AS ocean_txn_guid,
    cbd.ocean_main_txn_guid                     AS ocean_main_txn_guid,
    cbd.rrn                                     AS rrn,
    cbd.arn                                     AS arn,
    cbd.provision_code                          AS provision_code,
    cbd.card_no                                 AS card_no,

    cbd.merchant_name                           AS merchant_name,
    cbd.merchant_city                           AS merchant_city,
    cbd.merchant_country                        AS merchant_country,
    cbd.mcc                                     AS mcc,

    cbd.transaction_date                        AS transaction_date,
    cbd.transaction_time                        AS transaction_time,
    cbd.value_date                              AS value_date,
    cbd.end_of_day_date                         AS end_of_day_date,

    cbd.original_amount                         AS original_amount,
    cbd.original_currency                       AS original_currency,
    cbd.settlement_amount                       AS settlement_amount,
    cbd.settlement_currency                     AS settlement_currency,
    cbd.billing_amount                          AS billing_amount,
    cbd.billing_currency                        AS billing_currency,
    cbd.tax1                                    AS tax1,
    cbd.tax2                                    AS tax2,
    cbd.cashback_amount                         AS cashback_amount,
    cbd.surcharge_amount                        AS surcharge_amount,

    cbd.txn_stat                                AS txn_stat,
    cbd.response_code                           AS response_code,
    cbd.is_successful_txn                       AS is_successful_txn,
    cbd.is_txn_settle                           AS is_txn_settle,

    fl.reconciliation_status                    AS reconciliation_status,
    fl.duplicate_status                         AS duplicate_status,
    fl.duplicate_group_id                       AS duplicate_group_id,
    fl.matched_clearing_line_id                 AS matched_clearing_line_id,

    cbd.create_date                             AS create_date,
    cbd.record_status                           AS record_status

FROM ingestion.card_bkm_detail cbd
INNER JOIN ingestion.file_line fl ON fl.id = cbd.file_line_id
INNER JOIN ingestion.file f       ON f.id  = fl.file_id

UNION ALL

-- ── VISA Card Transactions ──────────────────────────────────────────────────
SELECT
    f.id                                        AS file_id,
    f.file_name                                 AS file_name,
    fl.id                                       AS file_line_id,
    'VISA'::text                                AS network,

    cvd.ocean_txn_guid                          AS ocean_txn_guid,
    cvd.ocean_main_txn_guid                     AS ocean_main_txn_guid,
    cvd.rrn                                     AS rrn,
    cvd.arn                                     AS arn,
    cvd.provision_code                          AS provision_code,
    cvd.card_no                                 AS card_no,

    cvd.merchant_name                           AS merchant_name,
    cvd.merchant_city                           AS merchant_city,
    cvd.merchant_country                        AS merchant_country,
    cvd.mcc                                     AS mcc,

    cvd.transaction_date                        AS transaction_date,
    cvd.transaction_time                        AS transaction_time,
    cvd.value_date                              AS value_date,
    cvd.end_of_day_date                         AS end_of_day_date,

    cvd.original_amount                         AS original_amount,
    cvd.original_currency                       AS original_currency,
    cvd.settlement_amount                       AS settlement_amount,
    cvd.settlement_currency                     AS settlement_currency,
    cvd.billing_amount                          AS billing_amount,
    cvd.billing_currency                        AS billing_currency,
    cvd.tax1                                    AS tax1,
    cvd.tax2                                    AS tax2,
    cvd.cashback_amount                         AS cashback_amount,
    cvd.surcharge_amount                        AS surcharge_amount,

    cvd.txn_stat                                AS txn_stat,
    cvd.response_code                           AS response_code,
    cvd.is_successful_txn                       AS is_successful_txn,
    cvd.is_txn_settle                           AS is_txn_settle,

    fl.reconciliation_status                    AS reconciliation_status,
    fl.duplicate_status                         AS duplicate_status,
    fl.duplicate_group_id                       AS duplicate_group_id,
    fl.matched_clearing_line_id                 AS matched_clearing_line_id,

    cvd.create_date                             AS create_date,
    cvd.record_status                           AS record_status

FROM ingestion.card_visa_detail cvd
INNER JOIN ingestion.file_line fl ON fl.id = cvd.file_line_id
INNER JOIN ingestion.file f       ON f.id  = fl.file_id

UNION ALL

-- ── MSC (Mastercard) Card Transactions ──────────────────────────────────────
SELECT
    f.id                                        AS file_id,
    f.file_name                                 AS file_name,
    fl.id                                       AS file_line_id,
    'MSC'::text                                 AS network,

    cmd.ocean_txn_guid                          AS ocean_txn_guid,
    cmd.ocean_main_txn_guid                     AS ocean_main_txn_guid,
    cmd.rrn                                     AS rrn,
    cmd.arn                                     AS arn,
    cmd.provision_code                          AS provision_code,
    cmd.card_no                                 AS card_no,

    cmd.merchant_name                           AS merchant_name,
    cmd.merchant_city                           AS merchant_city,
    cmd.merchant_country                        AS merchant_country,
    cmd.mcc                                     AS mcc,

    cmd.transaction_date                        AS transaction_date,
    cmd.transaction_time                        AS transaction_time,
    cmd.value_date                              AS value_date,
    cmd.end_of_day_date                         AS end_of_day_date,

    cmd.original_amount                         AS original_amount,
    cmd.original_currency                       AS original_currency,
    cmd.settlement_amount                       AS settlement_amount,
    cmd.settlement_currency                     AS settlement_currency,
    cmd.billing_amount                          AS billing_amount,
    cmd.billing_currency                        AS billing_currency,
    cmd.tax1                                    AS tax1,
    cmd.tax2                                    AS tax2,
    cmd.cashback_amount                         AS cashback_amount,
    cmd.surcharge_amount                        AS surcharge_amount,

    cmd.txn_stat                                AS txn_stat,
    cmd.response_code                           AS response_code,
    cmd.is_successful_txn                       AS is_successful_txn,
    cmd.is_txn_settle                           AS is_txn_settle,

    fl.reconciliation_status                    AS reconciliation_status,
    fl.duplicate_status                         AS duplicate_status,
    fl.duplicate_group_id                       AS duplicate_group_id,
    fl.matched_clearing_line_id                 AS matched_clearing_line_id,

    cmd.create_date                             AS create_date,
    cmd.record_status                           AS record_status

FROM ingestion.card_msc_detail cmd
INNER JOIN ingestion.file_line fl ON fl.id = cmd.file_line_id
INNER JOIN ingestion.file f       ON f.id  = fl.file_id;


-- ============================================================================
-- VIEW 13: reporting.vw_base_clearing_transaction
-- Purpose: Normalized union of BKM/VISA/MSC clearing detail tables.
-- ============================================================================
CREATE OR REPLACE VIEW reporting.vw_base_clearing_transaction AS

-- ── BKM Clearing Transactions ───────────────────────────────────────────────
SELECT
    f.id                                        AS file_id,
    f.file_name                                 AS file_name,
    fl.id                                       AS file_line_id,
    'BKM'::text                                 AS network,

    cbd.ocean_txn_guid                          AS ocean_txn_guid,
    cbd.rrn                                     AS rrn,
    cbd.arn                                     AS arn,
    cbd.provision_code                          AS provision_code,
    cbd.card_no                                 AS card_no,

    cbd.txn_type                                AS txn_type,
    cbd.io_flag                                 AS io_flag,
    cbd.control_stat                            AS control_stat,
    cbd.dispute_code                            AS dispute_code,

    cbd.source_amount                           AS source_amount,
    cbd.source_currency                         AS source_currency,
    cbd.destination_amount                      AS destination_amount,
    cbd.destination_currency                    AS destination_currency,
    cbd.cashback_amount                         AS cashback_amount,
    cbd.reimbursement_amount                    AS reimbursement_amount,

    cbd.merchant_name                           AS merchant_name,
    cbd.merchant_city                           AS merchant_city,

    cbd.txn_date                                AS txn_date,
    cbd.txn_time                                AS txn_time,

    fl.reconciliation_status                    AS reconciliation_status,
    fl.matched_clearing_line_id                 AS matched_clearing_line_id,

    cbd.create_date                             AS create_date,
    cbd.record_status                           AS record_status

FROM ingestion.clearing_bkm_detail cbd
INNER JOIN ingestion.file_line fl ON fl.id = cbd.file_line_id
INNER JOIN ingestion.file f       ON f.id  = fl.file_id

UNION ALL

-- ── VISA Clearing Transactions ──────────────────────────────────────────────
SELECT
    f.id                                        AS file_id,
    f.file_name                                 AS file_name,
    fl.id                                       AS file_line_id,
    'VISA'::text                                AS network,

    cvd.ocean_txn_guid                          AS ocean_txn_guid,
    cvd.rrn                                     AS rrn,
    cvd.arn                                     AS arn,
    cvd.provision_code                          AS provision_code,
    cvd.card_no                                 AS card_no,

    cvd.txn_type                                AS txn_type,
    cvd.io_flag                                 AS io_flag,
    cvd.control_stat                            AS control_stat,
    cvd.dispute_code                            AS dispute_code,

    cvd.source_amount                           AS source_amount,
    cvd.source_currency                         AS source_currency,
    cvd.destination_amount                      AS destination_amount,
    cvd.destination_currency                    AS destination_currency,
    cvd.cashback_amount                         AS cashback_amount,
    cvd.reimbursement_amount                    AS reimbursement_amount,

    cvd.merchant_name                           AS merchant_name,
    cvd.merchant_city                           AS merchant_city,

    cvd.txn_date                                AS txn_date,
    cvd.txn_time                                AS txn_time,

    fl.reconciliation_status                    AS reconciliation_status,
    fl.matched_clearing_line_id                 AS matched_clearing_line_id,

    cvd.create_date                             AS create_date,
    cvd.record_status                           AS record_status

FROM ingestion.clearing_visa_detail cvd
INNER JOIN ingestion.file_line fl ON fl.id = cvd.file_line_id
INNER JOIN ingestion.file f       ON f.id  = fl.file_id

UNION ALL

-- ── MSC (Mastercard) Clearing Transactions ──────────────────────────────────
SELECT
    f.id                                        AS file_id,
    f.file_name                                 AS file_name,
    fl.id                                       AS file_line_id,
    'MSC'::text                                 AS network,

    cmd.ocean_txn_guid                          AS ocean_txn_guid,
    cmd.rrn                                     AS rrn,
    cmd.arn                                     AS arn,
    cmd.provision_code                          AS provision_code,
    cmd.card_no                                 AS card_no,

    cmd.txn_type                                AS txn_type,
    cmd.io_flag                                 AS io_flag,
    cmd.control_stat                            AS control_stat,
    cmd.dispute_code                            AS dispute_code,

    cmd.source_amount                           AS source_amount,
    cmd.source_currency                         AS source_currency,
    cmd.destination_amount                      AS destination_amount,
    cmd.destination_currency                    AS destination_currency,
    cmd.cashback_amount                         AS cashback_amount,
    cmd.reimbursement_amount                    AS reimbursement_amount,

    cmd.merchant_name                           AS merchant_name,
    cmd.merchant_city                           AS merchant_city,

    cmd.txn_date                                AS txn_date,
    cmd.txn_time                                AS txn_time,

    fl.reconciliation_status                    AS reconciliation_status,
    fl.matched_clearing_line_id                 AS matched_clearing_line_id,

    cmd.create_date                             AS create_date,
    cmd.record_status                           AS record_status

FROM ingestion.clearing_msc_detail cmd
INNER JOIN ingestion.file_line fl ON fl.id = cmd.file_line_id
INNER JOIN ingestion.file f       ON f.id  = fl.file_id;


-- ############################################################################
-- PART 3: RECONCILIATION MATCHED PAIR VIEW
-- ############################################################################

-- ============================================================================
-- VIEW 14: reporting.vw_reconciliation_matched_pair
-- Purpose: Side-by-side card vs clearing comparison with mismatch flags.
-- ============================================================================
CREATE OR REPLACE VIEW reporting.vw_reconciliation_matched_pair AS
SELECT
    -- ── IDENTIFIERS ─────────────────────────────────────────────────────────
    c.file_id                                       AS file_id,
    c.file_name                                     AS file_name,
    c.file_line_id                                  AS card_file_line_id,
    clr.file_line_id                                AS clearing_file_line_id,
    c.network                                       AS network,

    -- ── CARD SIDE ───────────────────────────────────────────────────────────
    c.ocean_txn_guid                                AS card_ocean_txn_guid,
    c.rrn                                           AS card_rrn,
    c.arn                                           AS card_arn,
    c.card_no                                       AS card_card_no,
    c.merchant_name                                 AS card_merchant_name,
    c.transaction_date                              AS card_transaction_date,
    c.original_amount                               AS card_original_amount,
    c.original_currency                             AS card_original_currency,
    c.settlement_amount                             AS card_settlement_amount,
    c.settlement_currency                           AS card_settlement_currency,
    c.billing_amount                                AS card_billing_amount,
    c.is_successful_txn                             AS card_is_successful_txn,

    -- ── CLEARING SIDE ───────────────────────────────────────────────────────
    clr.ocean_txn_guid                              AS clearing_ocean_txn_guid,
    clr.rrn                                         AS clearing_rrn,
    clr.arn                                         AS clearing_arn,
    clr.card_no                                     AS clearing_card_no,
    clr.merchant_name                               AS clearing_merchant_name,
    clr.txn_date                                    AS clearing_txn_date,
    clr.source_amount                               AS clearing_source_amount,
    clr.source_currency                             AS clearing_source_currency,
    clr.destination_amount                          AS clearing_destination_amount,
    clr.destination_currency                        AS clearing_destination_currency,
    clr.control_stat                                AS clearing_control_stat,

    -- ── MATCH STATUS ────────────────────────────────────────────────────────
    CASE
        WHEN clr.file_line_id IS NULL THEN 'UNMATCHED_CARD'
        ELSE 'MATCHED'
    END                                             AS match_status,

    -- ── COMPARISON FIELDS ───────────────────────────────────────────────────
    c.original_amount - clr.source_amount           AS amount_difference,

    CASE
        WHEN clr.file_line_id IS NULL THEN NULL
        WHEN ABS(COALESCE(c.original_amount, 0) - COALESCE(clr.source_amount, 0)) > 0.01 THEN TRUE
        ELSE FALSE
    END                                             AS has_amount_mismatch,

    CASE
        WHEN clr.file_line_id IS NULL THEN NULL
        WHEN c.original_currency IS DISTINCT FROM clr.source_currency THEN TRUE
        ELSE FALSE
    END                                             AS has_currency_mismatch,

    CASE
        WHEN clr.file_line_id IS NULL THEN NULL
        WHEN c.transaction_date::text IS DISTINCT FROM clr.txn_date::text THEN TRUE
        ELSE FALSE
    END                                             AS has_date_mismatch,

    CASE
        WHEN clr.file_line_id IS NULL THEN NULL
        WHEN c.is_successful_txn = 'Successful'
             AND COALESCE(clr.control_stat, 'Normal') <> 'Normal' THEN TRUE
        WHEN COALESCE(c.is_successful_txn, 'Unsuccessful') = 'Unsuccessful'
             AND COALESCE(clr.control_stat, 'Normal') = 'Normal' THEN TRUE
        ELSE FALSE
    END                                             AS has_status_mismatch,

    -- ── RECON META ──────────────────────────────────────────────────────────
    c.reconciliation_status                         AS reconciliation_status,
    c.duplicate_status                              AS duplicate_status,

    -- ── AUDIT ───────────────────────────────────────────────────────────────
    c.create_date                                   AS card_create_date

FROM reporting.vw_base_card_transaction c
LEFT JOIN reporting.vw_base_clearing_transaction clr
    ON c.matched_clearing_line_id = clr.file_line_id;


-- ############################################################################
-- PART 4: RECONCILIATION BUSINESS VIEWS
-- ############################################################################

-- ============================================================================
-- VIEW 15: reporting.vw_reconciliation_unmatched_card
-- Purpose: Card transactions with NO corresponding clearing record.
-- ============================================================================
CREATE OR REPLACE VIEW reporting.vw_reconciliation_unmatched_card AS
SELECT
    file_id, file_name, card_file_line_id, clearing_file_line_id, network,
    card_ocean_txn_guid, card_rrn, card_arn, card_card_no, card_merchant_name,
    card_transaction_date, card_original_amount, card_original_currency,
    card_settlement_amount, card_settlement_currency, card_billing_amount,
    card_is_successful_txn,
    clearing_ocean_txn_guid, clearing_rrn, clearing_arn, clearing_card_no,
    clearing_merchant_name, clearing_txn_date, clearing_source_amount,
    clearing_source_currency, clearing_destination_amount,
    clearing_destination_currency, clearing_control_stat,
    match_status, amount_difference, has_amount_mismatch, has_currency_mismatch,
    has_date_mismatch, has_status_mismatch,
    reconciliation_status, duplicate_status, card_create_date
FROM reporting.vw_reconciliation_matched_pair
WHERE match_status = 'UNMATCHED_CARD';


-- ============================================================================
-- VIEW 16: reporting.vw_reconciliation_amount_mismatch
-- Purpose: Matched pairs where card amount and clearing amount differ > 0.01
-- ============================================================================
CREATE OR REPLACE VIEW reporting.vw_reconciliation_amount_mismatch AS
SELECT
    file_id, file_name, card_file_line_id, clearing_file_line_id, network,
    card_ocean_txn_guid, card_rrn, card_arn, card_card_no, card_merchant_name,
    card_transaction_date, card_original_amount, card_original_currency,
    card_settlement_amount, card_settlement_currency, card_billing_amount,
    card_is_successful_txn,
    clearing_ocean_txn_guid, clearing_rrn, clearing_arn, clearing_card_no,
    clearing_merchant_name, clearing_txn_date, clearing_source_amount,
    clearing_source_currency, clearing_destination_amount,
    clearing_destination_currency, clearing_control_stat,
    match_status, amount_difference, has_amount_mismatch, has_currency_mismatch,
    has_date_mismatch, has_status_mismatch,
    reconciliation_status, duplicate_status, card_create_date
FROM reporting.vw_reconciliation_matched_pair
WHERE match_status = 'MATCHED'
  AND has_amount_mismatch = TRUE;


-- ============================================================================
-- VIEW 17: reporting.vw_reconciliation_status_mismatch
-- Purpose: Matched pairs where success indicators disagree.
-- ============================================================================
CREATE OR REPLACE VIEW reporting.vw_reconciliation_status_mismatch AS
SELECT
    file_id, file_name, card_file_line_id, clearing_file_line_id, network,
    card_ocean_txn_guid, card_rrn, card_arn, card_card_no, card_merchant_name,
    card_transaction_date, card_original_amount, card_original_currency,
    card_settlement_amount, card_settlement_currency, card_billing_amount,
    card_is_successful_txn,
    clearing_ocean_txn_guid, clearing_rrn, clearing_arn, clearing_card_no,
    clearing_merchant_name, clearing_txn_date, clearing_source_amount,
    clearing_source_currency, clearing_destination_amount,
    clearing_destination_currency, clearing_control_stat,
    match_status, amount_difference, has_amount_mismatch, has_currency_mismatch,
    has_date_mismatch, has_status_mismatch,
    reconciliation_status, duplicate_status, card_create_date
FROM reporting.vw_reconciliation_matched_pair
WHERE match_status = 'MATCHED'
  AND has_status_mismatch = TRUE;


-- ============================================================================
-- VIEW 18: reporting.vw_reconciliation_clean_matched
-- Purpose: Matched pairs that pass ALL comparison checks — fully reconciled.
-- ============================================================================
CREATE OR REPLACE VIEW reporting.vw_reconciliation_clean_matched AS
SELECT
    file_id, file_name, card_file_line_id, clearing_file_line_id, network,
    card_ocean_txn_guid, card_rrn, card_arn, card_card_no, card_merchant_name,
    card_transaction_date, card_original_amount, card_original_currency,
    card_settlement_amount, card_settlement_currency, card_billing_amount,
    card_is_successful_txn,
    clearing_ocean_txn_guid, clearing_rrn, clearing_arn, clearing_card_no,
    clearing_merchant_name, clearing_txn_date, clearing_source_amount,
    clearing_source_currency, clearing_destination_amount,
    clearing_destination_currency, clearing_control_stat,
    match_status, amount_difference, has_amount_mismatch, has_currency_mismatch,
    has_date_mismatch, has_status_mismatch,
    reconciliation_status, duplicate_status, card_create_date
FROM reporting.vw_reconciliation_matched_pair
WHERE match_status = 'MATCHED'
  AND has_amount_mismatch   = FALSE
  AND has_currency_mismatch = FALSE
  AND has_date_mismatch     = FALSE;


-- ============================================================================
-- VIEW 19: reporting.vw_reconciliation_problem_records
-- Purpose: ALL records with at least one reconciliation issue.
-- ============================================================================
CREATE OR REPLACE VIEW reporting.vw_reconciliation_problem_records AS
SELECT
    file_id, file_name, card_file_line_id, clearing_file_line_id, network,
    card_ocean_txn_guid, card_rrn, card_arn, card_card_no, card_merchant_name,
    card_transaction_date, card_original_amount, card_original_currency,
    card_settlement_amount, card_settlement_currency, card_billing_amount,
    card_is_successful_txn,
    clearing_ocean_txn_guid, clearing_rrn, clearing_arn, clearing_card_no,
    clearing_merchant_name, clearing_txn_date, clearing_source_amount,
    clearing_source_currency, clearing_destination_amount,
    clearing_destination_currency, clearing_control_stat,
    match_status, amount_difference, has_amount_mismatch, has_currency_mismatch,
    has_date_mismatch, has_status_mismatch,
    reconciliation_status, duplicate_status, card_create_date
FROM reporting.vw_reconciliation_matched_pair
WHERE match_status = 'UNMATCHED_CARD'
   OR has_amount_mismatch   = TRUE
   OR has_currency_mismatch = TRUE
   OR has_date_mismatch     = TRUE
   OR has_status_mismatch   = TRUE
   OR (duplicate_status IS NOT NULL AND duplicate_status <> 'Unique');


-- ############################################################################
-- PART 5: RECONCILIATION SUMMARY VIEWS
-- ############################################################################

-- ============================================================================
-- VIEW 20: reporting.vw_reconciliation_summary_daily
-- Purpose: One row per card_transaction_date with reconciliation KPIs.
-- ============================================================================
CREATE OR REPLACE VIEW reporting.vw_reconciliation_summary_daily AS
SELECT
    card_transaction_date                           AS transaction_date,

    COUNT(*)                                        AS total_count,

    COUNT(*) FILTER (
        WHERE match_status = 'MATCHED'
    )                                               AS matched_count,

    COUNT(*) FILTER (
        WHERE match_status = 'UNMATCHED_CARD'
    )                                               AS unmatched_count,

    COUNT(*) FILTER (
        WHERE has_amount_mismatch = TRUE
    )                                               AS amount_mismatch_count,

    COUNT(*) FILTER (
        WHERE has_currency_mismatch = TRUE
    )                                               AS currency_mismatch_count,

    COUNT(*) FILTER (
        WHERE has_date_mismatch = TRUE
    )                                               AS date_mismatch_count,

    COUNT(*) FILTER (
        WHERE has_status_mismatch = TRUE
    )                                               AS status_mismatch_count,

    COUNT(*) FILTER (
        WHERE match_status = 'UNMATCHED_CARD'
           OR has_amount_mismatch   = TRUE
           OR has_currency_mismatch = TRUE
           OR has_date_mismatch     = TRUE
           OR has_status_mismatch   = TRUE
           OR COALESCE(duplicate_status, 'Unique') <> 'Unique'
    )                                               AS problem_count,

    COUNT(*) FILTER (
        WHERE match_status = 'MATCHED'
          AND has_amount_mismatch   = FALSE
          AND has_currency_mismatch = FALSE
          AND has_date_mismatch     = FALSE
          AND has_status_mismatch   = FALSE
    )                                               AS clean_count

FROM reporting.vw_reconciliation_matched_pair
GROUP BY card_transaction_date;


-- ============================================================================
-- VIEW 21: reporting.vw_reconciliation_summary_by_network
-- Purpose: One row per network (BKM / VISA / MSC) with reconciliation KPIs.
-- ============================================================================
CREATE OR REPLACE VIEW reporting.vw_reconciliation_summary_by_network AS
SELECT
    network                                         AS network,

    COUNT(*)                                        AS total_count,

    COUNT(*) FILTER (
        WHERE match_status = 'MATCHED'
    )                                               AS matched_count,

    COUNT(*) FILTER (
        WHERE match_status = 'UNMATCHED_CARD'
    )                                               AS unmatched_count,

    COUNT(*) FILTER (
        WHERE has_amount_mismatch = TRUE
    )                                               AS amount_mismatch_count,

    COUNT(*) FILTER (
        WHERE has_currency_mismatch = TRUE
    )                                               AS currency_mismatch_count,

    COUNT(*) FILTER (
        WHERE has_date_mismatch = TRUE
    )                                               AS date_mismatch_count,

    COUNT(*) FILTER (
        WHERE has_status_mismatch = TRUE
    )                                               AS status_mismatch_count,

    COUNT(*) FILTER (
        WHERE match_status = 'UNMATCHED_CARD'
           OR has_amount_mismatch   = TRUE
           OR has_currency_mismatch = TRUE
           OR has_date_mismatch     = TRUE
           OR has_status_mismatch   = TRUE
           OR COALESCE(duplicate_status, 'Unique') <> 'Unique'
    )                                               AS problem_count,

    COUNT(*) FILTER (
        WHERE match_status = 'MATCHED'
          AND has_amount_mismatch   = FALSE
          AND has_currency_mismatch = FALSE
          AND has_date_mismatch     = FALSE
          AND has_status_mismatch   = FALSE
    )                                               AS clean_count

FROM reporting.vw_reconciliation_matched_pair
GROUP BY network;


-- ============================================================================
-- VIEW 22: reporting.vw_reconciliation_summary_by_file
-- Purpose: One row per ingestion file with reconciliation KPIs.
-- ============================================================================
CREATE OR REPLACE VIEW reporting.vw_reconciliation_summary_by_file AS
SELECT
    file_id                                         AS file_id,
    file_name                                       AS file_name,

    COUNT(*)                                        AS total_count,

    COUNT(*) FILTER (
        WHERE match_status = 'MATCHED'
    )                                               AS matched_count,

    COUNT(*) FILTER (
        WHERE match_status = 'UNMATCHED_CARD'
    )                                               AS unmatched_count,

    COUNT(*) FILTER (
        WHERE has_amount_mismatch = TRUE
    )                                               AS amount_mismatch_count,

    COUNT(*) FILTER (
        WHERE has_currency_mismatch = TRUE
    )                                               AS currency_mismatch_count,

    COUNT(*) FILTER (
        WHERE has_date_mismatch = TRUE
    )                                               AS date_mismatch_count,

    COUNT(*) FILTER (
        WHERE has_status_mismatch = TRUE
    )                                               AS status_mismatch_count,

    COUNT(*) FILTER (
        WHERE match_status = 'UNMATCHED_CARD'
           OR has_amount_mismatch   = TRUE
           OR has_currency_mismatch = TRUE
           OR has_date_mismatch     = TRUE
           OR has_status_mismatch   = TRUE
           OR COALESCE(duplicate_status, 'Unique') <> 'Unique'
    )                                               AS problem_count,

    COUNT(*) FILTER (
        WHERE match_status = 'MATCHED'
          AND has_amount_mismatch   = FALSE
          AND has_currency_mismatch = FALSE
          AND has_date_mismatch     = FALSE
          AND has_status_mismatch   = FALSE
    )                                               AS clean_count

FROM reporting.vw_reconciliation_matched_pair
GROUP BY file_id, file_name;


-- ============================================================================
-- VIEW 23: reporting.vw_reconciliation_summary_overall
-- Purpose: Single-row grand total of all reconciliation KPIs.
-- ============================================================================
CREATE OR REPLACE VIEW reporting.vw_reconciliation_summary_overall AS
SELECT
    COUNT(*)                                        AS total_count,

    COUNT(*) FILTER (
        WHERE match_status = 'MATCHED'
    )                                               AS matched_count,

    COUNT(*) FILTER (
        WHERE match_status = 'UNMATCHED_CARD'
    )                                               AS unmatched_count,

    COUNT(*) FILTER (
        WHERE has_amount_mismatch = TRUE
    )                                               AS amount_mismatch_count,

    COUNT(*) FILTER (
        WHERE has_currency_mismatch = TRUE
    )                                               AS currency_mismatch_count,

    COUNT(*) FILTER (
        WHERE has_date_mismatch = TRUE
    )                                               AS date_mismatch_count,

    COUNT(*) FILTER (
        WHERE has_status_mismatch = TRUE
    )                                               AS status_mismatch_count,

    COUNT(*) FILTER (
        WHERE match_status = 'UNMATCHED_CARD'
           OR has_amount_mismatch   = TRUE
           OR has_currency_mismatch = TRUE
           OR has_date_mismatch     = TRUE
           OR has_status_mismatch   = TRUE
           OR COALESCE(duplicate_status, 'Unique') <> 'Unique'
    )                                               AS problem_count,

    COUNT(*) FILTER (
        WHERE match_status = 'MATCHED'
          AND has_amount_mismatch   = FALSE
          AND has_currency_mismatch = FALSE
          AND has_date_mismatch     = FALSE
          AND has_status_mismatch   = FALSE
    )                                               AS clean_count

FROM reporting.vw_reconciliation_matched_pair;

