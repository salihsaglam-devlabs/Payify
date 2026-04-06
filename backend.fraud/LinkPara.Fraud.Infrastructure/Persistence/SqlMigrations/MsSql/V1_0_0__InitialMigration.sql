IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Core')
    EXEC('CREATE SCHEMA Core');

IF NOT EXISTS (
    SELECT * FROM INFORMATION_SCHEMA.TABLES 
    WHERE TABLE_SCHEMA = 'Core' AND TABLE_NAME = 'IntegrationLog'
)
BEGIN
CREATE TABLE Core.IntegrationLog (
                                     Id UNIQUEIDENTIFIER NOT NULL,
                                     TransactionMonitoringId UNIQUEIDENTIFIER NOT NULL,
                                     Request NVARCHAR NULL,
                                     Response NVARCHAR NULL,
                                     IsSuccess BIT NOT NULL,
                                     CreateDate DATETIME2(7) NOT NULL,
                                     UpdateDate DATETIME2(7) NULL,
                                     CreatedBy varchar(50) NOT NULL,
                                     LastModifiedBy varchar(50) NULL,
                                     RecordStatus varchar(50) NOT NULL,
                                     CONSTRAINT PkIntegrationLog PRIMARY KEY (Id)
);
END;


IF NOT EXISTS (
    SELECT * FROM INFORMATION_SCHEMA.TABLES 
    WHERE TABLE_SCHEMA = 'Core' AND TABLE_NAME = 'SearchLog'
)
BEGIN
CREATE TABLE Core.SearchLog (
                                Id UNIQUEIDENTIFIER NOT NULL,
                                SearchName varchar(200) NOT NULL,
                                BirthYear varchar(10) NULL,
                                SearchType varchar(50) NOT NULL,
                                MatchStatus varchar(50) NOT NULL,
                                MatchRate INT NOT NULL,
                                IsBlackList BIT NOT NULL,
                                BlacklistName varchar(500) NULL,
                                CreateDate DATETIME2(7) NOT NULL,
                                UpdateDate DATETIME2(7) NULL,
                                CreatedBy varchar(50) NOT NULL,
                                LastModifiedBy varchar(50) NULL,
                                RecordStatus varchar(50) NOT NULL,
                                ExpireDate DATETIME2(7) NULL,
                                ClientIpAddress varchar(50) NULL,
                                CONSTRAINT PkSearchLog PRIMARY KEY (Id)
);
END;

IF NOT EXISTS (
    SELECT * FROM INFORMATION_SCHEMA.TABLES 
    WHERE TABLE_SCHEMA = 'Core' AND TABLE_NAME = 'TransactionMonitoring'
)
BEGIN
CREATE TABLE Core.TransactionMonitoring (
                                            Id UNIQUEIDENTIFIER NOT NULL,
    [Module] varchar(100) NOT NULL,
    CommandName varchar(200) NOT NULL,
    TransferRequest NVARCHAR NOT NULL,
    CommandJson NVARCHAR NOT NULL,
    SenderNumber varchar(50) NULL,
    ReceiverNumber varchar(50) NULL,
    Amount numeric(18, 4) NOT NULL,
    CurrencyCode varchar(10) NOT NULL,
    TransactionId varchar(50) NOT NULL,
    TotalScore INT NOT NULL,
    MonitoringStatus varchar(50) NOT NULL,
    RiskLevel varchar(50) NOT NULL,
    TransactionDate DATETIME2(7) NOT NULL,
    ErrorCode varchar(100) NULL,
    ErrorMessage varchar(300) NULL,
    CreateDate DATETIME2(7) NOT NULL,
    UpdateDate DATETIME2(7) NULL,
    CreatedBy varchar(50) NOT NULL,
    LastModifiedBy varchar(50) NULL,
    RecordStatus varchar(50) NOT NULL,
    ReceiverName varchar(100) NULL,
    SenderName varchar(100) NULL,
    ClientIpAddress varchar(50) NULL,
    CONSTRAINT PkTransactionMonitoring PRIMARY KEY (Id)
    );
END;

IF NOT EXISTS (
    SELECT * FROM INFORMATION_SCHEMA.TABLES 
    WHERE TABLE_SCHEMA = 'Core' AND TABLE_NAME = 'TriggeredRule'
)
BEGIN
CREATE TABLE Core.TriggeredRule (
                                    Id UNIQUEIDENTIFIER NOT NULL,
                                    RuleKey varchar(300) NOT NULL,
                                    Score INT NOT NULL,
                                    TransactionMonitoringId UNIQUEIDENTIFIER NOT NULL,
                                    CreateDate DATETIME2(7) NOT NULL,
                                    UpdateDate DATETIME2(7) NULL,
                                    CreatedBy varchar(50) NOT NULL,
                                    LastModifiedBy varchar(50) NULL,
                                    RecordStatus varchar(50) NOT NULL,
                                    CONSTRAINT PkTriggeredRule PRIMARY KEY (Id)
);
END;


