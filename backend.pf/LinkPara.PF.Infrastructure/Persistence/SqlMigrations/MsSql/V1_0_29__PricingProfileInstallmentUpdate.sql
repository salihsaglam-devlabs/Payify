-- PricingProfile -> ProfileSettlementMode kolonu ekleme
IF OBJECT_ID('Core.PricingProfile','U') IS NOT NULL
AND NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('Core.PricingProfile')
      AND name = 'ProfileSettlementMode'
)
BEGIN
    ALTER TABLE Core.PricingProfile
    ADD ProfileSettlementMode VARCHAR(50) NOT NULL DEFAULT 'SingleBlock';
END
GO

-- CostProfile -> ProfileSettlementMode kolonu ekleme
IF OBJECT_ID('Core.CostProfile','U') IS NOT NULL
AND NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('Core.CostProfile')
      AND name = 'ProfileSettlementMode'
)
BEGIN
    ALTER TABLE Core.CostProfile
    ADD ProfileSettlementMode VARCHAR(50) NOT NULL DEFAULT 'SingleBlock';
END
GO

-- CostProfileInstallment tablosu
IF OBJECT_ID('Core.CostProfileInstallment','U') IS NULL
BEGIN
    CREATE TABLE Core.CostProfileInstallment (
        Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        InstallmentSequence INT NOT NULL,
        BlockedDayNumber INT NOT NULL,
        CostProfileItemId UNIQUEIDENTIFIER NOT NULL,
        CreateDate DATETIME2 NOT NULL,
        UpdateDate DATETIME2 NULL,
        CreatedBy VARCHAR(50) NOT NULL,
        LastModifiedBy VARCHAR(50) NULL,
        RecordStatus VARCHAR(50) NOT NULL,
        CONSTRAINT FK_CostProfileInstallment_CostProfileItem
            FOREIGN KEY (CostProfileItemId)
            REFERENCES Core.CostProfileItem (Id)
    );
END
GO

-- PricingProfileInstallment tablosu
IF OBJECT_ID('Core.PricingProfileInstallment','U') IS NULL
BEGIN
    CREATE TABLE Core.PricingProfileInstallment (
        Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        InstallmentSequence INT NOT NULL,
        BlockedDayNumber INT NOT NULL,
        PricingProfileItemId UNIQUEIDENTIFIER NOT NULL,
        CreateDate DATETIME2 NOT NULL,
        UpdateDate DATETIME2 NULL,
        CreatedBy VARCHAR(50) NOT NULL,
        LastModifiedBy VARCHAR(50) NULL,
        RecordStatus VARCHAR(50) NOT NULL,
        CONSTRAINT FK_PricingProfileInstallment_PricingProfileItem
            FOREIGN KEY (PricingProfileItemId)
            REFERENCES Core.PricingProfileItem (Id)
    );
END
GO

-- CostProfileInstallment index
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_CostProfileInstallment_CostProfileItemId_InstallmentSequence'
)
BEGIN
    CREATE UNIQUE INDEX IX_CostProfileInstallment_CostProfileItemId_InstallmentSequence
    ON Core.CostProfileInstallment (CostProfileItemId, InstallmentSequence);
END
GO

-- PricingProfileInstallment index
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_PricingProfileInstallment_PricingProfileItemId_InstallmentSequence'
)
BEGIN
    CREATE UNIQUE INDEX IX_PricingProfileInstallment_PricingProfileItemId_InstallmentSequence
    ON Core.PricingProfileInstallment (PricingProfileItemId, InstallmentSequence);
END
GO