ALTER TABLE merchant.merchant_deduction ADD COLUMN IF NOT EXISTS processing_id uuid;
ALTER TABLE merchant.merchant_deduction ADD COLUMN IF NOT EXISTS processing_started_at timestamp without time zone NOT NULL DEFAULT TIMESTAMP '-infinity';

ALTER TABLE posting.balance ADD COLUMN IF NOT EXISTS processing_id uuid;
ALTER TABLE posting.balance ADD COLUMN IF NOT EXISTS processing_started_at timestamp without time zone NOT NULL DEFAULT TIMESTAMP '-infinity';