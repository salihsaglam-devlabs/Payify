----------------------------------------------------
-- 1) FOREIGN KEY DROP (varsa)
----------------------------------------------------
IF EXISTS (
    SELECT 1 FROM sys.foreign_keys 
    WHERE name = 'FK_AcquireBank_Bank_BankId'
      AND parent_object_id = OBJECT_ID('Bank.AcquireBank')
)
BEGIN
    ALTER TABLE Bank.AcquireBank DROP CONSTRAINT FK_AcquireBank_Bank_BankId;
END;

IF EXISTS (
    SELECT 1 FROM sys.foreign_keys 
    WHERE name = 'FK_BlockageDetail_Blockage_MerchantBlockageId'
      AND parent_object_id = OBJECT_ID('Merchant.BlockageDetail')
)
BEGIN
    ALTER TABLE Merchant.BlockageDetail DROP CONSTRAINT FK_BlockageDetail_Blockage_MerchantBlockageId;
END;

IF EXISTS (
    SELECT 1 FROM sys.foreign_keys 
    WHERE name = 'FK_CostProfile_Currency_CurrencyId'
      AND parent_object_id = OBJECT_ID('Core.CostProfile')
)
BEGIN
    ALTER TABLE Core.CostProfile DROP CONSTRAINT FK_CostProfile_Currency_CurrencyId;
END;

IF EXISTS (
    SELECT 1 FROM sys.foreign_keys 
    WHERE name = 'FK_Merchant_Mcc_MccId'
      AND parent_object_id = OBJECT_ID('Merchant.Merchant')
)
BEGIN
    ALTER TABLE Merchant.Merchant DROP CONSTRAINT FK_Merchant_Mcc_MccId;
END;

IF EXISTS (
    SELECT 1 FROM sys.foreign_keys 
    WHERE name = 'FK_PricingProfile_Currency_CurrencyId'
      AND parent_object_id = OBJECT_ID('Core.PricingProfile')
)
BEGIN
    ALTER TABLE Core.PricingProfile DROP CONSTRAINT FK_PricingProfile_Currency_CurrencyId;
END;


----------------------------------------------------
-- 2) FOREIGN KEY ADD (yoksa)
----------------------------------------------------
IF NOT EXISTS (
    SELECT 1 FROM sys.foreign_keys 
    WHERE name = 'FK_AcquireBank_Bank_BankCode'
      AND parent_object_id = OBJECT_ID('Bank.AcquireBank')
)
BEGIN
    ALTER TABLE Bank.AcquireBank
    ADD CONSTRAINT FK_AcquireBank_Bank_BankCode 
    FOREIGN KEY (BankCode) REFERENCES Bank.Bank (Code) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT 1 FROM sys.foreign_keys 
    WHERE name = 'FK_BlockageDetail_Blockage_MerchantBlockageId'
      AND parent_object_id = OBJECT_ID('Merchant.BlockageDetail')
)
BEGIN
    ALTER TABLE Merchant.BlockageDetail
    ADD CONSTRAINT FK_BlockageDetail_Blockage_MerchantBlockageId 
    FOREIGN KEY (MerchantBlockageId) REFERENCES Merchant.Blockage (Id) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT 1 FROM sys.foreign_keys 
    WHERE name = 'FK_CostProfile_Currency_CurrencyCode'
      AND parent_object_id = OBJECT_ID('Core.CostProfile')
)
BEGIN
    ALTER TABLE Core.CostProfile
    ADD CONSTRAINT FK_CostProfile_Currency_CurrencyCode 
    FOREIGN KEY (CurrencyCode) REFERENCES Core.Currency (Code) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT 1 FROM sys.foreign_keys 
    WHERE name = 'FK_Merchant_Mcc_MccCode'
      AND parent_object_id = OBJECT_ID('Merchant.Merchant')
)
BEGIN
    ALTER TABLE Merchant.Merchant
    ADD CONSTRAINT FK_Merchant_Mcc_MccCode 
    FOREIGN KEY (MccCode) REFERENCES Merchant.Mcc (Code) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT 1 FROM sys.foreign_keys 
    WHERE name = 'FK_PricingProfile_Currency_CurrencyCode'
      AND parent_object_id = OBJECT_ID('Core.PricingProfile')
)
BEGIN
    ALTER TABLE Core.PricingProfile
    ADD CONSTRAINT FK_PricingProfile_Currency_CurrencyCode 
    FOREIGN KEY (CurrencyCode) REFERENCES Core.Currency (Code) ON DELETE NO ACTION;
