DO $$
BEGIN
  IF NOT EXISTS (
      SELECT indexname FROM pg_indexes
      WHERE schemaname = 'core' AND tablename = 'customer' AND indexname = 'ix_customer_code'
    ) THEN
        CREATE INDEX  ix_customer_code ON core.customer (code);
END IF;
END $$;
