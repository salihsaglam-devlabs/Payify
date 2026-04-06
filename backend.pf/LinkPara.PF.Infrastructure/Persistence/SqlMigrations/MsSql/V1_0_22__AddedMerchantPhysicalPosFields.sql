IF NOT EXISTS (
    SELECT 1
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'Merchant'
      AND TABLE_NAME = 'MerchantPhysicalPos'
      AND COLUMN_NAME = 'BkmReferenceNumber'
)
BEGIN
    ALTER TABLE Merchant.MerchantPhysicalPos ADD BkmReferenceNumber VARCHAR(50);
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'Merchant'
      AND TABLE_NAME = 'MerchantPhysicalPos'
      AND COLUMN_NAME = 'TerminalStatus'
)
BEGIN
    ALTER TABLE Merchant.MerchantPhysicalPos ADD TerminalStatus VARCHAR(50) NOT NULL DEFAULT '';
END
GO