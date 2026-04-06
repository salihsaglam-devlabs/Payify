-- MSSQL idempotent DDL script

-- Core.WithdrawRequest.Amount
IF EXISTS (SELECT 1 FROM sys.columns c JOIN sys.tables t ON c.object_id = t.object_id JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE s.name = 'Core' AND t.name = 'WithdrawRequest' AND c.name = 'Amount' AND (c.precision <> 18 OR c.scale <> 2))
BEGIN
    ALTER TABLE Core.WithdrawRequest ALTER COLUMN Amount numeric(18,2);
END

-- Core.Wallet.CurrentBalanceCredit
IF EXISTS (SELECT 1 FROM sys.columns c JOIN sys.tables t ON c.object_id = t.object_id JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE s.name = 'Core' AND t.name = 'Wallet' AND c.name = 'CurrentBalanceCredit' AND (c.precision <> 18 OR c.scale <> 2))
BEGIN
    ALTER TABLE Core.Wallet ALTER COLUMN CurrentBalanceCredit numeric(18,2);
END

-- Core.Wallet.CurrentBalanceCash
IF EXISTS (SELECT 1 FROM sys.columns c JOIN sys.tables t ON c.object_id = t.object_id JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE s.name = 'Core' AND t.name = 'Wallet' AND c.name = 'CurrentBalanceCash' AND (c.precision <> 18 OR c.scale <> 2))
BEGIN
    ALTER TABLE Core.Wallet ALTER COLUMN CurrentBalanceCash numeric(18,2);
END

-- Core.Wallet.BlockedBalance
IF EXISTS (SELECT 1 FROM sys.columns c JOIN sys.tables t ON c.object_id = t.object_id JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE s.name = 'Core' AND t.name = 'Wallet' AND c.name = 'BlockedBalance' AND (c.precision <> 18 OR c.scale <> 2))
BEGIN
    ALTER TABLE Core.Wallet ALTER COLUMN BlockedBalance numeric(18,2);
END

-- Core.TransferOrder.Amount
IF EXISTS (SELECT 1 FROM sys.columns c JOIN sys.tables t ON c.object_id = t.object_id JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE s.name = 'Core' AND t.name = 'TransferOrder' AND c.name = 'Amount' AND (c.precision <> 18 OR c.scale <> 2))
BEGIN
    ALTER TABLE Core.TransferOrder ALTER COLUMN Amount numeric(18,2);
END

-- Core.Transaction.PreBalance
IF EXISTS (SELECT 1 FROM sys.columns c JOIN sys.tables t ON c.object_id = t.object_id JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE s.name = 'Core' AND t.name = 'Transaction' AND c.name = 'PreBalance' AND (c.precision <> 18 OR c.scale <> 2))
BEGIN
    ALTER TABLE Core.[Transaction] ALTER COLUMN PreBalance numeric(18,2);
END

-- Core.Transaction.CurrentBalance
IF EXISTS (SELECT 1 FROM sys.columns c JOIN sys.tables t ON c.object_id = t.object_id JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE s.name = 'Core' AND t.name = 'Transaction' AND c.name = 'CurrentBalance' AND (c.precision <> 18 OR c.scale <> 2))
BEGIN
    ALTER TABLE Core.[Transaction] ALTER COLUMN CurrentBalance numeric(18,2);
END

-- Core.Transaction.Amount
IF EXISTS (SELECT 1 FROM sys.columns c JOIN sys.tables t ON c.object_id = t.object_id JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE s.name = 'Core' AND t.name = 'Transaction' AND c.name = 'Amount' AND (c.precision <> 18 OR c.scale <> 2))
BEGIN
    ALTER TABLE Core.[Transaction] ALTER COLUMN Amount numeric(18,2);
END

-- Core.Transaction.IpAddress (varsa ekleme)
IF NOT EXISTS (
    SELECT 1 FROM sys.columns c JOIN sys.tables t ON c.object_id = t.object_id JOIN sys.schemas s ON t.schema_id = s.schema_id
    WHERE s.name = 'Core' AND t.name = 'Transaction' AND c.name = 'IpAddress'
)
BEGIN
    ALTER TABLE Core.[Transaction] ADD IpAddress varchar(50);
END

-- limit.TierLevel.MonthlyMaxWithdrawalAmount
IF EXISTS (SELECT 1 FROM sys.columns c JOIN sys.tables t ON c.object_id = t.object_id JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE s.name = 'limit' AND t.name = 'TierLevel' AND c.name = 'MonthlyMaxWithdrawalAmount' AND (c.precision <> 18 OR c.scale <> 2))
BEGIN
    ALTER TABLE [Limit].TierLevel ALTER COLUMN MonthlyMaxWithdrawalAmount numeric(18,2);
