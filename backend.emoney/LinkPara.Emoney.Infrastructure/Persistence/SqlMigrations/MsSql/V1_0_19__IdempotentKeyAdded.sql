IF NOT EXISTS (
    SELECT 1 
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'Core' 
      AND TABLE_NAME = 'Transaction' 
      AND COLUMN_NAME = 'IdempotentKey'
)
BEGIN
    ALTER TABLE Core.[Transaction]
        ADD IdempotentKey VARCHAR(100) NULL;
END
