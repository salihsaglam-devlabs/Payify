namespace LinkPara.SharedModels.Banking.Enums;

public enum MoneyTransferStatus
{
    Pending,
    PaymentInitiated,
    PaymentSucceeded,
    PaymentError,
    PaymentTimeout,
    ActionRequired,
    Cancelled,
    ReturnFromBankSucceeded,
    ManualPayment,
}
