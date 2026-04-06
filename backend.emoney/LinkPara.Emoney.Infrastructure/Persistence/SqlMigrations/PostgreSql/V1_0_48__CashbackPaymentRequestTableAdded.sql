DO $$
BEGIN
  IF NOT EXISTS (
      SELECT tablename FROM pg_tables
      WHERE schemaname = 'core' AND tablename = 'cashback_payment_request'
  ) THEN
    CREATE TABLE core.cashback_payment_request (
        id uuid NOT NULL,
        entitlement_id uuid NOT NULL,
        cashback_payment_status character varying(50) NOT NULL,
        transaction_id uuid,
        wallet_number character varying(30) NOT NULL,
        amount numeric(18,2) NOT NULL,
        currency_code character varying(10) NOT NULL,
        create_date timestamp without time zone NOT NULL,
        created_by character varying(50) NOT NULL,
        update_date timestamp without time zone,
        last_modified_by character varying(50),
        record_status character varying(50) NOT NULL,
        CONSTRAINT pk_cashback_payment_request PRIMARY KEY (id)
    );
  END IF;
END $$;