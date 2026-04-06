DO $$
    BEGIN
        IF EXISTS (
            SELECT 1 FROM information_schema.columns
            WHERE table_schema = 'core'
              AND table_name = 'commission'
              AND column_name = 'min_value'
        ) THEN
            ALTER TABLE core.commission
                ALTER COLUMN min_value TYPE numeric(18,4) USING min_value::numeric(18,4);
        END IF;

        IF EXISTS (
            SELECT 1 FROM information_schema.columns
            WHERE table_schema = 'core'
              AND table_name = 'commission'
              AND column_name = 'max_value'
        ) THEN
            ALTER TABLE core.commission
                ALTER COLUMN max_value TYPE numeric(18,4) USING max_value::numeric(18,4);
        END IF;
    END $$;