DO $$
BEGIN
    IF EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_schema = 'merchant'
          AND table_name = 'merchant'
          AND column_name = 'is_payment_to_main_merchant'
    ) THEN
UPDATE merchant.merchant
SET is_payment_to_main_merchant = false
WHERE is_payment_to_main_merchant IS NULL;

ALTER TABLE merchant.merchant
    ALTER COLUMN is_payment_to_main_merchant SET DEFAULT false,
ALTER COLUMN is_payment_to_main_merchant SET NOT NULL;
END IF;

    IF EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_schema = 'merchant'
          AND table_name = 'pool'
          AND column_name = 'is_payment_to_main_merchant'
    ) THEN
UPDATE merchant.pool
SET is_payment_to_main_merchant = false
WHERE is_payment_to_main_merchant IS NULL;

ALTER TABLE merchant.pool
    ALTER COLUMN is_payment_to_main_merchant SET DEFAULT false,
ALTER COLUMN is_payment_to_main_merchant SET NOT NULL;
END IF;

    IF EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_schema = 'core'
          AND table_name = 'pricing_profile'
          AND column_name = 'is_payment_to_main_merchant'
    ) THEN
UPDATE core.pricing_profile
SET is_payment_to_main_merchant = false
WHERE is_payment_to_main_merchant IS NULL;

ALTER TABLE core.pricing_profile
    ALTER COLUMN is_payment_to_main_merchant SET DEFAULT false,
ALTER COLUMN is_payment_to_main_merchant SET NOT NULL;
END IF;
END$$;