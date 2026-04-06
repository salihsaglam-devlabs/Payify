DO $$
BEGIN
  IF NOT EXISTS (
      SELECT column_name
      FROM information_schema.columns
      WHERE table_schema='core' AND table_name='card_topup_request' AND column_name='bank_name'
  ) THEN
    ALTER TABLE core.card_topup_request ADD COLUMN bank_name varchar(300) NULL;
  END IF;
END $$;


DO $$
BEGIN
  IF NOT EXISTS (
      SELECT column_name
      FROM information_schema.columns
      WHERE table_schema='core' AND table_name='card_topup_request' AND column_name='bank_code'
  ) THEN
    ALTER TABLE core.card_topup_request ADD COLUMN bank_code int NOT NULL DEFAULT 0;
  END IF;
END $$;
