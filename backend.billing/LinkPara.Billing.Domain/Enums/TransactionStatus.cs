namespace LinkPara.Billing.Domain.Enums;

public enum TransactionStatus
{
    New,
    Paid,
    Cancelled,
    Error,
    Poison,
    Timeout,
    ProvisionError,
    AccountingCancelled,
    NotFound,
    AccountingFinished,
    Notified,
    ReconciliationCancelled
}