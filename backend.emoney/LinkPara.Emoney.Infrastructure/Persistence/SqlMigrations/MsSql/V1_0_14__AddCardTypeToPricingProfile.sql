IF NOT EXISTS (
    SELECT 1 
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'Core' 
      AND TABLE_NAME = 'PricingProfile' 
      AND COLUMN_NAME = 'CardType'
)
BEGIN
    ALTER TABLE Core.PricingProfile 
        ADD CardType NVARCHAR(50) NULL;
END
