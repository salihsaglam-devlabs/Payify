IF NOT EXISTS (
    SELECT 1
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'Merchant'
      AND TABLE_NAME = 'Transaction'
      AND COLUMN_NAME = 'IsInsurancePayment'
)
BEGIN
ALTER TABLE Merchant.[Transaction] ADD IsInsurancePayment BIT NOT NULL DEFAULT 0;
END

IF NOT EXISTS (
    SELECT 1
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'Merchant'
      AND TABLE_NAME = 'Merchant'
      AND COLUMN_NAME = 'InsurancePaymentAllowed'
)
BEGIN
ALTER TABLE Merchant.[Merchant] ADD InsurancePaymentAllowed BIT NOT NULL DEFAULT 0;
END

IF NOT EXISTS (
    SELECT 1
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'Vpos'
      AND TABLE_NAME = 'Vpos'
      AND COLUMN_NAME = 'IsInsuranceVpos'
)
BEGIN
ALTER TABLE Vpos.[Vpos] ADD IsInsuranceVpos BIT NOT NULL DEFAULT 0;
END

IF NOT EXISTS (
    SELECT 1
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'Merchant'
      AND TABLE_NAME = 'Transaction'
      AND COLUMN_NAME = 'CardHolderIdentityNumber'
)
BEGIN
ALTER TABLE Merchant.[Transaction] ADD CardHolderIdentityNumber VARCHAR(15);
END