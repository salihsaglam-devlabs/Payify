IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.TABLES 
    WHERE TABLE_SCHEMA = 'Merchant' AND TABLE_NAME = 'InstallmentTransaction'
)
BEGIN
    CREATE TABLE Merchant."Transaction" (
        Id uniqueidentifier NOT NULL,
        MerchantId uniqueidentifier NOT NULL,
        MerchantTransactionId uniqueidentifier NOT NULL,
        ConversationId varchar(50) NOT NULL,
        IpAddress varchar(50) NULL,
        TransactionType varchar(50) NOT NULL,
        TransactionStatus varchar(50) NOT NULL,
        OrderId varchar(50) NULL,
        Amount numeric(18, 4) NOT NULL,
        PointAmount numeric(18, 4) NOT NULL,
        Currency int NOT NULL,
        InstallmentCount int NOT NULL,
        BinNumber varchar(10) NOT NULL,
        CardNumber varchar(50) NOT NULL,
        HasCvv bit NOT NULL,
        HasExpiryDate bit NOT NULL,
        IsInternational bit NOT NULL,
        IsAmex bit NOT NULL,
        IsReverse bit NOT NULL,
        ReverseDate datetime2 NOT NULL,
        IsReturn bit NOT NULL,
        ReturnDate datetime2 NOT NULL,
        ReturnAmount numeric(18, 4) NOT NULL,
        ReturnedTransactionId varchar(50) NULL,
        IsPreClose bit NOT NULL,
        PreCloseDate datetime2 NOT NULL,
        PreCloseTransactionId varchar(50) NULL,
        Is3ds bit NOT NULL,
        ThreeDSessionId varchar(200) NULL,
        BankCommissionRate numeric(4, 2) NOT NULL,
        BankCommissionAmount numeric(18, 4) NOT NULL,
        IssuerBankCode int NOT NULL,
        AcquireBankCode int NOT NULL,
        CardTransactionType varchar(50) NULL,
        IntegrationMode varchar(50) NOT NULL,
        ResponseCode varchar(20) NULL,
        ResponseDescription varchar(1000) NULL,
        TransactionStartDate datetime2 NOT NULL,
        TransactionEndDate datetime2 NOT NULL,
        VposId uniqueidentifier NOT NULL,
        LanguageCode varchar(100) NULL,
        BatchStatus varchar(50) NOT NULL,
        CardType varchar(50) NOT NULL,
        TransactionDate date NOT NULL,
        IsChargeback bit NOT NULL,
        IsSuspecious bit NOT NULL,
        SuspeciousDescription varchar(500) NULL,
        MerchantCustomerName varchar(200) NULL,
        MerchantCustomerPhoneNumber varchar(30) NULL,
        Description varchar(300) NULL,
        CardHolderName varchar(200) NULL,
        CreateDate datetime2 NOT NULL,
        UpdateDate datetime2 NULL,
        CreatedBy varchar(50) NOT NULL,
        LastModifiedBy varchar(50) NULL,
        RecordStatus varchar(50) NOT NULL,
        ReturnStatus varchar(50) NOT NULL DEFAULT '',
        CreatedNameBy varchar(200) NULL,
        AmountWithoutBankCommission numeric(18, 4) NOT NULL DEFAULT 0.0,
        AmountWithoutCommissions numeric(18, 4) NOT NULL DEFAULT 0.0,
        BsmvAmount numeric(18, 4) NOT NULL DEFAULT 0.0,
        PfCommissionAmount numeric(18, 4) NOT NULL DEFAULT 0.0,
        PfCommissionRate numeric(18, 4) NOT NULL DEFAULT 0.0,
        PfNetCommissionAmount numeric(18, 4) NOT NULL DEFAULT 0.0,
        PricingProfileItemId uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000',
        ProvisionNumber varchar(50) NULL,
        VposName varchar(100) NULL,
        BankPaymentDate date NOT NULL DEFAULT '0001-01-01',
        PfPaymentDate datetime2 NOT NULL DEFAULT '0001-01-01',
        IsManualReturn bit NOT NULL DEFAULT 0,
        PostingItemId uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000',
        PointCommissionAmount numeric(18, 4) NOT NULL DEFAULT 0.0,
        PointCommissionRate numeric(18, 4) NOT NULL DEFAULT 0.0,
        IsOnUsPayment bit NOT NULL DEFAULT 0,
        MerchantCustomerPhoneCode varchar(10) NULL,
        PfPerTransactionFee numeric(18, 4) NOT NULL DEFAULT 0.0,
        ServiceCommissionAmount numeric(18, 4) NOT NULL DEFAULT 0.0,
        ServiceCommissionRate numeric(18, 4) NOT NULL DEFAULT 0.0,
        BlockageStatus varchar(50) NOT NULL DEFAULT '',
        LastChargebackActivityDate datetime2 NOT NULL DEFAULT '0001-01-01',
        SubMerchantId uniqueidentifier NULL,
        SubMerchantName varchar(150) NULL,
        SubMerchantNumber varchar(15) NULL,
        AmountWithoutParentMerchantCommission numeric(18, 4) NOT NULL DEFAULT 0.0,
        ParentMerchantCommissionAmount numeric(18, 4) NOT NULL DEFAULT 0.0,
        ParentMerchantCommissionRate numeric(18, 4) NOT NULL DEFAULT 0.0,
        CONSTRAINT PK_InstallmentTransaction PRIMARY KEY (Id)
    );

    CREATE INDEX IX_InstallmentTransaction_AcquireBankCode ON Merchant.InstallmentTransaction(AcquireBankCode);
    CREATE INDEX IX_InstallmentTransaction_BatchStatus_RecordStatus ON Merchant.InstallmentTransaction(BatchStatus, RecordStatus);
    CREATE INDEX IX_InstallmentTransaction_IssuerBankCode ON Merchant.InstallmentTransaction(IssuerBankCode);
    CREATE INDEX IX_InstallmentTransaction_MerchantId ON Merchant.InstallmentTransaction(MerchantId);
    CREATE INDEX IX_InstallmentTransaction_PostingItemId ON Merchant.InstallmentTransaction(PostingItemId);
    CREATE INDEX IX_InstallmentTransaction_TransactionDate ON Merchant.InstallmentTransaction(TransactionDate);
END
GO

-- PricingProfile tablosundan kolon düşür
IF EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = 'Core' 
    AND TABLE_NAME = 'PricingProfile' 
    AND COLUMN_NAME = 'ProfileSettlementMode'
)
BEGIN
    ALTER TABLE Core.PricingProfile DROP COLUMN ProfileSettlementMode;
END;

-- Transaction tablosuna kolon ekle
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = 'Merchant' 
    AND TABLE_NAME = 'Transaction' 
    AND COLUMN_NAME = 'IsPerInstallment'
)
BEGIN
    ALTER TABLE Merchant."Transaction" ADD IsPerInstallment BIT NOT NULL DEFAULT 0;
END;

-- ThreeDVerification tablosuna CostProfileItemId ekle
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = 'Core' 
    AND TABLE_NAME = 'ThreeDVerification' 
    AND COLUMN_NAME = 'CostProfileItemId'
)
BEGIN
    ALTER TABLE Core.ThreeDVerification ADD CostProfileItemId UNIQUEIDENTIFIER NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';
END;

-- ThreeDVerification tablosuna IsPerInstallment ekle
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = 'Core' 
    AND TABLE_NAME = 'ThreeDVerification' 
    AND COLUMN_NAME = 'IsPerInstallment'
)
BEGIN
    ALTER TABLE Core.ThreeDVerification ADD IsPerInstallment BIT NOT NULL DEFAULT 0;
END;