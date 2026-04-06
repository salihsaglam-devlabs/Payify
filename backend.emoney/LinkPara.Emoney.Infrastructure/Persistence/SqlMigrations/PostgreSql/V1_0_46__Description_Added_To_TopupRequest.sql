
DO $$
BEGIN
  IF NOT EXISTS (
      SELECT column_name
      FROM information_schema.columns
      WHERE table_schema = 'core' AND table_name = 'card_topup_request' AND column_name = 'description'
  ) THEN
    ALTER TABLE core.card_topup_request ADD description character varying(300); 
  END IF;
END $$;
