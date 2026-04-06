DO $$
BEGIN
  IF NOT EXISTS (
      SELECT column_name
               FROM information_schema.columns
               WHERE table_schema='core' AND table_name='payment' AND column_name='failed_payment_retry_count'
    ) THEN
    ALTER TABLE core.payment ADD failed_payment_retry_count integer NOT NULL DEFAULT 0;
END IF;
END $$;
