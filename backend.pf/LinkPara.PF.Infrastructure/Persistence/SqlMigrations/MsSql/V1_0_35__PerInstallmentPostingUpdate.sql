IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Transaction_MerchantTransactionId' AND object_id = OBJECT_ID('Posting.Transaction'))
DROP INDEX IX_Transaction_MerchantTransactionId ON Posting.Transaction;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Posting.Transaction') AND name = 'InstallmentSequence')
ALTER TABLE Posting.Transaction ADD InstallmentSequence int NOT NULL DEFAULT 0;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Posting.Transaction') AND name = 'IsPerInstallment')
ALTER TABLE Posting.Transaction ADD IsPerInstallment bit NOT NULL DEFAULT 0;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Posting.Transaction') AND name = 'MerchantInstallmentTransactionId')
ALTER TABLE Posting.Transaction ADD MerchantInstallmentTransactionId uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Posting.PostingAdditionalTransaction') AND name = 'InstallmentSequence')
ALTER TABLE Posting.PostingAdditionalTransaction ADD InstallmentSequence int NOT NULL DEFAULT 0;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Posting.PostingAdditionalTransaction') AND name = 'IsPerInstallment')
ALTER TABLE Posting.PostingAdditionalTransaction ADD IsPerInstallment bit NOT NULL DEFAULT 0;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Posting.PostingAdditionalTransaction') AND name = 'MerchantInstallmentTransactionId')
ALTER TABLE Posting.PostingAdditionalTransaction ADD MerchantInstallmentTransactionId uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Transaction_MerchantTransactionId_MerchantInstallmentTr' AND object_id = OBJECT_ID('Posting.Transaction'))
CREATE UNIQUE INDEX IX_Transaction_MerchantTransactionId_MerchantInstallmentTr
    ON Posting.Transaction (MerchantTransactionId, MerchantInstallmentTransactionId);