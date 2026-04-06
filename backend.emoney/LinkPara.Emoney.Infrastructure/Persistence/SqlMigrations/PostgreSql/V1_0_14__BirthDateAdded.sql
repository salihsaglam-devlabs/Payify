DO $$
BEGIN
  IF NOT EXISTS (
      SELECT column_name
               FROM information_schema.columns
               WHERE table_schema='core' AND table_name='account' AND column_name='birth_date'
    ) THEN
      ALTER TABLE core.account ADD birth_date timestamp NOT NULL DEFAULT '-infinity'::timestamp without time zone;
END IF;
END $$;
