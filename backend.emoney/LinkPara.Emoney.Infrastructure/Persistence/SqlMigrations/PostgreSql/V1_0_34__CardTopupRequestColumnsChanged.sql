DO $$
BEGIN
    IF EXISTS (SELECT 1 FROM information_schema.columns
               WHERE table_schema = 'core' AND table_name = 'card_topup_request' AND column_name = 'commission_total'
               AND data_type <> 'numeric(18,4)') THEN
        ALTER TABLE core.card_topup_request ALTER COLUMN commission_total TYPE numeric(18,4);
    END IF;
    IF EXISTS (SELECT 1 FROM information_schema.columns
               WHERE table_schema = 'core' AND table_name = 'card_topup_request' AND column_name = 'commission_rate'
               AND data_type <> 'numeric(4,2)') THEN
        ALTER TABLE core.card_topup_request ALTER COLUMN commission_rate TYPE numeric(4,2);
    END IF;
    IF EXISTS (SELECT 1 FROM information_schema.columns
               WHERE table_schema = 'core' AND table_name = 'card_topup_request' AND column_name = 'bsmv_total'
               AND data_type <> 'numeric(18,4)') THEN
        ALTER TABLE core.card_topup_request ALTER COLUMN bsmv_total TYPE numeric(18,4);
    END IF;
    IF EXISTS (SELECT 1 FROM information_schema.columns
               WHERE table_schema = 'core' AND table_name = 'card_topup_request' AND column_name = 'amount'
               AND data_type <> 'numeric(18,4)') THEN
        ALTER TABLE core.card_topup_request ALTER COLUMN amount TYPE numeric(18,4);
    END IF;
    IF EXISTS (SELECT 1 FROM information_schema.columns
               WHERE table_schema = 'core' AND table_name = 'card_topup_request' AND column_name = 'wallet_number'
               AND character_maximum_length <> 10) THEN
        ALTER TABLE core.card_topup_request ALTER COLUMN wallet_number TYPE character varying(10);
    END IF;
END $$;
 
