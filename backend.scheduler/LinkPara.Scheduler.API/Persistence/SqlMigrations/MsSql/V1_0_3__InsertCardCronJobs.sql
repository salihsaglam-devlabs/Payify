IF EXISTS (SELECT 1 FROM Core.CronJob WHERE Name = 'ImportCardTransactionsFromFtpJob')
BEGIN
    UPDATE Core.CronJob
    SET
        CronExpression = '15,45 * * * *',
        Description = 'Imports card transaction files from FTP',
        Module = 'Card',
        CronJobType = 'QueueMessage',
        HttpType = 'None',
        Uri = NULL,
        UpdateDate = GETDATE(),
        LastModifiedBy = 'BATCH',
        RecordStatus = 'Active'
    WHERE Name = 'ImportCardTransactionsFromFtpJob';
END
ELSE
BEGIN
    INSERT INTO Core.CronJob
    (Id, Name, CronExpression, Description, Module, CronJobType, HttpType, Uri, CreateDate, UpdateDate, CreatedBy, LastModifiedBy, RecordStatus)
    VALUES
    ('bdb6d2ee-9198-4b7f-bb5a-02adf7236f7d', 'ImportCardTransactionsFromFtpJob', '15,45 * * * *', 'Imports card transaction files from FTP', 'Card', 'QueueMessage', 'None', NULL, GETDATE(), NULL, 'BATCH', NULL, 'Active');
END;

IF EXISTS (SELECT 1 FROM Core.CronJob WHERE Name = 'ImportClearingFromFtpJob')
BEGIN
    UPDATE Core.CronJob
    SET
        CronExpression = '0,30 * * * *',
        Description = 'Imports clearing files from FTP',
        Module = 'Card',
        CronJobType = 'QueueMessage',
        HttpType = 'None',
        Uri = NULL,
        UpdateDate = GETDATE(),
        LastModifiedBy = 'BATCH',
        RecordStatus = 'Active'
    WHERE Name = 'ImportClearingFromFtpJob';
END
ELSE
BEGIN
    INSERT INTO Core.CronJob
    (Id, Name, CronExpression, Description, Module, CronJobType, HttpType, Uri, CreateDate, UpdateDate, CreatedBy, LastModifiedBy, RecordStatus)
    VALUES
    ('2894ce4d-1575-4e3e-a549-b31612b97ec0', 'ImportClearingFromFtpJob', '0,30 * * * *', 'Imports clearing files from FTP', 'Card', 'QueueMessage', 'None', NULL, GETDATE(), NULL, 'BATCH', NULL, 'Active');
END;

IF EXISTS (SELECT 1 FROM Core.CronJob WHERE Name = 'ExecutePendingOperationsJob')
BEGIN
    UPDATE Core.CronJob
    SET
        CronExpression = '20 22 * * *',
        Description = 'Executes pending reconciliation operations',
        Module = 'Card',
        CronJobType = 'QueueMessage',
        HttpType = 'None',
        Uri = NULL,
        UpdateDate = GETDATE(),
        LastModifiedBy = 'BATCH',
        RecordStatus = 'Active'
    WHERE Name = 'ExecutePendingOperationsJob';
END
ELSE
BEGIN
    INSERT INTO Core.CronJob
    (Id, Name, CronExpression, Description, Module, CronJobType, HttpType, Uri, CreateDate, UpdateDate, CreatedBy, LastModifiedBy, RecordStatus)
    VALUES
    ('8f9fcf42-ec8f-4e4f-a318-39357b466098', 'ExecutePendingOperationsJob', '20 22 * * *', 'Executes pending reconciliation operations', 'Card', 'QueueMessage', 'None', NULL, GETDATE(), NULL, 'BATCH', NULL, 'Active');
END;

IF EXISTS (SELECT 1 FROM Core.CronJob WHERE Name = 'RegenerateOperationsJob')
BEGIN
    UPDATE Core.CronJob
    SET
        CronExpression = '1 22 * * *',
        Description = 'Regenerates reconciliation operations',
        Module = 'Card',
        CronJobType = 'QueueMessage',
        HttpType = 'None',
        Uri = NULL,
        UpdateDate = GETDATE(),
        LastModifiedBy = 'BATCH',
        RecordStatus = 'Active'
    WHERE Name = 'RegenerateOperationsJob';
END
ELSE
BEGIN
    INSERT INTO Core.CronJob
    (Id, Name, CronExpression, Description, Module, CronJobType, HttpType, Uri, CreateDate, UpdateDate, CreatedBy, LastModifiedBy, RecordStatus)
    VALUES
    ('949f1fab-0cb6-4e20-8d8d-c66a2632dfaf', 'RegenerateOperationsJob', '1 22 * * *', 'Regenerates reconciliation operations', 'Card', 'QueueMessage', 'None', NULL, GETDATE(), NULL, 'BATCH', NULL, 'Active');
END;
