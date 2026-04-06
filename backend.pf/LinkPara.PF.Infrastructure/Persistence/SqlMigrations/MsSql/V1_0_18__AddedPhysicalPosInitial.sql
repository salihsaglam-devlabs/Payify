BEGIN TRANSACTION;

-------------------------------------------------
-- SCHEMA: Physical
-------------------------------------------------
IF NOT EXISTS (
    SELECT 1
    FROM sys.schemas
    WHERE name = 'Physical'
)
BEGIN
    EXEC('CREATE SCHEMA Physical');
END;

-------------------------------------------------
-- Core.CostProfile : VposId nullable
-------------------------------------------------
IF EXISTS (
    SELECT 1
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'Core'
      AND TABLE_NAME = 'CostProfile'
      AND COLUMN_NAME = 'VposId'
      AND IS_NULLABLE = 'NO'
)
BEGIN
    ALTER TABLE Core.CostProfile
    ALTER COLUMN VposId uniqueidentifier NULL;
END;

-------------------------------------------------
-- Core.CostProfile : PhysicalPosId
-------------------------------------------------
IF NOT EXISTS (
    SELECT 1
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'Core'
      AND TABLE_NAME = 'CostProfile'
      AND COLUMN_NAME = 'PhysicalPosId'
)
BEGIN
    ALTER TABLE Core.CostProfile
    ADD PhysicalPosId uniqueidentifier NULL;
END;

-------------------------------------------------
-- Core.CostProfile : PosType
-------------------------------------------------
IF NOT EXISTS (
    SELECT 1
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'Core'
      AND TABLE_NAME = 'CostProfile'
      AND COLUMN_NAME = 'PosType'
)
BEGIN
    ALTER TABLE Core.CostProfile
    ADD PosType varchar(50) NOT NULL DEFAULT 'Virtual';
END;

-------------------------------------------------
-- Physical.DeviceInventory
-------------------------------------------------
IF NOT EXISTS (
    SELECT 1
    FROM INFORMATION_SCHEMA.TABLES
    WHERE TABLE_SCHEMA = 'Physical'
      AND TABLE_NAME = 'DeviceInventory'
)
BEGIN
    CREATE TABLE Physical.DeviceInventory (
        Id uniqueidentifier NOT NULL,
        SerialNo varchar(200),
        ContactlessSeparator varchar(50) NOT NULL,
        PhysicalPosVendor varchar(50) NOT NULL,
        DeviceModel varchar(50) NOT NULL,
        DeviceStatus varchar(50) NOT NULL,
        DeviceType varchar(50) NOT NULL,
        InventoryType varchar(50) NOT NULL,
        CreateDate datetime2 NOT NULL,
        UpdateDate datetime2,
        CreatedBy varchar(50) NOT NULL,
        LastModifiedBy varchar(50),
        RecordStatus varchar(50) NOT NULL,
        CONSTRAINT PK_DeviceInventory PRIMARY KEY (Id)
    );
END;

-------------------------------------------------
-- Physical.PhysicalPos
-------------------------------------------------
IF NOT EXISTS (
    SELECT 1
    FROM INFORMATION_SCHEMA.TABLES
    WHERE TABLE_SCHEMA = 'Physical'
      AND TABLE_NAME = 'PhysicalPos'
)
BEGIN
    CREATE TABLE Physical.PhysicalPos (
        Id uniqueidentifier NOT NULL,
        Name varchar(200),
        VPosStatus varchar(50) NOT NULL,
        AcquireBankId uniqueidentifier NOT NULL,
        VPosType varchar(50) NOT NULL,
        PfMainMerchantId varchar(200),
        CreateDate datetime2 NOT NULL,
        UpdateDate datetime2,
        CreatedBy varchar(50) NOT NULL,
        LastModifiedBy varchar(50),
        RecordStatus varchar(50) NOT NULL,
        CONSTRAINT PK_PhysicalPos PRIMARY KEY (Id)
    );
END;

-------------------------------------------------
-- FK: PhysicalPos → AcquireBank
-------------------------------------------------
IF NOT EXISTS (
    SELECT 1
    FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS
    WHERE CONSTRAINT_NAME = 'FK_PhysicalPos_AcquireBank'
)
BEGIN
    ALTER TABLE Physical.PhysicalPos
    ADD CONSTRAINT FK_PhysicalPos_AcquireBank
    FOREIGN KEY (AcquireBankId)
    REFERENCES Bank.AcquireBank (Id)
    ON DELETE CASCADE;
END;

-------------------------------------------------
-- Merchant.MerchantPhysicalDevice
-------------------------------------------------
IF NOT EXISTS (
    SELECT 1
    FROM INFORMATION_SCHEMA.TABLES
    WHERE TABLE_SCHEMA = 'Merchant'
      AND TABLE_NAME = 'MerchantPhysicalDevice'
)
BEGIN
    CREATE TABLE Merchant.MerchantPhysicalDevice (
        Id uniqueidentifier NOT NULL,
        OwnerPspNo varchar(100),
        IsPinPad bit NOT NULL,
        ConnectionType varchar(50) NOT NULL,
        AssignmentType varchar(50) NOT NULL,
        FiscalNo varchar(200),
        MerchantId uniqueidentifier NOT NULL,
        DeviceInventoryId uniqueidentifier NOT NULL,
        CreateDate datetime2 NOT NULL,
        UpdateDate datetime2,
        CreatedBy varchar(50) NOT NULL,
        LastModifiedBy varchar(50),
        RecordStatus varchar(50) NOT NULL,
        CONSTRAINT PK_MerchantPhysicalDevice PRIMARY KEY (Id)
    );
