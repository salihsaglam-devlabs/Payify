DO $$
BEGIN
  IF EXISTS (
      SELECT indexname FROM pg_indexes
      WHERE schemaname = 'core' AND tablename = 'wallet' AND indexname = 'ix_wallet_user_id'
    ) THEN
        DROP INDEX core.ix_wallet_user_id;
END IF;
END $$;

DO $$
BEGIN
  IF EXISTS (
      SELECT column_name
               FROM information_schema.columns
               WHERE table_schema='core' AND table_name='wallet' AND column_name='user_id'
    ) THEN
        ALTER TABLE core.wallet DROP COLUMN user_id;    
END IF;
END $$;

