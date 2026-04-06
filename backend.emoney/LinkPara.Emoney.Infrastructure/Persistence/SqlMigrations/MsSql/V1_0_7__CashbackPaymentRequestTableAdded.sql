IF NOT EXISTS (
    SELECT * FROM INFORMATION_SCHEMA.TABLES 
    WHERE TABLE_SCHEMA = 'Core' AND TABLE_NAME = 'CashbackPaymentRequest'
)
BEGIN
    CREATE TABLE Core.CashbackPaymentRequest (
        Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        EntitlementId UNIQUEIDENTIFIER NOT NULL,
        CashbackPaymentStatus NVARCHAR(50) NOT NULL,
        TransactionId UNIQUEIDENTIFIER,
        WalletNumber NVARCHAR(30) NOT NULL,
        Amount DECIMAL(18,2) NOT NULL,
        CurrencyCode NVARCHAR(10) NOT NULL,
        CreateDate DATETIME NOT NULL,
        UpdateDate DATETIME NULL,
        CreatedBy NVARCHAR(50) NOT NULL,
        LastModifiedBy NVARCHAR(50) NULL,
        RecordStatus NVARCHAR(50) NOT NULL
    );
END