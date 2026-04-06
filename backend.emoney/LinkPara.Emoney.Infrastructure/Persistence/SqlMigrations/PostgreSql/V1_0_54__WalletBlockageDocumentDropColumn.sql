DO $$
BEGIN
  IF EXISTS (
      SELECT column_name
               FROM information_schema.columns
               WHERE table_schema='core' AND table_name='wallet_blockage_document' AND column_name='wallet_blockage_id'
    ) THEN
ALTER TABLE core.wallet_blockage_document DROP wallet_blockage_id ;
END IF;
END $$;


