-- vpos schema
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Vpos')
BEGIN
    EXEC('CREATE SCHEMA Vpos');
END;

-- core schema
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Core')
BEGIN
    EXEC('CREATE SCHEMA Core');
END;

-- bank schema
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Bank')
BEGIN
    EXEC('CREATE SCHEMA Bank');
END;

-- card schema
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Card')
BEGIN
    EXEC('CREATE SCHEMA Card');
END;

-- merchant schema
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Merchant')
BEGIN
    EXEC('CREATE SCHEMA Merchant');
END;

-- api schema
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Api')
BEGIN
    EXEC('CREATE SCHEMA Api');
END;

-- hpp schema
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Hpp')
BEGIN
    EXEC('CREATE SCHEMA Hpp');
END;

-- limit schema (çift tırnak MSSQL'de gerekmez)
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Limit')
BEGIN
    EXEC('CREATE SCHEMA [Limit]');
END;

-- link schema
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Link')
BEGIN
    EXEC('CREATE SCHEMA Link');
END;

-- posting schema
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Posting')
BEGIN
    EXEC('CREATE SCHEMA Posting');
END;

-- submerchant schema
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Submerchant')
BEGIN
    EXEC('CREATE SCHEMA Submerchant');
END;
-- Drop table

-- DROP TABLE core.contact_person;

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'Core' AND TABLE_NAME = 'ContactPerson')
BEGIN
CREATE TABLE Core.ContactPerson (
	Id uniqueidentifier NOT NULL,
	ContactPersonType varchar(50) NOT NULL,
	IdentityNumber varchar(11) NULL,
	"Name" varchar(100) NOT NULL,
	Surname varchar(100) NOT NULL,
	Email varchar(256) NOT NULL,
	CompanyEmail varchar(256) NULL,
	BirthDate datetime2 NOT NULL,
	CompanyPhoneNumber varchar(20) NOT NULL,
	MobilePhoneNumber varchar(20) NOT NULL,
	MobilePhoneNumberSecond varchar(20) NULL,
	CreateDate datetime2  NOT NULL,
	UpdateDate datetime2  NULL,
	CreatedBy varchar(50) NOT NULL,
	LastModifiedBy varchar(50) NULL,
	RecordStatus varchar(50) NOT NULL,
	CONSTRAINT PkContactPerson PRIMARY KEY (Id)
);
END
-- core.currency definition

-- Drop table

-- DROP TABLE core.currency;

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'Core' AND TABLE_NAME = 'Currency')
BEGIN
CREATE TABLE Core.Currency (
	Id uniqueidentifier NOT NULL,
	Code varchar(10) NOT NULL,
	"Name" varchar(50) NOT NULL,
	Symbol varchar(5) NOT NULL,
	"Number" int NOT NULL,
	CurrencyType varchar(50) NOT NULL,
	CreateDate datetime2 NOT NULL,
	UpdateDate datetime2  NULL,
	CreatedBy varchar(50) NOT NULL,
	LastModifiedBy varchar(50) NULL,
	RecordStatus varchar(50) NOT NULL,
	CONSTRAINT Ak_CurrencyCode UNIQUE (Code),
	CONSTRAINT Pk_Currency PRIMARY KEY (Id)
);
CREATE UNIQUE INDEX IX_Currency_Code ON Core.Currency (Code);
END
-- core.customer definition

-- Drop table

-- DROP TABLE core.customer;
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'Core' AND TABLE_NAME = 'Customer')
BEGIN
CREATE TABLE Core.Customer (
	Id uniqueidentifier NOT NULL,
	CustomerStatus varchar(50) NOT NULL,
	CompanyType varchar(50) NOT NULL,
	CommercialTitle varchar(100) NOT NULL,
	TradeRegistrationNumber varchar(16) NOT NULL,
	TaxAdministration varchar(200) NOT NULL,
	TaxNumber varchar(11) NOT NULL,
	MersisNumber varchar(16) NULL,
	Country int NOT NULL,
	CountryName varchar(200) NOT NULL,
	City int NOT NULL,
	CityName varchar(200) NOT NULL,
	District int NOT NULL,
	DistrictName varchar(200) NOT NULL,
	PostalCode varchar(5) NOT NULL,
	Address varchar(256) NOT NULL,
	ContactPersonId uniqueidentifier NOT NULL,
	CreateDate datetime2 NOT NULL,
	UpdateDate datetime2 NULL,
	CreatedBy varchar(50) NOT NULL,
	LastModifiedBy varchar(50) NULL,
	RecordStatus varchar(50) NOT NULL,
	CustomerId uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000',
	CustomerNumber int NOT NULL DEFAULT 0,
    CONSTRAINT Pk_Customer PRIMARY KEY (Id),
	CONSTRAINT Fk_Customer_ContactPerson_ContactPersonId 
	FOREIGN KEY (ContactPersonId) REFERENCES Core.ContactPerson(Id) ON DELETE CASCADE
);

END
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Customer_ContactPersonId')
    CREATE INDEX IX_Customer_ContactPersonId ON Core.Customer(ContactPersonId);
-- Drop table

-- DROP TABLE bank.bank;
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'Bank' AND TABLE_NAME = 'Bank')
BEGIN
CREATE TABLE Bank.Bank (
	Id uniqueidentifier NOT NULL,
	Code int NOT NULL,
	"Name" varchar(100) NULL,
	CreateDate datetime2 NOT NULL,
	UpdateDate datetime2 NULL,
	CreatedBy varchar(50) NOT NULL,
	LastModifiedBy varchar(50) NULL,
	RecordStatus varchar(50) NOT NULL,
	CONSTRAINT Ak_BankCode UNIQUE (Code),
	CONSTRAINT Pk_Bank PRIMARY KEY (Id)
);
END

-- bank.bank_backup definition

-- Drop table

-- DROP TABLE bank.bank_backup;
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'Bank' AND TABLE_NAME = 'BankBackup')
BEGIN
CREATE TABLE Bank.BankBackup (
	Id uniqueidentifier NULL,
	Code int NULL,
	"Name" varchar(100) NULL,
	CreateDate datetime2 NULL,
	UpdateDate datetime2 NULL,
	CreatedBy varchar(50) NULL,
	LastModifiedBy varchar(50) NULL,
	RecordStatus varchar(50) NULL
);

END
-- bank.health_check_transaction definition

-- Drop table

-- DROP TABLE bank.health_check_transaction;
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'Bank' AND TABLE_NAME = 'HealthCheckTransaction')
BEGIN
CREATE TABLE Bank.HealthCheckTransaction (
	Id uniqueidentifier NOT NULL,
	TransactionType varchar(50) NOT NULL,
	TransactionStatus varchar(50) NOT NULL,
	AcquireBankCode int NOT NULL,
	BankTransactionDate datetime2 NOT NULL,
	CreateDate datetime2 NOT NULL,
	UpdateDate datetime2 NULL,
	CreatedBy varchar(50) NOT NULL,
	LastModifiedBy varchar(50) NULL,
	RecordStatus varchar(50) NOT NULL,
	CONSTRAINT PkHealthCheckTransaction PRIMARY KEY (Id)
);
END

-- bank.acquire_bank definition

-- Drop table

-- DROP TABLE bank.acquire_bank;
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'Bank' AND TABLE_NAME = 'AcquireBank')
BEGIN
CREATE TABLE Bank.AcquireBank (
	Id uniqueidentifier NOT NULL,
	BankCode int NOT NULL,
	EndOfDayHour int NOT NULL,
	EndOfDayMinute int NOT NULL,
	AcceptAmex bit NOT NULL,
	HasSubmerchantIntegration bit NOT NULL,
	CardNetwork varchar(50) NOT NULL,
	CreateDate datetime2 NOT NULL,
	UpdateDate datetime2 NULL,
	CreatedBy varchar(50) NOT NULL,
	LastModifiedBy varchar(50) NULL,
	RecordStatus varchar(50) NOT NULL,
	RestrictOwnCardNotOnUs bit NOT NULL DEFAULT 0,
	CONSTRAINT Pk_AcquireBank PRIMARY KEY (Id),
	CONSTRAINT Fk_AcquireBank_Bank_BankId FOREIGN KEY (BankCode) REFERENCES Bank.Bank(Code) ON DELETE NO ACTION
);
CREATE INDEX IX_AcquireBank_BankCode ON Bank.AcquireBank (BankCode);

END
-- bank.api_key definition

-- Drop table

-- DROP TABLE bank.api_key;
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'Bank' AND TABLE_NAME = 'ApiKey')
BEGIN
CREATE TABLE Bank.ApiKey (
	Id uniqueidentifier NOT NULL,
	AcquireBankId uniqueidentifier NOT NULL,
	"Key" varchar(50) NULL,
	MappingName varchar(50) NULL,
	CreateDate datetime2 NOT NULL,
	UpdateDate datetime2 NULL,
	CreatedBy varchar(50) NOT NULL,
	LastModifiedBy varchar(50) NULL,
	RecordStatus varchar(50) NOT NULL,
	Category varchar(50) NOT NULL DEFAULT '',
	CONSTRAINT Pk_ApiKey PRIMARY KEY (Id),
	CONSTRAINT Fk_ApiKey_AcquireBank_AcquireBankId FOREIGN KEY (AcquireBankId) REFERENCES Bank.AcquireBank(Id) ON DELETE CASCADE
);
 CREATE INDEX IX_ApiKey_AcquireBankId ON Bank.ApiKey (AcquireBankId);
END

-- bank.health_check definition

-- Drop table

-- DROP TABLE bank.health_check;
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'Bank' AND TABLE_NAME = 'HealthCheck')
BEGIN
CREATE TABLE Bank.HealthCheck (
	Id uniqueidentifier NOT NULL,
	AcquireBankId uniqueidentifier NOT NULL,
	LastCheckDate datetime2 NOT NULL,
	TotalTransactionCount int NOT NULL,
	FailTransactionCount int NOT NULL,
	FailTransactionRate int NOT NULL,
	HealthCheckType varchar(50) NOT NULL,
	IsHealthCheckAllowed bit NOT NULL,
	CreateDate datetime2 NOT NULL,
	UpdateDate datetime2 NULL,
	CreatedBy varchar(50) NOT NULL,
	LastModifiedBy varchar(50) NULL,
	RecordStatus varchar(50) NOT NULL,
	AllowedCheckDate datetime2 NOT NULL DEFAULT '0001-01-01',
	CONSTRAINT Pk_HealthCheck PRIMARY KEY (Id),
	CONSTRAINT Fk_HealthCheck_AcquireBank_AcquireBankId FOREIGN KEY (AcquireBankId) REFERENCES Bank.AcquireBank(Id) ON DELETE CASCADE
);
 CREATE INDEX IX_HealthCheck_AcquireBankId ON Bank.HealthCheck (AcquireBankId);
 END

-- bank."limit" definition

-- Drop table

-- DROP TABLE bank."limit";
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'Bank' AND TABLE_NAME = 'Limit')
BEGIN
    CREATE TABLE Bank.[Limit] (
        Id UNIQUEIDENTIFIER NOT NULL,
        AcquireBankId UNIQUEIDENTIFIER NOT NULL,
        MonthlyLimitAmount DECIMAL(18,4) NOT NULL,
        MarginRatio INT NOT NULL,
        TotalAmount DECIMAL(18,4) NOT NULL,
        BankLimitType VARCHAR(50) NOT NULL,
        CreateDate DATETIME2 NOT NULL,
        UpdateDate DATETIME2 NULL,
        CreatedBy VARCHAR(50) NOT NULL,
        LastModifiedBy VARCHAR(50) NULL,
        RecordStatus VARCHAR(50) NOT NULL,
        LastValidDate DATETIME2 NOT NULL DEFAULT '0001-01-01',
        IsExpired BIT NOT NULL DEFAULT 0,
        CONSTRAINT PK_Limit PRIMARY KEY (Id),
        CONSTRAINT FK_Limit_AcquireBank_AcquireBankId 
        FOREIGN KEY (AcquireBankId) REFERENCES Bank.AcquireBank(Id) ON DELETE CASCADE
    );

    CREATE INDEX IX_Limit_AcquireBankId ON Bank.[Limit] (AcquireBankId);
END

-- bank."transaction" definition

-- Drop table

-- DROP TABLE bank."transaction";


IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'Bank' AND TABLE_NAME = 'Transaction')
BEGIN
    CREATE TABLE Bank.[Transaction] (
        Id UNIQUEIDENTIFIER NOT NULL,
        TransactionType VARCHAR(50) NOT NULL,
        TransactionStatus VARCHAR(50) NOT NULL,
        OrderId VARCHAR(50) NULL,
        Amount DECIMAL(18,4) NOT NULL,
        PointAmount DECIMAL(18,4) NOT NULL,
        Currency INT NOT NULL,
        InstallmentCount INT NOT NULL,
        CardNumber VARCHAR(50) NULL,
        IsReverse BIT NOT NULL,
        ReverseDate DATETIME2 NOT NULL,
        Is3ds BIT NOT NULL,
        IssuerBankCode INT NOT NULL,
        AcquireBankCode INT NOT NULL,
        MerchantCode VARCHAR(200) NULL,
        SubMerchantCode VARCHAR(200) NULL,
        BankOrderId VARCHAR(50) NULL,
        RrnNumber VARCHAR(50) NULL,
        ApprovalCode VARCHAR(50) NULL,
        BankResponseCode VARCHAR(50) NULL,
        BankResponseDescription VARCHAR(1000) NULL,
        BankTransactionDate DATETIME2 NOT NULL,
        TransactionStartDate DATETIME2 NOT NULL,
        TransactionEndDate DATETIME2 NOT NULL,
        VposId UNIQUEIDENTIFIER NOT NULL,
        MerchantTransactionId UNIQUEIDENTIFIER NOT NULL,
        CreateDate DATETIME2 NOT NULL,
        UpdateDate DATETIME2 NULL,
        CreatedBy VARCHAR(50) NOT NULL,
        LastModifiedBy VARCHAR(50) NULL,
        RecordStatus VARCHAR(50) NOT NULL,
        Stan VARCHAR(50) NULL,
        CONSTRAINT PK_Transaction PRIMARY KEY (Id),
        CONSTRAINT FK_Transaction_Bank_AcquireBankCode 
            FOREIGN KEY (AcquireBankCode) REFERENCES Bank.Bank(Code) ON DELETE NO ACTION,
        CONSTRAINT FK_Transaction_Bank_IssuerBankCode 
            FOREIGN KEY (IssuerBankCode) REFERENCES Bank.Bank(Code) ON DELETE NO ACTION
    );

    CREATE INDEX IX_Transaction_AcquireBankCode ON Bank.[Transaction] (AcquireBankCode);
    CREATE INDEX IX_Transaction_IssuerBankCode ON Bank.[Transaction] (IssuerBankCode);
END

-- merchant.response_code definition

-- Drop table

-- DROP TABLE merchant.response_code;
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'Merchant' AND TABLE_NAME = 'ResponseCode')
BEGIN
CREATE TABLE Merchant.ResponseCode (
	Id uniqueidentifier NOT NULL,
	ResponseCode varchar(10) NOT NULL,
	Description varchar(100) NOT NULL,
	DisplayMessageTr varchar(500) NOT NULL,
	DisplayMessageEn varchar(500) NOT NULL,
	CreateDate datetime2 NOT NULL,
	UpdateDate datetime2 NULL,
	CreatedBy varchar(50) NOT NULL,
	LastModifiedBy varchar(50) NULL,
	RecordStatus varchar(50) NOT NULL,
	CONSTRAINT Pk_ResponseCode PRIMARY KEY (Id)
);
END
-- bank.response_code definition

-- Drop table

-- DROP TABLE bank.response_code;
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'Bank' AND TABLE_NAME = 'ResponseCode')
BEGIN
CREATE TABLE Bank.ResponseCode (
	Id uniqueidentifier NOT NULL,
	BankCode int NOT NULL,
	ResponseCode varchar(50) NOT NULL,
	Description varchar(256) NOT NULL,
	ProcessTimeoutManagement bit NOT NULL,
	MerchantResponseCodeId uniqueidentifier NULL,
	CreateDate datetime2 NOT NULL,
	UpdateDate datetime2 NULL,
	CreatedBy varchar(50) NOT NULL,
	LastModifiedBy varchar(50) NULL,
	RecordStatus varchar(50) NOT NULL,
	CONSTRAINT Pk_ResponseCode PRIMARY KEY (Id)
);
CREATE INDEX IX_ResponseCode_BankCode ON Bank.ResponseCode(BankCode);
CREATE INDEX IX_ResponseCode_MerchantResponseCodeId ON Bank.ResponseCode(MerchantResponseCodeId);

END

-- bank.response_code foreign keys

IF NOT EXISTS (
    SELECT 1
    FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
    WHERE TABLE_SCHEMA = 'Bank'
      AND TABLE_NAME = 'ResponseCode'
      AND CONSTRAINT_NAME = 'Fk_ResponseCode_Bank_BankCode'
)
BEGIN
    ALTER TABLE Bank.ResponseCode
    ADD CONSTRAINT Fk_ResponseCode_Bank_BankCode
        FOREIGN KEY (BankCode)
        REFERENCES Bank.Bank(Code)
        ON DELETE NO ACTION;
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
    WHERE TABLE_SCHEMA = 'Bank'
      AND TABLE_NAME = 'ResponseCode'
      AND CONSTRAINT_NAME = 'Fk_ResponseCode_Merchant_ResponseCode_MerchantResponseCode'
)
BEGIN
    ALTER TABLE Bank.ResponseCode
    ADD CONSTRAINT Fk_ResponseCode_Merchant_ResponseCode_MerchantResponseCode
        FOREIGN KEY (MerchantResponseCodeId)
        REFERENCES Merchant.ResponseCode(Id);
END
GO


-- vpos.vpos definition

-- Drop table

-- DROP TABLE vpos.vpos;
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'Vpos' AND TABLE_NAME = 'Vpos')
BEGIN
CREATE TABLE Vpos.Vpos (
	Id uniqueidentifier NOT NULL,
	"Name" varchar(100) NULL,
	VposStatus varchar(50) NOT NULL,
	AcquireBankId uniqueidentifier NOT NULL,
	SecurityType varchar(50) NOT NULL,
	CreateDate datetime2 NOT NULL,
	UpdateDate datetime2 NULL,
	CreatedBy varchar(50) NOT NULL,
	LastModifiedBy varchar(50) NULL,
	RecordStatus varchar(50) NOT NULL,
	VposType varchar(50) NOT NULL DEFAULT '',
	BlockageCode int NULL,
	IsOnUsVpos bit NOT NULL DEFAULT 0,
	CONSTRAINT Pk_Vpos PRIMARY KEY (Id)
);
CREATE INDEX IX_Vpos_AcquireBankId ON Vpos.Vpos(AcquireBankId);

END
-- card.bin_backup definition

-- Drop table

-- DROP TABLE card.bin_backup;
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'Card' AND TABLE_NAME = 'BinBackup')
BEGIN
CREATE TABLE Card.BinBackup (
	Id uniqueidentifier NULL,
	BinNumber varchar(10) NULL,
	CardBrand varchar(50) NULL,
	CardType varchar(50) NULL,
	CardSubType varchar(50) NULL,
	CardNetwork varchar(50) NULL,
	Country int NULL,
	CountryName varchar(200) NULL,
	IsVirtual bit NULL,
	BankCode int NULL,
	CreateDate datetime2 NULL,
	UpdateDate datetime2 NULL,
	CreatedBy varchar(50) NULL,
	LastModifiedBy varchar(50) NULL,
	RecordStatus varchar(50) NULL
);

END
-- card.loyalty_backup definition

-- Drop table

