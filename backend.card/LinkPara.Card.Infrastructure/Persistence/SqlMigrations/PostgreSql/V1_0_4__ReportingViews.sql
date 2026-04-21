DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_namespace WHERE nspname = 'reporting') THEN
        CREATE SCHEMA reporting;
    END IF;
END $$;

-- =====================================================================================
-- rep_action_radar
-- Cross-domain prioritized worklist: tells operations team what to look at first.
-- =====================================================================================
CREATE OR REPLACE VIEW reporting.rep_action_radar AS
WITH issues AS (
    SELECT 'LIVE'::text AS data_scope, 'FILES'::text AS category, 'FAILED'::text AS issue_type,
           COUNT(*)::bigint AS open_count, MIN(create_date) AS oldest_at
    FROM ingestion.file WHERE status = 'Failed'
    UNION ALL
    SELECT 'LIVE','FILES','PROCESSING_STUCK', COUNT(*), MIN(create_date)
    FROM ingestion.file
    WHERE status = 'Processing'
      AND EXTRACT(EPOCH FROM (NOW() - COALESCE(update_date, create_date))) > 3600
    UNION ALL
    SELECT 'LIVE','FILES','INCOMPLETE_DELIVERY', COUNT(*), MIN(create_date)
    FROM ingestion.file
    WHERE expected_line_count > 0
      AND processed_line_count < expected_line_count
      AND status NOT IN ('Processing','Pending')
    UNION ALL
    SELECT 'LIVE','FILE_LINES','FAILED', COUNT(*), MIN(create_date)
    FROM ingestion.file_line WHERE status = 'Failed'
    UNION ALL
    SELECT 'LIVE','FILE_LINES','DUPLICATE_CONFLICT', COUNT(*), MIN(create_date)
    FROM ingestion.file_line WHERE duplicate_status = 'Conflict'
    UNION ALL
    SELECT 'LIVE','FILE_LINES','RECON_FAILED', COUNT(*), MIN(create_date)
    FROM ingestion.file_line WHERE reconciliation_status = 'Failed'
    UNION ALL
    SELECT 'LIVE','FILE_LINES','RECON_READY_OVERDUE', COUNT(*), MIN(create_date)
    FROM ingestion.file_line
    WHERE reconciliation_status = 'Ready' AND EXTRACT(EPOCH FROM (NOW() - create_date)) > 86400
    UNION ALL
    SELECT 'LIVE','EVALUATIONS','FAILED', COUNT(*), MIN(create_date)
    FROM reconciliation.evaluation WHERE status = 'Failed'
    UNION ALL
    SELECT 'LIVE','EVALUATIONS','STUCK', COUNT(*), MIN(create_date)
    FROM reconciliation.evaluation
    WHERE status = 'Processing' AND EXTRACT(EPOCH FROM (NOW() - create_date)) > 3600
    UNION ALL
    SELECT 'LIVE','OPERATIONS','RETRIES_EXHAUSTED', COUNT(*), MIN(create_date)
    FROM reconciliation.operation WHERE status = 'Failed' AND retry_count >= COALESCE(max_retries, 0)
    UNION ALL
    SELECT 'LIVE','OPERATIONS','BLOCKED', COUNT(*), MIN(create_date)
    FROM reconciliation.operation WHERE status = 'Blocked'
    UNION ALL
    SELECT 'LIVE','OPERATIONS','LEASE_EXPIRED', COUNT(*), MIN(create_date)
    FROM reconciliation.operation
    WHERE status = 'Executing' AND lease_expires_at IS NOT NULL AND lease_expires_at < NOW()
    UNION ALL
    SELECT 'LIVE','OPERATIONS','MANUAL_PLANNED', COUNT(*), MIN(create_date)
    FROM reconciliation.operation WHERE status = 'Planned' AND is_manual = TRUE
    UNION ALL
    SELECT 'LIVE','EXECUTIONS','HUNG', COUNT(*), MIN(started_at)
    FROM reconciliation.operation_execution
    WHERE status = 'Executing' AND EXTRACT(EPOCH FROM (NOW() - started_at)) > 1800
    UNION ALL
    SELECT 'LIVE','REVIEWS','EXPIRED', COUNT(*), MIN(create_date)
    FROM reconciliation.review
    WHERE decision = 'Pending' AND expires_at IS NOT NULL AND expires_at < NOW()
    UNION ALL
    SELECT 'LIVE','REVIEWS','OVERDUE', COUNT(*), MIN(create_date)
    FROM reconciliation.review
    WHERE decision = 'Pending' AND EXTRACT(EPOCH FROM (NOW() - create_date)) > 86400
      AND (expires_at IS NULL OR expires_at >= NOW())
    UNION ALL
    SELECT 'LIVE','ALERTS','DELIVERY_FAILED', COUNT(*), MIN(create_date)
    FROM reconciliation.alert WHERE alert_status = 'Failed'
    UNION ALL
    SELECT 'LIVE','ALERTS','PENDING_STUCK', COUNT(*), MIN(create_date)
    FROM reconciliation.alert
    WHERE alert_status = 'Pending' AND EXTRACT(EPOCH FROM (NOW() - create_date)) > 1800
    UNION ALL
    SELECT 'LIVE','ARCHIVE_RUNS','FAILED', COUNT(*), MIN(create_date)
    FROM archive.archive_log WHERE status = 'Failed'
    UNION ALL
    SELECT 'LIVE','ARCHIVE_RUNS','STUCK', COUNT(*), MIN(create_date)
    FROM archive.archive_log
    WHERE status NOT IN ('Success','Failed') AND EXTRACT(EPOCH FROM (NOW() - create_date)) > 3600
    UNION ALL
    SELECT 'ARCHIVE','FILE_LINES','FAILED_HISTORICAL', COUNT(*), MIN(create_date)
    FROM archive.ingestion_file_line WHERE status = 'Failed'
    UNION ALL
    SELECT 'ARCHIVE','FILE_LINES','UNMATCHED_HISTORICAL', COUNT(*), MIN(create_date)
    FROM archive.ingestion_file_line WHERE matched_clearing_line_id IS NULL
    UNION ALL
    SELECT 'ARCHIVE','OPERATIONS','FAILED_HISTORICAL', COUNT(*), MIN(create_date)
    FROM archive.reconciliation_operation WHERE status = 'Failed'
    UNION ALL
    SELECT 'ARCHIVE','REVIEWS','EXPIRED_HISTORICAL', COUNT(*), MIN(create_date)
    FROM archive.reconciliation_review WHERE decision = 'Pending' AND expires_at IS NOT NULL AND expires_at < NOW()
    UNION ALL
    SELECT 'ARCHIVE','ALERTS','DELIVERY_FAILED_HISTORICAL', COUNT(*), MIN(create_date)
    FROM archive.reconciliation_alert WHERE alert_status = 'Failed'
)
SELECT
    data_scope,
    category,
    issue_type,
    open_count,
    ROUND((EXTRACT(EPOCH FROM (NOW() - oldest_at)) / 3600)::numeric, 1) AS oldest_age_hours,
    CASE
        WHEN data_scope = 'ARCHIVE' THEN 'P4'
        WHEN issue_type IN ('FAILED','PROCESSING_STUCK','RETRIES_EXHAUSTED','LEASE_EXPIRED','HUNG','EXPIRED','DELIVERY_FAILED') THEN 'P1'
        WHEN issue_type IN ('INCOMPLETE_DELIVERY','BLOCKED','STUCK','MANUAL_PLANNED','OVERDUE','PENDING_STUCK','DUPLICATE_CONFLICT','RECON_FAILED') THEN 'P2'
        WHEN issue_type IN ('RECON_READY_OVERDUE','FAILED_HISTORICAL') THEN 'P3'
        ELSE 'P4'
    END AS urgency,
    CASE issue_type
        WHEN 'FAILED'                       THEN 'INSPECT_FILE_AND_REPROCESS'
        WHEN 'PROCESSING_STUCK'             THEN 'CHECK_PROCESSOR_AND_REQUEUE'
        WHEN 'INCOMPLETE_DELIVERY'          THEN 'REPROCESS_MISSING_LINES'
        WHEN 'DUPLICATE_CONFLICT'           THEN 'RESOLVE_DUPLICATE_CONFLICTS'
        WHEN 'RECON_FAILED'                 THEN 'INVESTIGATE_RECONCILIATION_FAILURES'
        WHEN 'RECON_READY_OVERDUE'          THEN 'TRIGGER_OVERDUE_EVALUATIONS'
        WHEN 'STUCK'                        THEN 'RESTART_OR_REEVALUATE'
        WHEN 'RETRIES_EXHAUSTED'            THEN 'MANUAL_INTERVENTION_REQUIRED'
        WHEN 'BLOCKED'                      THEN 'UNBLOCK_OR_RESOLVE_DEPENDENCY'
        WHEN 'LEASE_EXPIRED'                THEN 'RECLAIM_LEASE'
        WHEN 'MANUAL_PLANNED'               THEN 'PERFORM_MANUAL_OPERATION'
        WHEN 'HUNG'                         THEN 'KILL_AND_RESTART_EXECUTION'
        WHEN 'EXPIRED'                      THEN 'APPLY_EXPIRATION_DEFAULT'
        WHEN 'OVERDUE'                      THEN 'ESCALATE_OVERDUE_REVIEW'
        WHEN 'DELIVERY_FAILED'              THEN 'RESEND_OR_FIX_CHANNEL'
        WHEN 'PENDING_STUCK'                THEN 'CHECK_NOTIFICATION_PIPELINE'
        ELSE 'HISTORICAL_REVIEW_ONLY'
    END AS recommended_action
FROM issues
WHERE open_count > 0;


-- =====================================================================================
-- rep_unhealthy_files
-- Files in poor operational health, classified by issue category and required action.
-- =====================================================================================
CREATE OR REPLACE VIEW reporting.rep_unhealthy_files AS
WITH unified AS (
    SELECT 'LIVE'::text AS data_scope, id AS file_id, file_name, file_type AS side, content_type AS network,
           status, expected_line_count, processed_line_count, successful_line_count, failed_line_count,
           create_date, update_date, message
    FROM ingestion.file
    UNION ALL
    SELECT 'ARCHIVE', id, file_name, file_type, content_type,
           status, expected_line_count, processed_line_count, successful_line_count, failed_line_count,
           create_date, update_date, message
    FROM archive.ingestion_file
)
SELECT
    data_scope,
    file_id,
    file_name,
    side,
    network,
    status AS file_status,
    expected_line_count,
    processed_line_count,
    failed_line_count,
    CASE WHEN processed_line_count > 0
         THEN ROUND(failed_line_count::numeric / processed_line_count * 100, 2) ELSE 0 END AS failure_rate_pct,
    ROUND((EXTRACT(EPOCH FROM (NOW() - create_date)) / 3600)::numeric, 1) AS age_hours,
    CASE
        WHEN status = 'Failed'                                                                          THEN 'FILE_REJECTED'
        WHEN status = 'Processing'
            AND EXTRACT(EPOCH FROM (NOW() - COALESCE(update_date, create_date))) > 3600                THEN 'STUCK_PROCESSING'
        WHEN expected_line_count > 0
            AND processed_line_count < expected_line_count
            AND status NOT IN ('Processing','Pending')                                                  THEN 'INCOMPLETE_DELIVERY'
        WHEN status = 'Success' AND processed_line_count > 0
            AND failed_line_count::numeric / processed_line_count >= 0.20                              THEN 'HIGH_LINE_FAILURE_RATE'
        WHEN status = 'Success' AND failed_line_count > 0                                              THEN 'PARTIAL_LINE_FAILURES'
        ELSE 'HEALTHY'
    END AS issue_category,
    CASE
        WHEN data_scope = 'ARCHIVE'                                                                    THEN 'P4'
        WHEN status = 'Failed'                                                                          THEN 'P1'
        WHEN status = 'Processing'
            AND EXTRACT(EPOCH FROM (NOW() - COALESCE(update_date, create_date))) > 3600               THEN 'P1'
        WHEN expected_line_count > 0
            AND processed_line_count < expected_line_count
            AND status NOT IN ('Processing','Pending')                                                  THEN 'P2'
        WHEN status = 'Success' AND processed_line_count > 0
            AND failed_line_count::numeric / processed_line_count >= 0.20                              THEN 'P2'
        WHEN status = 'Success' AND failed_line_count > 0                                              THEN 'P3'
        ELSE 'P5'
    END AS urgency,
    CASE
        WHEN status = 'Failed'                                                                          THEN 'INSPECT_FILE_AND_REPROCESS'
        WHEN status = 'Processing'
            AND EXTRACT(EPOCH FROM (NOW() - COALESCE(update_date, create_date))) > 3600               THEN 'CHECK_PROCESSOR_AND_REQUEUE'
        WHEN expected_line_count > 0
            AND processed_line_count < expected_line_count
            AND status NOT IN ('Processing','Pending')                                                  THEN 'REPROCESS_MISSING_LINES'
        WHEN status = 'Success' AND processed_line_count > 0
            AND failed_line_count::numeric / processed_line_count >= 0.20                              THEN 'INVESTIGATE_BULK_FAILURE_PATTERN'
        WHEN status = 'Success' AND failed_line_count > 0                                              THEN 'REVIEW_INDIVIDUAL_FAILED_LINES'
        ELSE 'NONE'
    END AS recommended_action,
    message AS file_message
FROM unified
WHERE status = 'Failed'
   OR (status = 'Processing' AND EXTRACT(EPOCH FROM (NOW() - COALESCE(update_date, create_date))) > 3600)
   OR (expected_line_count > 0 AND processed_line_count < expected_line_count AND status NOT IN ('Processing','Pending'))
   OR (status = 'Success' AND failed_line_count > 0);


-- =====================================================================================
-- rep_stuck_pipeline_items
-- Pipeline stalls (lines / evaluations / operations / executions) ranked by stuck time.
-- =====================================================================================
CREATE OR REPLACE VIEW reporting.rep_stuck_pipeline_items AS
WITH stuck AS (
    SELECT 'LIVE'::text scope, 'FILE_LINE'::text stage, fl.id::text item_id, fl.file_id::text related_id,
           fl.create_date AS started_at, NULL::timestamp AS lease_expires_at, NULL::text AS lease_owner
    FROM ingestion.file_line fl
    WHERE fl.status = 'Processing' AND EXTRACT(EPOCH FROM (NOW() - fl.create_date)) > 3600
    UNION ALL
    SELECT 'LIVE','EVALUATION', e.id::text, e.file_line_id::text, e.create_date, NULL, NULL
    FROM reconciliation.evaluation e
    WHERE e.status = 'Processing' AND EXTRACT(EPOCH FROM (NOW() - e.create_date)) > 3600
    UNION ALL
    SELECT 'LIVE','OPERATION', o.id::text, o.evaluation_id::text, o.create_date, o.lease_expires_at, o.lease_owner
    FROM reconciliation.operation o
    WHERE o.status = 'Executing'
      AND ((o.lease_expires_at IS NOT NULL AND o.lease_expires_at < NOW())
            OR EXTRACT(EPOCH FROM (NOW() - o.create_date)) > 3600)
    UNION ALL
    SELECT 'LIVE','EXECUTION', x.id::text, x.operation_id::text, x.started_at, NULL, NULL
    FROM reconciliation.operation_execution x
    WHERE x.status = 'Executing' AND EXTRACT(EPOCH FROM (NOW() - x.started_at)) > 1800
    UNION ALL
    SELECT 'ARCHIVE','FILE_LINE', fl.id::text, fl.file_id::text, fl.create_date, NULL, NULL
    FROM archive.ingestion_file_line fl WHERE fl.status = 'Processing'
    UNION ALL
    SELECT 'ARCHIVE','EVALUATION', e.id::text, e.file_line_id::text, e.create_date, NULL, NULL
    FROM archive.reconciliation_evaluation e WHERE e.status = 'Processing'
    UNION ALL
    SELECT 'ARCHIVE','OPERATION', o.id::text, o.evaluation_id::text, o.create_date, o.lease_expires_at, o.lease_owner
    FROM archive.reconciliation_operation o WHERE o.status = 'Executing'
    UNION ALL
    SELECT 'ARCHIVE','EXECUTION', x.id::text, x.operation_id::text, x.started_at, NULL, NULL
    FROM archive.reconciliation_operation_execution x WHERE x.status = 'Executing'
)
SELECT
    scope AS data_scope,
    stage,
    item_id,
    related_id,
    started_at,
    ROUND((EXTRACT(EPOCH FROM (NOW() - started_at)) / 60)::numeric, 1) AS stuck_minutes,
    lease_owner,
    lease_expires_at,
    CASE
        WHEN lease_expires_at IS NOT NULL AND lease_expires_at < NOW()       THEN 'LEASE_EXPIRED'
        WHEN EXTRACT(EPOCH FROM (NOW() - started_at)) > 14400                 THEN 'LONG_STUCK'
        ELSE 'STUCK'
    END AS stuck_state,
    CASE
        WHEN scope = 'ARCHIVE'                                                THEN 'P4'
        WHEN lease_expires_at IS NOT NULL AND lease_expires_at < NOW()       THEN 'P1'
        WHEN EXTRACT(EPOCH FROM (NOW() - started_at)) > 14400                 THEN 'P1'
        ELSE 'P2'
    END AS urgency,
    CASE stage
        WHEN 'FILE_LINE'  THEN 'REQUEUE_FILE_LINE_FOR_PROCESSING'
        WHEN 'EVALUATION' THEN 'RESTART_OR_REEVALUATE'
        WHEN 'OPERATION'  THEN CASE WHEN lease_expires_at IS NOT NULL AND lease_expires_at < NOW()
                                    THEN 'RECLAIM_LEASE_AND_RESCHEDULE'
                                    ELSE 'INSPECT_OPERATION_AND_RESCHEDULE' END
        WHEN 'EXECUTION'  THEN 'KILL_HUNG_EXECUTION_AND_RETRY'
        ELSE 'INVESTIGATE'
    END AS recommended_action
FROM stuck;


-- =====================================================================================
-- rep_recon_failure_categorization
-- Failed reconciliation operations grouped & classified by likely root cause.
-- =====================================================================================
CREATE OR REPLACE VIEW reporting.rep_recon_failure_categorization AS
WITH ops AS (
    SELECT 'LIVE'::text scope, code, branch, is_manual, retry_count, max_retries, last_error, create_date
    FROM reconciliation.operation WHERE status = 'Failed'
    UNION ALL
    SELECT 'ARCHIVE', code, branch, is_manual, retry_count, max_retries, last_error, create_date
    FROM archive.reconciliation_operation WHERE status = 'Failed'
)
SELECT
    scope AS data_scope,
    code  AS operation_code,
    branch,
    COUNT(*)::bigint                                                            AS failed_count,
    SUM(CASE WHEN retry_count >= COALESCE(max_retries, 0) THEN 1 ELSE 0 END)::bigint AS retries_exhausted_count,
    SUM(CASE WHEN is_manual THEN 1 ELSE 0 END)::bigint                          AS manual_operation_count,
    MIN(create_date)                                                            AS oldest_failure_at,
    ROUND((EXTRACT(EPOCH FROM (NOW() - MIN(create_date))) / 3600)::numeric, 1)             AS oldest_age_hours,
    CASE
        WHEN BOOL_OR(last_error ILIKE '%timeout%' OR last_error ILIKE '%timed out%')                                   THEN 'TIMEOUT_OR_LATENCY'
        WHEN BOOL_OR(last_error ILIKE '%duplicate%' OR last_error ILIKE '%conflict%')                                  THEN 'DUPLICATE_OR_CONFLICT'
        WHEN BOOL_OR(last_error ILIKE '%null%' OR last_error ILIKE '%missing%' OR last_error ILIKE '%not found%')      THEN 'MISSING_DATA'
        WHEN BOOL_OR(last_error ILIKE '%permission%' OR last_error ILIKE '%unauthor%' OR last_error ILIKE '%forbidden%') THEN 'AUTHORIZATION'
        WHEN BOOL_OR(last_error ILIKE '%connect%' OR last_error ILIKE '%refused%' OR last_error ILIKE '%network%' OR last_error ILIKE '%socket%') THEN 'CONNECTIVITY'
        WHEN BOOL_OR(last_error ILIKE '%validation%' OR last_error ILIKE '%invalid%')                                  THEN 'VALIDATION'
        WHEN BOOL_OR(last_error IS NOT NULL)                                                                            THEN 'BUSINESS_RULE_FAILURE'
        ELSE 'UNCATEGORIZED'
    END AS likely_root_cause,
    CASE
        WHEN scope = 'ARCHIVE'                                                                                          THEN 'P4'
        WHEN SUM(CASE WHEN retry_count >= COALESCE(max_retries, 0) THEN 1 ELSE 0 END) > 0                              THEN 'P1'
        WHEN COUNT(*) > 50                                                                                              THEN 'P2'
        ELSE 'P3'
    END AS urgency,
    CASE
        WHEN scope = 'ARCHIVE'                                                                                          THEN 'HISTORICAL_TREND_ANALYSIS_ONLY'
        WHEN SUM(CASE WHEN retry_count >= COALESCE(max_retries, 0) THEN 1 ELSE 0 END) > 0                              THEN 'MANUAL_INTERVENTION_FOR_EXHAUSTED_RETRIES'
        WHEN COUNT(*) > 50                                                                                              THEN 'INVESTIGATE_BULK_FAILURE_PATTERN'
        ELSE 'MONITOR_AUTO_RETRY_OUTCOME'
    END AS recommended_action
