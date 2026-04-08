DO
$$
BEGIN

    /*
      REMOTE JOBS - ACTIVE
    */

UPDATE core.cron_job
SET cron_expression  = '15 1 * * *', -- Her gün 01:15'te çalışır
    description      = 'Imports remote BKM card transaction files and queues evaluate/execute steps',
    "module"         = 'Card',
    cron_job_type    = 'QueueMessage',
    http_type        = 'None',
    uri              = NULL,
    update_date      = CURRENT_TIMESTAMP,
    last_modified_by = 'BATCH',
    record_status    = 'Active'
WHERE "name" = 'RemoteCardBkmJob';

IF
NOT FOUND THEN
        INSERT INTO core.cron_job
        (id, "name", cron_expression, description, "module", cron_job_type, http_type, uri, create_date, update_date, created_by, last_modified_by, record_status)
        VALUES
        ('e1a6d47c-5d64-4f63-9a54-9f8b0d5b1001'::uuid, 'RemoteCardBkmJob', '15 1 * * *', 'Imports remote BKM card transaction files and queues evaluate/execute steps', 'Card', 'QueueMessage', 'None', NULL, CURRENT_TIMESTAMP, NULL, 'BATCH', NULL, 'Active');
END IF;


UPDATE core.cron_job
SET cron_expression  = '45 15 * * *', -- Her gün 15:45'te çalışır
    description      = 'Imports remote VISA card transaction files and queues evaluate/execute steps',
    "module"         = 'Card',
    cron_job_type    = 'QueueMessage',
    http_type        = 'None',
    uri              = NULL,
    update_date      = CURRENT_TIMESTAMP,
    last_modified_by = 'BATCH',
    record_status    = 'Active'
WHERE "name" = 'RemoteCardVisaJob';

IF
NOT FOUND THEN
        INSERT INTO core.cron_job
        (id, "name", cron_expression, description, "module", cron_job_type, http_type, uri, create_date, update_date, created_by, last_modified_by, record_status)
        VALUES
        ('e1a6d47c-5d64-4f63-9a54-9f8b0d5b1002'::uuid, 'RemoteCardVisaJob', '45 15 * * *', 'Imports remote VISA card transaction files and queues evaluate/execute steps', 'Card', 'QueueMessage', 'None', NULL, CURRENT_TIMESTAMP, NULL, 'BATCH', NULL, 'Active');
END IF;


UPDATE core.cron_job
SET cron_expression  = '45 19 * * *', -- Her gün 19:45'te çalışır
    description      = 'Imports remote MSC card transaction files and queues evaluate/execute steps',
    "module"         = 'Card',
    cron_job_type    = 'QueueMessage',
    http_type        = 'None',
    uri              = NULL,
    update_date      = CURRENT_TIMESTAMP,
    last_modified_by = 'BATCH',
    record_status    = 'Active'
WHERE "name" = 'RemoteCardMscJob';

IF
NOT FOUND THEN
        INSERT INTO core.cron_job
        (id, "name", cron_expression, description, "module", cron_job_type, http_type, uri, create_date, update_date, created_by, last_modified_by, record_status)
        VALUES
        ('e1a6d47c-5d64-4f63-9a54-9f8b0d5b1003'::uuid, 'RemoteCardMscJob', '45 19 * * *', 'Imports remote MSC card transaction files and queues evaluate/execute steps', 'Card', 'QueueMessage', 'None', NULL, CURRENT_TIMESTAMP, NULL, 'BATCH', NULL, 'Active');
END IF;


UPDATE core.cron_job
SET cron_expression  = '15 16 * * *', -- Her gün 16:15'te çalışır
    description      = 'Imports remote BKM clearing files and queues evaluate/execute steps',
    "module"         = 'Card',
    cron_job_type    = 'QueueMessage',
    http_type        = 'None',
    uri              = NULL,
    update_date      = CURRENT_TIMESTAMP,
    last_modified_by = 'BATCH',
    record_status    = 'Active'
