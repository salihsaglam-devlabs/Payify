IF NOT EXISTS (
    SELECT 1 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = 'Merchant'
      AND TABLE_NAME = 'Transaction'
      AND COLUMN_NAME = 'TransactionSource'
)
BEGIN
ALTER TABLE Merchant.[Transaction]
    ADD TransactionSource NVARCHAR(50) NOT NULL DEFAULT 'VirtualPos';
END;

IF NOT EXISTS (
    SELECT 1 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = 'Posting'
      AND TABLE_NAME = 'Transaction'
      AND COLUMN_NAME = 'TransactionSource'
)
BEGIN
ALTER TABLE Posting.[Transaction]
    ADD TransactionSource NVARCHAR(50) NOT NULL DEFAULT 'VirtualPos';
END;