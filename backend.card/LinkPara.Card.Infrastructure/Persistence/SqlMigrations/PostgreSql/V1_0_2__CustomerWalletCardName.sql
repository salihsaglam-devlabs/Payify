
DO $$
BEGIN
  IF NOT EXISTS (
      SELECT column_name
               FROM information_schema.columns
               WHERE table_schema='core' AND table_name='customer_wallet_card' AND column_name='card_name'
    ) THEN
    ALTER TABLE core.customer_wallet_card ADD card_name character varying(30);
END IF;
END $$;