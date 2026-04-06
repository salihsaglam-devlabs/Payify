DO $$
BEGIN
  IF NOT EXISTS (
      SELECT column_name
               FROM information_schema.columns
               WHERE table_schema='core' AND table_name='provision' AND column_name='commission_amount'
    ) THEN
	ALTER TABLE core.provision ADD commission_amount numeric(18, 4) NOT NULL DEFAULT 0.0;
  END IF;
END $$;
 
DO $$
BEGIN
  IF NOT EXISTS (
      SELECT column_name
               FROM information_schema.columns
               WHERE table_schema='core' AND table_name='provision' AND column_name='bsmv_amount'
    ) THEN
	ALTER TABLE core.provision ADD bsmv_amount numeric(18, 4) NOT NULL DEFAULT 0.0;
  END IF;
END $$;