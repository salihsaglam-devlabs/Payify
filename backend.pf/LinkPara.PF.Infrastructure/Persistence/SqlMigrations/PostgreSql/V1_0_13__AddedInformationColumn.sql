DO $$
BEGIN

    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_schema = 'merchant'
          AND table_name = 'merchant'
          AND column_name = 'information'
    ) THEN
        ALTER TABLE merchant.merchant ADD information character varying(300);
    END IF;
END $$;
