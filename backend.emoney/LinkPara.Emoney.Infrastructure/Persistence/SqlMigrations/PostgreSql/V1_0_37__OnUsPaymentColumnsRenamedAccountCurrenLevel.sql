
DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'limit' AND table_name = 'account_current_level' AND column_name = 'monthly_on_us_payment_count') THEN
        ALTER TABLE "limit".account_current_level RENAME COLUMN monthly_max_on_us_payment_count TO monthly_on_us_payment_count;
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'limit' AND table_name = 'account_current_level' AND column_name = 'monthly_on_us_payment_amount') THEN
        ALTER TABLE "limit".account_current_level RENAME COLUMN monthly_max_on_us_payment_amount TO monthly_on_us_payment_amount;
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'limit' AND table_name = 'account_current_level' AND column_name = 'daily_on_us_payment_count') THEN
        ALTER TABLE "limit".account_current_level RENAME COLUMN daily_max_on_us_payment_count TO daily_on_us_payment_count;
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'limit' AND table_name = 'account_current_level' AND column_name = 'daily_on_us_payment_amount') THEN
        ALTER TABLE "limit".account_current_level RENAME COLUMN daily_max_on_us_payment_amount TO daily_on_us_payment_amount;
    END IF;
END $$;

