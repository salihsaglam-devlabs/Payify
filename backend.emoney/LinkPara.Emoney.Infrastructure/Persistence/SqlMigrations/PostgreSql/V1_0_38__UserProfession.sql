DO $$
BEGIN
  IF NOT EXISTS (
      SELECT column_name
               FROM information_schema.columns
               WHERE table_schema='core' AND table_name='account' AND column_name='profession'
    ) THEN
ALTER TABLE core.account ADD profession character varying(150) NULL;
END IF;
END $$;