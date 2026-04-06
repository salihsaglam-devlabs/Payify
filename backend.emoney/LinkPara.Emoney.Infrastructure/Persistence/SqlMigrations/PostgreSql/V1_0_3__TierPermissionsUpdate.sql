DO $$
BEGIN
  IF EXISTS (
      SELECT column_name
               FROM information_schema.columns
               WHERE table_schema='limit' AND table_name='tier_level' AND column_name='daily_max_corporate_wallet_transfer_amount'
    ) THEN
ALTER TABLE "limit".tier_level DROP COLUMN daily_max_corporate_wallet_transfer_amount;
END IF;
END $$;
    
DO $$
BEGIN
  IF EXISTS (
      SELECT column_name
               FROM information_schema.columns
               WHERE table_schema='limit' AND table_name='tier_level' AND column_name='max_balance_corporate_wallet'
    ) THEN
ALTER TABLE "limit".tier_level DROP COLUMN max_balance_corporate_wallet;
END IF;
END $$;
    
DO $$
BEGIN
  IF EXISTS (
      SELECT column_name
               FROM information_schema.columns
               WHERE table_schema='limit' AND table_name='tier_level' AND column_name='monthly_max_corporate_wallet_transfer_amount'
    ) THEN
ALTER TABLE "limit".tier_level DROP COLUMN monthly_max_corporate_wallet_transfer_amount;
END IF;
END $$;
    
DO $$
BEGIN
  IF EXISTS (
      SELECT column_name
               FROM information_schema.columns
               WHERE table_schema='limit' AND table_name='tier_level' AND column_name='monthly_max_corporate_wallet_transfer_count'
    ) THEN
ALTER TABLE "limit".tier_level DROP COLUMN monthly_max_corporate_wallet_transfer_count;
END IF;
END $$;
    
DO $$
BEGIN
  IF EXISTS (
      SELECT column_name
               FROM information_schema.columns
               WHERE table_schema='limit' AND table_name='tier_level' AND column_name='daily_max_corporate_wallet_transfer_count'
    ) THEN
ALTER TABLE "limit".tier_level DROP COLUMN daily_max_corporate_wallet_transfer_count;
END IF;
END $$;
    
DO $$
BEGIN
  IF NOT EXISTS (
      SELECT column_name
               FROM information_schema.columns
               WHERE table_schema='limit' AND table_name='tier_level' AND column_name='monthly_max_distinct_iban_withdrawal_count'
    ) THEN
ALTER TABLE "limit".tier_level ADD monthly_max_distinct_iban_withdrawal_count int NOT NULL DEFAULT 0;
END IF;
END $$;
    
DO $$
BEGIN
  IF NOT EXISTS (
      SELECT column_name
               FROM information_schema.columns
               WHERE table_schema='limit' AND table_name='tier_level' AND column_name='daily_max_distinct_iban_withdrawal_count'
    ) THEN
ALTER TABLE "limit".tier_level ADD daily_max_distinct_iban_withdrawal_count int NOT NULL DEFAULT 0;
END IF;
END $$;
    
DO $$
BEGIN
  IF NOT EXISTS (
      SELECT column_name
               FROM information_schema.columns
               WHERE table_schema='limit' AND table_name='tier_level' AND column_name='distinct_iban_withdrawal_limit_check_enabled'
    ) THEN
ALTER TABLE "limit".tier_level ADD distinct_iban_withdrawal_limit_check_enabled boolean NOT NULL DEFAULT FALSE;
END IF;
END $$;
    
DO $$
BEGIN
  IF NOT EXISTS (
      SELECT column_name
               FROM information_schema.columns
               WHERE table_schema='limit' AND table_name='tier_level' AND column_name='own_iban_limit_enabled'
    ) THEN
ALTER TABLE "limit".tier_level ADD own_iban_limit_enabled boolean NOT NULL DEFAULT FALSE;
END IF;
END $$;

CREATE TABLE IF NOT EXISTS "limit".tier_level_upgrade_path (
                                                 id uuid NOT NULL,
                                                 tier_level character varying(50) NOT NULL,
                                                 iban_validation boolean NOT NULL,
                                                 identity_validation boolean NOT NULL,
                                                 digital_kyc_validation boolean NOT NULL,
                                                 next_tier character varying(50) NOT NULL,
                                                 create_date timestamp without time zone NOT NULL,
                                                 update_date timestamp without time zone NULL,
                                                 created_by character varying(50) NOT NULL,
                                                 last_modified_by character varying(50) NULL,
                                                 record_status character varying(50) NOT NULL,
                                                 CONSTRAINT pk_tier_level_upgrade_path PRIMARY KEY (id)
);

CREATE TABLE IF NOT EXISTS "limit".tier_permission (
                                         id uuid NOT NULL,
                                         tier_level character varying(50) NOT NULL,
                                         permission_type character varying(50) NOT NULL,
                                         is_enabled boolean NOT NULL,
                                         create_date timestamp without time zone NOT NULL,
                                         update_date timestamp without time zone NULL,
                                         created_by character varying(50) NOT NULL,
                                         last_modified_by character varying(50) NULL,
                                         record_status character varying(50) NOT NULL,
                                         CONSTRAINT pk_tier_permission PRIMARY KEY (id)
);

