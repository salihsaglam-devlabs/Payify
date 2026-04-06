namespace LinkPara.PF.Domain.Enums;

public enum OrderStatus
{
    Unknown,
    Rejected,
    Cancelled,
    PreAuth,
    PostAuth,
    WaitingEndOfDay,
    EndOfDayCompleted,
    Refunded,
    SaleRefund,
    OrderNotFound
}