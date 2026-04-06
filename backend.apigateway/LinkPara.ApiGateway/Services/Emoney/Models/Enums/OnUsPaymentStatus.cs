namespace LinkPara.ApiGateway.Services.Emoney.Models.Enums;

public enum OnUsPaymentStatus
{
    Pending,
    Success,
    Failed,
    Expired,
    Rejected,
    Suspecious,
    Chargeback,
    Returned,
    PartiallyReturned,
    Canceled
}