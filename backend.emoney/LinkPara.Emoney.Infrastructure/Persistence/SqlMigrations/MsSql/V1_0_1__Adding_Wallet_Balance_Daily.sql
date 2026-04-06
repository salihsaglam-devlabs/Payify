IF NOT EXISTS (
    SELECT * FROM INFORMATION_SCHEMA.TABLES 
    WHERE TABLE_SCHEMA = 'Core' AND TABLE_NAME = 'WalletBalanceDaily'
)
BEGIN
    CREATE TABLE Core.WalletBalanceDaily (
        Id uniqueidentifier NOT NULL,
        CreateDate datetime2 NOT NULL,
        CreatedBy nvarchar(50) NOT NULL,
        Currency nvarchar(10),
        DailyBalance decimal(18,2) NOT NULL,
        JobDate datetime2 NOT NULL,
        LastModifiedBy nvarchar(50),
        RecordStatus nvarchar(50) NOT NULL,
        UpdateDate datetime2,
        CONSTRAINT PK_WalletBalanceDaily PRIMARY KEY (Id)
    );
END