DO $$
BEGIN
  IF NOT EXISTS (
      SELECT column_name
               FROM information_schema.columns
               WHERE table_schema='core' AND table_name='payment' AND column_name='iban_number'
    ) THEN
    ALTER TABLE core.payment ADD iban_number character varying(20) NULL DEFAULT '';
END IF;
END $$;
