DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_schema = 'merchant'
            AND table_name = 'pool'
            AND column_name = 'pos_type'
    ) THEN
        ALTER TABLE merchant.pool ADD pos_type character varying(50) NOT NULL DEFAULT 'Virtual';
    END IF;
 
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_schema = 'merchant'
            AND table_name = 'merchant'
            AND column_name = 'pos_type'
    ) THEN
        ALTER TABLE merchant.merchant ADD pos_type  character varying(50) NOT NULL DEFAULT 'Virtual';
    END IF;
END$$;