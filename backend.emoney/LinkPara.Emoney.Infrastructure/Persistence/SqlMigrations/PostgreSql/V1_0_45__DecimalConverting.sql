-- PostgreSQL idempotent DDL script

-- core.withdraw_request.amount
DO $$
BEGIN
    IF EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_schema = 'core' AND table_name = 'withdraw_request' AND column_name = 'amount'
        AND (data_type <> 'numeric' OR numeric_precision <> 18 OR numeric_scale <> 2)
    ) THEN
        ALTER TABLE core.withdraw_request ALTER COLUMN amount TYPE numeric(18,2);
    END IF;
END $$;

-- core.wallet.current_balance_credit
DO $$
BEGIN
    IF EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_schema = 'core' AND table_name = 'wallet' AND column_name = 'current_balance_credit'
        AND (data_type <> 'numeric' OR numeric_precision <> 18 OR numeric_scale <> 2)
    ) THEN
        ALTER TABLE core.wallet ALTER COLUMN current_balance_credit TYPE numeric(18,2);
    END IF;
END $$;

-- core.wallet.current_balance_cash
DO $$
BEGIN
    IF EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_schema = 'core' AND table_name = 'wallet' AND column_name = 'current_balance_cash'
        AND (data_type <> 'numeric' OR numeric_precision <> 18 OR numeric_scale <> 2)
    ) THEN
        ALTER TABLE core.wallet ALTER COLUMN current_balance_cash TYPE numeric(18,2);
    END IF;
END $$;

-- core.wallet.blocked_balance
DO $$
BEGIN
    IF EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_schema = 'core' AND table_name = 'wallet' AND column_name = 'blocked_balance'
        AND (data_type <> 'numeric' OR numeric_precision <> 18 OR numeric_scale <> 2)
    ) THEN
        ALTER TABLE core.wallet ALTER COLUMN blocked_balance TYPE numeric(18,2);
    END IF;
END $$;

-- core.transfer_order.amount
DO $$
BEGIN
    IF EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_schema = 'core' AND table_name = 'transfer_order' AND column_name = 'amount'
        AND (data_type <> 'numeric' OR numeric_precision <> 18 OR numeric_scale <> 2)
    ) THEN
        ALTER TABLE core.transfer_order ALTER COLUMN amount TYPE numeric(18,2);
    END IF;
END $$;

-- core.transaction.pre_balance
DO $$
BEGIN
    IF EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_schema = 'core' AND table_name = 'transaction' AND column_name = 'pre_balance'
        AND (data_type <> 'numeric' OR numeric_precision <> 18 OR numeric_scale <> 2)
    ) THEN
        ALTER TABLE core.transaction ALTER COLUMN pre_balance TYPE numeric(18,2);
    END IF;
END $$;

-- core.transaction.current_balance
DO $$
BEGIN
    IF EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_schema = 'core' AND table_name = 'transaction' AND column_name = 'current_balance'
        AND (data_type <> 'numeric' OR numeric_precision <> 18 OR numeric_scale <> 2)
    ) THEN
        ALTER TABLE core.transaction ALTER COLUMN current_balance TYPE numeric(18,2);
    END IF;
END $$;

-- core.transaction.amount
DO $$
BEGIN
    IF EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_schema = 'core' AND table_name = 'transaction' AND column_name = 'amount'
        AND (data_type <> 'numeric' OR numeric_precision <> 18 OR numeric_scale <> 2)
    ) THEN
        ALTER TABLE core.transaction ALTER COLUMN amount TYPE numeric(18,2);
    END IF;
END $$;

-- core.transaction.ip_address (varsa ekleme)
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_schema = 'core' AND table_name = 'transaction' AND column_name = 'ip_address'
    ) THEN
        ALTER TABLE core.transaction ADD COLUMN ip_address character varying(50);
    END IF;
END $$;