END

-- limit.TierLevel.MonthlyMaxOtherIbanWithdrawalAmount
IF EXISTS (SELECT 1 FROM sys.columns c JOIN sys.tables t ON c.object_id = t.object_id JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE s.name = 'limit' AND t.name = 'TierLevel' AND c.name = 'MonthlyMaxOtherIbanWithdrawalAmount' AND (c.precision <> 18 OR c.scale <> 2))
BEGIN
    ALTER TABLE [Limit].TierLevel ALTER COLUMN MonthlyMaxOtherIbanWithdrawalAmount numeric(18,2);
END

-- limit.TierLevel.MonthlyMaxOnUsPaymentAmount
IF EXISTS (SELECT 1 FROM sys.columns c JOIN sys.tables t ON c.object_id = t.object_id JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE s.name = 'limit' AND t.name = 'TierLevel' AND c.name = 'MonthlyMaxOnUsPaymentAmount' AND (c.precision <> 18 OR c.scale <> 2))
BEGIN
    ALTER TABLE [Limit].TierLevel ALTER COLUMN MonthlyMaxOnUsPaymentAmount numeric(18,2);
END

-- limit.TierLevel.MonthlyMaxInternationalTransferAmount
IF EXISTS (SELECT 1 FROM sys.columns c JOIN sys.tables t ON c.object_id = t.object_id JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE s.name = 'limit' AND t.name = 'TierLevel' AND c.name = 'MonthlyMaxInternationalTransferAmount' AND (c.precision <> 18 OR c.scale <> 2))
BEGIN
    ALTER TABLE [Limit].TierLevel ALTER COLUMN MonthlyMaxInternationalTransferAmount numeric(18,2);
END

-- limit.TierLevel.MonthlyMaxInternalTransferAmount
IF EXISTS (SELECT 1 FROM sys.columns c JOIN sys.tables t ON c.object_id = t.object_id JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE s.name = 'limit' AND t.name = 'TierLevel' AND c.name = 'MonthlyMaxInternalTransferAmount' AND (c.precision <> 18 OR c.scale <> 2))
BEGIN
    ALTER TABLE [Limit].TierLevel ALTER COLUMN MonthlyMaxInternalTransferAmount numeric(18,2);
END

-- limit.TierLevel.MonthlyMaxDepositAmount
IF EXISTS (SELECT 1 FROM sys.columns c JOIN sys.tables t ON c.object_id = t.object_id JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE s.name = 'limit' AND t.name = 'TierLevel' AND c.name = 'MonthlyMaxDepositAmount' AND (c.precision <> 18 OR c.scale <> 2))
BEGIN
    ALTER TABLE [Limit].TierLevel ALTER COLUMN MonthlyMaxDepositAmount numeric(18,2);
END

-- limit.TierLevel.MaxBalance
IF EXISTS (SELECT 1 FROM sys.columns c JOIN sys.tables t ON c.object_id = t.object_id JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE s.name = 'limit' AND t.name = 'TierLevel' AND c.name = 'MaxBalance' AND (c.precision <> 18 OR c.scale <> 2))
BEGIN
    ALTER TABLE [Limit].TierLevel ALTER COLUMN MaxBalance numeric(18,2);
END

-- limit.TierLevel.DailyMaxWithdrawalAmount
IF EXISTS (SELECT 1 FROM sys.columns c JOIN sys.tables t ON c.object_id = t.object_id JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE s.name = 'limit' AND t.name = 'TierLevel' AND c.name = 'DailyMaxWithdrawalAmount' AND (c.precision <> 18 OR c.scale <> 2))
BEGIN
    ALTER TABLE [Limit].TierLevel ALTER COLUMN DailyMaxWithdrawalAmount numeric(18,2);
END

-- limit.TierLevel.DailyMaxOtherIbanWithdrawalAmount
IF EXISTS (SELECT 1 FROM sys.columns c JOIN sys.tables t ON c.object_id = t.object_id JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE s.name = 'limit' AND t.name = 'TierLevel' AND c.name = 'DailyMaxOtherIbanWithdrawalAmount' AND (c.precision <> 18 OR c.scale <> 2))
BEGIN
    ALTER TABLE [Limit].TierLevel ALTER COLUMN DailyMaxOtherIbanWithdrawalAmount numeric(18,2);
