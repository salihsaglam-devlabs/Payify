namespace LinkPara.ApiGateway.Merchant.Services.Pf.Models.Enums;

public enum TransactionStatus
{
    Pending,
    Fail,
    Success,
    Returned,
    PartiallyReturned,
    Reversed,
    Closed,
    PartiallyClosed,
}