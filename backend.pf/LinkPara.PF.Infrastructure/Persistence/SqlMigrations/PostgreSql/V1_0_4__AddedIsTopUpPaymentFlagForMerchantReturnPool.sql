 DO $$
BEGIN
  IF NOT EXISTS (
     SELECT 1 FROM information_schema.columns 
     WHERE table_schema = 'merchant'
       AND table_name = 'merchant_return_pool'
       AND column_name = 'is_top_up_payment'
 ) THEN
     ALTER TABLE merchant.merchant_return_pool ADD is_top_up_payment boolean;
 END IF;
 END$$;