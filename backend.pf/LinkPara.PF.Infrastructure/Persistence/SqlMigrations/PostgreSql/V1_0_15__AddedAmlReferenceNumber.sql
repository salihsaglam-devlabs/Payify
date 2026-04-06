DO $$
BEGIN
    IF NOT EXISTS (
     SELECT 1 FROM information_schema.columns 
     WHERE table_schema = 'merchant'
       AND table_name = 'business_partner'
       AND column_name = 'aml_reference_number'
    ) THEN
     ALTER TABLE merchant.business_partner ADD aml_reference_number character varying(150);
    END IF;
 END$$;