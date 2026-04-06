
DO $$
BEGIN
  IF NOT EXISTS (
      SELECT column_name
               FROM information_schema.columns
               WHERE table_schema='core' AND table_name='bank_account' AND column_name='account_name'
    ) THEN
    ALTER TABLE core.bank_account ADD account_name character varying(300) NULL;
END IF;
END $$;




