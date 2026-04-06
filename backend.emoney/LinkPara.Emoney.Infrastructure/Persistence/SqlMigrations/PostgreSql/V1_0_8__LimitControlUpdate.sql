DO $$
BEGIN
  IF EXISTS (
      SELECT column_name
               FROM information_schema.columns
               WHERE table_schema='limit' AND table_name='tier_level_upgrade_path' AND column_name='digital_kyc_validation'
    ) THEN
ALTER TABLE "limit".tier_level_upgrade_path DROP COLUMN digital_kyc_validation;
END IF;
END $$;
    
DO $$
BEGIN
  IF EXISTS (
      SELECT column_name
               FROM information_schema.columns
               WHERE table_schema='limit' AND table_name='tier_level_upgrade_path' AND column_name='iban_validation'
    ) THEN
ALTER TABLE "limit".tier_level_upgrade_path DROP COLUMN iban_validation;
END IF;
END $$;
    
DO $$
BEGIN
  IF EXISTS (
      SELECT column_name
               FROM information_schema.columns
               WHERE table_schema='limit' AND table_name='tier_level_upgrade_path' AND column_name='identity_validation'
    ) THEN
ALTER TABLE "limit".tier_level_upgrade_path DROP COLUMN identity_validation;
END IF;
END $$;  

DO $$
BEGIN
  IF NOT EXISTS (
      SELECT column_name
               FROM information_schema.columns
               WHERE table_schema='limit' AND table_name='tier_level_upgrade_path' AND column_name='validation_type'
    ) THEN
ALTER TABLE "limit".tier_level_upgrade_path ADD validation_type character varying(50) NOT NULL DEFAULT '';
END IF;
END $$;

DO $$
BEGIN
  IF EXISTS (
      SELECT column_name
               FROM information_schema.columns
               WHERE table_schema='limit' AND table_name='account_current_level' AND column_name='monthly_corporate_wallet_transfer_count'
    ) THEN
ALTER TABLE "limit".account_current_level RENAME COLUMN monthly_corporate_wallet_transfer_count TO monthly_own_iban_withdrawal_count;
END IF;
END $$;
    
DO $$
BEGIN
  IF EXISTS (
      SELECT column_name
               FROM information_schema.columns
               WHERE table_schema='limit' AND table_name='account_current_level' AND column_name='daily_corporate_wallet_transfer_count'
    ) THEN
ALTER TABLE "limit".account_current_level RENAME COLUMN daily_corporate_wallet_transfer_count TO monthly_other_iban_withdrawal_count;
END IF;
END $$;
    
DO $$
BEGIN
  IF EXISTS (
      SELECT column_name
               FROM information_schema.columns
               WHERE table_schema='limit' AND table_name='account_current_level' AND column_name='daily_corporate_wallet_transfer_amount'
    ) THEN
ALTER TABLE "limit".account_current_level DROP COLUMN daily_corporate_wallet_transfer_amount;
END IF;
END $$;
    
DO $$
BEGIN
  IF EXISTS (
      SELECT column_name
               FROM information_schema.columns
               WHERE table_schema='limit' AND table_name='account_current_level' AND column_name='monthly_corporate_wallet_transfer_amount'
    ) THEN
ALTER TABLE "limit".account_current_level DROP COLUMN monthly_corporate_wallet_transfer_amount;
END IF;
END $$;

DO $$
BEGIN
  IF NOT EXISTS (
      SELECT column_name
               FROM information_schema.columns
               WHERE table_schema='limit' AND table_name='account_current_level' AND column_name='daily_other_iban_withdrawal_count'
    ) THEN
ALTER TABLE "limit".account_current_level ADD daily_other_iban_withdrawal_count integer NOT NULL DEFAULT 0;
END IF;
END $$;

DO $$
BEGIN
  IF NOT EXISTS (
      SELECT column_name
               FROM information_schema.columns
               WHERE table_schema='limit' AND table_name='account_current_level' AND column_name='daily_own_iban_withdrawal_count'
    ) THEN
ALTER TABLE "limit".account_current_level ADD daily_own_iban_withdrawal_count integer NOT NULL DEFAULT 0;
END IF;
END $$;

DO $$
BEGIN
  IF NOT EXISTS (
      SELECT column_name
               FROM information_schema.columns
               WHERE table_schema='core' AND table_name='withdraw_request' AND column_name='is_receiver_iban_owned'
    ) THEN
ALTER TABLE core.withdraw_request ADD is_receiver_iban_owned boolean NOT NULL DEFAULT FALSE;
END IF;
END $$;

