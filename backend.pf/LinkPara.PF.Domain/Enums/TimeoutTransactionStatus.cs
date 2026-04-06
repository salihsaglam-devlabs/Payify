namespace LinkPara.PF.Domain.Enums;

public enum TimeoutTransactionStatus
{
    Pending,
    Queued,
    Fail,
    NoActionNeeded,
    Canceled,
    Refunded,
    CancelFail
}