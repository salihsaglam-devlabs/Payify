DO $$
BEGIN
  IF NOT EXISTS (
      SELECT column_name
               FROM information_schema.columns
               WHERE table_schema='core' AND table_name='customer' AND column_name='customer_code'
    ) THEN
        ALTER TABLE core.customer ADD customer_code character varying(20) NULL;
END IF;
END $$;
