namespace LinkPara.Emoney.Domain.Enums;

public enum BulkTransferStatus
{
    Waiting,
    Rejected,
    WaitingMoneyTransfer,
    Success,
    Failed,
    PartialFailed,
    Processing
}
