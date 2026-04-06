
DO $$
BEGIN
  IF EXISTS (
      SELECT column_name
               FROM information_schema.columns
               WHERE table_schema='core' AND table_name='request' AND column_name='body'
    ) THEN
    ALTER TABLE core.request ALTER COLUMN body TYPE text;
END IF;
END $$;