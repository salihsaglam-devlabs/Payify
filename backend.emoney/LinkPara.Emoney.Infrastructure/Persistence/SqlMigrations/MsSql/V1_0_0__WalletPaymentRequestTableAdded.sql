
IF OBJECT_ID('Core.WalletPaymentRequest', 'U') IS NULL
BEGIN
    CREATE TABLE Core.WalletPaymentRequest (
        Id UNIQUEIDENTIFIER NOT NULL,
        PaymentReferenceId NVARCHAR(50) NOT NULL,
        InternalTransactionId UNIQUEIDENTIFIER NOT NULL,
        Status NVARCHAR(50) NOT NULL,
        Amount DECIMAL(18,4) NOT NULL,
        CurrencyCode NVARCHAR(3) NOT NULL,
        SenderWalletNo NVARCHAR(20) NOT NULL,
        SenderName NVARCHAR(150) NOT NULL,
        ReceiverWalletNo NVARCHAR(20) NOT NULL,
        ReceiverName NVARCHAR(150) NOT NULL,
        TransactionDate DATETIME2 NOT NULL,
        IsLoggedIn BIT NOT NULL DEFAULT 0,
        CreateDate DATETIME2 NOT NULL,
        UpdateDate DATETIME2 NULL,
        CreatedBy NVARCHAR(50) NOT NULL,
        LastModifiedBy NVARCHAR(50) NULL,
        RecordStatus NVARCHAR(50) NOT NULL,
        CONSTRAINT PK_WalletPaymentRequest PRIMARY KEY (Id)
    );
END
 
 
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_WalletPaymentRequest_PaymentReferenceId' AND object_id = OBJECT_ID('Core.WalletPaymentRequest'))
BEGIN
    CREATE INDEX IX_WalletPaymentRequest_PaymentReferenceId 
    ON Core.WalletPaymentRequest (PaymentReferenceId);
END