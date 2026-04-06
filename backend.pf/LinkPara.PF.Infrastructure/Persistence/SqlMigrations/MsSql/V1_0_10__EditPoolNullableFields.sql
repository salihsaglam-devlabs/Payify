IF EXISTS (
    SELECT 1
    FROM sys.foreign_keys
    WHERE name = 'FK_Pool_Bank_BankCode'
      AND parent_object_id = OBJECT_ID('Merchant.Pool')
)
BEGIN
    ALTER TABLE Merchant.Pool
    DROP CONSTRAINT FK_Pool_Bank_BankCode;
END;

IF EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'IX_Pool_BankCode'
      AND object_id = OBJECT_ID('Merchant.Pool')
)
BEGIN
    DROP INDEX IX_Pool_BankCode ON Merchant.Pool;
END;

IF EXISTS (
    SELECT 1
    FROM sys.columns
    WHERE object_id = OBJECT_ID('Merchant.Pool')
      AND name = 'Iban'
      AND is_nullable = 0
)
BEGIN
    ALTER TABLE Merchant.Pool
    ALTER COLUMN Iban varchar(26) NULL;
END;

IF EXISTS (
    SELECT 1
    FROM sys.columns
    WHERE object_id = OBJECT_ID('Merchant.MerchantReturnPool')
      AND name = 'BankName'
      AND is_nullable = 0
)
BEGIN
    ALTER TABLE Merchant.MerchantReturnPool
    ALTER COLUMN BankName varchar(100) NULL;
END;