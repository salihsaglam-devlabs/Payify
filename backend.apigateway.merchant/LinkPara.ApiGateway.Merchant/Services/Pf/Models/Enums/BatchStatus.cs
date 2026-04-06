namespace LinkPara.ApiGateway.Merchant.Services.Pf.Models.Enums;

public enum BatchStatus
{
    Pending,
    Queued,
    Error,
    Completed,
    DeductionCalculated,
    DeductionCalculationError,
    MoneyTransferError,
    PendingAccounting,
    ParentMerchantCommissionCalculated,
    EodPending,
    MoneyTransferProcessing,
    MoneyTransferPostponed
}
