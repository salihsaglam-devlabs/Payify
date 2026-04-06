DO $$
BEGIN
  IF NOT EXISTS (
      SELECT column_name
               FROM information_schema.columns
               WHERE table_schema='core' AND table_name='transaction' AND column_name='sender_account_number'
    ) THEN
ALTER TABLE core.transaction ADD sender_account_number character varying(26) NULL;
END IF;
END $$;