ALTER TABLE posting.balance DROP COLUMN IF EXISTS total_submerchant_deduction_amount;

ALTER TABLE posting.balance ADD COLUMN IF NOT EXISTS total_suspicious_transfer_amount numeric(18,4) NOT NULL DEFAULT 0.0;

ALTER TABLE posting.balance ADD COLUMN IF NOT EXISTS total_chargeback_commission_amount numeric(18,4) NOT NULL DEFAULT 0.0;

ALTER TABLE posting.balance ADD COLUMN IF NOT EXISTS total_chargeback_transfer_amount numeric(18,4) NOT NULL DEFAULT 0.0;

ALTER TABLE posting.balance ADD COLUMN IF NOT EXISTS total_due_transfer_amount numeric(18,4) NOT NULL DEFAULT 0.0;

ALTER TABLE posting.balance ADD COLUMN IF NOT EXISTS total_excess_return_on_commission_amount numeric(18,4) NOT NULL DEFAULT 0.0;

ALTER TABLE posting.balance ADD COLUMN IF NOT EXISTS total_excess_return_transfer_amount numeric(18,4) NOT NULL DEFAULT 0.0;

ALTER TABLE posting.balance ADD COLUMN IF NOT EXISTS total_suspicious_commission_amount numeric(18,4) NOT NULL DEFAULT 0.0;