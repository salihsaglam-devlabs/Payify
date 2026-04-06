
DO $$
BEGIN


IF EXISTS (
      SELECT column_name
               FROM information_schema.columns
               WHERE table_schema='core' AND table_name='iks_terminal' AND column_name='hosting_trade_name'
    ) THEN
        ALTER TABLE core.iks_terminal ALTER COLUMN hosting_trade_name TYPE character varying(150);
END IF;

END $$;

DO $$
BEGIN


IF EXISTS (
      SELECT column_name
               FROM information_schema.columns
               WHERE table_schema='core' AND table_name='iks_terminal' AND column_name='payment_gw_trade_name'
    ) THEN
        ALTER TABLE core.iks_terminal ALTER COLUMN payment_gw_trade_name TYPE character varying(150);
END IF;

END $$;