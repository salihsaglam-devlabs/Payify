DO $$
BEGIN
  IF NOT EXISTS (
      SELECT column_name
               FROM information_schema.columns
               WHERE table_schema='core' AND table_name='account' AND column_name='customer_number'
    ) THEN
ALTER TABLE core.account ADD customer_number integer NOT NULL DEFAULT 0;
END IF;
END $$;
