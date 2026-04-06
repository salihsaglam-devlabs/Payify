DO $$
BEGIN
    IF NOT EXISTS (
     SELECT 1 FROM information_schema.columns 
     WHERE table_schema = 'merchant'
       AND table_name = 'merchant'
       AND column_name = 'is_payment_to_main_merchant'
    ) THEN
     ALTER TABLE merchant.merchant ADD is_payment_to_main_merchant boolean;
    END IF;
 
    IF NOT EXISTS (
      SELECT 1 FROM information_schema.columns 
      WHERE table_schema = 'merchant'
        AND table_name = 'pool'
        AND column_name = 'is_payment_to_main_merchant'
    ) THEN
      ALTER TABLE merchant.pool ADD is_payment_to_main_merchant boolean;
    END IF;
  
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_schema = 'core'
          AND table_name = 'pricing_profile'
          AND column_name = 'is_payment_to_main_merchant'
    ) THEN
        ALTER TABLE core.pricing_profile ADD is_payment_to_main_merchant boolean;
    END IF;
 END$$;