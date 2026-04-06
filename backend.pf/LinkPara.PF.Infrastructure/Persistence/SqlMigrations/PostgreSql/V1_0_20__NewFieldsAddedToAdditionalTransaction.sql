ALTER TABLE posting.posting_additional_transaction
    ADD COLUMN IF NOT EXISTS merchant_physical_pos_id uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';

ALTER TABLE posting.posting_additional_transaction
    ADD COLUMN IF NOT EXISTS pf_transaction_source character varying(50) NOT NULL DEFAULT 'VirtualPos';