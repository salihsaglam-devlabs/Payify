-- Ensure schema 'Core' exists
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Core')
BEGIN
EXEC('CREATE SCHEMA Core');
END;

-- Create table 'IksTransaction'
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'Core' AND TABLE_NAME = 'IksTransaction')
BEGIN
CREATE TABLE Core.IksTransaction (
                                     Id UNIQUEIDENTIFIER NOT NULL,
                                     Operation NVARCHAR(MAX) NOT NULL,
                                     ResponseCode VARCHAR(20) NOT NULL,
                                     IsSuccess BIT NOT NULL,
                                     RequestDetails NVARCHAR(MAX) NULL,
                                     ResponseDetails NVARCHAR(MAX) NULL,
                                     CreateDate DATETIME2 NOT NULL,
                                     UpdateDate DATETIME2 NULL,
                                     CreatedBy VARCHAR(50) NOT NULL,
                                     LastModifiedBy VARCHAR(50) NULL,
                                     RecordStatus VARCHAR(50) NOT NULL,
                                     MerchantId UNIQUEIDENTIFIER NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000',
                                   
                                     CONSTRAINT PK_IksTransaction PRIMARY KEY (Id)
);
END;

-- Create table 'TimeoutIksTransaction'
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'Core' AND TABLE_NAME = 'TimeoutIksTransaction')
BEGIN
CREATE TABLE Core.TimeoutIksTransaction (
                                           Id UNIQUEIDENTIFIER NOT NULL,
                                           Operation NVARCHAR(MAX) NOT NULL,
                                           ResponseCode VARCHAR(20) NOT NULL,
                                           IsSuccess BIT NOT NULL,
                                           RequestDetails NVARCHAR(MAX) NULL,
                                           ResponseDetails NVARCHAR(MAX) NULL,
                                           TimeoutReturnDetails NVARCHAR(MAX) NULL,
                                           CreateDate DATETIME2 NOT NULL,
                                           UpdateDate DATETIME2 NULL,
                                           CreatedBy VARCHAR(50) NOT NULL,
                                           LastModifiedBy VARCHAR(50) NULL,
                                           RecordStatus VARCHAR(50) NOT NULL,
                                           MerchantId UNIQUEIDENTIFIER NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000',
                                         
                                           CONSTRAINT PK_TimeoutIksTransaction PRIMARY KEY (Id)
);
END;

-- Create table 'IksTerminal'
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'Core' AND TABLE_NAME = 'IksTerminal')
BEGIN
CREATE TABLE Core.IksTerminal (
                                 Id UNIQUEIDENTIFIER NOT NULL,
                                 MerchantId UNIQUEIDENTIFIER NOT NULL,
                                 ReferenceCode VARCHAR(12) NOT NULL,
                                 GlobalMerchantId VARCHAR(8) NOT NULL,
                                 PspMerchantId VARCHAR(15) NOT NULL,
                                 TerminalId VARCHAR(10) NULL,
                                 StatusCode VARCHAR(1) NOT NULL,
                                 [Type] VARCHAR(1) NOT NULL,
                                 BrandCode VARCHAR(1) NULL,
                                 Model VARCHAR(10) NULL,
                                 SerialNo VARCHAR(25) NULL,
                                 OwnerPspNo INT NOT NULL,
                                 OwnerTerminalId VARCHAR(10) NULL,
                                 BrandSharing VARCHAR(1) NULL,
                                 PinPad VARCHAR(1) NULL,
                                 Contactless VARCHAR(1) NULL,
                                 ConnectionType VARCHAR(1) NULL,
                                 VirtualPosUrl VARCHAR(150) NULL,
                                 HostingTaxNo VARCHAR(11) NULL,
                                 HostingTradeName VARCHAR(30) NULL,
                                 HostingUrl VARCHAR(150) NULL,
                                 PaymentGwTaxNo VARCHAR(11) NULL,
                                 PaymentGwTradeName VARCHAR(30) NULL,
                                 PaymentGwUrl VARCHAR(150) NULL,
                                 ServiceProviderPspNo INT NOT NULL,
                                 FiscalNo VARCHAR(12) NULL,
                                 TechPos INT NOT NULL,
                                 ServiceProviderPspMerchantId VARCHAR(15) NULL,
                                 PfMainMerchantId VARCHAR(15) NULL,
                                 ResponseCode VARCHAR(3) NULL,
                                 ResponseCodeExplanation VARCHAR(2000) NULL,
                                 CreateDate DATETIME2 NOT NULL,
                                 UpdateDate DATETIME2 NULL,
                                 CreatedBy VARCHAR(50) NOT NULL,
                                 LastModifiedBy VARCHAR(50) NULL,
                                 RecordStatus VARCHAR(50) NOT NULL,
                                 CreatedNameBy NVARCHAR(MAX) NULL,
                                 TerminalStatus VARCHAR(50) NOT NULL DEFAULT '',
                                 VposId UNIQUEIDENTIFIER NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000',
                                 CONSTRAINT PK_IksTerminal PRIMARY KEY (Id)
);
END;

-- Create table 'IksTerminalHistory'
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'Core' AND TABLE_NAME = 'IksTerminalHistory')
BEGIN
CREATE TABLE Core.IksTerminalHistory (
                                        Id UNIQUEIDENTIFIER NOT NULL,
                                        MerchantId UNIQUEIDENTIFIER NOT NULL,
                                        VposId UNIQUEIDENTIFIER NOT NULL,
                                        ReferenceCode VARCHAR(12) NOT NULL,
                                        NewData VARCHAR(1000) NULL,
                                        OldData VARCHAR(1000) NULL,
                                        ChangedField VARCHAR(100) NULL,
                                        ResponseCode VARCHAR(3) NULL,
                                        ResponseCodeExplanation VARCHAR(2000) NULL,
                                        QueryDate DATETIME2 NOT NULL,
                                        TerminalRecordType VARCHAR(50) NOT NULL,
                                        CreateDate DATETIME2 NOT NULL,
                                        UpdateDate DATETIME2 NULL,
                                        CreatedBy VARCHAR(50) NOT NULL,
                                        LastModifiedBy VARCHAR(50) NULL,
                                        RecordStatus VARCHAR(50) NOT NULL,

                                        CONSTRAINT PK_IksTerminalHistory PRIMARY KEY (Id)
);
END;