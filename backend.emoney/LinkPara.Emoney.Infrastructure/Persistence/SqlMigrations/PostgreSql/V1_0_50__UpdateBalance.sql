DO $$
BEGIN
  IF NOT EXISTS (
      SELECT column_name
               FROM information_schema.columns
               WHERE table_schema='core' AND table_name='transaction' AND column_name='customer_transaction_id'
    ) THEN
ALTER TABLE core.transaction ADD customer_transaction_id character varying(50);
END IF;
END $$;

DO $$
BEGIN
  IF NOT EXISTS (
      SELECT column_name
               FROM information_schema.columns
               WHERE table_schema='core' AND table_name='transaction' AND column_name='is_cancelled'
    ) THEN
ALTER TABLE core.transaction ADD is_cancelled boolean NOT NULL DEFAULT FALSE;
END IF;
END $$;

DO $$
BEGIN
  IF NOT EXISTS (
      SELECT column_name
               FROM information_schema.columns
               WHERE table_schema='core' AND table_name='transaction' AND column_name='is_settlement_received'
    ) THEN
ALTER TABLE core.transaction ADD is_settlement_received boolean NOT NULL DEFAULT FALSE;
END IF;
END $$;

DO $$
BEGIN
  IF NOT EXISTS (
      SELECT column_name
               FROM information_schema.columns
               WHERE table_schema='core' AND table_name='transaction' AND column_name='payment_channel'
    ) THEN
ALTER TABLE core.transaction ADD payment_channel character varying(300);
END IF;
END $$;

