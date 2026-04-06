DO $$
BEGIN
  IF NOT EXISTS (
      SELECT column_name
               FROM information_schema.columns
               WHERE table_schema='core' AND table_name='transaction' AND column_name='used_credit_amount'
    ) THEN
ALTER TABLE core.transaction ADD used_credit_amount numeric(18,2) NOT NULL DEFAULT 0.0;
END IF;
END $$;


