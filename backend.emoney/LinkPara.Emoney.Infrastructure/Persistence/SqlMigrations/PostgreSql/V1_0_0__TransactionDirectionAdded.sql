
DO $$
BEGIN
  IF NOT EXISTS (
      SELECT column_name
               FROM information_schema.columns
               WHERE table_schema='core' AND table_name='transaction' AND column_name='tag'
    ) THEN
ALTER TABLE core.transaction ALTER COLUMN tag TYPE character varying(200);
END IF;
END $$;

DO $$
BEGIN
  IF NOT EXISTS (
      SELECT column_name
               FROM information_schema.columns
               WHERE table_schema='core' AND table_name='transaction' AND column_name='tag_title'
    ) THEN
        ALTER TABLE core.transaction ADD tag_title character varying(50) NULL;
END IF;
END $$;

DO $$
BEGIN
  IF NOT EXISTS (
      SELECT column_name
               FROM information_schema.columns
               WHERE table_schema='core' AND table_name='transaction' AND column_name='transaction_direction'
    ) THEN
        ALTER TABLE core.transaction ADD transaction_direction character varying(50) NOT NULL DEFAULT '';
END IF;
END $$;


DO $$
BEGIN
  IF NOT EXISTS (
      SELECT indexname FROM pg_indexes
      WHERE schemaname = 'core' AND tablename = 'transaction' AND indexname = 'ix_transaction_transaction_type'
    ) THEN
        CREATE INDEX ix_transaction_transaction_type ON core.transaction (transaction_type);
END IF;
END $$;

DO $$
BEGIN
  IF NOT EXISTS (
      SELECT column_name
               FROM information_schema.columns
               WHERE table_schema='core' AND table_name='provision' AND column_name='payment_provision_id'
    ) THEN
        ALTER TABLE core.provision ADD payment_provision_id uuid NULL;
END IF;
END $$;

DO $$
BEGIN
  IF NOT EXISTS (
      SELECT indexname FROM pg_indexes
      WHERE schemaname = 'core' AND tablename = 'provision' AND indexname = 'ix_provision_payment_provision_id'
    ) THEN
        CREATE INDEX ix_provision_payment_provision_id ON core.provision (payment_provision_id);
END IF;
END $$;


DO $$
BEGIN
  IF NOT EXISTS (
      SELECT conname FROM pg_constraint
        WHERE conname = 'fk_provision_provision_payment_provision_id'
    ) THEN
        ALTER TABLE core.provision ADD CONSTRAINT fk_provision_provision_payment_provision_id 
            FOREIGN KEY (payment_provision_id) REFERENCES core.provision (id);
END IF;
END $$;

