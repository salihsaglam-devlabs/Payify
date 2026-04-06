DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_schema = 'merchant'
          AND table_name = 'transaction'
          AND column_name = 'transaction_source'
    ) THEN
ALTER TABLE merchant.transaction ADD transaction_source character varying(50) NOT NULL DEFAULT 'VirtualPos';
END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_schema = 'posting'
          AND table_name = 'transaction'
          AND column_name = 'transaction_source'
    ) THEN
ALTER TABLE posting.transaction ADD transaction_source character varying(50) NOT NULL DEFAULT 'VirtualPos';
END IF;
END $$;
