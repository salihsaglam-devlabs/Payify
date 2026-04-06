DO $$
BEGIN
    
    INSERT INTO core.cron_job
    (id, "name", cron_expression, description, "module", cron_job_type, http_type, uri, create_date, update_date, created_by, last_modified_by, record_status)
    select '90462ee4-9134-47c8-8a43-5ac5658dcb4d'::uuid, 'PostingDailyControlJob', '0 6 * * *', 'Controls Postings daily', 'Pf', 'QueueMessage', 'None', NULL, '2024-08-19 17:27:44.000', NULL, 'BATCH', 'BATCH', 'Active'
    where not exists (select * from core.cron_job where "name" = 'PostingDailyControlJob');

END $$;