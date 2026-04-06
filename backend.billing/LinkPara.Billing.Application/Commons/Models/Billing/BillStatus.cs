namespace LinkPara.Billing.Application.Commons.Models.Billing;

public enum BillStatus
{
    PaymentCancelled,
    Paid,
    AwaitingCancelConfirmation,
    AwaitingPaymentConfirmation,
    NotFound,
    AccountingFinished,
    Notified,
    ReconciliationCancelled
}