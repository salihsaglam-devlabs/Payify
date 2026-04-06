

DO $$
BEGIN
CREATE TABLE IF NOT EXISTS core.invoice (
    id uuid NOT NULL,
    code character varying(50) NULL,
    total_commission numeric(18,4) NOT NULL,
    total_bsmv numeric(18,4) NOT NULL,
    transaction_date timestamp without time zone NOT NULL,
    CONSTRAINT pk_register_invoice PRIMARY KEY (id)
);

END $$;

DO $$
BEGIN
  IF NOT EXISTS (
      SELECT column_name
               FROM information_schema.columns
               WHERE table_schema='core' AND table_name='invoice' AND column_name='transaction_type'
    ) THEN
    ALTER TABLE core.invoice ADD transaction_type character varying(50) NOT NULL DEFAULT '';
END IF;
END $$;

DO $$
BEGIN
  IF NOT EXISTS (
      SELECT column_name
               FROM information_schema.columns
               WHERE table_schema='core' AND table_name='payment' AND column_name='receiver_payment_invoice_status'
    ) THEN
    ALTER TABLE core.payment ADD receiver_payment_invoice_status character varying(50) NOT NULL DEFAULT '';
END IF;
END $$;

DO $$
BEGIN
  IF NOT EXISTS (
      SELECT column_name
               FROM information_schema.columns
               WHERE table_schema='core' AND table_name='payment' AND column_name='receiver_invoice_id'
    ) THEN
    ALTER TABLE core.payment ADD receiver_invoice_id uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';
END IF;
END $$;

DO $$
BEGIN
  IF NOT EXISTS (
      SELECT column_name
               FROM information_schema.columns
               WHERE table_schema='core' AND table_name='payment' AND column_name='sender_payment_invoice_status'
    ) THEN
    ALTER TABLE core.payment ADD sender_payment_invoice_status character varying(50) NOT NULL DEFAULT '';
END IF;
END $$;

DO $$
BEGIN
  IF NOT EXISTS (
      SELECT column_name
               FROM information_schema.columns
               WHERE table_schema='core' AND table_name='payment' AND column_name='sender_invoice_id'
    ) THEN
    ALTER TABLE core.payment ADD sender_invoice_id uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';
END IF;
END $$;