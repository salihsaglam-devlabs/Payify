DO $$
BEGIN
  IF NOT EXISTS (
      SELECT column_name
               FROM information_schema.columns
               WHERE table_schema='core' AND table_name='account' AND column_name='is_address_confirmed'
    ) THEN
    ALTER TABLE core.account ADD is_address_confirmed bool DEFAULT false;
END IF;
END $$;
