DO $$
BEGIN
CREATE UNIQUE INDEX if not exists ix_account_identity_number ON core.account (identity_number);
END $$;

DO $$
BEGIN
  IF EXISTS (
      SELECT column_name
               FROM information_schema.columns
               WHERE table_schema='core' AND table_name='account' AND column_name='identity_number'
    ) THEN
ALTER TABLE core.account ALTER COLUMN identity_number TYPE character varying(50);
END IF;
END $$;
