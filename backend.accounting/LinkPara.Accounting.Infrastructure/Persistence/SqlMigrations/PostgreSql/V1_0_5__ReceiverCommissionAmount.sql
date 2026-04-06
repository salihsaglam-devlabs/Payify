DO $$
BEGIN
  IF NOT EXISTS (
      SELECT column_name
               FROM information_schema.columns
               WHERE table_schema='core' AND table_name='payment' AND column_name='receiver_bsmv_amount'
    ) THEN
    ALTER TABLE core.payment ADD receiver_bsmv_amount numeric(18,4) NOT NULL DEFAULT 0.0;
END IF;
END $$;

DO $$
BEGIN
  IF NOT EXISTS (
      SELECT column_name
               FROM information_schema.columns
               WHERE table_schema='core' AND table_name='payment' AND column_name='receiver_commission_amount'
    ) THEN
    ALTER TABLE core.payment ADD receiver_commission_amount numeric(18,4) NOT NULL DEFAULT 0.0;
END IF;
END $$;