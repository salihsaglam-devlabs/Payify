DO
$$
BEGIN
    IF
NOT EXISTS (
         SELECT 1 FROM information_schema.columns 
         WHERE table_schema = 'merchant'
           AND table_name = 'merchant_physical_device'
           AND column_name = 'owner_terminal_id'
     ) THEN
ALTER TABLE merchant.merchant_physical_device
    ADD owner_terminal_id character varying(50);
END IF;
        
END $$;