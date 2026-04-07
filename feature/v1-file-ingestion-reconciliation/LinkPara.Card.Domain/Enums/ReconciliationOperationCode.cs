namespace LinkPara.Card.Domain.Enums;

public enum ReconciliationOperationCode
{
    AdjustResponseCode = 1,
    ReverseBalanceOperation = 2,
    SetExpireStatus = 3,
    CreateTransaction = 4,
    ApplyRefundToOriginal = 5,
    ApplyManualRefundIfApproved = 6,
    MarkOriginalCancelled = 7,
    ApplyReversalOperation = 8,
    ApplyShadowBalanceOperation = 9,
    QueueBalanceFixList = 10
}
