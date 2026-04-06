DO $$
BEGIN
 
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_schema = 'merchant'
          AND table_name = 'merchant_physical_pos'
          AND column_name = 'bkm_reference_number'
    ) THEN
        ALTER TABLE merchant.merchant_physical_pos ADD bkm_reference_number character varying(50);
    END IF;

    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_schema = 'merchant'
          AND table_name = 'merchant_physical_pos'
          AND column_name = 'terminal_status'
    ) THEN
        ALTER TABLE merchant.merchant_physical_pos ADD terminal_status character varying(50) NOT NULL DEFAULT '';
    END IF;

END $$;
