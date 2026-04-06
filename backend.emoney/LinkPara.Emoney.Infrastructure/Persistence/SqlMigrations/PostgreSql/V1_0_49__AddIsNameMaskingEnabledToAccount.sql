DO $$
BEGIN
  IF NOT EXISTS (
      SELECT column_name
               FROM information_schema.columns
               WHERE table_schema='core' AND table_name='account' AND column_name='is_name_masking_enabled'
    ) THEN
ALTER TABLE core.account ADD is_name_masking_enabled boolean NOT NULL DEFAULT FALSE ;
END IF;
END $$;