-- DROP TABLE card.loyalty_backup;
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'Card' AND TABLE_NAME = 'LoyaltyBackup')
BEGIN
CREATE TABLE Card.LoyaltyBackup (
	Id uniqueidentifier NULL,
	"Name" varchar(50) NULL,
	BankCode int NULL,
	CreateDate datetime2 NULL,
	UpdateDate datetime2 NULL,
	CreatedBy varchar(50) NULL,
	LastModifiedBy varchar(50) NULL,
	RecordStatus varchar(50) NULL
);

END
-- card.bin definition

-- Drop table

-- DROP TABLE card.bin;
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'Card' AND TABLE_NAME = 'Bin')
BEGIN
CREATE TABLE Card.Bin (
	Id uniqueidentifier NOT NULL,
	BinNumber varchar(10) NOT NULL,
	CardBrand varchar(50) NOT NULL,
	CardType varchar(50) NOT NULL,
	CardSubType varchar(50) NOT NULL,
	CardNetwork varchar(50) NOT NULL,
	Country int NOT NULL,
	CountryName varchar(200) NOT NULL,
	IsVirtual bit NOT NULL,
	BankCode int NOT NULL,
	CreateDate datetime2 NOT NULL,
	UpdateDate datetime2 NULL,
	CreatedBy varchar(50) NOT NULL,
	LastModifiedBy varchar(50) NULL,
	RecordStatus varchar(50) NOT NULL,
	CONSTRAINT Pk_Bin PRIMARY KEY (Id)
);
CREATE INDEX IX_Bin_BankCode ON Card.Bin(BankCode);
CREATE UNIQUE INDEX IX_Bin_BinNumber ON Card.Bin(BinNumber);

END

-- card.loyalty definition

-- Drop table

-- DROP TABLE card.loyalty;
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'Card' AND TABLE_NAME = 'Loyalty')
BEGIN
CREATE TABLE Card.Loyalty (
	Id uniqueidentifier NOT NULL,
	"Name" varchar(50) NOT NULL,
	BankCode int NOT NULL,
	CreateDate datetime2 NOT NULL,
	UpdateDate datetime2 NULL,
	CreatedBy varchar(50) NOT NULL,
	LastModifiedBy varchar(50) NULL,
	RecordStatus varchar(50) NOT NULL,
	CONSTRAINT Pk_Loyalty PRIMARY KEY (Id)
);
CREATE INDEX IX_Loyalty_BankCode ON Card.Loyalty(BankCode);
END

-- card.loyalty_exception definition

-- Drop table

-- DROP TABLE card.loyalty_exception;
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'Card' AND TABLE_NAME = 'LoyaltyException')
BEGIN
CREATE TABLE Card.LoyaltyException (
	Id uniqueidentifier NOT NULL,
	BankCode int NOT NULL,
	CounterBankCode int NOT NULL,
	AllowOnUs bit NOT NULL,
	AllowInstallment bit NOT NULL,
	CreateDate datetime2 NOT NULL,
	UpdateDate datetime2 NULL,
	CreatedBy varchar(50) NOT NULL,
	LastModifiedBy varchar(50) NULL,
	RecordStatus varchar(50) NOT NULL,
	AllowPoint bit NOT NULL DEFAULT 0,
	CONSTRAINT Pk_LoyaltyException PRIMARY KEY (Id)
);
CREATE INDEX IX_LoyaltyException_BankCode ON Card.LoyaltyException(BankCode);
CREATE INDEX IX_LoyaltyException_CounterBankCode ON Card.LoyaltyException(CounterBankCode);
END

-- card."token" definition

-- Drop table

-- DROP TABLE card."token";
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'Card' AND TABLE_NAME = 'Token')
BEGIN
    CREATE TABLE Card."Token" (
        Id uniqueidentifier NOT NULL,
        Token varchar(50) NOT NULL,
        ExpiryDate datetime2 NOT NULL,
        MerchantId uniqueidentifier NOT NULL,
        CvvEncrypted varchar(300) NULL,
        CreateDate datetime2 NOT NULL,
        UpdateDate datetime2 NULL,
        CreatedBy varchar(50) NOT NULL,
        LastModifiedBy varchar(50) NULL,
        RecordStatus varchar(50) NOT NULL,
        CardNumberEncrypted varchar(300) NOT NULL DEFAULT '',
        ExpireDateEncrypted varchar(300) NOT NULL DEFAULT '',
        CONSTRAINT PK_Token PRIMARY KEY (Id)
    );

    CREATE INDEX IX_Token_ExpiryDate ON Card."Token"(ExpiryDate);
    CREATE INDEX IX_Token_MerchantId ON Card."Token"(MerchantId);
END


-- card.bin foreign keys

IF NOT EXISTS (
    SELECT 1
    FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
    WHERE TABLE_SCHEMA = 'Card'
      AND TABLE_NAME = 'Bin'
      AND CONSTRAINT_NAME = 'Fk_Bin_Bank_BankCode'
)
BEGIN
    ALTER TABLE Card.Bin
    ADD CONSTRAINT Fk_Bin_Bank_BankCode
        FOREIGN KEY (BankCode)
        REFERENCES Bank.Bank(Code)
        ON DELETE NO ACTION;
END
GO

-- Card.Loyalty foreign key
IF NOT EXISTS (
    SELECT 1
    FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
    WHERE TABLE_SCHEMA = 'Card'
      AND TABLE_NAME = 'Loyalty'
      AND CONSTRAINT_NAME = 'Fk_Loyalty_Bank_BankCode'
)
BEGIN
    ALTER TABLE Card.Loyalty
    ADD CONSTRAINT Fk_Loyalty_Bank_BankCode
        FOREIGN KEY (BankCode)
        REFERENCES Bank.Bank(Code)
        ON DELETE NO ACTION;
END
GO

-- Card.LoyaltyException foreign key (BankCode)
IF NOT EXISTS (
    SELECT 1
    FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
    WHERE TABLE_SCHEMA = 'Card'
      AND TABLE_NAME = 'LoyaltyException'
      AND CONSTRAINT_NAME = 'Fk_LoyaltyException_Bank_BankCode'
)
BEGIN
    ALTER TABLE Card.LoyaltyException
    ADD CONSTRAINT Fk_LoyaltyException_Bank_BankCode
        FOREIGN KEY (BankCode)
        REFERENCES Bank.Bank(Code)
        ON DELETE NO ACTION;
END
GO

-- Card.LoyaltyException foreign key (CounterBankCode)
IF NOT EXISTS (
    SELECT 1
    FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
    WHERE TABLE_SCHEMA = 'Card'
      AND TABLE_NAME = 'LoyaltyException'
      AND CONSTRAINT_NAME = 'Fk_LoyaltyException_Bank_CounterBankCode'
)
BEGIN
    ALTER TABLE Card.LoyaltyException
    ADD CONSTRAINT Fk_LoyaltyException_Bank_CounterBankCode
        FOREIGN KEY (CounterBankCode)
        REFERENCES Bank.Bank(Code)
        ON DELETE NO ACTION;
END
GO




-- card."token" foreign keys


-- merchant.merchant_content definition

-- Drop table

-- DROP TABLE merchant.merchant_content;
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'Merchant' AND TABLE_NAME = 'MerchantContent')
BEGIN
CREATE TABLE Merchant.MerchantContent (
	Id uniqueidentifier NOT NULL,
	MerchantId uniqueidentifier NOT NULL,
	"Name" varchar(50) NOT NULL,
	ContentSource varchar(50) NOT NULL,
	CreateDate datetime2 NOT NULL,
	UpdateDate datetime2 NULL,
	CreatedBy varchar(50) NOT NULL,
	LastModifiedBy varchar(50) NULL,
	RecordStatus varchar(50) NOT NULL,
	CONSTRAINT Pk_MerchantContent PRIMARY KEY (Id)
);
CREATE INDEX IX_MerchantContent_RecordStatus ON Merchant.MerchantContent(RecordStatus);
END

-- merchant.merchant definition

-- Drop table

-- DROP TABLE merchant.merchant;
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'Merchant' AND TABLE_NAME = 'Merchant')
BEGIN
CREATE TABLE Merchant.Merchant (
	Id uniqueidentifier NOT NULL,
	"Name" varchar(150) NOT NULL,
	"Number" varchar(15) NOT NULL,
	MerchantStatus varchar(50) NOT NULL,
	ApplicationChannel varchar(50) NOT NULL,
	IntegrationMode varchar(50) NOT NULL,
	MccCode varchar(4) NULL,
	CustomerId uniqueidentifier NOT NULL,
	"Language" varchar(100) NULL,
	WebSiteUrl varchar(150) NOT NULL,
	MonthlyTurnover numeric(18, 4) NOT NULL,
	PhoneCode varchar(15) NOT NULL,
	AgreementDate datetime2 NOT NULL,
	SalesPersonId uniqueidentifier NULL,
	PaymentDueDay int NOT NULL,
	Is3dRequired bit NOT NULL,
	IsDocumentRequired bit NOT NULL,
	IsManuelPayment3dRequired bit NOT NULL,
	HalfSecureAllowed bit NOT NULL,
	InstallmentAllowed bit NOT NULL,
	InternationalCardAllowed bit NOT NULL,
	PreAuthorizationAllowed bit NOT NULL,
	FinancialTransactionAllowed bit NOT NULL,
	PaymentAllowed bit NOT NULL,
	RejectReason varchar(256) NULL,
	ParameterValue varchar(100) NULL,
	PricingProfileNumber varchar(6) NULL,
	MerchantPoolId uniqueidentifier NOT NULL,
	MerchantIntegratorId uniqueidentifier NULL,
	ContactPersonId uniqueidentifier NULL,
	GlobalMerchantId varchar(8) NULL,
	AnnulmentCode varchar(2) NULL,
	AnnulmentDescription varchar(300) NULL,
	AnnulmentDate datetime2 NOT NULL,
	CreateDate datetime2 NOT NULL,
	UpdateDate datetime2 NULL,
	CreatedBy varchar(50) NOT NULL,
	LastModifiedBy varchar(50) NULL,
	RecordStatus varchar(50) NOT NULL,
	IsPostAuthAmountHigherAllowed bit NOT NULL DEFAULT 0,
	IksNotificationDate datetime2 NOT NULL DEFAULT '0001-01-01',
	AnnulmentId varchar(6) NULL,
	AnnulmentAdditionalInfo varchar(300) NULL,
	IsAnnulment bit NULL,
	IsLinkPayment3dRequired bit NOT NULL DEFAULT 0,
	PaymentReturnAllowed bit NOT NULL DEFAULT 0,
	PaymentReverseAllowed bit NOT NULL DEFAULT 0,
	IsHostedPayment3dRequired bit NOT NULL DEFAULT 0,
	IsReturnApproved bit NOT NULL DEFAULT 0,
	IsCvvPaymentAllowed bit NOT NULL DEFAULT 0,
	IsExcessReturnAllowed bit NOT NULL DEFAULT 0,
	HostingTaxNo varchar(11) NULL,
	PostingPaymentChannel varchar(50) NOT NULL DEFAULT 'BankAccount',
	MerchantType varchar(50) NOT NULL DEFAULT '',
	IsInvoiceCommissionReflected bit NOT NULL DEFAULT 0,
	ParentMerchantId uniqueidentifier NULL,
	ParentMerchantName varchar(150) NULL,
	ParentMerchantNumber varchar(15) NULL,
	CONSTRAINT Pk_Merchant PRIMARY KEY (Id)
);
CREATE INDEX IX_Merchant_ContactPersonId ON Merchant.Merchant(ContactPersonId);
CREATE INDEX IX_Merchant_CustomerId ON Merchant.Merchant(CustomerId);
CREATE INDEX IX_Merchant_MccCode ON Merchant.Merchant(MccCode);
CREATE INDEX IX_Merchant_MerchantIntegratorId ON Merchant.Merchant(MerchantIntegratorId);
CREATE UNIQUE INDEX IX_Merchant_MerchantPoolId ON Merchant.Merchant(MerchantPoolId);
END


 IF NOT EXISTS (
        SELECT 1
        FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
        WHERE TABLE_SCHEMA = 'Card'
          AND TABLE_NAME = 'Token'
          AND CONSTRAINT_NAME = 'Fk_Token_Merchant_MerchantId'
    ) BEGIN
        ALTER TABLE Card."Token"
        ADD CONSTRAINT Fk_Token_Merchant_MerchantId
        FOREIGN KEY (MerchantId)
        REFERENCES Merchant.Merchant(Id)
        ON DELETE CASCADE;
    END 


-- merchant.counter definition

-- Drop table

-- DROP TABLE merchant.counter;
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'Merchant' AND TABLE_NAME = 'Counter')
BEGIN
CREATE TABLE Merchant.Counter (
	Id uniqueidentifier NOT NULL,
	NumberCounter int NOT NULL IDENTITY(1,1),
	CreateDate datetime2 NOT NULL,
	UpdateDate datetime2 NULL,
	CreatedBy varchar(50) NOT NULL,
	LastModifiedBy varchar(50) NULL,
	RecordStatus varchar(50) NOT NULL,
	CONSTRAINT Pk_Counter PRIMARY KEY (Id)
);
CREATE UNIQUE INDEX IX_Counter_NumberCounter ON Merchant.Counter(NumberCounter);
END

-- merchant.deduction_transaction definition

-- Drop table

-- DROP TABLE merchant.deduction_transaction;
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'Merchant' AND TABLE_NAME = 'DeductionTransaction')
BEGIN
CREATE TABLE Merchant.DeductionTransaction (
	Id uniqueidentifier NOT NULL,
	MerchantId uniqueidentifier NOT NULL,
	PostingBalanceId uniqueidentifier NOT NULL,
	MerchantDeductionId uniqueidentifier NOT NULL,
	DeductionType varchar(50) NOT NULL,
	Amount numeric(18, 4) NOT NULL,
	Currency int NOT NULL,
	CreateDate datetime2 NOT NULL,
	UpdateDate datetime2 NULL,
	CreatedBy varchar(50) NOT NULL,
	LastModifiedBy varchar(50) NULL,
	RecordStatus varchar(50) NOT NULL,
	CONSTRAINT Pk_DeductionTransaction PRIMARY KEY (Id)
);
END

-- merchant.integrator definition

-- Drop table

-- DROP TABLE merchant.integrator;
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'Merchant' AND TABLE_NAME = 'Integrator')
BEGIN
CREATE TABLE Merchant.Integrator (
	Id uniqueidentifier NOT NULL,
	"Name" varchar(100) NOT NULL,
	CommissionRate numeric(4, 2) NOT NULL,
	CreateDate datetime2 NOT NULL,
	UpdateDate datetime2 NULL,
	CreatedBy varchar(50) NOT NULL,
	LastModifiedBy varchar(50) NULL,
	RecordStatus varchar(50) NOT NULL,
	CONSTRAINT Pk_Integrator PRIMARY KEY (Id)
);
END

-- merchant.mcc definition

-- Drop table

-- DROP TABLE merchant.mcc;
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'Merchant' AND TABLE_NAME = 'Mcc')
BEGIN
CREATE TABLE Merchant.Mcc (
	Id uniqueidentifier NOT NULL,
	Code varchar(4) NOT NULL,
	"Name" varchar(256) NOT NULL,
	MaxIndividualInstallmentCount int NOT NULL,
	MaxCorporateInstallmentCount int NOT NULL,
	Description varchar(300) NULL,
	CreateDate datetime2 NOT NULL,
	UpdateDate datetime2 NULL,
	CreatedBy varchar(50) NOT NULL,
	LastModifiedBy varchar(50) NULL,
	RecordStatus varchar(50) NOT NULL,
	CONSTRAINT Ak_MccCode UNIQUE (Code),
	CONSTRAINT Pk_Mcc PRIMARY KEY (Id)
);
END
-- merchant.pool definition

-- Drop table

-- DROP TABLE merchant.pool;
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.TABLES 
    WHERE TABLE_SCHEMA = 'Merchant' AND TABLE_NAME = 'Pool'
)
BEGIN
CREATE TABLE Merchant.Pool (
	Id uniqueidentifier NOT NULL,
	MerchantPoolStatus varchar(50) NOT NULL,
	MerchantName text NULL,
	CompanyType varchar(50) NOT NULL,
	CommercialTitle varchar(100) NOT NULL,
	WebSiteUrl varchar(100) NOT NULL,
	MonthlyTurnover numeric(18, 4) NOT NULL,
	PostalCode varchar(5) NOT NULL,
	Address varchar(256) NOT NULL,
	PhoneCode varchar(15) NOT NULL,
	Country int NOT NULL,
	CountryName varchar(200) NOT NULL,
	City int NOT NULL,
	CityName varchar(200) NOT NULL,
	District int NOT NULL,
	DistrictName varchar(200) NOT NULL,
	TaxAdministration varchar(200) NOT NULL,
	TaxNumber varchar(11) NOT NULL,
	TradeRegistrationNumber varchar(16) NOT NULL,
	Iban varchar(26) NOT NULL,
	BankCode int NOT NULL,
	CurrencyCode varchar(10) NULL,
	RejectReason varchar(256) NULL,
	ParameterValue varchar(100) NULL,
	Channel varchar(50) NULL,
	Email varchar(256) NOT NULL,
	CompanyEmail varchar(256) NOT NULL,
	AuthorizedPersonIdentityNumber varchar(11) NULL,
	AuthorizedPersonName varchar(100) NOT NULL,
	AuthorizedPersonSurname varchar(100) NOT NULL,
	AuthorizedPersonBirthDate datetime2 NOT NULL,
	AuthorizedPersonCompanyPhoneNumber varchar(20) NOT NULL,
	AuthorizedPersonMobilePhoneNumber varchar(20) NOT NULL,
	AuthorizedPersonMobilePhoneNumberSecond varchar(20) NULL,
	CreateDate datetime2 NOT NULL,
	UpdateDate datetime2 NULL,
	CreatedBy varchar(50) NOT NULL,
	LastModifiedBy varchar(50) NULL,
	RecordStatus varchar(50) NOT NULL,
	PostingPaymentChannel varchar(50) NOT NULL DEFAULT 'BankAccount',
	WalletNumber varchar(26) NULL,
	MerchantType varchar(50) NOT NULL DEFAULT '',
	IsInvoiceCommissionReflected bit NOT NULL DEFAULT 0,
	ParentMerchantId uniqueidentifier NULL,
	ParentMerchantName varchar(150) NULL,
	ParentMerchantNumber varchar(15) NULL,
	CONSTRAINT Pk_Pool PRIMARY KEY (Id)
);
CREATE INDEX IX_Pool_BankCode ON Merchant.Pool(BankCode);
CREATE INDEX IX_Pool_CurrencyCode ON Merchant.Pool(CurrencyCode);
END

-- merchant.merchant_logo definition

-- Drop table

-- DROP TABLE merchant.merchant_logo;
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.TABLES 
    WHERE TABLE_SCHEMA = 'Merchant' AND TABLE_NAME = 'MerchantLogo'
)
BEGIN
CREATE TABLE Merchant.MerchantLogo (
	Id uniqueidentifier NOT NULL,
	MerchantId uniqueidentifier NOT NULL,
	Bytes varbinary(max) NULL,
	ContentType varchar(100) NOT NULL,
	CreateDate datetime2 NOT NULL,
	UpdateDate datetime2 NULL,
	CreatedBy varchar(50) NOT NULL,
	LastModifiedBy varchar(50) NULL,
	RecordStatus varchar(50) NOT NULL,
	FileName text NULL,
	CONSTRAINT Pk_MerchantLogo PRIMARY KEY (Id)
);
CREATE INDEX IX_MerchantLogo_MerchantId ON Merchant.MerchantLogo(MerchantId);

END
-- merchant.merchant_pre_application definition

-- Drop table

