DO $$
BEGIN
  IF NOT EXISTS (
      SELECT column_name
               FROM information_schema.columns
               WHERE table_schema='core' AND table_name='card_topup_request' AND column_name='account_key'
    ) THEN
ALTER TABLE core.card_topup_request ADD account_key character varying(50) NULL;
END IF;
END $$;

DO $$
BEGIN
  IF NOT EXISTS (
      SELECT column_name
               FROM information_schema.columns
               WHERE table_schema='core' AND table_name='card_topup_request' AND column_name='md_status'
    ) THEN
ALTER TABLE core.card_topup_request ADD md_status integer NULL;
END IF;
END $$;

DO $$
BEGIN
  IF EXISTS (
     SELECT column_name
     FROM information_schema.columns
     WHERE table_schema = 'core' AND table_name = 'card_topup_request' AND column_name = 'error_code'
   ) THEN
     ALTER TABLE core.card_topup_request ALTER COLUMN error_code TYPE character varying(50);
  END IF;
END $$;

DO $$
BEGIN
  IF NOT EXISTS (
      SELECT column_name
               FROM information_schema.columns
               WHERE table_schema='core' AND table_name='card_topup_request' AND column_name='payment_provider_type'
    ) THEN
ALTER TABLE core.card_topup_request ADD payment_provider_type character varying(50);
END IF;
END $$;
