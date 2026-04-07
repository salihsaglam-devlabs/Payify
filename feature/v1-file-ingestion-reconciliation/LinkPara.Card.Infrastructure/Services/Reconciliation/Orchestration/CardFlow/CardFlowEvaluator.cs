using LinkPara.Card.Application.Commons.Constants;
using LinkPara.Card.Domain.Constants;
using LinkPara.Card.Domain.Entities.FileIngestion;
using LinkPara.Card.Domain.Enums;

namespace LinkPara.Card.Infrastructure.Services.Reconciliation.Orchestration.CardFlow;

internal static class CardFlowEvaluator
{
    public static CardFlowResult EvaluateByProvider(
        CardTransactionRecord card,
        PrepaidLookupInfo prepaidLookup,
        bool hasExpireControlStatP,
        ClearingProvider? provider)
    {
        var resolvedProvider = provider.GetValueOrDefault(ClearingProvider.Unknown);
        return resolvedProvider switch
        {
            ClearingProvider.Bkm => EvaluateBkmCard(card, prepaidLookup, hasExpireControlStatP),
            ClearingProvider.Visa => EvaluateVisaCard(card, prepaidLookup, hasExpireControlStatP),
            ClearingProvider.Mastercard => EvaluateMasterCard(card, prepaidLookup, hasExpireControlStatP),
            _ => BuildResult(card)
        };
    }

    public static CardFlowResult EvaluateBkmCard(
        CardTransactionRecord card,
        PrepaidLookupInfo prepaidLookup,
        bool hasExpireControlStatP)
    {
        if (!BkmCardFlowBusinessRules.IsAllowedTxnSource(card.TxnSource))
        {
            return BuildResult(card);
        }

        var txnStat = CardFlowText.Normalize(card.TxnStat);
        if (txnStat is ReconciliationFieldValues.TxnStatReversal or ReconciliationFieldValues.TxnStatVoid)
        {
            return prepaidLookup.IsCancelled
                ? BuildResult(card)
                : BuildResult(card,
                    Select(ocs => ocs.MarkOriginalCancelled, oms => oms.Auto, ors => ors.ReversalOrVoidNotCancelled),
                    Select(ocs => ocs.ApplyReversalOperation, oms => oms.Auto, ors => ors.ReversalOrVoidNotCancelled));
        }

        var responseCode = CardFlowText.Normalize(card.ResponseCode);
        var isSuccessful = CardFlowText.Normalize(card.IsSuccessfulTxn);
        var isSettle = CardFlowText.Normalize(card.IsTxnSettle);

        var prepaidStatus = prepaidLookup.Status;
        if (prepaidStatus == PrepaidTransactionStatus.Unknown)
        {
            return BuildResult(card);
        }

        var isCardExpire = txnStat == ReconciliationFieldValues.TxnStatExpire;
        var isCardFailed = responseCode != ReconciliationFieldValues.ResponseCodeApproved
                           && isSuccessful == ReconciliationFieldValues.FlagNo;
        var isCardSuccessful = responseCode == ReconciliationFieldValues.ResponseCodeApproved
                               && isSuccessful == ReconciliationFieldValues.FlagYes;

        if (isCardExpire)
        {
            if (prepaidStatus == PrepaidTransactionStatus.Failed)
            {
                return BuildResult(card,
                    Select(ocs => ocs.SetExpireStatus, oms => oms.Auto, ors => ors.ExpirePrepaidFailed));
            }

            if (prepaidStatus == PrepaidTransactionStatus.Missing)
            {
                return BuildResult(card,
                    Select(ocs => ocs.CreateTransaction, oms => oms.Auto, ors => ors.ExpirePrepaidMissing),
                    Select(ocs => ocs.SetExpireStatus, oms => oms.Auto, ors => ors.ExpirePrepaidMissing));
            }

            if (hasExpireControlStatP)
            {
                return BuildResult(card,
                    Select(ocs => ocs.QueueBalanceFixList, oms => oms.Auto, ors => ors.ExpireControlStatPMatch));
            }

            return BuildResult(card,
                Select(ocs => ocs.SetExpireStatus, oms => oms.Auto, ors => ors.ExpirePrepaidSuccessfulNoAccP),
                Select(ocs => ocs.ReverseBalanceOperation, oms => oms.Auto, ors => ors.ExpirePrepaidSuccessfulNoAccP));
        }

        if (isCardFailed)
        {
            return prepaidStatus == PrepaidTransactionStatus.Successful
                ? BuildResult(card,
                    Select(ocs => ocs.AdjustResponseCode, oms => oms.Auto, ors => ors.CardFailedPrepaidSuccessful),
                    Select(ocs => ocs.ReverseBalanceOperation, oms => oms.Auto, ors => ors.CardFailedPrepaidSuccessful))
                : BuildResult(card);
        }

        if (!isCardSuccessful)
        {
            return BuildResult(card);
        }

        if (isSettle != ReconciliationFieldValues.FlagYes)
        {
            return BuildResult(card);
        }

        if (prepaidStatus == PrepaidTransactionStatus.Failed)
        {
            return BuildResult(card,
                Select(ocs => ocs.AdjustResponseCode, oms => oms.Auto, ors => ors.CardSuccessfulPrepaidFailed),
                Select(ocs => ocs.ReverseBalanceOperation, oms => oms.Auto, ors => ors.CardSuccessfulPrepaidFailed));
        }

        if (prepaidStatus == PrepaidTransactionStatus.Missing)
        {
            if (!BkmCardFlowBusinessRules.IsRefundTransaction(card))
            {
                return BuildResult(card,
                    Select(ocs => ocs.CreateTransaction, oms => oms.Auto, ors => ors.CardSuccessfulPrepaidMissingNonRefund),
                    Select(ocs => ocs.ApplyRefundToOriginal, oms => oms.Auto, ors => ors.CardSuccessfulPrepaidMissingNonRefund));
            }

            if (!BkmCardFlowBusinessRules.HasLinkedMainTxnGuid(card.OceanMainTxnGuid))
            {
                return BuildResult(card,
                    Select(ocs => ocs.ApplyManualRefundIfApproved, oms => oms.Manual, ors => ors.RefundUnlinkedManualReview));
            }

            return BuildResult(card,
                Select(ocs => ocs.ApplyRefundToOriginal, oms => oms.Auto, ors => ors.RefundLinkedAuto));
        }

        if (BkmCardFlowBusinessRules.IsRefundTransaction(card))
        {
            if (!BkmCardFlowBusinessRules.HasLinkedMainTxnGuid(card.OceanMainTxnGuid))
            {
                return BuildResult(card,
                    Select(ocs => ocs.ApplyManualRefundIfApproved, oms => oms.Manual, ors => ors.RefundUnlinkedManualReview));
            }

            return BuildResult(card,
                Select(ocs => ocs.ApplyRefundToOriginal, oms => oms.Auto, ors => ors.RefundLinkedAuto));
        }

        if (BkmCardFlowBusinessRules.AreAmountsEqual(prepaidLookup.Amount, card.BillingAmount, card.CardHolderBillingAmount))
        {
            return BuildResult(card);
        }

        if (BkmCardFlowBusinessRules.IsPrepaidAmountLessThanBilling(prepaidLookup.Amount, card.BillingAmount, card.CardHolderBillingAmount))
        {
            return BuildResult(card,
                Select(ocs => ocs.ApplyShadowBalanceOperation, oms => oms.Auto, ors => ors.AmountMismatchShadowBalance));
        }

        return BuildResult(card);
    }

