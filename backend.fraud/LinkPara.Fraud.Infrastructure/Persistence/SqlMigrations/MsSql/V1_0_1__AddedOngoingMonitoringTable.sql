IF NOT EXISTS (
    SELECT 1
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'Core'
      AND TABLE_NAME = 'SearchLog'
      AND COLUMN_NAME = 'ReferenceNumber'
)
BEGIN
    ALTER TABLE Core.SearchLog ADD ReferenceNumber  VARCHAR(50);
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM INFORMATION_SCHEMA.TABLES
    WHERE TABLE_SCHEMA = 'Core'
      AND TABLE_NAME = 'OngoingMonitoring'
)
BEGIN
    CREATE TABLE Core.OngoingMonitoring (
        Id UNIQUEIDENTIFIER NOT NULL,
        SearchName VARCHAR(200) NOT NULL,
        SearchType VARCHAR(50) NOT NULL,
        ScanId VARCHAR(50),
        Period VARCHAR(50) NOT NULL,
        IsOngoingList BIT NOT NULL,
        CreateDate DATETIME2 NOT NULL,
        UpdateDate DATETIME2,
        CreatedBy VARCHAR(50) NOT NULL,
        LastModifiedBy VARCHAR(50),
        RecordStatus VARCHAR(50) NOT NULL,
        CONSTRAINT PK_OngoingMonitoring PRIMARY KEY (Id)
    );
END
GO