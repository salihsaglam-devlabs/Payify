
IF NOT EXISTS (
    SELECT * FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'Core' AND TABLE_NAME = 'WalletBalanceDaily' AND COLUMN_NAME = 'Difference'
)
BEGIN
    ALTER TABLE Core.WalletBalanceDaily ADD Difference decimal(18,2) NULL;
END