    private static CardFlowResult EvaluateVisaCard(
        CardTransactionRecord card,
        PrepaidLookupInfo prepaidLookup,
        bool hasExpireControlStatP)
    {
        if (!VisaCardFlowBusinessRules.IsImplemented(prepaidLookup, hasExpireControlStatP))
        {
            return BuildResult(card);
        }

        return BuildResult(card);
    }

    private static CardFlowResult EvaluateMasterCard(
        CardTransactionRecord card,
        PrepaidLookupInfo prepaidLookup,
        bool hasExpireControlStatP)
    {
        if (!MasterCardFlowBusinessRules.IsImplemented(prepaidLookup, hasExpireControlStatP))
        {
            return BuildResult(card);
        }

        return BuildResult(card);
    }

    private static CardFlowResult BuildResult(
        CardTransactionRecord card,
        params FlowOperationSelection[] operations)
    {
        var mappedOperations = operations
            .Select((x, index) => new FlowOperation
            {
                Code = x.OperationCode,
                Mode = x.OperationMode,
                Index = index + 1,
                ReasonCode = OperationReasonSelector.ResolveCode(x.OperationReason),
                ReasonText = ResolveReasonText(x)
            })
            .ToArray();

        return new CardFlowResult
        {
            CardTransactionRecordId = card.Id,
            ClearingRecordId = null,
            Operations = mappedOperations
        };
    }

    private static string ResolveReasonText(FlowOperationSelection selection)
    {
        if (selection.OperationReason == ReconciliationOperationReason.None)
        {
            return string.Empty;
        }

        return OperationReasonSelector.ResolveText(selection.OperationReason);
    }

    private static FlowOperationSelection Select(
        Func<OperationCodeSelector, ReconciliationOperationCode> ocs,
        Func<OperationModeSelector, ReconciliationOperationMode> oms,
        Func<OperationReasonSelector, ReconciliationOperationReason> ors)
    {
        return new FlowOperationSelection(
            ocs(OperationCodeSelector.Instance),
            oms(OperationModeSelector.Instance),
            ors(OperationReasonSelector.Instance));
    }

