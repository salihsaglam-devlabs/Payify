
DO $$
BEGIN


IF NOT EXISTS (
      SELECT column_name
               FROM information_schema.columns
               WHERE table_schema='core' AND table_name='timeout_iks_transaction' AND column_name='merchant_id'
    ) THEN
        ALTER TABLE core.timeout_iks_transaction ADD merchant_id uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';
END IF;


IF NOT EXISTS (
      SELECT column_name
               FROM information_schema.columns
               WHERE table_schema='core' AND table_name='iks_transaction' AND column_name='merchant_id'
    ) THEN
       
    ALTER TABLE core.iks_transaction ADD merchant_id uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';
END IF;

END $$;