FROM ops
GROUP BY scope, code, branch;


-- =====================================================================================
-- rep_manual_review_pressure
-- SLA pressure on manual review queue with financial exposure of pending decisions.
-- =====================================================================================
CREATE OR REPLACE VIEW reporting.rep_manual_review_pressure AS
WITH line_amounts AS (
    SELECT 'LIVE'::text data_scope, fl.id AS file_line_id,
           COALESCE(amt.amount, 0) AS amount, amt.currency
    FROM ingestion.file_line fl
    JOIN ingestion.file f ON f.id = fl.file_id
    LEFT JOIN LATERAL (
        SELECT cv.original_amount AS amount, cv.original_currency AS currency
        FROM ingestion.card_visa_detail cv WHERE cv.file_line_id = fl.id AND f.file_type='Card' AND f.content_type='Visa'
        UNION ALL SELECT cm.original_amount, cm.original_currency
        FROM ingestion.card_msc_detail cm WHERE cm.file_line_id = fl.id AND f.file_type='Card' AND f.content_type='Msc'
        UNION ALL SELECT cb.original_amount, cb.original_currency
        FROM ingestion.card_bkm_detail cb WHERE cb.file_line_id = fl.id AND f.file_type='Card' AND f.content_type='Bkm'
        UNION ALL SELECT cv.source_amount, cv.source_currency
        FROM ingestion.clearing_visa_detail cv WHERE cv.file_line_id = fl.id AND f.file_type='Clearing' AND f.content_type='Visa'
        UNION ALL SELECT cm.source_amount, cm.source_currency
        FROM ingestion.clearing_msc_detail cm WHERE cm.file_line_id = fl.id AND f.file_type='Clearing' AND f.content_type='Msc'
        UNION ALL SELECT cb.source_amount, cb.source_currency
        FROM ingestion.clearing_bkm_detail cb WHERE cb.file_line_id = fl.id AND f.file_type='Clearing' AND f.content_type='Bkm'
    ) amt ON TRUE
    UNION ALL
    SELECT 'ARCHIVE', fl.id, COALESCE(amt.amount, 0), amt.currency
    FROM archive.ingestion_file_line fl
    LEFT JOIN LATERAL (
        SELECT cv.original_amount AS amount, cv.original_currency AS currency FROM archive.ingestion_card_visa_detail cv WHERE cv.id = fl.card_visa_detail_id
        UNION ALL SELECT cm.original_amount, cm.original_currency FROM archive.ingestion_card_msc_detail cm WHERE cm.id = fl.card_msc_detail_id
        UNION ALL SELECT cb.original_amount, cb.original_currency FROM archive.ingestion_card_bkm_detail cb WHERE cb.id = fl.card_bkm_detail_id
        UNION ALL SELECT cv.source_amount, cv.source_currency FROM archive.ingestion_clearing_visa_detail cv WHERE cv.id = fl.clearing_visa_detail_id
        UNION ALL SELECT cm.source_amount, cm.source_currency FROM archive.ingestion_clearing_msc_detail cm WHERE cm.id = fl.clearing_msc_detail_id
        UNION ALL SELECT cb.source_amount, cb.source_currency FROM archive.ingestion_clearing_bkm_detail cb WHERE cb.id = fl.clearing_bkm_detail_id
    ) amt ON TRUE
), reviews AS (
    SELECT 'LIVE'::text scope, r.file_line_id, r.expires_at, r.create_date, r.expiration_action
    FROM reconciliation.review r WHERE r.decision = 'Pending'
    UNION ALL
    SELECT 'ARCHIVE', r.file_line_id, r.expires_at, r.create_date, r.expiration_action
    FROM archive.reconciliation_review r WHERE r.decision = 'Pending'
)
SELECT
    r.scope AS data_scope,
    CASE
        WHEN r.expires_at IS NOT NULL AND r.expires_at < NOW()                              THEN 'EXPIRED'
        WHEN r.expires_at IS NOT NULL AND r.expires_at < NOW() + INTERVAL '4 hours'         THEN 'EXPIRING_SOON'
        WHEN EXTRACT(EPOCH FROM (NOW() - r.create_date)) / 3600 > 24                        THEN 'OVERDUE'
        ELSE 'NORMAL'
    END                                                                                      AS sla_bucket,
    r.expiration_action                                                                      AS default_on_expiry,
    COALESCE(la.currency::text, 'UNKNOWN')                                                         AS currency,
    COUNT(*)::bigint                                                                         AS pending_review_count,
    ROUND(MAX(EXTRACT(EPOCH FROM (NOW() - r.create_date)) / 3600)::numeric, 1)                        AS oldest_waiting_hours,
    SUM(la.amount)                                                                           AS exposure_amount,
    CASE
        WHEN r.scope = 'ARCHIVE'                                                             THEN 'P4'
        WHEN COUNT(*) FILTER (WHERE r.expires_at IS NOT NULL AND r.expires_at < NOW()) > 0  THEN 'P1'
        WHEN COUNT(*) FILTER (WHERE r.expires_at IS NOT NULL
                              AND r.expires_at < NOW() + INTERVAL '4 hours') > 0            THEN 'P1'
        WHEN MAX(EXTRACT(EPOCH FROM (NOW() - r.create_date)) / 3600) > 24                   THEN 'P2'
        ELSE 'P3'
    END                                                                                      AS urgency,
    CASE
        WHEN r.scope = 'ARCHIVE'                                                             THEN 'HISTORICAL_REFERENCE_ONLY'
        WHEN COUNT(*) FILTER (WHERE r.expires_at IS NOT NULL AND r.expires_at < NOW()) > 0  THEN 'APPLY_DEFAULT_OR_DECIDE_NOW'
        WHEN COUNT(*) FILTER (WHERE r.expires_at IS NOT NULL
                              AND r.expires_at < NOW() + INTERVAL '4 hours') > 0            THEN 'DECIDE_BEFORE_EXPIRY'
        WHEN MAX(EXTRACT(EPOCH FROM (NOW() - r.create_date)) / 3600) > 24                   THEN 'ESCALATE_OVERDUE_REVIEWS'
        ELSE 'WORK_THE_QUEUE'
    END                                                                                      AS recommended_action
FROM reviews r
LEFT JOIN line_amounts la ON la.data_scope = r.scope AND la.file_line_id = r.file_line_id
GROUP BY r.scope,
         CASE
             WHEN r.expires_at IS NOT NULL AND r.expires_at < NOW()                          THEN 'EXPIRED'
             WHEN r.expires_at IS NOT NULL AND r.expires_at < NOW() + INTERVAL '4 hours'     THEN 'EXPIRING_SOON'
             WHEN EXTRACT(EPOCH FROM (NOW() - r.create_date)) / 3600 > 24                    THEN 'OVERDUE'
             ELSE 'NORMAL'
         END,
         r.expiration_action,
         COALESCE(la.currency::text, 'UNKNOWN');


-- =====================================================================================
-- rep_alert_delivery_health
-- Delivery health of alerts (per type/severity) with operational signal.
-- =====================================================================================
CREATE OR REPLACE VIEW reporting.rep_alert_delivery_health AS
WITH alerts AS (
    SELECT 'LIVE'::text scope, severity, alert_type, alert_status, create_date
    FROM reconciliation.alert
    UNION ALL
    SELECT 'ARCHIVE', severity, alert_type, alert_status, create_date
    FROM archive.reconciliation_alert
)
SELECT
    scope AS data_scope,
    COALESCE(severity, 'UNKNOWN') AS severity,
    COALESCE(alert_type, 'UNKNOWN') AS alert_type,
    COUNT(*)::bigint                                                                          AS total_count,
    COUNT(*) FILTER (WHERE alert_status = 'Pending')::bigint                                  AS pending_count,
    COUNT(*) FILTER (WHERE alert_status = 'Failed')::bigint                                   AS failed_count,
    COUNT(*) FILTER (WHERE alert_status = 'Sent')::bigint                                     AS sent_count,
    CASE WHEN COUNT(*) > 0
         THEN ROUND(COUNT(*) FILTER (WHERE alert_status = 'Failed')::numeric / COUNT(*) * 100, 2)
         ELSE 0 END                                                                            AS failure_rate_pct,
    ROUND((MAX(EXTRACT(EPOCH FROM (NOW() - create_date)) / 3600)
            FILTER (WHERE alert_status IN ('Pending','Failed')))::numeric, 1)                            AS oldest_open_age_hours,
    CASE
        WHEN scope = 'ARCHIVE'                                                                  THEN 'HISTORICAL'
        WHEN COUNT(*) FILTER (WHERE alert_status = 'Failed') > 0                                THEN 'CRITICAL'
        WHEN COUNT(*) FILTER (WHERE alert_status = 'Pending') > 0
            AND MAX(EXTRACT(EPOCH FROM (NOW() - create_date)))
                FILTER (WHERE alert_status = 'Pending') > 1800                                  THEN 'DEGRADED'
        WHEN COUNT(*) FILTER (WHERE alert_status = 'Pending') > 0                               THEN 'WARMING'
        ELSE 'HEALTHY'
    END                                                                                         AS delivery_health_status,
    CASE
        WHEN scope = 'ARCHIVE'                                                                  THEN 'P4'
        WHEN COUNT(*) FILTER (WHERE alert_status = 'Failed') > 0                                THEN 'P1'
        WHEN COUNT(*) FILTER (WHERE alert_status = 'Pending') > 0
            AND MAX(EXTRACT(EPOCH FROM (NOW() - create_date)))
                FILTER (WHERE alert_status = 'Pending') > 1800                                  THEN 'P2'
        WHEN COUNT(*) FILTER (WHERE alert_status = 'Pending') > 0                               THEN 'P3'
        ELSE 'P5'
    END                                                                                         AS urgency,
    CASE
        WHEN scope = 'ARCHIVE'                                                                  THEN 'HISTORICAL_REFERENCE_ONLY'
        WHEN COUNT(*) FILTER (WHERE alert_status = 'Failed') > 0                                THEN 'RESEND_OR_FIX_DELIVERY_CHANNEL'
        WHEN COUNT(*) FILTER (WHERE alert_status = 'Pending') > 0
            AND MAX(EXTRACT(EPOCH FROM (NOW() - create_date)))
                FILTER (WHERE alert_status = 'Pending') > 1800                                  THEN 'CHECK_NOTIFICATION_PIPELINE'
        WHEN COUNT(*) FILTER (WHERE alert_status = 'Pending') > 0                               THEN 'MONITOR_PIPELINE'
        ELSE 'NONE'
    END                                                                                         AS recommended_action
FROM alerts
GROUP BY scope, COALESCE(severity, 'UNKNOWN'), COALESCE(alert_type, 'UNKNOWN');


-- =====================================================================================
-- rep_unmatched_financial_exposure
-- How much money is sitting unmatched, sliced by scope/side/network/currency/age bucket.
-- =====================================================================================
CREATE OR REPLACE VIEW reporting.rep_unmatched_financial_exposure AS
WITH lines AS (
    SELECT 'LIVE'::text data_scope, fl.id AS file_line_id, fl.create_date,
           f.file_type AS side, f.content_type AS network,
           (fl.matched_clearing_line_id IS NOT NULL) AS is_matched,
           amt.amount, amt.currency
    FROM ingestion.file_line fl
    JOIN ingestion.file f ON f.id = fl.file_id
    LEFT JOIN LATERAL (
        SELECT cv.original_amount AS amount, cv.original_currency AS currency
        FROM ingestion.card_visa_detail cv WHERE cv.file_line_id = fl.id AND f.file_type='Card' AND f.content_type='Visa'
        UNION ALL SELECT cm.original_amount, cm.original_currency
        FROM ingestion.card_msc_detail cm WHERE cm.file_line_id = fl.id AND f.file_type='Card' AND f.content_type='Msc'
        UNION ALL SELECT cb.original_amount, cb.original_currency
        FROM ingestion.card_bkm_detail cb WHERE cb.file_line_id = fl.id AND f.file_type='Card' AND f.content_type='Bkm'
        UNION ALL SELECT cv.source_amount, cv.source_currency
        FROM ingestion.clearing_visa_detail cv WHERE cv.file_line_id = fl.id AND f.file_type='Clearing' AND f.content_type='Visa'
        UNION ALL SELECT cm.source_amount, cm.source_currency
        FROM ingestion.clearing_msc_detail cm WHERE cm.file_line_id = fl.id AND f.file_type='Clearing' AND f.content_type='Msc'
        UNION ALL SELECT cb.source_amount, cb.source_currency
        FROM ingestion.clearing_bkm_detail cb WHERE cb.file_line_id = fl.id AND f.file_type='Clearing' AND f.content_type='Bkm'
    ) amt ON TRUE
    UNION ALL
    SELECT 'ARCHIVE', fl.id, fl.create_date,
           f.file_type, f.content_type,
           (fl.matched_clearing_line_id IS NOT NULL),
           amt.amount, amt.currency
    FROM archive.ingestion_file_line fl
    JOIN archive.ingestion_file f ON f.id = fl.file_id
    LEFT JOIN LATERAL (
        SELECT cv.original_amount AS amount, cv.original_currency AS currency FROM archive.ingestion_card_visa_detail cv WHERE cv.id = fl.card_visa_detail_id
        UNION ALL SELECT cm.original_amount, cm.original_currency FROM archive.ingestion_card_msc_detail cm WHERE cm.id = fl.card_msc_detail_id
        UNION ALL SELECT cb.original_amount, cb.original_currency FROM archive.ingestion_card_bkm_detail cb WHERE cb.id = fl.card_bkm_detail_id
        UNION ALL SELECT cv.source_amount, cv.source_currency FROM archive.ingestion_clearing_visa_detail cv WHERE cv.id = fl.clearing_visa_detail_id
        UNION ALL SELECT cm.source_amount, cm.source_currency FROM archive.ingestion_clearing_msc_detail cm WHERE cm.id = fl.clearing_msc_detail_id
        UNION ALL SELECT cb.source_amount, cb.source_currency FROM archive.ingestion_clearing_bkm_detail cb WHERE cb.id = fl.clearing_bkm_detail_id
    ) amt ON TRUE
)
SELECT
    data_scope,
    side,
    network,
    COALESCE(currency::text, 'UNKNOWN') AS currency,
    CASE
        WHEN EXTRACT(EPOCH FROM (NOW() - create_date)) / 86400 < 1  THEN '0-1d'
        WHEN EXTRACT(EPOCH FROM (NOW() - create_date)) / 86400 < 3  THEN '1-3d'
        WHEN EXTRACT(EPOCH FROM (NOW() - create_date)) / 86400 < 7  THEN '3-7d'
        WHEN EXTRACT(EPOCH FROM (NOW() - create_date)) / 86400 < 30 THEN '7-30d'
        ELSE '30d+'
    END                                              AS aging_bucket,
    COUNT(*)::bigint                                 AS unmatched_count,
    SUM(COALESCE(amount, 0))                         AS exposure_amount,
    MIN(create_date)                                 AS oldest_unmatched_at,
    ROUND((EXTRACT(EPOCH FROM (NOW() - MIN(create_date))) / 86400)::numeric, 1) AS oldest_age_days,
    CASE
        WHEN data_scope = 'ARCHIVE'                                                          THEN 'HISTORICAL'
        WHEN SUM(COALESCE(amount, 0)) >= 1000000
            OR EXTRACT(EPOCH FROM (NOW() - MIN(create_date))) / 86400 >= 7                  THEN 'CRITICAL'
        WHEN SUM(COALESCE(amount, 0)) >= 100000
            OR EXTRACT(EPOCH FROM (NOW() - MIN(create_date))) / 86400 >= 3                  THEN 'HIGH'
        WHEN EXTRACT(EPOCH FROM (NOW() - MIN(create_date))) / 86400 >= 1                    THEN 'MEDIUM'
        ELSE 'LOW'
    END                                              AS risk_flag,
    CASE
        WHEN data_scope = 'ARCHIVE'                                                          THEN 'P4'
        WHEN SUM(COALESCE(amount, 0)) >= 1000000
            OR EXTRACT(EPOCH FROM (NOW() - MIN(create_date))) / 86400 >= 7                  THEN 'P1'
        WHEN SUM(COALESCE(amount, 0)) >= 100000
            OR EXTRACT(EPOCH FROM (NOW() - MIN(create_date))) / 86400 >= 3                  THEN 'P2'
        WHEN EXTRACT(EPOCH FROM (NOW() - MIN(create_date))) / 86400 >= 1                    THEN 'P3'
        ELSE 'P4'
    END                                              AS urgency,
    CASE
        WHEN data_scope = 'ARCHIVE'                                                          THEN 'HISTORICAL_TREND_ANALYSIS_ONLY'
        WHEN SUM(COALESCE(amount, 0)) >= 1000000
            OR EXTRACT(EPOCH FROM (NOW() - MIN(create_date))) / 86400 >= 7                  THEN 'ESCALATE_AGED_HIGH_VALUE_EXPOSURE'
        WHEN EXTRACT(EPOCH FROM (NOW() - MIN(create_date))) / 86400 >= 1                    THEN 'INVESTIGATE_PERSISTENT_UNMATCHED'
        ELSE 'MONITOR'
    END                                              AS recommended_action
FROM lines
WHERE is_matched = FALSE
GROUP BY data_scope, side, network, COALESCE(currency::text, 'UNKNOWN'),
         CASE
             WHEN EXTRACT(EPOCH FROM (NOW() - create_date)) / 86400 < 1  THEN '0-1d'
             WHEN EXTRACT(EPOCH FROM (NOW() - create_date)) / 86400 < 3  THEN '1-3d'
             WHEN EXTRACT(EPOCH FROM (NOW() - create_date)) / 86400 < 7  THEN '3-7d'
             WHEN EXTRACT(EPOCH FROM (NOW() - create_date)) / 86400 < 30 THEN '7-30d'
             ELSE '30d+'
         END;