IF NOT EXISTS (
    SELECT * FROM INFORMATION_SCHEMA.TABLES 
    WHERE TABLE_SCHEMA = 'Core' AND TABLE_NAME = 'TriggeredRuleSetKey'
)
BEGIN
CREATE TABLE Core.TriggeredRuleSetKey (
                                          Id UNIQUEIDENTIFIER NOT NULL,
                                          Operation varchar(50) NOT NULL,
    [Level] varchar(50) NOT NULL,
    RuleSetKey varchar(50) NOT NULL,
    CreateDate DATETIME2(7) NOT NULL,
    UpdateDate DATETIME2(7) NULL,
    CreatedBy varchar(50) NOT NULL,
    LastModifiedBy varchar(50) NULL,
    RecordStatus varchar(50) NOT NULL,
    CONSTRAINT PkTriggeredRuleSetKey PRIMARY KEY (Id)
    );
END;

IF NOT EXISTS (
        SELECT 1
        FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
        WHERE TABLE_SCHEMA = 'Core'
          AND TABLE_NAME = 'TriggeredRule'
          AND CONSTRAINT_NAME = 'FkTriggeredRuleTransactionMonitoringTransactionMonitorin'
    ) BEGIN
ALTER TABLE [Core].TriggeredRule
    ADD CONSTRAINT FkTriggeredRuleTransactionMonitoringTransactionMonitorin
    FOREIGN KEY (TransactionMonitoringId)
    REFERENCES Core.TransactionMonitoring(Id)
    ON DELETE NO ACTION;
END;

IF NOT EXISTS (
        SELECT 1
        FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
        WHERE TABLE_SCHEMA = 'Core'
          AND TABLE_NAME = 'IntegrationLog'
          AND CONSTRAINT_NAME = 'FkIntegrationLogTransactionMonitoringTransactionMonitori'
    ) BEGIN
ALTER TABLE [Core].IntegrationLog
    ADD CONSTRAINT FkIntegrationLogTransactionMonitoringTransactionMonitori
    FOREIGN KEY (TransactionMonitoringId)
    REFERENCES Core.TransactionMonitoring(Id)
    ON DELETE NO ACTION;
END;

IF NOT EXISTS (


    SELECT 1 FROM sys.indexes 


    WHERE name = 'PkTriggeredRuleSetKey' 


      AND object_id = OBJECT_ID('Core.TriggeredRuleSetKey')


)


BEGIN


CREATE UNIQUE INDEX PkTriggeredRuleSetKey ON Core.TriggeredRuleSetKey (Id);


END;


IF NOT EXISTS (


    SELECT 1 FROM sys.indexes 


    WHERE name = 'PkSearchLog' 


      AND object_id = OBJECT_ID('Core.SearchLog')


)


BEGIN


CREATE UNIQUE INDEX PkSearchLog ON Core.SearchLog (Id);


END;


IF NOT EXISTS (


    SELECT 1 FROM sys.indexes 


    WHERE name = 'Ix_IntegrationLog_TransactionMonitoringId' 


      AND object_id = OBJECT_ID('Core.IntegrationLog')


)


BEGIN


CREATE INDEX Ix_IntegrationLog_TransactionMonitoringId ON Core.IntegrationLog (TransactionMonitoringId);


END;


IF NOT EXISTS (


    SELECT 1 FROM sys.indexes 


    WHERE name = 'PkIntegrationLog' 


      AND object_id = OBJECT_ID('Core.IntegrationLog')


)


BEGIN


CREATE UNIQUE INDEX PkIntegrationLog ON Core.IntegrationLog (Id);


END;


IF NOT EXISTS (


    SELECT 1 FROM sys.indexes 


    WHERE name = 'Ix_TriggeredRule_TransactionMonitoringId' 


      AND object_id = OBJECT_ID('Core.TriggeredRule')


)


BEGIN


CREATE INDEX Ix_TriggeredRule_TransactionMonitoringId ON Core.TriggeredRule (TransactionMonitoringId);


END;


IF NOT EXISTS (


    SELECT 1 FROM sys.indexes 


    WHERE name = 'PkTriggeredRule' 


      AND object_id = OBJECT_ID('Core.TriggeredRule')


)


BEGIN


CREATE UNIQUE INDEX PkTriggeredRule ON Core.TriggeredRule (Id);


END;


IF NOT EXISTS (


    SELECT 1 FROM sys.indexes 


    WHERE name = 'PkTransactionMonitoring' 


      AND object_id = OBJECT_ID('Core.TransactionMonitoring')


)


BEGIN


CREATE UNIQUE INDEX PkTransactionMonitoring ON Core.TransactionMonitoring (Id);


END;
