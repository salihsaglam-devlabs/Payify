IF EXISTS (
    SELECT 1
    FROM sys.columns
    WHERE object_id = OBJECT_ID('Merchant.Pool')
      AND name = 'BankCode'
      AND is_nullable = 0
)
BEGIN
    ALTER TABLE Merchant.Pool
    ALTER COLUMN BankCode INT NULL;
END;