-- =====================================================================================
-- rep_card_clearing_imbalance
-- Daily Card vs Clearing total amount gap per network/currency with imbalance severity.
-- =====================================================================================
CREATE OR REPLACE VIEW reporting.rep_card_clearing_imbalance AS
WITH amounts AS (
    SELECT 'LIVE'::text data_scope, f.content_type AS network, f.file_type AS side,
           DATE_TRUNC('day', f.create_date)::date AS report_date,
           amt.currency, COALESCE(amt.amount, 0) AS amount
    FROM ingestion.file_line fl
    JOIN ingestion.file f ON f.id = fl.file_id
    LEFT JOIN LATERAL (
        SELECT cv.original_amount AS amount, cv.original_currency AS currency
        FROM ingestion.card_visa_detail cv WHERE cv.file_line_id = fl.id AND f.file_type='Card' AND f.content_type='Visa'
        UNION ALL SELECT cm.original_amount, cm.original_currency
        FROM ingestion.card_msc_detail cm WHERE cm.file_line_id = fl.id AND f.file_type='Card' AND f.content_type='Msc'
        UNION ALL SELECT cb.original_amount, cb.original_currency
        FROM ingestion.card_bkm_detail cb WHERE cb.file_line_id = fl.id AND f.file_type='Card' AND f.content_type='Bkm'
        UNION ALL SELECT cv.source_amount, cv.source_currency
        FROM ingestion.clearing_visa_detail cv WHERE cv.file_line_id = fl.id AND f.file_type='Clearing' AND f.content_type='Visa'
        UNION ALL SELECT cm.source_amount, cm.source_currency
        FROM ingestion.clearing_msc_detail cm WHERE cm.file_line_id = fl.id AND f.file_type='Clearing' AND f.content_type='Msc'
        UNION ALL SELECT cb.source_amount, cb.source_currency
        FROM ingestion.clearing_bkm_detail cb WHERE cb.file_line_id = fl.id AND f.file_type='Clearing' AND f.content_type='Bkm'
    ) amt ON TRUE
    UNION ALL
    SELECT 'ARCHIVE', f.content_type, f.file_type,
           DATE_TRUNC('day', f.create_date)::date,
           amt.currency, COALESCE(amt.amount, 0)
    FROM archive.ingestion_file_line fl
    JOIN archive.ingestion_file f ON f.id = fl.file_id
    LEFT JOIN LATERAL (
        SELECT cv.original_amount AS amount, cv.original_currency AS currency FROM archive.ingestion_card_visa_detail cv WHERE cv.id = fl.card_visa_detail_id
        UNION ALL SELECT cm.original_amount, cm.original_currency FROM archive.ingestion_card_msc_detail cm WHERE cm.id = fl.card_msc_detail_id
        UNION ALL SELECT cb.original_amount, cb.original_currency FROM archive.ingestion_card_bkm_detail cb WHERE cb.id = fl.card_bkm_detail_id
        UNION ALL SELECT cv.source_amount, cv.source_currency FROM archive.ingestion_clearing_visa_detail cv WHERE cv.id = fl.clearing_visa_detail_id
        UNION ALL SELECT cm.source_amount, cm.source_currency FROM archive.ingestion_clearing_msc_detail cm WHERE cm.id = fl.clearing_msc_detail_id
        UNION ALL SELECT cb.source_amount, cb.source_currency FROM archive.ingestion_clearing_bkm_detail cb WHERE cb.id = fl.clearing_bkm_detail_id
    ) amt ON TRUE
), agg AS (
    SELECT data_scope, report_date, network, COALESCE(currency::text, 'UNKNOWN') AS currency,
           SUM(CASE WHEN side = 'Card'     THEN amount ELSE 0 END) AS card_total_amount,
           SUM(CASE WHEN side = 'Clearing' THEN amount ELSE 0 END) AS clearing_total_amount,
           COUNT(*) FILTER (WHERE side = 'Card')     AS card_line_count,
           COUNT(*) FILTER (WHERE side = 'Clearing') AS clearing_line_count
    FROM amounts
    GROUP BY data_scope, report_date, network, COALESCE(currency::text, 'UNKNOWN')
)
SELECT
    data_scope, report_date, network, currency,
    card_line_count, clearing_line_count,
    card_total_amount, clearing_total_amount,
    (card_total_amount - clearing_total_amount)                              AS amount_gap,
    ABS(card_total_amount - clearing_total_amount)                           AS abs_gap,
    CASE
        WHEN GREATEST(card_total_amount, clearing_total_amount) > 0
        THEN ROUND(ABS(card_total_amount - clearing_total_amount)::numeric
                   / GREATEST(card_total_amount, clearing_total_amount) * 100, 2)
        ELSE 0
    END                                                                       AS gap_ratio_pct,
    CASE
        WHEN ABS(card_total_amount - clearing_total_amount) = 0                                   THEN 'BALANCED'
        WHEN GREATEST(card_total_amount, clearing_total_amount) > 0
            AND ABS(card_total_amount - clearing_total_amount) / GREATEST(card_total_amount, clearing_total_amount) < 0.01 THEN 'MINOR_DRIFT'
        WHEN GREATEST(card_total_amount, clearing_total_amount) > 0
            AND ABS(card_total_amount - clearing_total_amount) / GREATEST(card_total_amount, clearing_total_amount) < 0.05 THEN 'NOTABLE_GAP'
        ELSE 'MATERIAL_IMBALANCE'
    END                                                                       AS imbalance_severity,
    CASE
        WHEN data_scope = 'ARCHIVE'                                          THEN 'P4'
        WHEN ABS(card_total_amount - clearing_total_amount) = 0              THEN 'P5'
        WHEN GREATEST(card_total_amount, clearing_total_amount) > 0
            AND ABS(card_total_amount - clearing_total_amount) / GREATEST(card_total_amount, clearing_total_amount) >= 0.05 THEN 'P1'
        WHEN GREATEST(card_total_amount, clearing_total_amount) > 0
            AND ABS(card_total_amount - clearing_total_amount) / GREATEST(card_total_amount, clearing_total_amount) >= 0.01 THEN 'P2'
        ELSE 'P3'
    END                                                                       AS urgency,
    CASE
        WHEN data_scope = 'ARCHIVE'                                          THEN 'HISTORICAL_TREND_ANALYSIS_ONLY'
        WHEN ABS(card_total_amount - clearing_total_amount) = 0              THEN 'NONE'
        WHEN GREATEST(card_total_amount, clearing_total_amount) > 0
            AND ABS(card_total_amount - clearing_total_amount) / GREATEST(card_total_amount, clearing_total_amount) >= 0.05 THEN 'ESCALATE_AND_INVESTIGATE_MATERIAL_GAP'
        WHEN GREATEST(card_total_amount, clearing_total_amount) > 0
            AND ABS(card_total_amount - clearing_total_amount) / GREATEST(card_total_amount, clearing_total_amount) >= 0.01 THEN 'INVESTIGATE_NOTABLE_GAP'
        ELSE 'MONITOR_DRIFT'
    END                                                                       AS recommended_action
FROM agg;


-- =====================================================================================
-- rep_reconciliation_quality_score
-- Composite quality grade per network/day combining match-rate, recon-failure-rate,
-- retry density and manual-review density. Highlights weakest dimension.
-- =====================================================================================
CREATE OR REPLACE VIEW reporting.rep_reconciliation_quality_score AS
WITH lines AS (
    SELECT 'LIVE'::text data_scope, DATE_TRUNC('day', f.create_date)::date AS report_date,
           f.content_type AS network,
           fl.matched_clearing_line_id IS NOT NULL AS is_matched,
           fl.reconciliation_status, fl.retry_count, fl.id AS file_line_id
    FROM ingestion.file_line fl JOIN ingestion.file f ON f.id = fl.file_id
    UNION ALL
    SELECT 'ARCHIVE', DATE_TRUNC('day', f.create_date)::date, f.content_type,
           fl.matched_clearing_line_id IS NOT NULL,
           fl.reconciliation_status, fl.retry_count, fl.id
    FROM archive.ingestion_file_line fl JOIN archive.ingestion_file f ON f.id = fl.file_id
), reviews AS (
    SELECT 'LIVE'::text data_scope, file_line_id FROM reconciliation.review
    UNION ALL
    SELECT 'ARCHIVE', file_line_id FROM archive.reconciliation_review
), agg AS (
    SELECT
        l.data_scope, l.report_date, l.network,
        COUNT(*)                                                              AS total_lines,
        SUM(CASE WHEN l.is_matched THEN 1 ELSE 0 END)                          AS matched_lines,
        SUM(CASE WHEN l.reconciliation_status = 'Failed' THEN 1 ELSE 0 END)    AS recon_failed_lines,
        SUM(COALESCE(l.retry_count, 0))                                        AS total_retries,
        COUNT(DISTINCT rv.file_line_id)                                        AS reviewed_lines
    FROM lines l
    LEFT JOIN reviews rv ON rv.data_scope = l.data_scope AND rv.file_line_id = l.file_line_id
    GROUP BY l.data_scope, l.report_date, l.network
)
SELECT
    data_scope, report_date, network,
    total_lines, matched_lines, recon_failed_lines, total_retries, reviewed_lines,
    CASE WHEN total_lines > 0 THEN ROUND(matched_lines::numeric/total_lines*100, 2) ELSE 0 END        AS match_rate_pct,
    CASE WHEN total_lines > 0 THEN ROUND(recon_failed_lines::numeric/total_lines*100, 2) ELSE 0 END   AS recon_failure_rate_pct,
    CASE WHEN total_lines > 0 THEN ROUND(total_retries::numeric/total_lines, 2) ELSE 0 END            AS avg_retries_per_line,
    CASE WHEN total_lines > 0 THEN ROUND(reviewed_lines::numeric/total_lines*100, 2) ELSE 0 END       AS manual_review_rate_pct,
    CASE
        WHEN total_lines = 0 THEN 'N/A'
        WHEN matched_lines::numeric/total_lines >= 0.99
             AND recon_failed_lines::numeric/total_lines <= 0.01
             AND total_retries::numeric/total_lines <= 0.05
             AND reviewed_lines::numeric/total_lines <= 0.02                                        THEN 'A'
        WHEN matched_lines::numeric/total_lines >= 0.95
             AND recon_failed_lines::numeric/total_lines <= 0.03
             AND reviewed_lines::numeric/total_lines <= 0.05                                        THEN 'B'
        WHEN matched_lines::numeric/total_lines >= 0.90                                              THEN 'C'
        WHEN matched_lines::numeric/total_lines >= 0.80                                              THEN 'D'
        ELSE 'F'
    END                                                                                              AS quality_grade,
    CASE
        WHEN total_lines = 0 THEN 'NO_DATA'
        WHEN recon_failed_lines::numeric / NULLIF(total_lines,0) >= 0.05                             THEN 'RECON_FAILURE_RATE'
        WHEN matched_lines::numeric / NULLIF(total_lines,0) < 0.90                                   THEN 'MATCH_RATE'
        WHEN total_retries::numeric / NULLIF(total_lines,0) >= 0.5                                   THEN 'RETRY_DENSITY'
        WHEN reviewed_lines::numeric / NULLIF(total_lines,0) >= 0.10                                 THEN 'MANUAL_REVIEW_LOAD'
        ELSE 'NONE'
    END                                                                                              AS weakest_dimension,
    CASE
        WHEN data_scope = 'ARCHIVE'                                                                  THEN 'P4'
        WHEN matched_lines::numeric / NULLIF(total_lines,0) < 0.80                                   THEN 'P1'
        WHEN matched_lines::numeric / NULLIF(total_lines,0) < 0.90                                   THEN 'P2'
        WHEN recon_failed_lines::numeric / NULLIF(total_lines,0) >= 0.05                             THEN 'P2'
        ELSE 'P3'
    END                                                                                              AS urgency,
    CASE
        WHEN data_scope = 'ARCHIVE'                                                                  THEN 'HISTORICAL_TREND_ANALYSIS_ONLY'
        WHEN matched_lines::numeric / NULLIF(total_lines,0) < 0.90                                   THEN 'INVESTIGATE_LOW_MATCH_RATE_ROOT_CAUSE'
        WHEN recon_failed_lines::numeric / NULLIF(total_lines,0) >= 0.05                             THEN 'INVESTIGATE_RECON_FAILURE_PATTERN'
        WHEN total_retries::numeric / NULLIF(total_lines,0) >= 0.5                                   THEN 'TUNE_RETRY_OR_FIX_TRANSIENT_ERRORS'
        WHEN reviewed_lines::numeric / NULLIF(total_lines,0) >= 0.10                                 THEN 'REDUCE_MANUAL_REVIEW_BY_RULE_TUNING'
        ELSE 'MONITOR'
    END                                                                                              AS recommended_action
FROM agg;


-- =====================================================================================
-- rep_misleading_success_cases
-- Days where line-count match looks good but amount-match is poor (or vice versa).
-- =====================================================================================
CREATE OR REPLACE VIEW reporting.rep_misleading_success_cases AS
WITH lines AS (
    SELECT 'LIVE'::text data_scope, DATE_TRUNC('day', f.create_date)::date AS report_date,
           f.content_type AS network, f.file_type AS side,
           (fl.matched_clearing_line_id IS NOT NULL) AS is_matched,
           amt.amount, amt.currency
    FROM ingestion.file_line fl JOIN ingestion.file f ON f.id = fl.file_id
    LEFT JOIN LATERAL (
        SELECT cv.original_amount AS amount, cv.original_currency AS currency
        FROM ingestion.card_visa_detail cv WHERE cv.file_line_id = fl.id AND f.file_type='Card' AND f.content_type='Visa'
        UNION ALL SELECT cm.original_amount, cm.original_currency
        FROM ingestion.card_msc_detail cm WHERE cm.file_line_id = fl.id AND f.file_type='Card' AND f.content_type='Msc'
        UNION ALL SELECT cb.original_amount, cb.original_currency
        FROM ingestion.card_bkm_detail cb WHERE cb.file_line_id = fl.id AND f.file_type='Card' AND f.content_type='Bkm'
        UNION ALL SELECT cv.source_amount, cv.source_currency
        FROM ingestion.clearing_visa_detail cv WHERE cv.file_line_id = fl.id AND f.file_type='Clearing' AND f.content_type='Visa'
        UNION ALL SELECT cm.source_amount, cm.source_currency
        FROM ingestion.clearing_msc_detail cm WHERE cm.file_line_id = fl.id AND f.file_type='Clearing' AND f.content_type='Msc'
        UNION ALL SELECT cb.source_amount, cb.source_currency
        FROM ingestion.clearing_bkm_detail cb WHERE cb.file_line_id = fl.id AND f.file_type='Clearing' AND f.content_type='Bkm'
    ) amt ON TRUE
    UNION ALL
    SELECT 'ARCHIVE', DATE_TRUNC('day', f.create_date)::date, f.content_type, f.file_type,
           (fl.matched_clearing_line_id IS NOT NULL),
           amt.amount, amt.currency
    FROM archive.ingestion_file_line fl JOIN archive.ingestion_file f ON f.id = fl.file_id
    LEFT JOIN LATERAL (
        SELECT cv.original_amount AS amount, cv.original_currency AS currency FROM archive.ingestion_card_visa_detail cv WHERE cv.id = fl.card_visa_detail_id
        UNION ALL SELECT cm.original_amount, cm.original_currency FROM archive.ingestion_card_msc_detail cm WHERE cm.id = fl.card_msc_detail_id
        UNION ALL SELECT cb.original_amount, cb.original_currency FROM archive.ingestion_card_bkm_detail cb WHERE cb.id = fl.card_bkm_detail_id
        UNION ALL SELECT cv.source_amount, cv.source_currency FROM archive.ingestion_clearing_visa_detail cv WHERE cv.id = fl.clearing_visa_detail_id
        UNION ALL SELECT cm.source_amount, cm.source_currency FROM archive.ingestion_clearing_msc_detail cm WHERE cm.id = fl.clearing_msc_detail_id
        UNION ALL SELECT cb.source_amount, cb.source_currency FROM archive.ingestion_clearing_bkm_detail cb WHERE cb.id = fl.clearing_bkm_detail_id
    ) amt ON TRUE
), agg AS (
    SELECT data_scope, report_date, network, side, COALESCE(currency::text, 'UNKNOWN') AS currency,
           COUNT(*)                                                                         AS line_count,
           SUM(CASE WHEN is_matched THEN 1 ELSE 0 END)                                       AS matched_count,
           SUM(COALESCE(amount, 0))                                                          AS total_amount,
           SUM(CASE WHEN is_matched THEN COALESCE(amount, 0) ELSE 0 END)                     AS matched_amount,
           SUM(CASE WHEN NOT is_matched THEN COALESCE(amount, 0) ELSE 0 END)                 AS unmatched_amount
    FROM lines
    GROUP BY data_scope, report_date, network, side, COALESCE(currency::text, 'UNKNOWN')
)
SELECT
    data_scope, report_date, network, side, currency,
    line_count, matched_count, total_amount, matched_amount, unmatched_amount,
    CASE WHEN line_count > 0    THEN ROUND(matched_count::numeric/line_count*100, 2)    ELSE 0 END AS count_match_rate_pct,
    CASE WHEN total_amount > 0  THEN ROUND(matched_amount::numeric/total_amount*100, 2) ELSE 0 END AS amount_match_rate_pct,
    CASE
        WHEN line_count > 0 AND total_amount > 0
            AND matched_count::numeric/line_count >= 0.95
            AND matched_amount::numeric/total_amount < 0.80                                 THEN 'GOOD_COUNT_BAD_AMOUNT'
        WHEN line_count > 0 AND total_amount > 0
            AND matched_amount::numeric/total_amount >= 0.95
            AND matched_count::numeric/line_count < 0.80                                    THEN 'GOOD_AMOUNT_BAD_COUNT'
        ELSE 'CONSISTENT'
    END                                                                                     AS misleading_pattern,
    CASE
        WHEN data_scope = 'ARCHIVE'                                                          THEN 'P4'
        WHEN line_count > 0 AND total_amount > 0
            AND matched_count::numeric/line_count >= 0.95
            AND matched_amount::numeric/total_amount < 0.80                                 THEN 'P1'
        WHEN line_count > 0 AND total_amount > 0
            AND matched_amount::numeric/total_amount >= 0.95
            AND matched_count::numeric/line_count < 0.80                                    THEN 'P2'
        ELSE 'P5'
    END                                                                                     AS urgency,
    CASE
        WHEN data_scope = 'ARCHIVE'                                                          THEN 'HISTORICAL_TREND_ANALYSIS_ONLY'
        WHEN line_count > 0 AND total_amount > 0
            AND matched_count::numeric/line_count >= 0.95
            AND matched_amount::numeric/total_amount < 0.80                                 THEN 'INVESTIGATE_HIGH_VALUE_UNMATCHED_TRANSACTIONS'
        WHEN line_count > 0 AND total_amount > 0
            AND matched_amount::numeric/total_amount >= 0.95
            AND matched_count::numeric/line_count < 0.80                                    THEN 'INVESTIGATE_MANY_LOW_VALUE_UNMATCHED'
        ELSE 'NONE'
    END                                                                                     AS recommended_action
FROM agg
WHERE line_count > 0;