DO $$
BEGIN
  IF EXISTS (
      SELECT column_name
               FROM information_schema.columns
               WHERE table_schema='limit' AND table_name='tier_level' AND column_name='own_iban_limit_enabled'
    ) THEN
ALTER TABLE "limit".tier_level RENAME COLUMN own_iban_limit_enabled TO max_withdrawal_limit_enabled;
END IF;
END $$;

DO $$
BEGIN
  IF EXISTS (
      SELECT column_name
               FROM information_schema.columns
               WHERE table_schema='limit' AND table_name='tier_level' AND column_name='monthly_max_distinct_iban_withdrawal_count'
    ) THEN
ALTER TABLE "limit".tier_level RENAME COLUMN monthly_max_distinct_iban_withdrawal_count TO monthly_max_own_iban_withdrawal_count;
END IF;
END $$;

DO $$
BEGIN
  IF EXISTS (
      SELECT column_name
               FROM information_schema.columns
               WHERE table_schema='limit' AND table_name='tier_level' AND column_name='distinct_iban_withdrawal_limit_check_enabled'
    ) THEN
ALTER TABLE "limit".tier_level RENAME COLUMN distinct_iban_withdrawal_limit_check_enabled TO max_own_iban_withdrawal_limit_enabled;
END IF;
END $$;

DO $$
BEGIN
  IF EXISTS (
      SELECT column_name
               FROM information_schema.columns
               WHERE table_schema='limit' AND table_name='tier_level' AND column_name='daily_max_distinct_iban_withdrawal_count'
    ) THEN
ALTER TABLE "limit".tier_level RENAME COLUMN daily_max_distinct_iban_withdrawal_count TO monthly_max_other_iban_withdrawal_count;
END IF;
END $$;

DO $$
BEGIN
  IF NOT EXISTS (
      SELECT column_name
               FROM information_schema.columns
               WHERE table_schema='limit' AND table_name='tier_level' AND column_name='daily_max_other_iban_withdrawal_count'
    ) THEN
ALTER TABLE "limit".tier_level ADD daily_max_other_iban_withdrawal_count integer NOT NULL DEFAULT 0;
END IF;
END $$;

DO $$
BEGIN
  IF NOT EXISTS (
      SELECT column_name
               FROM information_schema.columns
               WHERE table_schema='limit' AND table_name='tier_level' AND column_name='daily_max_own_iban_withdrawal_count'
    ) THEN
ALTER TABLE "limit".tier_level ADD daily_max_own_iban_withdrawal_count integer NOT NULL DEFAULT 0;
END IF;
END $$;

DO $$
BEGIN
  IF NOT EXISTS (
      SELECT column_name
               FROM information_schema.columns
               WHERE table_schema='limit' AND table_name='tier_level' AND column_name='max_balance_limit_enabled'
    ) THEN
ALTER TABLE "limit".tier_level ADD max_balance_limit_enabled boolean NOT NULL DEFAULT FALSE;
END IF;
END $$;

DO $$
BEGIN
  IF NOT EXISTS (
      SELECT column_name
               FROM information_schema.columns
               WHERE table_schema='limit' AND table_name='tier_level' AND column_name='max_deposit_limit_enabled'
    ) THEN
ALTER TABLE "limit".tier_level ADD max_deposit_limit_enabled boolean NOT NULL DEFAULT FALSE;
END IF;
END $$;

DO $$
BEGIN
  IF NOT EXISTS (
      SELECT column_name
               FROM information_schema.columns
               WHERE table_schema='limit' AND table_name='tier_level' AND column_name='max_internal_transfer_limit_enabled'
    ) THEN
ALTER TABLE "limit".tier_level ADD max_internal_transfer_limit_enabled boolean NOT NULL DEFAULT FALSE;
END IF;
END $$;

DO $$
BEGIN
  IF NOT EXISTS (
      SELECT column_name
               FROM information_schema.columns
               WHERE table_schema='limit' AND table_name='tier_level' AND column_name='max_international_transfer_limit_enabled'
    ) THEN
ALTER TABLE "limit".tier_level ADD max_international_transfer_limit_enabled boolean NOT NULL DEFAULT FALSE;
END IF;
END $$;

DO $$
BEGIN
  IF NOT EXISTS (
      SELECT column_name
               FROM information_schema.columns
               WHERE table_schema='limit' AND table_name='tier_level' AND column_name='max_other_iban_withdrawal_limit_enabled'
    ) THEN
ALTER TABLE "limit".tier_level ADD max_other_iban_withdrawal_limit_enabled boolean NOT NULL DEFAULT FALSE;
END IF;
END $$;