DO $$
BEGIN
    IF EXISTS (SELECT 1 FROM information_schema.columns
               WHERE table_schema = 'core' AND table_name = 'card_topup_request' AND column_name = 'card_brand'
               AND is_nullable = 'YES') then
		UPDATE core.card_topup_request SET card_brand = '' WHERE card_brand IS NULL;  
        ALTER TABLE core.card_topup_request ALTER COLUMN card_brand SET NOT NULL;
    END IF;
    IF EXISTS (SELECT 1 FROM information_schema.columns
                   WHERE table_schema = 'core' AND table_name = 'card_topup_request' AND column_name = 'card_brand'
                   AND column_default is null) then                                      
        ALTER TABLE core.card_topup_request ALTER COLUMN card_brand SET DEFAULT '';
    END IF;
    IF EXISTS (SELECT 1 FROM information_schema.columns
               WHERE table_schema = 'core' AND table_name = 'card_topup_request' AND column_name = 'card_type'
               AND is_nullable = 'YES') then
		UPDATE core.card_topup_request SET card_type = '' WHERE card_type IS NULL;  
        ALTER TABLE core.card_topup_request ALTER COLUMN card_type SET NOT NULL;
    END IF;
    IF EXISTS (SELECT 1 FROM information_schema.columns
                   WHERE table_schema = 'core' AND table_name = 'card_topup_request' AND column_name = 'card_type'
                   AND column_default is null) then
        ALTER TABLE core.card_topup_request ALTER COLUMN card_type SET DEFAULT '';
    END IF;
    IF EXISTS (SELECT 1 FROM information_schema.columns
               WHERE table_schema = 'core' AND table_name = 'card_topup_request' AND column_name = 'commission_total'
               AND is_nullable = 'YES') THEN
        ALTER TABLE core.card_topup_request ALTER COLUMN commission_total SET NOT null;
    END IF;
      IF EXISTS (SELECT 1 FROM information_schema.columns
               WHERE table_schema = 'core' AND table_name = 'card_topup_request' AND column_name = 'commission_total'
               AND column_default is null) THEN
        ALTER TABLE core.card_topup_request ALTER COLUMN commission_total SET DEFAULT 0.0;
    END IF;
    IF EXISTS (SELECT 1 FROM information_schema.columns
               WHERE table_schema = 'core' AND table_name = 'card_topup_request' AND column_name = 'commission_rate'
               AND is_nullable = 'YES') THEN
        ALTER TABLE core.card_topup_request ALTER COLUMN commission_rate SET NOT null;
    END IF;
    IF EXISTS (SELECT 1 FROM information_schema.columns
               WHERE table_schema = 'core' AND table_name = 'card_topup_request' AND column_name = 'fee'
               AND is_nullable = 'YES') THEN
        ALTER TABLE core.card_topup_request ALTER COLUMN fee SET NOT null;
    END IF;
    IF EXISTS (SELECT 1 FROM information_schema.columns
               WHERE table_schema = 'core' AND table_name = 'card_topup_request' AND column_name = 'fee'
               AND column_default is null) THEN
        ALTER TABLE core.card_topup_request ALTER COLUMN fee SET DEFAULT 0.0;
    END IF;
    IF EXISTS (SELECT 1 FROM information_schema.columns
                   WHERE table_schema = 'core' AND table_name = 'card_topup_request' AND column_name = 'payment_provider_type'
                   AND is_nullable = 'YES') then
		UPDATE core.card_topup_request SET payment_provider_type = '' WHERE payment_provider_type IS NULL;                      
    	ALTER TABLE core.card_topup_request ALTER COLUMN payment_provider_type SET NOT null;
    END IF;
    IF EXISTS (SELECT 1 FROM information_schema.columns
                   WHERE table_schema = 'core' AND table_name = 'card_topup_request' AND column_name = 'payment_provider_type'
                   AND column_default is null) THEN
        ALTER TABLE core.card_topup_request ALTER COLUMN payment_provider_type SET DEFAULT '';
    END IF;
    IF EXISTS (SELECT 1 FROM information_schema.columns
                   WHERE table_schema = 'core' AND table_name = 'card_topup_request' AND column_name = 'secure3d_type'
                   AND is_nullable = 'YES') then
        UPDATE core.card_topup_request SET secure3d_type = '' WHERE secure3d_type IS NULL;               
        ALTER TABLE core.card_topup_request ALTER COLUMN secure3d_type SET NOT null;
    END IF;
   IF EXISTS (SELECT 1 FROM information_schema.columns
                   WHERE table_schema = 'core' AND table_name = 'card_topup_request' AND column_name = 'secure3d_type'
                   AND column_default is null) THEN
        ALTER TABLE core.card_topup_request ALTER COLUMN secure3d_type SET DEFAULT '';
    END IF;
   IF EXISTS (SELECT 1 FROM information_schema.columns
                   WHERE table_schema = 'core' AND table_name = 'card_topup_request' AND column_name = 'status'
                   AND is_nullable = 'YES') THEN
        ALTER TABLE core.card_topup_request ALTER COLUMN status SET NOT null;
    END IF;
   IF EXISTS (SELECT 1 FROM information_schema.columns
                   WHERE table_schema = 'core' AND table_name = 'card_topup_request' AND column_name = 'wallet_id'
                   AND is_nullable = 'YES') THEN
        ALTER TABLE core.card_topup_request ALTER COLUMN wallet_id SET NOT null;
    END IF;
   IF EXISTS (SELECT 1 FROM information_schema.columns
               WHERE table_schema = 'core' AND table_name = 'card_topup_request' AND column_name = 'amount'
               AND is_nullable = 'YES') THEN
        ALTER TABLE core.card_topup_request ALTER COLUMN amount SET NOT null;
    END IF;
    IF EXISTS (SELECT 1 FROM information_schema.columns
               WHERE table_schema = 'core' AND table_name = 'card_topup_request' AND column_name = 'authentication_method'
               AND is_nullable = 'YES') then
		UPDATE core.card_topup_request SET authentication_method = '' WHERE authentication_method IS NULL;               
        ALTER TABLE core.card_topup_request ALTER COLUMN authentication_method SET NOT NULL;
    END IF;
   IF EXISTS (SELECT 1 FROM information_schema.columns
                   WHERE table_schema = 'core' AND table_name = 'card_topup_request' AND column_name = 'authentication_method'
                   AND column_default is null) THEN
        ALTER TABLE core.card_topup_request ALTER COLUMN authentication_method SET DEFAULT '';
    END IF;
    IF EXISTS (SELECT 1 FROM information_schema.columns
               WHERE table_schema = 'core' AND table_name = 'card_topup_request' AND column_name = 'bsmv_total'
               AND is_nullable = 'YES') THEN
        ALTER TABLE core.card_topup_request ALTER COLUMN bsmv_total SET NOT NULL;
    END IF;
   IF EXISTS (SELECT 1 FROM information_schema.columns
               WHERE table_schema = 'core' AND table_name = 'card_topup_request' AND column_name = 'bsmv_total'
               AND column_default is null) THEN
        ALTER TABLE core.card_topup_request ALTER COLUMN bsmv_total SET DEFAULT 0.0;
    END IF;
END $$;