DO $$
BEGIN
    IF EXISTS (
        SELECT 1
        FROM information_schema.table_constraints
        WHERE constraint_schema = 'merchant'
          AND table_name = 'pool'
          AND constraint_name = 'fk_pool_bank_bank_code'
    ) THEN
        ALTER TABLE merchant.pool
        DROP CONSTRAINT fk_pool_bank_bank_code;
    END IF;

    IF EXISTS (
        SELECT 1
        FROM pg_indexes
        WHERE schemaname = 'merchant'
          AND tablename = 'pool'
          AND indexname = 'ix_pool_bank_code'
    ) THEN
        DROP INDEX merchant.ix_pool_bank_code;
    END IF;
END$$;

DO $$
BEGIN
    IF EXISTS (
        SELECT 1
        FROM information_schema.columns
        WHERE table_schema = 'merchant'
          AND table_name = 'pool'
          AND column_name = 'iban'
          AND is_nullable = 'NO'
    ) THEN
        ALTER TABLE merchant.pool
        ALTER COLUMN iban DROP NOT NULL;
    END IF;

    IF EXISTS (
        SELECT 1
        FROM information_schema.columns
        WHERE table_schema = 'merchant'
          AND table_name = 'merchant_return_pool'
          AND column_name = 'bank_name'
          AND is_nullable = 'NO'
    ) THEN
        ALTER TABLE merchant.merchant_return_pool
        ALTER COLUMN bank_name DROP NOT NULL;
    END IF;
END$$;