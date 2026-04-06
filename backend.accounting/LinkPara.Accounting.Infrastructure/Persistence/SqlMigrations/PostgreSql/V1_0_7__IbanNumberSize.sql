DO $$
BEGIN
  IF EXISTS (
      SELECT column_name
               FROM information_schema.columns
               WHERE table_schema='core' AND table_name='payment' AND column_name='iban_number'
    ) THEN
    ALTER TABLE core.payment ALTER COLUMN iban_number TYPE character varying(50);
END IF;
END $$;
