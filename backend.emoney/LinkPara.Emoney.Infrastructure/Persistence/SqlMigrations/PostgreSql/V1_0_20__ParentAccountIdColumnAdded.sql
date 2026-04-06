DO $$
BEGIN
  IF NOT EXISTS (
      SELECT column_name
               FROM information_schema.columns
               WHERE table_schema='core' AND table_name='account' AND column_name='parent_account_id'
    ) THEN
    ALTER TABLE core.account ADD parent_account_id uuid DEFAULT '00000000-0000-0000-0000-000000000000'::uuid not null;
END IF;
END $$;
