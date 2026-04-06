IF EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'Core' AND TABLE_NAME = 'WalletBlockageDocument' AND COLUMN_NAME = 'WalletBlockageId'
)
BEGIN
    ALTER TABLE Core.WalletBlockageDocument DROP COLUMN WalletBlockageId ;
END
GO