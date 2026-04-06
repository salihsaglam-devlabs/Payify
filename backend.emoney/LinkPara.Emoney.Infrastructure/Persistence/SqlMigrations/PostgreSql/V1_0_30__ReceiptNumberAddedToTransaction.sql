DO $$
BEGIN
  IF NOT EXISTS (
      SELECT column_name
               FROM information_schema.columns
               WHERE table_schema='core' AND table_name='transaction' AND column_name='receipt_number'
    ) THEN
ALTER TABLE core.transaction ADD receipt_number character varying(30) NULL;
END IF;
END $$;