-- limit.tier_level.monthly_max_withdrawal_amount
DO $$
BEGIN
    IF EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_schema = 'limit' AND table_name = 'tier_level' AND column_name = 'monthly_max_withdrawal_amount'
        AND (data_type <> 'numeric' OR numeric_precision <> 18 OR numeric_scale <> 2)
    ) THEN
        ALTER TABLE "limit".tier_level ALTER COLUMN monthly_max_withdrawal_amount TYPE numeric(18,2);
    END IF;
END $$;

-- limit.tier_level.monthly_max_other_iban_withdrawal_amount
DO $$
BEGIN
    IF EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_schema = 'limit' AND table_name = 'tier_level' AND column_name = 'monthly_max_other_iban_withdrawal_amount'
        AND (data_type <> 'numeric' OR numeric_precision <> 18 OR numeric_scale <> 2)
    ) THEN
        ALTER TABLE "limit".tier_level ALTER COLUMN monthly_max_other_iban_withdrawal_amount TYPE numeric(18,2);
    END IF;
END $$;

-- limit.tier_level.monthly_max_on_us_payment_amount
DO $$
BEGIN
    IF EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_schema = 'limit' AND table_name = 'tier_level' AND column_name = 'monthly_max_on_us_payment_amount'
        AND (data_type <> 'numeric' OR numeric_precision <> 18 OR numeric_scale <> 2)
    ) THEN
        ALTER TABLE "limit".tier_level ALTER COLUMN monthly_max_on_us_payment_amount TYPE numeric(18,2);
    END IF;
END $$;

-- limit.tier_level.monthly_max_international_transfer_amount
DO $$
BEGIN
    IF EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_schema = 'limit' AND table_name = 'tier_level' AND column_name = 'monthly_max_international_transfer_amount'
        AND (data_type <> 'numeric' OR numeric_precision <> 18 OR numeric_scale <> 2)
    ) THEN
        ALTER TABLE "limit".tier_level ALTER COLUMN monthly_max_international_transfer_amount TYPE numeric(18,2);
    END IF;
END $$;

-- limit.tier_level.monthly_max_internal_transfer_amount
DO $$
BEGIN
    IF EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_schema = 'limit' AND table_name = 'tier_level' AND column_name = 'monthly_max_internal_transfer_amount'
        AND (data_type <> 'numeric' OR numeric_precision <> 18 OR numeric_scale <> 2)
    ) THEN
        ALTER TABLE "limit".tier_level ALTER COLUMN monthly_max_internal_transfer_amount TYPE numeric(18,2);
    END IF;
END $$;

-- limit.tier_level.monthly_max_deposit_amount
DO $$
BEGIN
    IF EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_schema = 'limit' AND table_name = 'tier_level' AND column_name = 'monthly_max_deposit_amount'
        AND (data_type <> 'numeric' OR numeric_precision <> 18 OR numeric_scale <> 2)
    ) THEN
        ALTER TABLE "limit".tier_level ALTER COLUMN monthly_max_deposit_amount TYPE numeric(18,2);
    END IF;
END $$;

-- limit.tier_level.max_balance
DO $$
BEGIN
    IF EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_schema = 'limit' AND table_name = 'tier_level' AND column_name = 'max_balance'
        AND (data_type <> 'numeric' OR numeric_precision <> 18 OR numeric_scale <> 2)
    ) THEN
        ALTER TABLE "limit".tier_level ALTER COLUMN max_balance TYPE numeric(18,2);
    END IF;
END $$;

-- limit.tier_level.daily_max_withdrawal_amount
DO $$
BEGIN
    IF EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_schema = 'limit' AND table_name = 'tier_level' AND column_name = 'daily_max_withdrawal_amount'
        AND (data_type <> 'numeric' OR numeric_precision <> 18 OR numeric_scale <> 2)
    ) THEN
        ALTER TABLE "limit".tier_level ALTER COLUMN daily_max_withdrawal_amount TYPE numeric(18,2);
    END IF;
END $$;

