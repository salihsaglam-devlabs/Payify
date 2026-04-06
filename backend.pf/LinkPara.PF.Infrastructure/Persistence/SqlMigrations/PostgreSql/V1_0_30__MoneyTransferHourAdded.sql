DO $$
BEGIN
 
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_schema = 'merchant'
          AND table_name = 'merchant'
          AND column_name = 'money_transfer_start_hour'
    ) THEN
        ALTER TABLE merchant.merchant ADD money_transfer_start_hour integer;
    END IF;

    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_schema = 'merchant'
          AND table_name = 'merchant'
          AND column_name = 'money_transfer_start_minute'
    ) THEN
        ALTER TABLE merchant.merchant ADD money_transfer_start_minute integer;
    END IF;
    
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_schema = 'merchant'
          AND table_name = 'pool'
          AND column_name = 'money_transfer_start_hour'
    ) THEN
        ALTER TABLE merchant.pool ADD money_transfer_start_hour integer;
    END IF;

    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_schema = 'merchant'
          AND table_name = 'pool'
          AND column_name = 'money_transfer_start_minute'
    ) THEN
        ALTER TABLE merchant.pool ADD money_transfer_start_minute integer;
    END IF;

END $$;