IF NOT EXISTS (
    SELECT * FROM INFORMATION_SCHEMA.TABLES 
    WHERE TABLE_SCHEMA = 'Core' AND TABLE_NAME = 'WalletBlockage'
)
BEGIN
    CREATE TABLE Core.WalletBlockage (
        Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        WalletId UNIQUEIDENTIFIER NOT NULL,
        WalletNumber NVARCHAR(20) NOT NULL,
        AccountName NVARCHAR(100) NOT NULL,
        WalletCurrencyCode NVARCHAR(5) NOT NULL,
        CashBlockageAmount DECIMAL(18,2) NOT NULL,
        CashCreditBlockageAmount DECIMAL(18,2) NOT NULL,
        OperationType NVARCHAR(50) NOT NULL,
        BlockageStatus NVARCHAR(20) NOT NULL,
        BlockageDescription NVARCHAR(1000) NULL,
        BlockageStartDate DATETIME NOT NULL,
        BlockageEndDate DATETIME NULL,        
        CreateDate DATETIME NOT NULL,
        UpdateDate DATETIME NULL,
        CreatedBy NVARCHAR(50) NOT NULL,
        LastModifiedBy NVARCHAR(50) NULL,
        RecordStatus NVARCHAR(50) NOT NULL
    );
END


IF NOT EXISTS (
    SELECT * FROM INFORMATION_SCHEMA.TABLES 
    WHERE TABLE_SCHEMA = 'Core' AND TABLE_NAME = 'WalletBlockageDocument'
)
BEGIN
    CREATE TABLE Core.WalletBlockageDocument (
        Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        WalletBlockageId UNIQUEIDENTIFIER NOT NULL,
        WalletId UNIQUEIDENTIFIER NOT NULL,
        DocumentId UNIQUEIDENTIFIER NOT NULL,
        DocumentTypeId UNIQUEIDENTIFIER NOT NULL,
        Description NVARCHAR(1000) NULL,
        FileName NVARCHAR(200) NULL,        
        CreateDate DATETIME NOT NULL,
        UpdateDate DATETIME NULL,
        CreatedBy NVARCHAR(50) NOT NULL,
        LastModifiedBy NVARCHAR(50) NULL,
        RecordStatus NVARCHAR(50) NOT NULL
    );
END
