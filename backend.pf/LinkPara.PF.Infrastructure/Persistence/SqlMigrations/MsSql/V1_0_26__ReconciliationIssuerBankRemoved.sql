IF EXISTS (
    SELECT 1
    FROM sys.columns 
    WHERE Name = N'IssuerBankId'
      AND Object_ID = Object_ID(N'Physical.ReconciliationTransaction')
)
BEGIN
    DECLARE @dfName sysname;

SELECT @dfName = dc.name
FROM sys.default_constraints dc
         JOIN sys.columns c
              ON c.default_object_id = dc.object_id
WHERE c.object_id = OBJECT_ID(N'Physical.ReconciliationTransaction')
  AND c.name = N'IssuerBankId';

IF @dfName IS NOT NULL
BEGIN
EXEC(N'ALTER TABLE Physical.ReconciliationTransaction DROP CONSTRAINT [' + @dfName + N']');
END

ALTER TABLE Physical.ReconciliationTransaction DROP COLUMN IssuerBankId;
END