IF COL_LENGTH('Merchant.MerchantDeduction', 'ProcessingId') IS NULL
BEGIN
ALTER TABLE Merchant.MerchantDeduction ADD ProcessingId uniqueidentifier NULL;
END

IF COL_LENGTH('Merchant.MerchantDeduction', 'ProcessingStartedAt') IS NULL
BEGIN
ALTER TABLE Merchant.MerchantDeduction ADD ProcessingStartedAt datetime2 NOT NULL CONSTRAINT DF_MerchantDeduction_ProcessingStartedAt DEFAULT ('0001-01-01T00:00:00');
END

IF COL_LENGTH('Posting.Balance', 'ProcessingId') IS NULL
BEGIN
ALTER TABLE Posting.Balance ADD ProcessingId uniqueidentifier NULL;
END

IF COL_LENGTH('Posting.Balance', 'ProcessingStartedAt') IS NULL
BEGIN
ALTER TABLE Posting.Balance ADD ProcessingStartedAt datetime2 NOT NULL CONSTRAINT DF_Balance_ProcessingStartedAt DEFAULT ('0001-01-01T00:00:00');
END