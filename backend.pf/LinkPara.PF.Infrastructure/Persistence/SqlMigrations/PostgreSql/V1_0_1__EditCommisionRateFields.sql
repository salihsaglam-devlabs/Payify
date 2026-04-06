DO $$
BEGIN
    IF EXISTS (
        SELECT 1 FROM information_schema.table_constraints 
        WHERE constraint_schema = 'bank' 
          AND table_name = 'acquire_bank' 
          AND constraint_name = 'fk_acquire_bank_bank_bank_id'
    ) THEN
        ALTER TABLE bank.acquire_bank DROP CONSTRAINT fk_acquire_bank_bank_bank_id;
    END IF;

    IF EXISTS (
        SELECT 1 FROM information_schema.table_constraints 
        WHERE constraint_schema = 'merchant' 
          AND table_name = 'blockage_detail' 
          AND constraint_name = 'fk_blockage_detail_blockage_merchant_blockage_id'
    ) THEN
        ALTER TABLE merchant.blockage_detail DROP CONSTRAINT fk_blockage_detail_blockage_merchant_blockage_id;
    END IF;

    IF EXISTS (
        SELECT 1 FROM information_schema.table_constraints 
        WHERE constraint_schema = 'core' 
          AND table_name = 'cost_profile' 
          AND constraint_name = 'fk_cost_profile_currency_currency_id'
    ) THEN
        ALTER TABLE core.cost_profile DROP CONSTRAINT fk_cost_profile_currency_currency_id;
    END IF;

    IF EXISTS (
        SELECT 1 FROM information_schema.table_constraints 
        WHERE constraint_schema = 'merchant' 
          AND table_name = 'merchant' 
          AND constraint_name = 'fk_merchant_mcc_mcc_id'
    ) THEN
        ALTER TABLE merchant.merchant DROP CONSTRAINT fk_merchant_mcc_mcc_id;
    END IF;

    IF EXISTS (
        SELECT 1 FROM information_schema.table_constraints 
        WHERE constraint_schema = 'core' 
          AND table_name = 'pricing_profile' 
          AND constraint_name = 'fk_pricing_profile_currency_currency_id'
    ) THEN
        ALTER TABLE core.pricing_profile DROP CONSTRAINT fk_pricing_profile_currency_currency_id;
    END IF;
END$$;

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.table_constraints 
        WHERE constraint_schema = 'bank' 
          AND table_name = 'acquire_bank' 
          AND constraint_name = 'fk_acquire_bank_bank_bank_code'
    ) THEN
        ALTER TABLE bank.acquire_bank 
        ADD CONSTRAINT fk_acquire_bank_bank_bank_code 
        FOREIGN KEY (bank_code) REFERENCES bank.bank (code) ON DELETE RESTRICT;
    END IF;

    IF NOT EXISTS (
        SELECT 1 FROM information_schema.table_constraints 
        WHERE constraint_schema = 'merchant' 
          AND table_name = 'blockage_detail' 
          AND constraint_name = 'fk_blockage_detail_blockage_merchant_blockage_id'
    ) THEN
        ALTER TABLE merchant.blockage_detail 
        ADD CONSTRAINT fk_blockage_detail_blockage_merchant_blockage_id 
        FOREIGN KEY (merchant_blockage_id) REFERENCES merchant.blockage (id) ON DELETE RESTRICT;
    END IF;

    IF NOT EXISTS (
        SELECT 1 FROM information_schema.table_constraints 
        WHERE constraint_schema = 'core' 
          AND table_name = 'cost_profile' 
          AND constraint_name = 'fk_cost_profile_currency_currency_code'
    ) THEN
        ALTER TABLE core.cost_profile 
        ADD CONSTRAINT fk_cost_profile_currency_currency_code 
        FOREIGN KEY (currency_code) REFERENCES core.currency (code) ON DELETE RESTRICT;
    END IF;

    IF NOT EXISTS (
        SELECT 1 FROM information_schema.table_constraints 
        WHERE constraint_schema = 'merchant' 
          AND table_name = 'merchant' 
          AND constraint_name = 'fk_merchant_mcc_mcc_code'
    ) THEN
        ALTER TABLE merchant.merchant 
        ADD CONSTRAINT fk_merchant_mcc_mcc_code 
        FOREIGN KEY (mcc_code) REFERENCES merchant.mcc (code) ON DELETE RESTRICT;
    END IF;

    IF NOT EXISTS (
        SELECT 1 FROM information_schema.table_constraints 
        WHERE constraint_schema = 'core' 
          AND table_name = 'pricing_profile' 
          AND constraint_name = 'fk_pricing_profile_currency_currency_code'
    ) THEN
        ALTER TABLE core.pricing_profile 
        ADD CONSTRAINT fk_pricing_profile_currency_currency_code 
        FOREIGN KEY (currency_code) REFERENCES core.currency (code) ON DELETE RESTRICT;
    END IF;
