DO $$
BEGIN
  IF NOT EXISTS (
      SELECT column_name
               FROM information_schema.columns
               WHERE table_schema='core' AND table_name='account' AND column_name='reopening_date'
    ) THEN
ALTER TABLE core.account ADD reopening_date timestamp without time zone NOT NULL DEFAULT TIMESTAMP '-infinity';
END IF;
END $$;
