DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM information_schema.columns
        WHERE table_schema = 'core'
          AND table_name = 'user'
          AND column_name = 'external_customer_id'
    ) THEN
        ALTER TABLE core."user" ADD COLUMN external_customer_id character varying(400);
    END IF;
END $$;


DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM information_schema.columns
        WHERE table_schema = 'core'
          AND table_name = 'user'
          AND column_name = 'external_person_id'
    ) THEN
        ALTER TABLE core."user" ADD COLUMN external_person_id character varying(400);
    END IF;
END $$;