END;


----------------------------------------------------
-- 3) COLUMN TYPE CHANGE
----------------------------------------------------
-- Posting.Transaction.TransactionStartDate
IF EXISTS (
    SELECT 1 FROM sys.columns 
    WHERE object_id = OBJECT_ID('Posting.Transaction')
      AND name = 'TransactionStartDate'
      AND system_type_id <> TYPE_ID('date')
)
BEGIN
    ALTER TABLE Posting."Transaction"
    ALTER COLUMN TransactionStartDate DATE;
END;

-- Posting.Transaction.TransactionEndDate
IF EXISTS (
    SELECT 1 FROM sys.columns 
    WHERE object_id = OBJECT_ID('Posting.Transaction')
      AND name = 'TransactionEndDate'
      AND system_type_id <> TYPE_ID('date')
)
BEGIN
    ALTER TABLE Posting."Transaction" 
    ALTER COLUMN TransactionEndDate DATE;
END;

-- Posting.PostingAdditionalTransaction.TransactionStartDate
IF EXISTS (
    SELECT 1 FROM sys.columns 
    WHERE object_id = OBJECT_ID('Posting.PostingAdditionalTransaction')
      AND name = 'TransactionStartDate'
      AND system_type_id <> TYPE_ID('date')
)
BEGIN
    ALTER TABLE Posting.PostingAdditionalTransaction 
    ALTER COLUMN TransactionStartDate DATE;
END;

-- Posting.PostingAdditionalTransaction.TransactionEndDate
IF EXISTS (
    SELECT 1 FROM sys.columns 
    WHERE object_id = OBJECT_ID('Posting.PostingAdditionalTransaction')
      AND name = 'TransactionEndDate'
      AND system_type_id <> TYPE_ID('date')
)
BEGIN
    ALTER TABLE Posting.PostingAdditionalTransaction 
    ALTER COLUMN TransactionEndDate DATE;
END;

-- Core.CostProfile.ServiceCommission
IF EXISTS (
    SELECT 1 FROM sys.columns 
    WHERE object_id = OBJECT_ID('Core.CostProfile')
      AND name = 'ServiceCommission'
)
BEGIN
    ALTER TABLE Core.CostProfile 
    ALTER COLUMN ServiceCommission numeric(5,3);
END;

-- Core.CostProfile.PointCommission
IF EXISTS (
    SELECT 1 FROM sys.columns 
    WHERE object_id = OBJECT_ID('Core.CostProfile')
      AND name = 'PointCommission'
)
BEGIN
    ALTER TABLE Core.CostProfile 
    ALTER COLUMN PointCommission numeric(5,3);
END;

-- Core.CostProfileItem.CommissionRate
IF EXISTS (
    SELECT 1 FROM sys.columns 
    WHERE object_id = OBJECT_ID('Core.CostProfileItem')
      AND name = 'CommissionRate'
)
BEGIN
    ALTER TABLE Core.CostProfileItem 
    ALTER COLUMN CommissionRate numeric(5,3);
END;

-- Merchant.Transaction.BankCommissionRate
IF EXISTS (
    SELECT 1 FROM sys.columns 
    WHERE object_id = OBJECT_ID('Merchant.Transaction')
      AND name = 'BankCommissionRate'
)
BEGIN
    ALTER TABLE Merchant."Transaction" 
    ALTER COLUMN BankCommissionRate numeric(5,3);
END;