WHERE "name" = 'RemoteClearingBkmJob';

IF
NOT FOUND THEN
        INSERT INTO core.cron_job
        (id, "name", cron_expression, description, "module", cron_job_type, http_type, uri, create_date, update_date, created_by, last_modified_by, record_status)
        VALUES
        ('e1a6d47c-5d64-4f63-9a54-9f8b0d5b1004'::uuid, 'RemoteClearingBkmJob', '45 13 * * *', 'Imports remote BKM clearing files and queues evaluate/execute steps', 'Card', 'QueueMessage', 'None', NULL, CURRENT_TIMESTAMP, NULL, 'BATCH', NULL, 'Active');
END IF;


UPDATE core.cron_job
SET cron_expression  = '45 15 * * *', -- Her gün 15:45'te çalışır
    description      = 'Imports remote VISA clearing files and queues evaluate/execute steps',
    "module"         = 'Card',
    cron_job_type    = 'QueueMessage',
    http_type        = 'None',
    uri              = NULL,
    update_date      = CURRENT_TIMESTAMP,
    last_modified_by = 'BATCH',
    record_status    = 'Active'
WHERE "name" = 'RemoteClearingVisaJob';

IF
NOT FOUND THEN
        INSERT INTO core.cron_job
        (id, "name", cron_expression, description, "module", cron_job_type, http_type, uri, create_date, update_date, created_by, last_modified_by, record_status)
        VALUES
        ('e1a6d47c-5d64-4f63-9a54-9f8b0d5b1005'::uuid, 'RemoteClearingVisaJob', '45 15 * * *', 'Imports remote VISA clearing files and queues evaluate/execute steps', 'Card', 'QueueMessage', 'None', NULL, CURRENT_TIMESTAMP, NULL, 'BATCH', NULL, 'Active');
END IF;


UPDATE core.cron_job
SET cron_expression  = '45 19 * * *', -- Her gün 19:45'te çalışır
    description      = 'Imports remote MSC clearing files and queues evaluate/execute steps',
    "module"         = 'Card',
    cron_job_type    = 'QueueMessage',
    http_type        = 'None',
    uri              = NULL,
    update_date      = CURRENT_TIMESTAMP,
    last_modified_by = 'BATCH',
    record_status    = 'Active'
WHERE "name" = 'RemoteClearingMscJob';

IF
NOT FOUND THEN
        INSERT INTO core.cron_job
        (id, "name", cron_expression, description, "module", cron_job_type, http_type, uri, create_date, update_date, created_by, last_modified_by, record_status)
        VALUES
        ('e1a6d47c-5d64-4f63-9a54-9f8b0d5b1006'::uuid, 'RemoteClearingMscJob', '45 19 * * *', 'Imports remote MSC clearing files and queues evaluate/execute steps', 'Card', 'QueueMessage', 'None', NULL, CURRENT_TIMESTAMP, NULL, 'BATCH', NULL, 'Active');
END IF;


    /*
      END OF DAY / AFTER MIDNIGHT CONTROL JOBS - ACTIVE
    */

UPDATE core.cron_job
SET cron_expression  = '15 23 * * *', -- Her gün 23:15'te çalışır
    description      = 'Runs default reconciliation evaluate step as end-of-day control after file pipelines',
    "module"         = 'Card',
    cron_job_type    = 'QueueMessage',
    http_type        = 'None',
    uri              = NULL,
    update_date      = CURRENT_TIMESTAMP,
    last_modified_by = 'BATCH',
    record_status    = 'Active'
WHERE "name" = 'EvaluateDefaultJob';

IF
NOT FOUND THEN
        INSERT INTO core.cron_job
        (id, "name", cron_expression, description, "module", cron_job_type, http_type, uri, create_date, update_date, created_by, last_modified_by, record_status)
        VALUES
        ('e1a6d47c-5d64-4f63-9a54-9f8b0d5b1007'::uuid, 'EvaluateDefaultJob', '15 23 * * *', 'Runs default reconciliation evaluate step as end-of-day control after file pipelines', 'Card', 'QueueMessage', 'None', NULL, CURRENT_TIMESTAMP, NULL, 'BATCH', NULL, 'Active');
