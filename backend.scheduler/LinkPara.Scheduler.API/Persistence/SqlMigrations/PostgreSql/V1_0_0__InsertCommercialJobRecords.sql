DO $$
BEGIN
    
    INSERT INTO core.cron_job
    (id, "name", cron_expression, description, "module", cron_job_type, http_type, uri, create_date, update_date, created_by, last_modified_by, record_status)
    select '4538ecc9-1829-41ef-9091-27e23e51fa2a'::uuid, 'CheckCommercialPricingActivationDateJob', '*/1 * * * *', 'Checks Commercial Pricing Activation dates and activates incoming pricing records', 'Emoney', 'QueueMessage', 'None', '', '2023-08-31 10:00:00.000', NULL, 'BATCH', 'BATCH', 'Active'
    where not exists (select * from core.cron_job where "name" = 'CheckCommercialPricingActivationDateJob');

    INSERT INTO core.cron_job
    (id, "name", cron_expression, description, "module", cron_job_type, http_type, uri, create_date, update_date, created_by, last_modified_by, record_status)
    select '09fc2785-1175-4be9-b95a-1ec4dcff3c2b'::uuid, 'ResetCommercialAccountsJob', '0 0 1 * *', 'Makes Accounts IsCommercial column ''false''', 'Emoney', 'QueueMessage', 'None', '', '2023-08-31 10:00:00.000', NULL, 'BATCH', 'BATCH', 'Active'
    where not exists (select * from core.cron_job where "name" = 'ResetCommercialAccountsJob');
    
END $$;