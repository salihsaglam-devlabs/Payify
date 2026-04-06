DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_schema = 'core' AND table_name = 'iks_terminal_history' AND column_name = 'physical_pos_id'
    ) THEN
        ALTER TABLE core.iks_terminal_history ADD physical_pos_id uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';
    END IF;
    
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_schema = 'core' AND table_name = 'iks_terminal' AND column_name = 'physical_pos_id'
    ) THEN
        ALTER TABLE core.iks_terminal ADD physical_pos_id uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';
    END IF;
END $$;