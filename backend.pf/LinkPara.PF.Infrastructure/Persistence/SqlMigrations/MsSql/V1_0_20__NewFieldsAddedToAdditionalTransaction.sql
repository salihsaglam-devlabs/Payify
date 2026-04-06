IF NOT EXISTS (
    SELECT 1 
    FROM sys.columns 
    WHERE object_id = OBJECT_ID('Posting.PostingAdditionalTransaction') 
    AND name = 'MerchantPhysicalPosId'
)
BEGIN
ALTER TABLE Posting.PostingAdditionalTransaction
    ADD MerchantPhysicalPosId UNIQUEIDENTIFIER NOT NULL
    CONSTRAINT DF_PostingAdditionalTransaction_MerchantPhysicalPosId
    DEFAULT '00000000-0000-0000-0000-000000000000';
END

IF NOT EXISTS (
    SELECT 1 
    FROM sys.columns 
    WHERE object_id = OBJECT_ID('Posting.PostingAdditionalTransaction') 
    AND name = 'PfTransactionSource'
)
BEGIN
ALTER TABLE Posting.PostingAdditionalTransaction
    ADD PfTransactionSource NVARCHAR(50) NOT NULL 
        CONSTRAINT DF_PostingAdditionalTransaction_PfTransactionSource 
        DEFAULT 'VirtualPos';
END