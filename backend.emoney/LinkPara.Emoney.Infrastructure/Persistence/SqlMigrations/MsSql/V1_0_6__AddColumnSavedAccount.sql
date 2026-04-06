IF NOT EXISTS (
    SELECT 1 
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'Core' 
      AND TABLE_NAME = 'SavedAccount' 
      AND COLUMN_NAME = 'ReceiverName'
)
BEGIN
    ALTER TABLE Core.SavedAccount 
        ADD ReceiverName VARCHAR(200) NULL;
END