-- =====================================================================================
-- rep_archive_pipeline_health
-- Archive pipeline observation: failed/stuck runs (ARCHIVE scope) + eligible-but-not-archived
-- backlog (LIVE scope). Each row carries health flag + recommended action.
-- =====================================================================================
CREATE OR REPLACE VIEW reporting.rep_archive_pipeline_health AS
WITH eligible_backlog AS (
    SELECT
        'LIVE'::text                                                          AS data_scope,
        'ELIGIBLE_BACKLOG'::text                                              AS perspective,
        f.id::text                                                            AS reference_id,
        f.file_name,
        f.file_type                                                           AS side,
        f.content_type                                                        AS network,
        f.create_date                                                         AS reference_date,
        ROUND((EXTRACT(EPOCH FROM (NOW() - f.create_date)) / 86400)::numeric, 1)         AS age_days,
        NULL::text                                                            AS archive_status,
        NULL::text                                                            AS archive_message
    FROM ingestion.file f
    WHERE f.status = 'Success'
      AND f.is_archived = FALSE
      AND NOT EXISTS (SELECT 1 FROM archive.ingestion_file af WHERE af.id = f.id)
), runs AS (
    SELECT
        'ARCHIVE'::text                                                       AS data_scope,
        CASE
            WHEN l.status = 'Failed'                                          THEN 'RUN_FAILED'
            WHEN l.status NOT IN ('Success','Failed')
                AND EXTRACT(EPOCH FROM (NOW() - l.create_date)) > 3600        THEN 'RUN_STUCK'
            WHEN l.status NOT IN ('Success','Failed')                         THEN 'RUN_IN_PROGRESS'
            ELSE 'RUN_COMPLETED'
        END                                                                   AS perspective,
        l.id::text                                                            AS reference_id,
        COALESCE(lf.file_name, af.file_name)                                  AS file_name,
        COALESCE(lf.file_type, af.file_type)                                  AS side,
        COALESCE(lf.content_type, af.content_type)                            AS network,
        l.create_date                                                         AS reference_date,
        ROUND((EXTRACT(EPOCH FROM (NOW() - l.create_date)) / 86400)::numeric, 1)          AS age_days,
        l.status                                                              AS archive_status,
        l.message                                                             AS archive_message
    FROM archive.archive_log l
    LEFT JOIN ingestion.file        lf ON lf.id = l.ingestion_file_id
    LEFT JOIN archive.ingestion_file af ON af.id = l.ingestion_file_id
    WHERE l.status = 'Failed'
       OR l.status NOT IN ('Success','Failed')
)
SELECT
    data_scope, perspective, reference_id, file_name, side, network, reference_date, age_days,
    archive_status, archive_message,
    CASE
        WHEN perspective = 'RUN_FAILED'              THEN 'CRITICAL'
        WHEN perspective = 'RUN_STUCK'               THEN 'CRITICAL'
        WHEN perspective = 'ELIGIBLE_BACKLOG'
             AND age_days >= 7                       THEN 'DEGRADED'
        WHEN perspective = 'ELIGIBLE_BACKLOG'        THEN 'WARMING'
        WHEN perspective = 'RUN_IN_PROGRESS'         THEN 'WARMING'
        ELSE 'HEALTHY'
    END                                              AS pipeline_health,
    CASE
        WHEN perspective IN ('RUN_FAILED','RUN_STUCK') THEN 'P1'
        WHEN perspective = 'ELIGIBLE_BACKLOG' AND age_days >= 7 THEN 'P2'
        WHEN perspective = 'ELIGIBLE_BACKLOG'        THEN 'P3'
        WHEN perspective = 'RUN_IN_PROGRESS'         THEN 'P4'
        ELSE 'P5'
    END                                              AS urgency,
    CASE perspective
        WHEN 'RUN_FAILED'        THEN 'INVESTIGATE_AND_RETRY_ARCHIVE_RUN'
        WHEN 'RUN_STUCK'         THEN 'CHECK_STUCK_ARCHIVE_PROCESS'
        WHEN 'ELIGIBLE_BACKLOG'  THEN 'TRIGGER_ARCHIVE_FOR_BACKLOG'
        WHEN 'RUN_IN_PROGRESS'   THEN 'MONITOR_PROGRESS'
        ELSE 'NONE'
    END                                              AS recommended_action
FROM (SELECT * FROM eligible_backlog UNION ALL SELECT * FROM runs) combined;



-- =====================================================================================
-- rep_daily_transaction_volume
-- Real business-date daily volume per network/financial_type/txn_effect/currency.
-- Uses transaction_date (YYYYMMDD int) instead of file ingestion date.
-- =====================================================================================
CREATE OR REPLACE VIEW reporting.rep_daily_transaction_volume AS
WITH src AS (
    SELECT 'LIVE'::text scope, 'BKM'::text network, transaction_date, original_amount, original_currency, financial_type, txn_effect FROM ingestion.card_bkm_detail
    UNION ALL SELECT 'LIVE','VISA', transaction_date, original_amount, original_currency, financial_type, txn_effect FROM ingestion.card_visa_detail
    UNION ALL SELECT 'LIVE','MSC',  transaction_date, original_amount, original_currency, financial_type, txn_effect FROM ingestion.card_msc_detail
    UNION ALL SELECT 'ARCHIVE','BKM', transaction_date, original_amount, original_currency, financial_type, txn_effect FROM archive.ingestion_card_bkm_detail
    UNION ALL SELECT 'ARCHIVE','VISA',transaction_date, original_amount, original_currency, financial_type, txn_effect FROM archive.ingestion_card_visa_detail
    UNION ALL SELECT 'ARCHIVE','MSC', transaction_date, original_amount, original_currency, financial_type, txn_effect FROM archive.ingestion_card_msc_detail
)
SELECT
    scope AS data_scope,
    CASE WHEN transaction_date BETWEEN 19000101 AND 99991231 THEN to_date(transaction_date::text, 'YYYYMMDD') END AS transaction_date,
    network,
    original_currency::text AS currency,
    financial_type,
    txn_effect,
    COUNT(*)::bigint                                                            AS transaction_count,
    SUM(original_amount)                                                        AS total_amount,
    SUM(CASE WHEN txn_effect = 'D' THEN original_amount ELSE 0 END)             AS debit_amount,
    SUM(CASE WHEN txn_effect = 'C' THEN original_amount ELSE 0 END)             AS credit_amount,
    SUM(CASE WHEN txn_effect = 'C' THEN original_amount ELSE -original_amount END) AS net_flow_amount,
    ROUND(AVG(original_amount), 2)                                              AS avg_amount,
    MAX(original_amount)                                                        AS max_amount,
    CASE
        WHEN scope = 'ARCHIVE' THEN 'HISTORICAL'
        WHEN ABS(SUM(CASE WHEN txn_effect = 'C' THEN original_amount ELSE -original_amount END)) >= 1000000 THEN 'MATERIAL_NET_FLOW'
        ELSE 'NORMAL'
    END                                                                          AS volume_flag,
    CASE
        WHEN scope = 'ARCHIVE' THEN 'P5'
        WHEN ABS(SUM(CASE WHEN txn_effect = 'C' THEN original_amount ELSE -original_amount END)) >= 1000000 THEN 'P3'
        ELSE 'P5'
    END                                                                          AS urgency,
    CASE
        WHEN scope = 'ARCHIVE' THEN 'HISTORICAL_TREND_ANALYSIS_ONLY'
        WHEN ABS(SUM(CASE WHEN txn_effect = 'C' THEN original_amount ELSE -original_amount END)) >= 1000000 THEN 'INVESTIGATE_LARGE_NET_FLOW'
        ELSE 'MONITOR_DAILY_VOLUME_TREND'
    END                                                                          AS recommended_action
FROM src
GROUP BY scope,
         CASE WHEN transaction_date BETWEEN 19000101 AND 99991231 THEN to_date(transaction_date::text, 'YYYYMMDD') END,
         network, original_currency, financial_type, txn_effect;


-- =====================================================================================
-- rep_mcc_revenue_concentration
-- Per MCC concentration: volume share, tax/surcharge/cashback economics, risk flag.
-- =====================================================================================
CREATE OR REPLACE VIEW reporting.rep_mcc_revenue_concentration AS
WITH src AS (
    SELECT 'LIVE'::text scope, 'BKM'::text network, mcc, original_amount, tax1, tax2, surcharge_amount, cashback_amount FROM ingestion.card_bkm_detail
    UNION ALL SELECT 'LIVE','VISA', mcc, original_amount, tax1, tax2, surcharge_amount, cashback_amount FROM ingestion.card_visa_detail
    UNION ALL SELECT 'LIVE','MSC',  mcc, original_amount, tax1, tax2, surcharge_amount, cashback_amount FROM ingestion.card_msc_detail
    UNION ALL SELECT 'ARCHIVE','BKM', mcc, original_amount, tax1, tax2, surcharge_amount, cashback_amount FROM archive.ingestion_card_bkm_detail
    UNION ALL SELECT 'ARCHIVE','VISA',mcc, original_amount, tax1, tax2, surcharge_amount, cashback_amount FROM archive.ingestion_card_visa_detail
    UNION ALL SELECT 'ARCHIVE','MSC', mcc, original_amount, tax1, tax2, surcharge_amount, cashback_amount FROM archive.ingestion_card_msc_detail
), agg AS (
    SELECT scope, network, mcc::text AS mcc,
           COUNT(*) AS transaction_count,
           SUM(original_amount) AS total_amount,
           SUM(tax1 + tax2) AS total_tax_amount,
           SUM(surcharge_amount) AS total_surcharge_amount,
           SUM(cashback_amount) AS total_cashback_amount
    FROM src GROUP BY scope, network, mcc
)
SELECT
    scope AS data_scope, network, mcc,
    transaction_count, total_amount, total_tax_amount, total_surcharge_amount, total_cashback_amount,
    ROUND(total_amount::numeric * 100 / NULLIF(SUM(total_amount) OVER (PARTITION BY scope, network), 0), 2) AS volume_share_pct,
    CASE
        WHEN scope = 'ARCHIVE' THEN 'HISTORICAL'
        WHEN total_amount::numeric / NULLIF(SUM(total_amount) OVER (PARTITION BY scope, network), 0) >= 0.30 THEN 'HIGH_CONCENTRATION'
        WHEN total_amount::numeric / NULLIF(SUM(total_amount) OVER (PARTITION BY scope, network), 0) >= 0.15 THEN 'NOTABLE_CONCENTRATION'
        ELSE 'DIVERSIFIED'
    END AS concentration_risk,
    CASE
        WHEN scope = 'ARCHIVE' THEN 'P5'
        WHEN total_amount::numeric / NULLIF(SUM(total_amount) OVER (PARTITION BY scope, network), 0) >= 0.30 THEN 'P2'
        WHEN total_amount::numeric / NULLIF(SUM(total_amount) OVER (PARTITION BY scope, network), 0) >= 0.15 THEN 'P3'
        ELSE 'P5'
    END AS urgency,
    CASE
        WHEN scope = 'ARCHIVE' THEN 'HISTORICAL_TREND_ANALYSIS_ONLY'
        WHEN total_amount::numeric / NULLIF(SUM(total_amount) OVER (PARTITION BY scope, network), 0) >= 0.30 THEN 'REVIEW_MCC_CONCENTRATION_RISK'
        WHEN total_amount::numeric / NULLIF(SUM(total_amount) OVER (PARTITION BY scope, network), 0) >= 0.15 THEN 'MONITOR_MCC_DEPENDENCY'
        ELSE 'NONE'
    END AS recommended_action
FROM agg;


-- =====================================================================================
-- rep_merchant_risk_hotspots
-- Per merchant: decline rate + unmatched rate + financial exposure with risk flag.
-- =====================================================================================
CREATE OR REPLACE VIEW reporting.rep_merchant_risk_hotspots AS
WITH src AS (
    SELECT 'LIVE'::text scope, 'BKM'::text network,
           d.merchant_id, d.merchant_name, d.merchant_country, d.original_amount, d.is_successful_txn, d.response_code,
           (fl.matched_clearing_line_id IS NULL) AS is_unmatched
    FROM ingestion.card_bkm_detail d JOIN ingestion.file_line fl ON fl.id = d.file_line_id
    UNION ALL
    SELECT 'LIVE','VISA', d.merchant_id, d.merchant_name, d.merchant_country, d.original_amount, d.is_successful_txn, d.response_code, fl.matched_clearing_line_id IS NULL
    FROM ingestion.card_visa_detail d JOIN ingestion.file_line fl ON fl.id = d.file_line_id
    UNION ALL
    SELECT 'LIVE','MSC',  d.merchant_id, d.merchant_name, d.merchant_country, d.original_amount, d.is_successful_txn, d.response_code, fl.matched_clearing_line_id IS NULL
    FROM ingestion.card_msc_detail d JOIN ingestion.file_line fl ON fl.id = d.file_line_id
    UNION ALL
    SELECT 'ARCHIVE','BKM', d.merchant_id, d.merchant_name, d.merchant_country, d.original_amount, d.is_successful_txn, d.response_code, fl.matched_clearing_line_id IS NULL
    FROM archive.ingestion_card_bkm_detail d JOIN archive.ingestion_file_line fl ON fl.id = d.file_line_id
    UNION ALL
    SELECT 'ARCHIVE','VISA', d.merchant_id, d.merchant_name, d.merchant_country, d.original_amount, d.is_successful_txn, d.response_code, fl.matched_clearing_line_id IS NULL
    FROM archive.ingestion_card_visa_detail d JOIN archive.ingestion_file_line fl ON fl.id = d.file_line_id
    UNION ALL
    SELECT 'ARCHIVE','MSC', d.merchant_id, d.merchant_name, d.merchant_country, d.original_amount, d.is_successful_txn, d.response_code, fl.matched_clearing_line_id IS NULL
    FROM archive.ingestion_card_msc_detail d JOIN archive.ingestion_file_line fl ON fl.id = d.file_line_id
), agg AS (
    SELECT scope, network,
           COALESCE(merchant_id, 'UNKNOWN')      AS merchant_id,
           COALESCE(merchant_name, 'UNKNOWN')    AS merchant_name,
           COALESCE(merchant_country, 'UNKNOWN') AS merchant_country,
           COUNT(*)                                                                                                            AS transaction_count,
           COUNT(*) FILTER (WHERE is_successful_txn = 'N' OR (response_code IS NOT NULL AND response_code <> '00'))             AS declined_count,
           COUNT(*) FILTER (WHERE is_unmatched)                                                                                  AS unmatched_count,
           SUM(original_amount)                                                                                                  AS total_amount,
           SUM(CASE WHEN is_unmatched THEN original_amount ELSE 0 END)                                                           AS unmatched_amount
    FROM src
    GROUP BY scope, network, COALESCE(merchant_id, 'UNKNOWN'), COALESCE(merchant_name, 'UNKNOWN'), COALESCE(merchant_country, 'UNKNOWN')
)
SELECT
    scope AS data_scope, network, merchant_id, merchant_name, merchant_country,
    transaction_count, declined_count, unmatched_count, total_amount, unmatched_amount,
    CASE WHEN transaction_count > 0 THEN ROUND(declined_count::numeric / transaction_count * 100, 2) ELSE 0 END  AS decline_rate_pct,
    CASE WHEN transaction_count > 0 THEN ROUND(unmatched_count::numeric / transaction_count * 100, 2) ELSE 0 END AS unmatched_rate_pct,
    CASE
        WHEN scope = 'ARCHIVE' THEN 'HISTORICAL'
        WHEN unmatched_count > 0 AND unmatched_count::numeric / NULLIF(transaction_count,0) >= 0.20 AND unmatched_amount >= 100000 THEN 'HIGH_RISK_MERCHANT'
        WHEN declined_count::numeric / NULLIF(transaction_count,0) >= 0.30 THEN 'HIGH_DECLINE_MERCHANT'
        WHEN unmatched_count > 0 THEN 'NEEDS_INVESTIGATION'
        ELSE 'HEALTHY'
    END AS risk_flag,
    CASE
        WHEN scope = 'ARCHIVE' THEN 'P5'
        WHEN unmatched_count > 0 AND unmatched_count::numeric / NULLIF(transaction_count,0) >= 0.20 AND unmatched_amount >= 100000 THEN 'P1'
        WHEN declined_count::numeric / NULLIF(transaction_count,0) >= 0.30 THEN 'P2'
        WHEN unmatched_count > 0 THEN 'P3'
        ELSE 'P5'
    END AS urgency,
    CASE
        WHEN scope = 'ARCHIVE' THEN 'HISTORICAL_TREND_ANALYSIS_ONLY'
        WHEN unmatched_count > 0 AND unmatched_count::numeric / NULLIF(transaction_count,0) >= 0.20 AND unmatched_amount >= 100000 THEN 'ESCALATE_TO_RISK_TEAM'
        WHEN declined_count::numeric / NULLIF(transaction_count,0) >= 0.30 THEN 'INVESTIGATE_DECLINE_PATTERN'
        WHEN unmatched_count > 0 THEN 'INVESTIGATE_UNMATCHED'
        ELSE 'NONE'
    END AS recommended_action
FROM agg;


-- =====================================================================================
-- rep_country_cross_border_exposure
-- Per merchant_country and FX pattern (original currency vs settlement currency).
-- =====================================================================================
CREATE OR REPLACE VIEW reporting.rep_country_cross_border_exposure AS
WITH src AS (
    SELECT 'LIVE'::text scope, 'BKM'::text network, merchant_country, original_amount, original_currency, settlement_currency FROM ingestion.card_bkm_detail
    UNION ALL SELECT 'LIVE','VISA', merchant_country, original_amount, original_currency, settlement_currency FROM ingestion.card_visa_detail
    UNION ALL SELECT 'LIVE','MSC',  merchant_country, original_amount, original_currency, settlement_currency FROM ingestion.card_msc_detail
    UNION ALL SELECT 'ARCHIVE','BKM', merchant_country, original_amount, original_currency, settlement_currency FROM archive.ingestion_card_bkm_detail
    UNION ALL SELECT 'ARCHIVE','VISA',merchant_country, original_amount, original_currency, settlement_currency FROM archive.ingestion_card_visa_detail
    UNION ALL SELECT 'ARCHIVE','MSC', merchant_country, original_amount, original_currency, settlement_currency FROM archive.ingestion_card_msc_detail
), agg AS (
    SELECT scope, network,
           COALESCE(merchant_country, 'UNKNOWN') AS merchant_country,
           CASE WHEN original_currency <> settlement_currency THEN 'CROSS_CURRENCY' ELSE 'SAME_CURRENCY' END AS fx_pattern,
           original_currency::text  AS original_currency,
           settlement_currency::text AS settlement_currency,
           COUNT(*)                  AS transaction_count,
           SUM(original_amount)      AS total_original_amount
    FROM src
    GROUP BY scope, network,
             COALESCE(merchant_country, 'UNKNOWN'),
             CASE WHEN original_currency <> settlement_currency THEN 'CROSS_CURRENCY' ELSE 'SAME_CURRENCY' END,
             original_currency, settlement_currency
)
SELECT
    scope AS data_scope, network, merchant_country, fx_pattern, original_currency, settlement_currency,
    transaction_count, total_original_amount,
    CASE
        WHEN scope = 'ARCHIVE' THEN 'HISTORICAL'
        WHEN fx_pattern = 'CROSS_CURRENCY' AND total_original_amount >= 1000000 THEN 'HIGH_FX_EXPOSURE'
        WHEN fx_pattern = 'CROSS_CURRENCY'                                       THEN 'FX_EXPOSURE'
        ELSE 'DOMESTIC'
    END AS exposure_flag,
    CASE
        WHEN scope = 'ARCHIVE' THEN 'P5'
        WHEN fx_pattern = 'CROSS_CURRENCY' AND total_original_amount >= 1000000 THEN 'P2'
        WHEN fx_pattern = 'CROSS_CURRENCY'                                       THEN 'P3'
        ELSE 'P5'
    END AS urgency,
    CASE
        WHEN scope = 'ARCHIVE' THEN 'HISTORICAL_TREND_ANALYSIS_ONLY'
        WHEN fx_pattern = 'CROSS_CURRENCY' AND total_original_amount >= 1000000 THEN 'HEDGE_OR_REVIEW_FX_EXPOSURE'
        WHEN fx_pattern = 'CROSS_CURRENCY'                                       THEN 'MONITOR_FX_EXPOSURE'
        ELSE 'NONE'
    END AS recommended_action
FROM agg;


