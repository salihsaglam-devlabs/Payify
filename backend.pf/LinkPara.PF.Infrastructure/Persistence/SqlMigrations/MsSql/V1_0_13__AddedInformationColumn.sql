
IF NOT EXISTS (
    SELECT 1
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'Merchant'
      AND TABLE_NAME = 'Merchant'
      AND COLUMN_NAME = 'Information'
)
BEGIN
    ALTER TABLE Merchant.Merchant ADD Information VARCHAR(200);
END;