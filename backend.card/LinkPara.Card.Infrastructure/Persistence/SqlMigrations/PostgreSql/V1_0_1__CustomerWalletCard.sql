DO $$
BEGIN
  IF EXISTS (
      SELECT column_name
               FROM information_schema.columns
               WHERE table_schema='core' AND table_name='customer_wallet_card' AND column_name='user_id'
    ) THEN
      ALTER TABLE core.customer_wallet_card DROP COLUMN user_id;
END IF;
END $$;

DO $$
BEGIN
  IF NOT EXISTS (
      SELECT column_name
               FROM information_schema.columns
               WHERE table_schema='core' AND table_name='customer_wallet_card' AND column_name='card_status'
    ) THEN
      ALTER TABLE core.customer_wallet_card ADD card_status text;
END IF;
END $$;