-- =====================================================================================
-- rep_response_code_decline_health
-- Response_code distribution per network with dominant-failure-reason flag.
-- =====================================================================================
CREATE OR REPLACE VIEW reporting.rep_response_code_decline_health AS
WITH src AS (
    SELECT 'LIVE'::text scope, 'BKM'::text network, response_code, is_successful_txn, original_amount FROM ingestion.card_bkm_detail
    UNION ALL SELECT 'LIVE','VISA', response_code, is_successful_txn, original_amount FROM ingestion.card_visa_detail
    UNION ALL SELECT 'LIVE','MSC',  response_code, is_successful_txn, original_amount FROM ingestion.card_msc_detail
    UNION ALL SELECT 'ARCHIVE','BKM', response_code, is_successful_txn, original_amount FROM archive.ingestion_card_bkm_detail
    UNION ALL SELECT 'ARCHIVE','VISA',response_code, is_successful_txn, original_amount FROM archive.ingestion_card_visa_detail
    UNION ALL SELECT 'ARCHIVE','MSC', response_code, is_successful_txn, original_amount FROM archive.ingestion_card_msc_detail
), agg AS (
    SELECT scope, network, COALESCE(response_code, 'NONE') AS response_code,
           COUNT(*) AS transaction_count, SUM(original_amount) AS total_amount,
           COUNT(*) FILTER (WHERE is_successful_txn = 'Y') AS successful_count,
           COUNT(*) FILTER (WHERE is_successful_txn = 'N') AS failed_count
    FROM src
    GROUP BY scope, network, COALESCE(response_code, 'NONE')
)
SELECT
    scope AS data_scope, network, response_code,
    transaction_count, total_amount, successful_count, failed_count,
    CASE WHEN transaction_count > 0 THEN ROUND(failed_count::numeric / transaction_count * 100, 2) ELSE 0 END AS failure_rate_pct,
    ROUND(transaction_count::numeric * 100 / NULLIF(SUM(transaction_count) OVER (PARTITION BY scope, network), 0), 2) AS network_share_pct,
    CASE
        WHEN scope = 'ARCHIVE' THEN 'HISTORICAL'
        WHEN response_code NOT IN ('00','NONE')
             AND transaction_count::numeric / NULLIF(SUM(transaction_count) OVER (PARTITION BY scope, network), 0) >= 0.05 THEN 'DOMINANT_FAILURE_REASON'
        WHEN response_code NOT IN ('00','NONE') THEN 'NORMAL_FAILURE'
        ELSE 'SUCCESS_OR_UNKNOWN'
    END AS health_flag,
    CASE
        WHEN scope = 'ARCHIVE' THEN 'P5'
        WHEN response_code NOT IN ('00','NONE')
             AND transaction_count::numeric / NULLIF(SUM(transaction_count) OVER (PARTITION BY scope, network), 0) >= 0.05 THEN 'P2'
        ELSE 'P5'
    END AS urgency,
    CASE
        WHEN scope = 'ARCHIVE' THEN 'HISTORICAL_TREND_ANALYSIS_ONLY'
        WHEN response_code NOT IN ('00','NONE')
             AND transaction_count::numeric / NULLIF(SUM(transaction_count) OVER (PARTITION BY scope, network), 0) >= 0.05 THEN 'INVESTIGATE_DECLINE_REASON'
        ELSE 'NONE'
    END AS recommended_action
FROM agg;


-- =====================================================================================
-- rep_settlement_lag_analysis
-- Daily lag between transaction_date and value_date / end_of_day_date / file ingest date.
-- Highlights chronic ingestion delays.
-- =====================================================================================
CREATE OR REPLACE VIEW reporting.rep_settlement_lag_analysis AS
WITH src AS (
    SELECT 'LIVE'::text scope, 'BKM'::text network, transaction_date, end_of_day_date, value_date, create_date, original_amount FROM ingestion.card_bkm_detail
    UNION ALL SELECT 'LIVE','VISA', transaction_date, end_of_day_date, value_date, create_date, original_amount FROM ingestion.card_visa_detail
    UNION ALL SELECT 'LIVE','MSC',  transaction_date, end_of_day_date, value_date, create_date, original_amount FROM ingestion.card_msc_detail
    UNION ALL SELECT 'ARCHIVE','BKM', transaction_date, end_of_day_date, value_date, create_date, original_amount FROM archive.ingestion_card_bkm_detail
    UNION ALL SELECT 'ARCHIVE','VISA',transaction_date, end_of_day_date, value_date, create_date, original_amount FROM archive.ingestion_card_visa_detail
    UNION ALL SELECT 'ARCHIVE','MSC', transaction_date, end_of_day_date, value_date, create_date, original_amount FROM archive.ingestion_card_msc_detail
), enriched AS (
    SELECT scope, network, original_amount,
           CASE WHEN transaction_date BETWEEN 19000101 AND 99991231 THEN to_date(transaction_date::text,'YYYYMMDD') END AS txn_dt,
           CASE WHEN end_of_day_date  BETWEEN 19000101 AND 99991231 THEN to_date(end_of_day_date::text,'YYYYMMDD')  END AS eod_dt,
           CASE WHEN value_date       BETWEEN 19000101 AND 99991231 THEN to_date(value_date::text,'YYYYMMDD')       END AS val_dt,
           create_date::date AS ingest_dt
    FROM src
)
SELECT
    scope AS data_scope, network, txn_dt AS transaction_date,
    COUNT(*)::bigint                                                                  AS transaction_count,
    SUM(original_amount)                                                              AS total_amount,
    ROUND(AVG((eod_dt - txn_dt))::numeric, 2)                                         AS avg_lag_to_eod_days,
    ROUND(AVG((val_dt - txn_dt))::numeric, 2)                                         AS avg_lag_to_value_days,
    ROUND(AVG((ingest_dt - txn_dt))::numeric, 2)                                      AS avg_lag_to_ingest_days,
    MAX(ingest_dt - txn_dt)                                                           AS max_lag_to_ingest_days,
    COUNT(*) FILTER (WHERE (ingest_dt - txn_dt) >= 3)::bigint                          AS late_ingest_count,
    CASE
        WHEN scope = 'ARCHIVE' THEN 'HISTORICAL'
        WHEN AVG(ingest_dt - txn_dt) >= 3 THEN 'CHRONIC_INGEST_DELAY'
        WHEN MAX(ingest_dt - txn_dt) >= 5 THEN 'SPORADIC_LATE_INGEST'
        ELSE 'TIMELY'
    END                                                                                AS lag_health,
    CASE
        WHEN scope = 'ARCHIVE' THEN 'P5'
        WHEN AVG(ingest_dt - txn_dt) >= 3 THEN 'P2'
        WHEN MAX(ingest_dt - txn_dt) >= 5 THEN 'P3'
        ELSE 'P5'
    END                                                                                AS urgency,
    CASE
        WHEN scope = 'ARCHIVE' THEN 'HISTORICAL_TREND_ANALYSIS_ONLY'
        WHEN AVG(ingest_dt - txn_dt) >= 3 THEN 'INVESTIGATE_INGESTION_PIPELINE_DELAY'
        WHEN MAX(ingest_dt - txn_dt) >= 5 THEN 'CHECK_OUTLIER_LATE_TRANSACTIONS'
        ELSE 'NONE'
    END                                                                                AS recommended_action
FROM enriched
WHERE txn_dt IS NOT NULL
GROUP BY scope, network, txn_dt;


-- =====================================================================================
-- rep_currency_fx_drift
-- Cross-currency transactions: aggregate drift between original/settlement/billing.
-- Surfaces FX gain/loss and inconsistent settlement logic.
-- =====================================================================================
CREATE OR REPLACE VIEW reporting.rep_currency_fx_drift AS
WITH src AS (
    SELECT 'LIVE'::text scope, 'BKM'::text network, original_amount, original_currency, settlement_amount, settlement_currency, billing_amount, billing_currency FROM ingestion.card_bkm_detail
    UNION ALL SELECT 'LIVE','VISA', original_amount, original_currency, settlement_amount, settlement_currency, billing_amount, billing_currency FROM ingestion.card_visa_detail
    UNION ALL SELECT 'LIVE','MSC',  original_amount, original_currency, settlement_amount, settlement_currency, billing_amount, billing_currency FROM ingestion.card_msc_detail
    UNION ALL SELECT 'ARCHIVE','BKM', original_amount, original_currency, settlement_amount, settlement_currency, billing_amount, billing_currency FROM archive.ingestion_card_bkm_detail
    UNION ALL SELECT 'ARCHIVE','VISA',original_amount, original_currency, settlement_amount, settlement_currency, billing_amount, billing_currency FROM archive.ingestion_card_visa_detail
    UNION ALL SELECT 'ARCHIVE','MSC', original_amount, original_currency, settlement_amount, settlement_currency, billing_amount, billing_currency FROM archive.ingestion_card_msc_detail
)
SELECT
    scope AS data_scope, network,
    original_currency::text   AS original_currency,
    settlement_currency::text AS settlement_currency,
    billing_currency::text    AS billing_currency,
    COUNT(*)::bigint                                              AS transaction_count,
    SUM(original_amount)                                          AS total_original_amount,
    SUM(settlement_amount)                                        AS total_settlement_amount,
    SUM(billing_amount)                                           AS total_billing_amount,
    SUM(settlement_amount - original_amount)                      AS settlement_drift,
    SUM(billing_amount - original_amount)                         AS billing_drift,
    CASE
        WHEN scope = 'ARCHIVE' THEN 'HISTORICAL'
        WHEN ABS(SUM(settlement_amount - original_amount)) >= 100000 THEN 'MATERIAL_DRIFT'
        WHEN ABS(SUM(settlement_amount - original_amount)) >= 10000  THEN 'NOTABLE_DRIFT'
        ELSE 'INSIGNIFICANT'
    END AS fx_drift_severity,
    CASE
        WHEN scope = 'ARCHIVE' THEN 'P5'
        WHEN ABS(SUM(settlement_amount - original_amount)) >= 100000 THEN 'P2'
        WHEN ABS(SUM(settlement_amount - original_amount)) >= 10000  THEN 'P3'
        ELSE 'P5'
    END AS urgency,
    CASE
        WHEN scope = 'ARCHIVE' THEN 'HISTORICAL_TREND_ANALYSIS_ONLY'
        WHEN ABS(SUM(settlement_amount - original_amount)) >= 100000 THEN 'REVIEW_FX_RATES_AND_SETTLEMENT_LOGIC'
        WHEN ABS(SUM(settlement_amount - original_amount)) >= 10000  THEN 'MONITOR_FX_DRIFT'
        ELSE 'NONE'
    END AS recommended_action
FROM src
WHERE original_currency <> settlement_currency
GROUP BY scope, network, original_currency, settlement_currency, billing_currency;


-- =====================================================================================
-- rep_installment_portfolio_summary
-- Installment buckets per network: volume share + amount share + long-term flag.
-- =====================================================================================
CREATE OR REPLACE VIEW reporting.rep_installment_portfolio_summary AS
WITH src AS (
    SELECT 'LIVE'::text scope, 'BKM'::text network, install_count, original_amount FROM ingestion.card_bkm_detail
    UNION ALL SELECT 'LIVE','VISA', install_count, original_amount FROM ingestion.card_visa_detail
    UNION ALL SELECT 'LIVE','MSC',  install_count, original_amount FROM ingestion.card_msc_detail
    UNION ALL SELECT 'ARCHIVE','BKM', install_count, original_amount FROM archive.ingestion_card_bkm_detail
    UNION ALL SELECT 'ARCHIVE','VISA',install_count, original_amount FROM archive.ingestion_card_visa_detail
    UNION ALL SELECT 'ARCHIVE','MSC', install_count, original_amount FROM archive.ingestion_card_msc_detail
), bucketed AS (
    SELECT scope, network,
           CASE
               WHEN install_count <= 1            THEN '1_SINGLE'
               WHEN install_count BETWEEN 2 AND 3 THEN '2-3'
               WHEN install_count BETWEEN 4 AND 6 THEN '4-6'
               WHEN install_count BETWEEN 7 AND 12 THEN '7-12'
               ELSE '13+'
           END AS installment_bucket,
           original_amount
    FROM src
), agg AS (
    SELECT scope, network, installment_bucket,
           COUNT(*) AS transaction_count, SUM(original_amount) AS total_amount, AVG(original_amount) AS avg_amount
    FROM bucketed
    GROUP BY scope, network, installment_bucket
)
SELECT
    scope AS data_scope, network, installment_bucket,
    transaction_count, total_amount, ROUND(avg_amount, 2) AS avg_amount,
    ROUND(transaction_count::numeric * 100 / NULLIF(SUM(transaction_count) OVER (PARTITION BY scope, network), 0), 2) AS volume_share_pct,
    ROUND(total_amount::numeric  * 100 / NULLIF(SUM(total_amount)  OVER (PARTITION BY scope, network), 0), 2)         AS amount_share_pct,
    CASE
        WHEN scope = 'ARCHIVE' THEN 'HISTORICAL'
        WHEN installment_bucket = '13+'
             AND total_amount::numeric / NULLIF(SUM(total_amount) OVER (PARTITION BY scope, network), 0) >= 0.10 THEN 'HIGH_LONG_TERM_INSTALLMENT_EXPOSURE'
        WHEN installment_bucket IN ('7-12','13+')
             AND total_amount::numeric / NULLIF(SUM(total_amount) OVER (PARTITION BY scope, network), 0) >= 0.20 THEN 'NOTABLE_LONG_TERM_EXPOSURE'
        ELSE 'NORMAL'
    END AS portfolio_flag,
    CASE
        WHEN scope = 'ARCHIVE' THEN 'P5'
        WHEN installment_bucket = '13+'
             AND total_amount::numeric / NULLIF(SUM(total_amount) OVER (PARTITION BY scope, network), 0) >= 0.10 THEN 'P3'
        ELSE 'P5'
    END AS urgency,
    CASE
        WHEN scope = 'ARCHIVE' THEN 'HISTORICAL_TREND_ANALYSIS_ONLY'
        WHEN installment_bucket = '13+'
             AND total_amount::numeric / NULLIF(SUM(total_amount) OVER (PARTITION BY scope, network), 0) >= 0.10 THEN 'REVIEW_LONG_TERM_INSTALLMENT_RISK'
        ELSE 'NONE'
    END AS recommended_action
FROM agg;


-- =====================================================================================
-- rep_loyalty_points_economy
-- Daily loyalty cost (bc/mc/cc point amounts) vs original transaction amount.
-- Surfaces days where loyalty subsidy is unusually high.
-- =====================================================================================
CREATE OR REPLACE VIEW reporting.rep_loyalty_points_economy AS
WITH src AS (
    SELECT 'LIVE'::text scope, 'BKM'::text network, transaction_date, original_amount, bc_point_amount, mc_point_amount, cc_point_amount FROM ingestion.card_bkm_detail
    UNION ALL SELECT 'LIVE','VISA', transaction_date, original_amount, bc_point_amount, mc_point_amount, cc_point_amount FROM ingestion.card_visa_detail
    UNION ALL SELECT 'LIVE','MSC',  transaction_date, original_amount, bc_point_amount, mc_point_amount, cc_point_amount FROM ingestion.card_msc_detail
    UNION ALL SELECT 'ARCHIVE','BKM', transaction_date, original_amount, bc_point_amount, mc_point_amount, cc_point_amount FROM archive.ingestion_card_bkm_detail
    UNION ALL SELECT 'ARCHIVE','VISA',transaction_date, original_amount, bc_point_amount, mc_point_amount, cc_point_amount FROM archive.ingestion_card_visa_detail
    UNION ALL SELECT 'ARCHIVE','MSC', transaction_date, original_amount, bc_point_amount, mc_point_amount, cc_point_amount FROM archive.ingestion_card_msc_detail
)
SELECT
    scope AS data_scope, network,
    CASE WHEN transaction_date BETWEEN 19000101 AND 99991231 THEN to_date(transaction_date::text,'YYYYMMDD') END AS transaction_date,
    COUNT(*)::bigint                                                            AS transaction_count,
    SUM(original_amount)                                                        AS total_original_amount,
    SUM(bc_point_amount)                                                        AS total_bc_point_amount,
    SUM(mc_point_amount)                                                        AS total_mc_point_amount,
    SUM(cc_point_amount)                                                        AS total_cc_point_amount,
    SUM(bc_point_amount + mc_point_amount + cc_point_amount)                    AS total_loyalty_amount,
    CASE WHEN SUM(original_amount) > 0
         THEN ROUND(SUM(bc_point_amount + mc_point_amount + cc_point_amount) / SUM(original_amount) * 100, 2)
         ELSE 0 END                                                             AS loyalty_to_amount_ratio_pct,
    CASE
        WHEN scope = 'ARCHIVE' THEN 'HISTORICAL'
        WHEN SUM(original_amount) > 0
             AND SUM(bc_point_amount + mc_point_amount + cc_point_amount) / SUM(original_amount) >= 0.10 THEN 'HIGH_LOYALTY_USAGE'
        WHEN SUM(original_amount) > 0
             AND SUM(bc_point_amount + mc_point_amount + cc_point_amount) / SUM(original_amount) >= 0.05 THEN 'NOTABLE_LOYALTY_USAGE'
        ELSE 'NORMAL'
    END                                                                          AS loyalty_intensity,
    CASE
        WHEN scope = 'ARCHIVE' THEN 'P5'
        WHEN SUM(original_amount) > 0
             AND SUM(bc_point_amount + mc_point_amount + cc_point_amount) / SUM(original_amount) >= 0.10 THEN 'P3'
        ELSE 'P5'
    END                                                                          AS urgency,
    CASE
        WHEN scope = 'ARCHIVE' THEN 'HISTORICAL_TREND_ANALYSIS_ONLY'
        WHEN SUM(original_amount) > 0
             AND SUM(bc_point_amount + mc_point_amount + cc_point_amount) / SUM(original_amount) >= 0.10 THEN 'REVIEW_LOYALTY_PROGRAM_COST'
        ELSE 'MONITOR'
    END                                                                          AS recommended_action
FROM src
GROUP BY scope, network,
         CASE WHEN transaction_date BETWEEN 19000101 AND 99991231 THEN to_date(transaction_date::text,'YYYYMMDD') END;


-- =====================================================================================
-- rep_clearing_dispute_summary
-- Clearing disputes: per (network, dispute_code, reason_code, control_stat).
-- Aggregates source amount and reimbursement exposure.
-- =====================================================================================
CREATE OR REPLACE VIEW reporting.rep_clearing_dispute_summary AS
WITH src AS (
    SELECT 'LIVE'::text scope, 'BKM'::text network, dispute_code, reason_code, control_stat, source_amount, reimbursement_amount, txn_date FROM ingestion.clearing_bkm_detail
    UNION ALL SELECT 'LIVE','VISA', dispute_code, reason_code, control_stat, source_amount, reimbursement_amount, txn_date FROM ingestion.clearing_visa_detail
    UNION ALL SELECT 'LIVE','MSC',  dispute_code, reason_code, control_stat, source_amount, reimbursement_amount, txn_date FROM ingestion.clearing_msc_detail
    UNION ALL SELECT 'ARCHIVE','BKM', dispute_code, reason_code, control_stat, source_amount, reimbursement_amount, txn_date FROM archive.ingestion_clearing_bkm_detail
    UNION ALL SELECT 'ARCHIVE','VISA',dispute_code, reason_code, control_stat, source_amount, reimbursement_amount, txn_date FROM archive.ingestion_clearing_visa_detail
    UNION ALL SELECT 'ARCHIVE','MSC', dispute_code, reason_code, control_stat, source_amount, reimbursement_amount, txn_date FROM archive.ingestion_clearing_msc_detail
)
SELECT
    scope AS data_scope, network,
    COALESCE(dispute_code, 'NONE') AS dispute_code,
    COALESCE(reason_code, 'NONE')  AS reason_code,
    control_stat,
    COUNT(*)::bigint                                                                                  AS transaction_count,
    SUM(source_amount)                                                                                AS total_source_amount,
    SUM(reimbursement_amount)                                                                         AS total_reimbursement_amount,
    MIN(CASE WHEN txn_date BETWEEN 19000101 AND 99991231 THEN to_date(txn_date::text,'YYYYMMDD') END) AS first_txn_date,
    MAX(CASE WHEN txn_date BETWEEN 19000101 AND 99991231 THEN to_date(txn_date::text,'YYYYMMDD') END) AS last_txn_date,
    CASE
        WHEN scope = 'ARCHIVE' THEN 'HISTORICAL'
        WHEN COALESCE(dispute_code,'NONE') <> 'NONE' AND SUM(reimbursement_amount) >= 100000 THEN 'HIGH_DISPUTE_EXPOSURE'
        WHEN COALESCE(dispute_code,'NONE') <> 'NONE'                                         THEN 'ACTIVE_DISPUTE'
        ELSE 'CLEAN'
    END AS dispute_flag,
    CASE
        WHEN scope = 'ARCHIVE' THEN 'P5'
        WHEN COALESCE(dispute_code,'NONE') <> 'NONE' AND SUM(reimbursement_amount) >= 100000 THEN 'P1'
        WHEN COALESCE(dispute_code,'NONE') <> 'NONE'                                         THEN 'P3'
        ELSE 'P5'
    END AS urgency,
    CASE
        WHEN scope = 'ARCHIVE' THEN 'HISTORICAL_TREND_ANALYSIS_ONLY'
        WHEN COALESCE(dispute_code,'NONE') <> 'NONE' AND SUM(reimbursement_amount) >= 100000 THEN 'ESCALATE_DISPUTE_RESOLUTION'
        WHEN COALESCE(dispute_code,'NONE') <> 'NONE'                                         THEN 'WORK_DISPUTE_QUEUE'
        ELSE 'NONE'
    END AS recommended_action
