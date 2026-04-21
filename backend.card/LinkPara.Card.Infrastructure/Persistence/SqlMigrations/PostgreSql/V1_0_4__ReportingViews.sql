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
    WHERE status = 'Evaluating' AND EXTRACT(EPOCH FROM (NOW() - create_date)) > 3600
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
    WHERE status = 'Started' AND EXTRACT(EPOCH FROM (NOW() - started_at)) > 1800
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
    WHERE status NOT IN ('Archived','Failed') AND EXTRACT(EPOCH FROM (NOW() - create_date)) > 3600
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
    WHERE e.status = 'Evaluating' AND EXTRACT(EPOCH FROM (NOW() - e.create_date)) > 3600
    UNION ALL
    SELECT 'LIVE','OPERATION', o.id::text, o.evaluation_id::text, o.create_date, o.lease_expires_at, o.lease_owner
    FROM reconciliation.operation o
    WHERE o.status = 'Executing'
      AND ((o.lease_expires_at IS NOT NULL AND o.lease_expires_at < NOW())
            OR EXTRACT(EPOCH FROM (NOW() - o.create_date)) > 3600)
    UNION ALL
    SELECT 'LIVE','EXECUTION', x.id::text, x.operation_id::text, x.started_at, NULL, NULL
    FROM reconciliation.operation_execution x
    WHERE x.status = 'Started' AND EXTRACT(EPOCH FROM (NOW() - x.started_at)) > 1800
    UNION ALL
    SELECT 'ARCHIVE','FILE_LINE', fl.id::text, fl.file_id::text, fl.create_date, NULL, NULL
    FROM archive.ingestion_file_line fl WHERE fl.status = 'Processing'
    UNION ALL
    SELECT 'ARCHIVE','EVALUATION', e.id::text, e.file_line_id::text, e.create_date, NULL, NULL
    FROM archive.reconciliation_evaluation e WHERE e.status = 'Evaluating'
    UNION ALL
    SELECT 'ARCHIVE','OPERATION', o.id::text, o.evaluation_id::text, o.create_date, o.lease_expires_at, o.lease_owner
    FROM archive.reconciliation_operation o WHERE o.status = 'Executing'
    UNION ALL
    SELECT 'ARCHIVE','EXECUTION', x.id::text, x.operation_id::text, x.started_at, NULL, NULL
    FROM archive.reconciliation_operation_execution x WHERE x.status = 'Started'
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
    COUNT(*) FILTER (WHERE alert_status = 'Consumed')::bigint                                  AS sent_count,
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
            WHEN l.status NOT IN ('Archived','Failed')
                AND EXTRACT(EPOCH FROM (NOW() - l.create_date)) > 3600        THEN 'RUN_STUCK'
            WHEN l.status NOT IN ('Archived','Failed')                        THEN 'RUN_IN_PROGRESS'
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
       OR l.status NOT IN ('Archived','Failed')
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
    SUM(CASE WHEN txn_effect = 'Debit'  THEN original_amount ELSE 0 END)        AS debit_amount,
    SUM(CASE WHEN txn_effect = 'Credit' THEN original_amount ELSE 0 END)        AS credit_amount,
    SUM(CASE WHEN txn_effect = 'Credit' THEN original_amount ELSE -original_amount END) AS net_flow_amount,
    ROUND(AVG(original_amount), 2)                                              AS avg_amount,
    MAX(original_amount)                                                        AS max_amount,
    CASE
        WHEN scope = 'ARCHIVE' THEN 'HISTORICAL'
        WHEN ABS(SUM(CASE WHEN txn_effect = 'Credit' THEN original_amount ELSE -original_amount END)) >= 1000000 THEN 'MATERIAL_NET_FLOW'
        ELSE 'NORMAL'
    END                                                                          AS volume_flag,
    CASE
        WHEN scope = 'ARCHIVE' THEN 'P5'
        WHEN ABS(SUM(CASE WHEN txn_effect = 'Credit' THEN original_amount ELSE -original_amount END)) >= 1000000 THEN 'P3'
        ELSE 'P5'
    END                                                                          AS urgency,
    CASE
        WHEN scope = 'ARCHIVE' THEN 'HISTORICAL_TREND_ANALYSIS_ONLY'
        WHEN ABS(SUM(CASE WHEN txn_effect = 'Credit' THEN original_amount ELSE -original_amount END)) >= 1000000 THEN 'INVESTIGATE_LARGE_NET_FLOW'
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
           COUNT(*) FILTER (WHERE is_successful_txn = 'Unsuccessful' OR (response_code IS NOT NULL AND response_code <> '00'))             AS declined_count,
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
           COUNT(*) FILTER (WHERE is_successful_txn = 'Successful')   AS successful_count,
           COUNT(*) FILTER (WHERE is_successful_txn = 'Unsuccessful') AS failed_count
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
    COUNT(*) FILTER (WHERE io_flag = 'Incoming')::bigint                                       AS incoming_count,
    COUNT(*) FILTER (WHERE io_flag = 'Outgoing')::bigint                                       AS outgoing_count,
    SUM(CASE WHEN io_flag = 'Incoming' THEN source_amount ELSE 0 END)                          AS incoming_amount,
    SUM(CASE WHEN io_flag = 'Outgoing' THEN source_amount ELSE 0 END)                          AS outgoing_amount,
    SUM(CASE WHEN io_flag = 'Incoming' THEN source_amount ELSE -source_amount END)             AS net_flow_amount,
    CASE
        WHEN scope = 'ARCHIVE' THEN 'HISTORICAL'
        WHEN ABS(SUM(CASE WHEN io_flag = 'Incoming' THEN source_amount ELSE -source_amount END)) >= 1000000 THEN 'MATERIAL_NET_IMBALANCE'
        WHEN ABS(SUM(CASE WHEN io_flag = 'Incoming' THEN source_amount ELSE -source_amount END)) >= 100000  THEN 'NOTABLE_NET_IMBALANCE'
        ELSE 'BALANCED'
    END                                                                                        AS imbalance_flag,
    CASE
        WHEN scope = 'ARCHIVE' THEN 'P5'
        WHEN ABS(SUM(CASE WHEN io_flag = 'Incoming' THEN source_amount ELSE -source_amount END)) >= 1000000 THEN 'P2'
        WHEN ABS(SUM(CASE WHEN io_flag = 'Incoming' THEN source_amount ELSE -source_amount END)) >= 100000  THEN 'P3'
        ELSE 'P5'
    END                                                                                        AS urgency,
    CASE
        WHEN scope = 'ARCHIVE' THEN 'HISTORICAL_TREND_ANALYSIS_ONLY'
        WHEN ABS(SUM(CASE WHEN io_flag = 'Incoming' THEN source_amount ELSE -source_amount END)) >= 1000000 THEN 'INVESTIGATE_NET_FLOW_IMBALANCE'
        WHEN ABS(SUM(CASE WHEN io_flag = 'Incoming' THEN source_amount ELSE -source_amount END)) >= 100000  THEN 'MONITOR_NET_FLOW'
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
-- For each report we publish:
--   * purpose / business question / interpretation / usage_time / target_user / action_guidance
--   * important_columns_* : FULL column catalogue with type + every possible value
--                           (status, flag, bucket, urgency, recommended_action lookups)
--   * notes_*             : source tables, threshold lookups, shared enumerations
--   * live_archive_interpretation_* : how to read LIVE vs ARCHIVE rows
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
    (
        $d$reporting.rep_action_radar$d$,
        $d$OPERATIONAL_OVERVIEW$d$,
        $d$Tum kategorilerdeki acik aksiyonlari oncelik (P1..P4) ve oncelikli is sirasi ile tek bir radarda gosterir; vardiya kontrolunun cikis noktasidir.$d$,
        $d$Single radar of every open action across categories with priority (P1..P4) and ordered worklist; the entry point for shift control.$d$,
        $d$Su an hangi konuya, hangi siklikta, hangi oncelikle bakmaliyim?$d$,
        $d$What should I focus on right now, in which order, and with what priority?$d$,
        $d$Her satir bir konu kumesi (ornegin OPERATIONS/RETRIES_EXHAUSTED). open_count > 0 calisilmaya devam edildigini, urgency P1 hemen mudahale gerekir; recommended_action calismayi ne yonde yapacagimizi soyler.$d$,
        $d$Each row is an issue cluster (e.g. OPERATIONS/RETRIES_EXHAUSTED). open_count > 0 means work in progress, P1 urgency requires immediate intervention; recommended_action prescribes the corrective step.$d$,
        $d$Vardiya basinda, gun ici 60-120 dakikada bir, escalation oncesi.$d$,
        $d$At shift start, every 60-120 minutes intraday, before any escalation.$d$,
        $d$Operasyon ekibi, takim liderleri, NOC.$d$,
        $d$Operations team, team leads, NOC.$d$,
        $d$Once tum P1 satirlari kapat; ardindan oldest_age_hours buyukten kucuge sirala ve P2/P3 i temizle. Her satir icin recommended_action sutununu uygula.$d$,
        $d$Close every P1 first; then process P2/P3 sorted by oldest_age_hours descending. Follow recommended_action for each row.$d$,
        $d$[KOLONLAR]
- data_scope (text): LIVE | ARCHIVE.
- category (text): FILES | FILE_LINES | EVALUATIONS | OPERATIONS | EXECUTIONS | REVIEWS | ALERTS | ARCHIVE_RUNS.
- issue_type (text): FAILED | PROCESSING_STUCK | INCOMPLETE_DELIVERY | DUPLICATE_CONFLICT | RECON_FAILED | RECON_READY_OVERDUE | STUCK | RETRIES_EXHAUSTED | BLOCKED | LEASE_EXPIRED | MANUAL_PLANNED | HUNG | EXPIRED | OVERDUE | DELIVERY_FAILED | PENDING_STUCK | FAILED_HISTORICAL | UNMATCHED_HISTORICAL | EXPIRED_HISTORICAL | DELIVERY_FAILED_HISTORICAL.
- open_count (bigint): kategorideki acik kayit sayisi (>0 ise sirada).
- oldest_age_hours (numeric): en eski acik kaydin saat cinsinden yasi.
- urgency (text): P1 | P2 | P3 | P4. ARCHIVE her zaman P4.
- recommended_action (text): INSPECT_FILE_AND_REPROCESS | CHECK_PROCESSOR_AND_REQUEUE | REPROCESS_MISSING_LINES | RESOLVE_DUPLICATE_CONFLICTS | INVESTIGATE_RECONCILIATION_FAILURES | TRIGGER_OVERDUE_EVALUATIONS | RESTART_OR_REEVALUATE | MANUAL_INTERVENTION_REQUIRED | UNBLOCK_OR_RESOLVE_DEPENDENCY | RECLAIM_LEASE | PERFORM_MANUAL_OPERATION | KILL_AND_RESTART_EXECUTION | APPLY_EXPIRATION_DEFAULT | ESCALATE_OVERDUE_REVIEW | RESEND_OR_FIX_CHANNEL | CHECK_NOTIFICATION_PIPELINE | HISTORICAL_REVIEW_ONLY.$d$,
        $d$[COLUMNS]
- data_scope (text): LIVE | ARCHIVE.
- category (text): FILES | FILE_LINES | EVALUATIONS | OPERATIONS | EXECUTIONS | REVIEWS | ALERTS | ARCHIVE_RUNS.
- issue_type (text): FAILED | PROCESSING_STUCK | INCOMPLETE_DELIVERY | DUPLICATE_CONFLICT | RECON_FAILED | RECON_READY_OVERDUE | STUCK | RETRIES_EXHAUSTED | BLOCKED | LEASE_EXPIRED | MANUAL_PLANNED | HUNG | EXPIRED | OVERDUE | DELIVERY_FAILED | PENDING_STUCK | FAILED_HISTORICAL | UNMATCHED_HISTORICAL | EXPIRED_HISTORICAL | DELIVERY_FAILED_HISTORICAL.
- open_count (bigint): number of open records in cluster (>0 means actionable).
- oldest_age_hours (numeric): age (hours) of the oldest open record.
- urgency (text): P1 | P2 | P3 | P4. ARCHIVE rows are always P4.
- recommended_action (text): INSPECT_FILE_AND_REPROCESS | CHECK_PROCESSOR_AND_REQUEUE | REPROCESS_MISSING_LINES | RESOLVE_DUPLICATE_CONFLICTS | INVESTIGATE_RECONCILIATION_FAILURES | TRIGGER_OVERDUE_EVALUATIONS | RESTART_OR_REEVALUATE | MANUAL_INTERVENTION_REQUIRED | UNBLOCK_OR_RESOLVE_DEPENDENCY | RECLAIM_LEASE | PERFORM_MANUAL_OPERATION | KILL_AND_RESTART_EXECUTION | APPLY_EXPIRATION_DEFAULT | ESCALATE_OVERDUE_REVIEW | RESEND_OR_FIX_CHANNEL | CHECK_NOTIFICATION_PIPELINE | HISTORICAL_REVIEW_ONLY.$d$,
        $d$LIVE: aktif aksiyon listesi. ARCHIVE: gecmis is yuku ve kategori dagilim trendi (urgency=P4).$d$,
        $d$LIVE: active action list. ARCHIVE: historical workload and category-distribution trend (urgency=P4).$d$,
        $d$[KAYNAK TABLOLAR]
ingestion.file, ingestion.file_line, reconciliation.evaluation, reconciliation.operation, reconciliation.operation_execution, reconciliation.review, reconciliation.alert, archive.archive_log, archive.ingestion_file_line, archive.reconciliation_operation, archive.reconciliation_review, archive.reconciliation_alert.
[ESIK DEGERLER]
PROCESSING_STUCK: file.update_date/create_date'den 3600sn (1 saat).
RECON_READY_OVERDUE: file_line.create_date'den 86400sn (1 gun).
STUCK (evaluation): create_date'den 3600sn.
PENDING_STUCK (alert): 1800sn (30 dk).
HUNG (execution): started_at'tan 1800sn.
ARCHIVE_RUNS/STUCK: archive_log.create_date'den 3600sn.
[NOT] Sayim bazlidir; finansal etki icin rep_unmatched_financial_exposure ve rep_high_value_unmatched_transactions ile birlikte oku.
[ORTAK ENUM TABLOSU]
data_scope: LIVE | ARCHIVE
urgency: P1 (kritik, dakikalar icinde) | P2 (yuksek, saat icinde) | P3 (orta, vardiya icinde) | P4 (dusuk, gunluk) | P5 (bilgi, izleme).
[ORTAK NETWORK / SIDE]
network: BKM | VISA | MSC (Mastercard); Visa MS view'lerinde 'Visa', 'Msc', 'Bkm'.
side / file_type: Card | Clearing.
content_type: Visa | Msc | Bkm.
$d$,
        $d$[SOURCE TABLES]
ingestion.file, ingestion.file_line, reconciliation.evaluation, reconciliation.operation, reconciliation.operation_execution, reconciliation.review, reconciliation.alert, archive.archive_log, archive.ingestion_file_line, archive.reconciliation_operation, archive.reconciliation_review, archive.reconciliation_alert.
[THRESHOLDS]
PROCESSING_STUCK: 3600s after file.update_date/create_date (1h).
RECON_READY_OVERDUE: 86400s after file_line.create_date (1d).
STUCK (evaluation): 3600s after create_date.
PENDING_STUCK (alert): 1800s (30 min).
HUNG (execution): 1800s after started_at.
ARCHIVE_RUNS/STUCK: 3600s after archive_log.create_date.
[NOTE] Count-based; pair with rep_unmatched_financial_exposure and rep_high_value_unmatched_transactions for financial context.
[SHARED ENUM TABLE]
data_scope: LIVE | ARCHIVE
urgency: P1 (critical, minutes) | P2 (high, hours) | P3 (medium, within shift) | P4 (low, daily) | P5 (info, watch).
[SHARED NETWORK / SIDE]
network: BKM | VISA | MSC (Mastercard); 'Visa', 'Msc', 'Bkm' in MS variants.
side / file_type: Card | Clearing.
content_type: Visa | Msc | Bkm.
$d$
    ),
    (
        $d$reporting.rep_unhealthy_files$d$,
        $d$FILE_PROCESSING$d$,
        $d$Dosya alim kanalindaki sagliksiz dosyalari (FAILED, STUCK, INCOMPLETE, HIGH_FAILURE_RATE, PARTIAL) sebep kategorisiyle birlikte listeler; alim katmaninda mudahale planinin iskeletidir.$d$,
        $d$Lists ingestion files in unhealthy state (FAILED, STUCK, INCOMPLETE, HIGH_FAILURE_RATE, PARTIAL) with categorised root cause; the skeleton of the ingestion-layer remediation plan.$d$,
        $d$Hangi dosyalar duzgun islenmedi, neden ve nasil mudahale etmeliyim?$d$,
        $d$Which files did not process correctly, why, and how should I intervene?$d$,
        $d$issue_category dosyanin sorun tipini belirtir; recommended_action birebir yapilmasi gerekeni soyler. failure_rate_pct %20 ustu yapisal sorunu, %0 ile %20 arasi munferit hatalari isaret eder.$d$,
        $d$issue_category names the failure type; recommended_action prescribes the exact next step. failure_rate_pct above 20% indicates a structural issue; 0-20% indicates isolated errors.$d$,
        $d$Saatlik ya da her dosya alim penceresi sonrasi.$d$,
        $d$Hourly or after each ingestion window.$d$,
        $d$Dosya isleme operasyonu, destek ekibi, kanal sahibi.$d$,
        $d$File processing operations, support team, channel owner.$d$,
        $d$Sirayla: (1) FILE_REJECTED ve STUCK_PROCESSING (P1) coz; (2) INCOMPLETE_DELIVERY icin upstream kanalla iletisime gec; (3) HIGH_LINE_FAILURE_RATE icin parser/mapping degisikligi degerlendir; (4) PARTIAL_LINE_FAILURES icin failed line larin detayini ac.$d$,
        $d$In order: (1) resolve FILE_REJECTED and STUCK_PROCESSING (P1); (2) for INCOMPLETE_DELIVERY contact upstream channel; (3) review parser/mapping for HIGH_LINE_FAILURE_RATE; (4) for PARTIAL_LINE_FAILURES drill into failed lines.$d$,
        $d$[KOLONLAR]
- data_scope (text): LIVE | ARCHIVE.
- file_id (uuid).
- file_name (text).
- side (text): Card | Clearing.
- network (text): Visa | Msc | Bkm.
- file_status (text, FileStatus enum): Pending | Processing | Success | Failed.
- expected_line_count, processed_line_count, failed_line_count (bigint).
- failure_rate_pct (numeric, 0-100).
- age_hours (numeric).
- issue_category (text): FILE_REJECTED | STUCK_PROCESSING | INCOMPLETE_DELIVERY | HIGH_LINE_FAILURE_RATE | PARTIAL_LINE_FAILURES | HEALTHY.
- urgency (text): P1 | P2 | P3 | P4 (ARCHIVE) | P5.
- recommended_action (text): INSPECT_FILE_AND_REPROCESS | CHECK_PROCESSOR_AND_REQUEUE | REPROCESS_MISSING_LINES | INVESTIGATE_BULK_FAILURE_PATTERN | REVIEW_INDIVIDUAL_FAILED_LINES | NONE.
- file_message (text): kaynak sistemden donen son hata/aciklama metni.$d$,
        $d$[COLUMNS]
- data_scope (text): LIVE | ARCHIVE.
- file_id (uuid).
- file_name (text).
- side (text): Card | Clearing.
- network (text): Visa | Msc | Bkm.
- file_status (text, FileStatus enum): Pending | Processing | Success | Failed.
- expected_line_count, processed_line_count, failed_line_count (bigint).
- failure_rate_pct (numeric, 0-100).
- age_hours (numeric).
- issue_category (text): FILE_REJECTED | STUCK_PROCESSING | INCOMPLETE_DELIVERY | HIGH_LINE_FAILURE_RATE | PARTIAL_LINE_FAILURES | HEALTHY.
- urgency (text): P1 | P2 | P3 | P4 (ARCHIVE) | P5.
- recommended_action (text): INSPECT_FILE_AND_REPROCESS | CHECK_PROCESSOR_AND_REQUEUE | REPROCESS_MISSING_LINES | INVESTIGATE_BULK_FAILURE_PATTERN | REVIEW_INDIVIDUAL_FAILED_LINES | NONE.
- file_message (text): last error/info text returned by the upstream parser.$d$,
        $d$LIVE: hemen aksiyon. ARCHIVE: gecmis hata profili ve trend (urgency=P4).$d$,
        $d$LIVE: act now. ARCHIVE: historical failure profile and trend (urgency=P4).$d$,
        $d$[KAYNAK TABLOLAR] ingestion.file, archive.ingestion_file.
[ESIK DEGERLER] STUCK_PROCESSING: 3600sn. HIGH_LINE_FAILURE_RATE: failed/processed >= %20.
[NOT] Tek dosya birden fazla kategoriye dusebilir; toplama yaparken file_id distinct alinmalidir.
[ORTAK ENUM TABLOSU]
data_scope: LIVE | ARCHIVE
urgency: P1 (kritik, dakikalar icinde) | P2 (yuksek, saat icinde) | P3 (orta, vardiya icinde) | P4 (dusuk, gunluk) | P5 (bilgi, izleme).
[ORTAK NETWORK / SIDE]
network: BKM | VISA | MSC (Mastercard); Visa MS view'lerinde 'Visa', 'Msc', 'Bkm'.
side / file_type: Card | Clearing.
content_type: Visa | Msc | Bkm.
$d$,
        $d$[SOURCE TABLES] ingestion.file, archive.ingestion_file.
[THRESHOLDS] STUCK_PROCESSING: 3600s. HIGH_LINE_FAILURE_RATE: failed/processed >= 20%.
[NOTE] A single file may match multiple categories; aggregate by distinct file_id.
[SHARED ENUM TABLE]
data_scope: LIVE | ARCHIVE
urgency: P1 (critical, minutes) | P2 (high, hours) | P3 (medium, within shift) | P4 (low, daily) | P5 (info, watch).
[SHARED NETWORK / SIDE]
network: BKM | VISA | MSC (Mastercard); 'Visa', 'Msc', 'Bkm' in MS variants.
side / file_type: Card | Clearing.
content_type: Visa | Msc | Bkm.
$d$
    ),
    (
        $d$reporting.rep_stuck_pipeline_items$d$,
        $d$PIPELINE_HEALTH$d$,
        $d$Pipeline asamalarinda (line / evaluation / operation / execution) takili kalmis kayitlari yas ve durum bilgisiyle gosterir; SRE ve operasyonun ilk teshis kaynagidir.$d$,
        $d$Surfaces records stuck at any pipeline stage (line / evaluation / operation / execution) with age and state; first diagnostic source for SRE and operations.$d$,
        $d$Pipeline da neler takildi, en uzun ne kadardir bekliyor ve hangi state te?$d$,
        $d$What is stuck in the pipeline, how long has it been waiting, and in which state?$d$,
        $d$stuck_minutes ile stuck_state birlikte aciliyeti belirler. LEASE_EXPIRED ve LONG_STUCK kritik; HUNG_EXECUTION sonlandirma gerektirir.$d$,
        $d$stuck_minutes plus stuck_state set urgency together. LEASE_EXPIRED and LONG_STUCK are critical; HUNG_EXECUTION needs to be terminated.$d$,
        $d$Surekli izleme; alarm aldiginda ilk acilacak rapor.$d$,
        $d$Continuous monitoring; the first report to open after an alert.$d$,
        $d$Operasyon, SRE, platform ekibi.$d$,
        $d$Operations, SRE, platform team.$d$,
        $d$LEASE_EXPIRED operasyonlarda lease serbest birakilir ve yeniden zamanlanir; HUNG_EXECUTION sonlandirilir; ayni stage te toplu tikaniklik altyapi alarmi olarak escalate edilir.$d$,
        $d$Release the lease and reschedule LEASE_EXPIRED operations; terminate HUNG_EXECUTION; bulk stalls at the same stage are escalated as an infrastructure alert.$d$,
        $d$[KOLONLAR]
- data_scope (text): LIVE | ARCHIVE.
- stage (text): FILE_LINE | EVALUATION | OPERATION | EXECUTION.
- item_id (text): asama kaydinin ID si.
- related_id (text): ust kaydin ID si (file_id / file_line_id / evaluation_id / operation_id).
- started_at (timestamp): UTC.
- stuck_minutes (numeric).
- lease_owner (text, nullable): yalnizca OPERATION asamasinda dolu (executor adi).
- lease_expires_at (timestamp, nullable): yalnizca OPERATION asamasinda.
- stuck_state (text): LEASE_EXPIRED | LONG_STUCK (>4 saat) | STUCK.
- urgency (text): P1 | P2 | P4 (ARCHIVE).
- recommended_action (text): REQUEUE_FILE_LINE_FOR_PROCESSING | RESTART_OR_REEVALUATE | RECLAIM_LEASE_AND_RESCHEDULE | INSPECT_OPERATION_AND_RESCHEDULE | KILL_HUNG_EXECUTION_AND_RETRY | INVESTIGATE.$d$,
        $d$[COLUMNS]
- data_scope (text): LIVE | ARCHIVE.
- stage (text): FILE_LINE | EVALUATION | OPERATION | EXECUTION.
- item_id (text): id of the stuck record.
- related_id (text): parent id (file_id / file_line_id / evaluation_id / operation_id).
- started_at (timestamp, UTC).
- stuck_minutes (numeric).
- lease_owner (text, nullable): set only for OPERATION stage.
- lease_expires_at (timestamp, nullable): set only for OPERATION stage.
- stuck_state (text): LEASE_EXPIRED | LONG_STUCK (>4h) | STUCK.
- urgency (text): P1 | P2 | P4 (ARCHIVE).
- recommended_action (text): REQUEUE_FILE_LINE_FOR_PROCESSING | RESTART_OR_REEVALUATE | RECLAIM_LEASE_AND_RESCHEDULE | INSPECT_OPERATION_AND_RESCHEDULE | KILL_HUNG_EXECUTION_AND_RETRY | INVESTIGATE.$d$,
        $d$LIVE: aktif tikaniklik. ARCHIVE: gecmis tikanikliklarin kaniti ve frekansi.$d$,
        $d$LIVE: active stalls. ARCHIVE: evidence and frequency of past stalls.$d$,
        $d$[KAYNAK TABLOLAR] ingestion.file_line, reconciliation.evaluation, reconciliation.operation, reconciliation.operation_execution + ARCHIVE muadilleri.
[ESIK DEGERLER] FILE_LINE/EVALUATION/OPERATION takilma esik 3600sn (1 saat); EXECUTION 1800sn (30 dk); LONG_STUCK 14400sn (4 saat).
[NOT] OPERATION asamasinda lease_expires_at ile NOW() farki gercek bekleme suresini verir.
[ORTAK ENUM TABLOSU]
data_scope: LIVE | ARCHIVE
urgency: P1 (kritik, dakikalar icinde) | P2 (yuksek, saat icinde) | P3 (orta, vardiya icinde) | P4 (dusuk, gunluk) | P5 (bilgi, izleme).
[ORTAK NETWORK / SIDE]
network: BKM | VISA | MSC (Mastercard); Visa MS view'lerinde 'Visa', 'Msc', 'Bkm'.
side / file_type: Card | Clearing.
content_type: Visa | Msc | Bkm.
$d$,
        $d$[SOURCE TABLES] ingestion.file_line, reconciliation.evaluation, reconciliation.operation, reconciliation.operation_execution + ARCHIVE counterparts.
[THRESHOLDS] FILE_LINE/EVALUATION/OPERATION stuck threshold 3600s (1h); EXECUTION 1800s (30 min); LONG_STUCK 14400s (4h).
[NOTE] On OPERATION rows the wait time is NOW() - lease_expires_at.
[SHARED ENUM TABLE]
data_scope: LIVE | ARCHIVE
urgency: P1 (critical, minutes) | P2 (high, hours) | P3 (medium, within shift) | P4 (low, daily) | P5 (info, watch).
[SHARED NETWORK / SIDE]
network: BKM | VISA | MSC (Mastercard); 'Visa', 'Msc', 'Bkm' in MS variants.
side / file_type: Card | Clearing.
content_type: Visa | Msc | Bkm.
$d$
    ),
    (
        $d$reporting.rep_recon_failure_categorization$d$,
        $d$RECONCILIATION$d$,
        $d$Basarisiz reconciliation operasyonlarini operation_code ve branch bazinda gruplayip last_error metnine bakarak olasi kok nedeni etiketler.$d$,
        $d$Groups failed reconciliation operations by operation_code and branch and tags the most likely root cause from the last_error text.$d$,
        $d$Reconciliation neden basarisiz oluyor, hangi tip hata baskin ve nereyi onarmaliyim?$d$,
        $d$Why is reconciliation failing, which error type dominates, and where should we fix it?$d$,
        $d$likely_root_cause arastirmanin baslangic noktasidir; retries_exhausted_count > 0 retry mekanizmasinin yetmedigini, manuel mudahale gerektigini gosterir.$d$,
        $d$likely_root_cause is the investigation entry point; retries_exhausted_count > 0 means retries did not help and manual intervention is needed.$d$,
        $d$Gunluk; ayrica hata sicramasinda anlik.$d$,
        $d$Daily; also ad-hoc on failure spikes.$d$,
        $d$Reconciliation operasyonu, gelistirme ekibi, SRE.$d$,
        $d$Reconciliation operations, engineering, SRE.$d$,
        $d$Once retries_exhausted satirlarini manuel coz; sonra root_cause kategorisine gore dagit (TIMEOUT/CONNECTIVITY -> SRE, MISSING_DATA/VALIDATION -> upstream, BUSINESS_RULE_FAILURE -> kural sahibi).$d$,
        $d$Resolve retries_exhausted rows manually first; then dispatch by root_cause (TIMEOUT/CONNECTIVITY -> SRE, MISSING_DATA/VALIDATION -> upstream, BUSINESS_RULE_FAILURE -> rule owner).$d$,
        $d$[KOLONLAR]
- data_scope (text): LIVE | ARCHIVE.
- operation_code (text): operasyonun is kodu.
- branch (text): operasyon branch'i.
- failed_count, retries_exhausted_count, manual_operation_count (bigint).
- oldest_failure_at (timestamp), oldest_age_hours (numeric).
- likely_root_cause (text): TIMEOUT_OR_LATENCY | DUPLICATE_OR_CONFLICT | MISSING_DATA | AUTHORIZATION | CONNECTIVITY | VALIDATION | BUSINESS_RULE_FAILURE | UNCATEGORIZED.
- urgency (text): P1 | P2 | P3 | P4 (ARCHIVE).
- recommended_action (text): MANUAL_INTERVENTION_FOR_EXHAUSTED_RETRIES | INVESTIGATE_BULK_FAILURE_PATTERN | MONITOR_AUTO_RETRY_OUTCOME | HISTORICAL_TREND_ANALYSIS_ONLY.$d$,
        $d$[COLUMNS]
- data_scope (text): LIVE | ARCHIVE.
- operation_code (text).
- branch (text).
- failed_count, retries_exhausted_count, manual_operation_count (bigint).
- oldest_failure_at (timestamp), oldest_age_hours (numeric).
- likely_root_cause (text): TIMEOUT_OR_LATENCY | DUPLICATE_OR_CONFLICT | MISSING_DATA | AUTHORIZATION | CONNECTIVITY | VALIDATION | BUSINESS_RULE_FAILURE | UNCATEGORIZED.
- urgency (text): P1 | P2 | P3 | P4 (ARCHIVE).
- recommended_action (text): MANUAL_INTERVENTION_FOR_EXHAUSTED_RETRIES | INVESTIGATE_BULK_FAILURE_PATTERN | MONITOR_AUTO_RETRY_OUTCOME | HISTORICAL_TREND_ANALYSIS_ONLY.$d$,
        $d$LIVE: aktif cozum surec listesi. ARCHIVE: hata profili degisimini izlemek icin.$d$,
        $d$LIVE: active resolution worklist. ARCHIVE: track the change in error profile over time.$d$,
        $d$[KAYNAK TABLOLAR] reconciliation.operation, archive.reconciliation_operation.
[ESIK DEGERLER] failed_count > 50 -> P2; retries_exhausted_count > 0 -> P1.
[NOT] likely_root_cause heuristic ILIKE eslesmesidir; yeni bir hata kategorisi cikar cikmaz aciklamasi runbook a eklenmelidir.
[ORTAK ENUM TABLOSU]
data_scope: LIVE | ARCHIVE
urgency: P1 (kritik, dakikalar icinde) | P2 (yuksek, saat icinde) | P3 (orta, vardiya icinde) | P4 (dusuk, gunluk) | P5 (bilgi, izleme).
[ORTAK NETWORK / SIDE]
network: BKM | VISA | MSC (Mastercard); Visa MS view'lerinde 'Visa', 'Msc', 'Bkm'.
side / file_type: Card | Clearing.
content_type: Visa | Msc | Bkm.
$d$,
        $d$[SOURCE TABLES] reconciliation.operation, archive.reconciliation_operation.
[THRESHOLDS] failed_count > 50 -> P2; retries_exhausted_count > 0 -> P1.
[NOTE] likely_root_cause is heuristic ILIKE matching; whenever a new error category appears, document it in the runbook.
[SHARED ENUM TABLE]
data_scope: LIVE | ARCHIVE
urgency: P1 (critical, minutes) | P2 (high, hours) | P3 (medium, within shift) | P4 (low, daily) | P5 (info, watch).
[SHARED NETWORK / SIDE]
network: BKM | VISA | MSC (Mastercard); 'Visa', 'Msc', 'Bkm' in MS variants.
side / file_type: Card | Clearing.
content_type: Visa | Msc | Bkm.
$d$
    ),
    (
        $d$reporting.rep_manual_review_pressure$d$,
        $d$MANUAL_REVIEW$d$,
        $d$Manuel review kuyrugunda biriken kararlarin SLA baskisini (EXPIRED / EXPIRING_SOON / OVERDUE / NORMAL) ve askidaki finansal hacmi gosterir.$d$,
        $d$Shows SLA pressure on the manual-review queue (EXPIRED / EXPIRING_SOON / OVERDUE / NORMAL) plus the financial volume on hold.$d$,
        $d$Hangi review kararlari SLA yi asti veya asacak, ne kadar para askida?$d$,
        $d$Which review decisions breached SLA or are about to, and how much money is on hold?$d$,
        $d$EXPIRED ve EXPIRING_SOON acildir; exposure_amount karar gecikmesinin parasal buyuklugudur ve currency bazinda ayrilir.$d$,
        $d$EXPIRED and EXPIRING_SOON are urgent; exposure_amount is the monetary impact of decision delay and is split by currency.$d$,
        $d$Vardiya basi, ogle kontrolu, vardiya sonu.$d$,
        $d$Shift start, midday check, shift end.$d$,
        $d$Backoffice review ekibi, ekip lideri.$d$,
        $d$Backoffice review team, team lead.$d$,
        $d$EXPIRED grubuna runbook taki default-action uygulanir veya derhal karar verilir; OVERDUE siraya alinir; NORMAL izlemede tutulur.$d$,
        $d$Apply the runbook default-action to EXPIRED or decide immediately; queue OVERDUE next; keep NORMAL under monitoring.$d$,
        $d$[KOLONLAR]
- data_scope (text): LIVE | ARCHIVE.
- sla_bucket (text): EXPIRED | EXPIRING_SOON (4 saat icinde) | OVERDUE (24 saat ustu) | NORMAL.
- default_on_expiry (text, ReviewExpirationAction enum): None | AutoApprove | AutoReject | EscalateToSupervisor | ApplyDefaultPolicy.
- currency (text): kayittaki original/source currency veya 'UNKNOWN'.
- pending_review_count (bigint).
- oldest_waiting_hours (numeric).
- exposure_amount (numeric): toplam askidaki tutar.
- urgency (text): P1 | P2 | P3 | P4 (ARCHIVE).
- recommended_action (text): APPLY_DEFAULT_OR_DECIDE_NOW | DECIDE_BEFORE_EXPIRY | ESCALATE_OVERDUE_REVIEWS | WORK_THE_QUEUE | HISTORICAL_REFERENCE_ONLY.$d$,
        $d$[COLUMNS]
- data_scope (text): LIVE | ARCHIVE.
- sla_bucket (text): EXPIRED | EXPIRING_SOON (within 4h) | OVERDUE (>24h) | NORMAL.
- default_on_expiry (text, ReviewExpirationAction enum): None | AutoApprove | AutoReject | EscalateToSupervisor | ApplyDefaultPolicy.
- currency (text): original/source currency or 'UNKNOWN'.
- pending_review_count (bigint).
- oldest_waiting_hours (numeric).
- exposure_amount (numeric): total monetary value on hold.
- urgency (text): P1 | P2 | P3 | P4 (ARCHIVE).
- recommended_action (text): APPLY_DEFAULT_OR_DECIDE_NOW | DECIDE_BEFORE_EXPIRY | ESCALATE_OVERDUE_REVIEWS | WORK_THE_QUEUE | HISTORICAL_REFERENCE_ONLY.$d$,
        $d$LIVE: bekleyen kararlar. ARCHIVE: tarihi review baskisi ve SLA basari trendi.$d$,
        $d$LIVE: pending decisions. ARCHIVE: historical review pressure and SLA achievement trend.$d$,
        $d$[KAYNAK TABLOLAR] reconciliation.review, archive.reconciliation_review + 6 detail tablosu (card_visa/msc/bkm, clearing_visa/msc/bkm) tutar icin LATERAL JOIN.
[ESIK DEGERLER] OVERDUE: 24 saat. EXPIRING_SOON: NOW + 4 saat.
[NOT] Currency bazinda ayrilir; toplam alirken currency degisikligini kontrol etmek gerekir.
[REVIEW DECISION ENUM] Pending | Approved | Rejected.
[ORTAK ENUM TABLOSU]
data_scope: LIVE | ARCHIVE
urgency: P1 (kritik, dakikalar icinde) | P2 (yuksek, saat icinde) | P3 (orta, vardiya icinde) | P4 (dusuk, gunluk) | P5 (bilgi, izleme).
[ORTAK NETWORK / SIDE]
network: BKM | VISA | MSC (Mastercard); Visa MS view'lerinde 'Visa', 'Msc', 'Bkm'.
side / file_type: Card | Clearing.
content_type: Visa | Msc | Bkm.
$d$,
        $d$[SOURCE TABLES] reconciliation.review, archive.reconciliation_review + 6 detail tables (card_visa/msc/bkm, clearing_visa/msc/bkm) joined LATERALly for amount.
[THRESHOLDS] OVERDUE: 24h. EXPIRING_SOON: NOW + 4h.
[NOTE] Split by currency; verify boundaries before summing.
[REVIEW DECISION ENUM] Pending | Approved | Rejected.
[SHARED ENUM TABLE]
data_scope: LIVE | ARCHIVE
urgency: P1 (critical, minutes) | P2 (high, hours) | P3 (medium, within shift) | P4 (low, daily) | P5 (info, watch).
[SHARED NETWORK / SIDE]
network: BKM | VISA | MSC (Mastercard); 'Visa', 'Msc', 'Bkm' in MS variants.
side / file_type: Card | Clearing.
content_type: Visa | Msc | Bkm.
$d$
    ),
    (
        $d$reporting.rep_alert_delivery_health$d$,
        $d$ALERTS$d$,
        $d$Bildirim altyapisinin teslim sagligini alert_type ve severity bazinda olcer; ileti ulasamayan kanallari ortaya cikarir.$d$,
        $d$Measures notification-delivery health per alert_type and severity; surfaces channels where messages do not arrive.$d$,
        $d$Bildirim altyapimiz saglikli mi, hangi kanal/tip bozuk?$d$,
        $d$Is our notification stack healthy, which channel or type is broken?$d$,
        $d$delivery_health_status CRITICAL kanal kirik; DEGRADED uzun bekleyen pending; WARMING sirayla isleniyor; HEALTHY normal akis.$d$,
        $d$CRITICAL means the channel is broken; DEGRADED indicates long-pending notifications; WARMING means draining; HEALTHY is normal flow.$d$,
        $d$Surekli izleme; CRITICAL dakika cinsinden ele alinmalidir.$d$,
        $d$Continuous monitoring; CRITICAL must be addressed within minutes.$d$,
        $d$Operasyon, platform, alarm sahibi takim.$d$,
        $d$Operations, platform, alert owner team.$d$,
        $d$CRITICAL satirlarinda failed alert ler yeniden gonderilir; kanal saglik kontrolu (transport, kotalar, kimlik dogrulama) yapilir; tekrari icin upstream e ticket acilir.$d$,
        $d$On CRITICAL rows resend failed alerts; perform channel health checks (transport, quotas, auth); open an upstream ticket to prevent recurrence.$d$,
        $d$[KOLONLAR]
- data_scope (text): LIVE | ARCHIVE.
- severity (text, AlertSeverity enum): Info | Low | Medium | High | Critical | UNKNOWN.
- alert_type (text, AlertType enum): MatchFailure | ThresholdBreach | DataQuality | LicenseExpiry | OperationalIncident | Custom | UNKNOWN.
- total_count (bigint).
- pending_count, failed_count, sent_count (bigint).
- failure_rate_pct (numeric, 0-100).
- oldest_open_age_hours (numeric, nullable).
- delivery_health_status (text): CRITICAL | DEGRADED | WARMING | HEALTHY | HISTORICAL.
- urgency (text): P1 | P2 | P3 | P4 (ARCHIVE) | P5.
- recommended_action (text): RESEND_OR_FIX_DELIVERY_CHANNEL | CHECK_NOTIFICATION_PIPELINE | MONITOR_PIPELINE | NONE | HISTORICAL_REFERENCE_ONLY.$d$,
        $d$[COLUMNS]
- data_scope (text): LIVE | ARCHIVE.
- severity (text, AlertSeverity enum): Info | Low | Medium | High | Critical | UNKNOWN.
- alert_type (text, AlertType enum): MatchFailure | ThresholdBreach | DataQuality | LicenseExpiry | OperationalIncident | Custom | UNKNOWN.
- total_count (bigint).
- pending_count, failed_count, sent_count (bigint).
- failure_rate_pct (numeric, 0-100).
- oldest_open_age_hours (numeric, nullable).
- delivery_health_status (text): CRITICAL | DEGRADED | WARMING | HEALTHY | HISTORICAL.
- urgency (text): P1 | P2 | P3 | P4 (ARCHIVE) | P5.
- recommended_action (text): RESEND_OR_FIX_DELIVERY_CHANNEL | CHECK_NOTIFICATION_PIPELINE | MONITOR_PIPELINE | NONE | HISTORICAL_REFERENCE_ONLY.$d$,
        $d$LIVE: gercek zamanli kanal sagligi. ARCHIVE: kanal guvenilirligi gecmisi.$d$,
        $d$LIVE: real-time channel health. ARCHIVE: historical channel reliability.$d$,
        $d$[KAYNAK TABLOLAR] reconciliation.alert, archive.reconciliation_alert.
[ALERT STATUS ENUM] Pending | Failed | Consumed (sent_count Consumed sayisidir).
[ESIK] DEGRADED: pending icin 1800sn (30 dk).
[NOT] Sent orani dususe, alarm uretim sureci de gozden gecirilmelidir.
[ORTAK ENUM TABLOSU]
data_scope: LIVE | ARCHIVE
urgency: P1 (kritik, dakikalar icinde) | P2 (yuksek, saat icinde) | P3 (orta, vardiya icinde) | P4 (dusuk, gunluk) | P5 (bilgi, izleme).
[ORTAK NETWORK / SIDE]
network: BKM | VISA | MSC (Mastercard); Visa MS view'lerinde 'Visa', 'Msc', 'Bkm'.
side / file_type: Card | Clearing.
content_type: Visa | Msc | Bkm.
$d$,
        $d$[SOURCE TABLES] reconciliation.alert, archive.reconciliation_alert.
[ALERT STATUS ENUM] Pending | Failed | Consumed (sent_count uses Consumed).
[THRESHOLDS] DEGRADED: pending older than 1800s (30 min).
[NOTE] If sent ratio drops, also review the alert evaluation step.
[SHARED ENUM TABLE]
data_scope: LIVE | ARCHIVE
urgency: P1 (critical, minutes) | P2 (high, hours) | P3 (medium, within shift) | P4 (low, daily) | P5 (info, watch).
[SHARED NETWORK / SIDE]
network: BKM | VISA | MSC (Mastercard); 'Visa', 'Msc', 'Bkm' in MS variants.
side / file_type: Card | Clearing.
content_type: Visa | Msc | Bkm.
$d$
    ),
    (
        $d$reporting.rep_unmatched_financial_exposure$d$,
        $d$FINANCIAL_RISK$d$,
        $d$Eslesmemis is islemleri scope, side, network, currency ve yas kovasina (0-1d, 1-3d, 3-7d, 7-30d, 30d+) gore toplayarak finansal riski ortaya koyar.$d$,
        $d$Aggregates unmatched transactions by scope, side, network, currency and aging bucket (0-1d, 1-3d, 3-7d, 7-30d, 30d+) to expose financial risk.$d$,
        $d$Eslesmemis ne kadar para var, nerede yaslaniyor, hangi network ve currency de?$d$,
        $d$How much money is unmatched, where is it aging, in which network and currency?$d$,
        $d$risk_flag CRITICAL veya HIGH ise finansal mudahale gerekir; oldest_age_days ne kadar gec kalindigini gosterir; aging_bucket yaslanmayi onceliklendirir.$d$,
        $d$risk_flag CRITICAL or HIGH requires financial intervention; oldest_age_days shows how late we are; aging_bucket prioritises the most aged items.$d$,
        $d$Gunluk finansal kapanis, haftalik risk degerlendirmesi, ay sonu denetim oncesi.$d$,
        $d$Daily financial close, weekly risk review, before month-end audit.$d$,
        $d$Finans operasyonu, recon ekibi, risk, denetim.$d$,
        $d$Finance operations, recon team, risk, audit.$d$,
        $d$CRITICAL satirlar derhal risk komitesine eskale; HIGH icin kaynak (network/issuer) bazinda incele; LOW izlemede tutulur.$d$,
        $d$Escalate CRITICAL rows to the risk committee; investigate HIGH by source (network/issuer); keep LOW on watch.$d$,
        $d$[KOLONLAR]
- data_scope (text): LIVE | ARCHIVE.
- side (text): Card | Clearing.
- network (text): Visa | Msc | Bkm.
- currency (text): ISO 4217 numerik (orn 949=TRY, 840=USD, 978=EUR) veya 'UNKNOWN'.
- aging_bucket (text): 0-1d | 1-3d | 3-7d | 7-30d | 30d+.
- unmatched_count (bigint), exposure_amount (numeric).
- oldest_unmatched_at (timestamp), oldest_age_days (numeric).
- risk_flag (text): CRITICAL | HIGH | MEDIUM | LOW | HISTORICAL.
- urgency (text): P1 | P2 | P3 | P4.
- recommended_action (text): ESCALATE_AGED_HIGH_VALUE_EXPOSURE | INVESTIGATE_PERSISTENT_UNMATCHED | MONITOR | HISTORICAL_TREND_ANALYSIS_ONLY.$d$,
        $d$[COLUMNS]
- data_scope (text): LIVE | ARCHIVE.
- side (text): Card | Clearing.
- network (text): Visa | Msc | Bkm.
- currency (text): ISO 4217 numeric (e.g. 949=TRY, 840=USD, 978=EUR) or 'UNKNOWN'.
- aging_bucket (text): 0-1d | 1-3d | 3-7d | 7-30d | 30d+.
- unmatched_count (bigint), exposure_amount (numeric).
- oldest_unmatched_at (timestamp), oldest_age_days (numeric).
- risk_flag (text): CRITICAL | HIGH | MEDIUM | LOW | HISTORICAL.
- urgency (text): P1 | P2 | P3 | P4.
- recommended_action (text): ESCALATE_AGED_HIGH_VALUE_EXPOSURE | INVESTIGATE_PERSISTENT_UNMATCHED | MONITOR | HISTORICAL_TREND_ANALYSIS_ONLY.$d$,
        $d$LIVE: aktif para riski. ARCHIVE: tarihsel exposure trendi ve mevsimsel desen.$d$,
        $d$LIVE: active financial risk. ARCHIVE: historical exposure trend and seasonal pattern.$d$,
        $d$[KAYNAK TABLOLAR] ingestion.file_line + ingestion.file + 6 detail (card_visa/msc/bkm, clearing_visa/msc/bkm) ve ARCHIVE muadilleri.
[ESIK DEGERLER] CRITICAL: exposure_amount >= 1.000.000 veya oldest_age_days >= 7. HIGH: >= 100.000 veya >= 3 gun. MEDIUM: >= 1 gun.
[NOT] Currency='UNKNOWN' veri kalitesi sorununa isaret eder.
[ORTAK ENUM TABLOSU]
data_scope: LIVE | ARCHIVE
urgency: P1 (kritik, dakikalar icinde) | P2 (yuksek, saat icinde) | P3 (orta, vardiya icinde) | P4 (dusuk, gunluk) | P5 (bilgi, izleme).
[ORTAK NETWORK / SIDE]
network: BKM | VISA | MSC (Mastercard); Visa MS view'lerinde 'Visa', 'Msc', 'Bkm'.
side / file_type: Card | Clearing.
content_type: Visa | Msc | Bkm.
$d$,
        $d$[SOURCE TABLES] ingestion.file_line + ingestion.file + 6 detail tables (card_visa/msc/bkm, clearing_visa/msc/bkm) and ARCHIVE counterparts.
[THRESHOLDS] CRITICAL: exposure_amount >= 1,000,000 or oldest_age_days >= 7. HIGH: >= 100,000 or >= 3 days. MEDIUM: >= 1 day.
[NOTE] Currency='UNKNOWN' signals a data-quality issue.
[SHARED ENUM TABLE]
data_scope: LIVE | ARCHIVE
urgency: P1 (critical, minutes) | P2 (high, hours) | P3 (medium, within shift) | P4 (low, daily) | P5 (info, watch).
[SHARED NETWORK / SIDE]
network: BKM | VISA | MSC (Mastercard); 'Visa', 'Msc', 'Bkm' in MS variants.
side / file_type: Card | Clearing.
content_type: Visa | Msc | Bkm.
$d$
    ),
    (
        $d$reporting.rep_card_clearing_imbalance$d$,
        $d$FINANCIAL_RECONCILIATION$d$,
        $d$Card ile Clearing taraflari arasindaki gunluk tutar farkini network ve currency bazinda olcer ve dengesizligin maddiyetini etiketler.$d$,
        $d$Measures the daily amount gap between Card and Clearing sides per network and currency, and labels the materiality of the imbalance.$d$,
        $d$Card ve Clearing taraflarimiz parasal olarak dengede mi, fark net olarak ne kadar?$d$,
        $d$Are Card and Clearing sides financially in balance, what is the net gap?$d$,
        $d$imbalance_severity MATERIAL_IMBALANCE ise reconciliation kalitesi bozulmustur; gap_ratio_pct buyuklugu yuzdesel olarak gosterir; BALANCED satirlar saglikidir.$d$,
        $d$MATERIAL_IMBALANCE means reconciliation quality has broken; gap_ratio_pct shows the magnitude as a percentage; BALANCED rows are healthy.$d$,
        $d$Her gun T+1 finansal kapanis kontrolunde.$d$,
        $d$Daily on T+1 financial close control.$d$,
        $d$Recon ekibi, finans, denetim.$d$,
        $d$Recon team, finance, audit.$d$,
        $d$MATERIAL_IMBALANCE de hemen escalate; eslesmemis is satirlarini ve hatali response_code ozetlerini birlikte incele.$d$,
        $d$Escalate MATERIAL_IMBALANCE immediately; jointly inspect unmatched rows and faulty response_code summaries.$d$,
        $d$[KOLONLAR]
- data_scope (text): LIVE | ARCHIVE.
- report_date (date): file.create_date'in gunu.
- network (text): Visa | Msc | Bkm.
- currency (text) veya 'UNKNOWN'.
- card_line_count, clearing_line_count (bigint).
- card_total_amount, clearing_total_amount (numeric).
- amount_gap (numeric, isaretli), abs_gap (numeric).
- gap_ratio_pct (numeric, 0-100).
- imbalance_severity (text): BALANCED | MINOR_DRIFT (<%1) | NOTABLE_GAP (<%5) | MATERIAL_IMBALANCE.
- urgency (text): P1 | P2 | P3 | P4 (ARCHIVE) | P5 (BALANCED).
- recommended_action (text): ESCALATE_AND_INVESTIGATE_MATERIAL_GAP | INVESTIGATE_NOTABLE_GAP | MONITOR_DRIFT | NONE | HISTORICAL_TREND_ANALYSIS_ONLY.$d$,
        $d$[COLUMNS]
- data_scope (text): LIVE | ARCHIVE.
- report_date (date): truncated file.create_date.
- network (text): Visa | Msc | Bkm.
- currency (text) or 'UNKNOWN'.
- card_line_count, clearing_line_count (bigint).
- card_total_amount, clearing_total_amount (numeric).
- amount_gap (numeric, signed), abs_gap (numeric).
- gap_ratio_pct (numeric, 0-100).
- imbalance_severity (text): BALANCED | MINOR_DRIFT (<1%) | NOTABLE_GAP (<5%) | MATERIAL_IMBALANCE.
- urgency (text): P1 | P2 | P3 | P4 (ARCHIVE) | P5 (BALANCED).
- recommended_action (text): ESCALATE_AND_INVESTIGATE_MATERIAL_GAP | INVESTIGATE_NOTABLE_GAP | MONITOR_DRIFT | NONE | HISTORICAL_TREND_ANALYSIS_ONLY.$d$,
        $d$LIVE: gunluk kapanis kontrolu. ARCHIVE: tarihsel uyum profili ve trend.$d$,
        $d$LIVE: daily close check. ARCHIVE: historical alignment profile and trend.$d$,
        $d$[KAYNAK TABLOLAR] ingestion.file/file_line + 6 detay tablo + ARCHIVE muadilleri.
[ESIK DEGERLER] gap_ratio < %1 -> MINOR_DRIFT, < %5 -> NOTABLE_GAP, >= %5 -> MATERIAL_IMBALANCE.
[NOT] BALANCED satirlarda ek aksiyon gerekmez fakat trend takibinde tutmak gerekir.
[ORTAK ENUM TABLOSU]
data_scope: LIVE | ARCHIVE
urgency: P1 (kritik, dakikalar icinde) | P2 (yuksek, saat icinde) | P3 (orta, vardiya icinde) | P4 (dusuk, gunluk) | P5 (bilgi, izleme).
[ORTAK NETWORK / SIDE]
network: BKM | VISA | MSC (Mastercard); Visa MS view'lerinde 'Visa', 'Msc', 'Bkm'.
side / file_type: Card | Clearing.
content_type: Visa | Msc | Bkm.
$d$,
        $d$[SOURCE TABLES] ingestion.file/file_line + 6 detail tables + ARCHIVE counterparts.
[THRESHOLDS] gap_ratio < 1% -> MINOR_DRIFT, < 5% -> NOTABLE_GAP, >= 5% -> MATERIAL_IMBALANCE.
[NOTE] BALANCED rows need no extra action but should be tracked for trend.
[SHARED ENUM TABLE]
data_scope: LIVE | ARCHIVE
urgency: P1 (critical, minutes) | P2 (high, hours) | P3 (medium, within shift) | P4 (low, daily) | P5 (info, watch).
[SHARED NETWORK / SIDE]
network: BKM | VISA | MSC (Mastercard); 'Visa', 'Msc', 'Bkm' in MS variants.
side / file_type: Card | Clearing.
content_type: Visa | Msc | Bkm.
$d$
    ),
    (
        $d$reporting.rep_reconciliation_quality_score$d$,
        $d$RECONCILIATION_KPI$d$,
        $d$Network/gun bazinda match-rate, recon-failure, retry yogunlugu ve manuel review oranini birlestirip A-F arasinda tek bir kalite notu uretir.$d$,
        $d$Combines match-rate, recon-failure rate, retry density and manual-review ratio per network/day into a single A-F quality grade.$d$,
        $d$Reconciliation kalitemiz hangi network te bozuluyor ve neden?$d$,
        $d$On which network is reconciliation quality degrading and why?$d$,
        $d$quality_grade A en iyi, F en kotu; D-F kalite kaybi olarak ele alinir; weakest_dimension hangi metrik nedeniyle dustugumuzu soyler.$d$,
        $d$quality_grade A is best, F is worst; D-F is treated as quality loss; weakest_dimension tells which metric is dragging the score down.$d$,
        $d$Gunluk yonetim raporu, haftalik trend, ay sonu kalite degerlendirmesi.$d$,
        $d$Daily management report, weekly trend, month-end quality review.$d$,
        $d$Yonetim, urun sahibi, recon ekibi.$d$,
        $d$Management, product owner, recon team.$d$,
        $d$Note D ise weakest_dimension a yonelik cozum uygula (rule tuning, retry tuning, root cause investigation); F kuralla cozulemez, mimari incelemesi gerekir.$d$,
        $d$On grade D apply the fix targeted at weakest_dimension (rule tuning, retry tuning, root cause investigation); F cannot be solved by rules and requires architectural review.$d$,
        $d$[KOLONLAR]
- data_scope (text): LIVE | ARCHIVE.
- report_date (date), network (text).
- total_lines, matched_lines, recon_failed_lines, total_retries, reviewed_lines (bigint).
- match_rate_pct, recon_failure_rate_pct, avg_retries_per_line, manual_review_rate_pct (numeric).
- quality_grade (text): A (>=99% match & <=1% fail & <=0.05 retry & <=2% review) | B (>=95% / <=3% / <=5%) | C (>=90%) | D (>=80%) | F | N/A (veri yok).
- weakest_dimension (text): RECON_FAILURE_RATE | MATCH_RATE | RETRY_DENSITY | MANUAL_REVIEW_LOAD | NONE | NO_DATA.
- urgency (text): P1 (<%80 match) | P2 (<%90 match) | P3 | P4 (ARCHIVE).
- recommended_action (text): INVESTIGATE_LOW_MATCH_RATE_ROOT_CAUSE | INVESTIGATE_RECON_FAILURE_PATTERN | TUNE_RETRY_OR_FIX_TRANSIENT_ERRORS | REDUCE_MANUAL_REVIEW_BY_RULE_TUNING | MONITOR | HISTORICAL_TREND_ANALYSIS_ONLY.$d$,
        $d$[COLUMNS]
- data_scope (text): LIVE | ARCHIVE.
- report_date (date), network (text).
- total_lines, matched_lines, recon_failed_lines, total_retries, reviewed_lines (bigint).
- match_rate_pct, recon_failure_rate_pct, avg_retries_per_line, manual_review_rate_pct (numeric).
- quality_grade (text): A (>=99% match & <=1% fail & <=0.05 retry & <=2% review) | B (>=95% / <=3% / <=5%) | C (>=90%) | D (>=80%) | F | N/A (no data).
- weakest_dimension (text): RECON_FAILURE_RATE | MATCH_RATE | RETRY_DENSITY | MANUAL_REVIEW_LOAD | NONE | NO_DATA.
- urgency (text): P1 (<80% match) | P2 (<90% match) | P3 | P4 (ARCHIVE).
- recommended_action (text): INVESTIGATE_LOW_MATCH_RATE_ROOT_CAUSE | INVESTIGATE_RECON_FAILURE_PATTERN | TUNE_RETRY_OR_FIX_TRANSIENT_ERRORS | REDUCE_MANUAL_REVIEW_BY_RULE_TUNING | MONITOR | HISTORICAL_TREND_ANALYSIS_ONLY.$d$,
        $d$LIVE: bugunku kalite. ARCHIVE: tarihsel kalite trendi ve mevsimsellik.$d$,
        $d$LIVE: today quality. ARCHIVE: historical quality trend and seasonality.$d$,
        $d$[KAYNAK TABLOLAR] ingestion.file/file_line, reconciliation.review + ARCHIVE muadilleri.
[LineReconciliationStatus ENUM] Ready | Processing | Success | Failed.
[NOT] Toplam satir 0 ise grade N/A doner; veri yokluguna isaret eder.
[ORTAK ENUM TABLOSU]
data_scope: LIVE | ARCHIVE
urgency: P1 (kritik, dakikalar icinde) | P2 (yuksek, saat icinde) | P3 (orta, vardiya icinde) | P4 (dusuk, gunluk) | P5 (bilgi, izleme).
[ORTAK NETWORK / SIDE]
network: BKM | VISA | MSC (Mastercard); Visa MS view'lerinde 'Visa', 'Msc', 'Bkm'.
side / file_type: Card | Clearing.
content_type: Visa | Msc | Bkm.
$d$,
        $d$[SOURCE TABLES] ingestion.file/file_line, reconciliation.review + ARCHIVE counterparts.
[LineReconciliationStatus ENUM] Ready | Processing | Success | Failed.
[NOTE] When total_lines = 0 the grade is N/A; signals absence of data.
[SHARED ENUM TABLE]
data_scope: LIVE | ARCHIVE
urgency: P1 (critical, minutes) | P2 (high, hours) | P3 (medium, within shift) | P4 (low, daily) | P5 (info, watch).
[SHARED NETWORK / SIDE]
network: BKM | VISA | MSC (Mastercard); 'Visa', 'Msc', 'Bkm' in MS variants.
side / file_type: Card | Clearing.
content_type: Visa | Msc | Bkm.
$d$
    ),
    (
        $d$reporting.rep_misleading_success_cases$d$,
        $d$FINANCIAL_RISK$d$,
        $d$Adet bazinda iyi gorunen ama tutar bazinda kotu olan (veya tersi) gunleri yakalar; sayim metriklerinin gizledigi finansal riski ortaya cikarir.$d$,
        $d$Catches days that look good by line count but bad by amount (or vice versa); exposes financial risk hidden by count-based KPIs.$d$,
        $d$Iyi gorunen gunlerin altinda gizli risk var mi?$d$,
        $d$Is there hidden risk under days that look fine by count?$d$,
        $d$GOOD_COUNT_BAD_AMOUNT az sayida yuksek tutarli islem eslesmemis demektir, ciddi finansal risktir; GOOD_AMOUNT_BAD_COUNT cok sayida kucuk islem eslesmemis ama parasal etki dusuk.$d$,
        $d$GOOD_COUNT_BAD_AMOUNT means a few high-value transactions are unmatched, a serious financial risk; GOOD_AMOUNT_BAD_COUNT means many small ones are unmatched with low monetary impact.$d$,
        $d$Gunluk kapanis sonrasi finansal sanity check.$d$,
        $d$After daily close as a financial sanity check.$d$,
        $d$Finans, recon, denetim.$d$,
        $d$Finance, recon, audit.$d$,
        $d$GOOD_COUNT_BAD_AMOUNT satirlarinda yuksek tutarli eslesmeyen islemleri ayri incele (rep_high_value_unmatched_transactions ile birlikte oku).$d$,
        $d$For GOOD_COUNT_BAD_AMOUNT rows inspect unmatched high-value transactions separately (cross-read with rep_high_value_unmatched_transactions).$d$,
        $d$[KOLONLAR]
- data_scope (text): LIVE | ARCHIVE.
- report_date (date), network (text), side (text), currency (text|'UNKNOWN').
- line_count, matched_count (bigint).
- total_amount, matched_amount, unmatched_amount (numeric).
- count_match_rate_pct, amount_match_rate_pct (numeric).
- misleading_pattern (text): GOOD_COUNT_BAD_AMOUNT | GOOD_AMOUNT_BAD_COUNT | CONSISTENT.
- urgency (text): P1 (count_iyi/amount_kotu) | P2 (amount_iyi/count_kotu) | P4 (ARCHIVE) | P5.
- recommended_action (text): INVESTIGATE_HIGH_VALUE_UNMATCHED_TRANSACTIONS | INVESTIGATE_MANY_LOW_VALUE_UNMATCHED | NONE | HISTORICAL_TREND_ANALYSIS_ONLY.$d$,
        $d$[COLUMNS]
- data_scope (text): LIVE | ARCHIVE.
- report_date (date), network (text), side (text), currency (text|'UNKNOWN').
- line_count, matched_count (bigint).
- total_amount, matched_amount, unmatched_amount (numeric).
- count_match_rate_pct, amount_match_rate_pct (numeric).
- misleading_pattern (text): GOOD_COUNT_BAD_AMOUNT | GOOD_AMOUNT_BAD_COUNT | CONSISTENT.
- urgency (text): P1 (count good/amount bad) | P2 (amount good/count bad) | P4 (ARCHIVE) | P5.
- recommended_action (text): INVESTIGATE_HIGH_VALUE_UNMATCHED_TRANSACTIONS | INVESTIGATE_MANY_LOW_VALUE_UNMATCHED | NONE | HISTORICAL_TREND_ANALYSIS_ONLY.$d$,
        $d$LIVE: gunluk gizli risk taramasi. ARCHIVE: pattern in gecmiste de gorulup gorulmedigi.$d$,
        $d$LIVE: daily hidden-risk scan. ARCHIVE: whether the pattern occurred historically.$d$,
        $d$[KAYNAK TABLOLAR] ingestion.file/file_line + 6 detay tablo + ARCHIVE muadilleri.
[ESIK DEGERLER] GOOD_COUNT_BAD_AMOUNT: count_match >= %95 ve amount_match < %80. Tersi GOOD_AMOUNT_BAD_COUNT.
[NOT] CONSISTENT satirlari saglikidir, listeden filtrelenebilir.
[ORTAK ENUM TABLOSU]
data_scope: LIVE | ARCHIVE
urgency: P1 (kritik, dakikalar icinde) | P2 (yuksek, saat icinde) | P3 (orta, vardiya icinde) | P4 (dusuk, gunluk) | P5 (bilgi, izleme).
[ORTAK NETWORK / SIDE]
network: BKM | VISA | MSC (Mastercard); Visa MS view'lerinde 'Visa', 'Msc', 'Bkm'.
side / file_type: Card | Clearing.
content_type: Visa | Msc | Bkm.
$d$,
        $d$[SOURCE TABLES] ingestion.file/file_line + 6 detail tables + ARCHIVE counterparts.
[THRESHOLDS] GOOD_COUNT_BAD_AMOUNT: count_match >= 95% and amount_match < 80%. Reverse for GOOD_AMOUNT_BAD_COUNT.
[NOTE] CONSISTENT rows are healthy and may be filtered out.
[SHARED ENUM TABLE]
data_scope: LIVE | ARCHIVE
urgency: P1 (critical, minutes) | P2 (high, hours) | P3 (medium, within shift) | P4 (low, daily) | P5 (info, watch).
[SHARED NETWORK / SIDE]
network: BKM | VISA | MSC (Mastercard); 'Visa', 'Msc', 'Bkm' in MS variants.
side / file_type: Card | Clearing.
content_type: Visa | Msc | Bkm.
$d$
    ),
    (
        $d$reporting.rep_archive_pipeline_health$d$,
        $d$ARCHIVE$d$,
        $d$Arsiv pipeline inin sagligini iki perspektiften gosterir: hatali/sikismis arsiv kosumlari (RUN_FAILED/RUN_STUCK) ve arsivlenmeyi bekleyen ama 7+ gundur bekleyen uygun dosyalar (ELIGIBLE_BACKLOG).$d$,
        $d$Archive pipeline health from two perspectives: failed/stuck archive runs (RUN_FAILED/RUN_STUCK) and eligible files waiting unarchived for 7+ days (ELIGIBLE_BACKLOG).$d$,
        $d$Arsiv sureci akiyor mu, biriken dosya var mi, kosumlar saglikli mi?$d$,
        $d$Is archiving flowing, is a backlog building, are runs healthy?$d$,
        $d$pipeline_health CRITICAL kosum hatali/sikismis; DEGRADED 7+ gun arsivlenmemis dosya birikimi; WARMING devam eden kosum; HEALTHY normal.$d$,
        $d$CRITICAL pipeline_health means a run failed/stuck; DEGRADED means files sitting unarchived for 7+ days; WARMING means an in-progress run; HEALTHY is normal.$d$,
        $d$Gunluk; backlog buyurken surekli.$d$,
        $d$Daily; continuously while backlog grows.$d$,
        $d$Operasyon, veri yonetimi, compliance.$d$,
        $d$Operations, data management, compliance.$d$,
        $d$CRITICAL satirlari hemen onar; ELIGIBLE_BACKLOG icin arsiv isi tetiklenir, RUN_STUCK kosumu sonlandirilir.$d$,
        $d$Fix CRITICAL rows immediately; trigger the archive job for ELIGIBLE_BACKLOG, terminate RUN_STUCK runs.$d$,
        $d$[KOLONLAR]
- data_scope (text): LIVE (backlog) | ARCHIVE (run history).
- perspective (text): ELIGIBLE_BACKLOG | RUN_FAILED | RUN_STUCK | RUN_IN_PROGRESS | RUN_COMPLETED.
- reference_id (text): file_id veya archive_log.id.
- file_name, side (Card|Clearing), network (Visa|Msc|Bkm).
- reference_date (timestamp), age_days (numeric).
- archive_status (text, ArchiveStatus enum, nullable): Pending | Archived | Failed.
- archive_message (text, nullable).
- pipeline_health (text): CRITICAL | DEGRADED | WARMING | HEALTHY.
- urgency (text): P1 (RUN_FAILED/STUCK) | P2 (BACKLOG>=7g) | P3 (BACKLOG) | P4 (RUN_IN_PROGRESS) | P5.
- recommended_action (text): INVESTIGATE_AND_RETRY_ARCHIVE_RUN | CHECK_STUCK_ARCHIVE_PROCESS | TRIGGER_ARCHIVE_FOR_BACKLOG | MONITOR_PROGRESS | NONE.$d$,
        $d$[COLUMNS]
- data_scope (text): LIVE (backlog) | ARCHIVE (run history).
- perspective (text): ELIGIBLE_BACKLOG | RUN_FAILED | RUN_STUCK | RUN_IN_PROGRESS | RUN_COMPLETED.
- reference_id (text): file_id or archive_log.id.
- file_name, side (Card|Clearing), network (Visa|Msc|Bkm).
- reference_date (timestamp), age_days (numeric).
- archive_status (text, ArchiveStatus enum, nullable): Pending | Archived | Failed.
- archive_message (text, nullable).
- pipeline_health (text): CRITICAL | DEGRADED | WARMING | HEALTHY.
- urgency (text): P1 (RUN_FAILED/STUCK) | P2 (BACKLOG>=7d) | P3 (BACKLOG) | P4 (RUN_IN_PROGRESS) | P5.
- recommended_action (text): INVESTIGATE_AND_RETRY_ARCHIVE_RUN | CHECK_STUCK_ARCHIVE_PROCESS | TRIGGER_ARCHIVE_FOR_BACKLOG | MONITOR_PROGRESS | NONE.$d$,
        $d$LIVE perspektifi backlog dur. ARCHIVE perspektifi kosum sagligidir.$d$,
        $d$LIVE perspective is backlog. ARCHIVE perspective is run health.$d$,
        $d$[KAYNAK TABLOLAR] ingestion.file (is_archived, status='Success') ve archive.archive_log + archive.ingestion_file.
[ESIK DEGERLER] RUN_STUCK: archive_log.create_date'den 3600sn. ELIGIBLE_BACKLOG/DEGRADED: 7 gun.
[NOT] RUN_IN_PROGRESS uzun surerse RUN_STUCK e doner; sureyi izleyin.
[ORTAK ENUM TABLOSU]
data_scope: LIVE | ARCHIVE
urgency: P1 (kritik, dakikalar icinde) | P2 (yuksek, saat icinde) | P3 (orta, vardiya icinde) | P4 (dusuk, gunluk) | P5 (bilgi, izleme).
[ORTAK NETWORK / SIDE]
network: BKM | VISA | MSC (Mastercard); Visa MS view'lerinde 'Visa', 'Msc', 'Bkm'.
side / file_type: Card | Clearing.
content_type: Visa | Msc | Bkm.
$d$,
        $d$[SOURCE TABLES] ingestion.file (is_archived, status='Success') and archive.archive_log + archive.ingestion_file.
[THRESHOLDS] RUN_STUCK: 3600s after archive_log.create_date. ELIGIBLE_BACKLOG/DEGRADED: 7 days.
[NOTE] RUN_IN_PROGRESS turns into RUN_STUCK if it lasts too long; watch the duration.
[SHARED ENUM TABLE]
data_scope: LIVE | ARCHIVE
urgency: P1 (critical, minutes) | P2 (high, hours) | P3 (medium, within shift) | P4 (low, daily) | P5 (info, watch).
[SHARED NETWORK / SIDE]
network: BKM | VISA | MSC (Mastercard); 'Visa', 'Msc', 'Bkm' in MS variants.
side / file_type: Card | Clearing.
content_type: Visa | Msc | Bkm.
$d$
    ),
    (
        $d$reporting.rep_daily_transaction_volume$d$,
        $d$FINANCIAL_VOLUME$d$,
        $d$Gercek transaction_date bazinda network/financial_type/txn_effect/currency dagiliminda gunluk hacim, debit/credit ve net akis.$d$,
        $d$Daily volume per network/financial_type/txn_effect/currency using real business transaction_date with debit, credit and net flow.$d$,
        $d$Hangi guneki finansal hacim ne kadardi, anormal bir tepe ya da cukur var mi?$d$,
        $d$What was the daily financial volume, are there abnormal peaks or troughs?$d$,
        $d$volume_flag MATERIAL_NET_FLOW net akisin 1M esigini astigini gosterir, denetim gerekir; NORMAL olagan dagilimdir.$d$,
        $d$MATERIAL_NET_FLOW means net flow exceeds the 1M threshold and warrants review; NORMAL indicates ordinary distribution.$d$,
        $d$Gunluk finansal kapanis ve haftalik trend incelemesi.$d$,
        $d$Daily financial close and weekly trend review.$d$,
        $d$Finans, recon, yonetim.$d$,
        $d$Finance, recon, management.$d$,
        $d$Anormal net akis tespit edilirse o gunun yuksek tutarli islemleri (rep_high_value_unmatched_transactions) ve cesidi (network/currency) ile incele.$d$,
        $d$On abnormal net flow drill into that day high-value transactions (rep_high_value_unmatched_transactions) and breakdown (network/currency).$d$,
        $d$[KOLONLAR]
- data_scope (text): LIVE | ARCHIVE.
- transaction_date (date): YYYYMMDD int4 -> date donusumu (1900-9999 arasi).
- network (text): BKM | VISA | MSC.
- currency (text, ISO 4217 numerik): 949=TRY, 840=USD, 978=EUR vb.
- financial_type (text, FinancialType enum): Authorization | PreAuth | Capture | Refund | Reversal | Adjustment | Fee | Settlement vb (kart sema kisitina gore).
- txn_effect (text, TxnEffect enum): Debit | Credit.
- transaction_count (bigint).
- total_amount, debit_amount, credit_amount, net_flow_amount, avg_amount, max_amount (numeric).
- volume_flag (text): MATERIAL_NET_FLOW | NORMAL | HISTORICAL.
- urgency (text): P3 (MATERIAL) | P5.
- recommended_action (text): INVESTIGATE_LARGE_NET_FLOW | MONITOR_DAILY_VOLUME_TREND | HISTORICAL_TREND_ANALYSIS_ONLY.$d$,
        $d$[COLUMNS]
- data_scope (text): LIVE | ARCHIVE.
- transaction_date (date): YYYYMMDD int4 cast to date (within 1900-9999).
- network (text): BKM | VISA | MSC.
- currency (text, ISO 4217 numeric): 949=TRY, 840=USD, 978=EUR etc.
- financial_type (text, FinancialType enum): Authorization | PreAuth | Capture | Refund | Reversal | Adjustment | Fee | Settlement (per card schema check).
- txn_effect (text, TxnEffect enum): Debit | Credit.
- transaction_count (bigint).
- total_amount, debit_amount, credit_amount, net_flow_amount, avg_amount, max_amount (numeric).
- volume_flag (text): MATERIAL_NET_FLOW | NORMAL | HISTORICAL.
- urgency (text): P3 (MATERIAL) | P5.
- recommended_action (text): INVESTIGATE_LARGE_NET_FLOW | MONITOR_DAILY_VOLUME_TREND | HISTORICAL_TREND_ANALYSIS_ONLY.$d$,
        $d$LIVE: bugunku/dunku gercek tarihler. ARCHIVE: tarihsel hacim profili ve trend.$d$,
        $d$LIVE: today/yesterday actuals. ARCHIVE: historical volume profile and trend.$d$,
        $d$[KAYNAK TABLOLAR] ingestion.card_visa_detail | card_msc_detail | card_bkm_detail + ARCHIVE muadilleri.
[ESIK DEGERLER] MATERIAL_NET_FLOW: |net_flow_amount| >= 1.000.000.
[NOT] transaction_date int4 YYYYMMDD formatindadir; out-of-range degerler atlanir (1900-9999).
[ORTAK ENUM TABLOSU]
data_scope: LIVE | ARCHIVE
urgency: P1 (kritik, dakikalar icinde) | P2 (yuksek, saat icinde) | P3 (orta, vardiya icinde) | P4 (dusuk, gunluk) | P5 (bilgi, izleme).
[ORTAK NETWORK / SIDE]
network: BKM | VISA | MSC (Mastercard); Visa MS view'lerinde 'Visa', 'Msc', 'Bkm'.
side / file_type: Card | Clearing.
content_type: Visa | Msc | Bkm.
$d$,
        $d$[SOURCE TABLES] ingestion.card_visa_detail | card_msc_detail | card_bkm_detail + ARCHIVE counterparts.
[THRESHOLDS] MATERIAL_NET_FLOW: |net_flow_amount| >= 1,000,000.
[NOTE] transaction_date is int4 YYYYMMDD; out-of-range values are skipped.
[SHARED ENUM TABLE]
data_scope: LIVE | ARCHIVE
urgency: P1 (critical, minutes) | P2 (high, hours) | P3 (medium, within shift) | P4 (low, daily) | P5 (info, watch).
[SHARED NETWORK / SIDE]
network: BKM | VISA | MSC (Mastercard); 'Visa', 'Msc', 'Bkm' in MS variants.
side / file_type: Card | Clearing.
content_type: Visa | Msc | Bkm.
$d$
    ),
    (
        $d$reporting.rep_mcc_revenue_concentration$d$,
        $d$FINANCIAL_CONCENTRATION$d$,
        $d$MCC bazinda hacim payi, vergi (tax1+tax2), surcharge ve cashback ekonomisi; konsantrasyon riski etiketi.$d$,
        $d$Per-MCC volume share, tax (tax1+tax2), surcharge and cashback economics with concentration risk flag.$d$,
        $d$Hacmimiz hangi MCC lere bagli, tek bir MCC ye asiri bagimliyiz?$d$,
        $d$Which MCCs drive our volume, are we over-dependent on a single one?$d$,
        $d$concentration_risk HIGH_CONCENTRATION MCC payinin %30 unu astigini gosterir; is surekliligi ve gelir cesitlendirme riskidir.$d$,
        $d$HIGH_CONCENTRATION concentration_risk means an MCC exceeds 30% share; a business-continuity and revenue diversification risk.$d$,
        $d$Aylik portfoy degerlendirmesi.$d$,
        $d$Monthly portfolio review.$d$,
        $d$Risk, finans, ticari ekipler.$d$,
        $d$Risk, finance, commercial teams.$d$,
        $d$HIGH_CONCENTRATION MCC ler portfoy diversifikasyonu calismasina alinir; ticari ekiple yeni MCC kazanimi planlanir.$d$,
        $d$HIGH_CONCENTRATION MCCs are sent to portfolio diversification work; new MCC acquisition is planned with the commercial team.$d$,
        $d$[KOLONLAR]
- data_scope (text): LIVE | ARCHIVE.
- network (text): BKM | VISA | MSC.
- mcc (text): ISO 18245 4 hane (orn 5411=Grocery, 5812=Restaurant, 5999=Misc Retail).
- transaction_count (bigint).
- total_amount, total_tax_amount (tax1+tax2), total_surcharge_amount, total_cashback_amount (numeric).
- volume_share_pct (numeric, 0-100): network icindeki MCC payi.
- concentration_risk (text): HIGH_CONCENTRATION (>=%30) | NOTABLE_CONCENTRATION (>=%15) | DIVERSIFIED | HISTORICAL.
- urgency (text): P2 | P3 | P5.
- recommended_action (text): REVIEW_MCC_CONCENTRATION_RISK | MONITOR_MCC_DEPENDENCY | NONE | HISTORICAL_TREND_ANALYSIS_ONLY.$d$,
        $d$[COLUMNS]
- data_scope (text): LIVE | ARCHIVE.
- network (text): BKM | VISA | MSC.
- mcc (text): ISO 18245 4-digit (e.g. 5411=Grocery, 5812=Restaurant, 5999=Misc Retail).
- transaction_count (bigint).
- total_amount, total_tax_amount (tax1+tax2), total_surcharge_amount, total_cashback_amount (numeric).
- volume_share_pct (numeric, 0-100): MCC share within the network.
- concentration_risk (text): HIGH_CONCENTRATION (>=30%) | NOTABLE_CONCENTRATION (>=15%) | DIVERSIFIED | HISTORICAL.
- urgency (text): P2 | P3 | P5.
- recommended_action (text): REVIEW_MCC_CONCENTRATION_RISK | MONITOR_MCC_DEPENDENCY | NONE | HISTORICAL_TREND_ANALYSIS_ONLY.$d$,
        $d$LIVE: guncel portfoy. ARCHIVE: tarihsel konsantrasyon trendi.$d$,
        $d$LIVE: current portfolio. ARCHIVE: historical concentration trend.$d$,
        $d$[KAYNAK TABLOLAR] ingestion.card_*_detail + ARCHIVE muadilleri.
[NOT] Vergi geliri (tax1+tax2) ve cashback gideri ayni satirda gorulur, net katki hesaplanabilir.
[ORTAK ENUM TABLOSU]
data_scope: LIVE | ARCHIVE
urgency: P1 (kritik, dakikalar icinde) | P2 (yuksek, saat icinde) | P3 (orta, vardiya icinde) | P4 (dusuk, gunluk) | P5 (bilgi, izleme).
[ORTAK NETWORK / SIDE]
network: BKM | VISA | MSC (Mastercard); Visa MS view'lerinde 'Visa', 'Msc', 'Bkm'.
side / file_type: Card | Clearing.
content_type: Visa | Msc | Bkm.
$d$,
        $d$[SOURCE TABLES] ingestion.card_*_detail + ARCHIVE counterparts.
[NOTE] Tax revenue (tax1+tax2) and cashback cost appear side by side; net contribution is computable.
[SHARED ENUM TABLE]
data_scope: LIVE | ARCHIVE
urgency: P1 (critical, minutes) | P2 (high, hours) | P3 (medium, within shift) | P4 (low, daily) | P5 (info, watch).
[SHARED NETWORK / SIDE]
network: BKM | VISA | MSC (Mastercard); 'Visa', 'Msc', 'Bkm' in MS variants.
side / file_type: Card | Clearing.
content_type: Visa | Msc | Bkm.
$d$
    ),
    (
        $d$reporting.rep_merchant_risk_hotspots$d$,
        $d$FINANCIAL_RISK$d$,
        $d$Merchant bazinda decline orani, eslesmemis is orani ve eslesmemis is finansal etki ile risk siniflandirmasi (HIGH_RISK_MERCHANT / HIGH_DECLINE_MERCHANT / NEEDS_INVESTIGATION / HEALTHY).$d$,
        $d$Per-merchant decline rate, unmatched rate and unmatched financial impact with risk classification (HIGH_RISK_MERCHANT / HIGH_DECLINE_MERCHANT / NEEDS_INVESTIGATION / HEALTHY).$d$,
        $d$Hangi merchant lar yuksek risk tasiyor, hangi merchant da decline patlamasi var?$d$,
        $d$Which merchants carry high risk, where is decline spiking?$d$,
        $d$HIGH_RISK_MERCHANT %20+ unmatched ve 100k+ tutar; HIGH_DECLINE_MERCHANT decline ozellikle yogun (%30+).$d$,
        $d$HIGH_RISK_MERCHANT means 20%+ unmatched and 100k+ amount; HIGH_DECLINE_MERCHANT means decline is particularly intense (30%+).$d$,
        $d$Haftalik merchant risk degerlendirmesi.$d$,
        $d$Weekly merchant risk review.$d$,
        $d$Risk, ticari, finans.$d$,
        $d$Risk, commercial, finance.$d$,
        $d$HIGH_RISK_MERCHANT ler risk ekibine eskale; HIGH_DECLINE_MERCHANT lar icin decline pattern (issuer/limit/3DS) incelenir.$d$,
        $d$Escalate HIGH_RISK_MERCHANT to the risk team; for HIGH_DECLINE_MERCHANT investigate decline pattern (issuer/limit/3DS).$d$,
        $d$[KOLONLAR]
- data_scope (text): LIVE | ARCHIVE.
- network (text): BKM | VISA | MSC.
- merchant_id, merchant_name, merchant_country (text|'UNKNOWN').
- transaction_count, declined_count, unmatched_count (bigint).
- total_amount, unmatched_amount (numeric).
- decline_rate_pct, unmatched_rate_pct (numeric, 0-100).
- risk_flag (text): HIGH_RISK_MERCHANT | HIGH_DECLINE_MERCHANT | NEEDS_INVESTIGATION | HEALTHY | HISTORICAL.
- urgency (text): P1 | P2 | P3 | P5.
- recommended_action (text): ESCALATE_TO_RISK_TEAM | INVESTIGATE_DECLINE_PATTERN | INVESTIGATE_UNMATCHED | NONE | HISTORICAL_TREND_ANALYSIS_ONLY.$d$,
        $d$[COLUMNS]
- data_scope (text): LIVE | ARCHIVE.
- network (text): BKM | VISA | MSC.
- merchant_id, merchant_name, merchant_country (text|'UNKNOWN').
- transaction_count, declined_count, unmatched_count (bigint).
- total_amount, unmatched_amount (numeric).
- decline_rate_pct, unmatched_rate_pct (numeric, 0-100).
- risk_flag (text): HIGH_RISK_MERCHANT | HIGH_DECLINE_MERCHANT | NEEDS_INVESTIGATION | HEALTHY | HISTORICAL.
- urgency (text): P1 | P2 | P3 | P5.
- recommended_action (text): ESCALATE_TO_RISK_TEAM | INVESTIGATE_DECLINE_PATTERN | INVESTIGATE_UNMATCHED | NONE | HISTORICAL_TREND_ANALYSIS_ONLY.$d$,
        $d$LIVE: aktif merchant riski. ARCHIVE: gecmis profili.$d$,
        $d$LIVE: active merchant risk. ARCHIVE: historical profile.$d$,
        $d$[KAYNAK TABLOLAR] ingestion.card_*_detail JOIN ingestion.file_line + ARCHIVE muadilleri.
[is_successful_txn ENUM] Successful | Unsuccessful (CHECK constraint).
[ESIK DEGERLER] HIGH_RISK_MERCHANT: unmatched_rate >= %20 ve unmatched_amount >= 100k. HIGH_DECLINE_MERCHANT: decline_rate >= %30.
[NOT] Decline orani response_code <> '00' veya is_successful_txn = 'Unsuccessful' kombinasyonuyla hesaplanir.
[ORTAK ENUM TABLOSU]
data_scope: LIVE | ARCHIVE
urgency: P1 (kritik, dakikalar icinde) | P2 (yuksek, saat icinde) | P3 (orta, vardiya icinde) | P4 (dusuk, gunluk) | P5 (bilgi, izleme).
[ORTAK NETWORK / SIDE]
network: BKM | VISA | MSC (Mastercard); Visa MS view'lerinde 'Visa', 'Msc', 'Bkm'.
side / file_type: Card | Clearing.
content_type: Visa | Msc | Bkm.
$d$,
        $d$[SOURCE TABLES] ingestion.card_*_detail JOIN ingestion.file_line + ARCHIVE counterparts.
[is_successful_txn ENUM] Successful | Unsuccessful (CHECK constraint).
[THRESHOLDS] HIGH_RISK_MERCHANT: unmatched_rate >= 20% and unmatched_amount >= 100k. HIGH_DECLINE_MERCHANT: decline_rate >= 30%.
[NOTE] Decline rate is computed from response_code <> '00' or is_successful_txn = 'Unsuccessful'.
[SHARED ENUM TABLE]
data_scope: LIVE | ARCHIVE
urgency: P1 (critical, minutes) | P2 (high, hours) | P3 (medium, within shift) | P4 (low, daily) | P5 (info, watch).
[SHARED NETWORK / SIDE]
network: BKM | VISA | MSC (Mastercard); 'Visa', 'Msc', 'Bkm' in MS variants.
side / file_type: Card | Clearing.
content_type: Visa | Msc | Bkm.
$d$
    ),
    (
        $d$reporting.rep_country_cross_border_exposure$d$,
        $d$FINANCIAL_RISK$d$,
        $d$Merchant ulkesi ve original/settlement currency esitsizligine gore yurt disi ve FX maruziyetini gosterir.$d$,
        $d$Surfaces cross-border and FX exposure by merchant_country and original-vs-settlement currency.$d$,
        $d$Yurt disi ve farkli para birimi islemlerine ne kadar maruziz?$d$,
        $d$How exposed are we to cross-border and cross-currency transactions?$d$,
        $d$exposure_flag HIGH_FX_EXPOSURE yuksek tutarli (1M+) FX maruziyetidir, hedge degerlendirilmelidir.$d$,
        $d$HIGH_FX_EXPOSURE indicates large (1M+) FX exposure; consider hedging.$d$,
        $d$Aylik FX risk degerlendirmesi.$d$,
        $d$Monthly FX risk review.$d$,
        $d$Hazine, finans, risk.$d$,
        $d$Treasury, finance, risk.$d$,
        $d$HIGH_FX_EXPOSURE icin hedging politikasi gozden gecirilir; ulke bazinda diversifikasyon planlanir.$d$,
        $d$Review hedging policy on HIGH_FX_EXPOSURE; plan country-level diversification.$d$,
        $d$[KOLONLAR]
- data_scope (text): LIVE | ARCHIVE.
- network (text): BKM | VISA | MSC.
- merchant_country (text, ISO 3166 alpha-3 veya kod) veya 'UNKNOWN'.
- fx_pattern (text): CROSS_CURRENCY | SAME_CURRENCY.
- original_currency, settlement_currency (text, ISO 4217 numerik).
- transaction_count (bigint), total_original_amount (numeric).
- exposure_flag (text): HIGH_FX_EXPOSURE | FX_EXPOSURE | DOMESTIC | HISTORICAL.
- urgency (text): P2 | P3 | P5.
- recommended_action (text): HEDGE_OR_REVIEW_FX_EXPOSURE | MONITOR_FX_EXPOSURE | NONE | HISTORICAL_TREND_ANALYSIS_ONLY.$d$,
        $d$[COLUMNS]
- data_scope (text): LIVE | ARCHIVE.
- network (text): BKM | VISA | MSC.
- merchant_country (text, ISO 3166 alpha-3 or code) or 'UNKNOWN'.
- fx_pattern (text): CROSS_CURRENCY | SAME_CURRENCY.
- original_currency, settlement_currency (text, ISO 4217 numeric).
- transaction_count (bigint), total_original_amount (numeric).
- exposure_flag (text): HIGH_FX_EXPOSURE | FX_EXPOSURE | DOMESTIC | HISTORICAL.
- urgency (text): P2 | P3 | P5.
- recommended_action (text): HEDGE_OR_REVIEW_FX_EXPOSURE | MONITOR_FX_EXPOSURE | NONE | HISTORICAL_TREND_ANALYSIS_ONLY.$d$,
        $d$LIVE: guncel exposure. ARCHIVE: tarihsel FX riski.$d$,
        $d$LIVE: current exposure. ARCHIVE: historical FX risk.$d$,
        $d$[KAYNAK TABLOLAR] ingestion.card_*_detail + ARCHIVE muadilleri.
[ESIK] HIGH_FX_EXPOSURE: CROSS_CURRENCY ve total_original_amount >= 1.000.000.
[NOT] Currency kodlari ISO 4217 numeriktir (orn 949=TRY).
[ORTAK ENUM TABLOSU]
data_scope: LIVE | ARCHIVE
urgency: P1 (kritik, dakikalar icinde) | P2 (yuksek, saat icinde) | P3 (orta, vardiya icinde) | P4 (dusuk, gunluk) | P5 (bilgi, izleme).
[ORTAK NETWORK / SIDE]
network: BKM | VISA | MSC (Mastercard); Visa MS view'lerinde 'Visa', 'Msc', 'Bkm'.
side / file_type: Card | Clearing.
content_type: Visa | Msc | Bkm.
$d$,
        $d$[SOURCE TABLES] ingestion.card_*_detail + ARCHIVE counterparts.
[THRESHOLDS] HIGH_FX_EXPOSURE: CROSS_CURRENCY and total_original_amount >= 1,000,000.
[NOTE] Currency codes are ISO 4217 numeric (e.g. 949=TRY).
[SHARED ENUM TABLE]
data_scope: LIVE | ARCHIVE
urgency: P1 (critical, minutes) | P2 (high, hours) | P3 (medium, within shift) | P4 (low, daily) | P5 (info, watch).
[SHARED NETWORK / SIDE]
network: BKM | VISA | MSC (Mastercard); 'Visa', 'Msc', 'Bkm' in MS variants.
side / file_type: Card | Clearing.
content_type: Visa | Msc | Bkm.
$d$
    ),
    (
        $d$reporting.rep_response_code_decline_health$d$,
        $d$AUTHORIZATION_HEALTH$d$,
        $d$Network bazinda response_code dagilimi, basarisizlik orani ve baskin red sebepleri.$d$,
        $d$Per-network response_code distribution with failure rate and dominant decline reasons.$d$,
        $d$Iadeler ve redler hangi response_code dan geliyor?$d$,
        $d$Which response_codes are driving declines and refusals?$d$,
        $d$DOMINANT_FAILURE_REASON %5 ustu paya sahip bir red sebebidir, kaynak arastirilmalidir; SUCCESS_OR_UNKNOWN normaldir.$d$,
        $d$DOMINANT_FAILURE_REASON means a decline reason exceeds 5% share; investigate the root cause; SUCCESS_OR_UNKNOWN is normal.$d$,
        $d$Gunluk operasyonel takip.$d$,
        $d$Daily operational monitoring.$d$,
        $d$Operasyon, urun, risk.$d$,
        $d$Operations, product, risk.$d$,
        $d$DOMINANT_FAILURE_REASON satirlari icin kaynak (issuer/network/limit) arastirilir; gerekirse 3DS/limit ayarlari guncellenir.$d$,
        $d$Investigate the source (issuer/network/limit) for DOMINANT_FAILURE_REASON rows; update 3DS/limit settings if needed.$d$,
        $d$[KOLONLAR]
- data_scope (text): LIVE | ARCHIVE.
- network (text): BKM | VISA | MSC.
- response_code (text, ISO 8583): 00=Approved, 01=Refer to issuer, 05=Do not honor, 12=Invalid txn, 14=Invalid card, 41=Lost, 43=Stolen, 51=Insufficient funds, 54=Expired card, 57/58=Not permitted, 61=Exceeds limit, 65=Activity limit, 91=Issuer unavailable, 96=System error, NONE=eksik veri.
- transaction_count (bigint), total_amount (numeric).
- successful_count, failed_count (bigint).
- failure_rate_pct, network_share_pct (numeric, 0-100).
- health_flag (text): DOMINANT_FAILURE_REASON (>=%5 pay) | NORMAL_FAILURE | SUCCESS_OR_UNKNOWN | HISTORICAL.
- urgency (text): P2 | P5.
- recommended_action (text): INVESTIGATE_DECLINE_REASON | NONE | HISTORICAL_TREND_ANALYSIS_ONLY.$d$,
        $d$[COLUMNS]
- data_scope (text): LIVE | ARCHIVE.
- network (text): BKM | VISA | MSC.
- response_code (text, ISO 8583): 00=Approved, 01=Refer to issuer, 05=Do not honor, 12=Invalid txn, 14=Invalid card, 41=Lost, 43=Stolen, 51=Insufficient funds, 54=Expired card, 57/58=Not permitted, 61=Exceeds limit, 65=Activity limit, 91=Issuer unavailable, 96=System error, NONE=missing data.
- transaction_count (bigint), total_amount (numeric).
- successful_count, failed_count (bigint).
- failure_rate_pct, network_share_pct (numeric, 0-100).
- health_flag (text): DOMINANT_FAILURE_REASON (>=5% share) | NORMAL_FAILURE | SUCCESS_OR_UNKNOWN | HISTORICAL.
- urgency (text): P2 | P5.
- recommended_action (text): INVESTIGATE_DECLINE_REASON | NONE | HISTORICAL_TREND_ANALYSIS_ONLY.$d$,
        $d$LIVE: anlik authorization sagligi. ARCHIVE: tarihsel red profili.$d$,
        $d$LIVE: real-time authorization health. ARCHIVE: historical decline profile.$d$,
        $d$[KAYNAK TABLOLAR] ingestion.card_*_detail + ARCHIVE muadilleri.
[is_successful_txn ENUM] Successful | Unsuccessful.
[NOT] response_code='00' basari kabul edilir; NONE veri eksikligini gosterir.
[ORTAK ENUM TABLOSU]
data_scope: LIVE | ARCHIVE
urgency: P1 (kritik, dakikalar icinde) | P2 (yuksek, saat icinde) | P3 (orta, vardiya icinde) | P4 (dusuk, gunluk) | P5 (bilgi, izleme).
[ORTAK NETWORK / SIDE]
network: BKM | VISA | MSC (Mastercard); Visa MS view'lerinde 'Visa', 'Msc', 'Bkm'.
side / file_type: Card | Clearing.
content_type: Visa | Msc | Bkm.
$d$,
        $d$[SOURCE TABLES] ingestion.card_*_detail + ARCHIVE counterparts.
[is_successful_txn ENUM] Successful | Unsuccessful.
[NOTE] response_code='00' is success; 'NONE' indicates missing data.
[SHARED ENUM TABLE]
data_scope: LIVE | ARCHIVE
urgency: P1 (critical, minutes) | P2 (high, hours) | P3 (medium, within shift) | P4 (low, daily) | P5 (info, watch).
[SHARED NETWORK / SIDE]
network: BKM | VISA | MSC (Mastercard); 'Visa', 'Msc', 'Bkm' in MS variants.
side / file_type: Card | Clearing.
content_type: Visa | Msc | Bkm.
$d$
    ),
    (
        $d$reporting.rep_settlement_lag_analysis$d$,
        $d$OPERATIONS_KPI$d$,
        $d$Islem tarihi ile end_of_day_date, value_date ve dosya alim tarihi arasindaki gecikmeleri olcer.$d$,
        $d$Measures lag between transaction_date and end_of_day_date, value_date, and file ingestion date.$d$,
        $d$Islemler ne kadar zamaninda sisteme dusuyor, hangi noktada gecikme olusuyor?$d$,
        $d$How timely are transactions arriving in the system, where does the delay accumulate?$d$,
        $d$lag_health CHRONIC_INGEST_DELAY yapisal bir alim gecikmesidir; SPORADIC_LATE_INGEST tek tek geciken kayitlar; TIMELY SLA icindedir.$d$,
        $d$CHRONIC_INGEST_DELAY means structurally lagging ingestion; SPORADIC_LATE_INGEST means isolated late records; TIMELY is within SLA.$d$,
        $d$Gunluk SLA takibi.$d$,
        $d$Daily SLA monitoring.$d$,
        $d$Operasyon, SRE, recon.$d$,
        $d$Operations, SRE, recon.$d$,
        $d$CHRONIC_INGEST_DELAY de upstream provider/kanal ile temas kurulur; transport ya da batch zamanlamasi degistirilir.$d$,
        $d$On CHRONIC_INGEST_DELAY contact upstream provider/channel; change transport or batch schedule.$d$,
        $d$[KOLONLAR]
- data_scope (text): LIVE | ARCHIVE.
- network (text): BKM | VISA | MSC.
- transaction_date (date).
- transaction_count (bigint), total_amount (numeric).
- avg_lag_to_eod_days, avg_lag_to_value_days, avg_lag_to_ingest_days, max_lag_to_ingest_days (numeric, gun).
- late_ingest_count (bigint): ingest_dt - txn_dt >= 3 gun olan kayit sayisi.
- lag_health (text): CHRONIC_INGEST_DELAY (avg >= 3 gun) | SPORADIC_LATE_INGEST (max >= 5 gun) | TIMELY | HISTORICAL.
- urgency (text): P2 | P3 | P5.
- recommended_action (text): INVESTIGATE_INGESTION_PIPELINE_DELAY | CHECK_OUTLIER_LATE_TRANSACTIONS | NONE | HISTORICAL_TREND_ANALYSIS_ONLY.$d$,
        $d$[COLUMNS]
- data_scope (text): LIVE | ARCHIVE.
- network (text): BKM | VISA | MSC.
- transaction_date (date).
- transaction_count (bigint), total_amount (numeric).
- avg_lag_to_eod_days, avg_lag_to_value_days, avg_lag_to_ingest_days, max_lag_to_ingest_days (numeric, days).
- late_ingest_count (bigint): records where ingest_dt - txn_dt >= 3 days.
- lag_health (text): CHRONIC_INGEST_DELAY (avg >= 3d) | SPORADIC_LATE_INGEST (max >= 5d) | TIMELY | HISTORICAL.
- urgency (text): P2 | P3 | P5.
- recommended_action (text): INVESTIGATE_INGESTION_PIPELINE_DELAY | CHECK_OUTLIER_LATE_TRANSACTIONS | NONE | HISTORICAL_TREND_ANALYSIS_ONLY.$d$,
        $d$LIVE: guncel SLA durumu. ARCHIVE: tarihsel SLA profili.$d$,
        $d$LIVE: current SLA status. ARCHIVE: historical SLA profile.$d$,
        $d$[KAYNAK TABLOLAR] ingestion.card_*_detail + ARCHIVE muadilleri.
[NOT] transaction_date, end_of_day_date, value_date YYYYMMDD int4 olarak tutulur (1900-9999 disi atilir).
[ORTAK ENUM TABLOSU]
data_scope: LIVE | ARCHIVE
urgency: P1 (kritik, dakikalar icinde) | P2 (yuksek, saat icinde) | P3 (orta, vardiya icinde) | P4 (dusuk, gunluk) | P5 (bilgi, izleme).
[ORTAK NETWORK / SIDE]
network: BKM | VISA | MSC (Mastercard); Visa MS view'lerinde 'Visa', 'Msc', 'Bkm'.
side / file_type: Card | Clearing.
content_type: Visa | Msc | Bkm.
$d$,
        $d$[SOURCE TABLES] ingestion.card_*_detail + ARCHIVE counterparts.
[NOTE] transaction_date, end_of_day_date, value_date are stored as YYYYMMDD int4 (out-of-range skipped).
[SHARED ENUM TABLE]
data_scope: LIVE | ARCHIVE
urgency: P1 (critical, minutes) | P2 (high, hours) | P3 (medium, within shift) | P4 (low, daily) | P5 (info, watch).
[SHARED NETWORK / SIDE]
network: BKM | VISA | MSC (Mastercard); 'Visa', 'Msc', 'Bkm' in MS variants.
side / file_type: Card | Clearing.
content_type: Visa | Msc | Bkm.
$d$
    ),
    (
        $d$reporting.rep_currency_fx_drift$d$,
        $d$FINANCIAL_RISK$d$,
        $d$Cross-currency islemlerde original ve settlement (ve billing) tutar farklarini toplayarak FX drift i (kazanc/kayip) gosterir.$d$,
        $d$For cross-currency transactions aggregates the original-vs-settlement (and billing) amount drift to surface FX gain/loss.$d$,
        $d$FX donusumlerinde sistematik bir kayip ya da kazanc var mi?$d$,
        $d$Is there systematic FX gain/loss in our currency conversions?$d$,
        $d$MATERIAL_DRIFT 100k+ kumulatif drift birikmistir; settlement mantigi ve kur kaynagi denetlenmelidir.$d$,
        $d$MATERIAL_DRIFT means cumulative drift exceeds 100k; review settlement logic and FX rate source.$d$,
        $d$Aylik FX denetimi.$d$,
        $d$Monthly FX audit.$d$,
        $d$Hazine, finans, denetim.$d$,
        $d$Treasury, finance, audit.$d$,
        $d$MATERIAL_DRIFT te kur kaynagi (Reuters/issuer rate) ve settlement formulu denetlenir; gerekirse rezerv ayrilir.$d$,
        $d$On MATERIAL_DRIFT audit the FX rate source (Reuters/issuer rate) and settlement formula; reserve if needed.$d$,
        $d$[KOLONLAR]
- data_scope (text): LIVE | ARCHIVE.
- network (text): BKM | VISA | MSC.
- original_currency, settlement_currency, billing_currency (text, ISO 4217 numerik).
- transaction_count (bigint).
- total_original_amount, total_settlement_amount, total_billing_amount (numeric).
- settlement_drift = SUM(settlement_amount - original_amount) (numeric, isaretli).
- billing_drift = SUM(billing_amount - original_amount) (numeric, isaretli).
- fx_drift_severity (text): MATERIAL_DRIFT (|drift|>=100k) | NOTABLE_DRIFT (|drift|>=10k) | INSIGNIFICANT | HISTORICAL.
- urgency (text): P2 | P3 | P5.
- recommended_action (text): REVIEW_FX_RATES_AND_SETTLEMENT_LOGIC | MONITOR_FX_DRIFT | NONE | HISTORICAL_TREND_ANALYSIS_ONLY.$d$,
        $d$[COLUMNS]
- data_scope (text): LIVE | ARCHIVE.
- network (text): BKM | VISA | MSC.
- original_currency, settlement_currency, billing_currency (text, ISO 4217 numeric).
- transaction_count (bigint).
- total_original_amount, total_settlement_amount, total_billing_amount (numeric).
- settlement_drift = SUM(settlement_amount - original_amount) (numeric, signed).
- billing_drift = SUM(billing_amount - original_amount) (numeric, signed).
- fx_drift_severity (text): MATERIAL_DRIFT (|drift|>=100k) | NOTABLE_DRIFT (|drift|>=10k) | INSIGNIFICANT | HISTORICAL.
- urgency (text): P2 | P3 | P5.
- recommended_action (text): REVIEW_FX_RATES_AND_SETTLEMENT_LOGIC | MONITOR_FX_DRIFT | NONE | HISTORICAL_TREND_ANALYSIS_ONLY.$d$,
        $d$LIVE: guncel FX drift. ARCHIVE: tarihsel FX drift profili.$d$,
        $d$LIVE: current FX drift. ARCHIVE: historical FX drift profile.$d$,
        $d$[KAYNAK TABLOLAR] ingestion.card_*_detail + ARCHIVE muadilleri.
[NOT] Sadece original_currency <> settlement_currency olan satirlar ele alinir.
[ORTAK ENUM TABLOSU]
data_scope: LIVE | ARCHIVE
urgency: P1 (kritik, dakikalar icinde) | P2 (yuksek, saat icinde) | P3 (orta, vardiya icinde) | P4 (dusuk, gunluk) | P5 (bilgi, izleme).
[ORTAK NETWORK / SIDE]
network: BKM | VISA | MSC (Mastercard); Visa MS view'lerinde 'Visa', 'Msc', 'Bkm'.
side / file_type: Card | Clearing.
content_type: Visa | Msc | Bkm.
$d$,
        $d$[SOURCE TABLES] ingestion.card_*_detail + ARCHIVE counterparts.
[NOTE] Only original_currency <> settlement_currency rows are considered.
[SHARED ENUM TABLE]
data_scope: LIVE | ARCHIVE
urgency: P1 (critical, minutes) | P2 (high, hours) | P3 (medium, within shift) | P4 (low, daily) | P5 (info, watch).
[SHARED NETWORK / SIDE]
network: BKM | VISA | MSC (Mastercard); 'Visa', 'Msc', 'Bkm' in MS variants.
side / file_type: Card | Clearing.
content_type: Visa | Msc | Bkm.
$d$
    ),
    (
        $d$reporting.rep_installment_portfolio_summary$d$,
        $d$PORTFOLIO_RISK$d$,
        $d$Network bazinda taksit kovasi (1_SINGLE, 2-3, 4-6, 7-12, 13+) ile portfoy dagilimi ve uzun vadeli maruziyet bayragi.$d$,
        $d$Per-network installment buckets (1_SINGLE, 2-3, 4-6, 7-12, 13+) with portfolio share and long-term exposure flag.$d$,
        $d$Taksitli portfoyumuz ne kadar uzun vadeye yayilmis?$d$,
        $d$How much of our portfolio is in long-term installments?$d$,
        $d$HIGH_LONG_TERM_INSTALLMENT_EXPOSURE 13+ taksit %10 unu astigini gosterir, kredi riski artmistir.$d$,
        $d$HIGH_LONG_TERM_INSTALLMENT_EXPOSURE means 13+ installment exceeds 10%, increasing credit risk.$d$,
        $d$Aylik portfoy degerlendirmesi.$d$,
        $d$Monthly portfolio review.$d$,
        $d$Risk, kredi, finans.$d$,
        $d$Risk, credit, finance.$d$,
        $d$HIGH_LONG_TERM_INSTALLMENT_EXPOSURE durumunda taksit politikasi/skorlama incelenmelidir.$d$,
        $d$Review installment policy/scoring on HIGH_LONG_TERM_INSTALLMENT_EXPOSURE.$d$,
        $d$[KOLONLAR]
- data_scope (text): LIVE | ARCHIVE.
- network (text): BKM | VISA | MSC.
- installment_bucket (text): 1_SINGLE | 2-3 | 4-6 | 7-12 | 13+.
- transaction_count (bigint), total_amount (numeric), avg_amount (numeric).
- volume_share_pct (count payi), amount_share_pct (tutar payi) (numeric, 0-100).
- portfolio_flag (text): HIGH_LONG_TERM_INSTALLMENT_EXPOSURE (13+ >=%10) | NOTABLE_LONG_TERM_EXPOSURE (7-12+13+ >=%20) | NORMAL | HISTORICAL.
- urgency (text): P3 | P5.
- recommended_action (text): REVIEW_LONG_TERM_INSTALLMENT_RISK | NONE | HISTORICAL_TREND_ANALYSIS_ONLY.$d$,
        $d$[COLUMNS]
- data_scope (text): LIVE | ARCHIVE.
- network (text): BKM | VISA | MSC.
- installment_bucket (text): 1_SINGLE | 2-3 | 4-6 | 7-12 | 13+.
- transaction_count (bigint), total_amount (numeric), avg_amount (numeric).
- volume_share_pct (count share), amount_share_pct (amount share) (numeric, 0-100).
- portfolio_flag (text): HIGH_LONG_TERM_INSTALLMENT_EXPOSURE (13+ >=10%) | NOTABLE_LONG_TERM_EXPOSURE (7-12+13+ >=20%) | NORMAL | HISTORICAL.
- urgency (text): P3 | P5.
- recommended_action (text): REVIEW_LONG_TERM_INSTALLMENT_RISK | NONE | HISTORICAL_TREND_ANALYSIS_ONLY.$d$,
        $d$LIVE: guncel portfoy. ARCHIVE: tarihsel taksit egilimi.$d$,
        $d$LIVE: current portfolio. ARCHIVE: historical installment trend.$d$,
        $d$[KAYNAK TABLOLAR] ingestion.card_*_detail + ARCHIVE muadilleri.
[NOT] install_count detail tablosundaki gercek taksit sayisidir; install_order taksit sirasidir.
[ORTAK ENUM TABLOSU]
data_scope: LIVE | ARCHIVE
urgency: P1 (kritik, dakikalar icinde) | P2 (yuksek, saat icinde) | P3 (orta, vardiya icinde) | P4 (dusuk, gunluk) | P5 (bilgi, izleme).
[ORTAK NETWORK / SIDE]
network: BKM | VISA | MSC (Mastercard); Visa MS view'lerinde 'Visa', 'Msc', 'Bkm'.
side / file_type: Card | Clearing.
content_type: Visa | Msc | Bkm.
$d$,
        $d$[SOURCE TABLES] ingestion.card_*_detail + ARCHIVE counterparts.
[NOTE] install_count is the real installment count from detail; install_order is the installment index.
[SHARED ENUM TABLE]
data_scope: LIVE | ARCHIVE
urgency: P1 (critical, minutes) | P2 (high, hours) | P3 (medium, within shift) | P4 (low, daily) | P5 (info, watch).
[SHARED NETWORK / SIDE]
network: BKM | VISA | MSC (Mastercard); 'Visa', 'Msc', 'Bkm' in MS variants.
side / file_type: Card | Clearing.
content_type: Visa | Msc | Bkm.
$d$
    ),
    (
        $d$reporting.rep_loyalty_points_economy$d$,
        $d$PROGRAM_ECONOMICS$d$,
        $d$Gunluk bazda BC/MC/CC puan tutarlarinin orijinal islem tutarina oranini gosterir; sadakat program maliyetini olcer.$d$,
        $d$Daily BC/MC/CC point amounts vs original transaction amount; measures loyalty program cost.$d$,
        $d$Sadakat programi gunluk olarak ne kadara mal oluyor?$d$,
        $d$How much does the loyalty program cost daily?$d$,
        $d$HIGH_LOYALTY_USAGE %10+ subvansiyon var; program maliyeti gozden gecirilmelidir. NOTABLE_LOYALTY_USAGE %5+ izleme.$d$,
        $d$HIGH_LOYALTY_USAGE means subsidy exceeds 10%; review program cost. NOTABLE_LOYALTY_USAGE >= 5% needs monitoring.$d$,
        $d$Aylik program degerlendirmesi.$d$,
        $d$Monthly program review.$d$,
        $d$Sadakat, urun, finans.$d$,
        $d$Loyalty, product, finance.$d$,
        $d$HIGH_LOYALTY_USAGE gunlerinde puan kazanim/harcama kurallari ve kampanyalar gozden gecirilir.$d$,
        $d$On HIGH_LOYALTY_USAGE days review point earning/burning rules and campaigns.$d$,
        $d$[KOLONLAR]
- data_scope (text): LIVE | ARCHIVE.
- network (text): BKM | VISA | MSC.
- transaction_date (date).
- transaction_count (bigint).
- total_original_amount, total_bc_point_amount, total_mc_point_amount, total_cc_point_amount, total_loyalty_amount (numeric, TL).
- loyalty_to_amount_ratio_pct (numeric, 0-100).
- loyalty_intensity (text): HIGH_LOYALTY_USAGE (>=%10) | NOTABLE_LOYALTY_USAGE (>=%5) | NORMAL | HISTORICAL.
- urgency (text): P3 | P5.
- recommended_action (text): REVIEW_LOYALTY_PROGRAM_COST | MONITOR | HISTORICAL_TREND_ANALYSIS_ONLY.$d$,
        $d$[COLUMNS]
- data_scope (text): LIVE | ARCHIVE.
- network (text): BKM | VISA | MSC.
- transaction_date (date).
- transaction_count (bigint).
- total_original_amount, total_bc_point_amount, total_mc_point_amount, total_cc_point_amount, total_loyalty_amount (numeric, TL).
- loyalty_to_amount_ratio_pct (numeric, 0-100).
- loyalty_intensity (text): HIGH_LOYALTY_USAGE (>=10%) | NOTABLE_LOYALTY_USAGE (>=5%) | NORMAL | HISTORICAL.
- urgency (text): P3 | P5.
- recommended_action (text): REVIEW_LOYALTY_PROGRAM_COST | MONITOR | HISTORICAL_TREND_ANALYSIS_ONLY.$d$,
        $d$LIVE: gunluk program maliyeti. ARCHIVE: tarihsel maliyet trendi.$d$,
        $d$LIVE: daily program cost. ARCHIVE: historical cost trend.$d$,
        $d$[KAYNAK TABLOLAR] ingestion.card_*_detail + ARCHIVE muadilleri.
[NOT] BC=Bonus Card, MC=Maximum Card, CC=Credit Card; bc/mc/cc point_amount alanlari TL bazinda tutar olarak saklanir.
[ORTAK ENUM TABLOSU]
data_scope: LIVE | ARCHIVE
urgency: P1 (kritik, dakikalar icinde) | P2 (yuksek, saat icinde) | P3 (orta, vardiya icinde) | P4 (dusuk, gunluk) | P5 (bilgi, izleme).
[ORTAK NETWORK / SIDE]
network: BKM | VISA | MSC (Mastercard); Visa MS view'lerinde 'Visa', 'Msc', 'Bkm'.
side / file_type: Card | Clearing.
content_type: Visa | Msc | Bkm.
$d$,
        $d$[SOURCE TABLES] ingestion.card_*_detail + ARCHIVE counterparts.
[NOTE] BC=Bonus Card, MC=Maximum Card, CC=Credit Card; bc/mc/cc point_amount fields are stored as TL amounts.
[SHARED ENUM TABLE]
data_scope: LIVE | ARCHIVE
urgency: P1 (critical, minutes) | P2 (high, hours) | P3 (medium, within shift) | P4 (low, daily) | P5 (info, watch).
[SHARED NETWORK / SIDE]
network: BKM | VISA | MSC (Mastercard); 'Visa', 'Msc', 'Bkm' in MS variants.
side / file_type: Card | Clearing.
content_type: Visa | Msc | Bkm.
$d$
    ),
    (
        $d$reporting.rep_clearing_dispute_summary$d$,
        $d$DISPUTES$d$,
        $d$Clearing tarafindaki dispute_code/reason_code/control_stat bazinda islem ve reimbursement tutarlari.$d$,
        $d$Clearing dispute aggregations by dispute_code/reason_code/control_stat with reimbursement amount.$d$,
        $d$Itiraz/chargeback yuku ne kadar, hangi reason_code lar baskin?$d$,
        $d$What is our chargeback load, which reason codes dominate?$d$,
        $d$HIGH_DISPUTE_EXPOSURE reimbursement tutarinin 100k yi astigini gosterir, eskale edilmelidir.$d$,
        $d$HIGH_DISPUTE_EXPOSURE means reimbursement exceeds 100k; escalate.$d$,
        $d$Haftalik dispute degerlendirmesi.$d$,
        $d$Weekly dispute review.$d$,
        $d$Dispute ekibi, finans, denetim.$d$,
        $d$Dispute team, finance, audit.$d$,
        $d$HIGH_DISPUTE_EXPOSURE icin musteri/issuer ile temas plani olusturulur; reason_code bazinda root cause incelenir.$d$,
        $d$For HIGH_DISPUTE_EXPOSURE create a contact plan with customer/issuer; investigate root cause per reason_code.$d$,
        $d$[KOLONLAR]
- data_scope (text): LIVE | ARCHIVE.
- network (text): BKM | VISA | MSC.
- dispute_code (text): network e ozgu kod (orn Visa: 11.1, 11.2, 11.3, 13.1, 13.2; Mastercard: 4837, 4853, 4855, 4870; BKM: yerel kodlar) veya 'NONE'.
- reason_code (text, network e ozgu): chargeback gerekce kodu veya 'NONE'.
- control_stat (text): kayit kontrol durumu (network bazli; orn 'O' open, 'C' closed, 'P' presented vb.).
- transaction_count (bigint).
- total_source_amount, total_reimbursement_amount (numeric).
- first_txn_date, last_txn_date (date).
- dispute_flag (text): HIGH_DISPUTE_EXPOSURE (reimbursement>=100k) | ACTIVE_DISPUTE | CLEAN | HISTORICAL.
- urgency (text): P1 | P3 | P5.
- recommended_action (text): ESCALATE_DISPUTE_RESOLUTION | WORK_DISPUTE_QUEUE | NONE | HISTORICAL_TREND_ANALYSIS_ONLY.$d$,
        $d$[COLUMNS]
- data_scope (text): LIVE | ARCHIVE.
- network (text): BKM | VISA | MSC.
- dispute_code (text): network-specific code (e.g. Visa: 11.1, 11.2, 11.3, 13.1, 13.2; Mastercard: 4837, 4853, 4855, 4870; BKM: local codes) or 'NONE'.
- reason_code (text, network-specific): chargeback reason code or 'NONE'.
- control_stat (text): record control status (network-specific; e.g. 'O' open, 'C' closed, 'P' presented).
- transaction_count (bigint).
- total_source_amount, total_reimbursement_amount (numeric).
- first_txn_date, last_txn_date (date).
- dispute_flag (text): HIGH_DISPUTE_EXPOSURE (reimbursement>=100k) | ACTIVE_DISPUTE | CLEAN | HISTORICAL.
- urgency (text): P1 | P3 | P5.
- recommended_action (text): ESCALATE_DISPUTE_RESOLUTION | WORK_DISPUTE_QUEUE | NONE | HISTORICAL_TREND_ANALYSIS_ONLY.$d$,
        $d$LIVE: aktif disputes. ARCHIVE: tarihsel dispute profili.$d$,
        $d$LIVE: active disputes. ARCHIVE: historical dispute profile.$d$,
        $d$[KAYNAK TABLOLAR] ingestion.clearing_visa_detail | clearing_msc_detail | clearing_bkm_detail + ARCHIVE muadilleri.
[NOT] Sadece clearing detay tablolari kullanilir; card detail tarafinda dispute alani yoktur.
[ORTAK ENUM TABLOSU]
data_scope: LIVE | ARCHIVE
urgency: P1 (kritik, dakikalar icinde) | P2 (yuksek, saat icinde) | P3 (orta, vardiya icinde) | P4 (dusuk, gunluk) | P5 (bilgi, izleme).
[ORTAK NETWORK / SIDE]
network: BKM | VISA | MSC (Mastercard); Visa MS view'lerinde 'Visa', 'Msc', 'Bkm'.
side / file_type: Card | Clearing.
content_type: Visa | Msc | Bkm.
$d$,
        $d$[SOURCE TABLES] ingestion.clearing_visa_detail | clearing_msc_detail | clearing_bkm_detail + ARCHIVE counterparts.
[NOTE] Only clearing detail tables are used; card detail does not have a dispute field.
[SHARED ENUM TABLE]
data_scope: LIVE | ARCHIVE
urgency: P1 (critical, minutes) | P2 (high, hours) | P3 (medium, within shift) | P4 (low, daily) | P5 (info, watch).
[SHARED NETWORK / SIDE]
network: BKM | VISA | MSC (Mastercard); 'Visa', 'Msc', 'Bkm' in MS variants.
side / file_type: Card | Clearing.
content_type: Visa | Msc | Bkm.
$d$
    ),
    (
        $d$reporting.rep_clearing_io_imbalance$d$,
        $d$CLEARING_FLOW$d$,
        $d$Gunluk clearing incoming/outgoing tutarlari ve net akis dengesizligi.$d$,
        $d$Daily clearing incoming/outgoing amounts and net flow imbalance.$d$,
        $d$Clearing incoming/outgoing dengemiz nasil, net akis ne yonde?$d$,
        $d$How is our incoming/outgoing clearing balance, in which direction is the net flow?$d$,
        $d$MATERIAL_NET_IMBALANCE 1M yi asan net akis demektir; recon veya reconciliation gecikmesi gostergesidir. NOTABLE_NET_IMBALANCE 100k+ izleme gerektirir.$d$,
        $d$MATERIAL_NET_IMBALANCE means net flow exceeds 1M; indicates recon delay/imbalance. NOTABLE_NET_IMBALANCE >= 100k needs monitoring.$d$,
        $d$Gunluk T+1 takibi.$d$,
        $d$Daily T+1 monitoring.$d$,
        $d$Recon, finans.$d$,
        $d$Recon, finance.$d$,
        $d$MATERIAL_NET_IMBALANCE icin gunluk reconciliation kapanis akisi denetlenir.$d$,
        $d$On MATERIAL_NET_IMBALANCE check the daily reconciliation close flow.$d$,
        $d$[KOLONLAR]
- data_scope (text): LIVE | ARCHIVE.
- network (text): BKM | VISA | MSC.
- txn_date (date): YYYYMMDD'den donusturulmus is gunu.
- transaction_count (bigint).
- incoming_count, outgoing_count (bigint).
- incoming_amount, outgoing_amount (numeric).
- net_flow_amount (numeric, isaretli; +incoming -outgoing).
- imbalance_flag (text): MATERIAL_NET_IMBALANCE (>=1M) | NOTABLE_NET_IMBALANCE (>=100k) | BALANCED | HISTORICAL.
- urgency (text): P2 | P3 | P5.
- recommended_action (text): INVESTIGATE_NET_FLOW_IMBALANCE | MONITOR_NET_FLOW | NONE | HISTORICAL_TREND_ANALYSIS_ONLY.$d$,
        $d$[COLUMNS]
- data_scope (text): LIVE | ARCHIVE.
- network (text): BKM | VISA | MSC.
- txn_date (date): business day cast from YYYYMMDD.
- transaction_count (bigint).
- incoming_count, outgoing_count (bigint).
- incoming_amount, outgoing_amount (numeric).
- net_flow_amount (numeric, signed; +incoming -outgoing).
- imbalance_flag (text): MATERIAL_NET_IMBALANCE (>=1M) | NOTABLE_NET_IMBALANCE (>=100k) | BALANCED | HISTORICAL.
- urgency (text): P2 | P3 | P5.
- recommended_action (text): INVESTIGATE_NET_FLOW_IMBALANCE | MONITOR_NET_FLOW | NONE | HISTORICAL_TREND_ANALYSIS_ONLY.$d$,
        $d$LIVE: guncel akis dengesi. ARCHIVE: tarihsel akis profili.$d$,
        $d$LIVE: current flow balance. ARCHIVE: historical flow profile.$d$,
        $d$[KAYNAK TABLOLAR] ingestion.clearing_*_detail + ARCHIVE muadilleri.
[io_flag ENUM] Incoming | Outgoing (CHECK constraint).
[ORTAK ENUM TABLOSU]
data_scope: LIVE | ARCHIVE
urgency: P1 (kritik, dakikalar icinde) | P2 (yuksek, saat icinde) | P3 (orta, vardiya icinde) | P4 (dusuk, gunluk) | P5 (bilgi, izleme).
[ORTAK NETWORK / SIDE]
network: BKM | VISA | MSC (Mastercard); Visa MS view'lerinde 'Visa', 'Msc', 'Bkm'.
side / file_type: Card | Clearing.
content_type: Visa | Msc | Bkm.
$d$,
        $d$[SOURCE TABLES] ingestion.clearing_*_detail + ARCHIVE counterparts.
[io_flag ENUM] Incoming | Outgoing (CHECK constraint).
[SHARED ENUM TABLE]
data_scope: LIVE | ARCHIVE
urgency: P1 (critical, minutes) | P2 (high, hours) | P3 (medium, within shift) | P4 (low, daily) | P5 (info, watch).
[SHARED NETWORK / SIDE]
network: BKM | VISA | MSC (Mastercard); 'Visa', 'Msc', 'Bkm' in MS variants.
side / file_type: Card | Clearing.
content_type: Visa | Msc | Bkm.
$d$
    ),
    (
        $d$reporting.rep_high_value_unmatched_transactions$d$,
        $d$FINANCIAL_RISK$d$,
        $d$Tek tek 100k+ tutarli eslesmemis islemleri merchant adi, ulke ve PAN-mask edilmis kart bilgisi ile listeler; tek-noktali yuksek riski hedefler.$d$,
        $d$Lists individual unmatched transactions of 100k+ with merchant name, country and PAN-masked card; targets single-item high risk.$d$,
        $d$Yuksek tutarli hangi tek tek islemler hala beklemede?$d$,
        $d$Which individual high-value transactions are still pending match?$d$,
        $d$risk_flag CRITICAL_HIGH_VALUE_UNMATCHED 1M+ islemdir, derhal incelenmelidir; HIGH_VALUE_UNMATCHED 500k+ ikinci sira; NOTABLE_VALUE_UNMATCHED 100k-500k arasi.$d$,
        $d$CRITICAL_HIGH_VALUE_UNMATCHED is a 1M+ transaction; inspect immediately. HIGH_VALUE_UNMATCHED is the 500k+ second tier; NOTABLE_VALUE_UNMATCHED is 100k-500k.$d$,
        $d$Gunluk finansal kapanis sonrasi.$d$,
        $d$After the daily financial close.$d$,
        $d$Recon, finans, risk, KYC ekibi.$d$,
        $d$Recon, finance, risk, KYC team.$d$,
        $d$CRITICAL_HIGH_VALUE_UNMATCHED satirlari risk komitesine eskale, gerekirse manuel hareketle dengele; HIGH_VALUE_UNMATCHED operasyon ekibinde isleme alinir.$d$,
        $d$Escalate CRITICAL_HIGH_VALUE_UNMATCHED to the risk committee, balance manually if required; HIGH_VALUE_UNMATCHED is processed by operations.$d$,
        $d$[KOLONLAR]
- data_scope (text): LIVE | ARCHIVE.
- network (text): BKM | VISA | MSC.
- detail_id (uuid), file_line_id (uuid).
- transaction_date (date).
- original_amount (numeric, >= 100000).
- currency (text, ISO 4217 numerik).
- merchant_name, merchant_country (text|'UNKNOWN').
- card_mask (text, nullable): ilk 6 + '****' + son 4 (PAN >=10 hane ise).
- risk_flag (text): CRITICAL_HIGH_VALUE_UNMATCHED (>=1M) | HIGH_VALUE_UNMATCHED (>=500k) | NOTABLE_VALUE_UNMATCHED (>=100k) | HISTORICAL_HIGH_VALUE.
- urgency (text): P1 | P2 | P3 | P4 (ARCHIVE).
- recommended_action (text): INVESTIGATE_HIGH_VALUE_UNMATCHED_TRANSACTION | INVESTIGATE_UNMATCHED_TRANSACTION | HISTORICAL_TREND_ANALYSIS_ONLY.$d$,
        $d$[COLUMNS]
- data_scope (text): LIVE | ARCHIVE.
- network (text): BKM | VISA | MSC.
- detail_id (uuid), file_line_id (uuid).
- transaction_date (date).
- original_amount (numeric, >= 100000).
- currency (text, ISO 4217 numeric).
- merchant_name, merchant_country (text|'UNKNOWN').
- card_mask (text, nullable): first 6 + '****' + last 4 (only when PAN length >= 10).
- risk_flag (text): CRITICAL_HIGH_VALUE_UNMATCHED (>=1M) | HIGH_VALUE_UNMATCHED (>=500k) | NOTABLE_VALUE_UNMATCHED (>=100k) | HISTORICAL_HIGH_VALUE.
- urgency (text): P1 | P2 | P3 | P4 (ARCHIVE).
- recommended_action (text): INVESTIGATE_HIGH_VALUE_UNMATCHED_TRANSACTION | INVESTIGATE_UNMATCHED_TRANSACTION | HISTORICAL_TREND_ANALYSIS_ONLY.$d$,
        $d$LIVE: aktif yuksek tutarli risk. ARCHIVE: tarihi yuksek tutarli profil.$d$,
        $d$LIVE: active high-value risk. ARCHIVE: historical high-value profile.$d$,
        $d$[KAYNAK TABLOLAR] ingestion.card_*_detail JOIN ingestion.file_line + ARCHIVE muadilleri.
[ESIK] Tek satir bazinda original_amount >= 100.000.
[NOT] Kart numarasi PAN-mask edilir (ilk6 + **** + son4); 100k esiginin altindaki islemler dahil edilmez.
[ORTAK ENUM TABLOSU]
data_scope: LIVE | ARCHIVE
urgency: P1 (kritik, dakikalar icinde) | P2 (yuksek, saat icinde) | P3 (orta, vardiya icinde) | P4 (dusuk, gunluk) | P5 (bilgi, izleme).
[ORTAK NETWORK / SIDE]
network: BKM | VISA | MSC (Mastercard); Visa MS view'lerinde 'Visa', 'Msc', 'Bkm'.
side / file_type: Card | Clearing.
content_type: Visa | Msc | Bkm.
$d$,
        $d$[SOURCE TABLES] ingestion.card_*_detail JOIN ingestion.file_line + ARCHIVE counterparts.
[THRESHOLD] Per-row original_amount >= 100,000.
[NOTE] Card number is PAN-masked (first6 + **** + last4); transactions below 100k are excluded.
[SHARED ENUM TABLE]
data_scope: LIVE | ARCHIVE
urgency: P1 (critical, minutes) | P2 (high, hours) | P3 (medium, within shift) | P4 (low, daily) | P5 (info, watch).
[SHARED NETWORK / SIDE]
network: BKM | VISA | MSC (Mastercard); 'Visa', 'Msc', 'Bkm' in MS variants.
side / file_type: Card | Clearing.
content_type: Visa | Msc | Bkm.
$d$
    )
) AS d(
    view_name, report_group,
    purpose_tr, purpose_en, business_question_tr, business_question_en,
    interpretation_tr, interpretation_en, usage_time_tr, usage_time_en,
    target_user_tr, target_user_en, action_guidance_tr, action_guidance_en,
    important_columns_tr, important_columns_en,
    live_archive_interpretation_tr, live_archive_interpretation_en,
    notes_tr, notes_en
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

