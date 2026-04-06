ALTER TABLE physical.unacceptable_transaction DROP COLUMN IF EXISTS issuer_bank_id;

ALTER TABLE physical.unacceptable_transaction ADD COLUMN IF NOT EXISTS end_of_day_status character varying(50) NOT NULL DEFAULT 'Pending';

ALTER TABLE physical.unacceptable_transaction ADD COLUMN IF NOT EXISTS merchant_transaction_id uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';

ALTER TABLE physical.reconciliation_transaction ADD COLUMN IF NOT EXISTS error_code character varying(20);

ALTER TABLE physical.reconciliation_transaction ADD COLUMN IF NOT EXISTS error_message character varying(300);