-- DROP TABLE merchant.merchant_pre_application;
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.TABLES 
    WHERE TABLE_SCHEMA = 'Merchant' AND TABLE_NAME = 'MerchantPreApplication'
)
BEGIN
CREATE TABLE Merchant.MerchantPreApplication (
	Id uniqueidentifier NOT NULL,
	FullName varchar(50) NOT NULL,
	Email varchar(50) NOT NULL,
	PhoneNumber text NOT NULL,
	MonthlyTurnover varchar(50) NOT NULL,
	ApplicationStatus varchar(50) NOT NULL,
	Website varchar(50) NOT NULL,
	ConsentConfirmation bit NOT NULL,
	KvkkConfirmation bit NOT NULL,
	CreateDate datetime2 NOT NULL,
	UpdateDate datetime2 NULL,
	CreatedBy varchar(50) NOT NULL,
	LastModifiedBy varchar(50) NULL,
	RecordStatus varchar(50) NOT NULL,
	ResponsiblePerson text NULL,
	ProductTypes varchar(50) NOT NULL,
	CONSTRAINT Pk_MerchantPreApplication PRIMARY KEY (Id)
);
END

-- merchant.merchant_statement definition

-- Drop table

-- DROP TABLE merchant.merchant_statement;
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.TABLES 
    WHERE TABLE_SCHEMA = 'Merchant' AND TABLE_NAME = 'MerchantStatement'
)
BEGIN
CREATE TABLE Merchant.MerchantStatement (
	Id uniqueidentifier NOT NULL,
	MerchantId uniqueidentifier NOT NULL,
	MailAddress varchar(50) NOT NULL,
	StatementStartDate datetime2 NOT NULL,
	StatementEndDate datetime2 NOT NULL,
	PdfPath varchar(256) NULL,
	FileName varchar(50) NULL,
	CreateDate datetime2 NOT NULL,
	UpdateDate datetime2 NULL,
	CreatedBy varchar(50) NOT NULL,
	LastModifiedBy varchar(50) NULL,
	RecordStatus varchar(50) NOT NULL,
	MerchantName varchar(200) NULL,
	StatementMonth int NOT NULL DEFAULT 0,
	StatementYear int NOT NULL DEFAULT 0,
	Description varchar(300) NULL,
	ReceiptNumber varchar(50) NULL,
	StatementStatus varchar(50) NOT NULL DEFAULT '',
	StatementType varchar(50) NOT NULL DEFAULT '',
	ExcelPath varchar(256) NULL,
	CONSTRAINT Pk_MerchantStatement PRIMARY KEY (Id)
);
END




-- merchant.merchant_content_version definition

-- Drop table

-- DROP TABLE merchant.merchant_content_version;
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.TABLES 
    WHERE TABLE_SCHEMA = 'Merchant' AND TABLE_NAME = 'MerchantContentVersion'
)
BEGIN
CREATE TABLE Merchant.MerchantContentVersion (
	Id uniqueidentifier NOT NULL,
	MerchantContentId uniqueidentifier NOT NULL,
	Title varchar(150) NOT NULL,
	"Content" text NULL,
	LanguageCode varchar(10) NOT NULL,
	CreateDate datetime2 NOT NULL,
	UpdateDate datetime2 NULL,
	CreatedBy varchar(50) NOT NULL,
	LastModifiedBy varchar(50) NULL,
	RecordStatus varchar(50) NOT NULL,
	CONSTRAINT Pk_MerchantContentVersion PRIMARY KEY (Id),
	CONSTRAINT Fk_MerchantContentVersion_MerchantContent_MerchantContent 
	FOREIGN KEY (MerchantContentId) REFERENCES Merchant.MerchantContent(Id) ON DELETE CASCADE
);
CREATE INDEX IX_MerchantContentVersion_MerchantContentId ON Merchant.MerchantContentVersion (MerchantContentId);

END
-- merchant.merchant_pre_application_history definition

-- Drop table

-- DROP TABLE merchant.merchant_pre_application_history;
IF NOT EXISTS (
	SELECT 1 FROM INFORMATION_SCHEMA.TABLES 
	WHERE TABLE_SCHEMA = 'Merchant' AND TABLE_NAME = 'MerchantPreApplicationHistory'
)
BEGIN
CREATE TABLE Merchant.MerchantPreApplicationHistory (
	Id uniqueidentifier NOT NULL,
	MerchantPreApplicationId uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000',
	UserId uniqueidentifier NOT NULL,
	UserName varchar(50) NOT NULL,
	OperationType varchar(50) NOT NULL,
	OperationDate datetime2 NOT NULL,
	OperationNote text NULL,
	CreateDate datetime2 NOT NULL,
	UpdateDate datetime2 NULL,
	CreatedBy varchar(50) NOT NULL,
	LastModifiedBy varchar(50) NULL,
	RecordStatus varchar(50) NOT NULL,
	CONSTRAINT Pk_MerchantPreApplicationHistory PRIMARY KEY (Id),
	CONSTRAINT Fk_MerchantPreApplicationHistory_MerchantPreApplication 
	FOREIGN KEY (MerchantPreApplicationId) REFERENCES Merchant.MerchantPreApplication(Id) ON DELETE CASCADE
);
CREATE INDEX IX_MerchantPreApplicationHistory_MerchantPreApplicationId ON Merchant.MerchantPreApplicationHistory(MerchantPreApplicationId);
END

-- merchant.api_key definition

-- Drop table

-- DROP TABLE merchant.api_key;
IF NOT EXISTS (
	SELECT 1 FROM INFORMATION_SCHEMA.TABLES 
	WHERE TABLE_SCHEMA = 'Merchant' AND TABLE_NAME = 'ApiKey'
)
BEGIN
CREATE TABLE Merchant.ApiKey (
	Id uniqueidentifier NOT NULL,
	PublicKey varchar(100) NOT NULL,
	PrivateKeyEncrypted varchar(100) NOT NULL,
	MerchantId uniqueidentifier NOT NULL,
	CreateDate datetime2 NOT NULL,
	UpdateDate datetime2 NULL,
	CreatedBy varchar(50) NOT NULL,
	LastModifiedBy varchar(50) NULL,
	RecordStatus varchar(50) NOT NULL,
	CONSTRAINT Pk_ApiKey PRIMARY KEY (Id)
);
CREATE INDEX IX_ApiKey_MerchantId ON Merchant.ApiKey(MerchantId);

END
-- merchant.bank_account definition

-- Drop table

-- DROP TABLE merchant.bank_account;
IF NOT EXISTS (
	SELECT 1 FROM INFORMATION_SCHEMA.TABLES 
	WHERE TABLE_SCHEMA = 'Merchant' AND TABLE_NAME = 'BankAccount'
)
BEGIN
CREATE TABLE Merchant.BankAccount (
	Id uniqueidentifier NOT NULL,
	Iban varchar(26) NOT NULL,
	BankCode int NOT NULL,
	MerchantId uniqueidentifier NOT NULL,
	CreateDate datetime2 NOT NULL,
	UpdateDate datetime2 NULL,
	CreatedBy varchar(50) NOT NULL,
	LastModifiedBy varchar(50) NULL,
	RecordStatus varchar(50) NOT NULL,
	CONSTRAINT Pk_BankAccount PRIMARY KEY (Id)
);
CREATE INDEX IX_BankAccount_BankCode ON Merchant.BankAccount(BankCode);
CREATE INDEX IX_BankAccount_MerchantId ON Merchant.BankAccount(MerchantId);

END
-- merchant.blockage definition

-- Drop table

-- DROP TABLE merchant.blockage;
IF NOT EXISTS (
	SELECT 1 FROM INFORMATION_SCHEMA.TABLES 
	WHERE TABLE_SCHEMA = 'Merchant' AND TABLE_NAME = 'Blockage'
)
BEGIN
CREATE TABLE Merchant.Blockage (
	Id uniqueidentifier NOT NULL,
	TotalAmount numeric(18, 4) NOT NULL,
	BlockageAmount numeric(18, 4) NOT NULL,
	RemainingAmount numeric(18, 4) NOT NULL,
	MerchantBlockageStatus varchar(50) NOT NULL,
	MerchantId uniqueidentifier NOT NULL,
	CreateDate datetime2 NOT NULL,
	UpdateDate datetime2 NULL,
	CreatedBy varchar(50) NOT NULL,
	LastModifiedBy varchar(50) NULL,
	RecordStatus varchar(50) NOT NULL,
	CONSTRAINT Pk_Blockage PRIMARY KEY (Id)
);
CREATE INDEX IX_Blockage_MerchantId ON Merchant.Blockage(MerchantId);

END
-- merchant.blockage_detail definition

-- Drop table

-- DROP TABLE merchant.blockage_detail;
IF NOT EXISTS (
	SELECT 1 FROM INFORMATION_SCHEMA.TABLES 
	WHERE TABLE_SCHEMA = 'Merchant' AND TABLE_NAME = 'BlockageDetail'
)
BEGIN
CREATE TABLE Merchant.BlockageDetail (
	Id uniqueidentifier NOT NULL,
	PostingDate date NOT NULL,
	MerchantId uniqueidentifier NOT NULL,
	TotalAmount numeric(18, 4) NOT NULL,
	BlockageAmount numeric(18, 4) NOT NULL,
	RemainingAmount numeric(18, 4) NOT NULL,
	BlockageStatus varchar(50) NOT NULL,
	MerchantBlockageId uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000',
	CreateDate datetime2 NOT NULL,
	UpdateDate datetime2 NULL,
	CreatedBy varchar(50) NOT NULL,
	LastModifiedBy varchar(50) NULL,
	RecordStatus varchar(50) NOT NULL,
	CONSTRAINT Pk_BlockageDetail PRIMARY KEY (Id)
);
CREATE INDEX IX_BlockageDetail_MerchantBlockageId ON Merchant.BlockageDetail(MerchantBlockageId);
CREATE INDEX IX_BlockageDetail_MerchantId ON Merchant.BlockageDetail(MerchantId);
END

-- merchant.business_partner definition

-- Drop table

-- DROP TABLE merchant.business_partner;
IF NOT EXISTS (
	SELECT 1 FROM INFORMATION_SCHEMA.TABLES 
	WHERE TABLE_SCHEMA = 'Merchant' AND TABLE_NAME = 'BusinessPartner'
)
BEGIN
CREATE TABLE Merchant.BusinessPartner (
	Id uniqueidentifier NOT NULL,
	FirstName varchar(100) NOT NULL,
	LastName varchar(100) NOT NULL,
	Email varchar(256) NOT NULL,
	PhoneNumber varchar(50) NOT NULL,
	IdentityNumber varchar(20) NOT NULL,
	BirthDate datetime2 NOT NULL,
	MerchantId uniqueidentifier NOT NULL,
	CreateDate datetime2 NOT NULL,
	UpdateDate datetime2 NULL,
	CreatedBy varchar(50) NOT NULL,
	LastModifiedBy varchar(50) NULL,
	RecordStatus varchar(50) NOT NULL,
	CONSTRAINT Pk_BusinessPartner PRIMARY KEY (Id)
);
CREATE INDEX IX_BusinessPartner_MerchantId ON Merchant.BusinessPartner(MerchantId);
END

-- merchant."document" definition

-- Drop table

-- DROP TABLE merchant."document";
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.TABLES 
    WHERE TABLE_SCHEMA = 'Merchant' AND TABLE_NAME = 'Document'
)
BEGIN
    CREATE TABLE Merchant."Document" (
        Id uniqueidentifier NOT NULL,
        DocumentId uniqueidentifier NOT NULL,
        DocumentTypeId uniqueidentifier NOT NULL,
        DocumentName varchar(256) NOT NULL,
        MerchantId uniqueidentifier NOT NULL,
        CreateDate datetime2 NOT NULL,
        UpdateDate datetime2 NULL,
        CreatedBy varchar(50) NOT NULL,
        LastModifiedBy varchar(50) NULL,
        RecordStatus varchar(50) NOT NULL,
        MerchantTransactionId uniqueidentifier NULL,
        CONSTRAINT PK_Document PRIMARY KEY (Id)
    );

    CREATE INDEX IX_Document_MerchantId ON Merchant."Document"(MerchantId);
END


-- merchant.email definition

-- Drop table

-- DROP TABLE merchant.email;
IF NOT EXISTS (
	SELECT 1 FROM INFORMATION_SCHEMA.TABLES 
	WHERE TABLE_SCHEMA = 'Merchant' AND TABLE_NAME = 'Email'
)
BEGIN
CREATE TABLE Merchant.Email (
	Id uniqueidentifier NOT NULL,
	Email varchar(256) NOT NULL,
	EmailType varchar(50) NOT NULL,
	ReportAllowed bit NOT NULL,
	MerchantId uniqueidentifier NOT NULL,
	CreateDate datetime2 NOT NULL,
	UpdateDate datetime2 NULL,
	CreatedBy varchar(50) NOT NULL,
	LastModifiedBy varchar(50) NULL,
	RecordStatus varchar(50) NOT NULL,
	CONSTRAINT Pk_Email PRIMARY KEY (Id)
);
CREATE INDEX IX_Email_MerchantId ON Merchant.Email(MerchantId);
END

-- merchant.history definition

-- Drop table

-- DROP TABLE merchant.history;
IF NOT EXISTS (
	SELECT 1 FROM INFORMATION_SCHEMA.TABLES 
	WHERE TABLE_SCHEMA = 'Merchant' AND TABLE_NAME = 'History'
)
BEGIN
CREATE TABLE Merchant.History (
	Id uniqueidentifier NOT NULL,
	MerchantId uniqueidentifier NOT NULL,
	PermissionOperationType varchar(50) NOT NULL,
	NewData varchar(1000) NOT NULL,
	OldData varchar(1000) NOT NULL,
	Detail varchar(256) NOT NULL,
	CreateDate datetime2 NOT NULL,
	UpdateDate datetime2 NULL,
	CreatedBy varchar(50) NOT NULL,
	LastModifiedBy varchar(50) NULL,
	RecordStatus varchar(50) NOT NULL,
	CreatedNameBy varchar(256) NOT NULL DEFAULT '',
	CONSTRAINT Pk_History PRIMARY KEY (Id)
);
CREATE INDEX IX_History_MerchantId ON Merchant.History(MerchantId);
END

-- merchant.merchant_deduction definition

-- Drop table

-- DROP TABLE merchant.merchant_deduction;
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'Merchant' AND TABLE_NAME = 'MerchantDeduction')
BEGIN
CREATE TABLE Merchant.MerchantDeduction (
	Id uniqueidentifier NOT NULL,
	MerchantTransactionId uniqueidentifier NOT NULL,
	MerchantId uniqueidentifier NOT NULL,
	TotalDeductionAmount numeric(18, 4) NOT NULL,
	RemainingDeductionAmount numeric(18, 4) NOT NULL,
	DeductionType varchar(50) NOT NULL,
	DeductionStatus varchar(50) NOT NULL,
	CreateDate datetime2 NOT NULL,
	UpdateDate datetime2 NULL,
	CreatedBy varchar(50) NOT NULL,
	LastModifiedBy varchar(50) NULL,
	RecordStatus varchar(50) NOT NULL,
	Currency int NOT NULL DEFAULT 0,
	ExecutionDate date NOT NULL DEFAULT '0001-01-01',
	MerchantDueId uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000',
	PostingBalanceId uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000',
	ConversationId varchar(50) NULL,
	DeductionAmountWithCommission numeric(18, 4) NOT NULL DEFAULT 0.0,
	SubMerchantDeductionId uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000',
	CONSTRAINT Pk_MerchantDeduction PRIMARY KEY (Id)
);
CREATE INDEX IX_MerchantDeduction_MerchantId ON Merchant.MerchantDeduction(MerchantId);
END

-- merchant.merchant_due definition

-- Drop table

-- DROP TABLE merchant.merchant_due;
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'Merchant' AND TABLE_NAME = 'MerchantDue')
BEGIN
CREATE TABLE Merchant.MerchantDue (
	Id uniqueidentifier NOT NULL,
	MerchantId uniqueidentifier NOT NULL,
	CreateDate datetime2 NOT NULL,
	UpdateDate datetime2 NULL,
	CreatedBy varchar(50) NOT NULL,
	LastModifiedBy varchar(50) NULL,
	RecordStatus varchar(50) NOT NULL,
	LastExecutionDate datetime2 NOT NULL DEFAULT '0001-01-01',
	TotalExecutionCount int NOT NULL DEFAULT 0,
	DueProfileId uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000',
	CONSTRAINT Pk_MerchantDue PRIMARY KEY (Id)
);
CREATE INDEX IX_MerchantDue_DueProfileId ON Merchant.MerchantDue(DueProfileId);
CREATE INDEX IX_MerchantDue_MerchantId ON Merchant.MerchantDue(MerchantId);
END

-- merchant.merchant_return_pool definition

-- Drop table

-- DROP TABLE merchant.merchant_return_pool;
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'Merchant' AND TABLE_NAME = 'MerchantReturnPool')
BEGIN
CREATE TABLE Merchant.MerchantReturnPool (
	Id uniqueidentifier NOT NULL,
	ActionDate datetime2 NOT NULL,
	ActionUser uniqueidentifier NOT NULL,
	ReturnStatus varchar(50) NOT NULL,
	Amount numeric(18, 4) NOT NULL,
	OrderId varchar(50) NOT NULL,
	ConversationId varchar(50) NOT NULL,
	ClientIpAddress varchar(50) NOT NULL,
	LanguageCode text NULL,
	MerchantId uniqueidentifier NOT NULL,
	CreateDate datetime2 NOT NULL,
	UpdateDate datetime2 NULL,
	CreatedBy varchar(50) NOT NULL,
	LastModifiedBy varchar(50) NULL,
	RecordStatus varchar(50) NOT NULL,
	BankCode int NOT NULL DEFAULT 0,
	BankName varchar(100) NOT NULL DEFAULT '',
	BankStatus bit NULL,
	CardNumber varchar(50) NULL,
	RejectDescription varchar(400) NULL,
	CurrencyCode varchar(10) NULL,
	RejectReason varchar(400) NULL,
	BankResponseCode varchar(50) NULL,
	BankResponseDescription varchar(1000) NULL,
	CONSTRAINT Pk_MerchantReturnPool PRIMARY KEY (Id)
);
CREATE INDEX IX_MerchantReturnPool_MerchantId ON Merchant.MerchantReturnPool(MerchantId);

END

-- merchant.score definition

-- Drop table

-- DROP TABLE merchant.score;
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'Merchant' AND TABLE_NAME = 'Score')
BEGIN
CREATE TABLE Merchant.Score (
	Id uniqueidentifier NOT NULL,
	HasScoreCard bit NOT NULL,
	ScoreCardScore int NULL,
	HasFindeksRiskReport bit NOT NULL,
	FindeksScore int NULL,
	AlexaRank varchar(10) NULL,
	GoogleRank varchar(10) NULL,
	MerchantId uniqueidentifier NOT NULL,
	CreateDate datetime2 NOT NULL,
	UpdateDate datetime2 NULL,
	CreatedBy varchar(50) NOT NULL,
	LastModifiedBy varchar(50) NULL,
	RecordStatus varchar(50) NOT NULL,
	CONSTRAINT Pk_Score PRIMARY KEY (Id)
);
CREATE INDEX IX_Score_MerchantId ON Merchant.Score(MerchantId);
END

-- merchant."transaction" definition

-- Drop table

