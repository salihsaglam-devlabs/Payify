DO $$
BEGIN
  IF NOT EXISTS (
      SELECT tablename FROM pg_tables
      WHERE schemaname = 'core' AND tablename = 'wallet_balance_daily'
  ) THEN
    CREATE TABLE core.wallet_balance_daily (
        id uuid NOT NULL DEFAULT gen_random_uuid(),
        create_date timestamp without time zone NOT NULL,
        created_by character varying(50) NOT NULL,
        currency character varying(10),
        daily_balance numeric NOT NULL,
        job_date timestamp without time zone NOT NULL,
        last_modified_by character varying(50),
        record_status character varying(50) NOT NULL,
        update_date timestamp without time zone,
        CONSTRAINT pk_wallet_balance_daily PRIMARY KEY (id)
    );
  END IF;
END $$;