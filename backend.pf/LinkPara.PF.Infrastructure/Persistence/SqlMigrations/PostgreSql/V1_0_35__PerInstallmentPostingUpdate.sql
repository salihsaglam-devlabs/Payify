DROP INDEX IF EXISTS posting.ix_transaction_merchant_transaction_id;

ALTER TABLE posting.transaction ADD COLUMN IF NOT EXISTS installment_sequence integer NOT NULL DEFAULT 0;
ALTER TABLE posting.transaction ADD COLUMN IF NOT EXISTS is_per_installment boolean NOT NULL DEFAULT FALSE;
ALTER TABLE posting.transaction ADD COLUMN IF NOT EXISTS merchant_installment_transaction_id uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';

ALTER TABLE posting.posting_additional_transaction ADD COLUMN IF NOT EXISTS installment_sequence integer NOT NULL DEFAULT 0;
ALTER TABLE posting.posting_additional_transaction ADD COLUMN IF NOT EXISTS is_per_installment boolean NOT NULL DEFAULT FALSE;
ALTER TABLE posting.posting_additional_transaction ADD COLUMN IF NOT EXISTS merchant_installment_transaction_id uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';

CREATE UNIQUE INDEX IF NOT EXISTS ix_transaction_merchant_transaction_id_merchant_installment_tr
    ON posting.transaction (merchant_transaction_id, merchant_installment_transaction_id);