-- DROP TABLE merchant."transaction";
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.TABLES 
    WHERE TABLE_SCHEMA = 'Merchant' AND TABLE_NAME = 'Transaction'
)
BEGIN
    CREATE TABLE Merchant."Transaction" (
        Id uniqueidentifier NOT NULL,
        MerchantId uniqueidentifier NOT NULL,
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
        CONSTRAINT PK_Transaction PRIMARY KEY (Id)
    );

    CREATE INDEX IX_Transaction_AcquireBankCode ON Merchant."Transaction"(AcquireBankCode);
    CREATE INDEX IX_Transaction_BatchStatus_RecordStatus ON Merchant."Transaction"(BatchStatus, RecordStatus);
    CREATE INDEX IX_Transaction_IssuerBankCode ON Merchant."Transaction"(IssuerBankCode);
    CREATE INDEX IX_Transaction_MerchantId ON Merchant."Transaction"(MerchantId);
    CREATE INDEX IX_Transaction_PostingItemId ON Merchant."Transaction"(PostingItemId);
    CREATE INDEX IX_Transaction_TransactionDate ON Merchant."Transaction"(TransactionDate);
END
-- merchant."user" definition

-- Drop table

-- DROP TABLE merchant."user";
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.TABLES 
    WHERE TABLE_SCHEMA = 'Merchant' AND TABLE_NAME = 'User'
)
BEGIN
    CREATE TABLE Merchant."User" (
        Id uniqueidentifier NOT NULL,
        UserId uniqueidentifier NOT NULL,
        Name varchar(100) NOT NULL,
        Surname varchar(100) NOT NULL,
        Email varchar(100) NOT NULL,
        MobilePhoneNumber varchar(20) NOT NULL,
        RoleId varchar(50) NULL,
        RoleName varchar(150) NULL,
        MerchantId uniqueidentifier NOT NULL,
        CreateDate datetime2 NOT NULL,
        UpdateDate datetime2 NULL,
        CreatedBy varchar(50) NOT NULL,
        LastModifiedBy varchar(50) NULL,
        RecordStatus varchar(50) NOT NULL,
        BirthDate datetime2 NOT NULL DEFAULT '0001-01-01',
        CONSTRAINT PK_User PRIMARY KEY (Id)
    );

    CREATE INDEX IX_User_MerchantId ON Merchant."User"(MerchantId);
END

-- merchant.vpos definition

-- Drop table

-- DROP TABLE merchant.vpos;
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'Merchant' AND TABLE_NAME = 'Vpos')
BEGIN
CREATE TABLE Merchant.Vpos (
	Id uniqueidentifier NOT NULL,
	MerchantId uniqueidentifier NOT NULL,
	VposId uniqueidentifier NOT NULL,
	SubMerchantCode varchar(50) NULL,
	Priority int NOT NULL,
	CreateDate datetime2 NOT NULL,
	UpdateDate datetime2 NULL,
	CreatedBy varchar(50) NOT NULL,
	LastModifiedBy varchar(50) NULL,
	RecordStatus varchar(50) NOT NULL,
	IsTerminalNotification bit NOT NULL DEFAULT 0,
	"Password" varchar(50) NULL,
	TerminalNo varchar(20) NULL,
	ApiKey varchar(50) NULL,
	ProviderKey varchar(50) NULL,
	CONSTRAINT Pk_Vpos PRIMARY KEY (Id)
);
CREATE INDEX IX_Vpos_MerchantId ON Merchant.Vpos(MerchantId);
CREATE INDEX IX_Vpos_VposId ON Merchant.Vpos(VposId);

END
-- merchant.wallet definition

-- Drop table

-- DROP TABLE merchant.wallet;
IF NOT EXISTS (
    (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'Merchant' AND TABLE_NAME = 'Wallet')
)
BEGIN
CREATE TABLE Merchant.Wallet (
	Id uniqueidentifier NOT NULL,
	WalletNumber varchar(26) NOT NULL,
	MerchantId uniqueidentifier NOT NULL,
	CreateDate datetime2 NOT NULL,
	UpdateDate datetime2 NULL,
	CreatedBy varchar(50) NOT NULL,
	LastModifiedBy varchar(50) NULL,
	RecordStatus varchar(50) NOT NULL,
	CONSTRAINT Pk_Wallet PRIMARY KEY (Id)
);
CREATE INDEX IX_Wallet_MerchantId ON Merchant.Wallet(MerchantId);
END

-- Merchant.ApiKey foreign key
IF NOT EXISTS (
    SELECT 1
    FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
    WHERE TABLE_SCHEMA = 'Merchant'
      AND TABLE_NAME = 'ApiKey'
      AND CONSTRAINT_NAME = 'Fk_ApiKey_Merchant_MerchantId'
)
BEGIN
    ALTER TABLE Merchant.ApiKey
    ADD CONSTRAINT Fk_ApiKey_Merchant_MerchantId
        FOREIGN KEY (MerchantId)
        REFERENCES Merchant.Merchant(Id)
        ON DELETE CASCADE;
END
GO


-- Merchant.BankAccount foreign key (BankCode)
IF NOT EXISTS (
    SELECT 1
    FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
    WHERE TABLE_SCHEMA = 'Merchant'
      AND TABLE_NAME = 'BankAccount'
      AND CONSTRAINT_NAME = 'Fk_BankAccount_Bank_BankCode'
)
BEGIN
    ALTER TABLE Merchant.BankAccount
    ADD CONSTRAINT Fk_BankAccount_Bank_BankCode
        FOREIGN KEY (BankCode)
        REFERENCES Bank.Bank(Code)
        ON DELETE NO ACTION;
END
GO


-- Merchant.BankAccount foreign key (MerchantId)
IF NOT EXISTS (
    SELECT 1
    FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
    WHERE TABLE_SCHEMA = 'Merchant'
      AND TABLE_NAME = 'BankAccount'
      AND CONSTRAINT_NAME = 'Fk_BankAccount_Merchant_MerchantId'
)
BEGIN
    ALTER TABLE Merchant.BankAccount
    ADD CONSTRAINT Fk_BankAccount_Merchant_MerchantId
        FOREIGN KEY (MerchantId)
        REFERENCES Merchant.Merchant(Id)
        ON DELETE CASCADE;
END
GO



-- core.due_profile definition

-- Drop table

-- DROP TABLE core.due_profile;
IF NOT EXISTS (
    SELECT * FROM INFORMATION_SCHEMA.TABLES 
    WHERE TABLE_SCHEMA = 'Core' AND TABLE_NAME = 'DueProfile'
)
BEGIN
CREATE TABLE Core.DueProfile (
	Id uniqueidentifier NOT NULL,
	DueType varchar(50) NOT NULL,
	Amount numeric(18, 4) NOT NULL,
	Currency int NOT NULL,
	OccurenceInterval varchar(50) NOT NULL,
	IsDefault bit NOT NULL,
	CreateDate datetime2 NOT NULL,
	UpdateDate datetime2 NULL,
	CreatedBy varchar(50) NOT NULL,
	LastModifiedBy varchar(50) NULL,
	RecordStatus varchar(50) NOT NULL,
	Title varchar(250) NOT NULL DEFAULT '',
	CONSTRAINT Pk_DueProfile PRIMARY KEY (Id)
);
END

-- core.on_us_payment definition

-- Drop table

-- DROP TABLE core.on_us_payment;
IF NOT EXISTS (
    SELECT * FROM INFORMATION_SCHEMA.TABLES 
    WHERE TABLE_SCHEMA = 'Core' AND TABLE_NAME = 'OnUsPayment'
)
BEGIN
CREATE TABLE Core.OnUsPayment (
	Id uniqueidentifier NOT NULL,
	Status varchar(50) NOT NULL,
	PaymentStatus varchar(50) NOT NULL,
	WebhookStatus varchar(50) NOT NULL,
	MerchantId uniqueidentifier NOT NULL,
	MerchantTransactionId uniqueidentifier NOT NULL,
	MerchantName varchar(150) NOT NULL,
	MerchantNumber varchar(15) NOT NULL,
	"Name" varchar(150) NULL,
	Surname varchar(150) NULL,
	Email varchar(256) NULL,
	PhoneCode varchar(10) NOT NULL,
	PhoneNumber varchar(30) NOT NULL,
	WalletNumber varchar(10) NULL,
	EmoneyReferenceNumber varchar(256) NULL,
	EmoneyTransactionId uniqueidentifier NOT NULL,
	ExpiryDate datetime2 NOT NULL,
	WebhookRetryCount int NOT NULL,
	CallbackUrl varchar(250) NOT NULL,
	OrderId varchar(50) NOT NULL,
	Amount numeric(18, 4) NOT NULL,
	Currency int NOT NULL,
	CreateDate datetime2 NOT NULL,
	UpdateDate datetime2 NULL,
	CreatedBy varchar(50) NOT NULL,
	LastModifiedBy varchar(50) NULL,
	RecordStatus varchar(50) NOT NULL,
	CONSTRAINT Pk_OnUsPayment PRIMARY KEY (Id)
);
CREATE INDEX IX_OnUsPayment_ExpiryDate ON Core.OnUsPayment(ExpiryDate);
CREATE INDEX IX_OnUsPayment_MerchantTransactionId ON Core.OnUsPayment(MerchantTransactionId);
CREATE INDEX IX_OnUsPayment_PaymentStatus ON Core.OnUsPayment(PaymentStatus);
CREATE INDEX IX_OnUsPayment_Status ON Core.OnUsPayment(Status);
CREATE INDEX IX_OnUsPayment_WebhookStatus ON Core.OnUsPayment(WebhookStatus);

END


-- core.pricing_profile definition

-- Drop table

-- DROP TABLE core.pricing_profile;
IF NOT EXISTS (
    SELECT * FROM INFORMATION_SCHEMA.TABLES 
    WHERE TABLE_SCHEMA = 'Core' AND TABLE_NAME = 'PricingProfile'
)
BEGIN
CREATE TABLE Core.PricingProfile (
	Id uniqueidentifier NOT NULL,
	"Name" varchar(50) NULL,
	PricingProfileNumber varchar(6) NULL,
	ActivationDate datetime2 NOT NULL,
	ProfileStatus varchar(50) NOT NULL,
	CurrencyCode varchar(10) NULL,
	PerTransactionFee numeric(4, 2) NOT NULL,
	CreateDate datetime2 NOT NULL,
	UpdateDate datetime2 NULL,
	CreatedBy varchar(50) NOT NULL,
	LastModifiedBy varchar(50) NULL,
	RecordStatus varchar(50) NOT NULL,
	ProfileType varchar(50) NOT NULL DEFAULT '',
	CONSTRAINT Pk_PricingProfile PRIMARY KEY (Id),
	CONSTRAINT Fk_PricingProfile_Currency_CurrencyId 
	FOREIGN KEY (CurrencyCode) REFERENCES Core.Currency(Code) ON DELETE NO ACTION
);
CREATE INDEX IX_PricingProfile_CurrencyCode ON Core.PricingProfile(CurrencyCode);

END
-- core.pricing_profile_item definition

-- Drop table

-- DROP TABLE core.pricing_profile_item;
IF NOT EXISTS (
    SELECT * FROM INFORMATION_SCHEMA.TABLES 
    WHERE TABLE_SCHEMA = 'Core' AND TABLE_NAME = 'PricingProfileItem'
)
BEGIN
CREATE TABLE Core.PricingProfileItem (
	Id uniqueidentifier NOT NULL,
	ProfileCardType varchar(50) NOT NULL,
	InstallmentNumber int NOT NULL,
	InstallmentNumberEnd int NOT NULL,
	CommissionRate numeric(4, 2) NOT NULL,
	BlockedDayNumber int NOT NULL,
	IsActive bit NOT NULL,
	PricingProfileId uniqueidentifier NOT NULL,
	CreateDate datetime2 NOT NULL,
	UpdateDate datetime2 NULL,
	CreatedBy varchar(50) NOT NULL,
	LastModifiedBy varchar(50) NULL,
	RecordStatus varchar(50) NOT NULL,
	ParentMerchantCommissionRate numeric NOT NULL DEFAULT 0.0,
	CONSTRAINT Pk_PricingProfileItem PRIMARY KEY (Id),
	CONSTRAINT Fk_PricingProfileItem_PricingProfile_PricingProfileId 
	FOREIGN KEY (PricingProfileId) REFERENCES Core.PricingProfile(Id) ON DELETE CASCADE
);
CREATE INDEX IX_PricingProfileItem_PricingProfileId ON Core.PricingProfileItem(PricingProfileId);

END
-- core.cost_profile definition

-- Drop table

-- DROP TABLE core.cost_profile;
IF NOT EXISTS (
    SELECT * FROM INFORMATION_SCHEMA.TABLES 
    WHERE TABLE_SCHEMA = 'Core' AND TABLE_NAME = 'CostProfile'
)
BEGIN
CREATE TABLE Core.CostProfile (
	Id uniqueidentifier NOT NULL,
	"Name" varchar(50) NULL,
	ActivationDate datetime2 NOT NULL,
	PointCommission numeric(4, 2) NOT NULL,
	ServiceCommission numeric(4, 2) NOT NULL,
	ProfileStatus varchar(50) NOT NULL,
	VposId uniqueidentifier NOT NULL,
	CurrencyCode varchar(10) NULL,
	CreateDate datetime2 NOT NULL,
	UpdateDate datetime2 NULL,
	CreatedBy varchar(50) NOT NULL,
	LastModifiedBy varchar(50) NULL,
	RecordStatus varchar(50) NOT NULL,
	CONSTRAINT Pk_CostProfile PRIMARY KEY (Id)
);
CREATE INDEX IX_CostProfile_CurrencyCode ON Core.CostProfile(CurrencyCode);
CREATE INDEX IX_CostProfile_VposId ON Core.CostProfile(VposId);

END
-- core.cost_profile_item definition

-- Drop table

-- DROP TABLE core.cost_profile_item;
IF NOT EXISTS (
    SELECT * FROM INFORMATION_SCHEMA.TABLES 
    WHERE TABLE_SCHEMA = 'Core' AND TABLE_NAME = 'CostProfileItem'
)
BEGIN
CREATE TABLE Core.CostProfileItem (
	Id uniqueidentifier NOT NULL,
	CardTransactionType varchar(50) NOT NULL,
	ProfileCardType varchar(50) NOT NULL,
	InstallmentNumber int NOT NULL,
	InstallmentNumberEnd int NOT NULL,
	CommissionRate numeric(4, 2) NOT NULL,
	BlockedDayNumber int NOT NULL,
	IsActive bit NOT NULL,
	InstallmentSupport bit NULL,
	CostProfileId uniqueidentifier NOT NULL,
	CreateDate datetime2 NOT NULL,
	UpdateDate datetime2 NULL,
	CreatedBy varchar(50) NOT NULL,
	LastModifiedBy varchar(50) NULL,
	RecordStatus varchar(50) NOT NULL,
	CONSTRAINT Pk_CostProfileItem PRIMARY KEY (Id)
);
CREATE INDEX IX_CostProfileItem_CostProfileId ON Core.CostProfileItem(CostProfileId);
END

-- core.three_d_verification definition

-- Drop table

-- DROP TABLE core.three_d_verification;
IF NOT EXISTS (
    SELECT * FROM INFORMATION_SCHEMA.TABLES 
    WHERE TABLE_SCHEMA = 'Core' AND TABLE_NAME = 'ThreeDVerification'
)
BEGIN
CREATE TABLE Core.ThreeDVerification (
	Id uniqueidentifier NOT NULL,
	TransactionType varchar(50) NOT NULL,
	OrderId varchar(50) NULL,
	CardToken varchar(50) NOT NULL,
	InstallmentCount int NOT NULL,
	Amount numeric(18, 4) NOT NULL,
	PointAmount numeric(18, 4) NOT NULL,
	CurrentStep varchar(50) NOT NULL,
	CallbackUrl varchar(250) NULL,
	MerchantId uniqueidentifier NOT NULL,
	IssuerBankCode int NOT NULL,
	AcquireBankCode int NOT NULL,
	MerchantCode varchar(50) NOT NULL,
	SubMerchantCode varchar(50) NOT NULL,
	BinNumber varchar(8) NULL,
	Currency int NOT NULL,
	SessionExpiryDate datetime2 NOT NULL,
	BankCommissionAmount numeric(18, 4) NOT NULL,
	BankCommissionRate numeric(4, 2) NOT NULL,
	Md varchar(256) NULL,
	MdStatus varchar(2) NULL,
	MdErrorMessage varchar(256) NULL,
	Xid varchar(50) NULL,
	Eci varchar(50) NULL,
	Cavv varchar(50) NULL,
	PayerTxnId varchar(100) NULL,
	TxnStat varchar(50) NULL,
	ThreeDStatus varchar(50) NULL,
	HashKey varchar(100) NULL,
	BankTransactionDate datetime2 NOT NULL,
	BankResponseCode varchar(40) NULL,
	BankResponseDescription varchar(256) NULL,
	VposId uniqueidentifier NOT NULL,
	CreateDate datetime2 NOT NULL,
	UpdateDate datetime2 NULL,
	CreatedBy varchar(50) NOT NULL,
	LastModifiedBy varchar(50) NULL,
	RecordStatus varchar(50) NOT NULL,
	SubMerchantTerminalNo varchar(20) NOT NULL DEFAULT '',
	ConversationId varchar(50) NULL,
	BankBlockedDayNumber int NOT NULL DEFAULT 0,
	BankPacket varchar(500) NULL,
	CONSTRAINT Pk_ThreeDVerification PRIMARY KEY (Id)
);
CREATE INDEX IX_ThreeDVerification_VposId ON Core.ThreeDVerification(VposId);
END

-- core.time_out_transaction definition

-- Drop table

-- DROP TABLE core.time_out_transaction;
IF NOT EXISTS (
    SELECT * FROM INFORMATION_SCHEMA.TABLES 
    WHERE TABLE_SCHEMA = 'Core' AND TABLE_NAME = 'TimeOutTransaction'
)
BEGIN
CREATE TABLE Core.TimeOutTransaction (
	Id uniqueidentifier NOT NULL,
	TimeoutTransactionStatus varchar(50) NOT NULL,
	TransactionType varchar(50) NOT NULL,
	CardNumber varchar(50) NULL,
	OriginalOrderId varchar(50) NOT NULL,
	ConversationId varchar(50) NULL,
	OrderId varchar(50) NULL,
	Amount numeric(18, 4) NOT NULL,
	SubMerchantCode varchar(200) NULL,
	TransactionDate datetime2 NOT NULL,
	MerchantId uniqueidentifier NOT NULL,
	AcquireBankCode int NOT NULL,
	VposId uniqueidentifier NOT NULL,
	MerchantTransactionId uniqueidentifier NOT NULL,
	BankTransactionId uniqueidentifier NOT NULL,
	Currency int NOT NULL,
	LanguageCode varchar(100) NULL,
	RetryCount int NOT NULL,
	NextTryTime datetime2 NULL,
	PosErrorCode varchar(20) NULL,
	PosErrorMessage varchar(1000) NULL,
	ErrorCode varchar(10) NULL,
	ErrorMessage varchar(256) NULL,
	ResponseCode varchar(10) NULL,
	ResponseMessage varchar(256) NULL,
	Description varchar(256) NOT NULL,
	CreateDate datetime2 NOT NULL,
	UpdateDate datetime2 NULL,
	CreatedBy varchar(50) NOT NULL,
	LastModifiedBy varchar(50) NULL,
	RecordStatus varchar(50) NOT NULL,
	ClientIpAddress varchar(50) NOT NULL DEFAULT '',
	CONSTRAINT Pk_TimeOutTransaction PRIMARY KEY (Id)
);
CREATE INDEX IX_TimeOutTransaction_AcquireBankCode ON Core.TimeOutTransaction(AcquireBankCode);
CREATE INDEX IX_TimeOutTransaction_MerchantId ON Core.TimeOutTransaction(MerchantId);

END
-- core.cost_profile foreign keys

