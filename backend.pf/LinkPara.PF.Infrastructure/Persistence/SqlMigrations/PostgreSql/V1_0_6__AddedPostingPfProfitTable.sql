DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM information_schema.columns
        WHERE table_schema = 'posting'
          AND table_name = 'transaction'
          AND column_name = 'bank_payment_date'
    ) THEN
        ALTER TABLE posting.transaction
        ADD COLUMN bank_payment_date DATE NOT NULL DEFAULT '0001-01-01';
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM information_schema.tables
        WHERE table_schema = 'posting'
          AND table_name = 'pf_profit'
    ) THEN
        CREATE TABLE posting.pf_profit (
            id UUID PRIMARY KEY,
            payment_date DATE NOT NULL,
            amount_without_bank_commission NUMERIC(18,4) NOT NULL,
            total_paying_amount NUMERIC(18,4) NOT NULL,
            total_pf_net_commission_amount NUMERIC(18,4) NOT NULL,
            protection_transfer_amount NUMERIC(18,4) NOT NULL,
            currency INTEGER NOT NULL,
            create_date TIMESTAMP NOT NULL,
            update_date TIMESTAMP,
            created_by VARCHAR(50) NOT NULL,
            last_modified_by VARCHAR(50),
            record_status VARCHAR(50) NOT NULL
        );
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM information_schema.tables
        WHERE table_schema = 'posting'
          AND table_name = 'pf_profit_detail'
    ) THEN
        CREATE TABLE posting.pf_profit_detail (
            id UUID PRIMARY KEY,
            acquire_bank_code INTEGER NOT NULL,
            bank_name VARCHAR(20) NOT NULL,
            bank_paying_amount NUMERIC(18,4) NOT NULL,
            currency INTEGER NOT NULL,
            posting_pf_profit_id UUID NOT NULL,
            create_date TIMESTAMP NOT NULL,
            update_date TIMESTAMP,
            created_by VARCHAR(50) NOT NULL,
            last_modified_by VARCHAR(50),
            record_status VARCHAR(50) NOT NULL,
            CONSTRAINT fk_pf_profit_detail_pf_profit_posting_pf_profit_id
                FOREIGN KEY (posting_pf_profit_id)
                REFERENCES posting.pf_profit (id)
                ON DELETE RESTRICT
        );
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM pg_indexes
        WHERE schemaname = 'posting'
          AND tablename = 'pf_profit_detail'
          AND indexname = 'ix_pf_profit_detail_posting_pf_profit_id'
    ) THEN
        CREATE INDEX ix_pf_profit_detail_posting_pf_profit_id ON posting.pf_profit_detail USING btree (posting_pf_profit_id);
    END IF;
END $$;