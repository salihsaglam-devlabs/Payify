IF NOT EXISTS (
    SELECT 1
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'Merchant'
      AND TABLE_NAME = 'MerchantPhysicalDevice'
      AND COLUMN_NAME = 'OwnerTerminalId'
)
BEGIN
ALTER TABLE Merchant.MerchantPhysicalDevice ADD OwnerTerminalId VARCHAR(50);
END