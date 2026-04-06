-- merchant.vpos
IF NOT EXISTS (
    SELECT 1
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'Merchant'
      AND TABLE_NAME = 'Vpos'
      AND COLUMN_NAME = 'BkmReferenceNumber'
)
BEGIN
    ALTER TABLE Merchant.Vpos ADD BkmReferenceNumber VARCHAR(50);
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'Merchant'
      AND TABLE_NAME = 'Vpos'
      AND COLUMN_NAME = 'ServiceProviderPspMerchantId'
)
BEGIN
    ALTER TABLE Merchant.Vpos ADD ServiceProviderPspMerchantId VARCHAR(100);
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'Merchant'
      AND TABLE_NAME = 'Vpos'
      AND COLUMN_NAME = 'TerminalStatus'
)
BEGIN
    ALTER TABLE Merchant.Vpos ADD TerminalStatus VARCHAR(50) NOT NULL DEFAULT '';
END
GO

-- merchant.merchant
IF NOT EXISTS (
    SELECT 1
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'Merchant'
      AND TABLE_NAME = 'Merchant'
      AND COLUMN_NAME = 'HostingTradeName'
)
BEGIN
    ALTER TABLE Merchant.Merchant ADD HostingTradeName VARCHAR(150);
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'Merchant'
      AND TABLE_NAME = 'Merchant'
      AND COLUMN_NAME = 'HostingUrl'
)
BEGIN
    ALTER TABLE Merchant.Merchant ADD HostingUrl VARCHAR(150);
END
GO

-- bank.api_key
IF NOT EXISTS (
    SELECT 1
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'Bank'
      AND TABLE_NAME = 'ApiKey'
      AND COLUMN_NAME = 'IsPfMainMerchantId'
)
BEGIN
    ALTER TABLE Bank.ApiKey ADD IsPfMainMerchantId BIT NOT NULL DEFAULT 0;
END
GO

-- bank.acquire_bank
IF NOT EXISTS (
    SELECT 1
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'Bank'
      AND TABLE_NAME = 'AcquireBank'
      AND COLUMN_NAME = 'PaymentGwTaxNo'
)
BEGIN
    ALTER TABLE Bank.AcquireBank ADD PaymentGwTaxNo VARCHAR(11);
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'Bank'
      AND TABLE_NAME = 'AcquireBank'
      AND COLUMN_NAME = 'PaymentGwTradeName'
)
BEGIN
    ALTER TABLE Bank.AcquireBank ADD PaymentGwTradeName VARCHAR(150);
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'Bank'
      AND TABLE_NAME = 'AcquireBank'
      AND COLUMN_NAME = 'PaymentGwUrl'
)
BEGIN
    ALTER TABLE Bank.AcquireBank ADD PaymentGwUrl VARCHAR(150);
END
GO
