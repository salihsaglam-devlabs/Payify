-- Drop column if exists
IF EXISTS (
    SELECT 1
    FROM sys.columns 
    WHERE Name = N'TotalSubmerchantDeductionAmount'
      AND Object_ID = Object_ID(N'Posting.Balance')
)

BEGIN
    DECLARE @dfName sysname;

    SELECT @dfName = dc.name
    FROM sys.default_constraints dc
    JOIN sys.columns c
      ON c.default_object_id = dc.object_id
    WHERE c.object_id = OBJECT_ID(N'Posting.Balance')
      AND c.name = N'TotalSubmerchantDeductionAmount';

    IF @dfName IS NOT NULL
    BEGIN
        EXEC(N'ALTER TABLE Posting.Balance DROP CONSTRAINT [' + @dfName + N']');
    END

    ALTER TABLE Posting.Balance DROP COLUMN TotalSubmerchantDeductionAmount;
END


-- Add columns if not exists
    IF NOT EXISTS (
    SELECT 1 FROM sys.columns 
    WHERE Name = N'TotalSuspiciousTransferAmount'
      AND Object_ID = Object_ID(N'Posting.Balance')
)
BEGIN
ALTER TABLE Posting.Balance
    ADD TotalSuspiciousTransferAmount numeric(18,4) NOT NULL DEFAULT(0.0);
END


    IF NOT EXISTS (
    SELECT 1 FROM sys.columns 
    WHERE Name = N'TotalChargebackCommissionAmount'
      AND Object_ID = Object_ID(N'Posting.Balance')
)
BEGIN
ALTER TABLE Posting.Balance
    ADD TotalChargebackCommissionAmount numeric(18,4) NOT NULL DEFAULT(0.0);
END


    IF NOT EXISTS (
    SELECT 1 FROM sys.columns 
    WHERE Name = N'TotalChargebackTransferAmount'
      AND Object_ID = Object_ID(N'Posting.Balance')
)
BEGIN
ALTER TABLE Posting.Balance
    ADD TotalChargebackTransferAmount numeric(18,4) NOT NULL DEFAULT(0.0);
END


    IF NOT EXISTS (
    SELECT 1 FROM sys.columns 
    WHERE Name = N'TotalDueTransferAmount'
      AND Object_ID = Object_ID(N'Posting.Balance')
)
BEGIN
ALTER TABLE Posting.Balance
    ADD TotalDueTransferAmount numeric(18,4) NOT NULL DEFAULT(0.0);
END


    IF NOT EXISTS (
    SELECT 1 FROM sys.columns 
    WHERE Name = N'TotalExcessReturnOnCommissionAmount'
      AND Object_ID = Object_ID(N'Posting.Balance')
)
BEGIN
ALTER TABLE Posting.Balance
    ADD TotalExcessReturnOnCommissionAmount numeric(18,4) NOT NULL DEFAULT(0.0);
END


    IF NOT EXISTS (
    SELECT 1 FROM sys.columns 
    WHERE Name = N'TotalExcessReturnTransferAmount'
      AND Object_ID = Object_ID(N'Posting.Balance')
)
BEGIN
ALTER TABLE Posting.Balance
    ADD TotalExcessReturnTransferAmount numeric(18,4) NOT NULL DEFAULT(0.0);
END


    IF NOT EXISTS (
    SELECT 1 FROM sys.columns 
    WHERE Name = N'TotalSuspiciousCommissionAmount'
      AND Object_ID = Object_ID(N'Posting.Balance')
)
BEGIN
ALTER TABLE Posting.Balance
    ADD TotalSuspiciousCommissionAmount numeric(18,4) NOT NULL DEFAULT(0.0);
END
