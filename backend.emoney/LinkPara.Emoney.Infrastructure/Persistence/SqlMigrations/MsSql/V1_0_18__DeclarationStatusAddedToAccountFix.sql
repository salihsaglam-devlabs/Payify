IF NOT EXISTS (
    SELECT 1
    FROM sys.default_constraints dc
    INNER JOIN sys.columns c 
        ON dc.parent_object_id = c.object_id 
        AND dc.parent_column_id = c.column_id
    WHERE dc.parent_object_id = OBJECT_ID('Core.Account')
      AND c.name = 'DeclarationStatus'
)
BEGIN
    ALTER TABLE Core.Account
    ADD CONSTRAINT DF_Account_DeclarationStatus
        DEFAULT ('None') FOR DeclarationStatus;
END


UPDATE Core.Account
SET DeclarationStatus = 'None'
WHERE DeclarationStatus IS NULL;

-------

IF NOT EXISTS (
    SELECT 1
    FROM sys.default_constraints dc
    INNER JOIN sys.columns c 
        ON dc.parent_object_id = c.object_id 
        AND dc.parent_column_id = c.column_id
    WHERE dc.parent_object_id = OBJECT_ID('Core.PricingProfile')
      AND c.name = 'CardType'
)
BEGIN
    ALTER TABLE Core.PricingProfile
    ADD CONSTRAINT DF_PricingProfile_CardType
        DEFAULT ('') FOR CardType;
END


UPDATE Core.PricingProfile
SET CardType = ''
WHERE CardType IS NULL;


-------------

ALTER TABLE Core.TransferOrder
ALTER COLUMN PaymentType NVARCHAR(100) NULL;