END

-- limit.TierLevel.DailyMaxOnUsPaymentAmount
IF EXISTS (SELECT 1 FROM sys.columns c JOIN sys.tables t ON c.object_id = t.object_id JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE s.name = 'limit' AND t.name = 'TierLevel' AND c.name = 'DailyMaxOnUsPaymentAmount' AND (c.precision <> 18 OR c.scale <> 2))
BEGIN
    ALTER TABLE [Limit].TierLevel ALTER COLUMN DailyMaxOnUsPaymentAmount numeric(18,2);
END

-- limit.TierLevel.DailyMaxInternationalTransferAmount
IF EXISTS (SELECT 1 FROM sys.columns c JOIN sys.tables t ON c.object_id = t.object_id JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE s.name = 'limit' AND t.name = 'TierLevel' AND c.name = 'DailyMaxInternationalTransferAmount' AND (c.precision <> 18 OR c.scale <> 2))
BEGIN
    ALTER TABLE [Limit].TierLevel ALTER COLUMN DailyMaxInternationalTransferAmount numeric(18,2);
END

-- limit.TierLevel.DailyMaxInternalTransferAmount
IF EXISTS (SELECT 1 FROM sys.columns c JOIN sys.tables t ON c.object_id = t.object_id JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE s.name = 'limit' AND t.name = 'TierLevel' AND c.name = 'DailyMaxInternalTransferAmount' AND (c.precision <> 18 OR c.scale <> 2))
BEGIN
    ALTER TABLE [Limit].TierLevel ALTER COLUMN DailyMaxInternalTransferAmount numeric(18,2);
END

-- limit.TierLevel.DailyMaxDepositAmount
IF EXISTS (SELECT 1 FROM sys.columns c JOIN sys.tables t ON c.object_id = t.object_id JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE s.name = 'limit' AND t.name = 'TierLevel' AND c.name = 'DailyMaxDepositAmount' AND (c.precision <> 18 OR c.scale <> 2))
BEGIN
    ALTER TABLE [Limit].TierLevel ALTER COLUMN DailyMaxDepositAmount numeric(18,2);
END

-- Core.Provision.CommissionAmount
IF EXISTS (SELECT 1 FROM sys.columns c JOIN sys.tables t ON c.object_id = t.object_id JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE s.name = 'Core' AND t.name = 'Provision' AND c.name = 'CommissionAmount' AND (c.precision <> 18 OR c.scale <> 2))
BEGIN
    ALTER TABLE Core.Provision ALTER COLUMN CommissionAmount numeric(18,2);
END

-- Core.Provision.BsmvAmount
IF EXISTS (SELECT 1 FROM sys.columns c JOIN sys.tables t ON c.object_id = t.object_id JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE s.name = 'Core' AND t.name = 'Provision' AND c.name = 'BsmvAmount' AND (c.precision <> 18 OR c.scale <> 2))
BEGIN
    ALTER TABLE Core.Provision ALTER COLUMN BsmvAmount numeric(18,2);
END

-- Core.Provision.Amount
IF EXISTS (SELECT 1 FROM sys.columns c JOIN sys.tables t ON c.object_id = t.object_id JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE s.name = 'Core' AND t.name = 'Provision' AND c.name = 'Amount' AND (c.precision <> 18 OR c.scale <> 2))
BEGIN
    ALTER TABLE Core.Provision ALTER COLUMN Amount numeric(18,2);
END

-- Core.PricingProfileItem.MinAmount
IF EXISTS (SELECT 1 FROM sys.columns c JOIN sys.tables t ON c.object_id = t.object_id JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE s.name = 'Core' AND t.name = 'PricingProfileItem' AND c.name = 'MinAmount' AND (c.precision <> 18 OR c.scale <> 2))
BEGIN
    ALTER TABLE Core.PricingProfileItem ALTER COLUMN MinAmount numeric(18,2);
END

-- Core.PricingProfileItem.MaxAmount
IF EXISTS (SELECT 1 FROM sys.columns c JOIN sys.tables t ON c.object_id = t.object_id JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE s.name = 'Core' AND t.name = 'PricingProfileItem' AND c.name = 'MaxAmount' AND (c.precision <> 18 OR c.scale <> 2))
BEGIN
    ALTER TABLE Core.PricingProfileItem ALTER COLUMN MaxAmount numeric(18,2);
END