FROM src
GROUP BY scope, network, COALESCE(dispute_code, 'NONE'), COALESCE(reason_code, 'NONE'), control_stat;


-- =====================================================================================
-- rep_clearing_io_imbalance
-- Daily clearing incoming vs outgoing flow per network with imbalance flag.
-- =====================================================================================
CREATE OR REPLACE VIEW reporting.rep_clearing_io_imbalance AS
WITH src AS (
    SELECT 'LIVE'::text scope, 'BKM'::text network, io_flag, source_amount, txn_date FROM ingestion.clearing_bkm_detail
    UNION ALL SELECT 'LIVE','VISA', io_flag, source_amount, txn_date FROM ingestion.clearing_visa_detail
    UNION ALL SELECT 'LIVE','MSC',  io_flag, source_amount, txn_date FROM ingestion.clearing_msc_detail
    UNION ALL SELECT 'ARCHIVE','BKM', io_flag, source_amount, txn_date FROM archive.ingestion_clearing_bkm_detail
    UNION ALL SELECT 'ARCHIVE','VISA',io_flag, source_amount, txn_date FROM archive.ingestion_clearing_visa_detail
    UNION ALL SELECT 'ARCHIVE','MSC', io_flag, source_amount, txn_date FROM archive.ingestion_clearing_msc_detail
)
SELECT
    scope AS data_scope, network,
    CASE WHEN txn_date BETWEEN 19000101 AND 99991231 THEN to_date(txn_date::text,'YYYYMMDD') END AS txn_date,
    COUNT(*)::bigint                                                                          AS transaction_count,
    COUNT(*) FILTER (WHERE io_flag = 'I')::bigint                                              AS incoming_count,
    COUNT(*) FILTER (WHERE io_flag = 'O')::bigint                                              AS outgoing_count,
    SUM(CASE WHEN io_flag = 'I' THEN source_amount ELSE 0 END)                                 AS incoming_amount,
    SUM(CASE WHEN io_flag = 'O' THEN source_amount ELSE 0 END)                                 AS outgoing_amount,
    SUM(CASE WHEN io_flag = 'I' THEN source_amount ELSE -source_amount END)                    AS net_flow_amount,
    CASE
        WHEN scope = 'ARCHIVE' THEN 'HISTORICAL'
        WHEN ABS(SUM(CASE WHEN io_flag = 'I' THEN source_amount ELSE -source_amount END)) >= 1000000 THEN 'MATERIAL_NET_IMBALANCE'
        WHEN ABS(SUM(CASE WHEN io_flag = 'I' THEN source_amount ELSE -source_amount END)) >= 100000  THEN 'NOTABLE_NET_IMBALANCE'
        ELSE 'BALANCED'
    END                                                                                        AS imbalance_flag,
    CASE
        WHEN scope = 'ARCHIVE' THEN 'P5'
        WHEN ABS(SUM(CASE WHEN io_flag = 'I' THEN source_amount ELSE -source_amount END)) >= 1000000 THEN 'P2'
        WHEN ABS(SUM(CASE WHEN io_flag = 'I' THEN source_amount ELSE -source_amount END)) >= 100000  THEN 'P3'
        ELSE 'P5'
    END                                                                                        AS urgency,
    CASE
        WHEN scope = 'ARCHIVE' THEN 'HISTORICAL_TREND_ANALYSIS_ONLY'
        WHEN ABS(SUM(CASE WHEN io_flag = 'I' THEN source_amount ELSE -source_amount END)) >= 1000000 THEN 'INVESTIGATE_NET_FLOW_IMBALANCE'
        WHEN ABS(SUM(CASE WHEN io_flag = 'I' THEN source_amount ELSE -source_amount END)) >= 100000  THEN 'MONITOR_NET_FLOW'
        ELSE 'NONE'
    END                                                                                        AS recommended_action
FROM src
GROUP BY scope, network,
         CASE WHEN txn_date BETWEEN 19000101 AND 99991231 THEN to_date(txn_date::text,'YYYYMMDD') END;


-- =====================================================================================
-- rep_high_value_unmatched_transactions
-- Per-record unmatched alerts above 100k with merchant context and masked card number.
-- =====================================================================================
CREATE OR REPLACE VIEW reporting.rep_high_value_unmatched_transactions AS
WITH src AS (
    SELECT 'LIVE'::text scope, 'BKM'::text network, d.id AS detail_id, d.file_line_id,
           d.transaction_date, d.original_amount, d.original_currency,
           d.merchant_name, d.merchant_country, d.card_no
    FROM ingestion.card_bkm_detail d JOIN ingestion.file_line fl ON fl.id = d.file_line_id
    WHERE fl.matched_clearing_line_id IS NULL AND d.original_amount >= 100000
    UNION ALL
    SELECT 'LIVE','VISA', d.id, d.file_line_id, d.transaction_date, d.original_amount, d.original_currency,
           d.merchant_name, d.merchant_country, d.card_no
    FROM ingestion.card_visa_detail d JOIN ingestion.file_line fl ON fl.id = d.file_line_id
    WHERE fl.matched_clearing_line_id IS NULL AND d.original_amount >= 100000
    UNION ALL
    SELECT 'LIVE','MSC',  d.id, d.file_line_id, d.transaction_date, d.original_amount, d.original_currency,
           d.merchant_name, d.merchant_country, d.card_no
    FROM ingestion.card_msc_detail d JOIN ingestion.file_line fl ON fl.id = d.file_line_id
    WHERE fl.matched_clearing_line_id IS NULL AND d.original_amount >= 100000
    UNION ALL
    SELECT 'ARCHIVE','BKM', d.id, d.file_line_id, d.transaction_date, d.original_amount, d.original_currency,
           d.merchant_name, d.merchant_country, d.card_no
    FROM archive.ingestion_card_bkm_detail d JOIN archive.ingestion_file_line fl ON fl.id = d.file_line_id
    WHERE fl.matched_clearing_line_id IS NULL AND d.original_amount >= 100000
    UNION ALL
    SELECT 'ARCHIVE','VISA', d.id, d.file_line_id, d.transaction_date, d.original_amount, d.original_currency,
           d.merchant_name, d.merchant_country, d.card_no
    FROM archive.ingestion_card_visa_detail d JOIN archive.ingestion_file_line fl ON fl.id = d.file_line_id
    WHERE fl.matched_clearing_line_id IS NULL AND d.original_amount >= 100000
    UNION ALL
    SELECT 'ARCHIVE','MSC', d.id, d.file_line_id, d.transaction_date, d.original_amount, d.original_currency,
           d.merchant_name, d.merchant_country, d.card_no
    FROM archive.ingestion_card_msc_detail d JOIN archive.ingestion_file_line fl ON fl.id = d.file_line_id
    WHERE fl.matched_clearing_line_id IS NULL AND d.original_amount >= 100000
)
SELECT
    scope AS data_scope, network, detail_id, file_line_id,
    CASE WHEN transaction_date BETWEEN 19000101 AND 99991231 THEN to_date(transaction_date::text,'YYYYMMDD') END AS transaction_date,
    original_amount,
    original_currency::text AS currency,
    COALESCE(merchant_name, 'UNKNOWN')    AS merchant_name,
    COALESCE(merchant_country, 'UNKNOWN') AS merchant_country,
    CASE WHEN card_no IS NOT NULL AND length(card_no) >= 10
         THEN substr(card_no, 1, 6) || '****' || right(card_no, 4) END AS card_mask,
    CASE
        WHEN scope = 'ARCHIVE'              THEN 'HISTORICAL_HIGH_VALUE'
        WHEN original_amount >= 1000000     THEN 'CRITICAL_HIGH_VALUE_UNMATCHED'
        WHEN original_amount >= 500000      THEN 'HIGH_VALUE_UNMATCHED'
        ELSE 'NOTABLE_VALUE_UNMATCHED'
    END AS risk_flag,
    CASE
        WHEN scope = 'ARCHIVE'              THEN 'P4'
        WHEN original_amount >= 1000000     THEN 'P1'
        WHEN original_amount >= 500000      THEN 'P2'
        ELSE 'P3'
    END AS urgency,
    CASE
        WHEN scope = 'ARCHIVE'              THEN 'HISTORICAL_TREND_ANALYSIS_ONLY'
        WHEN original_amount >= 500000      THEN 'INVESTIGATE_HIGH_VALUE_UNMATCHED_TRANSACTION'
        ELSE 'INVESTIGATE_UNMATCHED_TRANSACTION'
    END AS recommended_action
FROM src;


-- =====================================================================================
-- rep_documentation
-- Hard-coded TR + EN documentation for every reporting view above.
-- =====================================================================================
-- =====================================================================================
-- reporting.rep_documentation source (regenerated; richer professional descriptions).
-- All entries cover every reporting.rep_* view exposed by the dynamic reporting catalog.
-- =====================================================================================
CREATE OR REPLACE VIEW reporting.rep_documentation AS
SELECT d.view_name, d.report_group,
       d.purpose_tr, d.purpose_en,
       d.business_question_tr, d.business_question_en,
       d.interpretation_tr, d.interpretation_en,
       d.usage_time_tr, d.usage_time_en,
       d.target_user_tr, d.target_user_en,
       d.action_guidance_tr, d.action_guidance_en,
       d.important_columns_tr, d.important_columns_en,
       d.live_archive_interpretation_tr, d.live_archive_interpretation_en,
       d.notes_tr, d.notes_en
