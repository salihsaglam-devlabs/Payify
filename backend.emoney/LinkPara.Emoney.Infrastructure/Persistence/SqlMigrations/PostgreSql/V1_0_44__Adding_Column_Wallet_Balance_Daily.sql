
DO $$
BEGIN
  IF NOT EXISTS (
      SELECT column_name
      FROM information_schema.columns
      WHERE table_schema = 'core' AND table_name = 'wallet_balance_daily' AND column_name = 'difference'
  ) THEN
    ALTER TABLE core.wallet_balance_daily ADD difference numeric(18,2);
  END IF;
END $$;
