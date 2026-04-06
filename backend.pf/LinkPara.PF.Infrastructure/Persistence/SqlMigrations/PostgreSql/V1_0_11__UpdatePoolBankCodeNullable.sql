DO $$
BEGIN
    IF EXISTS (
        SELECT 1
        FROM information_schema.columns
        WHERE table_schema = 'merchant'
          AND table_name = 'pool'
          AND column_name = 'bank_code'
          AND is_nullable = 'NO'
    ) THEN
        ALTER TABLE merchant.pool
        ALTER COLUMN bank_code DROP NOT NULL;
    END IF;
END$$;