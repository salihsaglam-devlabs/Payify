namespace LinkPara.PF.Domain.Enums;

public enum PostingMoneyTransferStatus
{
    Pending,
    PaymentInitiated,
    PaymentDelivered,
    PaymentSucceeded,
    PaymentError,
    PaymentReturned,
    PaymentWaiting,
    PaymentNotRequired,
    PaymentBlocked
}