END IF;


UPDATE core.cron_job
SET cron_expression  = '15 0 * * *', -- Her gün 00:15'te çalışır
    description      = 'Runs default reconciliation execute step after end-of-day evaluate control',
    "module"         = 'Card',
    cron_job_type    = 'QueueMessage',
    http_type        = 'None',
    uri              = NULL,
    update_date      = CURRENT_TIMESTAMP,
    last_modified_by = 'BATCH',
    record_status    = 'Active'
WHERE "name" = 'ExecuteDefaultJob';

IF
NOT FOUND THEN
        INSERT INTO core.cron_job
        (id, "name", cron_expression, description, "module", cron_job_type, http_type, uri, create_date, update_date, created_by, last_modified_by, record_status)
        VALUES
        ('e1a6d47c-5d64-4f63-9a54-9f8b0d5b1008'::uuid, 'ExecuteDefaultJob', '15 0 * * *', 'Runs default reconciliation execute step after end-of-day evaluate control', 'Card', 'QueueMessage', 'None', NULL, CURRENT_TIMESTAMP, NULL, 'BATCH', NULL, 'Active');
END IF;


    /*
      LOCAL JOBS - DEFINED BUT DISABLED IN SCRIPT
      These are intentionally not executed now.
    */
    IF
1 = 2 THEN

UPDATE core.cron_job
SET cron_expression  = '0 9 * * *', -- Her gün 09:00'da çalışır
    description      = 'Imports local BKM card transaction files',
    "module"         = 'Card',
    cron_job_type    = 'QueueMessage',
    http_type        = 'None',
    uri              = NULL,
    update_date      = CURRENT_TIMESTAMP,
    last_modified_by = 'BATCH',
    record_status    = 'Active'
WHERE "name" = 'LocalCardBkmJob';

IF
NOT FOUND THEN
            INSERT INTO core.cron_job
            (id, "name", cron_expression, description, "module", cron_job_type, http_type, uri, create_date, update_date, created_by, last_modified_by, record_status)
            VALUES
            ('e1a6d47c-5d64-4f63-9a54-9f8b0d5b1009'::uuid, 'LocalCardBkmJob', '0 9 * * *', 'Imports local BKM card transaction files', 'Card', 'QueueMessage', 'None', NULL, CURRENT_TIMESTAMP, NULL, 'BATCH', NULL, 'Active');
END IF;


UPDATE core.cron_job
SET cron_expression  = '5 9 * * *', -- Her gün 09:05'te çalışır
    description      = 'Imports local VISA card transaction files',
    "module"         = 'Card',
    cron_job_type    = 'QueueMessage',
    http_type        = 'None',
    uri              = NULL,
    update_date      = CURRENT_TIMESTAMP,
    last_modified_by = 'BATCH',
    record_status    = 'Active'
WHERE "name" = 'LocalCardVisaJob';

IF
NOT FOUND THEN
            INSERT INTO core.cron_job
            (id, "name", cron_expression, description, "module", cron_job_type, http_type, uri, create_date, update_date, created_by, last_modified_by, record_status)
            VALUES
            ('e1a6d47c-5d64-4f63-9a54-9f8b0d5b1010'::uuid, 'LocalCardVisaJob', '5 9 * * *', 'Imports local VISA card transaction files', 'Card', 'QueueMessage', 'None', NULL, CURRENT_TIMESTAMP, NULL, 'BATCH', NULL, 'Active');
END IF;


UPDATE core.cron_job
SET cron_expression  = '10 9 * * *', -- Her gün 09:10'da çalışır
    description      = 'Imports local MSC card transaction files',
    "module"         = 'Card',
    cron_job_type    = 'QueueMessage',
    http_type        = 'None',
    uri              = NULL,
    update_date      = CURRENT_TIMESTAMP,
    last_modified_by = 'BATCH',
    record_status    = 'Active'
