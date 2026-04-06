IF EXISTS (
    SELECT 1
    FROM sys.columns 
    WHERE Name = N'IssuerBankId'
      AND Object_ID = Object_ID(N'Physical.UnacceptableTransaction')
)
BEGIN
    DECLARE @dfName sysname;

SELECT @dfName = dc.name
FROM sys.default_constraints dc
         JOIN sys.columns c
              ON c.default_object_id = dc.object_id
WHERE c.object_id = OBJECT_ID(N'Physical.UnacceptableTransaction')
  AND c.name = N'IssuerBankId';

IF @dfName IS NOT NULL
BEGIN
EXEC(N'ALTER TABLE Physical.UnacceptableTransaction DROP CONSTRAINT [' + @dfName + N']');
END

ALTER TABLE Physical.UnacceptableTransaction DROP COLUMN IssuerBankId;
END

IF NOT EXISTS (
    SELECT 1
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'Physical'
      AND TABLE_NAME = 'UnacceptableTransaction'
      AND COLUMN_NAME = 'EndOfDayStatus'
)
BEGIN
ALTER TABLE Physical.UnacceptableTransaction ADD EndOfDayStatus VARCHAR(50) NOT NULL CONSTRAINT DF_Physical_UnacceptableTransaction_EndOfDayStatus DEFAULT 'Pending';
END
GO


IF COL_LENGTH('Physical.UnacceptableTransaction','MerchantTransactionId') IS NULL
BEGIN
ALTER TABLE Physical.[UnacceptableTransaction]
    ADD MerchantTransactionId UNIQUEIDENTIFIER NOT NULL
    CONSTRAINT DF_Physical_UnacceptableTransaction_MerchantTransactionId
    DEFAULT CONVERT(uniqueidentifier,'00000000-0000-0000-0000-000000000000');
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'Physical'
      AND TABLE_NAME = 'ReconciliationTransaction'
      AND COLUMN_NAME = 'ErrorCode'
)
BEGIN
ALTER TABLE Physical.ReconciliationTransaction ADD ErrorCode VARCHAR(20);
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'Physical'
      AND TABLE_NAME = 'ReconciliationTransaction'
      AND COLUMN_NAME = 'ErrorMessage'
)
BEGIN
ALTER TABLE Physical.ReconciliationTransaction ADD ErrorMessage VARCHAR(300);
END
GO
