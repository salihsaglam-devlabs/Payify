/* Ensure schema exists */
IF SCHEMA_ID('Physical') IS NULL
    EXEC('CREATE SCHEMA Physical');
GO

/* ---------------- COLUMN RENAMES ---------------- */

IF COL_LENGTH('Posting.Transaction','TransactionSource') IS NOT NULL
AND COL_LENGTH('Posting.Transaction','PfTransactionSource') IS NULL
BEGIN
EXEC sp_rename 'Posting.Transaction.TransactionSource','PfTransactionSource','COLUMN';
END
GO

IF COL_LENGTH('Merchant.Transaction','TransactionSource') IS NOT NULL
AND COL_LENGTH('Merchant.Transaction','PfTransactionSource') IS NULL
BEGIN
EXEC sp_rename 'Merchant.Transaction.TransactionSource','PfTransactionSource','COLUMN';
END
GO

/* ---------------- POSTING.TRANSACTION ---------------- */

IF COL_LENGTH('Posting.Transaction','MerchantPhysicalPosId') IS NULL
BEGIN
ALTER TABLE Posting.[Transaction]
    ADD MerchantPhysicalPosId UNIQUEIDENTIFIER NOT NULL
    CONSTRAINT DF_Posting_Transaction_MerchantPhysicalPosId
    DEFAULT CONVERT(uniqueidentifier,'00000000-0000-0000-0000-000000000000');
END
GO

/* ---------------- MERCHANT.TRANSACTION ---------------- */

IF COL_LENGTH('Merchant.Transaction','EndOfDayStatus') IS NULL
BEGIN
ALTER TABLE Merchant.[Transaction]
    ADD EndOfDayStatus VARCHAR(50) NOT NULL
    CONSTRAINT DF_Merchant_Transaction_EndOfDayStatus DEFAULT ('Pending');
END
GO

IF COL_LENGTH('Merchant.Transaction','MerchantPhysicalPosId') IS NULL
BEGIN
ALTER TABLE Merchant.[Transaction]
    ADD MerchantPhysicalPosId UNIQUEIDENTIFIER NOT NULL
    CONSTRAINT DF_Merchant_Transaction_MerchantPhysicalPosId
    DEFAULT CONVERT(uniqueidentifier,'00000000-0000-0000-0000-000000000000');
END
GO

IF COL_LENGTH('Merchant.Transaction','PhysicalPosEodId') IS NULL
BEGIN
ALTER TABLE Merchant.[Transaction]
    ADD PhysicalPosEodId UNIQUEIDENTIFIER NOT NULL
    CONSTRAINT DF_Merchant_Transaction_PhysicalPosEodId
    DEFAULT CONVERT(uniqueidentifier,'00000000-0000-0000-0000-000000000000');
END
GO

IF COL_LENGTH('Merchant.Transaction','PhysicalPosOldEodId') IS NULL
BEGIN
ALTER TABLE Merchant.[Transaction]
    ADD PhysicalPosOldEodId UNIQUEIDENTIFIER NOT NULL
    CONSTRAINT DF_Merchant_Transaction_PhysicalPosOldEodId
    DEFAULT CONVERT(uniqueidentifier,'00000000-0000-0000-0000-000000000000');
END
GO

/* ---------------- BANK.TRANSACTION ---------------- */

IF COL_LENGTH('Bank.Transaction','EndOfDayStatus') IS NULL
BEGIN
ALTER TABLE Bank.[Transaction]
    ADD EndOfDayStatus VARCHAR(50) NOT NULL
    CONSTRAINT DF_Bank_Transaction_EndOfDayStatus DEFAULT ('Pending');
END
GO

IF COL_LENGTH('Bank.Transaction','MerchantPhysicalPosId') IS NULL
BEGIN
ALTER TABLE Bank.[Transaction]
    ADD MerchantPhysicalPosId UNIQUEIDENTIFIER NOT NULL
    CONSTRAINT DF_Bank_Transaction_MerchantPhysicalPosId
    DEFAULT CONVERT(uniqueidentifier,'00000000-0000-0000-0000-000000000000');
END
GO

IF COL_LENGTH('Bank.Transaction','PhysicalPosEodId') IS NULL
BEGIN
ALTER TABLE Bank.[Transaction]
    ADD PhysicalPosEodId UNIQUEIDENTIFIER NOT NULL
    CONSTRAINT DF_Bank_Transaction_PhysicalPosEodId
    DEFAULT CONVERT(uniqueidentifier,'00000000-0000-0000-0000-000000000000');
END
GO

/* ---------------- MERCHANT.MERCHANTPHYSICALPOS ---------------- */

IF COL_LENGTH('Merchant.MerchantPhysicalPos','PosMerchantId') IS NULL
ALTER TABLE Merchant.MerchantPhysicalPos ADD PosMerchantId VARCHAR(50);
GO

IF COL_LENGTH('Merchant.MerchantPhysicalPos','PosTerminalId') IS NULL
ALTER TABLE Merchant.MerchantPhysicalPos ADD PosTerminalId VARCHAR(50);
GO

/* ---------------- TABLES ---------------- */

