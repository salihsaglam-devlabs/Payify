DO $$
BEGIN
    IF EXISTS (
        SELECT 1
        FROM information_schema.columns
        WHERE table_schema = 'core'
          AND table_name = 'three_d_verification'
          AND column_name = 'hash_key'
    ) THEN
        ALTER TABLE core.three_d_verification
            ALTER COLUMN hash_key TYPE character varying(200);
    END IF;
END$$;