-- Core.CostProfile foreign key (CurrencyCode)
IF NOT EXISTS (
    SELECT 1
    FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
    WHERE TABLE_SCHEMA = 'Core'
      AND TABLE_NAME = 'CostProfile'
      AND CONSTRAINT_NAME = 'Fk_CostProfile_Currency_CurrencyCode'
)
BEGIN
    ALTER TABLE Core.CostProfile
    ADD CONSTRAINT Fk_CostProfile_Currency_CurrencyCode
        FOREIGN KEY (CurrencyCode)
        REFERENCES Core.Currency(Code)
        ON DELETE NO ACTION;
END
GO


-- Core.CostProfile foreign key (VposId)
IF NOT EXISTS (
    SELECT 1
    FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
    WHERE TABLE_SCHEMA = 'Core'
      AND TABLE_NAME = 'CostProfile'
      AND CONSTRAINT_NAME = 'Fk_CostProfile_Vpos_VposId'
)
BEGIN
    ALTER TABLE Core.CostProfile
    ADD CONSTRAINT Fk_CostProfile_Vpos_VposId
        FOREIGN KEY (VposId)
        REFERENCES Vpos.Vpos(Id)
        ON DELETE CASCADE;
END
GO


-- Core.CostProfileItem foreign key (CostProfileId)
IF NOT EXISTS (
    SELECT 1
    FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
    WHERE TABLE_SCHEMA = 'Core'
      AND TABLE_NAME = 'CostProfileItem'
      AND CONSTRAINT_NAME = 'Fk_CostProfileItem_CostProfile_CostProfileId'
)
BEGIN
    ALTER TABLE Core.CostProfileItem
    ADD CONSTRAINT Fk_CostProfileItem_CostProfile_CostProfileId
        FOREIGN KEY (CostProfileId)
        REFERENCES Core.CostProfile(Id)
        ON DELETE CASCADE;
END
GO


-- Core.ThreeDVerification foreign key (VposId)
IF NOT EXISTS (
    SELECT 1
    FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
    WHERE TABLE_SCHEMA = 'Core'
      AND TABLE_NAME = 'ThreeDVerification'
      AND CONSTRAINT_NAME = 'Fk_ThreeDVerification_Vpos_VposId'
)
BEGIN
    ALTER TABLE Core.ThreeDVerification
    ADD CONSTRAINT Fk_ThreeDVerification_Vpos_VposId
        FOREIGN KEY (VposId)
        REFERENCES Vpos.Vpos(Id)
        ON DELETE CASCADE;
END
GO


-- Core.TimeOutTransaction foreign key (AcquireBankCode)
IF NOT EXISTS (
    SELECT 1
    FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
    WHERE TABLE_SCHEMA = 'Core'
      AND TABLE_NAME = 'TimeOutTransaction'
      AND CONSTRAINT_NAME = 'Fk_TimeOutTransaction_Bank_AcquireBankCode'
)
BEGIN
    ALTER TABLE Core.TimeOutTransaction
    ADD CONSTRAINT Fk_TimeOutTransaction_Bank_AcquireBankCode
        FOREIGN KEY (AcquireBankCode)
        REFERENCES Bank.Bank(Code)
        ON DELETE NO ACTION;
END
GO


-- Core.TimeOutTransaction foreign key (MerchantId)
IF NOT EXISTS (
    SELECT 1
    FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
    WHERE TABLE_SCHEMA = 'Core'
      AND TABLE_NAME = 'TimeOutTransaction'
      AND CONSTRAINT_NAME = 'Fk_TimeOutTransaction_Merchant_MerchantId'
)
BEGIN
    ALTER TABLE Core.TimeOutTransaction
    ADD CONSTRAINT Fk_TimeOutTransaction_Merchant_MerchantId
        FOREIGN KEY (MerchantId)
        REFERENCES Merchant.Merchant(Id)
        ON DELETE CASCADE;
END
GO




-- Drop table

-- DROP TABLE vpos.bank_api_info;
IF NOT EXISTS (
SELECT * FROM INFORMATION_SCHEMA.TABLES 
    WHERE TABLE_SCHEMA = 'Vpos' AND TABLE_NAME = 'BankApiInfo'
)
BEGIN
CREATE TABLE Vpos.BankApiInfo (
	Id uniqueidentifier NOT NULL,
	VposId uniqueidentifier NOT NULL,
	KeyId uniqueidentifier NOT NULL,
	Value varchar(150) NULL,
	CreateDate datetime2 NOT NULL,
	UpdateDate datetime2 NULL,
	CreatedBy varchar(50) NOT NULL,
	LastModifiedBy varchar(50) NULL,
	RecordStatus varchar(50) NOT NULL,
	CONSTRAINT Pk_BankApiInfo PRIMARY KEY (Id)
);
CREATE INDEX IX_BankApiInfo_KeyId ON Vpos.BankApiInfo(KeyId);
CREATE INDEX IX_BankApiInfo_VposId ON Vpos.BankApiInfo(VposId);
END

-- vpos.currency definition

-- Drop table

-- DROP TABLE vpos.currency;
IF NOT EXISTS (
	SELECT * FROM INFORMATION_SCHEMA.TABLES 
    WHERE TABLE_SCHEMA = 'Vpos' AND TABLE_NAME = 'Currency'
)
BEGIN
CREATE TABLE Vpos.Currency (
	Id uniqueidentifier NOT NULL,
	VposId uniqueidentifier NOT NULL,
	CurrencyCode varchar(10) NULL,
	CreateDate datetime2 NOT NULL,
	UpdateDate datetime2 NULL,
	CreatedBy varchar(50) NOT NULL,
	LastModifiedBy varchar(50) NULL,
	RecordStatus varchar(50) NOT NULL,
	CONSTRAINT Pk_Currency PRIMARY KEY (Id)
);
CREATE INDEX IX_Currency_CurrencyCode ON Vpos.Currency(CurrencyCode);
CREATE INDEX IX_Currency_VposId ON Vpos.currency(VposId);
END
-------------------------------------------------

-- Merchant.ApiKey foreign key
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
    WHERE TABLE_SCHEMA = 'Merchant' AND TABLE_NAME = 'ApiKey' AND CONSTRAINT_NAME = 'Fk_ApiKey_Merchant_MerchantId'
)
BEGIN
    ALTER TABLE Merchant.ApiKey
    ADD CONSTRAINT Fk_ApiKey_Merchant_MerchantId
        FOREIGN KEY (MerchantId) REFERENCES Merchant.Merchant(Id) ON DELETE CASCADE;
END
GO


-- Merchant.BankAccount foreign key (BankCode)
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
    WHERE TABLE_SCHEMA = 'Merchant' AND TABLE_NAME = 'BankAccount' AND CONSTRAINT_NAME = 'Fk_BankAccount_Bank_BankCode'
)
BEGIN
    ALTER TABLE Merchant.BankAccount
    ADD CONSTRAINT Fk_BankAccount_Bank_BankCode
        FOREIGN KEY (BankCode) REFERENCES Bank.Bank(Code) ON DELETE NO ACTION;
END
GO


-- Merchant.BankAccount foreign key (MerchantId)
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
    WHERE TABLE_SCHEMA = 'Merchant' AND TABLE_NAME = 'BankAccount' AND CONSTRAINT_NAME = 'Fk_BankAccount_Merchant_MerchantId'
)
BEGIN
    ALTER TABLE Merchant.BankAccount
    ADD CONSTRAINT Fk_BankAccount_Merchant_MerchantId
        FOREIGN KEY (MerchantId) REFERENCES Merchant.Merchant(Id) ON DELETE CASCADE;
END
GO


-- Vpos.BankApiInfo foreign key (KeyId)
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
    WHERE TABLE_SCHEMA = 'Vpos' AND TABLE_NAME = 'BankApiInfo' AND CONSTRAINT_NAME = 'Fk_BankApiInfo_ApiKey_KeyId'
)
BEGIN
    ALTER TABLE Vpos.BankApiInfo
    ADD CONSTRAINT Fk_BankApiInfo_ApiKey_KeyId
        FOREIGN KEY (KeyId) REFERENCES Bank.ApiKey(Id) ON DELETE NO ACTION;
END
GO


-- Vpos.BankApiInfo foreign key (VposId)
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
    WHERE TABLE_SCHEMA = 'Vpos' AND TABLE_NAME = 'BankApiInfo' AND CONSTRAINT_NAME = 'Fk_BankApiInfo_Vpos_VposId'
)
BEGIN
    ALTER TABLE Vpos.BankApiInfo
    ADD CONSTRAINT Fk_BankApiInfo_Vpos_VposId
        FOREIGN KEY (VposId) REFERENCES Vpos.Vpos(Id) ON DELETE CASCADE;
END
GO


-- Vpos.Currency foreign key (CurrencyCode)
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
    WHERE TABLE_SCHEMA = 'Vpos' AND TABLE_NAME = 'Currency' AND CONSTRAINT_NAME = 'Fk_Currency_Currency_CurrencyCode'
)
BEGIN
    ALTER TABLE Vpos.Currency
    ADD CONSTRAINT Fk_Currency_Currency_CurrencyCode
        FOREIGN KEY (CurrencyCode) REFERENCES Core.Currency(Code) ON DELETE NO ACTION;
END
GO


-- Vpos.Currency foreign key (VposId)
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
    WHERE TABLE_SCHEMA = 'Vpos' AND TABLE_NAME = 'Currency' AND CONSTRAINT_NAME = 'Fk_Currency_Vpos_VposId'
)
BEGIN
    ALTER TABLE Vpos.Currency
    ADD CONSTRAINT Fk_Currency_Vpos_VposId
        FOREIGN KEY (VposId) REFERENCES Vpos.Vpos(Id) ON DELETE CASCADE;
END
GO


-- Vpos.Vpos foreign key (AcquireBankId)
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
    WHERE TABLE_SCHEMA = 'Vpos' AND TABLE_NAME = 'Vpos' AND CONSTRAINT_NAME = 'Fk_Vpos_AcquireBank_AcquireBankId'
)
BEGIN
    ALTER TABLE Vpos.Vpos
    ADD CONSTRAINT Fk_Vpos_AcquireBank_AcquireBankId
        FOREIGN KEY (AcquireBankId) REFERENCES Bank.AcquireBank(Id) ON DELETE CASCADE;
END
GO



-- hpp.hosted_payment definition

-- Drop table

-- DROP TABLE hpp.hosted_payment;
IF NOT EXISTS (
	SELECT * FROM INFORMATION_SCHEMA.TABLES 
    WHERE TABLE_SCHEMA = 'Hpp' AND TABLE_NAME = 'HostedPayment'
)
BEGIN
CREATE TABLE Hpp.HostedPayment (
	Id uniqueidentifier NOT NULL,
	TrackingId varchar(24) NOT NULL,
	HppStatus varchar(50) NOT NULL,
	HppPaymentStatus varchar(50) NOT NULL,
	WebhookStatus varchar(50) NOT NULL,
	OrderId varchar(24) NOT NULL,
	Amount numeric(18, 4) NOT NULL,
	Currency int NOT NULL,
	Is3dRequired bit NOT NULL,
	CallbackUrl varchar(250) NOT NULL,
	"Name" varchar(150) NULL,
	Surname varchar(150) NULL,
	Email varchar(256) NOT NULL,
	PhoneNumber varchar(10) NOT NULL,
	ClientIpAddress varchar(50) NOT NULL,
	LanguageCode varchar(2) NOT NULL,
	ExpiryDate datetime2 NOT NULL,
	MerchantId uniqueidentifier NOT NULL,
	MerchantName varchar(150) NOT NULL,
	MerchantNumber varchar(15) NOT NULL,
	WebhookRetryCount int NOT NULL,
	CreateDate datetime2 NOT NULL,
	UpdateDate datetime2 NULL,
	CreatedBy varchar(50) NOT NULL,
	LastModifiedBy varchar(50) NULL,
	RecordStatus varchar(50) NOT NULL,
	PageViewType varchar(50) NOT NULL DEFAULT '',
	ReturnUrl varchar(250) NULL,
	EnableInstallments bit NOT NULL DEFAULT 0,
	CommissionFromCustomer bit NOT NULL DEFAULT 0,
	CONSTRAINT Pk_HostedPayment PRIMARY KEY (Id)
);
CREATE INDEX IX_HostedPayment_ExpiryDate ON Hpp.HostedPayment(ExpiryDate);
CREATE UNIQUE INDEX IX_HostedPayment_TrackingId ON Hpp.HostedPayment(TrackingId);
CREATE INDEX IX_HostedPayment_WebhookStatus ON Hpp.HostedPayment(WebhookStatus);

END
-- hpp.hosted_payment_transaction definition

-- Drop table

-- DROP TABLE hpp.hosted_payment_transaction;
IF NOT EXISTS (
	SELECT * FROM INFORMATION_SCHEMA.TABLES 
    WHERE TABLE_SCHEMA = 'Hpp' AND TABLE_NAME = 'HostedPaymentTransaction'
)
BEGIN
CREATE TABLE Hpp.HostedPaymentTransaction (
	Id uniqueidentifier NOT NULL,
	MerchantTransactionId uniqueidentifier NOT NULL,
	TrackingId varchar(24) NOT NULL,
	TransactionType varchar(50) NOT NULL,
	TransactionDate datetime2 NOT NULL,
	HppPaymentStatus varchar(50) NOT NULL,
	OrderId varchar(24) NULL,
	Amount numeric(18, 4) NOT NULL,
	InstallmentCount int NOT NULL,
	Currency int NOT NULL,
	Is3dRequired bit NOT NULL,
	CreateDate datetime2 NOT NULL,
	UpdateDate datetime2 NULL,
	CreatedBy varchar(50) NOT NULL,
	LastModifiedBy varchar(50) NULL,
	RecordStatus varchar(50) NOT NULL,
	ThreeDSessionId varchar(200) NULL,
	CONSTRAINT Pk_HostedPaymentTransaction PRIMARY KEY (Id)
);
CREATE INDEX IX_HostedPaymentTransaction_TrackingId ON Hpp.HostedPaymentTransaction(TrackingId);
CREATE INDEX IX_HostedPaymentTransaction_TransactionDate ON Hpp.HostedPaymentTransaction(TransactionDate);
END

-- hpp.hosted_payment_installment definition

-- Drop table

-- DROP TABLE hpp.hosted_payment_installment;
IF NOT EXISTS (
	SELECT * FROM INFORMATION_SCHEMA.TABLES 
    WHERE TABLE_SCHEMA = 'Hpp' AND TABLE_NAME = 'HostedPaymentInstallment'
)
BEGIN
CREATE TABLE Hpp.HostedPaymentInstallment (
	Id uniqueidentifier NOT NULL,
	HostedPaymentId uniqueidentifier NOT NULL,
	Installment int NOT NULL,
	CreateDate datetime2 NOT NULL,
	UpdateDate datetime2 NULL,
	CreatedBy varchar(50) NOT NULL,
	LastModifiedBy varchar(50) NULL,
	RecordStatus varchar(50) NOT NULL,
	Amount numeric(18, 4) NULL,
	CardNetwork varchar(50) NOT NULL DEFAULT '',
	CONSTRAINT Pk_HostedPaymentInstallment PRIMARY KEY (Id),
	CONSTRAINT Fk_HostedPaymentInstallment_HostedPayment_HostedPaymentId 
	FOREIGN KEY (HostedPaymentId) REFERENCES Hpp.HostedPayment(Id) ON DELETE NO ACTION
);
CREATE INDEX IX_HostedPaymentInstallment_HostedPaymentId ON Hpp.HostedPaymentInstallment(HostedPaymentId);
END
-- "limit".merchant_daily_usage definition

-- Drop table

-- DROP TABLE "limit".merchant_daily_usage;
IF NOT EXISTS (
    SELECT * FROM INFORMATION_SCHEMA.TABLES 
    WHERE TABLE_SCHEMA = 'Limit' AND TABLE_NAME = 'MerchantDailyUsage'
)
BEGIN
    CREATE TABLE "Limit"."MerchantDailyUsage" (
        Id uniqueidentifier NOT NULL,
        Date datetime2 NOT NULL,
        Count int NOT NULL,
        Amount numeric(18, 4) NOT NULL,
        MerchantId uniqueidentifier NOT NULL,
        Currency varchar(50) NOT NULL,
        CreateDate datetime2 NOT NULL,
        UpdateDate datetime2 NULL,
        CreatedBy varchar(50) NOT NULL,
        LastModifiedBy varchar(50) NULL,
        RecordStatus varchar(50) NOT NULL,
        TransactionLimitType varchar(50) NOT NULL DEFAULT '',
        CONSTRAINT PK_MerchantDailyUsage PRIMARY KEY (Id)
    );

    CREATE INDEX IX_MerchantDailyUsage_MerchantId ON "Limit"."MerchantDailyUsage"(MerchantId);
END
-- "limit".merchant_limit definition

-- Drop table

-- DROP TABLE "limit".merchant_limit;
IF NOT EXISTS (
    SELECT * FROM INFORMATION_SCHEMA.TABLES 
    WHERE TABLE_SCHEMA = 'Limit' AND TABLE_NAME = 'MerchantLimit'
)
BEGIN
    CREATE TABLE "Limit"."MerchantLimit" (
        Id uniqueidentifier NOT NULL,
        TransactionLimitType varchar(50) NOT NULL,
        Period varchar(50) NOT NULL,
        LimitType varchar(50) NOT NULL,
        MaxPiece int NULL,
        MaxAmount numeric(18, 4) NULL,
        MerchantId uniqueidentifier NOT NULL,
        Currency varchar(50) NULL,
        CreateDate datetime2 NOT NULL,
        UpdateDate datetime2 NULL,
        CreatedBy varchar(50) NOT NULL,
        LastModifiedBy varchar(50) NULL,
        RecordStatus varchar(50) NOT NULL,
        CONSTRAINT PK_MerchantLimit PRIMARY KEY (Id)
    );

    CREATE INDEX IX_MerchantLimit_MerchantId ON "Limit"."MerchantLimit"(MerchantId);
END

-- "limit".merchant_monthly_usage definition

-- Drop table

-- DROP TABLE "limit".merchant_monthly_usage;
IF NOT EXISTS (
    SELECT * FROM INFORMATION_SCHEMA.TABLES 
    WHERE TABLE_SCHEMA = 'Limit' AND TABLE_NAME = 'MerchantMonthlyUsage'
)
BEGIN
    CREATE TABLE "Limit"."MerchantMonthlyUsage" (
        Id uniqueidentifier NOT NULL,
        Date datetime2 NOT NULL,
        Count int NOT NULL,
        Amount numeric(18, 4) NOT NULL,
        MerchantId uniqueidentifier NOT NULL,
        Currency varchar(50) NOT NULL,
        CreateDate datetime2 NOT NULL,
        UpdateDate datetime2 NULL,
        CreatedBy varchar(50) NOT NULL,
        LastModifiedBy varchar(50) NULL,
        RecordStatus varchar(50) NOT NULL,
        TransactionLimitType varchar(50) NOT NULL DEFAULT '',
        CONSTRAINT PK_MerchantMonthlyUsage PRIMARY KEY (Id)
    );

    CREATE INDEX IX_MerchantMonthlyUsage_MerchantId ON "Limit"."MerchantMonthlyUsage"(MerchantId);
END
-- "limit".merchant_daily_usage foreign keys

-- Limit.MerchantDailyUsage foreign key (MerchantId)
IF NOT EXISTS (
    SELECT 1
    FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
    WHERE TABLE_SCHEMA = 'Limit'
      AND TABLE_NAME = 'MerchantDailyUsage'
      AND CONSTRAINT_NAME = 'Fk_MerchantDailyUsage_Merchant_MerchantId'
)
BEGIN
    ALTER TABLE Limit.MerchantDailyUsage
    ADD CONSTRAINT Fk_MerchantDailyUsage_Merchant_MerchantId
        FOREIGN KEY (MerchantId)
        REFERENCES Merchant.Merchant(Id)
        ON DELETE CASCADE;
END
GO