FROM (VALUES
    ('reporting.rep_action_radar'::text, 'OPERATIONAL_OVERVIEW'::text, 'Tum kategorilerdeki acik aksiyonlari oncelik (P1/P2/P3) ve oncelikli is sirasi ile tek bir radarda gosterir; vardiya kontrolunun cikis noktasidir.'::text, 'Single radar of every open action across categories with priority (P1/P2/P3) and ordered worklist; the entry point for shift control.'::text, 'Su an hangi konuya, hangi siklikta, hangi onceliki ile bakmaliyim?'::text, 'What should I focus on right now, in which order, and with what priority?'::text, 'Her satir bir konu kumesi (ornegin TIMEOUT/RECON FAIL); open_count > 0 calisilmaya devam edildigini, urgency P1 hemen mudahale gerekir; recommended_action calismayi ne yonde yapacagimizi soyler.'::text, 'Each row is an issue cluster (e.g. TIMEOUT/RECON FAIL); open_count > 0 means work in progress, P1 urgency requires immediate intervention; recommended_action prescribes the corrective step.'::text, 'Vardiya basinda, gun ici 60-120 dakikada bir, escalation oncesi.'::text, 'At shift start, every 60-120 minutes intraday, before any escalation.'::text, 'Operasyon ekibi, takim liderleri, NOC.'::text, 'Operations team, team leads, NOC.'::text, 'Once tum P1 satirlari kapat; ardindan oldest_age_hours buyukten kucuge sirala ve P2/P3 yi temizle. Her satir icin recommended_action sutununu uygula.'::text, 'Close every P1 first; then process P2/P3 sorted by oldest_age_hours descending. Follow recommended_action for each row.'::text, 'category, issue_type, open_count, oldest_age_hours, urgency, recommended_action'::text, 'category, issue_type, open_count, oldest_age_hours, urgency, recommended_action'::text, 'LIVE: aktif aksiyon listesi; ARCHIVE: gecmis is yuku ve kategori dagilim trendi.'::text, 'LIVE: active action list; ARCHIVE: historical workload and category-distribution trend.'::text, 'Sayim bazlidir; finansal etki icin rep_unmatched_financial_exposure, rep_high_value_unmatched_transactions raporlarini birlikte oku.'::text, 'Count-based; pair with rep_unmatched_financial_exposure and rep_high_value_unmatched_transactions for financial context.'::text),
    ('reporting.rep_unhealthy_files'::text, 'FILE_PROCESSING'::text, 'Dosya alim kanalindaki saglik siz dosyalari (FAILED, STUCK, INCOMPLETE, HIGH_FAILURE_RATE) sebep kategorisiyle birlikte listeler; alim katmaninda mudahale planinin iskeletidir.'::text, 'Lists ingestion files in unhealthy state (FAILED, STUCK, INCOMPLETE, HIGH_FAILURE_RATE) with categorised root cause; the skeleton of the ingestion-layer remediation plan.'::text, 'Hangi dosyalar duzgun islenmedi, neden ve nasil mudahale etmeliyim?'::text, 'Which files did not process correctly, why, and how should I intervene?'::text, 'issue_category dosyanin tipini belirtir; recommended_action ise birebir yapilmasi gerekeni soyler. failure_rate_pct %20 ustu yapisal bir sorunu gosterir.'::text, 'issue_category names the failure type; recommended_action prescribes the exact next step. failure_rate_pct above 20% indicates a structural issue.'::text, 'Saatlik ya da her dosya alim penceresi sonrasi.'::text, 'Hourly or after each ingestion window.'::text, 'Dosya isleme operasyonu, destek ekibi, kanal sahibi.'::text, 'File processing operations, support team, channel owner.'::text, 'Sirayla: (1) FILE_REJECTED ve STUCK_PROCESSING (P1) coz; (2) INCOMPLETE_DELIVERY icin upstream kanalla iletisime gec; (3) HIGH_FAILURE_RATE icin parser/mapping degisikligi degerlendir.'::text, 'In order: (1) resolve FILE_REJECTED and STUCK_PROCESSING (P1); (2) for INCOMPLETE_DELIVERY contact upstream channel; (3) review parser/mapping for HIGH_FAILURE_RATE.'::text, 'file_status, failure_rate_pct, age_hours, issue_category, recommended_action'::text, 'file_status, failure_rate_pct, age_hours, issue_category, recommended_action'::text, 'LIVE: hemen aksiyon. ARCHIVE: gecmis hata profili ve trend.'::text, 'LIVE: act now. ARCHIVE: historical failure profile and trend.'::text, 'Tek bir dosya birden fazla kategoriye dusebilir; toplama yaparken file_id distinct alinmalidir.'::text, 'A single file may match multiple categories; aggregate by distinct file_id when summarising.'::text),
    ('reporting.rep_stuck_pipeline_items'::text, 'PIPELINE_HEALTH'::text, 'Pipeline asamalarinda (line / evaluation / operation / execution) takili kalmis kayitlari yas ve durum bilgisiyle gosterir; SRE ve operasyonun ilk teshis kaynagidir.'::text, 'Surfaces records stuck at any pipeline stage (line / evaluation / operation / execution) with age and state; first diagnostic source for SRE and operations.'::text, 'Pipeline da neler takildi, en uzun ne kadardir bekliyor ve hangi state te?'::text, 'What is stuck in the pipeline, how long has it been waiting, and in which state?'::text, 'stuck_minutes ile stuck_state birlikte aciliyeti belirler. LEASE_EXPIRED ve LONG_STUCK kritik; HUNG_EXECUTION operasyonu sonlandirma gerektirir.'::text, 'stuck_minutes plus stuck_state set urgency together. LEASE_EXPIRED and LONG_STUCK are critical; HUNG_EXECUTION needs to be terminated.'::text, 'Surekli izleme; alarm aldigin an ilk acilacak rapor.'::text, 'Continuous monitoring; the first report to open after an alert.'::text, 'Operasyon, SRE, platform ekibi.'::text, 'Operations, SRE, platform team.'::text, 'LEASE_EXPIRED operasyonlarda lease serbest birakilir ve yeniden zamanlanir; HUNG_EXECUTION sonlandirilir; ayni stage te toplu tikaniklik altyapi alarmi olarak escalate edilir.'::text, 'Release the lease and reschedule LEASE_EXPIRED operations; terminate HUNG_EXECUTION; bulk stalls at the same stage are escalated as an infrastructure alert.'::text, 'stage, stuck_minutes, stuck_state, lease_expires_at, recommended_action'::text, 'stage, stuck_minutes, stuck_state, lease_expires_at, recommended_action'::text, 'LIVE: aktif tikaniklik. ARCHIVE: gecmis tikanikliklarin kaniti ve frekansi.'::text, 'LIVE: active stalls. ARCHIVE: evidence and frequency of past stalls.'::text, 'lease_expires_at gecmisi ile suanin farki gercek bekleme suresini verir; saat dilimi UTC dir.'::text, 'Difference between now and lease_expires_at gives the real wait time; timezone is UTC.'::text),
    ('reporting.rep_recon_failure_categorization'::text, 'RECONCILIATION'::text, 'Basarisiz reconciliation operasyonlarini operation_code ve branch bazinda gruplayip olasi kok nedeni (TIMEOUT, CONNECTIVITY, DATA_QUALITY, RULE_MISMATCH) etiketler.'::text, 'Groups failed reconciliation operations by operation_code and branch, then tags the most likely root cause (TIMEOUT, CONNECTIVITY, DATA_QUALITY, RULE_MISMATCH).'::text, 'Reconciliation neden basarisiz oluyor, hangi tip hata baskin ve nereyi onarmaliyim?'::text, 'Why is reconciliation failing, which error type dominates, and where should we fix it?'::text, 'likely_root_cause arastirmanin baslangic noktasidir; retries_exhausted_count > 0 retry mekanizmasinin yetmedigini, manuel mudahale gerektigini gosterir.'::text, 'likely_root_cause is the investigation entry point; retries_exhausted_count > 0 means retries did not help and manual intervention is needed.'::text, 'Gunluk; ayrica hata sicramasinda anlik.'::text, 'Daily; also ad-hoc on failure spikes.'::text, 'Reconciliation operasyonu, gelistirme ekibi, SRE.'::text, 'Reconciliation operations, engineering, SRE.'::text, 'Once retries_exhausted satirlarini manuel coz; sonra root_cause kategorisine gore dagit (TIMEOUT/CONNECTIVITY -> SRE, DATA_QUALITY -> upstream, RULE_MISMATCH -> kural sahibi).'::text, 'Resolve retries_exhausted rows manually first, then dispatch by root_cause category (TIMEOUT/CONNECTIVITY -> SRE, DATA_QUALITY -> upstream, RULE_MISMATCH -> rule owner).'::text, 'operation_code, failed_count, retries_exhausted_count, likely_root_cause, recommended_action'::text, 'operation_code, failed_count, retries_exhausted_count, likely_root_cause, recommended_action'::text, 'LIVE: aktif cozum surec listesi. ARCHIVE: hata profili degisimini izlemek icin.'::text, 'LIVE: active resolution worklist. ARCHIVE: track the change in error profile over time.'::text, 'Yeni bir kategori cikar cikmaz aciklamasi runbook a eklenmelidir.'::text, 'Whenever a new category emerges its description must be added to the runbook.'::text),
    ('reporting.rep_manual_review_pressure'::text, 'MANUAL_REVIEW'::text, 'Manuel review kuyrugunda biriken kararlarin SLA baskisini (EXPIRED / EXPIRING_SOON / OVERDUE / ON_TRACK) ve askidaki finansal hacmi gosterir.'::text, 'Shows SLA pressure on the manual-review queue (EXPIRED / EXPIRING_SOON / OVERDUE / ON_TRACK) plus the financial volume on hold.'::text, 'Hangi review kararlari SLA yi asti veya asacak, ne kadar para askida?'::text, 'Which review decisions breached SLA or are about to, and how much money is on hold?'::text, 'EXPIRED ve EXPIRING_SOON acildir; exposure_amount karar gecikmesinin parasal buyuklugudur ve currency bazinda ayrilir.'::text, 'EXPIRED and EXPIRING_SOON are urgent; exposure_amount is the monetary impact of decision delay and is split by currency.'::text, 'Vardiya basi, ogle kontrolu, vardiya sonu.'::text, 'Shift start, midday check, shift end.'::text, 'Backoffice review ekibi, ekip lideri.'::text, 'Backoffice review team, team lead.'::text, 'EXPIRED grubuna runbook taki default-action uygulanir veya derhal karar verilir; OVERDUE siraya alinir; ON_TRACK izlemede tutulur.'::text, 'Apply the runbook default-action to EXPIRED or decide immediately; queue OVERDUE next; keep ON_TRACK under monitoring.'::text, 'sla_bucket, pending_review_count, oldest_waiting_hours, exposure_amount, currency'::text, 'sla_bucket, pending_review_count, oldest_waiting_hours, exposure_amount, currency'::text, 'LIVE: bekleyen kararlar. ARCHIVE: tarihi review baskisi ve SLA basari trendi.'::text, 'LIVE: pending decisions. ARCHIVE: historical review pressure and SLA achievement trend.'::text, 'Currency bazinda ayrilir; toplam alirken currency degisikligini kontrol etmek gerekir.'::text, 'Split by currency; verify currency boundaries before summing across buckets.'::text),
    ('reporting.rep_alert_delivery_health'::text, 'ALERTS'::text, 'Bildirim altyapisinin teslim sagligini alert_type ve severity bazinda olcer; ileti ulasamayan kanallari ortaya cikarir.'::text, 'Measures notification-delivery health per alert_type and severity; surfaces channels where messages do not arrive.'::text, 'Bildirim altyapimiz saglikli mi, hangi kanal/tip bozuk?'::text, 'Is our notification stack healthy, which channel or type is broken?'::text, 'delivery_health_status CRITICAL kanal kirik; DEGRADED uzun bekleyen pending; HEALTHY normal akis.'::text, 'CRITICAL delivery_health_status means the channel is broken; DEGRADED indicates long-pending notifications; HEALTHY is normal flow.'::text, 'Surekli izleme; CRITICAL dakika cinsinden ele alinmalidir.'::text, 'Continuous monitoring; CRITICAL must be addressed within minutes.'::text, 'Operasyon, platform, alarm sahibi takim.'::text, 'Operations, platform, alert owner team.'::text, 'CRITICAL satirlarinda failed alert ler yeniden gonderilir; kanal saglik kontrolu (transport, kotalar, kimlik dogrulama) yapilir; tekrari icin upstream e ticket acilir.'::text, 'On CRITICAL rows resend failed alerts; perform channel health checks (transport, quotas, auth); open an upstream ticket to prevent recurrence.'::text, 'severity, alert_type, failed_count, oldest_open_age_hours, delivery_health_status'::text, 'severity, alert_type, failed_count, oldest_open_age_hours, delivery_health_status'::text, 'LIVE: gercek zamanli kanal sagligi. ARCHIVE: kanal guvenilirligi gecmisi.'::text, 'LIVE: real-time channel health. ARCHIVE: historical channel reliability.'::text, 'Sent orani dususe, alarm degerlendirme sureci de gozden gecirilmelidir; sadece teslim degil, uretim de incelenmelidir.'::text, 'If sent ratio drops review the alert evaluation step too; not only delivery but also generation should be inspected.'::text),
    ('reporting.rep_unmatched_financial_exposure'::text, 'FINANCIAL_RISK'::text, 'Eslesmesim is islemleri scope, side, network, currency ve yas kovasina (0-1, 1-3, 3-7, 7+ gun) gore toplayarak finansal riski ortaya koyar.'::text, 'Aggregates unmatched transactions by scope, side, network, currency and aging bucket (0-1, 1-3, 3-7, 7+ days) to expose financial risk.'::text, 'Eslesmesim is ne kadar para var, nerede yaslaniyor, hangi network ve currency de?'::text, 'How much money is unmatched, where is it aging, in which network and currency?'::text, 'risk_flag CRITICAL veya HIGH ise finansal mudahale gerekir; oldest_age_days ne kadar gec kalindigini gosterir; aging_bucket ileri yaslanmayi onceliklendirir.'::text, 'risk_flag CRITICAL or HIGH requires financial intervention; oldest_age_days shows how late we are; aging_bucket prioritises the most aged items.'::text, 'Gunluk finansal kapanis, haftalik risk degerlendirmesi, ay sonu denetim oncesi.'::text, 'Daily financial close, weekly risk review, before month-end audit.'::text, 'Finans operasyonu, recon ekibi, risk, denetim.'::text, 'Finance operations, recon team, risk, audit.'::text, 'CRITICAL satirlar derhal risk komitesine eskale; HIGH icin kaynak (network/issuer) bazinda incele; LOW izlemede tutulur.'::text, 'Escalate CRITICAL rows to the risk committee; investigate HIGH by source (network/issuer); keep LOW on watch.'::text, 'scope, side, network, currency, aging_bucket, unmatched_count, exposure_amount, oldest_age_days, risk_flag'::text, 'scope, side, network, currency, aging_bucket, unmatched_count, exposure_amount, oldest_age_days, risk_flag'::text, 'LIVE: aktif para riski. ARCHIVE: tarihsel exposure trendi ve mevsimsel desen.'::text, 'LIVE: active financial risk. ARCHIVE: historical exposure trend and seasonal pattern.'::text, 'Currency UNKNOWN kayittaki para birimi alanlari bos demektir; veri kalitesi sorunu olarak ele alinmalidir.'::text, 'Currency UNKNOWN means the currency fields are missing on the record; treat it as a data-quality issue.'::text),
    ('reporting.rep_card_clearing_imbalance'::text, 'FINANCIAL_RECONCILIATION'::text, 'Card ile Clearing taraflari arasindaki gunluk tutar farkini network ve currency bazinda olcer ve dengesizligin maddiyetini etiketler.'::text, 'Measures the daily amount gap between Card and Clearing sides per network and currency, and labels the materiality of the imbalance.'::text, 'Card ve Clearing taraflarimiz parasal olarak dengede mi, fark net olarak ne kadar?'::text, 'Are Card and Clearing sides financially in balance, what is the net gap?'::text, 'imbalance_severity MATERIAL_IMBALANCE ise reconciliation kalitesi bozulmustur; gap_ratio_pct buyuklugu yuzdesel olarak gosterir; BALANCED satirlar saglikidir.'::text, 'MATERIAL_IMBALANCE imbalance_severity means reconciliation quality has broken; gap_ratio_pct shows the magnitude as a percentage; BALANCED rows are healthy.'::text, 'Her gun T+1 finansal kapanis kontrolunde.'::text, 'Daily on T+1 financial close control.'::text, 'Recon ekibi, finans, denetim.'::text, 'Recon team, finance, audit.'::text, 'MATERIAL_IMBALANCE de hemen escalate; eslesmemis is satirlarini ve hatali response_code ozetlerini birlikte incele.'::text, 'Escalate MATERIAL_IMBALANCE immediately; jointly inspect unmatched rows and faulty response_code summaries.'::text, 'network, currency, amount_gap, gap_ratio_pct, imbalance_severity, recommended_action'::text, 'network, currency, amount_gap, gap_ratio_pct, imbalance_severity, recommended_action'::text, 'LIVE: gunluk kapanis kontrolu. ARCHIVE: tarihsel uyum profili ve trend.'::text, 'LIVE: daily close check. ARCHIVE: historical alignment profile and trend.'::text, 'BALANCED satirlarda ek aksiyon gerekmez fakat trend takibinde tutmak gerekir.'::text, 'BALANCED rows need no extra action but should be kept in trend monitoring.'::text),
    ('reporting.rep_reconciliation_quality_score'::text, 'RECONCILIATION_KPI'::text, 'Network/gun bazinda match-rate, recon-failure, retry yogunlugu ve manuel review oranini birlestirip A-F arasinda tek bir kalite notu uretir.'::text, 'Combines match-rate, recon-failure rate, retry density and manual-review ratio per network/day into a single A-F quality grade.'::text, 'Reconciliation kalitemiz hangi network te bozuluyor ve neden?'::text, 'On which network is reconciliation quality degrading and why?'::text, 'quality_grade A en iyi, F en kotu; D-F kalite kaybi olarak ele alinir; weakest_dimension hangi metrik nedeniyle dustugumuzu soyler.'::text, 'quality_grade A is best, F is worst; D-F is treated as quality loss; weakest_dimension tells which metric is dragging the score down.'::text, 'Gunluk yonetim raporu, haftalik trend, ay sonu kalite degerlendirmesi.'::text, 'Daily management report, weekly trend, month-end quality review.'::text, 'Yonetim, urun sahibi, recon ekibi.'::text, 'Management, product owner, recon team.'::text, 'Note D ise weakest_dimension a yonelik cozum uygula (rule tuning, retry tuning, root cause investigation); F kuralla cozulemez, mimari incelemesi gerekir.'::text, 'On grade D apply the fix targeted at weakest_dimension (rule tuning, retry tuning, root cause investigation); F cannot be solved by rules and requires architectural review.'::text, 'network, business_date, match_rate_pct, recon_failure_rate_pct, manual_review_rate_pct, quality_grade, weakest_dimension'::text, 'network, business_date, match_rate_pct, recon_failure_rate_pct, manual_review_rate_pct, quality_grade, weakest_dimension'::text, 'LIVE: bugunku kalite. ARCHIVE: tarihsel kalite trendi ve mevsimsellik.'::text, 'LIVE: today quality. ARCHIVE: historical quality trend and seasonality.'::text, 'Toplam satir 0 ise N/A doner; veri yokluguna isaret eder, kanali kontrol et.'::text, 'When the total row count is 0 the grade is N/A; signals absence of data, verify the channel.'::text),
    ('reporting.rep_misleading_success_cases'::text, 'FINANCIAL_RISK'::text, 'Adet bazinda iyi gorunen ama tutar bazinda kotu olan (veya tersi) gunleri yakalar; sayim metriklerinin gizledigi finansal riski ortaya cikarir.'::text, 'Catches days that look good by line count but bad by amount (or vice versa); exposes financial risk hidden by count-based KPIs.'::text, 'Iyi gorunen gunlerin altinda gizli risk var mi?'::text, 'Is there hidden risk under days that look fine by count?'::text, 'GOOD_COUNT_BAD_AMOUNT az sayida yuksek tutarli islem eslesmemis demektir, ciddi finansal risktir; BAD_COUNT_GOOD_AMOUNT cok sayida kucuk islem eslesmemis ama parasal etki dusuk.'::text, 'GOOD_COUNT_BAD_AMOUNT means a few high-value transactions are unmatched, a serious financial risk; BAD_COUNT_GOOD_AMOUNT means many small ones are unmatched with low monetary impact.'::text, 'Gunluk kapanis sonrasi finansal sanity check.'::text, 'After daily close as a financial sanity check.'::text, 'Finans, recon, denetim.'::text, 'Finance, recon, audit.'::text, 'GOOD_COUNT_BAD_AMOUNT satirlarinda yuksek tutarli eslesmeyen islemleri ayri incele (rep_high_value_unmatched_transactions ile birlikte oku).'::text, 'For GOOD_COUNT_BAD_AMOUNT rows inspect unmatched high-value transactions separately (cross-read with rep_high_value_unmatched_transactions).'::text, 'business_date, count_match_rate_pct, amount_match_rate_pct, misleading_pattern'::text, 'business_date, count_match_rate_pct, amount_match_rate_pct, misleading_pattern'::text, 'LIVE: gunluk gizli risk taramasi. ARCHIVE: pattern in gecmiste de gorulup gorulmedigi.'::text, 'LIVE: daily hidden-risk scan. ARCHIVE: whether the pattern occurred historically.'::text, 'CONSISTENT satirlari saglikidir, listeden filtrelenebilir.'::text, 'CONSISTENT rows are healthy and may be filtered out.'::text),
    ('reporting.rep_high_value_unmatched_transactions'::text, 'FINANCIAL_RISK'::text, 'Tek tek 100k+ tutarli eslesmemis islemleri merchant adi, kanal ve PAN-mask edilmis kart bilgisi ile listeler; tek-noktali yuksek riski hedefler.'::text, 'Lists individual unmatched transactions of 100k+ with merchant name, channel and PAN-masked card; targets single-item high risk.'::text, 'Yuksek tutarli hangi tek tek islemler hala beklemede?'::text, 'Which individual high-value transactions are still pending match?'::text, 'risk_flag CRITICAL_HIGH_VALUE_UNMATCHED 1M+ islemdir, derhal incelenmelidir; HIGH_VALUE_UNMATCHED ikinci sira riskidir.'::text, 'CRITICAL_HIGH_VALUE_UNMATCHED risk_flag is a 1M+ transaction; inspect immediately. HIGH_VALUE_UNMATCHED is the second-tier risk.'::text, 'Gunluk finansal kapanis sonrasi.'::text, 'After the daily financial close.'::text, 'Recon, finans, risk, KYC ekibi.'::text, 'Recon, finance, risk, KYC team.'::text, 'CRITICAL_HIGH_VALUE_UNMATCHED satirlari risk komitesine eskale, gerekirse manuel hareketle deniklestir; HIGH_VALUE_UNMATCHED operasyon ekibinde isleme alinir.'::text, 'Escalate CRITICAL_HIGH_VALUE_UNMATCHED to the risk committee, balance manually if required; HIGH_VALUE_UNMATCHED is processed by operations.'::text, 'detail_id, transaction_date, original_amount, merchant_name, card_mask, risk_flag'::text, 'detail_id, transaction_date, original_amount, merchant_name, card_mask, risk_flag'::text, 'LIVE: aktif yuksek tutarli risk. ARCHIVE: tarihi yuksek tutarli profil.'::text, 'LIVE: active high-value risk. ARCHIVE: historical high-value profile.'::text, 'Kart numarasi PAN-mask edilir (ilk6 + **** + son4); 100k esiginin altindaki islemler dahil edilmez.'::text, 'Card number is PAN-masked (first6 + **** + last4); transactions below the 100k threshold are excluded.'::text),
    ('reporting.rep_archive_pipeline_health'::text, 'ARCHIVE'::text, 'Arsiv pipeline inin sagligini iki perspektiften gosterir: hatali/sikismis arsiv kosumlari ve arsivlenmeyi bekleyen ama 7+ gundur bekleyen uygun dosyalar.'::text, 'Archive pipeline health from two perspectives: failed/stuck archive runs, and eligible files that have been waiting 7+ days unarchived.'::text, 'Arsiv sureci akiyor mu, biriken dosya var mi, kosumlar saglikli mi?'::text, 'Is archiving flowing, is a backlog building, are runs healthy?'::text, 'pipeline_health CRITICAL kosum hatali/sikismis; DEGRADED 7+ gun arsivlenmemis dosya birikimi; HEALTHY normal.'::text, 'CRITICAL pipeline_health means a run failed/stuck; DEGRADED means files are sitting unarchived for 7+ days; HEALTHY is normal.'::text, 'Gunluk; backlog buyurken surekli.'::text, 'Daily; continuously while backlog grows.'::text, 'Operasyon, veri yonetimi, compliance.'::text, 'Operations, data management, compliance.'::text, 'CRITICAL satirlari hemen onar; ELIGIBLE_BACKLOG icin arsiv isi tetiklenir, RUN_STUCK kosumu sonlandirilir.'::text, 'Fix CRITICAL rows immediately; trigger the archive job for ELIGIBLE_BACKLOG, terminate RUN_STUCK runs.'::text, 'perspective, age_days, pipeline_health, recommended_action'::text, 'perspective, age_days, pipeline_health, recommended_action'::text, 'LIVE perspektifi backlog dur. ARCHIVE perspektifi kosum sagligidir.'::text, 'LIVE perspective is backlog. ARCHIVE perspective is run health.'::text, 'RUN_IN_PROGRESS uzun surerse RUN_STUCK e doner; sureyi izleyin.'::text, 'RUN_IN_PROGRESS turns into RUN_STUCK if it lasts too long; watch the duration.'::text),
    ('reporting.rep_daily_transaction_volume'::text, 'FINANCIAL_VOLUME'::text, 'Islem tarihine (transaction_date) gore network/financial_type/txn_effect/currency bazinda gunluk hacim, debit/credit ve net akis.'::text, 'Daily volume per network/financial_type/txn_effect/currency using real business transaction_date with debit, credit and net flow.'::text, 'Hangi guneki finansal hacim ne kadardi, anormal bir tepe ya da cukur var mi?'::text, 'What was the daily financial volume, are there abnormal peaks or troughs?'::text, 'volume_flag MATERIAL_NET_FLOW net akis 1M esigini astigini gosterir, denetim gerekir; NORMAL_VOLUME ise olagan dagilimdir.'::text, 'MATERIAL_NET_FLOW volume_flag means net flow exceeds the 1M threshold and warrants review; NORMAL_VOLUME indicates ordinary distribution.'::text, 'Gunluk finansal kapanis ve haftalik trend incelemesi.'::text, 'Daily financial close and weekly trend review.'::text, 'Finans, recon, yonetim.'::text, 'Finance, recon, management.'::text, 'Anormal net akis tespit edilirse o gunun yuksek tutarli islemleri (rep_high_value_unmatched_transactions) ve cesidi (network/currency) ile incele.'::text, 'On abnormal net flow drill into that day high-value transactions (rep_high_value_unmatched_transactions) and breakdown (network/currency).'::text, 'transaction_date, network, currency, debit_amount, credit_amount, net_flow_amount, volume_flag'::text, 'transaction_date, network, currency, debit_amount, credit_amount, net_flow_amount, volume_flag'::text, 'LIVE: bugunku/dunku gercek tarihler. ARCHIVE: tarihsel hacim profili ve trend.'::text, 'LIVE: today/yesterday actuals. ARCHIVE: historical volume profile and trend.'::text, 'transaction_date int4 YYYYMMDD formatindadir; out-of-range degerler atlanir (1900-9999).'::text, 'transaction_date is int4 YYYYMMDD; out-of-range values are skipped (1900-9999).'::text),
    ('reporting.rep_mcc_revenue_concentration'::text, 'FINANCIAL_CONCENTRATION'::text, 'MCC bazinda hacim payi, vergi/komisyon, surcharge ve cashback ekonomisi; konsantrasyon riski etiketi.'::text, 'Per-MCC volume share, tax/commission, surcharge and cashback economics with concentration risk flag.'::text, 'Hacmimiz hangi MCC lere bagli, tek bir MCC ye asiri bagimliyiz?'::text, 'Which MCCs drive our volume, are we over-dependent on a single one?'::text, 'concentration_risk HIGH_CONCENTRATION MCC payinin %30 unu astigini gosterir; is surekliligi ve gelir cesitlendirme riskidir.'::text, 'HIGH_CONCENTRATION concentration_risk means an MCC exceeds 30% share; a business-continuity and revenue diversification risk.'::text, 'Aylik portfoy degerlendirmesi.'::text, 'Monthly portfolio review.'::text, 'Risk, finans, ticari ekipler.'::text, 'Risk, finance, commercial teams.'::text, 'HIGH_CONCENTRATION MCC ler portfoy diversifikasyonu calismasina alinir; ticari ekiple yeni MCC kazanimi planlanir.'::text, 'HIGH_CONCENTRATION MCCs are sent to portfolio diversification work; new MCC acquisition is planned with the commercial team.'::text, 'mcc, volume_share_pct, total_tax_amount, total_cashback_amount, concentration_risk'::text, 'mcc, volume_share_pct, total_tax_amount, total_cashback_amount, concentration_risk'::text, 'LIVE: guncel portfoy. ARCHIVE: tarihsel konsantrasyon trendi.'::text, 'LIVE: current portfolio. ARCHIVE: historical concentration trend.'::text, 'Vergi geliri (tax1+tax2) ve cashback gideri ayni satirda goruldugu icin net katki hesaplanabilir.'::text, 'Tax revenue (tax1+tax2) and cashback cost are visible side by side; net contribution is computable.'::text),
    ('reporting.rep_merchant_risk_hotspots'::text, 'FINANCIAL_RISK'::text, 'Merchant bazinda decline orani, eslesmemis is orani ve eslesmesim is finansal etki ile risk siniflandirmasi (HIGH_RISK / MEDIUM_RISK / LOW_RISK / HIGH_DECLINE).'::text, 'Per-merchant decline rate, unmatched rate and unmatched financial impact with risk classification (HIGH_RISK / MEDIUM_RISK / LOW_RISK / HIGH_DECLINE).'::text, 'Hangi merchant lar yuksek risk tasiyor, hangi merchant da decline patlamasi var?'::text, 'Which merchants carry high risk, where is decline spiking?'::text, 'HIGH_RISK_MERCHANT %20+ unmatched ve 100k+ tutar; HIGH_DECLINE_MERCHANT decline ozellikle yogun.'::text, 'HIGH_RISK_MERCHANT means 20%+ unmatched and 100k+ amount; HIGH_DECLINE_MERCHANT means decline is particularly intense.'::text, 'Haftalik merchant risk degerlendirmesi.'::text, 'Weekly merchant risk review.'::text, 'Risk, ticari, finans.'::text, 'Risk, commercial, finance.'::text, 'HIGH_RISK_MERCHANT ler risk ekibine eskale; HIGH_DECLINE_MERCHANT lar icin decline pattern (issuer/limit/3DS) incelenir.'::text, 'Escalate HIGH_RISK_MERCHANT to the risk team; for HIGH_DECLINE_MERCHANT investigate decline pattern (issuer/limit/3DS).'::text, 'merchant_id, decline_rate_pct, unmatched_rate_pct, unmatched_amount, risk_flag'::text, 'merchant_id, decline_rate_pct, unmatched_rate_pct, unmatched_amount, risk_flag'::text, 'LIVE: aktif merchant riski. ARCHIVE: gecmis profili.'::text, 'LIVE: active merchant risk. ARCHIVE: historical profile.'::text, 'Decline orani response_code <> 00 veya is_successful_txn = N kombinasyonuyla hesaplanir.'::text, 'Decline rate is computed from response_code <> 00 or is_successful_txn = N.'::text),
    ('reporting.rep_country_cross_border_exposure'::text, 'FINANCIAL_RISK'::text, 'Merchant ulkesi ve original/settlement currency esitsizligine gore yurt disi ve FX maruziyetini gosterir.'::text, 'Surfaces cross-border and FX exposure by merchant_country and original-vs-settlement currency.'::text, 'Yurt disi ve farkli para birimi islemlerine ne kadar maruziz?'::text, 'How exposed are we to cross-border and cross-currency transactions?'::text, 'exposure_flag HIGH_FX_EXPOSURE yuksek tutarli FX maruziyetidir, hedge degerlendirilmelidir.'::text, 'HIGH_FX_EXPOSURE exposure_flag indicates large FX exposure; consider hedging.'::text, 'Aylik FX risk degerlendirmesi.'::text, 'Monthly FX risk review.'::text, 'Hazine, finans, risk.'::text, 'Treasury, finance, risk.'::text, 'HIGH_FX_EXPOSURE icin hedging politikasi gozden gecirilir; ulke bazinda diversifikasyon planlanir.'::text, 'Review hedging policy on HIGH_FX_EXPOSURE; plan country-level diversification.'::text, 'merchant_country, fx_pattern, total_original_amount, exposure_flag'::text, 'merchant_country, fx_pattern, total_original_amount, exposure_flag'::text, 'LIVE: guncel exposure. ARCHIVE: tarihsel FX riski.'::text, 'LIVE: current exposure. ARCHIVE: historical FX risk.'::text, 'Currency kodlari ISO 4217 numerik (orn 949 = TRY) olabilir.'::text, 'Currency codes are likely ISO 4217 numeric (e.g. 949 = TRY).'::text),
    ('reporting.rep_response_code_decline_health'::text, 'AUTHORIZATION_HEALTH'::text, 'Network bazinda response_code dagilimi, basarisizlik orani ve baskin red sebepleri.'::text, 'Per-network response_code distribution with failure rate and dominant decline reasons.'::text, 'Iadeler ve redler hangi response_code dan geliyor?'::text, 'Which response_codes are driving declines and refusals?'::text, 'DOMINANT_FAILURE_REASON %5 ustu paya sahip bir red sebebi vardir, kaynak arastirilmalidir; HEALTHY normaldir.'::text, 'DOMINANT_FAILURE_REASON means a decline reason exceeds 5% share; investigate the root cause; HEALTHY is normal.'::text, 'Gunluk operasyonel takip.'::text, 'Daily operational monitoring.'::text, 'Operasyon, urun, risk.'::text, 'Operations, product, risk.'::text, 'DOMINANT_FAILURE_REASON satirlari icin kaynak (issuer/network/limit) arastirilir; gerekirse 3DS/limit ayarlari guncellenir.'::text, 'Investigate the source (issuer/network/limit) for DOMINANT_FAILURE_REASON rows; update 3DS/limit settings if needed.'::text, 'network, response_code, failure_rate_pct, network_share_pct, health_flag'::text, 'network, response_code, failure_rate_pct, network_share_pct, health_flag'::text, 'LIVE: anlik authorization sagligi. ARCHIVE: tarihsel red profili.'::text, 'LIVE: real-time authorization health. ARCHIVE: historical decline profile.'::text, 'response_code = 00 basari kabul edilir; NONE veri eksikligini gosterir.'::text, 'response_code = 00 is treated as success; NONE indicates missing data.'::text),
    ('reporting.rep_settlement_lag_analysis'::text, 'OPERATIONS_KPI'::text, 'Islem tarihi ile end_of_day_date, value_date ve dosya alim tarihi arasindaki gecikmeleri olcer.'::text, 'Measures lag between transaction_date and end_of_day_date, value_date, and file ingestion date.'::text, 'Islemler ne kadar zamaninda sisteme dusuyor, hangi noktada gecikme olusuyor?'::text, 'How timely are transactions arriving in the system, where does the delay accumulate?'::text, 'lag_health CHRONIC_INGEST_DELAY yapisal bir alim gecikmesidir; ON_TARGET SLA icindedir.'::text, 'CHRONIC_INGEST_DELAY lag_health means structurally lagging ingestion; ON_TARGET means within SLA.'::text, 'Gunluk SLA takibi.'::text, 'Daily SLA monitoring.'::text, 'Operasyon, SRE, recon.'::text, 'Operations, SRE, recon.'::text, 'CHRONIC_INGEST_DELAY de upstream provider/kanal ile temas kurulur; transport ya da batch zamanlamasi degistirilir.'::text, 'On CHRONIC_INGEST_DELAY contact upstream provider/channel; change transport or batch schedule.'::text, 'channel, avg_lag_to_ingest_days, max_lag_to_ingest_days, late_ingest_count, lag_health'::text, 'channel, avg_lag_to_ingest_days, max_lag_to_ingest_days, late_ingest_count, lag_health'::text, 'LIVE: guncel SLA durumu. ARCHIVE: tarihsel SLA profili.'::text, 'LIVE: current SLA status. ARCHIVE: historical SLA profile.'::text, 'transaction_date, end_of_day_date, value_date YYYYMMDD int4 olarak tutulur.'::text, 'transaction_date, end_of_day_date, value_date are stored as YYYYMMDD int4.'::text),
    ('reporting.rep_currency_fx_drift'::text, 'FINANCIAL_RISK'::text, 'Cross-currency islemlerde original ve settlement tutar farklarini toplayarak FX drift i (kazanc/kayip) gosterir.'::text, 'For cross-currency transactions aggregates the original-vs-settlement amount drift to surface FX gain/loss.'::text, 'FX donusumlerinde sistematik bir kayip ya da kazanc var mi?'::text, 'Is there systematic FX gain/loss in our currency conversions?'::text, 'MATERIAL_DRIFT 100k+ kumulatif drift birikmistir; settlement mantigi ve kur kaynagi denetlenmelidir.'::text, 'MATERIAL_DRIFT means cumulative drift exceeds 100k; review settlement logic and FX rate source.'::text, 'Aylik FX denetimi.'::text, 'Monthly FX audit.'::text, 'Hazine, finans, denetim.'::text, 'Treasury, finance, audit.'::text, 'MATERIAL_DRIFT te kur kaynagi (Reuters/issuer rate) ve settlement formulu denetlenir; gerekirse rezerv ayrilir.'::text, 'On MATERIAL_DRIFT audit the FX rate source (Reuters/issuer rate) and settlement formula; reserve if needed.'::text, 'currency_pair, settlement_drift, billing_drift, fx_drift_severity'::text, 'currency_pair, settlement_drift, billing_drift, fx_drift_severity'::text, 'LIVE: guncel FX drift. ARCHIVE: tarihsel FX drift profili.'::text, 'LIVE: current FX drift. ARCHIVE: historical FX drift profile.'::text, 'Sadece original_currency <> settlement_currency olan satirlar ele alinir.'::text, 'Only original_currency <> settlement_currency rows are considered.'::text),
    ('reporting.rep_installment_portfolio_summary'::text, 'PORTFOLIO_RISK'::text, 'Network bazinda taksit kovasi (1, 2-3, 4-6, 7-12, 13+) ile portfoy dagilimi ve uzun vadeli maruziyet bayragi.'::text, 'Per-network installment buckets (1, 2-3, 4-6, 7-12, 13+) with portfolio share and long-term exposure flag.'::text, 'Taksitli portfoyumuz ne kadar uzun vadeye yayilmis?'::text, 'How much of our portfolio is in long-term installments?'::text, 'HIGH_LONG_TERM_INSTALLMENT_EXPOSURE 13+ taksit %10 unu astigini gosterir, kredi riski artmistir.'::text, 'HIGH_LONG_TERM_INSTALLMENT_EXPOSURE means 13+ installment exceeds 10%, increasing credit risk.'::text, 'Aylik portfoy degerlendirmesi.'::text, 'Monthly portfolio review.'::text, 'Risk, kredi, finans.'::text, 'Risk, credit, finance.'::text, 'HIGH_LONG_TERM_INSTALLMENT_EXPOSURE durumunda taksit politikasi/skorlama incelenmelidir.'::text, 'Review installment policy/scoring on HIGH_LONG_TERM_INSTALLMENT_EXPOSURE.'::text, 'network, installment_bucket, volume_share_pct, amount_share_pct, portfolio_flag'::text, 'network, installment_bucket, volume_share_pct, amount_share_pct, portfolio_flag'::text, 'LIVE: guncel portfoy. ARCHIVE: tarihsel taksit egilimi.'::text, 'LIVE: current portfolio. ARCHIVE: historical installment trend.'::text, 'install_count detail tabloundaki gercek taksit sayisidir; install_order taksit sirasidir.'::text, 'install_count is the real installment count from detail; install_order is the order index.'::text),
    ('reporting.rep_loyalty_points_economy'::text, 'PROGRAM_ECONOMICS'::text, 'Gunluk bazda BC/MC/CC puan tutarlarinin orijinal islem tutarina oranini gosterir; sadakat program maliyetini olcer.'::text, 'Daily BC/MC/CC point amounts vs original transaction amount; measures loyalty program cost.'::text, 'Sadakat programi gunluk olarak ne kadara mal oluyor?'::text, 'How much does the loyalty program cost daily?'::text, 'HIGH_LOYALTY_USAGE %10+ subvansiyon var; program maliyeti gozden gecirilmelidir.'::text, 'HIGH_LOYALTY_USAGE means subsidy exceeds 10%; review program cost.'::text, 'Aylik program degerlendirmesi.'::text, 'Monthly program review.'::text, 'Sadakat, urun, finans.'::text, 'Loyalty, product, finance.'::text, 'HIGH_LOYALTY_USAGE gunlerinde puan kazanim/harcama kurallari ve kampanyalar gozden gecirilir.'::text, 'On HIGH_LOYALTY_USAGE days review point earning/burning rules and campaigns.'::text, 'business_date, total_loyalty_amount, loyalty_to_amount_ratio_pct, loyalty_intensity'::text, 'business_date, total_loyalty_amount, loyalty_to_amount_ratio_pct, loyalty_intensity'::text, 'LIVE: gunluk program maliyeti. ARCHIVE: tarihsel maliyet trendi.'::text, 'LIVE: daily program cost. ARCHIVE: historical cost trend.'::text, 'BC/MC/CC point_amount alanlari TL bazinda tutar olarak saklanir.'::text, 'BC/MC/CC point_amount fields are stored as currency amounts.'::text),
    ('reporting.rep_clearing_dispute_summary'::text, 'DISPUTES'::text, 'Clearing tarafindaki dispute_code/reason_code/control_stat bazinda islem ve reimbursement tutarlari.'::text, 'Clearing dispute aggregations by dispute_code/reason_code/control_stat with reimbursement amount.'::text, 'Itiraz/chargeback yuku ne kadar, hangi reason_code lar baskin?'::text, 'What is our chargeback load, which reason codes dominate?'::text, 'HIGH_DISPUTE_EXPOSURE reimbursement tutarinin 100k yi astigini gosterir, eskale edilmelidir.'::text, 'HIGH_DISPUTE_EXPOSURE means reimbursement exceeds 100k; escalate.'::text, 'Haftalik dispute degerlendirmesi.'::text, 'Weekly dispute review.'::text, 'Dispute ekibi, finans, denetim.'::text, 'Dispute team, finance, audit.'::text, 'HIGH_DISPUTE_EXPOSURE icin musteri/issuer ile temas plani olusturulur; reason_code bazinda root cause incelenir.'::text, 'For HIGH_DISPUTE_EXPOSURE create a contact plan with customer/issuer; investigate root cause per reason_code.'::text, 'dispute_code, reason_code, control_stat, total_reimbursement_amount, dispute_flag'::text, 'dispute_code, reason_code, control_stat, total_reimbursement_amount, dispute_flag'::text, 'LIVE: aktif disputes. ARCHIVE: tarihsel dispute profili.'::text, 'LIVE: active disputes. ARCHIVE: historical dispute profile.'::text, 'Sadece clearing detay tablolari kullanilir; card detail tarafinda dispute alani yoktur.'::text, 'Only clearing detail tables are used; card detail does not have a dispute field.'::text),
    ('reporting.rep_clearing_io_imbalance'::text, 'CLEARING_FLOW'::text, 'Gunluk clearing incoming/outgoing tutarlari ve net akis dengesizligi.'::text, 'Daily clearing incoming/outgoing amounts and net flow imbalance.'::text, 'Clearing incoming/outgoing dengemiz nasil, net akis ne yonde?'::text, 'How is our incoming/outgoing clearing balance, in which direction is the net flow?'::text, 'MATERIAL_NET_IMBALANCE 1M yi asan net akis demektir; recon veya reconciliation gecikmesi gostergesidir.'::text, 'MATERIAL_NET_IMBALANCE means net flow exceeds 1M; indicates recon delay or imbalance.'::text, 'Gunluk T+1 takibi.'::text, 'Daily T+1 monitoring.'::text, 'Recon, finans.'::text, 'Recon, finance.'::text, 'MATERIAL_NET_IMBALANCE icin gunluk reconciliation kapanis akisi denetlenir.'::text, 'On MATERIAL_NET_IMBALANCE check the daily reconciliation close flow.'::text, 'business_date, incoming_amount, outgoing_amount, net_flow_amount, imbalance_flag'::text, 'business_date, incoming_amount, outgoing_amount, net_flow_amount, imbalance_flag'::text, 'LIVE: guncel akis dengesi. ARCHIVE: tarihsel akis profili.'::text, 'LIVE: current flow balance. ARCHIVE: historical flow profile.'::text, 'io_flag I = incoming, O = outgoing kabul edilir.'::text, 'io_flag I = incoming, O = outgoing.'::text)
) AS d(
    view_name, report_group, purpose_tr, purpose_en, business_question_tr, business_question_en, interpretation_tr, interpretation_en, usage_time_tr, usage_time_en, target_user_tr, target_user_en, action_guidance_tr, action_guidance_en, important_columns_tr, important_columns_en, live_archive_interpretation_tr, live_archive_interpretation_en, notes_tr, notes_en
);

