IF NOT EXISTS (
    SELECT 1 
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'Core' 
      AND TABLE_NAME = 'Account' 
      AND COLUMN_NAME = 'DeclarationStatus'
)
BEGIN
    ALTER TABLE Core.Account 
        ADD DeclarationStatus VARCHAR(20) NULL;
END
