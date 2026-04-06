DO $$
BEGIN
  IF NOT EXISTS (
      SELECT column_name
               FROM information_schema.columns
               WHERE table_schema='limit' AND table_name='account_current_level' AND column_name='daily_max_on_us_payment_amount'
    ) THEN
    ALTER TABLE "limit".account_current_level ADD daily_max_on_us_payment_amount numeric(18,4) NOT NULL DEFAULT 0.0;
END IF;
END $$;


DO $$
BEGIN
  IF NOT EXISTS (
      SELECT column_name
               FROM information_schema.columns
               WHERE table_schema='limit' AND table_name='account_current_level' AND column_name='monthly_max_on_us_payment_amount'
    ) THEN
    ALTER TABLE "limit".account_current_level ADD monthly_max_on_us_payment_amount numeric(18,4) NOT NULL DEFAULT 0.0;
END IF;
END $$;
    
DO $$
BEGIN
  IF NOT EXISTS (
      SELECT column_name
               FROM information_schema.columns
               WHERE table_schema='limit' AND table_name='account_current_level' AND column_name='daily_max_on_us_payment_count'
    ) THEN
     ALTER TABLE "limit".account_current_level ADD daily_max_on_us_payment_count int NOT NULL DEFAULT 0;
END IF;
END $$;

DO $$
BEGIN
  IF NOT EXISTS (
      SELECT column_name
               FROM information_schema.columns
               WHERE table_schema='limit' AND table_name='account_current_level' AND column_name='monthly_max_on_us_payment_count'
    ) THEN
     ALTER TABLE "limit".account_current_level ADD monthly_max_on_us_payment_count int NOT NULL DEFAULT 0;
END IF;
END $$;




DO $$
BEGIN
  IF NOT EXISTS (
      SELECT column_name
               FROM information_schema.columns
               WHERE table_schema='limit' AND table_name='tier_level' AND column_name='max_on_us_payment_limit_enabled'
    ) THEN
    ALTER TABLE "limit".tier_level ADD max_on_us_payment_limit_enabled boolean NOT NULL DEFAULT FALSE;
END IF;
END $$;



DO $$
BEGIN
  IF NOT EXISTS (
      SELECT column_name
               FROM information_schema.columns
               WHERE table_schema='limit' AND table_name='tier_level' AND column_name='daily_max_on_us_payment_amount'
    ) THEN
    ALTER TABLE "limit".tier_level ADD daily_max_on_us_payment_amount numeric(18,4) NOT NULL DEFAULT 0.0;
END IF;
END $$;


DO $$
BEGIN
  IF NOT EXISTS (
      SELECT column_name
               FROM information_schema.columns
               WHERE table_schema='limit' AND table_name='tier_level' AND column_name='monthly_max_on_us_payment_amount'
    ) THEN
    ALTER TABLE "limit".tier_level ADD monthly_max_on_us_payment_amount numeric(18,4) NOT NULL DEFAULT 0.0;
END IF;
END $$;
    
DO $$
BEGIN
  IF NOT EXISTS (
      SELECT column_name
               FROM information_schema.columns
               WHERE table_schema='limit' AND table_name='tier_level' AND column_name='daily_max_on_us_payment_count'
    ) THEN
     ALTER TABLE "limit".tier_level ADD daily_max_on_us_payment_count int NOT NULL DEFAULT 0;
END IF;
END $$;

DO $$
BEGIN
  IF NOT EXISTS (
      SELECT column_name
               FROM information_schema.columns
               WHERE table_schema='limit' AND table_name='tier_level' AND column_name='monthly_max_on_us_payment_count'
    ) THEN
     ALTER TABLE "limit".tier_level ADD monthly_max_on_us_payment_count int NOT NULL DEFAULT 0;
END IF;
END $$;