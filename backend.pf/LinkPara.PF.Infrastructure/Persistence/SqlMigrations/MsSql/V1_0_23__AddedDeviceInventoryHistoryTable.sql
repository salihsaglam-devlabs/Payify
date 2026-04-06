
IF NOT EXISTS (
    SELECT 1
    FROM INFORMATION_SCHEMA.TABLES
    WHERE TABLE_SCHEMA = 'Physical'
      AND TABLE_NAME = 'DeviceInventoryHistory'
)
BEGIN
    CREATE TABLE Physical.DeviceInventoryHistory (
      Id UNIQUEIDENTIFIER NOT NULL,
        DeviceInventoryId UNIQUEIDENTIFIER NOT NULL,
        DeviceHistoryType NVARCHAR(50) NOT NULL,
        NewData NVARCHAR(500) NULL,
        OldData NVARCHAR(500) NULL,
        Detail NVARCHAR(256) NULL,
        CreatedNameBy NVARCHAR(256) NULL,
        CreateDate DATETIME2 NOT NULL,
        UpdateDate DATETIME2 NULL,
        CreatedBy NVARCHAR(50) NOT NULL,
        LastModifiedBy NVARCHAR(50) NULL,
        RecordStatus NVARCHAR(50) NOT NULL,

        CONSTRAINT PK_DeviceInventoryHistory PRIMARY KEY (Id),

        CONSTRAINT FK_DeviceInventoryHistory_DeviceInventory_DeviceInventoryId
            FOREIGN KEY (DeviceInventoryId)
            REFERENCES Physical.DeviceInventory (Id)
            ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'IX_DeviceInventoryHistory_DeviceInventoryId'
      AND object_id = OBJECT_ID('[Physical].[DeviceInventoryHistory]')
)
BEGIN
    CREATE INDEX IX_DeviceInventoryHistory_DeviceInventoryId
    ON Physical.DeviceInventoryHistory (DeviceInventoryId);
END;