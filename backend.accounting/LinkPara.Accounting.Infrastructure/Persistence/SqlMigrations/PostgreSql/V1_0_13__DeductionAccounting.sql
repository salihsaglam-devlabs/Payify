ALTER TABLE core.payment ADD COLUMN IF NOT EXISTS bank_commission_amount numeric(18,4) NOT NULL DEFAULT 0.0;

ALTER TABLE core.payment ADD COLUMN IF NOT EXISTS chargeback_amount numeric(18,4) NOT NULL DEFAULT 0.0;

ALTER TABLE core.payment ADD COLUMN IF NOT EXISTS chargeback_return_amount numeric(18,4) NOT NULL DEFAULT 0.0;

ALTER TABLE core.payment ADD COLUMN IF NOT EXISTS due_amount numeric(18,4) NOT NULL DEFAULT 0.0;

ALTER TABLE core.payment ADD COLUMN IF NOT EXISTS return_amount numeric(18,4) NOT NULL DEFAULT 0.0;

ALTER TABLE core.payment ADD COLUMN IF NOT EXISTS suspicious_amount numeric(18,4) NOT NULL DEFAULT 0.0;

ALTER TABLE core.payment ADD COLUMN IF NOT EXISTS suspicious_return_amount numeric(18,4) NOT NULL DEFAULT 0.0;

ALTER TABLE core.bank_account ADD COLUMN IF NOT EXISTS account_tag character varying(350) NOT NULL DEFAULT '{{BankAccountNumber}}';   