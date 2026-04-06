DO $$
BEGIN
    -- merchant.vpos
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_schema = 'merchant'
          AND table_name = 'vpos'
          AND column_name = 'bkm_reference_number'
    ) THEN
        ALTER TABLE merchant.vpos ADD bkm_reference_number character varying(50);
    END IF;

    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_schema = 'merchant'
          AND table_name = 'vpos'
          AND column_name = 'service_provider_psp_merchant_id'
    ) THEN
        ALTER TABLE merchant.vpos ADD service_provider_psp_merchant_id character varying(100);
    END IF;

    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_schema = 'merchant'
          AND table_name = 'vpos'
          AND column_name = 'terminal_status'
    ) THEN
        ALTER TABLE merchant.vpos ADD terminal_status character varying(50) NOT NULL DEFAULT '';
    END IF;

    -- merchant.merchant
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_schema = 'merchant'
          AND table_name = 'merchant'
          AND column_name = 'hosting_trade_name'
    ) THEN
        ALTER TABLE merchant.merchant ADD hosting_trade_name character varying(150);
    END IF;

    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_schema = 'merchant'
          AND table_name = 'merchant'
          AND column_name = 'hosting_url'
    ) THEN
        ALTER TABLE merchant.merchant ADD hosting_url character varying(150);
    END IF;

    -- bank.api_key
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_schema = 'bank'
          AND table_name = 'api_key'
          AND column_name = 'is_pf_main_merchant_id'
    ) THEN
        ALTER TABLE bank.api_key ADD is_pf_main_merchant_id boolean NOT NULL DEFAULT FALSE;
    END IF;

    -- bank.acquire_bank
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_schema = 'bank'
          AND table_name = 'acquire_bank'
          AND column_name = 'payment_gw_tax_no'
    ) THEN
        ALTER TABLE bank.acquire_bank ADD payment_gw_tax_no character varying(11);
    END IF;

    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_schema = 'bank'
          AND table_name = 'acquire_bank'
          AND column_name = 'payment_gw_trade_name'
    ) THEN
        ALTER TABLE bank.acquire_bank ADD payment_gw_trade_name character varying(150);
    END IF;

    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_schema = 'bank'
          AND table_name = 'acquire_bank'
          AND column_name = 'payment_gw_url'
    ) THEN
        ALTER TABLE bank.acquire_bank ADD payment_gw_url character varying(150);
    END IF;
END $$;