-- Limit.MerchantLimit foreign key (MerchantId)
IF NOT EXISTS (
    SELECT 1
    FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
    WHERE TABLE_SCHEMA = 'Limit'
      AND TABLE_NAME = 'MerchantLimit'
      AND CONSTRAINT_NAME = 'Fk_MerchantLimit_Merchant_MerchantId'
)
BEGIN
    ALTER TABLE Limit.MerchantLimit
    ADD CONSTRAINT Fk_MerchantLimit_Merchant_MerchantId
        FOREIGN KEY (MerchantId)
        REFERENCES Merchant.Merchant(Id)
        ON DELETE CASCADE;
END
GO


-- Limit.MerchantMonthlyUsage foreign key (MerchantId)
IF NOT EXISTS (
    SELECT 1
    FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
    WHERE TABLE_SCHEMA = 'Limit'
      AND TABLE_NAME = 'MerchantMonthlyUsage'
      AND CONSTRAINT_NAME = 'Fk_MerchantMonthlyUsage_Merchant_MerchantId'
)
BEGIN
    ALTER TABLE Limit.MerchantMonthlyUsage
    ADD CONSTRAINT Fk_MerchantMonthlyUsage_Merchant_MerchantId
        FOREIGN KEY (MerchantId)
        REFERENCES Merchant.Merchant(Id)
        ON DELETE CASCADE;
END
GO


-- link.link definition

-- Drop table

-- DROP TABLE link.link;
IF NOT EXISTS (
	SELECT * FROM INFORMATION_SCHEMA.TABLES 
    WHERE TABLE_SCHEMA = 'Link' AND TABLE_NAME = 'Link'
)
BEGIN
CREATE TABLE Link.Link (
	Id uniqueidentifier NOT NULL,
	LinkStatus varchar(50) NOT NULL,
	LinkType varchar(50) NOT NULL,
	ExpiryDate datetime2 NOT NULL,
	CurrentUsageCount int NOT NULL,
	MaxUsageCount int NOT NULL,
	OrderId varchar(24) NULL,
	LinkAmountType varchar(50) NOT NULL,
	Amount numeric(18, 4) NOT NULL,
	Currency int NOT NULL,
	CommissionFromCustomer bit NOT NULL,
	Is3dRequired bit NOT NULL,
	MerchantName varchar(150) NOT NULL DEFAULT '',
	MerchantNumber varchar(15) NOT NULL DEFAULT '',
	ProductName varchar(100) NOT NULL DEFAULT '',
	ProductDescription varchar(400) NOT NULL DEFAULT '',
	ReturnUrl varchar(150) NULL,
	IsNameRequired bit NOT NULL,
	IsEmailRequired bit NOT NULL,
	IsPhoneNumberRequired bit NOT NULL,
	IsAddressRequired bit NOT NULL,
	IsNoteRequired bit NOT NULL,
	CreateDate datetime2 NOT NULL,
	UpdateDate datetime2 NULL,
	CreatedBy varchar(50) NOT NULL,
	LastModifiedBy varchar(50) NULL,
	RecordStatus varchar(50) NOT NULL,
	LinkPaymentStatus varchar(50) NOT NULL DEFAULT '',
	LinkCode varchar(24) NOT NULL DEFAULT '',
	MerchantId uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000',
	SubMerchantId uniqueidentifier NULL,
	SubMerchantName varchar(150) NULL DEFAULT '',
	SubMerchantNumber varchar(15) NULL DEFAULT '',
	CONSTRAINT Pk_Link PRIMARY KEY (Id)
);
CREATE INDEX IX_Link_ExpiryDate ON Link.Link(ExpiryDate);
CREATE UNIQUE INDEX IX_Link_LinkCode ON Link.Link(LinkCode);
END

-- link.link_customer definition

-- Drop table

-- DROP TABLE link.link_customer;
IF NOT EXISTS (
	SELECT * FROM INFORMATION_SCHEMA.TABLES 
    WHERE TABLE_SCHEMA = 'Link' AND TABLE_NAME = 'LinkCustomer'
)
BEGIN
CREATE TABLE Link.LinkCustomer (
	Id uniqueidentifier NOT NULL,
	LinkTransactionId uniqueidentifier NOT NULL,
	"Name" varchar(100) NULL,
	Email varchar(100) NULL,
	PhoneNumber varchar(30) NULL,
	Address varchar(256) NULL,
	Note varchar(256) NULL,
	CreateDate datetime2 NOT NULL,
	UpdateDate datetime2 NULL,
	CreatedBy varchar(50) NOT NULL,
	LastModifiedBy varchar(50) NULL,
	RecordStatus varchar(50) NOT NULL,
	CONSTRAINT Pk_LinkCustomer PRIMARY KEY (Id)
);
CREATE INDEX IX_LinkCustomer_LinkTransactionId ON Link.LinkCustomer(LinkTransactionId);
END

-- link.link_transaction definition

-- Drop table

-- DROP TABLE link.link_transaction;
IF NOT EXISTS (
	SELECT * FROM INFORMATION_SCHEMA.TABLES 
    WHERE TABLE_SCHEMA = 'Link' AND TABLE_NAME = 'LinkTransaction'
)
BEGIN
CREATE TABLE Link.LinkTransaction (
	Id uniqueidentifier NOT NULL,
	MerchantTransactionId uniqueidentifier NOT NULL,
	LinkCode varchar(24) NOT NULL,
	LinkPaymentStatus varchar(50) NOT NULL,
	LinkType varchar(50) NOT NULL,
	OrderId varchar(24) NULL,
	Amount numeric(18, 4) NOT NULL,
	Currency int NOT NULL,
	CommissionFromCustomer bit NOT NULL,
	Is3dRequired bit NOT NULL,
	CreateDate datetime2 NOT NULL,
	UpdateDate datetime2 NULL,
	CreatedBy varchar(50) NOT NULL,
	LastModifiedBy varchar(50) NULL,
	RecordStatus varchar(50) NOT NULL,
	CustomerId uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000',
	CommissionAmount numeric NOT NULL DEFAULT 0.0,
	InstallmentCount int NOT NULL DEFAULT 0,
	TransactionDate datetime2 NOT NULL DEFAULT '0001-01-01',
	TransactionType varchar(50) NOT NULL DEFAULT '',
	ThreeDSessionId varchar(200) NULL,
	CONSTRAINT Pk_LinkTransaction PRIMARY KEY (Id)
);
CREATE INDEX IX_LinkTransaction_LinkCode ON Link.LinkTransaction(LinkCode);
CREATE INDEX IX_LinkTransaction_TransactionDate ON Link.LinkTransaction(TransactionDate);
END

-- link.link_installment definition

-- Drop table

-- DROP TABLE link.link_installment;
IF NOT EXISTS (
    SELECT * FROM INFORMATION_SCHEMA.TABLES 
    WHERE TABLE_SCHEMA = 'Link' AND TABLE_NAME = 'LinkInstallment'
	)
BEGIN
CREATE TABLE Link.LinkInstallment (
	Id uniqueidentifier NOT NULL,
	Installment int NOT NULL,
	CreateDate datetime2 NOT NULL,
	UpdateDate datetime2 NULL,
	CreatedBy varchar(50) NOT NULL,
	LastModifiedBy varchar(50) NULL,
	RecordStatus varchar(50) NOT NULL,
	LinkId uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000',
	CONSTRAINT Pk_LinkInstallment PRIMARY KEY (Id),
	CONSTRAINT Fk_LinkInstallment_Link_LinkId 
	FOREIGN KEY (LinkId) REFERENCES Link.Link(Id) ON DELETE NO ACTION
);
CREATE INDEX IX_LinkInstallment_LinkId ON Link.LinkInstallment(LinkId);
END
-- posting.batch_status definition

-- Drop table

-- DROP TABLE posting.batch_status;
IF NOT EXISTS (
   SELECT * FROM INFORMATION_SCHEMA.TABLES 
    WHERE TABLE_SCHEMA = 'Posting' AND TABLE_NAME = 'BatchStatus'
)
BEGIN
CREATE TABLE Posting.BatchStatus (
	Id uniqueidentifier NOT NULL,
	PostingBatchLevel varchar(50) NOT NULL,
	BatchSummary varchar(200) NOT NULL,
	IsCriticalError bit NOT NULL,
	PostingDate date NOT NULL,
	StartTime datetime2 NOT NULL,
	FinishTime datetime2 NOT NULL,
	BatchStatus varchar(50) NOT NULL,
	BatchOrder int NOT NULL,
	CreateDate datetime2 NOT NULL,
	UpdateDate datetime2 NULL,
	CreatedBy varchar(50) NOT NULL,
	LastModifiedBy varchar(50) NULL,
	RecordStatus varchar(50) NOT NULL,
	CONSTRAINT Pk_BatchStatus PRIMARY KEY (Id)
);
CREATE INDEX IX_BatchStatus_PostingDate ON Posting.BatchStatus(PostingDate);
END

-- posting.item definition

-- Drop table

-- DROP TABLE posting.item;
IF NOT EXISTS (
    SELECT * FROM INFORMATION_SCHEMA.TABLES 
    WHERE TABLE_SCHEMA = 'Posting' AND TABLE_NAME = 'Item'
)
BEGIN
CREATE TABLE Posting.Item (
	Id uniqueidentifier NOT NULL,
	MerchantId uniqueidentifier NOT NULL,
	ErrorCount int NOT NULL,
	TotalCount int NOT NULL,
	PostingDate date NOT NULL,
	BatchStatus varchar(50) NOT NULL,
	CreateDate datetime2 NOT NULL,
	UpdateDate datetime2 NULL,
	CreatedBy varchar(50) NOT NULL,
	LastModifiedBy varchar(50) NULL,
	RecordStatus varchar(50) NOT NULL,
	CONSTRAINT Pk_Item PRIMARY KEY (Id)
);
END

IF NOT EXISTS (
    SELECT * FROM sys.indexes 
    WHERE name = 'IX_Item_MerchantId' AND object_id = OBJECT_ID('[Posting].[Item]')
)
BEGIN
    CREATE INDEX [IX_Item_MerchantId] ON [Posting].[Item] ([MerchantId]);
END

IF NOT EXISTS (
    SELECT * FROM sys.indexes 
    WHERE name = 'IX_Item_MerchantId_PostingDate' AND object_id = OBJECT_ID('[Posting].[Item]')
)
BEGIN
    CREATE INDEX [IX_Item_MerchantId_PostingDate] ON [Posting].[Item] ([PostingDate]);

END
-- posting.posting_additional_transaction definition

-- Drop table

-- DROP TABLE posting.posting_additional_transaction;
IF NOT EXISTS (
    SELECT * FROM INFORMATION_SCHEMA.TABLES 
    WHERE TABLE_SCHEMA = 'Posting' AND TABLE_NAME = 'PostingAdditionalTransaction'
)
BEGIN
CREATE TABLE Posting.PostingAdditionalTransaction (
	Id uniqueidentifier NOT NULL,
	AcquireBankCode int NOT NULL DEFAULT 0,
	Amount numeric(18, 4) NOT NULL DEFAULT 0.0,
	AmountWithoutBankCommission numeric(18, 4) NOT NULL DEFAULT 0.0,
	AmountWithoutCommissions numeric(18, 4) NOT NULL DEFAULT 0.0,
	BTransStatus varchar(50) NOT NULL DEFAULT '',
	BankCommissionAmount numeric(18, 4) NOT NULL DEFAULT 0.0,
	BankCommissionRate numeric(4, 2) NOT NULL DEFAULT 0.0,
	BatchStatus varchar(50) NOT NULL DEFAULT '',
	BlockageStatus varchar(50) NOT NULL DEFAULT '',
	CardNumber varchar(50) NOT NULL DEFAULT '',
	ConversationId varchar(50) NOT NULL DEFAULT '',
	CreateDate datetime2 NOT NULL DEFAULT '0001-01-01',
	CreatedBy varchar(50) NOT NULL DEFAULT '',
	Currency int NOT NULL DEFAULT 0,
	InstallmentCount int NOT NULL DEFAULT 0,
	LastModifiedBy varchar(50) NULL,
	MerchantId uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000',
	MerchantTransactionId uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000',
	OldPaymentDate date NOT NULL DEFAULT '0001-01-01',
	OrderId varchar(50) NULL,
	PaymentDate date NOT NULL DEFAULT '0001-01-01',
	PfCommissionAmount numeric(18, 4) NOT NULL DEFAULT 0.0,
	PfCommissionRate numeric(4, 2) NOT NULL DEFAULT 0.0,
	PfNetCommissionAmount numeric(18, 4) NOT NULL DEFAULT 0.0,
	PfPerTransactionFee numeric(18, 4) NOT NULL DEFAULT 0.0,
	PointAmount numeric(18, 4) NOT NULL DEFAULT 0.0,
	PostingBalanceId uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000',
	PostingBankBalanceId uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000',
	PostingDate date NOT NULL DEFAULT '0001-01-01',
	PricingProfileItemId uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000',
	PricingProfileNumber text NOT NULL DEFAULT '',
	RecordStatus varchar(50) NOT NULL DEFAULT '',
	TransactionDate date NOT NULL DEFAULT '0001-01-01',
	TransactionEndDate datetime2 NOT NULL DEFAULT '0001-01-01',
	TransactionStartDate datetime2 NOT NULL DEFAULT '0001-01-01',
	TransactionType varchar(50) NOT NULL DEFAULT '',
	UpdateDate datetime2 NULL,
	VposId uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000',
	MerchantDeductionId uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000',
	RelatedPostingBalanceId uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000',
	SubMerchantId uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000',
	SubMerchantName varchar(150) NULL,
	SubMerchantNumber varchar(15) NULL,
	EasySubMerchantName varchar(150) NULL,
	EasySubMerchantNumber varchar(15) NULL,
	AmountWithoutParentMerchantCommission numeric(18, 4) NOT NULL DEFAULT 0.0,
	ParentMerchantCommissionAmount numeric(18, 4) NOT NULL DEFAULT 0.0,
	ParentMerchantCommissionRate numeric(18, 4) NOT NULL DEFAULT 0.0,
	CONSTRAINT Pk_PostingAdditionalTransaction PRIMARY KEY (Id)
);

END

IF NOT EXISTS (
    SELECT * FROM sys.indexes 
    WHERE name = 'IX_PostingAdditionalTransaction_BatchStatus_RecordStatus'
      AND object_id = OBJECT_ID('[Posting].[PostingAdditionalTransaction]')
)
BEGIN
    CREATE INDEX [IX_PostingAdditionalTransaction_BatchStatus_RecordStatus]
    ON [Posting].[PostingAdditionalTransaction] ([BatchStatus], [RecordStatus]);
END



-- posting."transaction" definition

-- Drop table

-- DROP TABLE posting."transaction";
IF NOT EXISTS (
    SELECT * FROM INFORMATION_SCHEMA.TABLES 
    WHERE TABLE_SCHEMA = 'Posting' AND TABLE_NAME = 'Transaction'
)
BEGIN
    CREATE TABLE Posting."Transaction" (
        Id uniqueidentifier NOT NULL,
        MerchantId uniqueidentifier NOT NULL,
        TransactionType varchar(50) NOT NULL,
        TransactionDate date NOT NULL,
        PostingDate date NOT NULL,
        PaymentDate date NOT NULL,
        OldPaymentDate date NOT NULL,
        CardNumber varchar(50) NOT NULL,
        OrderId varchar(50) NULL,
        InstallmentCount int NOT NULL,
        Currency int NOT NULL,
        Amount numeric(18, 4) NOT NULL,
        PointAmount numeric(18, 4) NOT NULL,
        BankCommissionRate numeric(4, 2) NOT NULL,
        BankCommissionAmount numeric(18, 4) NOT NULL,
        AmountWithoutBankCommission numeric(18, 4) NOT NULL,
        PfCommissionRate numeric(4, 2) NOT NULL,
        PfPerTransactionFee numeric(18, 4) NOT NULL,
        PfCommissionAmount numeric(18, 4) NOT NULL,
        PfNetCommissionAmount numeric(18, 4) NOT NULL,
        AmountWithoutCommissions numeric(18, 4) NOT NULL,
        PricingProfileNumber text NOT NULL,
        BatchStatus varchar(50) NOT NULL,
        BlockageStatus varchar(50) NOT NULL,
        MerchantTransactionId uniqueidentifier NOT NULL,
        PostingBankBalanceId uniqueidentifier NOT NULL,
        PostingBalanceId uniqueidentifier NOT NULL,
        AcquireBankCode int NOT NULL,
        CreateDate datetime2 NOT NULL,
        UpdateDate datetime2 NULL,
        CreatedBy varchar(50) NOT NULL,
        LastModifiedBy varchar(50) NULL,
        RecordStatus varchar(50) NOT NULL,
        PricingProfileItemId uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000',
        TransactionEndDate datetime2 NOT NULL DEFAULT '0001-01-01',
        TransactionStartDate datetime2 NOT NULL DEFAULT '0001-01-01',
        VposId uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000',
        BTransStatus varchar(50) NOT NULL DEFAULT '',
        ConversationId varchar(50) NOT NULL DEFAULT '',
        AmountWithoutParentMerchantCommission numeric(18, 4) NOT NULL DEFAULT 0.0,
        ParentMerchantCommissionAmount numeric(18, 4) NOT NULL DEFAULT 0.0,
        ParentMerchantCommissionRate numeric(18, 4) NOT NULL DEFAULT 0.0,
        ParentMerchantId uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000',
        EasySubMerchantName varchar(150) NULL,
        EasySubMerchantNumber varchar(15) NULL,
        MerchantDeductionId uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000',
        RelatedPostingBalanceId uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000',
        SubMerchantId uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000',
        SubMerchantName varchar(150) NULL,
        SubMerchantNumber varchar(15) NULL,
        CONSTRAINT PK_Transaction PRIMARY KEY (Id)
    );

    CREATE INDEX IX_Transaction_BatchStatus_RecordStatus1 
        ON Posting."Transaction"(BatchStatus, RecordStatus);

    CREATE UNIQUE INDEX IX_Transaction_MerchantTransactionId 
        ON Posting."Transaction"(MerchantTransactionId);
END


-- posting.balance definition

-- Drop table

