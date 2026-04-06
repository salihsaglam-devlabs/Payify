DO
$$
BEGIN
    IF
NOT EXISTS (
         SELECT 1 FROM information_schema.columns 
         WHERE table_schema = 'merchant'
           AND table_name = 'merchant'
           AND column_name = 'branch_count'
     ) THEN
ALTER TABLE merchant.merchant
    ADD branch_count int4 NOT NULL DEFAULT 0;
END IF;
    
    IF
NOT EXISTS (
         SELECT 1 FROM information_schema.columns 
         WHERE table_schema = 'merchant'
           AND table_name = 'merchant'
           AND column_name = 'employee_count'
     ) THEN
ALTER TABLE merchant.merchant
    ADD employee_count int4 NOT NULL DEFAULT 0;
END IF;
    
    IF
NOT EXISTS (
         SELECT 1 FROM information_schema.columns 
         WHERE table_schema = 'merchant'
           AND table_name = 'merchant'
           AND column_name = 'business_activity'
     ) THEN
ALTER TABLE merchant.merchant
    ADD business_activity character varying(140) NOT NULL DEFAULT '';
END IF;
    
    IF
NOT EXISTS (
         SELECT 1 FROM information_schema.columns 
         WHERE table_schema = 'merchant'
           AND table_name = 'merchant'
           AND column_name = 'business_model'
     ) THEN
ALTER TABLE merchant.merchant
    ADD business_model character varying(50) NOT NULL DEFAULT '';
END IF;
        
    IF
NOT EXISTS (
         SELECT 1 FROM information_schema.columns 
         WHERE table_schema = 'merchant'
           AND table_name = 'merchant'
           AND column_name = 'establishment_date'
     ) THEN
ALTER TABLE merchant.merchant
    ADD establishment_date timestamp without time zone 
NOT NULL DEFAULT '1900-01-01';
END IF;
END $$;