    private readonly record struct FlowOperationSelection(
        ReconciliationOperationCode OperationCode,
        ReconciliationOperationMode OperationMode,
        ReconciliationOperationReason OperationReason);

    private readonly struct OperationCodeSelector
    {
        public static OperationCodeSelector Instance => default;
        public ReconciliationOperationCode MarkOriginalCancelled => ReconciliationOperationCode.MarkOriginalCancelled;
        public ReconciliationOperationCode ApplyReversalOperation => ReconciliationOperationCode.ApplyReversalOperation;
        public ReconciliationOperationCode SetExpireStatus => ReconciliationOperationCode.SetExpireStatus;
        public ReconciliationOperationCode CreateTransaction => ReconciliationOperationCode.CreateTransaction;
        public ReconciliationOperationCode ReverseBalanceOperation => ReconciliationOperationCode.ReverseBalanceOperation;
        public ReconciliationOperationCode AdjustResponseCode => ReconciliationOperationCode.AdjustResponseCode;
        public ReconciliationOperationCode ApplyRefundToOriginal => ReconciliationOperationCode.ApplyRefundToOriginal;
        public ReconciliationOperationCode ApplyManualRefundIfApproved => ReconciliationOperationCode.ApplyManualRefundIfApproved;
        public ReconciliationOperationCode ApplyShadowBalanceOperation => ReconciliationOperationCode.ApplyShadowBalanceOperation;
        public ReconciliationOperationCode QueueBalanceFixList => ReconciliationOperationCode.QueueBalanceFixList;
    }

    private readonly struct OperationModeSelector
    {
        public static OperationModeSelector Instance => default;
        public ReconciliationOperationMode Auto => ReconciliationOperationMode.Auto;
        public ReconciliationOperationMode Manual => ReconciliationOperationMode.Manual;
    }

    private readonly struct OperationReasonSelector
    {
        public static OperationReasonSelector Instance => default;
        public ReconciliationOperationReason ReversalOrVoidNotCancelled => ReconciliationOperationReason.ReversalOrVoidNotCancelled;
        public ReconciliationOperationReason ExpirePrepaidFailed => ReconciliationOperationReason.ExpirePrepaidFailed;
        public ReconciliationOperationReason ExpirePrepaidMissing => ReconciliationOperationReason.ExpirePrepaidMissing;
        public ReconciliationOperationReason ExpirePrepaidSuccessfulNoAccP => ReconciliationOperationReason.ExpirePrepaidSuccessfulNoAccP;
        public ReconciliationOperationReason CardFailedPrepaidSuccessful => ReconciliationOperationReason.CardFailedPrepaidSuccessful;
        public ReconciliationOperationReason CardSuccessfulPrepaidFailed => ReconciliationOperationReason.CardSuccessfulPrepaidFailed;
        public ReconciliationOperationReason CardSuccessfulPrepaidMissingNonRefund => ReconciliationOperationReason.CardSuccessfulPrepaidMissingNonRefund;
        public ReconciliationOperationReason RefundUnlinkedManualReview => ReconciliationOperationReason.RefundUnlinkedManualReview;
        public ReconciliationOperationReason RefundLinkedAuto => ReconciliationOperationReason.RefundLinkedAuto;
        public ReconciliationOperationReason AmountMismatchShadowBalance => ReconciliationOperationReason.AmountMismatchShadowBalance;
        public ReconciliationOperationReason ExpireControlStatPMatch => ReconciliationOperationReason.ExpireControlStatPMatch;

        public static string ResolveCode(ReconciliationOperationReason reason)
        {
            return reason switch
            {
                ReconciliationOperationReason.ReversalOrVoidNotCancelled => ReconciliationFlowReasonCodes.ReversalOrVoidNotCancelled,
                ReconciliationOperationReason.ExpirePrepaidFailed => ReconciliationFlowReasonCodes.ExpirePrepaidFailed,
                ReconciliationOperationReason.ExpirePrepaidMissing => ReconciliationFlowReasonCodes.ExpirePrepaidMissing,
                ReconciliationOperationReason.ExpirePrepaidSuccessfulNoAccP => ReconciliationFlowReasonCodes.ExpirePrepaidSuccessfulNoAccP,
                ReconciliationOperationReason.CardFailedPrepaidSuccessful => ReconciliationFlowReasonCodes.CardFailedPrepaidSuccessful,
                ReconciliationOperationReason.CardSuccessfulPrepaidFailed => ReconciliationFlowReasonCodes.CardSuccessfulPrepaidFailed,
                ReconciliationOperationReason.CardSuccessfulPrepaidMissingNonRefund => ReconciliationFlowReasonCodes.CardSuccessfulPrepaidMissingNonRefund,
                ReconciliationOperationReason.RefundUnlinkedManualReview => ReconciliationFlowReasonCodes.RefundUnlinkedManualReview,
                ReconciliationOperationReason.RefundLinkedAuto => ReconciliationFlowReasonCodes.RefundLinkedAuto,
                ReconciliationOperationReason.AmountMismatchShadowBalance => ReconciliationFlowReasonCodes.AmountMismatchShadowBalance,
                ReconciliationOperationReason.ExpireControlStatPMatch => ReconciliationFlowReasonCodes.ExpireControlStatPMatch,
                _ => string.Empty
            };
        }

        public static string ResolveText(ReconciliationOperationReason reason)
        {
            return reason switch
            {
                ReconciliationOperationReason.ReversalOrVoidNotCancelled => "Reversal/void requires cancellation workflow.",
                ReconciliationOperationReason.ExpirePrepaidFailed => "Card expired and prepaid failed.",
                ReconciliationOperationReason.ExpirePrepaidMissing => "Card expired and prepaid missing.",
                ReconciliationOperationReason.ExpirePrepaidSuccessfulNoAccP => "Card expired and prepaid successful without ACC P.",
                ReconciliationOperationReason.CardFailedPrepaidSuccessful => "Card failed and prepaid successful.",
                ReconciliationOperationReason.CardSuccessfulPrepaidFailed => "Card successful and prepaid failed.",
                ReconciliationOperationReason.CardSuccessfulPrepaidMissingNonRefund => "Card successful and prepaid missing for non-refund.",
                ReconciliationOperationReason.RefundUnlinkedManualReview => "Refund is unlinked and requires manual review.",
                ReconciliationOperationReason.RefundLinkedAuto => "Refund is linked and can be processed automatically.",
                ReconciliationOperationReason.AmountMismatchShadowBalance => "Amount mismatch requires shadow balance operation.",
                ReconciliationOperationReason.ExpireControlStatPMatch => "ACC ControlStat=P match; add to balance-fix list.",
                _ => string.Empty
            };
        }
    }
}

