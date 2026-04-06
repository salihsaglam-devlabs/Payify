IF NOT EXISTS (
 SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
 WHERE TABLE_SCHEMA = 'Merchant'
   AND TABLE_NAME = 'BusinessPartner'
   AND COLUMN_NAME = 'AmlReferenceNumber'
) 
BEGIN
    ALTER TABLE Merchant.[BusinessPartner] ADD AmlReferenceNumber VARCHAR(150);
END;