-- =====================================================================
-- Helper view: dynamic reporting catalog
-- Auto-discovers every "reporting.rep_*" view (excluding the helper itself
-- and the documentation view) and produces JSON contracts that are consumed
-- by the generic /api/reporting/dynamic endpoint.
-- =====================================================================
CREATE OR REPLACE VIEW reporting.rep_contract_catalog AS
WITH rep_views AS (
    SELECT
        c.relname                       AS report_name,
        'reporting.' || c.relname       AS full_view_name,
        c.oid                           AS view_oid
    FROM pg_class c
    JOIN pg_namespace n ON n.oid = c.relnamespace
    WHERE n.nspname = 'reporting'
      AND c.relkind IN ('v','m')
      AND c.relname LIKE 'rep_%'
      AND c.relname NOT IN ('rep_contract_catalog', 'rep_documentation')
),
cols AS (
    SELECT
        v.report_name,
        v.full_view_name,
        a.attname                                    AS column_name,
        a.attnum                                     AS ordinal,
        NOT a.attnotnull                             AS is_nullable,
        format_type(a.atttypid, a.atttypmod)         AS pg_type,
        CASE
            WHEN format_type(a.atttypid, NULL) IN
                 ('text','character varying','varchar','character','char','bpchar','citext','uuid','name')
                THEN 'string'
            WHEN format_type(a.atttypid, NULL) IN
                 ('smallint','integer','bigint','numeric','decimal','real','double precision','money')
                THEN 'number'
            WHEN format_type(a.atttypid, NULL) = 'boolean'
                THEN 'boolean'
            WHEN format_type(a.atttypid, NULL) IN
                 ('date','timestamp without time zone','timestamp with time zone',
                  'time without time zone','time with time zone')
                THEN 'datetime'
            WHEN format_type(a.atttypid, NULL) IN ('json','jsonb')
                THEN 'object'
            ELSE 'string'
        END                                          AS api_type
    FROM rep_views v
    JOIN pg_attribute a ON a.attrelid = v.view_oid
    WHERE a.attnum > 0 AND NOT a.attisdropped
),
cols_with_ops AS (
    SELECT
        c.*,
        CASE c.api_type
            WHEN 'string'   THEN to_jsonb(ARRAY['eq','neq','contains','startsWith','endsWith','in','isNull','isNotNull'])
            WHEN 'number'   THEN to_jsonb(ARRAY['eq','neq','gt','gte','lt','lte','between','in','isNull','isNotNull'])
            WHEN 'datetime' THEN to_jsonb(ARRAY['eq','neq','gt','gte','lt','lte','between','isNull','isNotNull'])
            WHEN 'boolean'  THEN to_jsonb(ARRAY['eq','neq','isNull','isNotNull'])
            ELSE                 to_jsonb(ARRAY['eq','neq','isNull','isNotNull'])
        END AS operators
    FROM cols c
)
SELECT
    v.report_name,
    v.full_view_name,
    jsonb_build_object(
        'filters',
        COALESCE((
            SELECT jsonb_agg(
                       jsonb_build_object(
                           'field',     c.column_name,
                           'type',      c.api_type,
                           'nullable',  c.is_nullable,
                           'operators', c.operators
                       )
                       ORDER BY c.ordinal
                   )
            FROM cols_with_ops c
            WHERE c.report_name = v.report_name
        ), '[]'::jsonb)
    ) AS request_contract_json,
    jsonb_build_object(
        'type',    'Dictionary<string, object?>',
        'columns',
        COALESCE((
            SELECT jsonb_agg(
                       jsonb_build_object(
                           'field',    c.column_name,
                           'type',     c.api_type,
                           'nullable', c.is_nullable,
                           'ordinal',  c.ordinal
                       )
                       ORDER BY c.ordinal
                   )
            FROM cols_with_ops c
            WHERE c.report_name = v.report_name
        ), '[]'::jsonb)
    ) AS response_contract_json
FROM rep_views v;

