IF NOT EXISTS (
    SELECT 1 
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'Core' 
      AND TABLE_NAME = 'TransferOrder' 
      AND COLUMN_NAME = 'PaymentType'
)
BEGIN
    ALTER TABLE Core.TransferOrder 
        ADD PaymentType VARCHAR(100) NULL;
END