WHERE "name" = 'LocalCardMscJob';

IF
NOT FOUND THEN
            INSERT INTO core.cron_job
            (id, "name", cron_expression, description, "module", cron_job_type, http_type, uri, create_date, update_date, created_by, last_modified_by, record_status)
            VALUES
            ('e1a6d47c-5d64-4f63-9a54-9f8b0d5b1011'::uuid, 'LocalCardMscJob', '10 9 * * *', 'Imports local MSC card transaction files', 'Card', 'QueueMessage', 'None', NULL, CURRENT_TIMESTAMP, NULL, 'BATCH', NULL, 'Active');
END IF;


UPDATE core.cron_job
SET cron_expression  = '15 9 * * *', -- Her gün 09:15'te çalışır
    description      = 'Imports local BKM clearing files',
    "module"         = 'Card',
    cron_job_type    = 'QueueMessage',
    http_type        = 'None',
    uri              = NULL,
    update_date      = CURRENT_TIMESTAMP,
    last_modified_by = 'BATCH',
    record_status    = 'Active'
WHERE "name" = 'LocalClearingBkmJob';

IF
NOT FOUND THEN
            INSERT INTO core.cron_job
            (id, "name", cron_expression, description, "module", cron_job_type, http_type, uri, create_date, update_date, created_by, last_modified_by, record_status)
            VALUES
            ('e1a6d47c-5d64-4f63-9a54-9f8b0d5b1012'::uuid, 'LocalClearingBkmJob', '15 9 * * *', 'Imports local BKM clearing files', 'Card', 'QueueMessage', 'None', NULL, CURRENT_TIMESTAMP, NULL, 'BATCH', NULL, 'Active');
END IF;


UPDATE core.cron_job
SET cron_expression  = '20 9 * * *', -- Her gün 09:20'de çalışır
    description      = 'Imports local VISA clearing files',
    "module"         = 'Card',
    cron_job_type    = 'QueueMessage',
    http_type        = 'None',
    uri              = NULL,
    update_date      = CURRENT_TIMESTAMP,
    last_modified_by = 'BATCH',
    record_status    = 'Active'
WHERE "name" = 'LocalClearingVisaJob';

IF
NOT FOUND THEN
            INSERT INTO core.cron_job
            (id, "name", cron_expression, description, "module", cron_job_type, http_type, uri, create_date, update_date, created_by, last_modified_by, record_status)
            VALUES
            ('e1a6d47c-5d64-4f63-9a54-9f8b0d5b1013'::uuid, 'LocalClearingVisaJob', '20 9 * * *', 'Imports local VISA clearing files', 'Card', 'QueueMessage', 'None', NULL, CURRENT_TIMESTAMP, NULL, 'BATCH', NULL, 'Active');
END IF;


UPDATE core.cron_job
SET cron_expression  = '25 9 * * *', -- Her gün 09:25'te çalışır
    description      = 'Imports local MSC clearing files',
    "module"         = 'Card',
    cron_job_type    = 'QueueMessage',
    http_type        = 'None',
    uri              = NULL,
    update_date      = CURRENT_TIMESTAMP,
    last_modified_by = 'BATCH',
    record_status    = 'Active'
WHERE "name" = 'LocalClearingMscJob';

IF
NOT FOUND THEN
            INSERT INTO core.cron_job
            (id, "name", cron_expression, description, "module", cron_job_type, http_type, uri, create_date, update_date, created_by, last_modified_by, record_status)
            VALUES
            ('e1a6d47c-5d64-4f63-9a54-9f8b0d5b1014'::uuid, 'LocalClearingMscJob', '25 9 * * *', 'Imports local MSC clearing files', 'Card', 'QueueMessage', 'None', NULL, CURRENT_TIMESTAMP, NULL, 'BATCH', NULL, 'Active');
END IF;

END IF;

END $$;