namespace LinkPara.Card.Application.Commons.Constants;

public static class ReconciliationTextValues
{
    public const string PendingManualDecision = "Pending manual decision";
}

public static class ReconciliationFieldValues
{
    public const string ResponseCodeApproved = "00";
    public const string ResponseCodeDeclined = "05";
    public const string FlagYes = "Y";
    public const string FlagNo = "N";

    public const string TxnStatReversal = "R";
    public const string TxnStatVoid = "V";
    public const string TxnStatExpire = "E";

    public const string ClearingControlStatNormal = "N";
    public const string ClearingControlStatPending = "P";
    public const string ClearingControlStatProblemToNormal = "X";
    public const string TxnEffectRefund = "R";

    public const string EMoneyStateFailed = "FAILED";
    public const string EMoneyStateReject = "REJECT";
    public const string EMoneyStateRejected = "REJECTED";
    public const string EMoneyStateDeclined = "DECLINED";
}

public static class ReconciliationDecisionCodes
{
    public const string ActionPlanCreated = "ACTION_PLAN_CREATED";
    public const string WaitingReevaluation = "WAITING_REEVALUATION";
    public const string WaitingClearing = "WAITING_CLEARING";
    public const string NoActionRequired = "NO_ACTION_REQUIRED";
    public const string DuplicateConflictingSignature = "DUPLICATE_CONFLICTING_SIGNATURE";
    public const string DuplicateSameSignature = "DUPLICATE_SAME_SIGNATURE";
}

public static class ReconciliationStateReasons
{
    public const string NoOperationPlanned = "NO_OPERATION_PLANNED";
    public const string AutoOperationPlanned = "AUTO_OPERATION_PLANNED";
    public const string ManualOperationPlanned = "MANUAL_OPERATION_PLANNED";
    public const string ManualReviewPending = "MANUAL_REVIEW_PENDING";
    public const string NoOperationsFound = "NO_OPERATIONS_FOUND";
    public const string RunNotFinished = "RUN_NOT_FINISHED";
    public const string RunFailed = "RUN_FAILED";
    public const string RunRejected = "RUN_REJECTED";
    public const string RunDone = "RUN_DONE";
    public const string NewCardIngested = "NEW_CARD_INGESTED";
    public const string ClearingArrived = "CLEARING_ARRIVED";
}

public static class ReconciliationAlarmCodes
{
    public const string ReconciliationManualReview = "RECON_MANUAL_REVIEW";
    public const string ReconciliationManualRejected = "RECON_MANUAL_REJECTED";
    public const string ReconciliationDuplicateConflictingSignature = "RECON_DUPLICATE_CONFLICTING_SIGNATURE";
    public const string ReconciliationDuplicateSameSignature = "RECON_DUPLICATE_SAME_SIGNATURE";
}

public static class ReconciliationErrorCodes
{
    public const string ObsoleteByReevaluation = "OBSOLETE_BY_REEVALUATION";
    public const string ManualRejected = "MANUAL_REJECTED";
    public const string UpstreamRejected = "UPSTREAM_REJECTED";
    public const string UpstreamFailed = "UPSTREAM_FAILED";
    public const string AlreadyAppliedEffect = "ALREADY_APPLIED_EFFECT";
    public const string HandlerNotFound = "HANDLER_NOT_FOUND";
    public const string HandlerException = "HANDLER_EXCEPTION";
    public const string HandlerFailed = "HANDLER_FAILED";
    public const string DependencyNotFound = "DEPENDENCY_NOT_FOUND";
}

public static class ReconciliationDerivedFieldKeys
{
    public const string ResponseCodeTransition = "responseCodeTransition";
    public const string ShouldSetExpire = "shouldSetExpire";
    public const string ShouldCreateTransaction = "shouldCreateTransaction";
    public const string ShouldMarkCancelled = "shouldMarkCancelled";
    public const string ReferenceTxnGuid = "referenceTxnGuid";
    public const string BalanceEffectHint = "balanceEffectHint";
    public const string CardBillingAmount = "cardBillingAmount";
    public const string CardBillingCurrency = "cardBillingCurrency";
    public const string CurrentOperationId = "currentOperationId";
    public const string ApprovedManualOperationId = "approvedManualOperationId";
}

public static class ReconciliationDerivedFieldValues
{
    public const string NoChange = "NO_CHANGE";
    public const string SuccessToFailed = "SUCCESS_TO_FAILED";
    public const string FailedToSuccess = "FAILED_TO_SUCCESS";
    public const string RefundEffect = "REFUND_EFFECT";
    public const string OriginalEffect = "ORIGINAL_EFFECT";
}

public static class ReconciliationTransactionDescriptionMarkers
{
    public const string OriginalEffectReversed = "ORIGINAL_EFFECT_REVERSED";
    public const string BalanceEffectReversed = "BALANCE_EFFECT_REVERSED";
    public const string ReversalEffectApplied = "REVERSAL_EFFECT_APPLIED";
    public const string ShadowBalanceRequired = "SHADOW_BALANCE_REQUIRED";
    public const string OriginalCancelledByD7 = "ORIGINAL_CANCELLED_BY_D7";
}

public static class ReconciliationFlowReasonCodes
{
    public const string ReversalOrVoidNotCancelled = "REVERSAL_OR_VOID_NOT_CANCELLED";
    public const string ExpirePrepaidFailed = "EXPIRE_PREPAID_FAILED";
    public const string ExpirePrepaidMissing = "EXPIRE_PREPAID_MISSING";
    public const string ExpirePrepaidSuccessfulNoAccP = "EXPIRE_PREPAID_SUCCESSFUL_NO_ACC_P";
    public const string CardFailedPrepaidSuccessful = "CARD_FAILED_PREPAID_SUCCESSFUL";
    public const string CardSuccessfulPrepaidFailed = "CARD_SUCCESSFUL_PREPAID_FAILED";
    public const string CardSuccessfulPrepaidMissingNonRefund = "CARD_SUCCESSFUL_PREPAID_MISSING_NON_REFUND";
    public const string RefundUnlinkedManualReview = "REFUND_UNLINKED_MANUAL_REVIEW";
    public const string RefundLinkedAuto = "REFUND_LINKED_AUTO";
    public const string AmountMismatchShadowBalance = "AMOUNT_MISMATCH_SHADOW_BALANCE";
    public const string ExpireControlStatPMatch = "EXPIRE_CONTROL_STAT_P_MATCH";
}

public static class ReconciliationFlowValues
{
    public const string Refund = "REFUND";
    public const string ReferenceRefund = "REFERENCEREFUND";
    public const string Zero = "0";
    public const string CancelToken = "CANCEL";
    public const string ReverseToken = "REVERSE";
    public const string VoidToken = "VOID";
}