internal static class BkmCardFlowBusinessRules
{
    public static bool IsAllowedTxnSource(string txnSource)
    {
        var normalized = CardFlowText.Normalize(txnSource);
        return normalized == CardLookupCodes.TxnSource.Domestic
               || normalized == CardLookupCodes.TxnSource.Onus;
    }

    public static bool IsRefundTransaction(CardTransactionRecord card)
    {
        var bankingTxnCode = CardFlowText.Normalize(card.BankingTxnCode);
        if (bankingTxnCode is ReconciliationFlowValues.Refund or ReconciliationFlowValues.ReferenceRefund)
        {
            return true;
        }

        return CardFlowText.Normalize(card.TxnEffect) == CardLookupCodes.TxnEffect.Refund;
    }

    public static bool HasLinkedMainTxnGuid(string oceanMainTxnGuid)
    {
        var normalized = CardFlowText.Normalize(oceanMainTxnGuid);
        return !string.IsNullOrWhiteSpace(normalized) && normalized != ReconciliationFlowValues.Zero;
    }

    public static bool AreAmountsEqual(decimal? prepaidAmount, decimal? billingAmount, decimal? cardHolderBillingAmount)
    {
        if (!prepaidAmount.HasValue)
        {
            return false;
        }

        var candidate = billingAmount.HasValue ? billingAmount : cardHolderBillingAmount;
        if (!candidate.HasValue)
        {
            return false;
        }

        return Math.Abs(prepaidAmount.Value - candidate.Value) < 0.01m;
    }

    public static bool IsPrepaidAmountLessThanBilling(decimal? prepaidAmount, decimal? billingAmount, decimal? cardHolderBillingAmount)
    {
        if (!prepaidAmount.HasValue)
        {
            return false;
        }

        var candidate = billingAmount.HasValue ? billingAmount : cardHolderBillingAmount;
        if (!candidate.HasValue)
        {
            return false;
        }

        return prepaidAmount.Value < candidate.Value;
    }
}

internal static class VisaCardFlowBusinessRules
{
    public static bool IsImplemented(PrepaidLookupInfo prepaidLookup, bool hasExpireControlStatP)
    {
        _ = prepaidLookup;
        _ = hasExpireControlStatP;
        return false;
    }
}

internal static class MasterCardFlowBusinessRules
{
    public static bool IsImplemented(PrepaidLookupInfo prepaidLookup, bool hasExpireControlStatP)
    {
        _ = prepaidLookup;
        _ = hasExpireControlStatP;
        return false;
    }
}

internal static class CardFlowText
{
    public static string Normalize(string value)
    {
        return (value ?? string.Empty).Trim().ToUpperInvariant();
    }
}
