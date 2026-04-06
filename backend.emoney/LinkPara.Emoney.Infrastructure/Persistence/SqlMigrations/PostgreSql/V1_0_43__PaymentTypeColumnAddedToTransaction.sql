DO $$
BEGIN
    IF (SELECT table_schema FROM information_schema.tables WHERE table_name = 'wallet_balance_daily') <> 'core' THEN
        ALTER TABLE wallet_balance_daily SET SCHEMA core;
    END IF;
END $$;




DO $$
BEGIN
  IF NOT EXISTS (
      SELECT column_name
               FROM information_schema.columns
               WHERE table_schema='core' AND table_name='transaction' AND column_name='payment_type'
    ) THEN
    ALTER TABLE core.transaction ADD payment_type character varying(100); 
END IF;
END $$;

