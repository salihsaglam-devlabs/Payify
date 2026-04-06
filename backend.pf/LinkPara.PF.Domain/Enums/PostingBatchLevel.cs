namespace LinkPara.PF.Domain.Enums;

public enum PostingBatchLevel
{
    BatchManager,
    Transfer,
    TransferValidation,
    MerchantBlockage,
    BankBalancer,
    GrandBalancer,
    DeductionBalancer,
    ParentMerchantBalancer,
    DeductionTransfer,
    DeductionCalculation,
    PosBlockageAccounting,
    MoneyTransfer
}