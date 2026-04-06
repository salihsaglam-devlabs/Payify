DO $$
BEGIN
  IF NOT EXISTS (
      SELECT column_name
               FROM information_schema.columns
               WHERE table_schema='core' AND table_name='customer' AND column_name='accounting_customer_code'
    ) THEN
    ALTER TABLE core.customer ADD accounting_customer_code character varying(20) NULL DEFAULT '';
END IF;
END $$;
