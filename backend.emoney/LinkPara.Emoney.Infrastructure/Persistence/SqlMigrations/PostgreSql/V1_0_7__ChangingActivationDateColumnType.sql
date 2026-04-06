DO $$
BEGIN
    --Creating temp activation date column
    ALTER TABLE core.pricing_commercial
        ADD COLUMN IF NOT EXISTS temp_activation_timestamp TIMESTAMP;
END $$;

DO $$
BEGIN
    --Copying values of old activation date column
    UPDATE core.pricing_commercial
    SET temp_activation_timestamp = activation_date::TIMESTAMP;
END $$;

DO $$
BEGIN 
    --Removing activation date column
    ALTER TABLE core.pricing_commercial
        DROP COLUMN IF EXISTS activation_date;
END $$;
    
DO $$
BEGIN
    --Renaming temp column -> activation_date

    IF EXISTS(       SELECT *
                     FROM information_schema.columns
                     WHERE table_schema  = 'core' 
                       and table_name='pricing_commercial' 
                       and column_name='temp_activation_timestamp')
    THEN
        ALTER TABLE core.pricing_commercial
            RENAME COLUMN temp_activation_timestamp TO activation_date;
    END IF;
    
    
END $$