
DO $$
BEGIN
  IF NOT EXISTS (
      SELECT column_name
      FROM information_schema.columns
      WHERE table_schema = 'core' AND table_name = 'transfer_order' AND column_name = 'payment_type'
  ) THEN
    ALTER TABLE core.transfer_order ADD payment_type character varying(100); 
  END IF;
END $$;
