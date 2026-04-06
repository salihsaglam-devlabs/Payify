-- Add ExternalPersonId to Merchant.User
IF NOT EXISTS (
    SELECT 1
    FROM sys.columns
    WHERE Name = 'ExternalPersonId'
      AND Object_ID = OBJECT_ID('Merchant.[User]')
)
BEGIN
ALTER TABLE Merchant.[User]
    ADD ExternalPersonId UNIQUEIDENTIFIER NOT NULL
    CONSTRAINT DF_User_ExternalPersonId DEFAULT ('00000000-0000-0000-0000-000000000000');
END
GO

-- Add ExternalCustomerId to Core.Customer
IF NOT EXISTS (
    SELECT 1
    FROM sys.columns
    WHERE Name = 'ExternalCustomerId'
      AND Object_ID = OBJECT_ID('Core.Customer')
)
BEGIN
ALTER TABLE Core.Customer
    ADD ExternalCustomerId UNIQUEIDENTIFIER NOT NULL
    CONSTRAINT DF_Customer_ExternalCustomerId DEFAULT ('00000000-0000-0000-0000-000000000000');
END
GO

-- Add ExternalPersonId to Core.ContactPerson
IF NOT EXISTS (
    SELECT 1
    FROM sys.columns
    WHERE Name = 'ExternalPersonId'
      AND Object_ID = OBJECT_ID('Core.ContactPerson')
)
BEGIN
ALTER TABLE Core.ContactPerson
    ADD ExternalPersonId UNIQUEIDENTIFIER NOT NULL
    CONSTRAINT DF_ContactPerson_ExternalPersonId DEFAULT ('00000000-0000-0000-0000-000000000000');
END
GO
