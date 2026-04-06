DO $$
BEGIN
  IF NOT EXISTS (
      SELECT column_name
               FROM information_schema.columns
               WHERE table_schema='limit' AND table_name='tier_level' AND column_name='daily_max_distinct_other_iban_withdrawal_count'
    ) THEN
ALTER TABLE "limit".tier_level ADD daily_max_distinct_other_iban_withdrawal_count integer NOT NULL DEFAULT 0;
END IF;
END $$;
    
DO $$
BEGIN
  IF NOT EXISTS (
      SELECT column_name
               FROM information_schema.columns
               WHERE table_schema='limit' AND table_name='tier_level' AND column_name='daily_max_other_iban_withdrawal_amount'
    ) THEN
ALTER TABLE "limit".tier_level ADD daily_max_other_iban_withdrawal_amount numeric(18,4) NOT NULL DEFAULT 0.0;
END IF;
END $$;

DO $$
BEGIN
  IF NOT EXISTS (
      SELECT column_name
               FROM information_schema.columns
               WHERE table_schema='limit' AND table_name='tier_level' AND column_name='monthly_max_distinct_other_iban_withdrawal_count'
    ) THEN
ALTER TABLE "limit".tier_level ADD monthly_max_distinct_other_iban_withdrawal_count integer NOT NULL DEFAULT 0;
END IF;
END $$;

DO $$
BEGIN
  IF NOT EXISTS (
      SELECT column_name
               FROM information_schema.columns
               WHERE table_schema='limit' AND table_name='tier_level' AND column_name='monthly_max_other_iban_withdrawal_amount'
    ) THEN
ALTER TABLE "limit".tier_level ADD monthly_max_other_iban_withdrawal_amount numeric(18,4) NOT NULL DEFAULT 0.0;
END IF;
END $$;

DO $$
BEGIN
  IF NOT EXISTS (
      SELECT column_name
               FROM information_schema.columns
               WHERE table_schema='limit' AND table_name='account_current_level' AND column_name='daily_distinct_other_iban_withdrawal_count'
    ) THEN
ALTER TABLE "limit".account_current_level ADD daily_distinct_other_iban_withdrawal_count integer NOT NULL DEFAULT 0;
END IF;
END $$;

DO $$
BEGIN
  IF NOT EXISTS (
      SELECT column_name
               FROM information_schema.columns
               WHERE table_schema='limit' AND table_name='account_current_level' AND column_name='daily_other_iban_withdrawal_amount'
    ) THEN
ALTER TABLE "limit".account_current_level ADD daily_other_iban_withdrawal_amount numeric(18,4) NOT NULL DEFAULT 0.0;
END IF;
END $$;

DO $$
BEGIN
  IF NOT EXISTS (
      SELECT column_name
               FROM information_schema.columns
               WHERE table_schema='limit' AND table_name='account_current_level' AND column_name='monthly_distinct_other_iban_withdrawal_count'
    ) THEN
ALTER TABLE "limit".account_current_level ADD monthly_distinct_other_iban_withdrawal_count integer NOT NULL DEFAULT 0;
END IF;
END $$;

DO $$
BEGIN
  IF NOT EXISTS (
      SELECT column_name
               FROM information_schema.columns
               WHERE table_schema='limit' AND table_name='account_current_level' AND column_name='monthly_other_iban_withdrawal_amount'
    ) THEN
ALTER TABLE "limit".account_current_level ADD monthly_other_iban_withdrawal_amount numeric(18,4) NOT NULL DEFAULT 0.0;
END IF;
END $$;
