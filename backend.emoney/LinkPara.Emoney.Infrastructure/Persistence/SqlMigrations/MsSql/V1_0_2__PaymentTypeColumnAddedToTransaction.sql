IF OBJECT_ID('dbo.WalletBalanceDaily') IS NOT NULL AND OBJECT_ID('Core.WalletBalanceDaily') IS NULL
BEGIN
    ALTER SCHEMA Core TRANSFER dbo.WalletBalanceDaily;
END

IF NOT EXISTS (
    SELECT 1
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'Core'
      AND TABLE_NAME = 'Transaction'
      AND COLUMN_NAME = 'PaymentType'
)
BEGIN
    ALTER TABLE Core.[Transaction]
    ADD PaymentType VARCHAR(100);
END;