-- limit.tier_level.daily_max_other_iban_withdrawal_amount
DO $$
BEGIN
    IF EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_schema = 'limit' AND table_name = 'tier_level' AND column_name = 'daily_max_other_iban_withdrawal_amount'
        AND (data_type <> 'numeric' OR numeric_precision <> 18 OR numeric_scale <> 2)
    ) THEN
        ALTER TABLE "limit".tier_level ALTER COLUMN daily_max_other_iban_withdrawal_amount TYPE numeric(18,2);
    END IF;
END $$;

-- limit.tier_level.daily_max_on_us_payment_amount
DO $$
BEGIN
    IF EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_schema = 'limit' AND table_name = 'tier_level' AND column_name = 'daily_max_on_us_payment_amount'
        AND (data_type <> 'numeric' OR numeric_precision <> 18 OR numeric_scale <> 2)
    ) THEN
        ALTER TABLE "limit".tier_level ALTER COLUMN daily_max_on_us_payment_amount TYPE numeric(18,2);
    END IF;
END $$;

-- limit.tier_level.daily_max_international_transfer_amount
DO $$
BEGIN
    IF EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_schema = 'limit' AND table_name = 'tier_level' AND column_name = 'daily_max_international_transfer_amount'
        AND (data_type <> 'numeric' OR numeric_precision <> 18 OR numeric_scale <> 2)
    ) THEN
        ALTER TABLE "limit".tier_level ALTER COLUMN daily_max_international_transfer_amount TYPE numeric(18,2);
    END IF;
END $$;

-- limit.tier_level.daily_max_internal_transfer_amount
DO $$
BEGIN
    IF EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_schema = 'limit' AND table_name = 'tier_level' AND column_name = 'daily_max_internal_transfer_amount'
        AND (data_type <> 'numeric' OR numeric_precision <> 18 OR numeric_scale <> 2)
    ) THEN
        ALTER TABLE "limit".tier_level ALTER COLUMN daily_max_internal_transfer_amount TYPE numeric(18,2);
    END IF;
END $$;

-- limit.tier_level.daily_max_deposit_amount
DO $$
BEGIN
    IF EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_schema = 'limit' AND table_name = 'tier_level' AND column_name = 'daily_max_deposit_amount'
        AND (data_type <> 'numeric' OR numeric_precision <> 18 OR numeric_scale <> 2)
    ) THEN
        ALTER TABLE "limit".tier_level ALTER COLUMN daily_max_deposit_amount TYPE numeric(18,2);
    END IF;
END $$;

-- core.provision.commission_amount
DO $$
BEGIN
    IF EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_schema = 'core' AND table_name = 'provision' AND column_name = 'commission_amount'
        AND (data_type <> 'numeric' OR numeric_precision <> 18 OR numeric_scale <> 2)
    ) THEN
        ALTER TABLE core.provision ALTER COLUMN commission_amount TYPE numeric(18,2);
    END IF;
END $$;

-- core.provision.bsmv_amount
DO $$
BEGIN
    IF EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_schema = 'core' AND table_name = 'provision' AND column_name = 'bsmv_amount'
        AND (data_type <> 'numeric' OR numeric_precision <> 18 OR numeric_scale <> 2)
    ) THEN
        ALTER TABLE core.provision ALTER COLUMN bsmv_amount TYPE numeric(18,2);
    END IF;
END $$;

-- core.provision.amount
DO $$
BEGIN
    IF EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_schema = 'core' AND table_name = 'provision' AND column_name = 'amount'
        AND (data_type <> 'numeric' OR numeric_precision <> 18 OR numeric_scale <> 2)
    ) THEN
        ALTER TABLE core.provision ALTER COLUMN amount TYPE numeric(18,2);
    END IF;
END $$;

-- core.pricing_profile_item.min_amount
DO $$
BEGIN
    IF EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_schema = 'core' AND table_name = 'pricing_profile_item' AND column_name = 'min_amount'
        AND (data_type <> 'numeric' OR numeric_precision <> 18 OR numeric_scale <> 2)
    ) THEN
        ALTER TABLE core.pricing_profile_item ALTER COLUMN min_amount TYPE numeric(18,2);
    END IF;
END $$;

