namespace LinkPara.Card.Domain.Enums;

public enum CardReconciliationState
{
    AwaitingReevaluation = 0,
    ReadyForReconcile = 1,
    ManualReviewRequired = 2,
    ReconcileCompleted = 3,
    ReconcileFailed = 4
}
