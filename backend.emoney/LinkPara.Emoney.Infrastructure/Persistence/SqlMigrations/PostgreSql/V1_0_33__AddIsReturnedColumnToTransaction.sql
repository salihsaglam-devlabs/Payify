DO $$
BEGIN
  IF NOT EXISTS (
      SELECT column_name
      FROM information_schema.columns
      WHERE table_schema='core' AND table_name='transaction' AND column_name='is_returned'
  ) THEN
    ALTER TABLE core.transaction ADD is_returned boolean NOT NULL DEFAULT false;
  END IF;
END $$; 