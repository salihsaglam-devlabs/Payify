namespace LinkPara.SharedModels.Banking.Enums;

public enum IncomingTransactionStatus
{
    Pending,
    NotDelivered,
    Completed,
    ActionRequired,
    WaitingReturnToBank,
    ReturnToBankCompleted,
    ReturnToBankFailed,
    ManualPayment,
}