IF OBJECT_ID('Physical.EndOfDay','U') IS NULL
BEGIN
CREATE TABLE Physical.EndOfDay (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    MerchantId UNIQUEIDENTIFIER NOT NULL,
    BatchId VARCHAR(50) NOT NULL,
    PosMerchantId VARCHAR(50) NOT NULL,
    PosTerminalId VARCHAR(50) NOT NULL,
    [Date] DATETIME2 NOT NULL,
    SaleCount INT NOT NULL,
    VoidCount INT NOT NULL,
    RefundCount INT NOT NULL,
    InstallmentSaleCount INT NOT NULL,
    FailedCount INT NOT NULL,
    SaleAmount DECIMAL(18,4) NOT NULL,
    VoidAmount DECIMAL(18,4) NOT NULL,
    RefundAmount DECIMAL(18,4) NOT NULL,
    InstallmentSaleAmount DECIMAL(18,4) NOT NULL,
    Currency VARCHAR(10) NOT NULL,
    InstitutionId INT NOT NULL,
    Vendor VARCHAR(20),
    SerialNumber VARCHAR(200) NOT NULL,
    Status VARCHAR(50) NOT NULL CONSTRAINT DF_EndOfDay_Status DEFAULT ('Pending'),
    CreateDate DATETIME2 NOT NULL,
    UpdateDate DATETIME2,
    CreatedBy VARCHAR(50) NOT NULL,
    LastModifiedBy VARCHAR(50),
    RecordStatus VARCHAR(50) NOT NULL
);
END
GO

IF OBJECT_ID('Physical.ReconciliationTransaction','U') IS NULL
BEGIN
CREATE TABLE Physical.ReconciliationTransaction (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    PaymentId VARCHAR(50) NOT NULL,
    BatchId VARCHAR(50) NOT NULL,
    [Date] DATETIME2 NOT NULL,
    Type VARCHAR(50) NOT NULL,
    Status VARCHAR(20) NOT NULL,
    Currency VARCHAR(10) NOT NULL,
    MerchantId VARCHAR(50) NOT NULL,
    TerminalId VARCHAR(50) NOT NULL,
    Amount DECIMAL(18,4) NOT NULL,
    PointAmount DECIMAL(18,4) NOT NULL,
    Installment INT NOT NULL,
    MaskedCardNo VARCHAR(50) NOT NULL,
    BinNumber VARCHAR(10) NOT NULL,
    ProvisionNo VARCHAR(50),
    IssuerBankId VARCHAR(20) NOT NULL,
    AcquirerResponseCode VARCHAR(10),
    InstitutionId INT NOT NULL,
    Vendor VARCHAR(20),
    Rrn VARCHAR(50),
    Stan VARCHAR(50),
    PosEntryMode VARCHAR(20),
    PinEntryInfo VARCHAR(20),
    BankRef VARCHAR(50) NOT NULL,
    OriginalRef VARCHAR(50),
    PfMerchantId UNIQUEIDENTIFIER NOT NULL,
    ConversationId VARCHAR(50) NOT NULL,
    ClientIpAddress VARCHAR(50),
    SerialNumber VARCHAR(200) NOT NULL,
    ReconciliationStatus VARCHAR(50) NOT NULL CONSTRAINT DF_Recon_Status DEFAULT ('Pending'),
    MerchantTransactionId UNIQUEIDENTIFIER NOT NULL,
    UnacceptableTransactionId UNIQUEIDENTIFIER NOT NULL,
    PhysicalPosEodId UNIQUEIDENTIFIER NOT NULL,
    CreateDate DATETIME2 NOT NULL,
    UpdateDate DATETIME2,
    CreatedBy VARCHAR(50) NOT NULL,
    LastModifiedBy VARCHAR(50),
    RecordStatus VARCHAR(50) NOT NULL
);
END
GO

IF OBJECT_ID('Physical.UnacceptableTransaction', 'U') IS NULL
BEGIN
CREATE TABLE Physical.UnacceptableTransaction (
    Id UNIQUEIDENTIFIER NOT NULL,
    PaymentId VARCHAR(50) NOT NULL,
    BatchId VARCHAR(50) NOT NULL,
    [Date] DATETIME2 NOT NULL,
    Type VARCHAR(50) NOT NULL,
    Status VARCHAR(20) NOT NULL,
    Currency VARCHAR(10) NOT NULL,
    MerchantId VARCHAR(50) NOT NULL,
    TerminalId VARCHAR(50) NOT NULL,
    Amount DECIMAL(18,4) NOT NULL,
    PointAmount DECIMAL(18,4) NOT NULL,
    Installment INT NOT NULL,
    MaskedCardNo VARCHAR(50) NOT NULL,
    BinNumber VARCHAR(10) NOT NULL,
    ProvisionNo VARCHAR(50),
    IssuerBankId VARCHAR(20) NOT NULL,
    AcquirerResponseCode VARCHAR(10),
    InstitutionId INT NOT NULL,
    Vendor VARCHAR(20),
    Rrn VARCHAR(50),
    Stan VARCHAR(50),
    PosEntryMode VARCHAR(20),
    PinEntryInfo VARCHAR(20),
    BankRef VARCHAR(50) NOT NULL,
    OriginalRef VARCHAR(50),
    PfMerchantId UNIQUEIDENTIFIER NOT NULL,
    ConversationId VARCHAR(50) NOT NULL,
    ClientIpAddress VARCHAR(50),
    SerialNumber VARCHAR(200) NOT NULL,
    Gateway VARCHAR(20),
    ErrorCode VARCHAR(20),
    ErrorMessage VARCHAR(300),
    CurrentStatus VARCHAR(50) NOT NULL,
    PhysicalPosEodId UNIQUEIDENTIFIER NOT NULL,
    CreateDate DATETIME2 NOT NULL,
    UpdateDate DATETIME2,
    CreatedBy VARCHAR(50) NOT NULL,
    LastModifiedBy VARCHAR(50),
    RecordStatus VARCHAR(50) NOT NULL,
    CONSTRAINT PK_UnacceptableTransaction PRIMARY KEY (Id)
);
END
GO