END;

-------------------------------------------------
-- FK: MerchantPhysicalDevice → DeviceInventory
-------------------------------------------------
IF NOT EXISTS (
    SELECT 1
    FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS
    WHERE CONSTRAINT_NAME = 'FK_MerchantPhysicalDevice_DeviceInventory'
)
BEGIN
    ALTER TABLE Merchant.MerchantPhysicalDevice
    ADD CONSTRAINT FK_MerchantPhysicalDevice_DeviceInventory
    FOREIGN KEY (DeviceInventoryId)
    REFERENCES Physical.DeviceInventory (Id)
    ON DELETE CASCADE;
END;

-------------------------------------------------
-- FK: MerchantPhysicalDevice → Merchant
-------------------------------------------------
IF NOT EXISTS (
    SELECT 1
    FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS
    WHERE CONSTRAINT_NAME = 'FK_MerchantPhysicalDevice_Merchant'
)
BEGIN
    ALTER TABLE Merchant.MerchantPhysicalDevice
    ADD CONSTRAINT FK_MerchantPhysicalDevice_Merchant
    FOREIGN KEY (MerchantId)
    REFERENCES Merchant.Merchant (Id)
    ON DELETE CASCADE;
END;

-------------------------------------------------
-- Physical.PhysicalPosCurrency
-------------------------------------------------
IF NOT EXISTS (
    SELECT 1
    FROM INFORMATION_SCHEMA.TABLES
    WHERE TABLE_SCHEMA = 'Physical'
      AND TABLE_NAME = 'PhysicalPosCurrency'
)
BEGIN
    CREATE TABLE Physical.PhysicalPosCurrency (
        Id uniqueidentifier NOT NULL,
        PhysicalPosId uniqueidentifier NOT NULL,
        CurrencyCode varchar(10),
        CreateDate datetime2 NOT NULL,
        UpdateDate datetime2,
        CreatedBy varchar(50) NOT NULL,
        LastModifiedBy varchar(50),
        RecordStatus varchar(50) NOT NULL,
        CONSTRAINT PK_PhysicalPosCurrency PRIMARY KEY (Id)
    );
END;

-------------------------------------------------
-- Merchant.MerchantDeviceApiKey
-------------------------------------------------
IF NOT EXISTS (
    SELECT 1
    FROM INFORMATION_SCHEMA.TABLES
    WHERE TABLE_SCHEMA = 'Merchant'
      AND TABLE_NAME = 'MerchantDeviceApiKey'
)
BEGIN
    CREATE TABLE Merchant.MerchantDeviceApiKey (
        Id uniqueidentifier NOT NULL,
        PublicKey varchar(100) NOT NULL,
        PrivateKeyEncrypted varchar(100) NOT NULL,
        MerchantPhysicalDeviceId uniqueidentifier NOT NULL,
        CreateDate datetime2 NOT NULL,
        UpdateDate datetime2,
        CreatedBy varchar(50) NOT NULL,
        LastModifiedBy varchar(50),
        RecordStatus varchar(50) NOT NULL,
        CONSTRAINT PK_MerchantDeviceApiKey PRIMARY KEY (Id)
    );
END;

-------------------------------------------------
-- Merchant.MerchantPhysicalPos
-------------------------------------------------
IF NOT EXISTS (
    SELECT 1
    FROM INFORMATION_SCHEMA.TABLES
    WHERE TABLE_SCHEMA = 'Merchant'
      AND TABLE_NAME = 'MerchantPhysicalPos'
)
BEGIN
    CREATE TABLE Merchant.MerchantPhysicalPos (
        Id uniqueidentifier NOT NULL,
        MerchantPhysicalDeviceId uniqueidentifier NOT NULL,
        PhysicalPosId uniqueidentifier NOT NULL,
        CreateDate datetime2 NOT NULL,
        UpdateDate datetime2,
        CreatedBy varchar(50) NOT NULL,
        LastModifiedBy varchar(50),
        RecordStatus varchar(50) NOT NULL,
        CONSTRAINT PK_MerchantPhysicalPos PRIMARY KEY (Id)
    );
END;

-------------------------------------------------
-- INDEX: CostProfile.PhysicalPosId
-------------------------------------------------
IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'IX_CostProfile_PhysicalPosId'
)
BEGIN
    CREATE INDEX IX_CostProfile_PhysicalPosId
    ON Core.CostProfile (PhysicalPosId);
END;

-------------------------------------------------
-- FK: CostProfile → PhysicalPos
-------------------------------------------------
IF NOT EXISTS (
    SELECT 1
    FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS
    WHERE CONSTRAINT_NAME = 'FK_CostProfile_PhysicalPos'
)
BEGIN
    ALTER TABLE Core.CostProfile
    ADD CONSTRAINT FK_CostProfile_PhysicalPos
    FOREIGN KEY (PhysicalPosId)
    REFERENCES Physical.PhysicalPos (Id)
    ON DELETE NO ACTION;
END;

COMMIT;