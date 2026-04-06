DO $$
BEGIN
  IF NOT EXISTS (
      SELECT column_name
      FROM information_schema.columns
      WHERE table_schema = 'core' 
        AND table_name = 'saved_account' 
        AND column_name = 'receiver_name'
  ) THEN
    ALTER TABLE core.saved_account 
      ADD receiver_name character varying(200);
  END IF;
END $$;
