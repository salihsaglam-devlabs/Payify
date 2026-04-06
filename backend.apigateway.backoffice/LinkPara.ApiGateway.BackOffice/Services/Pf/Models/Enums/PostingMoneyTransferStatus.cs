namespace LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Enums;

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
