DO $$
BEGIN
  IF NOT EXISTS (
      SELECT column_name
               FROM information_schema.columns
               WHERE table_schema='core' AND table_name='transaction' AND column_name='bsmv_amount'
    ) THEN
	ALTER TABLE core.transaction ADD bsmv_amount numeric(18, 4) NOT NULL DEFAULT 0.0;
  END IF;
END $$;