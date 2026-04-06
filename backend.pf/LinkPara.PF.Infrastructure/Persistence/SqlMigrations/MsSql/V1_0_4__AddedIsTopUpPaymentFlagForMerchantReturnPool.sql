IF NOT EXISTS (
    SELECT 1
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'Merchant'
      AND TABLE_NAME = 'MerchantReturnPool'
      AND COLUMN_NAME = 'IsTopUpPayment'
)
BEGIN
    ALTER TABLE Merchant.MerchantReturnPool ADD IsTopUpPayment BIT;
END
GO