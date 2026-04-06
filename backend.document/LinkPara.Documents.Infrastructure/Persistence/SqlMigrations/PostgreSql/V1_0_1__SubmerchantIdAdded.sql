DO $$
BEGIN
  IF NOT EXISTS (
      SELECT column_name
               FROM information_schema.columns
               WHERE table_schema='core' AND table_name='document' AND column_name='sub_merchant_id'
    ) THEN
ALTER TABLE core.document ADD sub_merchant_id uuid NULL;
END IF;
END $$;