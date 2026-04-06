DO $$
BEGIN
    IF EXISTS (SELECT 1 FROM information_schema.tables
               WHERE table_schema = 'core' AND table_name = 'transaction_receipt_counter') THEN
        DROP TABLE core.transaction_receipt_counter;
    END IF;
END $$;
 