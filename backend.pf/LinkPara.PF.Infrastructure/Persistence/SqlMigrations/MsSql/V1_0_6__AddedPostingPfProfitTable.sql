IF NOT EXISTS (
    SELECT 1
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'Posting'
      AND TABLE_NAME = 'Transaction'
      AND COLUMN_NAME = 'BankPaymentDate'
)
BEGIN
ALTER TABLE Posting.[Transaction]
    ADD BankPaymentDate DATE NOT NULL DEFAULT '0001-01-01';
END;

IF NOT EXISTS (
    SELECT 1
    FROM INFORMATION_SCHEMA.TABLES
    WHERE TABLE_SCHEMA = 'Posting'
      AND TABLE_NAME = 'PfProfit'
)
BEGIN
CREATE TABLE Posting.PfProfit (
                                  Id UNIQUEIDENTIFIER PRIMARY KEY,
                                  PaymentDate DATE NOT NULL,
                                  AmountWithoutBankCommission NUMERIC(18,4) NOT NULL,
                                  TotalPayingAmount NUMERIC(18,4) NOT NULL,
                                  TotalPfNetCommissionAmount NUMERIC(18,4) NOT NULL,
                                  ProtectionTransferAmount NUMERIC(18,4) NOT NULL,
                                  Currency INT NOT NULL,
                                  CreateDate DATETIME2 NOT NULL,
                                  UpdateDate DATETIME2 NULL,
                                  CreatedBy VARCHAR(50) NOT NULL,
                                  LastModifiedBy VARCHAR(50) NULL,
                                  RecordStatus VARCHAR(50) NOT NULL
);
END;

IF NOT EXISTS (
    SELECT 1
    FROM INFORMATION_SCHEMA.TABLES
    WHERE TABLE_SCHEMA = 'Posting'
      AND TABLE_NAME = 'PfProfitDetail'
)
BEGIN
    CREATE TABLE Posting.PfProfitDetail (
        Id UNIQUEIDENTIFIER PRIMARY KEY,
        AcquireBankCode INT NOT NULL,
        BankName VARCHAR(20) NOT NULL,
        BankPayingAmount NUMERIC(18,4) NOT NULL,
        Currency INT NOT NULL,
        PostingPfProfitId UNIQUEIDENTIFIER NOT NULL,
        CreateDate DATETIME2 NOT NULL,
        UpdateDate DATETIME2 NULL,
        CreatedBy VARCHAR(50) NOT NULL,
        LastModifiedBy VARCHAR(50) NULL,
        RecordStatus VARCHAR(50) NOT NULL,
        CONSTRAINT FK_PfProfitDetail_PfProfit_PostingPfProfitId
            FOREIGN KEY (PostingPfProfitId)
                REFERENCES Posting.PfProfit (Id)
                ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'IX_PfProfitDetail_PostingPfProfitId'
      AND object_id = OBJECT_ID('[Posting].[PfProfitDetail]')
)
BEGIN
    CREATE INDEX IX_PfProfitDetail_PostingPfProfitId
    ON Posting.PfProfitDetail (PostingPfProfitId);
END;