-- DROP TABLE posting.balance;
IF NOT EXISTS (
    SELECT * FROM INFORMATION_SCHEMA.TABLES 
    WHERE TABLE_SCHEMA = 'Posting' AND TABLE_NAME = 'Balance'
)
BEGIN
CREATE TABLE Posting.Balance (
	Id uniqueidentifier NOT NULL,
	MerchantId uniqueidentifier NOT NULL,
	TransactionDate date NOT NULL,
	PostingDate date NOT NULL,
	PaymentDate date NOT NULL,
	Currency int NOT NULL,
	TotalAmount numeric(18, 4) NOT NULL,
	TotalPointAmount numeric(18, 4) NOT NULL,
	TotalBankCommissionAmount numeric(18, 4) NOT NULL,
	TotalAmountWithoutBankCommission numeric(18, 4) NOT NULL,
	TotalPfCommissionAmount numeric(18, 4) NOT NULL,
	TotalPfNetCommissionAmount numeric(18, 4) NOT NULL,
	TotalAmountWithoutCommissions numeric(18, 4) NOT NULL,
	TotalDueAmount numeric(18, 4) NOT NULL,
	TotalPayingAmount numeric(18, 4) NOT NULL,
	TotalChargebackAmount numeric(18, 4) NOT NULL,
	TotalSuspiciousAmount numeric(18, 4) NOT NULL,
	MoneyTransferPaymentDate date NOT NULL,
	MoneyTransferStatus varchar(50) NOT NULL,
	MoneyTransferReferenceId uniqueidentifier NOT NULL,
	RetryCount int NOT NULL,
	BatchStatus varchar(50) NOT NULL,
	BlockageStatus varchar(50) NOT NULL,
	CreateDate datetime2 NOT NULL,
	UpdateDate datetime2 NULL,
	CreatedBy varchar(50) NOT NULL,
	LastModifiedBy varchar(50) NULL,
	RecordStatus varchar(50) NOT NULL,
	PostingBalanceType varchar(50) NOT NULL DEFAULT '',
	Iban varchar(26) NULL,
	MoneyTransferBankCode int NOT NULL DEFAULT 0,
	MoneyTransferBankName varchar(50) NULL,
	TransactionSourceId uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000',
	BTransStatus varchar(50) NOT NULL DEFAULT '',
	AccountingStatus varchar(50) NOT NULL DEFAULT '',
	TotalExcessReturnAmount numeric(18, 4) NOT NULL DEFAULT 0.0,
	TotalNegativeBalanceAmount numeric(18, 4) NOT NULL DEFAULT 0.0,
	OldPaymentDate date NOT NULL DEFAULT '0001-01-01',
	TransactionCount int NOT NULL DEFAULT 0,
	PostingPaymentChannel varchar(50) NOT NULL DEFAULT '',
	WalletNumber varchar(26) NULL,
	ParentMerchantId uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000',
	TotalParentMerchantCommissionAmount numeric(18, 4) NOT NULL DEFAULT 0.0,
	TotalSubmerchantDeductionAmount numeric(18, 4) NOT NULL DEFAULT 0.0,
	CONSTRAINT Pk_Balance PRIMARY KEY (Id)
);
END
IF NOT EXISTS (
    SELECT * FROM sys.indexes 
    WHERE name = 'IX_Balance_MerchantId' 
      AND object_id = OBJECT_ID('[Posting].[Balance]')
)
BEGIN
    CREATE INDEX [IX_Balance_MerchantId] ON [Posting].[Balance] ([MerchantId]);
END
IF NOT EXISTS (
    SELECT * FROM sys.indexes 
    WHERE name = 'IX_Balance_MoneyTransferStatus_BTransStatus' 
      AND object_id = OBJECT_ID('[Posting].[Balance]')
)
BEGIN
    CREATE INDEX [IX_Balance_MoneyTransferStatus_BTransStatus]
    ON [Posting].[Balance] ([MoneyTransferStatus], [BTransStatus]);
END

-- posting.bank_balance definition

-- Drop table

-- DROP TABLE posting.bank_balance;
IF NOT EXISTS (
    SELECT * FROM INFORMATION_SCHEMA.TABLES 
    WHERE TABLE_SCHEMA = 'Posting' AND TABLE_NAME = 'BankBalance'
)
BEGIN
CREATE TABLE Posting.BankBalance (
	Id uniqueidentifier NOT NULL,
	MerchantId uniqueidentifier NOT NULL,
	AcquireBankCode int NOT NULL,
	PostingDate date NOT NULL,
	PaymentDate date NOT NULL,
	Currency int NOT NULL,
	TotalAmount numeric(18, 4) NOT NULL,
	TotalPointAmount numeric(18, 4) NOT NULL,
	TotalBankCommissionAmount numeric(18, 4) NOT NULL,
	TotalAmountWithoutBankCommission numeric(18, 4) NOT NULL,
	TotalPfCommissionAmount numeric(18, 4) NOT NULL,
	TotalPfNetCommissionAmount numeric(18, 4) NOT NULL,
	TotalAmountWithoutCommissions numeric(18, 4) NOT NULL,
	TotalPayingAmount numeric(18, 4) NOT NULL,
	BatchStatus varchar(50) NOT NULL,
	BlockageStatus varchar(50) NOT NULL,
	CreateDate datetime2 NOT NULL,
	UpdateDate datetime2 NULL,
	CreatedBy varchar(50) NOT NULL,
	LastModifiedBy varchar(50) NULL,
	RecordStatus varchar(50) NOT NULL,
	TransactionDate date NOT NULL DEFAULT '0001-01-01',
	PostingBalanceId uniqueidentifier NULL,
	AccountingStatus varchar(50) NOT NULL DEFAULT '',
	OldPaymentDate date NOT NULL DEFAULT '0001-01-01',
	TotalReturnAmount numeric(18, 4) NOT NULL DEFAULT 0.0,
	TransactionCount int NOT NULL DEFAULT 0,
	ParentMerchantId uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000',
	TotalParentMerchantCommissionAmount numeric(18, 4) NOT NULL DEFAULT 0.0,
	CONSTRAINT Pk_BankBalance PRIMARY KEY (Id)
);
END
IF NOT EXISTS (
    SELECT * FROM sys.indexes 
    WHERE name = 'IX_BankBalance_MerchantId' 
      AND object_id = OBJECT_ID('[Posting].[BankBalance]')
)
BEGIN
    CREATE INDEX [IX_BankBalance_MerchantId]
    ON [Posting].[BankBalance] ([MerchantId]);
END
IF NOT EXISTS (
    SELECT * FROM sys.indexes 
    WHERE name = 'IX_BankBalance_PostingBalanceId' 
      AND object_id = OBJECT_ID('[Posting].[BankBalance]')
)
BEGIN
    CREATE INDEX [IX_BankBalance_PostingBalanceId]
    ON [Posting].[BankBalance] ([PostingBalanceId]);
END
-- posting.posting_bill definition

-- Drop table

-- DROP TABLE posting.posting_bill;
IF NOT EXISTS (
    SELECT * FROM INFORMATION_SCHEMA.TABLES 
    WHERE TABLE_SCHEMA = 'Posting' AND TABLE_NAME = 'PostingBill'
)
BEGIN
CREATE TABLE Posting.PostingBill (
	Id uniqueidentifier NOT NULL,
	MerchantId uniqueidentifier NOT NULL,
	TotalAmount numeric(18, 4) NOT NULL,
	TotalPfCommissionAmount numeric(18, 4) NOT NULL,
	TotalPayingAmount numeric(18, 4) NOT NULL,
	TotalDueAmount numeric(18, 4) NOT NULL,
	ClientReferenceId uniqueidentifier NOT NULL,
	BillUrl text NULL,
	CreateDate datetime2 NOT NULL,
	UpdateDate datetime2 NULL,
	CreatedBy varchar(50) NOT NULL,
	LastModifiedBy varchar(50) NULL,
	RecordStatus varchar(50) NOT NULL,
	BillDate date NOT NULL DEFAULT '0001-01-01',
	BillMonth int NOT NULL DEFAULT 0,
	Currency int NOT NULL DEFAULT 0,
	BillYear int NOT NULL DEFAULT 0,
	TotalBankCommissionAmount numeric(18, 4) NOT NULL DEFAULT 0.0,
	CONSTRAINT Pk_PostingBill PRIMARY KEY (Id)
);
END
IF NOT EXISTS (
    SELECT * FROM sys.indexes 
    WHERE name = 'IX_PostingBill_MerchantId_BillMonth_BillYear'
      AND object_id = OBJECT_ID('[Posting].[PostingBill]')
)
BEGIN
    CREATE INDEX [IX_PostingBill_MerchantId_BillMonth_BillYear]
    ON [Posting].[PostingBill] ([MerchantId], [BillMonth], [BillYear]);
END

-- posting.transfer_error definition

-- Drop table

-- DROP TABLE posting.transfer_error;
IF NOT EXISTS (
    SELECT * FROM INFORMATION_SCHEMA.TABLES
    WHERE TABLE_SCHEMA = 'Posting' AND TABLE_NAME = 'TransferError'
)
BEGIN
CREATE TABLE Posting.TransferError (
	Id uniqueidentifier NOT NULL,
	PostingDate datetime2 NOT NULL,
	MerchantTransactionId uniqueidentifier NOT NULL,
	MerchantId uniqueidentifier NULL,
	TransferErrorCategory varchar(50) NOT NULL,
	ErrorMessage varchar(500) NOT NULL,
	StackTrace varchar(2000) NULL,
	CreateDate datetime2 NOT NULL,
	UpdateDate datetime2 NULL,
	CreatedBy varchar(50) NOT NULL,
	LastModifiedBy varchar(50) NULL,
	RecordStatus varchar(50) NOT NULL,
	CONSTRAINT Pk_TransferError PRIMARY KEY (Id)
);
END
-- IX_TransferError_MerchantId
IF NOT EXISTS (
    SELECT * FROM sys.indexes 
    WHERE name = 'IX_TransferError_MerchantId'
      AND object_id = OBJECT_ID('[Posting].[TransferError]')
)
BEGIN
    CREATE INDEX [IX_TransferError_MerchantId]
    ON [Posting].[TransferError] ([MerchantId]);
END

-- IX_TransferError_MerchantTransactionId
IF NOT EXISTS (
    SELECT * FROM sys.indexes 
    WHERE name = 'IX_TransferError_MerchantTransactionId'
      AND object_id = OBJECT_ID('[Posting].[TransferError]')
)
BEGIN
    CREATE INDEX [IX_TransferError_MerchantTransactionId]
    ON [Posting].[TransferError] ([MerchantTransactionId]);
END

-- Merchant.ApiKey foreign key
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
    WHERE TABLE_SCHEMA = 'Merchant' AND TABLE_NAME = 'ApiKey' AND CONSTRAINT_NAME = 'Fk_ApiKey_Merchant_MerchantId'
)
BEGIN
    ALTER TABLE Merchant.ApiKey
    ADD CONSTRAINT Fk_ApiKey_Merchant_MerchantId
        FOREIGN KEY (MerchantId) REFERENCES Merchant.Merchant(Id) ON DELETE CASCADE;
END
GO


-- Merchant.BankAccount foreign keys
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
    WHERE TABLE_SCHEMA = 'Merchant' AND TABLE_NAME = 'BankAccount' AND CONSTRAINT_NAME = 'Fk_BankAccount_Bank_BankCode'
)
BEGIN
    ALTER TABLE Merchant.BankAccount
    ADD CONSTRAINT Fk_BankAccount_Bank_BankCode
        FOREIGN KEY (BankCode) REFERENCES Bank.Bank(Code) ON DELETE NO ACTION;
END
GO

IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
    WHERE TABLE_SCHEMA = 'Merchant' AND TABLE_NAME = 'BankAccount' AND CONSTRAINT_NAME = 'Fk_BankAccount_Merchant_MerchantId'
)
BEGIN
    ALTER TABLE Merchant.BankAccount
    ADD CONSTRAINT Fk_BankAccount_Merchant_MerchantId
        FOREIGN KEY (MerchantId) REFERENCES Merchant.Merchant(Id) ON DELETE CASCADE;
END
GO


-- Vpos.BankApiInfo foreign keys
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
    WHERE TABLE_SCHEMA = 'Vpos' AND TABLE_NAME = 'BankApiInfo' AND CONSTRAINT_NAME = 'Fk_BankApiInfo_ApiKey_KeyId'
)
BEGIN
    ALTER TABLE Vpos.BankApiInfo
    ADD CONSTRAINT Fk_BankApiInfo_ApiKey_KeyId
        FOREIGN KEY (KeyId) REFERENCES Bank.ApiKey(Id) ON DELETE NO ACTION;
END
GO

IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
    WHERE TABLE_SCHEMA = 'Vpos' AND TABLE_NAME = 'BankApiInfo' AND CONSTRAINT_NAME = 'Fk_BankApiInfo_Vpos_VposId'
)
BEGIN
    ALTER TABLE Vpos.BankApiInfo
    ADD CONSTRAINT Fk_BankApiInfo_Vpos_VposId
        FOREIGN KEY (VposId) REFERENCES Vpos.Vpos(Id) ON DELETE CASCADE;
END
GO


-- Vpos.Currency foreign keys
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
    WHERE TABLE_SCHEMA = 'Vpos' AND TABLE_NAME = 'Currency' AND CONSTRAINT_NAME = 'Fk_Currency_Currency_CurrencyCode'
)
BEGIN
    ALTER TABLE Vpos.Currency
    ADD CONSTRAINT Fk_Currency_Currency_CurrencyCode
        FOREIGN KEY (CurrencyCode) REFERENCES Core.Currency(Code) ON DELETE NO ACTION;
END
GO

IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
    WHERE TABLE_SCHEMA = 'Vpos' AND TABLE_NAME = 'Currency' AND CONSTRAINT_NAME = 'Fk_Currency_Vpos_VposId'
)
BEGIN
    ALTER TABLE Vpos.Currency
    ADD CONSTRAINT Fk_Currency_Vpos_VposId
        FOREIGN KEY (VposId) REFERENCES Vpos.Vpos(Id) ON DELETE CASCADE;
END
GO


-- Vpos.Vpos foreign key
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
    WHERE TABLE_SCHEMA = 'Vpos' AND TABLE_NAME = 'Vpos' AND CONSTRAINT_NAME = 'Fk_Vpos_AcquireBank_AcquireBankId'
)
BEGIN
    ALTER TABLE Vpos.Vpos
    ADD CONSTRAINT Fk_Vpos_AcquireBank_AcquireBankId
        FOREIGN KEY (AcquireBankId) REFERENCES Bank.AcquireBank(Id) ON DELETE CASCADE;
END
GO


-- Merchant.MerchantReturnPool foreign key
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
    WHERE TABLE_SCHEMA = 'Merchant' AND TABLE_NAME = 'MerchantReturnPool' AND CONSTRAINT_NAME = 'Fk_MerchantReturnPool_Merchant_MerchantId'
)
BEGIN
    ALTER TABLE Merchant.MerchantReturnPool
    ADD CONSTRAINT Fk_MerchantReturnPool_Merchant_MerchantId
        FOREIGN KEY (MerchantId) REFERENCES Merchant.Merchant(Id) ON DELETE CASCADE;
END
GO


-- Posting.BankBalance foreign keys
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
    WHERE TABLE_SCHEMA = 'Posting' AND TABLE_NAME = 'BankBalance' AND CONSTRAINT_NAME = 'Fk_BankBalance_Balance_PostingBalanceId'
)
BEGIN
    ALTER TABLE Posting.BankBalance
    ADD CONSTRAINT Fk_BankBalance_Balance_PostingBalanceId
        FOREIGN KEY (PostingBalanceId) REFERENCES Posting.Balance(Id);
END
GO

IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
    WHERE TABLE_SCHEMA = 'Posting' AND TABLE_NAME = 'BankBalance' AND CONSTRAINT_NAME = 'Fk_BankBalance_Merchant_MerchantId'
)
BEGIN
    ALTER TABLE Posting.BankBalance
    ADD CONSTRAINT Fk_BankBalance_Merchant_MerchantId
        FOREIGN KEY (MerchantId) REFERENCES Merchant.Merchant(Id) ON DELETE CASCADE;
END
GO


-- Posting.PostingBill foreign key
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
    WHERE TABLE_SCHEMA = 'Posting' AND TABLE_NAME = 'PostingBill' AND CONSTRAINT_NAME = 'Fk_PostingBill_Merchant_MerchantId'
)
BEGIN
    ALTER TABLE Posting.PostingBill
    ADD CONSTRAINT Fk_PostingBill_Merchant_MerchantId
        FOREIGN KEY (MerchantId) REFERENCES Merchant.Merchant(Id) ON DELETE CASCADE;
END
GO


-- Posting.TransferError foreign keys
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
    WHERE TABLE_SCHEMA = 'Posting' AND TABLE_NAME = 'TransferError' AND CONSTRAINT_NAME = 'Fk_TransferError_Merchant_MerchantId'
)
BEGIN
    ALTER TABLE Posting.TransferError
    ADD CONSTRAINT Fk_TransferError_Merchant_MerchantId
        FOREIGN KEY (MerchantId) REFERENCES Merchant.Merchant(Id);
END
GO

IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
    WHERE TABLE_SCHEMA = 'Posting' AND TABLE_NAME = 'TransferError' AND CONSTRAINT_NAME = 'Fk_TransferError_Transaction_MerchantTransactionId'
)
BEGIN
    ALTER TABLE Posting.TransferError
    ADD CONSTRAINT Fk_TransferError_Transaction_MerchantTransactionId
        FOREIGN KEY (MerchantTransactionId) REFERENCES Merchant."Transaction"(Id) ON DELETE CASCADE;
END
GO



	-- submerchant.daily_usage definition


-- submerchant.sub_merchant definition

-- Drop table

-- DROP TABLE submerchant.sub_merchant;
IF NOT EXISTS (
    SELECT * FROM INFORMATION_SCHEMA.TABLES
    WHERE TABLE_SCHEMA = 'Submerchant' AND TABLE_NAME = 'Submerchant'
)
BEGIN
CREATE TABLE Submerchant.SubMerchant (
	Id uniqueidentifier NOT NULL,
	"Name" varchar(150) NOT NULL,
	"Number" varchar(15) NOT NULL,
	MerchantType varchar(50) NOT NULL,
	MerchantId uniqueidentifier NOT NULL,
	City int NOT NULL,
	CityName text NULL,
	IsManuelPaymentPageAllowed bit NOT NULL,
	IsLinkPaymentPageAllowed bit NOT NULL,
	PreAuthorizationAllowed bit NOT NULL,
	PaymentReverseAllowed bit NOT NULL,
	PaymentReturnAllowed bit NOT NULL,
	InstallmentAllowed bit NOT NULL,
	Is3dRequired bit NOT NULL,
	IsExcessReturnAllowed bit NOT NULL,
	InternationalCardAllowed bit NOT NULL,
	CreateDate datetime2 NOT NULL,
	UpdateDate datetime2 NULL,
	CreatedBy varchar(50) NOT NULL,
	LastModifiedBy varchar(50) NULL,
	RecordStatus varchar(50) NOT NULL,
	ParameterValue varchar(100) NULL,
	RejectReason varchar(256) NULL,
	IsOnUsPaymentPageAllowed bit NOT NULL DEFAULT 0,
	CONSTRAINT Pk_SubMerchant PRIMARY KEY (Id)
);
CREATE INDEX IX_Submerchant_MerchantId ON Submerchant.Submerchant(MerchantId);

END
-- Drop table

-- DROP TABLE submerchant.daily_usage;
IF NOT EXISTS (
    SELECT * FROM INFORMATION_SCHEMA.TABLES
    WHERE TABLE_SCHEMA = 'Submerchant' AND TABLE_NAME = 'DailyUsage'
)
BEGIN
CREATE TABLE Submerchant.DailyUsage (
	Id uniqueidentifier NOT NULL,
	"Date" datetime2 NOT NULL,
	Count int NOT NULL,
	Amount numeric(18, 4) NOT NULL,
	SubMerchantId uniqueidentifier NOT NULL,
	Currency varchar(50) NOT NULL,
	TransactionLimitType varchar(50) NOT NULL,
	CreateDate datetime2 NOT NULL,
	UpdateDate datetime2 NULL,
	CreatedBy varchar(50) NOT NULL,
	LastModifiedBy varchar(50) NULL,
	RecordStatus varchar(50) NOT NULL,
	CONSTRAINT Pk_DailyUsage PRIMARY KEY (Id)
);
CREATE INDEX IX_DailyUsage_SubMerchantId ON Submerchant.DailyUsage(SubMerchantId);
END

-- submerchant."document" definition

-- Drop table

-- DROP TABLE submerchant."document";
IF NOT EXISTS (
    SELECT * FROM INFORMATION_SCHEMA.TABLES
    WHERE TABLE_SCHEMA = 'Submerchant' AND TABLE_NAME = 'Document'
)
BEGIN
    CREATE TABLE Submerchant."Document" (
        Id uniqueidentifier NOT NULL,
        DocumentId uniqueidentifier NOT NULL,
        DocumentTypeId uniqueidentifier NOT NULL,
        DocumentName varchar(256) NOT NULL,
        SubMerchantId uniqueidentifier NOT NULL,
        CreateDate datetime2 NOT NULL,
        UpdateDate datetime2 NULL,
        CreatedBy varchar(50) NOT NULL,
        LastModifiedBy varchar(50) NULL,
        RecordStatus varchar(50) NOT NULL,
        CONSTRAINT PK_Document PRIMARY KEY (Id)
    );

    CREATE INDEX IX_Document_SubMerchantId ON Submerchant."Document"(SubMerchantId);
