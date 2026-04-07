namespace LinkPara.Card.Domain.Enums;

public enum ReconciliationOperationReason
{
    None = 0,
    ReversalOrVoidNotCancelled = 1,
    ExpirePrepaidFailed = 2,
    ExpirePrepaidMissing = 3,
    ExpirePrepaidSuccessfulNoAccP = 4,
    CardFailedPrepaidSuccessful = 5,
    CardSuccessfulPrepaidFailed = 6,
    CardSuccessfulPrepaidMissingNonRefund = 7,
    RefundUnlinkedManualReview = 8,
    RefundLinkedAuto = 9,
    AmountMismatchShadowBalance = 10,
    ExpireControlStatPMatch = 11
}
