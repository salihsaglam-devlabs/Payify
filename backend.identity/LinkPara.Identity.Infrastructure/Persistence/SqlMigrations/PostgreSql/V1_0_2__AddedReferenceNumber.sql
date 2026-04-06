DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM information_schema.columns
        WHERE table_schema = 'core'
          AND table_name = 'user'
          AND column_name = 'aml_reference_number'
    ) THEN
        ALTER TABLE core."user" ADD COLUMN aml_reference_number character varying(150);
    END IF;
END $$;