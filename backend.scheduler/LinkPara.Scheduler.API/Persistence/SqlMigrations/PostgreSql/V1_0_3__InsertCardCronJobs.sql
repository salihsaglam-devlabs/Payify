DO $$
BEGIN

    UPDATE core.cron_job
    SET
        cron_expression = '15,45 * * * *',
        description = 'Imports card transaction files from FTP',
        "module" = 'Card',
        cron_job_type = 'QueueMessage',
        http_type = 'None',
        uri = NULL,
        update_date = CURRENT_TIMESTAMP,
        last_modified_by = 'BATCH',
        record_status = 'Active'
    WHERE "name" = 'ImportCardTransactionsFromFtpJob';

    IF NOT FOUND THEN
        INSERT INTO core.cron_job
        (id, "name", cron_expression, description, "module", cron_job_type, http_type, uri, create_date, update_date, created_by, last_modified_by, record_status)
        VALUES
        ('bdb6d2ee-9198-4b7f-bb5a-02adf7236f7d'::uuid, 'ImportCardTransactionsFromFtpJob', '15,45 * * * *', 'Imports card transaction files from FTP', 'Card', 'QueueMessage', 'None', NULL, CURRENT_TIMESTAMP, NULL, 'BATCH', NULL, 'Active');
    END IF;

    UPDATE core.cron_job
    SET
        cron_expression = '0,30 * * * *',
        description = 'Imports clearing files from FTP',
        "module" = 'Card',
        cron_job_type = 'QueueMessage',
        http_type = 'None',
        uri = NULL,
        update_date = CURRENT_TIMESTAMP,
        last_modified_by = 'BATCH',
        record_status = 'Active'
    WHERE "name" = 'ImportClearingFromFtpJob';

    IF NOT FOUND THEN
        INSERT INTO core.cron_job
        (id, "name", cron_expression, description, "module", cron_job_type, http_type, uri, create_date, update_date, created_by, last_modified_by, record_status)
        VALUES
        ('2894ce4d-1575-4e3e-a549-b31612b97ec0'::uuid, 'ImportClearingFromFtpJob', '0,30 * * * *', 'Imports clearing files from FTP', 'Card', 'QueueMessage', 'None', NULL, CURRENT_TIMESTAMP, NULL, 'BATCH', NULL, 'Active');
    END IF;

    UPDATE core.cron_job
    SET
        cron_expression = '20 22 * * *',
        description = 'Executes pending reconciliation operations',
        "module" = 'Card',
        cron_job_type = 'QueueMessage',
        http_type = 'None',
        uri = NULL,
        update_date = CURRENT_TIMESTAMP,
        last_modified_by = 'BATCH',
        record_status = 'Active'
    WHERE "name" = 'ExecutePendingOperationsJob';

    IF NOT FOUND THEN
        INSERT INTO core.cron_job
        (id, "name", cron_expression, description, "module", cron_job_type, http_type, uri, create_date, update_date, created_by, last_modified_by, record_status)
        VALUES
        ('8f9fcf42-ec8f-4e4f-a318-39357b466098'::uuid, 'ExecutePendingOperationsJob', '20 22 * * *', 'Executes pending reconciliation operations', 'Card', 'QueueMessage', 'None', NULL, CURRENT_TIMESTAMP, NULL, 'BATCH', NULL, 'Active');
    END IF;

    UPDATE core.cron_job
    SET
        cron_expression = '1 22 * * *',
        description = 'Regenerates reconciliation operations',
        "module" = 'Card',
        cron_job_type = 'QueueMessage',
        http_type = 'None',
        uri = NULL,
        update_date = CURRENT_TIMESTAMP,
        last_modified_by = 'BATCH',
        record_status = 'Active'
    WHERE "name" = 'RegenerateOperationsJob';

    IF NOT FOUND THEN
        INSERT INTO core.cron_job
        (id, "name", cron_expression, description, "module", cron_job_type, http_type, uri, create_date, update_date, created_by, last_modified_by, record_status)
        VALUES
        ('949f1fab-0cb6-4e20-8d8d-c66a2632dfaf'::uuid, 'RegenerateOperationsJob', '1 22 * * *', 'Regenerates reconciliation operations', 'Card', 'QueueMessage', 'None', NULL, CURRENT_TIMESTAMP, NULL, 'BATCH', NULL, 'Active');
    END IF;

END $$;