END

-- submerchant."limit" definition

-- Drop table

-- DROP TABLE submerchant."limit";
IF NOT EXISTS (
    SELECT * FROM INFORMATION_SCHEMA.TABLES
    WHERE TABLE_SCHEMA = 'Submerchant' AND TABLE_NAME = 'Limit'
)
BEGIN
    CREATE TABLE Submerchant."Limit" (
        Id uniqueidentifier NOT NULL,
        TransactionLimitType varchar(50) NOT NULL,
        Period varchar(50) NOT NULL,
        LimitType varchar(50) NOT NULL,
        MaxPiece int NULL,
        MaxAmount numeric(18, 4) NULL,
        SubMerchantId uniqueidentifier NOT NULL,
        Currency varchar(50) NULL,
        CreateDate datetime2 NOT NULL,
        UpdateDate datetime2 NULL,
        CreatedBy varchar(50) NOT NULL,
        LastModifiedBy varchar(50) NULL,
        RecordStatus varchar(50) NOT NULL,
        CONSTRAINT PK_Limit PRIMARY KEY (Id)
    );

    CREATE INDEX IX_Limit_SubMerchantId ON Submerchant."Limit"(SubMerchantId);
END

-- submerchant.monthly_usage definition

-- Drop table

-- DROP TABLE submerchant.monthly_usage;
IF NOT EXISTS (
    SELECT * FROM INFORMATION_SCHEMA.TABLES
    WHERE TABLE_SCHEMA = 'Submerchant' AND TABLE_NAME = 'MonthlyUsage'
)
BEGIN
CREATE TABLE Submerchant.MonthlyUsage (
	Id uniqueidentifier NOT NULL,
	"Date" datetime2 NOT NULL,
	Count int NOT NULL,
	Amount numeric(18, 4) NOT NULL,
	SubMerchantId uniqueidentifier NOT NULL,
	Currency varchar(50) NOT NULL,
	TransactionLimitType varchar(50) NOT NULL,
	CreateDate datetime2 NOT NULL,
	UpdateDate datetime2 NULL,
	CreatedBy varchar(50) NOT NULL,
	LastModifiedBy varchar(50) NULL,
	RecordStatus varchar(50) NOT NULL,
	CONSTRAINT Pk_MonthlyUsage PRIMARY KEY (Id)
);
CREATE INDEX IX_MonthlyUsage_SubMerchantId ON Submerchant.MonthlyUsage(SubMerchantId);
END



-- submerchant."user" definition

-- Drop table

-- DROP TABLE submerchant."user";
IF NOT EXISTS (
    SELECT * FROM INFORMATION_SCHEMA.TABLES
    WHERE TABLE_SCHEMA = 'Submerchant' AND TABLE_NAME = 'User'
)
BEGIN
    CREATE TABLE Submerchant."User" (
        Id uniqueidentifier NOT NULL,
        UserId uniqueidentifier NOT NULL,
        Name varchar(100) NOT NULL,
        Surname varchar(100) NOT NULL,
        BirthDate datetime2 NOT NULL,
        Email varchar(100) NOT NULL,
        MobilePhoneNumber varchar(20) NOT NULL,
        RoleId varchar(50) NULL,
        RoleName varchar(150) NULL,
        SubMerchantId uniqueidentifier NOT NULL,
        CreateDate datetime2 NOT NULL,
        UpdateDate datetime2 NULL,
        CreatedBy varchar(50) NOT NULL,
        LastModifiedBy varchar(50) NULL,
        RecordStatus varchar(50) NOT NULL,
        IdentityNumber varchar(50) NULL,
        CONSTRAINT PK_User PRIMARY KEY (Id)
    );

    CREATE INDEX IX_User_SubMerchantId ON Submerchant."User"(SubMerchantId);
END;


-- submerchant.daily_usage foreign keys

-- Merchant.ApiKey foreign key
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
    WHERE TABLE_SCHEMA = 'Merchant' AND TABLE_NAME = 'ApiKey' AND CONSTRAINT_NAME = 'Fk_ApiKey_Merchant_MerchantId'
)
BEGIN
    ALTER TABLE Merchant.ApiKey
    ADD CONSTRAINT Fk_ApiKey_Merchant_MerchantId
        FOREIGN KEY (MerchantId) REFERENCES Merchant.Merchant(Id) ON DELETE CASCADE;
END
GO


-- Merchant.BankAccount foreign keys
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
    WHERE TABLE_SCHEMA = 'Merchant' AND TABLE_NAME = 'BankAccount' AND CONSTRAINT_NAME = 'Fk_BankAccount_Bank_BankCode'
)
BEGIN
    ALTER TABLE Merchant.BankAccount
    ADD CONSTRAINT Fk_BankAccount_Bank_BankCode
        FOREIGN KEY (BankCode) REFERENCES Bank.Bank(Code) ON DELETE NO ACTION;
END
GO

IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
    WHERE TABLE_SCHEMA = 'Merchant' AND TABLE_NAME = 'BankAccount' AND CONSTRAINT_NAME = 'Fk_BankAccount_Merchant_MerchantId'
)
BEGIN
    ALTER TABLE Merchant.BankAccount
    ADD CONSTRAINT Fk_BankAccount_Merchant_MerchantId
        FOREIGN KEY (MerchantId) REFERENCES Merchant.Merchant(Id) ON DELETE CASCADE;
END
GO


-- Vpos.BankApiInfo foreign keys
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
    WHERE TABLE_SCHEMA = 'Vpos' AND TABLE_NAME = 'BankApiInfo' AND CONSTRAINT_NAME = 'Fk_BankApiInfo_ApiKey_KeyId'
)
BEGIN
    ALTER TABLE Vpos.BankApiInfo
    ADD CONSTRAINT Fk_BankApiInfo_ApiKey_KeyId
        FOREIGN KEY (KeyId) REFERENCES Bank.ApiKey(Id) ON DELETE NO ACTION;
END
GO

IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
    WHERE TABLE_SCHEMA = 'Vpos' AND TABLE_NAME = 'BankApiInfo' AND CONSTRAINT_NAME = 'Fk_BankApiInfo_Vpos_VposId'
)
BEGIN
    ALTER TABLE Vpos.BankApiInfo
    ADD CONSTRAINT Fk_BankApiInfo_Vpos_VposId
        FOREIGN KEY (VposId) REFERENCES Vpos.Vpos(Id) ON DELETE CASCADE;
END
GO


-- Vpos.Currency foreign keys
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
    WHERE TABLE_SCHEMA = 'Vpos' AND TABLE_NAME = 'Currency' AND CONSTRAINT_NAME = 'Fk_Currency_Currency_CurrencyCode'
)
BEGIN
    ALTER TABLE Vpos.Currency
    ADD CONSTRAINT Fk_Currency_Currency_CurrencyCode
        FOREIGN KEY (CurrencyCode) REFERENCES Core.Currency(Code) ON DELETE NO ACTION;
END
GO

IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
    WHERE TABLE_SCHEMA = 'Vpos' AND TABLE_NAME = 'Currency' AND CONSTRAINT_NAME = 'Fk_Currency_Vpos_VposId'
)
BEGIN
    ALTER TABLE Vpos.Currency
    ADD CONSTRAINT Fk_Currency_Vpos_VposId
        FOREIGN KEY (VposId) REFERENCES Vpos.Vpos(Id) ON DELETE CASCADE;
END
GO


-- Vpos.Vpos foreign keys
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
    WHERE TABLE_SCHEMA = 'Vpos' AND TABLE_NAME = 'Vpos' AND CONSTRAINT_NAME = 'Fk_Vpos_AcquireBank_AcquireBankId'
)
BEGIN
    ALTER TABLE Vpos.Vpos
    ADD CONSTRAINT Fk_Vpos_AcquireBank_AcquireBankId
        FOREIGN KEY (AcquireBankId) REFERENCES Bank.AcquireBank(Id) ON DELETE CASCADE;
END
GO


-- Merchant.MerchantReturnPool foreign keys
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
    WHERE TABLE_SCHEMA = 'Merchant' AND TABLE_NAME = 'MerchantReturnPool' AND CONSTRAINT_NAME = 'Fk_MerchantReturnPool_Merchant_MerchantId'
)
BEGIN
    ALTER TABLE Merchant.MerchantReturnPool
    ADD CONSTRAINT Fk_MerchantReturnPool_Merchant_MerchantId
        FOREIGN KEY (MerchantId) REFERENCES Merchant.Merchant(Id) ON DELETE CASCADE;
END
GO


-- Posting.BankBalance foreign keys
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
    WHERE TABLE_SCHEMA = 'Posting' AND TABLE_NAME = 'BankBalance' AND CONSTRAINT_NAME = 'Fk_BankBalance_Balance_PostingBalanceId'
)
BEGIN
    ALTER TABLE Posting.BankBalance
    ADD CONSTRAINT Fk_BankBalance_Balance_PostingBalanceId
        FOREIGN KEY (PostingBalanceId) REFERENCES Posting.Balance(Id);
END
GO

IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
    WHERE TABLE_SCHEMA = 'Posting' AND TABLE_NAME = 'BankBalance' AND CONSTRAINT_NAME = 'Fk_BankBalance_Merchant_MerchantId'
)
BEGIN
    ALTER TABLE Posting.BankBalance
    ADD CONSTRAINT Fk_BankBalance_Merchant_MerchantId
        FOREIGN KEY (MerchantId) REFERENCES Merchant.Merchant(Id) ON DELETE CASCADE;
END
GO


-- Posting.PostingBill foreign keys
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
    WHERE TABLE_SCHEMA = 'Posting' AND TABLE_NAME = 'PostingBill' AND CONSTRAINT_NAME = 'Fk_PostingBill_Merchant_MerchantId'
)
BEGIN
    ALTER TABLE Posting.PostingBill
    ADD CONSTRAINT Fk_PostingBill_Merchant_MerchantId
        FOREIGN KEY (MerchantId) REFERENCES Merchant.Merchant(Id) ON DELETE CASCADE;
END
GO


-- Posting.TransferError foreign keys
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
    WHERE TABLE_SCHEMA = 'Posting' AND TABLE_NAME = 'TransferError' AND CONSTRAINT_NAME = 'Fk_TransferError_Merchant_MerchantId'
)
BEGIN
    ALTER TABLE Posting.TransferError
    ADD CONSTRAINT Fk_TransferError_Merchant_MerchantId
        FOREIGN KEY (MerchantId) REFERENCES Merchant.Merchant(Id);
END
GO

IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
    WHERE TABLE_SCHEMA = 'Posting' AND TABLE_NAME = 'TransferError' AND CONSTRAINT_NAME = 'Fk_TransferError_Transaction_MerchantTransactionId'
)
BEGIN
    ALTER TABLE Posting.TransferError
    ADD CONSTRAINT Fk_TransferError_Transaction_MerchantTransactionId
        FOREIGN KEY (MerchantTransactionId) REFERENCES Merchant."Transaction"(Id) ON DELETE CASCADE;
END
GO


-- Submerchant.DailyUsage foreign keys
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
    WHERE TABLE_SCHEMA = 'Submerchant' AND TABLE_NAME = 'DailyUsage' AND CONSTRAINT_NAME = 'Fk_DailyUsage_SubMerchant_SubMerchantId'
)
BEGIN
    ALTER TABLE Submerchant.DailyUsage
    ADD CONSTRAINT Fk_DailyUsage_SubMerchant_SubMerchantId
        FOREIGN KEY (SubMerchantId) REFERENCES Submerchant.SubMerchant(Id) ON DELETE CASCADE;
END
GO


-- Submerchant.Document foreign keys
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
    WHERE TABLE_SCHEMA = 'Submerchant' AND TABLE_NAME = 'Document' AND CONSTRAINT_NAME = 'Fk_Document_SubMerchant_SubMerchantId'
)
BEGIN
    ALTER TABLE Submerchant.Document
    ADD CONSTRAINT Fk_Document_SubMerchant_SubMerchantId
        FOREIGN KEY (SubMerchantId) REFERENCES Submerchant.SubMerchant(Id) ON DELETE CASCADE;
END
GO


-- Submerchant.Limit foreign keys
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
    WHERE TABLE_SCHEMA = 'Submerchant' AND TABLE_NAME = 'Limit' AND CONSTRAINT_NAME = 'Fk_Limit_SubMerchant_SubMerchantId'
)
BEGIN
    ALTER TABLE Submerchant.Limit
    ADD CONSTRAINT Fk_Limit_SubMerchant_SubMerchantId
        FOREIGN KEY (SubMerchantId) REFERENCES Submerchant.SubMerchant(Id) ON DELETE CASCADE;
END
GO


-- Submerchant.MonthlyUsage foreign keys
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
    WHERE TABLE_SCHEMA = 'Submerchant' AND TABLE_NAME = 'MonthlyUsage' AND CONSTRAINT_NAME = 'Fk_MonthlyUsage_SubMerchant_SubMerchantId'
)
BEGIN
    ALTER TABLE Submerchant.MonthlyUsage
    ADD CONSTRAINT Fk_MonthlyUsage_SubMerchant_SubMerchantId
        FOREIGN KEY (SubMerchantId) REFERENCES Submerchant.SubMerchant(Id) ON DELETE CASCADE;
END
GO


-- Submerchant.SubMerchant foreign keys
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
    WHERE TABLE_SCHEMA = 'Submerchant' AND TABLE_NAME = 'SubMerchant' AND CONSTRAINT_NAME = 'Fk_SubMerchant_Merchant_MerchantId'
)
BEGIN
    ALTER TABLE Submerchant.SubMerchant
    ADD CONSTRAINT Fk_SubMerchant_Merchant_MerchantId
        FOREIGN KEY (MerchantId) REFERENCES Merchant.Merchant(Id) ON DELETE CASCADE;
END
GO


-- Submerchant.User foreign keys
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
    WHERE TABLE_SCHEMA = 'Submerchant' AND TABLE_NAME = 'User' AND CONSTRAINT_NAME = 'Fk_User_SubMerchant_SubMerchantId'
)
BEGIN
    ALTER TABLE Submerchant."User"
    ADD CONSTRAINT Fk_User_SubMerchant_SubMerchantId
        FOREIGN KEY (SubMerchantId) REFERENCES Submerchant.SubMerchant(Id) ON DELETE CASCADE;
END
GO


	-- api.log definition

-- Drop table

-- DROP TABLE api.log;
IF NOT EXISTS (
    SELECT * FROM INFORMATION_SCHEMA.TABLES
    WHERE TABLE_SCHEMA = 'Api' AND TABLE_NAME = 'Log'
)
BEGIN
CREATE TABLE Api.Log (
	Id uniqueidentifier NOT NULL,
	MerchantId uniqueidentifier NOT NULL,
	PaymentType varchar(50) NOT NULL,
	Request text NULL,
	Response text NULL,
	ErrorCode varchar(10) NOT NULL,
	ErrorMessage varchar(256) NOT NULL,
	CreateDate datetime2 NOT NULL,
	UpdateDate datetime2 NULL,
	CreatedBy varchar(50) NOT NULL,
	LastModifiedBy varchar(50) NULL,
	RecordStatus varchar(50) NOT NULL,
	CONSTRAINT Pk_Log PRIMARY KEY (Id)
);
CREATE INDEX IX_Log_MerchantId ON Api.log(MerchantId);
END

-- api.response_code definition

-- Drop table

-- DROP TABLE api.response_code;
IF NOT EXISTS (
    SELECT * FROM INFORMATION_SCHEMA.TABLES
    WHERE TABLE_SCHEMA = 'Api' AND TABLE_NAME = 'ResponseCode'
)
BEGIN
CREATE TABLE Api.ResponseCode (
	Id uniqueidentifier NOT NULL,
	ResponseCode varchar(10) NOT NULL,
	Description varchar(256) NOT NULL,
	MerchantResponseCodeId uniqueidentifier NULL,
	CreateDate datetime2 NOT NULL,
	UpdateDate datetime2 NULL,
	CreatedBy varchar(50) NOT NULL,
	LastModifiedBy varchar(50) NULL,
	RecordStatus varchar(50) NOT NULL,
	CONSTRAINT Pk_ResponseCode PRIMARY KEY (Id)
);
CREATE INDEX IX_ResponseCode_MerchantResponseCodeId ON Api.ResponseCode(MerchantResponseCodeId);
END

-- api.validation_log definition

-- Drop table

-- DROP TABLE api.validation_log;
IF NOT EXISTS (
    SELECT * FROM INFORMATION_SCHEMA.TABLES
    WHERE TABLE_SCHEMA = 'Api' AND TABLE_NAME = 'ValidationLog'
)
BEGIN
CREATE TABLE Api.ValidationLog (
	Id uniqueidentifier NOT NULL,
	MerchantId uniqueidentifier NOT NULL,
	TransactionType varchar(50) NOT NULL,
	ErrorCode varchar(10) NOT NULL,
	ErrorMessage varchar(256) NOT NULL,
	Amount numeric(18, 4) NOT NULL,
	PointAmount numeric(18, 4) NOT NULL,
	CardToken varchar(50) NULL,
	Currency varchar(50) NULL,
	InstallmentCount int NOT NULL,
	ThreeDSessionId varchar(200) NULL,
	ConversationId varchar(50) NULL,
	OriginalReferenceNumber varchar(50) NULL,
	ClientIpAddress varchar(50) NULL,
	LanguageCode varchar(100) NULL,
	ApiName varchar(200) NULL,
	CreateDate datetime2 NOT NULL,
	UpdateDate datetime2 NULL,
	CreatedBy varchar(50) NOT NULL,
	LastModifiedBy varchar(50) NULL,
	RecordStatus varchar(50) NOT NULL,
	CONSTRAINT Pk_ValidationLog PRIMARY KEY (Id)
);
CREATE INDEX IX_ValidationLog_MerchantId ON Api.ValidationLog(MerchantId);
END

-- api.log foreign keys

-- Api.Log foreign key
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
    WHERE TABLE_SCHEMA = 'Api' AND TABLE_NAME = 'Log' AND CONSTRAINT_NAME = 'Fk_Log_Merchant_MerchantId'
)
BEGIN
    ALTER TABLE Api.Log
    ADD CONSTRAINT Fk_Log_Merchant_MerchantId
        FOREIGN KEY (MerchantId) REFERENCES Merchant.Merchant(Id) ON DELETE CASCADE;
END
GO


-- Api.ResponseCode foreign keys
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
    WHERE TABLE_SCHEMA = 'Api' AND TABLE_NAME = 'ResponseCode' AND CONSTRAINT_NAME = 'Fk_ResponseCode_MerchantResponseCode_MerchantResponseCode'
)
BEGIN
    ALTER TABLE Api.ResponseCode
    ADD CONSTRAINT Fk_ResponseCode_MerchantResponseCode_MerchantResponseCode
        FOREIGN KEY (MerchantResponseCodeId) REFERENCES Merchant.ResponseCode(Id);
END
GO


-- Api.ValidationLog foreign keys
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
    WHERE TABLE_SCHEMA = 'Api' AND TABLE_NAME = 'ValidationLog' AND CONSTRAINT_NAME = 'Fk_ValidationLog_Merchant_MerchantId'
)
BEGIN
    ALTER TABLE Api.ValidationLog
    ADD CONSTRAINT Fk_ValidationLog_Merchant_MerchantId
        FOREIGN KEY (MerchantId) REFERENCES Merchant.Merchant(Id) ON DELETE CASCADE;
END
GO