-- core.pricing_profile_item.max_amount
DO $$
BEGIN
    IF EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_schema = 'core' AND table_name = 'pricing_profile_item' AND column_name = 'max_amount'
        AND (data_type <> 'numeric' OR numeric_precision <> 18 OR numeric_scale <> 2)
    ) THEN
        ALTER TABLE core.pricing_profile_item ALTER COLUMN max_amount TYPE numeric(18,2);
    END IF;
END $$;

-- core.pricing_profile_item.fee
DO $$
BEGIN
    IF EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_schema = 'core' AND table_name = 'pricing_profile_item' AND column_name = 'fee'
        AND (data_type <> 'numeric' OR numeric_precision <> 18 OR numeric_scale <> 2)
    ) THEN
        ALTER TABLE core.pricing_profile_item ALTER COLUMN fee TYPE numeric(18,2);
    END IF;
END $$;

-- core.pricing_profile_item.commission_rate
DO $$
BEGIN
    IF EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_schema = 'core' AND table_name = 'pricing_profile_item' AND column_name = 'commission_rate'
        AND (data_type <> 'numeric' OR numeric_precision <> 18 OR numeric_scale <> 2)
    ) THEN
        ALTER TABLE core.pricing_profile_item ALTER COLUMN commission_rate TYPE numeric(18,2);
    END IF;
END $$;

-- core.pricing_commercial.max_distinct_sender_amount
DO $$
BEGIN
    IF EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_schema = 'core' AND table_name = 'pricing_commercial' AND column_name = 'max_distinct_sender_amount'
        AND (data_type <> 'numeric' OR numeric_precision <> 18 OR numeric_scale <> 2)
    ) THEN
        ALTER TABLE core.pricing_commercial ALTER COLUMN max_distinct_sender_amount TYPE numeric(18,2);
    END IF;
END $$;

-- core.pricing_commercial.commission_rate
DO $$
BEGIN
    IF EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_schema = 'core' AND table_name = 'pricing_commercial' AND column_name = 'commission_rate'
        AND (data_type <> 'numeric' OR numeric_precision <> 18 OR numeric_scale <> 2)
    ) THEN
        ALTER TABLE core.pricing_commercial ALTER COLUMN commission_rate TYPE numeric(18,2);
    END IF;
END $$;

-- core.onus_payment_request.amount
DO $$
BEGIN
    IF EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_schema = 'core' AND table_name = 'onus_payment_request' AND column_name = 'amount'
        AND (data_type <> 'numeric' OR numeric_precision <> 18 OR numeric_scale <> 2)
    ) THEN
        ALTER TABLE core.onus_payment_request ALTER COLUMN amount TYPE numeric(18,2);
    END IF;
END $$;

-- core.chargeback.amount
DO $$
BEGIN
    IF EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_schema = 'core' AND table_name = 'chargeback' AND column_name = 'amount'
        AND (data_type <> 'numeric' OR numeric_precision <> 18 OR numeric_scale <> 2)
    ) THEN
        ALTER TABLE core.chargeback ALTER COLUMN amount TYPE numeric(18,2);
    END IF;
END $$;

-- core.card_topup_request.fee
DO $$
BEGIN
    IF EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_schema = 'core' AND table_name = 'card_topup_request' AND column_name = 'fee'
        AND (data_type <> 'numeric' OR numeric_precision <> 18 OR numeric_scale <> 2)
    ) THEN
        ALTER TABLE core.card_topup_request ALTER COLUMN fee TYPE numeric(18,2);
    END IF;
END $$;

-- core.card_topup_request.commission_total
DO $$
BEGIN
    IF EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_schema = 'core' AND table_name = 'card_topup_request' AND column_name = 'commission_total'
        AND (data_type <> 'numeric' OR numeric_precision <> 18 OR numeric_scale <> 2)
    ) THEN
        ALTER TABLE core.card_topup_request ALTER COLUMN commission_total TYPE numeric(18,2);
    END IF;
END $$;

