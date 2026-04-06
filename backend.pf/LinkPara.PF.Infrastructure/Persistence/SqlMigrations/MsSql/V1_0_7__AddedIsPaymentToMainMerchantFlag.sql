IF NOT EXISTS (
 SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
 WHERE TABLE_SCHEMA = 'Merchant'
   AND TABLE_NAME = 'Merchant'
   AND COLUMN_NAME = 'IsPaymentToMainMerchant'
) 
BEGIN
    ALTER TABLE Merchant.[Merchant] ADD IsPaymentToMainMerchant BIT;
END;

IF NOT EXISTS (
  SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
  WHERE TABLE_SCHEMA = 'Merchant'
    AND TABLE_NAME = 'Pool'
    AND COLUMN_NAME = 'IsPaymentToMainMerchant'
) 
BEGIN
    ALTER TABLE PF.Merchant.Pool ADD IsPaymentToMainMerchant BIT;
END;

IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = 'Core'
      AND TABLE_NAME = 'PricingProfile'
      AND COLUMN_NAME = 'IsPaymentToMainMerchant'
)
BEGIN
    ALTER TABLE PF.Core.PricingProfile ADD IsPaymentToMainMerchant BIT;
END;