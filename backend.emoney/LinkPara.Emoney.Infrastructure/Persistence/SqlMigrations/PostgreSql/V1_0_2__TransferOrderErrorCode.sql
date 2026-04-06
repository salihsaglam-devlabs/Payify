
DO $$
BEGIN
  IF NOT EXISTS (
      SELECT column_name
               FROM information_schema.columns
               WHERE table_schema='core' AND table_name='transfer_order' AND column_name='error_message'
    ) THEN
ALTER TABLE core.transfer_order ADD error_message character varying(500) NULL;
END IF;
END $$;
