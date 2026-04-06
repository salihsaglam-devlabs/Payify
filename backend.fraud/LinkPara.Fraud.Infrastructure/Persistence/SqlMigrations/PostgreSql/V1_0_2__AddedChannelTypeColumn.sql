DO $$
BEGIN

    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_schema = 'core'
          AND table_name = 'search_log'
          AND column_name = 'channel_type'
    ) THEN
        ALTER TABLE core.search_log ADD channel_type character varying(200);
    END IF;
END $$;
