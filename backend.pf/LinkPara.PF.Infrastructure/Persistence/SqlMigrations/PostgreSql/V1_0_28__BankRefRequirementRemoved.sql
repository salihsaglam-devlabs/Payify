DO $$
BEGIN
    IF EXISTS (
        SELECT 1
        FROM information_schema.columns
        WHERE table_schema = 'physical'
          AND table_name = 'unacceptable_transaction'
          AND column_name = 'bank_ref'
          AND is_nullable = 'NO'
    ) THEN
ALTER TABLE physical.unacceptable_transaction
    ALTER COLUMN bank_ref DROP NOT NULL;
END IF;

    IF EXISTS (
        SELECT 1
        FROM information_schema.columns
        WHERE table_schema = 'physical'
          AND table_name = 'reconciliation_transaction'
          AND column_name = 'bank_ref'
          AND is_nullable = 'NO'
    ) THEN
ALTER TABLE physical.reconciliation_transaction
    ALTER COLUMN bank_ref DROP NOT NULL;
END IF;
END $$;