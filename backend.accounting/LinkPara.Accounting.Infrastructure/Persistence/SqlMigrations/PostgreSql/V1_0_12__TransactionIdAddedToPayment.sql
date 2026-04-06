DO $$
BEGIN
  IF NOT EXISTS (
      SELECT column_name
               FROM information_schema.columns
               WHERE table_schema='core' AND table_name='payment' AND column_name='transaction_id'
    ) THEN
    ALTER TABLE core.payment ADD transaction_id character varying(40) NULL DEFAULT '';
END IF;
END $$;
