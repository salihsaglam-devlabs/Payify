IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Core')
BEGIN
    EXEC('CREATE SCHEMA Core');
END
GO
IF NOT EXISTS (
    SELECT 1 
    FROM sys.tables t
    JOIN sys.schemas s ON t.schema_id = s.schema_id
    WHERE t.name = 'CustomerWalletCard' AND s.name = 'Core'
)
BEGIN
    CREATE TABLE Core.CustomerWalletCard (
        Id UNIQUEIDENTIFIER NOT NULL,
        BankingCustomerNo VARCHAR(16),
        WalletNumber VARCHAR(50),
        CardNumber VARCHAR(50),
        ProductCode VARCHAR(50),
        UserId UNIQUEIDENTIFIER NOT NULL,
        IsActive BIT NOT NULL,
        CreateDate DATETIME2 NOT NULL,
        UpdateDate DATETIME2 NULL,
        CreatedBy VARCHAR(50) NOT NULL,
        LastModifiedBy VARCHAR(50),
        RecordStatus VARCHAR(50) NOT NULL,
        CONSTRAINT PK_CustomerWalletCard PRIMARY KEY (Id)
    );
END
GO

IF NOT EXISTS (
    SELECT 1 
    FROM sys.tables t
    JOIN sys.schemas s ON t.schema_id = s.schema_id
    WHERE t.name = 'DebitAuthorization' AND s.name = 'Core'
)
BEGIN
    CREATE TABLE Core.DebitAuthorization (
        Id UNIQUEIDENTIFIER NOT NULL,
        CorrelationId BIGINT NOT NULL,
        OceanTxnGuid BIGINT NOT NULL,
        BankingCustomerNo VARCHAR(MAX),
        CardNo VARCHAR(MAX),
        AccountNo VARCHAR(MAX),
        AccountBranch VARCHAR(MAX),
        AccountSuffix VARCHAR(MAX),
        AccountCurrency INT,
        Iban VARCHAR(MAX),
        AcquirerCountryCode VARCHAR(MAX),
        NationalSwitchId VARCHAR(MAX),
        AcquirerId VARCHAR(MAX),
        TerminalId VARCHAR(MAX),
        MerchantId VARCHAR(MAX),
        MerchantName VARCHAR(MAX),
        Rrn VARCHAR(MAX),
        ProvisionCode VARCHAR(MAX),
        TransactionAmount DECIMAL(18,2) NOT NULL,
        TransactionCurrency INT NOT NULL,
        BillingAmount DECIMAL(18,2) NOT NULL,
        BillingCurrency INT NOT NULL,
        ReplacementTransactionAmount DECIMAL(18,2),
        ReplacementTransactionCurrency INT,
        ReplacementBillingAmount DECIMAL(18,2),
        ReplacementBillingCurrency INT,
        RequestDate BIGINT NOT NULL,
        RequestTime BIGINT NOT NULL,
        Mcc VARCHAR(MAX),
        IsSimulation BIT NOT NULL,
        IsAdvice BIT NOT NULL,
        RequestType VARCHAR(MAX),
        TransactionType VARCHAR(MAX),
        ExpirationTime INT,
        Channel VARCHAR(MAX),
        TerminalType VARCHAR(MAX),
        BankingRefNo VARCHAR(MAX),
        TransactionSource CHAR(1) NOT NULL,
        CardDci CHAR(1) NOT NULL,
        CardBrand CHAR(1) NOT NULL,
        EntryType CHAR(1) NOT NULL,
        PartialAcceptor BIT,
        TransferInformationType CHAR(1),
        TransferInformationName VARCHAR(MAX),
        TransferInformationCardNo VARCHAR(MAX),
        BusinessApplicationIdentifier VARCHAR(MAX),
        QrData VARCHAR(MAX),
        SecurityLevelIndicator INT,
        IsReturn BIT NOT NULL,
        CreateDate DATETIME2 NOT NULL,
        UpdateDate DATETIME2 NULL,
        CreatedBy VARCHAR(50) NOT NULL,
        LastModifiedBy VARCHAR(50),
        RecordStatus VARCHAR(50) NOT NULL,
        CONSTRAINT PK_DebitAuthorization PRIMARY KEY (Id)
    );
END
GO

IF NOT EXISTS (
    SELECT 1 
    FROM sys.tables t
    JOIN sys.schemas s ON t.schema_id = s.schema_id
    WHERE t.name = 'DebitAuthorizationFee' AND s.name = 'Core'
)
BEGIN
    CREATE TABLE core.debit_authorization_fee (
    id UNIQUEIDENTIFIER NOT NULL,
    ocean_txn_guid BIGINT NOT NULL,
    type NVARCHAR(MAX),
    amount DECIMAL(18, 2) NOT NULL,
    currency_code INT NOT NULL,
    tax1amount DECIMAL(18, 2),
    tax2amount DECIMAL(18, 2),
    create_date DATETIME2 NOT NULL,
    update_date DATETIME2 NULL,
    created_by NVARCHAR(50) NOT NULL,
    last_modified_by NVARCHAR(50) NULL,
    record_status NVARCHAR(50) NOT NULL,
    CONSTRAINT pk_debit_authorization_fee PRIMARY KEY (id)
);
END
GO