-- core.card_topup_request.bsmv_total
DO $$
BEGIN
    IF EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_schema = 'core' AND table_name = 'card_topup_request' AND column_name = 'bsmv_total'
        AND (data_type <> 'numeric' OR numeric_precision <> 18 OR numeric_scale <> 2)
    ) THEN
        ALTER TABLE core.card_topup_request ALTER COLUMN bsmv_total TYPE numeric(18,2);
    END IF;
END $$;

-- core.card_topup_request.amount
DO $$
BEGIN
    IF EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_schema = 'core' AND table_name = 'card_topup_request' AND column_name = 'amount'
        AND (data_type <> 'numeric' OR numeric_precision <> 18 OR numeric_scale <> 2)
    ) THEN
        ALTER TABLE core.card_topup_request ALTER COLUMN amount TYPE numeric(18,2);
    END IF;
END $$;

-- limit.account_current_level.monthly_withdrawal_amount
DO $$
BEGIN
    IF EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_schema = 'limit' AND table_name = 'account_current_level' AND column_name = 'monthly_withdrawal_amount'
        AND (data_type <> 'numeric' OR numeric_precision <> 18 OR numeric_scale <> 2)
    ) THEN
        ALTER TABLE "limit".account_current_level ALTER COLUMN monthly_withdrawal_amount TYPE numeric(18,2);
    END IF;
END $$;

-- limit.account_current_level.monthly_other_iban_withdrawal_amount
DO $$
BEGIN
    IF EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_schema = 'limit' AND table_name = 'account_current_level' AND column_name = 'monthly_other_iban_withdrawal_amount'
        AND (data_type <> 'numeric' OR numeric_precision <> 18 OR numeric_scale <> 2)
    ) THEN
        ALTER TABLE "limit".account_current_level ALTER COLUMN monthly_other_iban_withdrawal_amount TYPE numeric(18,2);
    END IF;
END $$;

-- limit.account_current_level.monthly_on_us_payment_amount
DO $$
BEGIN
    IF EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_schema = 'limit' AND table_name = 'account_current_level' AND column_name = 'monthly_on_us_payment_amount'
        AND (data_type <> 'numeric' OR numeric_precision <> 18 OR numeric_scale <> 2)
    ) THEN
        ALTER TABLE "limit".account_current_level ALTER COLUMN monthly_on_us_payment_amount TYPE numeric(18,2);
    END IF;
END $$;

-- limit.account_current_level.monthly_international_transfer_amount
DO $$
BEGIN
    IF EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_schema = 'limit' AND table_name = 'account_current_level' AND column_name = 'monthly_international_transfer_amount'
        AND (data_type <> 'numeric' OR numeric_precision <> 18 OR numeric_scale <> 2)
    ) THEN
        ALTER TABLE "limit".account_current_level ALTER COLUMN monthly_international_transfer_amount TYPE numeric(18,2);
    END IF;
END $$;

-- limit.account_current_level.monthly_internal_transfer_amount
DO $$
BEGIN
    IF EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_schema = 'limit' AND table_name = 'account_current_level' AND column_name = 'monthly_internal_transfer_amount'
        AND (data_type <> 'numeric' OR numeric_precision <> 18 OR numeric_scale <> 2)
    ) THEN
        ALTER TABLE "limit".account_current_level ALTER COLUMN monthly_internal_transfer_amount TYPE numeric(18,2);
    END IF;
END $$;

-- limit.account_current_level.monthly_deposit_amount
DO $$
BEGIN
    IF EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_schema = 'limit' AND table_name = 'account_current_level' AND column_name = 'monthly_deposit_amount'
        AND (data_type <> 'numeric' OR numeric_precision <> 18 OR numeric_scale <> 2)
    ) THEN
        ALTER TABLE "limit".account_current_level ALTER COLUMN monthly_deposit_amount TYPE numeric(18,2);
    END IF;
END $$;

