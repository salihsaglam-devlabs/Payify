IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Core')
    EXEC('CREATE SCHEMA Core');

IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.TABLES
    WHERE TABLE_SCHEMA = 'Core' AND TABLE_NAME = 'CronJob'
)
BEGIN
    CREATE TABLE Core.CronJob
    (
        Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PkCronJob PRIMARY KEY,
        Name NVARCHAR(100) NOT NULL,
        CronExpression NVARCHAR(20) NOT NULL,
        Description NVARCHAR(300) NULL,
        Module NVARCHAR(100) NOT NULL,
        CronJobType NVARCHAR(50) NOT NULL,
        HttpType NVARCHAR(50) NOT NULL,
        Uri NVARCHAR(500) NULL,
        CreateDate DATETIME2(7) NOT NULL,
        UpdateDate DATETIME2(7) NULL,
        CreatedBy NVARCHAR(50) NOT NULL,
        LastModifiedBy NVARCHAR(50) NULL,
        RecordStatus NVARCHAR(50) NOT NULL
    );
END;