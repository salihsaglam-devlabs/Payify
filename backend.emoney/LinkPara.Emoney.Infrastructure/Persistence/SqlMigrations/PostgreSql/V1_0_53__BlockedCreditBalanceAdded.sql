DO $$
BEGIN
  IF NOT EXISTS (
      SELECT column_name
               FROM information_schema.columns
               WHERE table_schema='core' AND table_name='wallet' AND column_name='blocked_balance_credit'
    ) THEN
ALTER TABLE core.wallet ADD blocked_balance_credit numeric(18,2) NOT NULL DEFAULT 0.0;
END IF;
END $$;


