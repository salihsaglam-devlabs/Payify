DO $$
BEGIN 
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_schema = 'core'
          AND table_name = 'triggered_rule_set_key'
          AND column_name = 'compliance_rule_set_key'
    ) THEN
        ALTER TABLE core.triggered_rule_set_key ADD compliance_rule_set_key character varying(50);
    END IF;
END $$;
