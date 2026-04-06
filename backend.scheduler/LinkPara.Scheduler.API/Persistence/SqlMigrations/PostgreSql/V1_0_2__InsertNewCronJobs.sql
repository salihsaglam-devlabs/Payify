DO $$
BEGIN
    
    INSERT INTO core.cron_job
    (id, "name", cron_expression, description, "module", cron_job_type, http_type, uri, create_date, update_date, created_by, last_modified_by, record_status)
    VALUES('32f8e76f-ae92-40fb-bf3e-a3d7f9db448e'::uuid, 'CheckBankLimitStatusJob', '0 1 * * *', 'Checks bank limit statuses', 'Pf', 'QueueMessage', 'None', NULL, '2023-05-01 17:27:44.000', NULL, 'BATCH', 'BATCH', 'Active') ON CONFLICT DO NOTHING;

    INSERT INTO core.cron_job
    (id, "name", cron_expression, description, "module", cron_job_type, http_type, uri, create_date, update_date, created_by, last_modified_by, record_status)
    VALUES('9b9da251-08c4-4d92-9504-c796be3e4d09'::uuid, 'CheckBlacklistControlJob', '0 0 1 * *', 'Check Merchant Blacklist Control', 'Pf', 'QueueMessage', 'None', NULL, '2023-11-14 17:27:44.000', NULL, 'BATCH', 'BATCH', 'Active') ON CONFLICT DO NOTHING;

    INSERT INTO core.cron_job
    (id, "name", cron_expression, description, "module", cron_job_type, http_type, uri, create_date, update_date, created_by, last_modified_by, record_status)
    VALUES('c8e40821-f793-40c2-9734-42bd86cb84c6'::uuid, 'DeleteBankHealthCheckTransactionJob', '0 23 * * *', 'Deletes old bank health check transactions', 'Pf', 'QueueMessage', 'None', NULL, '2024-08-21 17:27:44.000', NULL, 'BATCH', 'BATCH', 'Active') ON CONFLICT DO NOTHING;

END $$;