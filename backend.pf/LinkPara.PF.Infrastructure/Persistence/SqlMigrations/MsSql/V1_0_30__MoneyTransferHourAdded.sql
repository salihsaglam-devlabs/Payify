IF NOT EXISTS (
    SELECT 1
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'Merchant'
      AND TABLE_NAME = 'Merchant'
      AND COLUMN_NAME = 'MoneyTransferStartHour'
)
BEGIN
    ALTER TABLE Merchant.Merchant ADD MoneyTransferStartHour INT;
END

IF NOT EXISTS (
    SELECT 1
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'Merchant'
      AND TABLE_NAME = 'Merchant'
      AND COLUMN_NAME = 'MoneyTransferStartMinute'
)
BEGIN
    ALTER TABLE Merchant.Merchant ADD MoneyTransferStartMinute INT;
END

IF NOT EXISTS (
    SELECT 1
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'Merchant'
      AND TABLE_NAME = 'Pool'
      AND COLUMN_NAME = 'MoneyTransferStartHour'
)
BEGIN
    ALTER TABLE Merchant.Pool ADD MoneyTransferStartHour INT;
END

IF NOT EXISTS (
    SELECT 1
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'Merchant'
      AND TABLE_NAME = 'Pool'
      AND COLUMN_NAME = 'MoneyTransferStartMinute'
)
BEGIN
    ALTER TABLE Merchant.Pool ADD MoneyTransferStartMinute INT;
END