-- Core.PricingProfileItem.Fee
IF EXISTS (SELECT 1 FROM sys.columns c JOIN sys.tables t ON c.object_id = t.object_id JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE s.name = 'Core' AND t.name = 'PricingProfileItem' AND c.name = 'Fee' AND (c.precision <> 18 OR c.scale <> 2))
BEGIN
    ALTER TABLE Core.PricingProfileItem ALTER COLUMN Fee numeric(18,2);
END

-- Core.PricingProfileItem.CommissionRate
IF EXISTS (SELECT 1 FROM sys.columns c JOIN sys.tables t ON c.object_id = t.object_id JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE s.name = 'Core' AND t.name = 'PricingProfileItem' AND c.name = 'CommissionRate' AND (c.precision <> 18 OR c.scale <> 2))
BEGIN
    ALTER TABLE Core.PricingProfileItem ALTER COLUMN CommissionRate numeric(18,2);
END

-- Core.PricingCommercial.MaxDistinctSenderAmount
IF EXISTS (
    SELECT 1 
    FROM sys.columns c
    JOIN sys.tables t ON c.object_id = t.object_id
    JOIN sys.schemas s ON t.schema_id = s.schema_id
    WHERE s.name = 'Core' 
      AND t.name = 'PricingCommercial' 
      AND c.name = 'MaxDistinctSenderAmount' 
      AND (c.precision <> 18 OR c.scale <> 2)
)
BEGIN
    DECLARE @df_name NVARCHAR(128);

    SELECT @df_name = df.name
    FROM sys.default_constraints df
    JOIN sys.columns c 
        ON df.parent_object_id = c.object_id 
       AND df.parent_column_id = c.column_id
    JOIN sys.tables t ON c.object_id = t.object_id
    JOIN sys.schemas s ON t.schema_id = s.schema_id
    WHERE s.name = 'Core' 
      AND t.name = 'PricingCommercial'
      AND c.name = 'MaxDistinctSenderAmount';

    IF @df_name IS NOT NULL
        EXEC('ALTER TABLE Core.PricingCommercial DROP CONSTRAINT [' + @df_name + ']');

    ALTER TABLE Core.PricingCommercial 
        ALTER COLUMN MaxDistinctSenderAmount numeric(18,2);
END

-- Core.PricingCommercial.CommissionRate
IF EXISTS (SELECT 1 FROM sys.columns c JOIN sys.tables t ON c.object_id = t.object_id JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE s.name = 'Core' AND t.name = 'PricingCommercial' AND c.name = 'CommissionRate' AND (c.precision <> 18 OR c.scale <> 2))
BEGIN
    ALTER TABLE Core.PricingCommercial ALTER COLUMN CommissionRate numeric(18,2);
END

-- Core.OnusPaymentRequest.Amount
IF EXISTS (SELECT 1 FROM sys.columns c JOIN sys.tables t ON c.object_id = t.object_id JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE s.name = 'Core' AND t.name = 'OnusPaymentRequest' AND c.name = 'Amount' AND (c.precision <> 18 OR c.scale <> 2))
BEGIN
    ALTER TABLE Core.OnusPaymentRequest ALTER COLUMN Amount numeric(18,2);
END

-- Core.Chargeback.Amount
IF EXISTS (SELECT 1 FROM sys.columns c JOIN sys.tables t ON c.object_id = t.object_id JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE s.name = 'Core' AND t.name = 'Chargeback' AND c.name = 'Amount' AND (c.precision <> 18 OR c.scale <> 2))
BEGIN
    ALTER TABLE Core.Chargeback ALTER COLUMN Amount numeric(18,2);
END

-- Core.CardTopupRequest.Fee
IF EXISTS (SELECT 1 FROM sys.columns c JOIN sys.tables t ON c.object_id = t.object_id JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE s.name = 'Core' AND t.name = 'CardTopupRequest' AND c.name = 'Fee' AND (c.precision <> 18 OR c.scale <> 2))
BEGIN
    ALTER TABLE Core.CardTopupRequest ALTER COLUMN Fee numeric(18,2);
END

-- Core.CardTopupRequest.CommissionTotal
IF EXISTS (SELECT 1 FROM sys.columns c JOIN sys.tables t ON c.object_id = t.object_id JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE s.name = 'Core' AND t.name = 'CardTopupRequest' AND c.name = 'CommissionTotal' AND (c.precision <> 18 OR c.scale <> 2))
BEGIN
    ALTER TABLE Core.CardTopupRequest ALTER COLUMN CommissionTotal numeric(18,2);
END

