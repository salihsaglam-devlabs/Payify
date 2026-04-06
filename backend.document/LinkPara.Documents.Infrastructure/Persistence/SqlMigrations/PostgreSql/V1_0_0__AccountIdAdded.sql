DO $$
BEGIN
  IF NOT EXISTS (
      SELECT column_name
               FROM information_schema.columns
               WHERE table_schema='core' AND table_name='document' AND column_name='account_id'
    ) THEN
        ALTER TABLE core.document ADD account_id uuid NULL;
END IF;
END $$;