END$$;

DO $$
BEGIN
    -- posting.transaction.transaction_start_date
    IF EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_schema = 'posting'
          AND table_name = 'transaction'
          AND column_name = 'transaction_start_date'
          AND data_type != 'date'
    ) THEN
        ALTER TABLE posting.transaction 
        ALTER COLUMN transaction_start_date TYPE date;
    END IF;

    -- posting.transaction.transaction_end_date
    IF EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_schema = 'posting'
          AND table_name = 'transaction'
          AND column_name = 'transaction_end_date'
          AND data_type != 'date'
    ) THEN
        ALTER TABLE posting.transaction 
        ALTER COLUMN transaction_end_date TYPE date;
    END IF;

    -- posting.posting_additional_transaction.transaction_start_date
    IF EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_schema = 'posting'
          AND table_name = 'posting_additional_transaction'
          AND column_name = 'transaction_start_date'
          AND data_type != 'date'
    ) THEN
        ALTER TABLE posting.posting_additional_transaction 
        ALTER COLUMN transaction_start_date TYPE date;
    END IF;

    -- posting.posting_additional_transaction.transaction_end_date
    IF EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_schema = 'posting'
          AND table_name = 'posting_additional_transaction'
          AND column_name = 'transaction_end_date'
          AND data_type != 'date'
    ) THEN
        ALTER TABLE posting.posting_additional_transaction 
        ALTER COLUMN transaction_end_date TYPE date;
    END IF;

    -- core.cost_profile.service_commission
    IF EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_schema = 'core'
          AND table_name = 'cost_profile'
          AND column_name = 'service_commission'
          AND (data_type != 'numeric' OR numeric_precision != 5 OR numeric_scale != 3)
    ) THEN
        ALTER TABLE core.cost_profile 
        ALTER COLUMN service_commission TYPE numeric(5,3);
    END IF;

    -- core.cost_profile.point_commission
    IF EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_schema = 'core'
          AND table_name = 'cost_profile'
          AND column_name = 'point_commission'
          AND (data_type != 'numeric' OR numeric_precision != 5 OR numeric_scale != 3)
    ) THEN
        ALTER TABLE core.cost_profile 
        ALTER COLUMN point_commission TYPE numeric(5,3);
    END IF;

     IF EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_schema = 'core'
          AND table_name = 'cost_profile_item'
          AND column_name = 'commission_rate'
          AND (data_type != 'numeric' OR numeric_precision != 5 OR numeric_scale != 3)
    ) THEN
        ALTER TABLE core.cost_profile_item 
        ALTER COLUMN commission_rate TYPE numeric(5,3);
    END IF;

     IF EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_schema = 'merchant'
          AND table_name = 'transaction'
          AND column_name = 'bank_commission_rate'
          AND (data_type != 'numeric' OR numeric_precision != 5 OR numeric_scale != 3)
    ) THEN
        ALTER TABLE merchant."transaction"
        ALTER COLUMN bank_commission_rate TYPE numeric(5,3);
    END IF;
END$$;
