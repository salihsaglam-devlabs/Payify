IF OBJECT_ID('Physical.ReconciliationTransaction','U') IS NOT NULL
AND EXISTS (
    SELECT 1
    FROM sys.columns
    WHERE object_id = OBJECT_ID('Physical.ReconciliationTransaction')
      AND name = 'BankRef'
      AND is_nullable = 0
)
BEGIN
ALTER TABLE Physical.ReconciliationTransaction
ALTER COLUMN BankRef VARCHAR(50) NULL;
END
GO

IF OBJECT_ID('Physical.UnacceptableTransaction','U') IS NOT NULL
AND EXISTS (
    SELECT 1
    FROM sys.columns
    WHERE object_id = OBJECT_ID('Physical.UnacceptableTransaction')
      AND name = 'BankRef'
      AND is_nullable = 0
)
BEGIN
ALTER TABLE Physical.UnacceptableTransaction
ALTER COLUMN BankRef VARCHAR(50) NULL;
END
GO