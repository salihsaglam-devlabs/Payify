DO $$
BEGIN
    IF NOT EXISTS (
         SELECT 1 FROM information_schema.columns 
         WHERE table_schema = 'merchant'
           AND table_name = 'transaction'
           AND column_name = 'is_insurance_payment'
     ) THEN
    ALTER TABLE merchant.transaction ADD is_insurance_payment boolean NOT NULL DEFAULT false;
    END IF;
    
    IF NOT EXISTS (
             SELECT 1 FROM information_schema.columns 
             WHERE table_schema = 'vpos'
               AND table_name = 'vpos'
               AND column_name = 'is_insurance_vpos'
         ) THEN
    ALTER TABLE vpos.vpos ADD is_insurance_vpos boolean NOT NULL DEFAULT false;
    END IF;
    
    IF NOT EXISTS (
                 SELECT 1 FROM information_schema.columns 
                 WHERE table_schema = 'merchant'
                   AND table_name = 'merchant'
                   AND column_name = 'insurance_payment_allowed'
             ) THEN
    ALTER TABLE merchant.merchant ADD insurance_payment_allowed boolean NOT NULL DEFAULT false;
    END IF;
    
    IF NOT EXISTS (
                 SELECT 1 FROM information_schema.columns 
                 WHERE table_schema = 'merchant'
                   AND table_name = 'transaction'
                   AND column_name = 'card_holder_identity_number'
         ) THEN
    ALTER TABLE merchant.transaction ADD card_holder_identity_number varchar(15);
    END IF;
END $$;