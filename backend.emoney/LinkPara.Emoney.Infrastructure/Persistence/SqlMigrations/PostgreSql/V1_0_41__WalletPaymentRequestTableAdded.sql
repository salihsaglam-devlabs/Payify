DO $$
BEGIN
    IF EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'fk_account_current_level_currency_currency_id') THEN
        ALTER TABLE "limit".account_current_level DROP CONSTRAINT fk_account_current_level_currency_currency_id;
    END IF;
 
    IF EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'fk_pricing_profile_bank_bank_id') THEN
        ALTER TABLE core.pricing_profile DROP CONSTRAINT fk_pricing_profile_bank_bank_id;
    END IF;
 
    IF EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'fk_pricing_profile_currency_currency_id') THEN
        ALTER TABLE core.pricing_profile DROP CONSTRAINT fk_pricing_profile_currency_currency_id;
    END IF;
 
    IF EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'fk_tier_level_currency_currency_id') THEN
        ALTER TABLE "limit".tier_level DROP CONSTRAINT fk_tier_level_currency_currency_id;
    END IF;
 
    IF EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'fk_transaction_currency_currency_id') THEN
        ALTER TABLE core.transaction DROP CONSTRAINT fk_transaction_currency_currency_id;
    END IF;
 
    IF EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'fk_transfer_order_currency_currency_id') THEN
        ALTER TABLE core.transfer_order DROP CONSTRAINT fk_transfer_order_currency_currency_id;
    END IF;
 
    IF EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'fk_wallet_currency_currency_id') THEN
        ALTER TABLE core.wallet DROP CONSTRAINT fk_wallet_currency_currency_id;
    END IF;
END $$;
 
 
-- CREATE TABLE IF NOT EXISTS
CREATE TABLE IF NOT EXISTS core.wallet_payment_request (
    id uuid NOT NULL,
    payment_reference_id character varying(50) NOT NULL,
    internal_transaction_id uuid NOT NULL,
    status character varying(50) NOT NULL,
    amount numeric(18,4) NOT NULL,
    currency_code character varying(3) NOT NULL,
    sender_wallet_no character varying(20) NOT NULL,
    sender_name character varying(150) NOT NULL,
    receiver_wallet_no character varying(20) NOT NULL,
    receiver_name character varying(150) NOT NULL,
    transaction_date timestamp without time zone NOT NULL,
    is_logged_in boolean NOT NULL DEFAULT FALSE,
    create_date timestamp without time zone NOT NULL,
    update_date timestamp without time zone,
    created_by character varying(50) NOT NULL,
    last_modified_by character varying(50),
    record_status character varying(50) NOT NULL,
    CONSTRAINT pk_wallet_payment_request PRIMARY KEY (id)
);
 
 
-- CREATE INDEX IF NOT EXISTS
CREATE INDEX IF NOT EXISTS ix_wallet_payment_request_payment_reference_id 
ON core.wallet_payment_request (payment_reference_id);
 
 
-- ADD CONSTRAINTS IF NOT EXISTS
DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'fk_account_current_level_currency_currency_code') THEN
        ALTER TABLE "limit".account_current_level 
        ADD CONSTRAINT fk_account_current_level_currency_currency_code FOREIGN KEY (currency_code) REFERENCES core.currency (code) ON DELETE RESTRICT;
    END IF;
 
    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'fk_pricing_profile_bank_bank_code') THEN
        ALTER TABLE core.pricing_profile 
        ADD CONSTRAINT fk_pricing_profile_bank_bank_code FOREIGN KEY (bank_code) REFERENCES core.bank (code) ON DELETE RESTRICT;
    END IF;
 
    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'fk_pricing_profile_currency_currency_code') THEN
        ALTER TABLE core.pricing_profile 
        ADD CONSTRAINT fk_pricing_profile_currency_currency_code FOREIGN KEY (currency_code) REFERENCES core.currency (code) ON DELETE RESTRICT;
    END IF;
 
    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'fk_tier_level_currency_currency_code') THEN
        ALTER TABLE "limit".tier_level 
        ADD CONSTRAINT fk_tier_level_currency_currency_code FOREIGN KEY (currency_code) REFERENCES core.currency (code) ON DELETE RESTRICT;
    END IF;
 
    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'fk_transaction_currency_currency_code') THEN
        ALTER TABLE core.transaction 
        ADD CONSTRAINT fk_transaction_currency_currency_code FOREIGN KEY (currency_code) REFERENCES core.currency (code) ON DELETE RESTRICT;
    END IF;
 
    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'fk_transfer_order_currency_currency_code') THEN
        ALTER TABLE core.transfer_order 
        ADD CONSTRAINT fk_transfer_order_currency_currency_code FOREIGN KEY (currency_code) REFERENCES core.currency (code) ON DELETE RESTRICT;
    END IF;
 
    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'fk_wallet_currency_currency_code') THEN
        ALTER TABLE core.wallet 
        ADD CONSTRAINT fk_wallet_currency_currency_code FOREIGN KEY (currency_code) REFERENCES core.currency (code) ON DELETE RESTRICT;
    END IF;
END $$;