-- Core.CardTopupRequest.BsmvTotal
IF EXISTS (SELECT 1 FROM sys.columns c JOIN sys.tables t ON c.object_id = t.object_id JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE s.name = 'Core' AND t.name = 'CardTopupRequest' AND c.name = 'BsmvTotal' AND (c.precision <> 18 OR c.scale <> 2))
BEGIN
    ALTER TABLE Core.CardTopupRequest ALTER COLUMN BsmvTotal numeric(18,2);
END

-- Core.CardTopupRequest.Amount
IF EXISTS (SELECT 1 FROM sys.columns c JOIN sys.tables t ON c.object_id = t.object_id JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE s.name = 'Core' AND t.name = 'CardTopupRequest' AND c.name = 'Amount' AND (c.precision <> 18 OR c.scale <> 2))
BEGIN
    ALTER TABLE Core.CardTopupRequest ALTER COLUMN Amount numeric(18,2);
END

-- limit.AccountCurrentLevel.MonthlyWithdrawalAmount
IF EXISTS (SELECT 1 FROM sys.columns c JOIN sys.tables t ON c.object_id = t.object_id JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE s.name = 'limit' AND t.name = 'AccountCurrentLevel' AND c.name = 'MonthlyWithdrawalAmount' AND (c.precision <> 18 OR c.scale <> 2))
BEGIN
    ALTER TABLE [Limit].AccountCurrentLevel ALTER COLUMN MonthlyWithdrawalAmount numeric(18,2);
END

-- limit.AccountCurrentLevel.MonthlyOtherIbanWithdrawalAmount
IF EXISTS (SELECT 1 FROM sys.columns c JOIN sys.tables t ON c.object_id = t.object_id JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE s.name = 'limit' AND t.name = 'AccountCurrentLevel' AND c.name = 'MonthlyOtherIbanWithdrawalAmount' AND (c.precision <> 18 OR c.scale <> 2))
BEGIN
    ALTER TABLE [Limit].AccountCurrentLevel ALTER COLUMN MonthlyOtherIbanWithdrawalAmount numeric(18,2);
END

-- limit.AccountCurrentLevel.MonthlyOnUsPaymentAmount
IF EXISTS (SELECT 1 FROM sys.columns c JOIN sys.tables t ON c.object_id = t.object_id JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE s.name = 'limit' AND t.name = 'AccountCurrentLevel' AND c.name = 'MonthlyOnUsPaymentAmount' AND (c.precision <> 18 OR c.scale <> 2))
BEGIN
    ALTER TABLE [Limit].AccountCurrentLevel ALTER COLUMN MonthlyOnUsPaymentAmount numeric(18,2);
END

-- limit.AccountCurrentLevel.MonthlyInternationalTransferAmount
IF EXISTS (SELECT 1 FROM sys.columns c JOIN sys.tables t ON c.object_id = t.object_id JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE s.name = 'limit' AND t.name = 'AccountCurrentLevel' AND c.name = 'MonthlyInternationalTransferAmount' AND (c.precision <> 18 OR c.scale <> 2))
BEGIN
    ALTER TABLE [Limit].AccountCurrentLevel ALTER COLUMN MonthlyInternationalTransferAmount numeric(18,2);
END

-- limit.AccountCurrentLevel.MonthlyInternalTransferAmount
IF EXISTS (SELECT 1 FROM sys.columns c JOIN sys.tables t ON c.object_id = t.object_id JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE s.name = 'limit' AND t.name = 'AccountCurrentLevel' AND c.name = 'MonthlyInternalTransferAmount' AND (c.precision <> 18 OR c.scale <> 2))
BEGIN
    ALTER TABLE [Limit].AccountCurrentLevel ALTER COLUMN MonthlyInternalTransferAmount numeric(18,2);
END

-- limit.AccountCurrentLevel.MonthlyDepositAmount
IF EXISTS (SELECT 1 FROM sys.columns c JOIN sys.tables t ON c.object_id = t.object_id JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE s.name = 'limit' AND t.name = 'AccountCurrentLevel' AND c.name = 'MonthlyDepositAmount' AND (c.precision <> 18 OR c.scale <> 2))
BEGIN
    ALTER TABLE [Limit].AccountCurrentLevel ALTER COLUMN MonthlyDepositAmount numeric(18,2);
END

