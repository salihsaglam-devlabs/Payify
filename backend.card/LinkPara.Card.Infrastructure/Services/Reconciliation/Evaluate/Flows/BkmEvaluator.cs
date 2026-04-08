using System.Globalization;
using System.Text.Json;
using LinkPara.Card.Domain.Entities.FileIngestion;
using LinkPara.Card.Domain.Enums.FileIngestion;
using LinkPara.Card.Infrastructure.Persistence;
using LinkPara.Card.Infrastructure.Services.Reconciliation.Execute;
using LinkPara.Card.Infrastructure.Services.Reconciliation.Integrations.Emoney;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.Card.Infrastructure.Services.Reconciliation.Evaluate;

internal sealed class BkmEvaluator : IEvaluator
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly CardDbContext _dbContext;
    private readonly IEmoneyService _emoneyService;

    public BkmEvaluator(
        CardDbContext dbContext,
        IEmoneyService emoneyService)
    {
        _dbContext = dbContext;
        _emoneyService = emoneyService;
    }

    public bool CanEvaluate(FileContentType fileContentType) => fileContentType == FileContentType.Bkm;

    public async Task<EvaluationResult> EvaluateAsync(
        EvaluationContext context,
        CancellationToken cancellationToken = default)
    {
        var bkmContext = (BkmEvaluationContext)context;
        var result = new EvaluationResult();
        var currentCard = GetRootCardDetail(bkmContext);
        bkmContext.CachedRootDetail = currentCard;

        if (TryHandleMissingCardRow(result, bkmContext, currentCard))
        {
            return result;
        }

        if (currentCard is null)
        {
            throw new InvalidOperationException("Current card row is missing.");
        }

        var detail = currentCard;
        var latestEmoney = GetLatestEmoneyTransaction(bkmContext.EmoneyTransactions);
        if (HasFileLengthValidationFailure(bkmContext))
        {
            result.SetNote(note: "File-level length validation failed. Raise an alert.");
            result.AddAutoOperation(
                OperationCodes.RaiseAlert,
                "File-level length validation failed.",
                BkmSnapshotBuilder.Create(bkmContext, OperationCodes.RaiseAlert, "C1"));
            return result;
        }

        if (TryHandleDuplicateRow(result, bkmContext))
        {
            return result;
        }

        var payifyStatus = ResolvePayifyStatus(bkmContext.EmoneyTransactions);
        if (TryHandleAmbiguousPayifyLookup(result, bkmContext, payifyStatus))
        {
            return result;
        }

        if (IsCancelOrReversal(detail))
        {
            return await EvaluateCancelOrReversalAsync(result, bkmContext, detail, cancellationToken);
        }

        if (IsFailed(detail))
        {
            return EvaluateFailedTransaction(result, bkmContext, payifyStatus);
        }

        if (IsExpired(detail))
        {
            return await EvaluateExpiredTransactionAsync(result, bkmContext, detail, payifyStatus, cancellationToken);
        }

        if (IsSuccessful(detail))
        {
            return EvaluateSuccessfulTransaction(result, bkmContext, detail, payifyStatus, latestEmoney);
        }

        return BuildUnmatchedFlowResult(result, bkmContext);
    }

    private async Task<EvaluationResult> EvaluateCancelOrReversalAsync(
        EvaluationResult result,
        BkmEvaluationContext context,
        CardBkmDetail detail,
        CancellationToken cancellationToken)
    {
        var originalResolution = await ResolveOriginalTransactionAsync(detail, cancellationToken);
        if (originalResolution.Status is OriginalTransactionStatus.Missing or OriginalTransactionStatus.Ambiguous)
        {
            result.SetNote(note: "The original transaction could not be resolved for the cancel/reversal record. Raise an alert and require manual review.");
            result.AddAutoOperation(
                OperationCodes.RaiseAlert,
                "The original transaction could not be resolved for the cancel/reversal record.",
                BkmSnapshotBuilder.Create(context, OperationCodes.RaiseAlert, "C3"));
            result.AddManualOperation(
                OperationCodes.CreateManualReview,
                "The cancel/reversal record is sent to manual review.",
                code => BkmSnapshotBuilder.Create(context, code, "C3"),
                approveCode: OperationCodes.BindOriginalTransactionAndContinue,
                approveNote: "Bind the original transaction and re-evaluate the flow.",
                rejectCode: OperationCodes.RejectReversalRecord,
                rejectNote: "Close the cancel/reversal record manually.");
            return result;
        }

        if (originalResolution.Transaction?.IsCancelled == true)
        {
            result.SetNote(note: "The transaction is already cancelled. No action is required.");
            return result;
        }

        result.SetNote(note: "This is a reversal/void record for a transaction that is not yet cancelled. Cancel the original transaction and create the reverse entry.");
        result.AddAutoOperation(
            OperationCodes.MarkOriginalTransactionCancelled,
            "Mark the original transaction as cancelled.",
            BkmSnapshotBuilder.Create(context, OperationCodes.MarkOriginalTransactionCancelled, "D7", ("originalTransactionId", originalResolution.Transaction?.Id)));
        result.AddAutoOperation(
            OperationCodes.ReverseOriginalTransaction,
            "Reverse the effect of the original transaction.",
            BkmSnapshotBuilder.Create(context, OperationCodes.ReverseOriginalTransaction, "D7", ("originalTransactionId", originalResolution.Transaction?.Id)));
        return result;
    }

    private EvaluationResult EvaluateFailedTransaction(
        EvaluationResult result,
        BkmEvaluationContext context,
        PayifyStatus payifyStatus)
    {
        if (payifyStatus == PayifyStatus.Failed)
        {
            result.SetNote(note: "The Card Transaction record is FAILED and the Payify record is also FAILED. No action is required.");
            return result;
        }

        if (payifyStatus == PayifyStatus.Missing)
        {
            result.SetNote(note: "The Card Transaction record is FAILED and there is no Payify record. No action is required.");
            return result;
        }

        result.SetNote(note: "The Card Transaction record is FAILED while the Payify record is SUCCESSFUL. Correct the response code and transaction status, then reverse the balance effect.");
        result.AddAutoOperation(
            OperationCodes.CorrectResponseCode,
            "Correct the response code.",
            BkmSnapshotBuilder.Create(context, OperationCodes.CorrectResponseCode, "D1"));
        result.AddAutoOperation(
            OperationCodes.ConvertTransactionToFailed,
            "Move the transaction to FAILED status.",
            BkmSnapshotBuilder.Create(context, OperationCodes.ConvertTransactionToFailed, "D1"));
        result.AddAutoOperation(
            OperationCodes.ReverseByBalanceEffect,
            "Create a reverse entry based on the balance effect.",
            BkmSnapshotBuilder.Create(context, OperationCodes.ReverseByBalanceEffect, "D1"));
        return result;
    }

    private async Task<EvaluationResult> EvaluateExpiredTransactionAsync(
        EvaluationResult result,
        BkmEvaluationContext context,
        CardBkmDetail detail,
        PayifyStatus payifyStatus,
        CancellationToken cancellationToken)
    {
        if (payifyStatus == PayifyStatus.Failed)
        {
            result.SetNote(note: "The Payify status is FAILED. Move the transaction to EXPIRE status and close the flow.");
            result.AddAutoOperation(
                OperationCodes.MoveTransactionToExpired,
                "Move the transaction to EXPIRE status.",
                BkmSnapshotBuilder.Create(context, OperationCodes.MoveTransactionToExpired, "C7"));
            return result;
        }

        if (payifyStatus == PayifyStatus.Missing)
        {
            result.SetNote(note: "There is no Payify record. Create the transaction and move it to EXPIRE status.");
            result.AddAutoOperation(
                OperationCodes.CreateTransaction,
                "Create the missing transaction.",
                BkmSnapshotBuilder.Create(context, OperationCodes.CreateTransaction, "C8"));
            result.AddAutoOperation(
                OperationCodes.MoveCreatedTransactionToExpired,
                "Move the created transaction to EXPIRE status.",
                BkmSnapshotBuilder.Create(context, OperationCodes.MoveCreatedTransactionToExpired, "C8"));
            return result;
        }

        if (await HasAccPendingMatchAsync(detail, cancellationToken))
        {
            result.SetNote(note: "An ACC ControlStat=P match was found within the last 20 days for the EXPIRE record. Raise an alert and require manual review.");
            result.AddAutoOperation(
                OperationCodes.RaiseAlert,
                "An ACC ControlStat=P match was found for the EXPIRE record.",
                BkmSnapshotBuilder.Create(context, OperationCodes.RaiseAlert, "C10"));
            result.AddManualOperation(
                OperationCodes.CreateManualReview,
                "Send the EXPIRE record to manual review.",
                code => BkmSnapshotBuilder.Create(context, code, "C10"),
                approveCode: OperationCodes.ApprovePendingAccReview,
                approveNote: "Close the case after manual review.",
                rejectCode: OperationCodes.RejectPendingAccReview,
                rejectNote: "Close the case after manual review.");
            return result;
        }

        result.SetNote(note: "The Card Transaction record is EXPIRE, the Payify record is SUCCESSFUL, and there is no ACC match. Move the transaction to EXPIRE status and reverse the balance effect.");
        result.AddAutoOperation(
            OperationCodes.MoveTransactionToExpired,
            "Move the transaction to EXPIRE status.",
            BkmSnapshotBuilder.Create(context, OperationCodes.MoveTransactionToExpired, "D2"));
        result.AddAutoOperation(
            OperationCodes.ReverseByBalanceEffect,
            "Create a reverse entry based on the balance effect.",
            BkmSnapshotBuilder.Create(context, OperationCodes.ReverseByBalanceEffect, "D2"));
        return result;
    }

    private EvaluationResult EvaluateSuccessfulTransaction(
        EvaluationResult result,
        BkmEvaluationContext context,
        CardBkmDetail detail,
        PayifyStatus payifyStatus,
        EmoneyCustomerTransactionDto? latestEmoney)
    {
        if (!IsSettled(detail))
        {
            result.SetNote(note: "TxnSettle != Y. No action is required.");
            return result;
        }

        if (payifyStatus == PayifyStatus.Failed)
        {
            result.SetNote(note: "The Card Transaction record is SUCCESSFUL while the Payify record is FAILED. Correct the response code and transaction status, then reverse the balance effect.");
            result.AddAutoOperation(
                OperationCodes.CorrectResponseCode,
                "Correct the response code.",
                BkmSnapshotBuilder.Create(context, OperationCodes.CorrectResponseCode, "D3"));
            result.AddAutoOperation(
                OperationCodes.ConvertTransactionToSuccessful,
                "Move the transaction to SUCCESSFUL status.",
                BkmSnapshotBuilder.Create(context, OperationCodes.ConvertTransactionToSuccessful, "D3"));
            result.AddAutoOperation(
                OperationCodes.ReverseByBalanceEffect,
                "Create a reverse entry based on the balance effect.",
                BkmSnapshotBuilder.Create(context, OperationCodes.ReverseByBalanceEffect, "D3"));
            return result;
        }

        if (payifyStatus == PayifyStatus.Missing)
        {
            if (IsRefund(detail))
            {
                result.AddAutoOperation(
                    OperationCodes.CreateTransaction,
                    "Create the missing transaction.",
                    BkmSnapshotBuilder.Create(context, OperationCodes.CreateTransaction, "C13"));
                return EvaluateRefund(result, context, detail, "C13");
            }

            result.SetNote(note: "The Card Transaction record is SUCCESSFUL while the Payify record is MISSING. Create the transaction and apply the refund/effect based on the original transaction.");
            result.AddAutoOperation(
                OperationCodes.CreateTransaction,
                "Create the transaction.",
                BkmSnapshotBuilder.Create(context, OperationCodes.CreateTransaction, "D4"));
            result.AddAutoOperation(
                OperationCodes.ApplyOriginalEffectOrRefund,
                "Execute the refund by original transaction.",
                BkmSnapshotBuilder.Create(context, OperationCodes.ApplyOriginalEffectOrRefund, "D4"));
            return result;
        }

        if (IsRefund(detail))
        {
            return EvaluateRefund(result, context, detail, null);
        }

        if (latestEmoney is null)
        {
            result.SetNote(note: "The Payify record could not be resolved. Raise an alert and require manual review.");
            result.AddAutoOperation(
                OperationCodes.RaiseAlert,
                "The Payify record could not be resolved.",
                BkmSnapshotBuilder.Create(context, OperationCodes.RaiseAlert, "C19"));
            result.AddManualOperation(
                OperationCodes.CreateManualReview,
                "Send the record to manual review.",
                code => BkmSnapshotBuilder.Create(context, code, "C19"),
                approveCode: OperationCodes.ApproveMissingPayifyTransaction,
                approveNote: "Approve the case manually.",
                rejectCode: OperationCodes.RejectMissingPayifyTransaction,
                rejectNote: "Reject the case manually.");
            return result;
        }

        if (AreAmountsEqual(latestEmoney.Amount, detail.BillingAmount))
        {
            result.SetNote(note: "Transaction.Amount equals CardTransaction.BillingAmount. No action is required.");
            return result;
        }

        if (IsTransactionAmountLessThanBilling(latestEmoney.Amount, detail.BillingAmount))
        {
            var difference = decimal.Round(detail.BillingAmount - latestEmoney.Amount, 2, MidpointRounding.AwayFromZero);
            result.SetNote(note: "Transaction.Amount is less than CardTransaction.BillingAmount. Create a shadow balance entry.");
            result.AddAutoOperation(
                OperationCodes.InsertShadowBalanceEntry,
                "Create a shadow balance entry.",
                BkmSnapshotBuilder.Create(context, OperationCodes.InsertShadowBalanceEntry, "D8", ("differenceAmount", difference)));
            result.AddAutoOperation(
                OperationCodes.RunShadowBalanceProcess,
                "Run the shadow balance process.",
                BkmSnapshotBuilder.Create(context, OperationCodes.RunShadowBalanceProcess, "D8", ("differenceAmount", difference)));
            return result;
        }

        result.SetNote(note: "Transaction.Amount is greater than BillingAmount. No action is required.");
        return result;
    }

    private EvaluationResult EvaluateRefund(
        EvaluationResult result,
        BkmEvaluationContext context,
        CardBkmDetail detail,
        string? missingPayifyDecisionPoint)
    {
        if (IsLinkedRefund(detail))
        {
            result.SetNote(note: "The refund is linked. Execute the matched refund.");
            result.AddAutoOperation(
                OperationCodes.ApplyLinkedRefund,
                "Execute the matched refund.",
                BkmSnapshotBuilder.Create(context, OperationCodes.ApplyLinkedRefund, "C17"));
            return result;
        }

        result.SetNote(note: "The refund is unlinked. Create a manual review item and await approve/reject.");
        result.AddManualOperation(
            OperationCodes.CreateManualReview,
            "Create a manual review item for the unlinked refund.",
            code => BkmSnapshotBuilder.Create(context, code, "C16"),
            approveCode: OperationCodes.ApplyUnlinkedRefundEffect,
            approveNote: "Apply the effect execution plan after approval.",
            rejectCode: OperationCodes.StartChargeback,
            rejectNote: "Start the chargeback flow after rejection.");
        return result;
    }

    private static EvaluationResult BuildUnmatchedFlowResult(
        EvaluationResult result,
        BkmEvaluationContext context)
    {
        result.SetNote(note: "The record does not match any supported flow. Raise an alert and require manual review.");
        result.AddAutoOperation(
            OperationCodes.RaiseAlert,
            "The record does not match any supported flow.",
            BkmSnapshotBuilder.Create(context, OperationCodes.RaiseAlert, "UNMATCHED"));
        result.AddManualOperation(
            OperationCodes.CreateManualReview,
            "Send the record to manual review.",
            code => BkmSnapshotBuilder.Create(context, code, "UNMATCHED"),
            approveCode: OperationCodes.ApproveUnmatchedFlow,
            approveNote: "Close the case manually.",
            rejectCode: OperationCodes.RejectUnmatchedFlow,
            rejectNote: "Close the case manually.");
        return result;
    }

    private static bool TryHandleMissingCardRow(
        EvaluationResult result,
        BkmEvaluationContext context,
        CardBkmDetail? currentCard)
    {
        if (currentCard is not null)
        {
            return false;
        }

        result.SetNote(note: "The Card Transaction row could not be resolved. Raise an alert and require manual review.");
        result.AddAutoOperation(
            OperationCodes.RaiseAlert,
            "The Card Transaction row could not be resolved.",
            BkmSnapshotBuilder.Create(context, OperationCodes.RaiseAlert, "CARD_ROW_MISSING"));
        result.AddManualOperation(
            OperationCodes.CreateManualReview,
            "Send the record to manual review.",
            code => BkmSnapshotBuilder.Create(context, code, "CARD_ROW_MISSING"),
            approveCode: OperationCodes.RecoverMissingCardRow,
            approveNote: "Requeue the record for re-evaluation.",
            rejectCode: OperationCodes.DropMissingCardRow,
            rejectNote: "Close the record manually.");
        return true;
    }

    private static bool TryHandleDuplicateRow(
        EvaluationResult result,
        BkmEvaluationContext context)
    {
        switch (ResolveDuplicateStatus(context.RootRow))
        {
            case DuplicateStatus.Conflict:
                result.SetNote(note: "The duplicate records are conflicting. Do not process both records and raise an alert.");
                result.AddAutoOperation(
                    OperationCodes.RaiseAlert,
                    "The duplicate records are conflicting.",
                    BkmSnapshotBuilder.Create(context, OperationCodes.RaiseAlert, "C2"));
                return true;
            case DuplicateStatus.Secondary:
                result.SetNote(note: "Equivalent duplicate records will be processed as a single transaction. Raise an alert.");
                result.AddAutoOperation(
                    OperationCodes.RaiseAlert,
                    "Equivalent duplicate records were detected.",
                    BkmSnapshotBuilder.Create(context, OperationCodes.RaiseAlert, "C2"));
                return true;
            case DuplicateStatus.Primary:
                result.SetNote(note: "Equivalent duplicate records will be processed as a single transaction. Raise an alert.");
                result.AddAutoOperation(
                    OperationCodes.RaiseAlert,
                    "Equivalent duplicate records were detected.",
                    BkmSnapshotBuilder.Create(context, OperationCodes.RaiseAlert, "C2"));
                return false;
            default:
                return false;
        }
    }

    private static bool TryHandleAmbiguousPayifyLookup(
        EvaluationResult result,
        BkmEvaluationContext context,
        PayifyStatus payifyStatus)
    {
        if (payifyStatus != PayifyStatus.Ambiguous)
        {
            return false;
        }

        result.SetNote(note: "The Payify lookup returned multiple candidates. Raise an alert and require manual review.");
        result.AddAutoOperation(
            OperationCodes.RaiseAlert,
            "The Payify lookup returned multiple candidates.",
            BkmSnapshotBuilder.Create(context, OperationCodes.RaiseAlert, "C19"));
        result.AddManualOperation(
            OperationCodes.CreateManualReview,
            "Send the record to manual review.",
            code => BkmSnapshotBuilder.Create(context, code, "C19"),
            approveCode: OperationCodes.ApproveAmbiguousPayifyRecord,
            approveNote: "Re-evaluate the flow with the selected record.",
            rejectCode: OperationCodes.RejectAmbiguousPayifyRecord,
            rejectNote: "Close the ambiguous record manually.");
        return true;
    }

    private async Task<OriginalTransactionResolution> ResolveOriginalTransactionAsync(
        CardBkmDetail detail,
        CancellationToken cancellationToken)
    {
        if (detail.OceanMainTxnGuid <= 0)
        {
            return new OriginalTransactionResolution(OriginalTransactionStatus.Missing, null);
        }

        var originalTransactions = (await _emoneyService.GetByCustomerTransactionIdAsync(
                detail.OceanMainTxnGuid.ToString(CultureInfo.InvariantCulture),
                cancellationToken))
            .OrderByDescending(x => x.TransactionDate)
            .ThenByDescending(x => x.Id)
            .ToList();

        if (originalTransactions.Count == 0)
        {
            return new OriginalTransactionResolution(OriginalTransactionStatus.Missing, null);
        }

        if (originalTransactions.Count > 1)
        {
            return new OriginalTransactionResolution(OriginalTransactionStatus.Ambiguous, null);
        }

        return new OriginalTransactionResolution(OriginalTransactionStatus.Resolved, originalTransactions[0]);
    }

    private async Task<bool> HasAccPendingMatchAsync(
        CardBkmDetail detail,
        CancellationToken cancellationToken)
    {
        var windowStart = ParseTransactionDate(detail.TransactionDate)?.AddDays(-20);

        var parsedDataQuery = _dbContext.IngestionFileLines
            .AsNoTracking()
            .Where(x => x.IngestionFile.FileType == FileType.Clearing)
            .Where(x => x.IngestionFile.ContentType == FileContentType.Bkm)
            .Where(x => x.RecordType == "D")
            .Where(x => x.Status == FileRowStatus.Success)
            .Where(x => x.DuplicateDetectionKey != null && x.DuplicateDetectionKey.EndsWith(":Problem"))
            .Select(x => x.ParsedData!);

        await foreach (var parsedData in parsedDataQuery.AsAsyncEnumerable().WithCancellation(cancellationToken))
        {
            if (string.IsNullOrWhiteSpace(parsedData))
                continue;

            ClearingBkmDetail? clearing;
            try
            {
                clearing = JsonSerializer.Deserialize<ClearingBkmDetail>(parsedData, JsonOptions);
            }
            catch
            {
                continue;
            }

            if (clearing is null)
                continue;

            if (windowStart.HasValue)
            {
                var ioDate = ParseDate(clearing.IoDate);
                if (ioDate.HasValue && ioDate.Value < windowStart.Value)
                    continue;
            }

            if (HasSameAccSignature(detail, clearing))
                return true;
        }

        return false;
    }

    private static bool HasSameAccSignature(CardBkmDetail card, ClearingBkmDetail clearing)
    {
        if (!EqualsNullable(card.Rrn, clearing.Rrn))
        {
            return false;
        }

        return string.Equals(card.CardNo, clearing.CardNo, StringComparison.OrdinalIgnoreCase) &&
               string.Equals(card.ProvisionCode, clearing.ProvisionCode, StringComparison.OrdinalIgnoreCase) &&
               string.Equals(card.Arn, clearing.Arn, StringComparison.OrdinalIgnoreCase) &&
               string.Equals(card.Mcc.ToString(), clearing.MccCode, StringComparison.OrdinalIgnoreCase) &&
               AreAmountsEqual(card.OriginalAmount, clearing.SourceAmount) &&
               string.Equals(card.OriginalCurrency.ToString(), clearing.SourceCurrency.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    private static bool EqualsNullable(string? left, string? right)
    {
        if (string.IsNullOrWhiteSpace(left) || string.IsNullOrWhiteSpace(right))
        {
            return true;
        }

        return string.Equals(left.Trim(), right.Trim(), StringComparison.OrdinalIgnoreCase);
    }

    private static CardBkmDetail? GetRootCardDetail(BkmEvaluationContext context) => DeserializeRootCardDetail(context.RootRow);

    private static EmoneyCustomerTransactionDto? GetLatestEmoneyTransaction(IReadOnlyList<EmoneyCustomerTransactionDto> transactions)
    {
        return transactions
            .OrderByDescending(x => x.TransactionDate)
            .ThenByDescending(x => x.Id)
            .FirstOrDefault();
    }

    private static bool HasFileLengthValidationFailure(BkmEvaluationContext context) =>
        context.RootFile.ExpectedCount != context.RootFile.TotalCount;

    private static DuplicateStatus ResolveDuplicateStatus(IngestionFileLine row)
    {
        return Enum.TryParse<DuplicateStatus>(row.DuplicateStatus, out var duplicateStatus)
            ? duplicateStatus
            : DuplicateStatus.Unique;
    }

    private static PayifyStatus ResolvePayifyStatus(IReadOnlyList<EmoneyCustomerTransactionDto> transactions)
    {
        return transactions.Count switch
        {
            0 => PayifyStatus.Missing,
            > 1 => PayifyStatus.Ambiguous,
            _ when transactions[0].IsCancelled => PayifyStatus.Failed,
            _ => PayifyStatus.Successful
        };
    }

    private static bool IsCancelOrReversal(CardBkmDetail detail) =>
        detail.TxnStat == CardBkmTxnStat.Reverse || detail.TxnStat == CardBkmTxnStat.Void;

    private static bool IsExpired(CardBkmDetail detail) => detail.TxnStat == CardBkmTxnStat.Expired;

    private static bool IsSuccessful(CardBkmDetail detail) =>
        string.Equals(detail.ResponseCode, "00", StringComparison.OrdinalIgnoreCase) &&
        detail.IsSuccessfulTxn == CardBkmIsSuccessfulTxn.Successful;

    private static bool IsFailed(CardBkmDetail detail) =>
        !string.Equals(detail.ResponseCode, "00", StringComparison.OrdinalIgnoreCase) &&
        detail.IsSuccessfulTxn == CardBkmIsSuccessfulTxn.Unsuccessful;

    private static bool IsSettled(CardBkmDetail detail) => detail.IsTxnSettle == CardBkmIsTxnSettle.Settled;

    private static bool IsRefund(CardBkmDetail detail)
    {
        var normalized = (detail.BankingTxnCode ?? string.Empty).Trim();
        return string.Equals(normalized, "Refund", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(normalized, "ReferenceRefund", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsLinkedRefund(CardBkmDetail detail) => detail.OceanMainTxnGuid > 0;

    private static bool AreAmountsEqual(decimal left, decimal right) =>
        decimal.Round(left, 2, MidpointRounding.AwayFromZero) ==
        decimal.Round(right, 2, MidpointRounding.AwayFromZero);

    private static bool IsTransactionAmountLessThanBilling(decimal transactionAmount, decimal billingAmount) =>
        decimal.Round(transactionAmount, 2, MidpointRounding.AwayFromZero) <
        decimal.Round(billingAmount, 2, MidpointRounding.AwayFromZero);

    private static DateTime? ParseTransactionDate(long value) => ParseDate(value.ToString(CultureInfo.InvariantCulture));

    private static DateTime? ParseDate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return DateTime.TryParseExact(value.Trim(), "yyyyMMdd", null, DateTimeStyles.None, out var parsed)
            ? parsed
            : null;
    }

    private static CardBkmDetail? DeserializeRootCardDetail(IngestionFileLine row)
    {
        if (!string.Equals(row.RecordType, "D", StringComparison.OrdinalIgnoreCase) ||
            string.IsNullOrWhiteSpace(row.ParsedData))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<CardBkmDetail>(row.ParsedData, JsonOptions);
        }
        catch
        {
            return null;
        }
    }

    private sealed record OriginalTransactionResolution(OriginalTransactionStatus Status, EmoneyCustomerTransactionDto? Transaction);

    private enum OriginalTransactionStatus
    {
        Missing,
        Ambiguous,
        Resolved
    }

    private enum PayifyStatus
    {
        Missing,
        Ambiguous,
        Failed,
        Successful
    }
}
