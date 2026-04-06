DO $$
BEGIN
  IF NOT EXISTS (
      SELECT column_name
               FROM information_schema.columns
               WHERE table_schema='core' AND table_name='transaction' AND column_name='idempotent_key'
    ) THEN
ALTER TABLE core.transaction ADD idempotent_key character varying(100) NULL;
END IF;
END $$;