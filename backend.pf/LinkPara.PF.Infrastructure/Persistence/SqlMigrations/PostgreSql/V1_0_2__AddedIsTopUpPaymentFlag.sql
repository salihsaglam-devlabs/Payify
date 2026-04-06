 DO $$
BEGIN
 IF NOT EXISTS (
     SELECT 1 FROM information_schema.columns 
     WHERE table_schema = 'vpos'
       AND table_name = 'vpos'
       AND column_name = 'is_top_up_vpos'
 ) THEN
     ALTER TABLE vpos.vpos ADD is_top_up_vpos boolean;
 END IF;
 
  IF NOT EXISTS (
     SELECT 1 FROM information_schema.columns 
     WHERE table_schema = 'merchant'
       AND table_name = 'transaction'
       AND column_name = 'is_top_up_payment'
 ) THEN
     ALTER TABLE merchant.transaction ADD is_top_up_payment boolean;
 END IF;
 END$$;