DO $$
BEGIN
  IF NOT EXISTS (
      SELECT column_name
               FROM information_schema.columns
               WHERE table_schema='core' AND table_name='withdraw_request' AND column_name='is_processed'
    ) THEN
    ALTER TABLE core.withdraw_request ADD is_processed bool NOT NULL DEFAULT false;
END IF;
END $$;

DO $$
BEGIN
  IF NOT EXISTS (
      SELECT column_name
               FROM information_schema.columns
               WHERE table_schema='core' AND table_name='return_transaction_request' AND column_name='is_processed'
    ) THEN
    ALTER TABLE core.return_transaction_request ADD is_processed bool NOT NULL DEFAULT false;
END IF;
END $$;
