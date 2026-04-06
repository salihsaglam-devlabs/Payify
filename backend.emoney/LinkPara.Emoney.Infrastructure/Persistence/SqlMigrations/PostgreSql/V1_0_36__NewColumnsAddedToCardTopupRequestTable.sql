DO $$
BEGIN
  IF NOT EXISTS (
      SELECT column_name
               FROM information_schema.columns
               WHERE table_schema='core' AND table_name='card_topup_request' AND column_name='authentication_method'
    ) THEN
ALTER TABLE core.card_topup_request ADD authentication_method character varying(50);
END IF;
END $$;

DO $$
BEGIN
  IF NOT EXISTS (
      SELECT column_name
               FROM information_schema.columns
               WHERE table_schema='core' AND table_name='card_topup_request' AND column_name='secure3d_type'
    ) THEN
ALTER TABLE core.card_topup_request ADD secure3d_type character varying(50);
END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT column_name
        FROM information_schema.columns
        WHERE table_schema='core' AND table_name='card_topup_request' AND column_name='transaction_date'
    ) THEN
ALTER TABLE core.card_topup_request ADD COLUMN transaction_date timestamp without time zone NULL;
    END IF;
END $$;