-- limit.AccountCurrentLevel.DailyWithdrawalAmount
IF EXISTS (SELECT 1 FROM sys.columns c JOIN sys.tables t ON c.object_id = t.object_id JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE s.name = 'limit' AND t.name = 'AccountCurrentLevel' AND c.name = 'DailyWithdrawalAmount' AND (c.precision <> 18 OR c.scale <> 2))
BEGIN
    ALTER TABLE [Limit].AccountCurrentLevel ALTER COLUMN DailyWithdrawalAmount numeric(18,2);
END

-- limit.AccountCurrentLevel.DailyOtherIbanWithdrawalAmount
IF EXISTS (SELECT 1 FROM sys.columns c JOIN sys.tables t ON c.object_id = t.object_id JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE s.name = 'limit' AND t.name = 'AccountCurrentLevel' AND c.name = 'DailyOtherIbanWithdrawalAmount' AND (c.precision <> 18 OR c.scale <> 2))
BEGIN
    ALTER TABLE [Limit].AccountCurrentLevel ALTER COLUMN DailyOtherIbanWithdrawalAmount numeric(18,2);
END

-- limit.AccountCurrentLevel.DailyOnUsPaymentAmount
IF EXISTS (SELECT 1 FROM sys.columns c JOIN sys.tables t ON c.object_id = t.object_id JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE s.name = 'limit' AND t.name = 'AccountCurrentLevel' AND c.name = 'DailyOnUsPaymentAmount' AND (c.precision <> 18 OR c.scale <> 2))
BEGIN
    ALTER TABLE [Limit].AccountCurrentLevel ALTER COLUMN DailyOnUsPaymentAmount numeric(18,2);
END

-- limit.AccountCurrentLevel.DailyInternationalTransferAmount
IF EXISTS (SELECT 1 FROM sys.columns c JOIN sys.tables t ON c.object_id = t.object_id JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE s.name = 'limit' AND t.name = 'AccountCurrentLevel' AND c.name = 'DailyInternationalTransferAmount' AND (c.precision <> 18 OR c.scale <> 2))
BEGIN
    ALTER TABLE [Limit].AccountCurrentLevel ALTER COLUMN DailyInternationalTransferAmount numeric(18,2);
END

-- limit.AccountCurrentLevel.DailyInternalTransferAmount
IF EXISTS (SELECT 1 FROM sys.columns c JOIN sys.tables t ON c.object_id = t.object_id JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE s.name = 'limit' AND t.name = 'AccountCurrentLevel' AND c.name = 'DailyInternalTransferAmount' AND (c.precision <> 18 OR c.scale <> 2))
BEGIN
    ALTER TABLE [Limit].AccountCurrentLevel ALTER COLUMN DailyInternalTransferAmount numeric(18,2);
END

-- limit.AccountCurrentLevel.DailyDepositAmount
IF EXISTS (SELECT 1 FROM sys.columns c JOIN sys.tables t ON c.object_id = t.object_id JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE s.name = 'limit' AND t.name = 'AccountCurrentLevel' AND c.name = 'DailyDepositAmount' AND (c.precision <> 18 OR c.scale <> 2))
BEGIN
    ALTER TABLE [Limit].AccountCurrentLevel ALTER COLUMN DailyDepositAmount numeric(18,2);
END

-- Core.AccountActivity.Amount
IF EXISTS (SELECT 1 FROM sys.columns c JOIN sys.tables t ON c.object_id = t.object_id JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE s.name = 'Core' AND t.name = 'AccountActivity' AND c.name = 'Amount' AND (c.precision <> 18 OR c.scale <> 2))
BEGIN
    ALTER TABLE Core.AccountActivity ALTER COLUMN Amount numeric(18,2);
END

-- Core.ReturnTransactionRequest.Amount
IF EXISTS (SELECT 1 FROM sys.columns c JOIN sys.tables t ON c.object_id = t.object_id JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE s.name = 'Core' AND t.name = 'ReturnTransactionRequest' AND c.name = 'Amount' AND (c.precision <> 18 OR c.scale <> 2))
BEGIN
    ALTER TABLE Core.ReturnTransactionRequest ALTER COLUMN Amount numeric(18,2);
END

-- Core.WalletPaymentRequest.Amount
IF EXISTS (SELECT 1 FROM sys.columns c JOIN sys.tables t ON c.object_id = t.object_id JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE s.name = 'Core' AND t.name = 'WalletPaymentRequest' AND c.name = 'Amount' AND (c.precision <> 18 OR c.scale <> 2))
BEGIN
    ALTER TABLE Core.WalletPaymentRequest ALTER COLUMN Amount numeric(18,2);
END
