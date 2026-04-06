
DO $$
BEGIN
  IF NOT EXISTS (
      SELECT column_name
      FROM information_schema.columns
      WHERE table_schema = 'core' AND table_name = 'account' AND column_name = 'declaration_status'
  ) THEN
    ALTER TABLE core.account ADD declaration_status character varying(20) NOT NULL DEFAULT 'None';
  END IF;
END $$;
