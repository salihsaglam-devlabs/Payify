namespace LinkPara.Emoney.Domain.Enums;

public enum PfTransactionStatus
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