-- limit.account_current_level.daily_withdrawal_amount
DO $$
BEGIN
    IF EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_schema = 'limit' AND table_name = 'account_current_level' AND column_name = 'daily_withdrawal_amount'
        AND (data_type <> 'numeric' OR numeric_precision <> 18 OR numeric_scale <> 2)
    ) THEN
        ALTER TABLE "limit".account_current_level ALTER COLUMN daily_withdrawal_amount TYPE numeric(18,2);
    END IF;
END $$;

-- limit.account_current_level.daily_other_iban_withdrawal_amount
DO $$
BEGIN
    IF EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_schema = 'limit' AND table_name = 'account_current_level' AND column_name = 'daily_other_iban_withdrawal_amount'
        AND (data_type <> 'numeric' OR numeric_precision <> 18 OR numeric_scale <> 2)
    ) THEN
        ALTER TABLE "limit".account_current_level ALTER COLUMN daily_other_iban_withdrawal_amount TYPE numeric(18,2);
    END IF;
END $$;

-- limit.account_current_level.daily_on_us_payment_amount
DO $$
BEGIN
    IF EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_schema = 'limit' AND table_name = 'account_current_level' AND column_name = 'daily_on_us_payment_amount'
        AND (data_type <> 'numeric' OR numeric_precision <> 18 OR numeric_scale <> 2)
    ) THEN
        ALTER TABLE "limit".account_current_level ALTER COLUMN daily_on_us_payment_amount TYPE numeric(18,2);
    END IF;
END $$;

-- limit.account_current_level.daily_international_transfer_amount
DO $$
BEGIN
    IF EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_schema = 'limit' AND table_name = 'account_current_level' AND column_name = 'daily_international_transfer_amount'
        AND (data_type <> 'numeric' OR numeric_precision <> 18 OR numeric_scale <> 2)
    ) THEN
        ALTER TABLE "limit".account_current_level ALTER COLUMN daily_international_transfer_amount TYPE numeric(18,2);
    END IF;
END $$;

-- limit.account_current_level.daily_internal_transfer_amount
DO $$
BEGIN
    IF EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_schema = 'limit' AND table_name = 'account_current_level' AND column_name = 'daily_internal_transfer_amount'
        AND (data_type <> 'numeric' OR numeric_precision <> 18 OR numeric_scale <> 2)
    ) THEN
        ALTER TABLE "limit".account_current_level ALTER COLUMN daily_internal_transfer_amount TYPE numeric(18,2);
    END IF;
END $$;

-- limit.account_current_level.daily_deposit_amount
DO $$
BEGIN
    IF EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_schema = 'limit' AND table_name = 'account_current_level' AND column_name = 'daily_deposit_amount'
        AND (data_type <> 'numeric' OR numeric_precision <> 18 OR numeric_scale <> 2)
    ) THEN
        ALTER TABLE "limit".account_current_level ALTER COLUMN daily_deposit_amount TYPE numeric(18,2);
    END IF;
END $$;

-- core.account_activity.amount
DO $$
BEGIN
    IF EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_schema = 'core' AND table_name = 'account_activity' AND column_name = 'amount'
        AND (data_type <> 'numeric' OR numeric_precision <> 18 OR numeric_scale <> 2)
    ) THEN
        ALTER TABLE core.account_activity ALTER COLUMN amount TYPE numeric(18,2);
    END IF;
END $$;

-- core.return_transaction_request.amount
DO $$
BEGIN
    IF EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_schema = 'core' AND table_name = 'return_transaction_request' AND column_name = 'amount'
        AND (data_type <> 'numeric' OR numeric_precision <> 18 OR numeric_scale <> 2)
    ) THEN
        ALTER TABLE core.return_transaction_request ALTER COLUMN amount TYPE numeric(18,2);
    END IF;
END $$;

-- core.wallet_payment_request.amount
DO $$
BEGIN
    IF EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_schema = 'core' AND table_name = 'wallet_payment_request' AND column_name = 'amount'
        AND (data_type <> 'numeric' OR numeric_precision <> 18 OR numeric_scale <> 2)
    ) THEN
        ALTER TABLE core.wallet_payment_request ALTER COLUMN amount TYPE numeric(18,2);
    END IF;
END $$;
