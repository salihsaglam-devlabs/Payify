using System.Globalization;
using System.Text.Json;
using LinkPara.Card.Domain.Entities.FileIngestion;
using LinkPara.Card.Domain.Enums.FileIngestion;
using LinkPara.Card.Infrastructure.Persistence;
using LinkPara.Card.Infrastructure.Services.Reconciliation.Execute;
using LinkPara.Card.Infrastructure.Services.Reconciliation.Integrations.Emoney;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace LinkPara.Card.Infrastructure.Services.Reconciliation.Evaluate;

internal sealed class BkmEvaluator : IEvaluator
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly CardDbContext _dbContext;
    private readonly IEmoneyService _emoneyService;
    private readonly IStringLocalizer _localizer;

    public BkmEvaluator(
        CardDbContext dbContext,
        IEmoneyService emoneyService,
        Func<LinkPara.Card.Application.Commons.Localization.LocalizerResource, IStringLocalizer> localizerFactory)
    {
        _dbContext = dbContext;
        _emoneyService = emoneyService;
        _localizer = localizerFactory(LinkPara.Card.Application.Commons.Localization.LocalizerResource.Messages);
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
            throw new InvalidOperationException(_localizer.Get("Reconciliation.Bkm.CurrentCardRowMissing"));
        }

        var detail = currentCard;
        var latestEmoney = GetLatestEmoneyTransaction(bkmContext.EmoneyTransactions);
        if (HasFileLengthValidationFailure(bkmContext))
        {
            result.SetNote(note: _localizer.Get("Reconciliation.Bkm.FileLengthValidationNote"));
            result.AddAutoOperation(
                OperationCodes.RaiseAlert,
                _localizer.Get("Reconciliation.Bkm.FileLengthValidationOp"),
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
            result.SetNote(note: _localizer.Get("Reconciliation.Bkm.OriginalTxnNotResolvedNote"));
            result.AddAutoOperation(
                OperationCodes.RaiseAlert,
                _localizer.Get("Reconciliation.Bkm.OriginalTxnNotResolvedOp"),
                BkmSnapshotBuilder.Create(context, OperationCodes.RaiseAlert, "C3"));
            result.AddManualOperation(
                OperationCodes.CreateManualReview,
                _localizer.Get("Reconciliation.Bkm.CancelReversalManualReviewOp"),
                code => BkmSnapshotBuilder.Create(context, code, "C3"),
                approveCode: OperationCodes.BindOriginalTransactionAndContinue,
                approveNote: _localizer.Get("Reconciliation.Bkm.BindOriginalAndReEvaluate"),
                rejectCode: OperationCodes.RejectReversalRecord,
                rejectNote: _localizer.Get("Reconciliation.Bkm.CloseCancelReversalManually"));
            return result;
        }

        if (originalResolution.Transaction?.IsCancelled == true)
        {
            result.SetNote(note: _localizer.Get("Reconciliation.Bkm.AlreadyCancelledNote"));
            return result;
        }

        result.SetNote(note: _localizer.Get("Reconciliation.Bkm.ReversalNotCancelledNote"));
        result.AddAutoOperation(
            OperationCodes.MarkOriginalTransactionCancelled,
            _localizer.Get("Reconciliation.Bkm.MarkOriginalCancelledOp"),
            BkmSnapshotBuilder.Create(context, OperationCodes.MarkOriginalTransactionCancelled, "D7", ("originalTransactionId", originalResolution.Transaction?.Id)));
        result.AddAutoOperation(
            OperationCodes.ReverseOriginalTransaction,
            _localizer.Get("Reconciliation.Bkm.ReverseOriginalOp"),
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
            result.SetNote(note: _localizer.Get("Reconciliation.Bkm.FailedBothNote"));
            return result;
        }

        if (payifyStatus == PayifyStatus.Missing)
        {
            result.SetNote(note: _localizer.Get("Reconciliation.Bkm.FailedMissingPayifyNote"));
            return result;
        }

        result.SetNote(note: _localizer.Get("Reconciliation.Bkm.FailedSuccessfulPayifyNote"));
        result.AddAutoOperation(
            OperationCodes.CorrectResponseCode,
            _localizer.Get("Reconciliation.Bkm.CorrectResponseCodeOp"),
            BkmSnapshotBuilder.Create(context, OperationCodes.CorrectResponseCode, "D1"));
        result.AddAutoOperation(
            OperationCodes.ConvertTransactionToFailed,
            _localizer.Get("Reconciliation.Bkm.ConvertToFailedOp"),
            BkmSnapshotBuilder.Create(context, OperationCodes.ConvertTransactionToFailed, "D1"));
        result.AddAutoOperation(
            OperationCodes.ReverseByBalanceEffect,
            _localizer.Get("Reconciliation.Bkm.ReverseByBalanceOp"),
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
            result.SetNote(note: _localizer.Get("Reconciliation.Bkm.ExpiredPayifyFailedNote"));
            result.AddAutoOperation(
                OperationCodes.MoveTransactionToExpired,
                _localizer.Get("Reconciliation.Bkm.MoveToExpiredOp"),
                BkmSnapshotBuilder.Create(context, OperationCodes.MoveTransactionToExpired, "C7"));
            return result;
        }

        if (payifyStatus == PayifyStatus.Missing)
        {
            result.SetNote(note: _localizer.Get("Reconciliation.Bkm.ExpiredMissingPayifyNote"));
            result.AddAutoOperation(
                OperationCodes.CreateTransaction,
                _localizer.Get("Reconciliation.Bkm.CreateTransactionOp"),
                BkmSnapshotBuilder.Create(context, OperationCodes.CreateTransaction, "C8"));
            result.AddAutoOperation(
                OperationCodes.MoveCreatedTransactionToExpired,
                _localizer.Get("Reconciliation.Bkm.MoveCreatedToExpiredOp"),
                BkmSnapshotBuilder.Create(context, OperationCodes.MoveCreatedTransactionToExpired, "C8"));
            return result;
        }

        if (await HasAccPendingMatchAsync(detail, cancellationToken))
        {
            result.SetNote(note: _localizer.Get("Reconciliation.Bkm.AccPendingMatchNote"));
            result.AddAutoOperation(
                OperationCodes.RaiseAlert,
                _localizer.Get("Reconciliation.Bkm.AccPendingMatchOp"),
                BkmSnapshotBuilder.Create(context, OperationCodes.RaiseAlert, "C10"));
            result.AddManualOperation(
                OperationCodes.CreateManualReview,
                _localizer.Get("Reconciliation.Bkm.ExpiredManualReviewOp"),
                code => BkmSnapshotBuilder.Create(context, code, "C10"),
                approveCode: OperationCodes.ApprovePendingAccReview,
                approveNote: _localizer.Get("Reconciliation.Bkm.CloseAfterManualReview"),
                rejectCode: OperationCodes.RejectPendingAccReview,
                rejectNote: _localizer.Get("Reconciliation.Bkm.CloseAfterManualReview"));
            return result;
        }

        result.SetNote(note: _localizer.Get("Reconciliation.Bkm.ExpiredSuccessfulNoAccNote"));
        result.AddAutoOperation(
            OperationCodes.MoveTransactionToExpired,
            _localizer.Get("Reconciliation.Bkm.MoveToExpiredOp"),
            BkmSnapshotBuilder.Create(context, OperationCodes.MoveTransactionToExpired, "D2"));
        result.AddAutoOperation(
            OperationCodes.ReverseByBalanceEffect,
            _localizer.Get("Reconciliation.Bkm.ReverseByBalanceOp"),
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
            result.SetNote(note: _localizer.Get("Reconciliation.Bkm.NotSettledNote"));
            return result;
        }

        if (payifyStatus == PayifyStatus.Failed)
        {
            result.SetNote(note: _localizer.Get("Reconciliation.Bkm.SuccessfulFailedPayifyNote"));
            result.AddAutoOperation(
                OperationCodes.CorrectResponseCode,
                _localizer.Get("Reconciliation.Bkm.CorrectResponseCodeOp"),
                BkmSnapshotBuilder.Create(context, OperationCodes.CorrectResponseCode, "D3"));
            result.AddAutoOperation(
                OperationCodes.ConvertTransactionToSuccessful,
                _localizer.Get("Reconciliation.Bkm.ConvertToSuccessfulOp"),
                BkmSnapshotBuilder.Create(context, OperationCodes.ConvertTransactionToSuccessful, "D3"));
            result.AddAutoOperation(
                OperationCodes.ReverseByBalanceEffect,
                _localizer.Get("Reconciliation.Bkm.ReverseByBalanceOp"),
                BkmSnapshotBuilder.Create(context, OperationCodes.ReverseByBalanceEffect, "D3"));
            return result;
        }

        if (payifyStatus == PayifyStatus.Missing)
        {
            if (IsRefund(detail))
            {
                result.AddAutoOperation(
                    OperationCodes.CreateTransaction,
                    _localizer.Get("Reconciliation.Bkm.CreateTransactionOp"),
                    BkmSnapshotBuilder.Create(context, OperationCodes.CreateTransaction, "C13"));
                return EvaluateRefund(result, context, detail, "C13");
            }

            result.SetNote(note: _localizer.Get("Reconciliation.Bkm.SuccessfulMissingRefundNote"));
            result.AddAutoOperation(
                OperationCodes.CreateTransaction,
                _localizer.Get("Reconciliation.Bkm.CreateTransactionGenericOp"),
                BkmSnapshotBuilder.Create(context, OperationCodes.CreateTransaction, "D4"));
            result.AddAutoOperation(
                OperationCodes.ApplyOriginalEffectOrRefund,
                _localizer.Get("Reconciliation.Bkm.ApplyOriginalEffectOp"),
                BkmSnapshotBuilder.Create(context, OperationCodes.ApplyOriginalEffectOrRefund, "D4"));
            return result;
        }

        if (IsRefund(detail))
        {
            return EvaluateRefund(result, context, detail, null);
        }

        if (latestEmoney is null)
        {
            result.SetNote(note: _localizer.Get("Reconciliation.Bkm.PayifyNotResolvedNote"));
            result.AddAutoOperation(
                OperationCodes.RaiseAlert,
                _localizer.Get("Reconciliation.Bkm.PayifyNotResolvedOp"),
                BkmSnapshotBuilder.Create(context, OperationCodes.RaiseAlert, "C19"));
            result.AddManualOperation(
                OperationCodes.CreateManualReview,
                _localizer.Get("Reconciliation.Bkm.SendToManualReviewOp"),
                code => BkmSnapshotBuilder.Create(context, code, "C19"),
                approveCode: OperationCodes.ApproveMissingPayifyTransaction,
                approveNote: _localizer.Get("Reconciliation.Bkm.ApproveCaseManually"),
                rejectCode: OperationCodes.RejectMissingPayifyTransaction,
                rejectNote: _localizer.Get("Reconciliation.Bkm.RejectCaseManually"));
            return result;
        }

        if (AreAmountsEqual(latestEmoney.Amount, detail.BillingAmount))
        {
            result.SetNote(note: _localizer.Get("Reconciliation.Bkm.AmountsEqualNote"));
            return result;
        }

        if (IsTransactionAmountLessThanBilling(latestEmoney.Amount, detail.BillingAmount))
        {
            var difference = decimal.Round(detail.BillingAmount - latestEmoney.Amount, 2, MidpointRounding.AwayFromZero);
            result.SetNote(note: _localizer.Get("Reconciliation.Bkm.AmountLessThanBillingNote"));
            result.AddAutoOperation(
                OperationCodes.InsertShadowBalanceEntry,
                _localizer.Get("Reconciliation.Bkm.InsertShadowBalanceOp"),
                BkmSnapshotBuilder.Create(context, OperationCodes.InsertShadowBalanceEntry, "D8", ("differenceAmount", difference)));
            result.AddAutoOperation(
                OperationCodes.RunShadowBalanceProcess,
                _localizer.Get("Reconciliation.Bkm.RunShadowBalanceOp"),
                BkmSnapshotBuilder.Create(context, OperationCodes.RunShadowBalanceProcess, "D8", ("differenceAmount", difference)));
            return result;
        }

        result.SetNote(note: _localizer.Get("Reconciliation.Bkm.AmountGreaterNote"));
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
            result.SetNote(note: _localizer.Get("Reconciliation.Bkm.LinkedRefundNote"));
            result.AddAutoOperation(
                OperationCodes.ApplyLinkedRefund,
                _localizer.Get("Reconciliation.Bkm.LinkedRefundOp"),
                BkmSnapshotBuilder.Create(context, OperationCodes.ApplyLinkedRefund, "C17"));
            return result;
        }

        result.SetNote(note: _localizer.Get("Reconciliation.Bkm.UnlinkedRefundNote"));
        result.AddManualOperation(
            OperationCodes.CreateManualReview,
            _localizer.Get("Reconciliation.Bkm.UnlinkedRefundManualOp"),
            code => BkmSnapshotBuilder.Create(context, code, "C16"),
            approveCode: OperationCodes.ApplyUnlinkedRefundEffect,
            approveNote: _localizer.Get("Reconciliation.Bkm.ApplyEffectAfterApproval"),
            rejectCode: OperationCodes.StartChargeback,
            rejectNote: _localizer.Get("Reconciliation.Bkm.StartChargebackAfterRejection"));
        return result;
    }

    private EvaluationResult BuildUnmatchedFlowResult(
        EvaluationResult result,
        BkmEvaluationContext context)
    {
        result.SetNote(note: _localizer.Get("Reconciliation.Bkm.UnmatchedFlowNote"));
        result.AddAutoOperation(
            OperationCodes.RaiseAlert,
            _localizer.Get("Reconciliation.Bkm.UnmatchedFlowOp"),
            BkmSnapshotBuilder.Create(context, OperationCodes.RaiseAlert, "UNMATCHED"));
        result.AddManualOperation(
            OperationCodes.CreateManualReview,
            _localizer.Get("Reconciliation.Bkm.SendToManualReviewOp"),
            code => BkmSnapshotBuilder.Create(context, code, "UNMATCHED"),
            approveCode: OperationCodes.ApproveUnmatchedFlow,
            approveNote: _localizer.Get("Reconciliation.Bkm.CloseCaseManually"),
            rejectCode: OperationCodes.RejectUnmatchedFlow,
            rejectNote: _localizer.Get("Reconciliation.Bkm.CloseCaseManually"));
        return result;
    }

    private bool TryHandleMissingCardRow(
        EvaluationResult result,
        BkmEvaluationContext context,
        CardBkmDetail? currentCard)
    {
        if (currentCard is not null)
        {
            return false;
        }

        result.SetNote(note: _localizer.Get("Reconciliation.Bkm.CardRowMissingNote"));
        result.AddAutoOperation(
            OperationCodes.RaiseAlert,
            _localizer.Get("Reconciliation.Bkm.CardRowMissingOp"),
            BkmSnapshotBuilder.Create(context, OperationCodes.RaiseAlert, "CARD_ROW_MISSING"));
        result.AddManualOperation(
            OperationCodes.CreateManualReview,
            _localizer.Get("Reconciliation.Bkm.SendToManualReviewOp"),
            code => BkmSnapshotBuilder.Create(context, code, "CARD_ROW_MISSING"),
            approveCode: OperationCodes.RecoverMissingCardRow,
            approveNote: _localizer.Get("Reconciliation.Bkm.RequeueForReEvaluation"),
            rejectCode: OperationCodes.DropMissingCardRow,
            rejectNote: _localizer.Get("Reconciliation.Bkm.CloseRecordManually"));
        return true;
    }

    private bool TryHandleDuplicateRow(
        EvaluationResult result,
        BkmEvaluationContext context)
    {
        switch (ResolveDuplicateStatus(context.RootRow))
        {
            case DuplicateStatus.Conflict:
                result.SetNote(note: _localizer.Get("Reconciliation.Bkm.DuplicateConflictNote"));
                result.AddAutoOperation(
                    OperationCodes.RaiseAlert,
                    _localizer.Get("Reconciliation.Bkm.DuplicateConflictOp"),
                    BkmSnapshotBuilder.Create(context, OperationCodes.RaiseAlert, "C2"));
                return true;
            case DuplicateStatus.Secondary:
                result.SetNote(note: _localizer.Get("Reconciliation.Bkm.DuplicateEquivalentNote"));
                result.AddAutoOperation(
                    OperationCodes.RaiseAlert,
                    _localizer.Get("Reconciliation.Bkm.DuplicateEquivalentOp"),
                    BkmSnapshotBuilder.Create(context, OperationCodes.RaiseAlert, "C2"));
                return true;
            case DuplicateStatus.Primary:
                result.SetNote(note: _localizer.Get("Reconciliation.Bkm.DuplicateEquivalentNote"));
                result.AddAutoOperation(
                    OperationCodes.RaiseAlert,
                    _localizer.Get("Reconciliation.Bkm.DuplicateEquivalentOp"),
                    BkmSnapshotBuilder.Create(context, OperationCodes.RaiseAlert, "C2"));
                return false;
            default:
                return false;
        }
    }

    private bool TryHandleAmbiguousPayifyLookup(
        EvaluationResult result,
        BkmEvaluationContext context,
        PayifyStatus payifyStatus)
    {
        if (payifyStatus != PayifyStatus.Ambiguous)
        {
            return false;
        }

        result.SetNote(note: _localizer.Get("Reconciliation.Bkm.AmbiguousPayifyNote"));
        result.AddAutoOperation(
            OperationCodes.RaiseAlert,
            _localizer.Get("Reconciliation.Bkm.AmbiguousPayifyOp"),
            BkmSnapshotBuilder.Create(context, OperationCodes.RaiseAlert, "C19"));
        result.AddManualOperation(
            OperationCodes.CreateManualReview,
            _localizer.Get("Reconciliation.Bkm.SendToManualReviewOp"),
            code => BkmSnapshotBuilder.Create(context, code, "C19"),
            approveCode: OperationCodes.ApproveAmbiguousPayifyRecord,
            approveNote: _localizer.Get("Reconciliation.Bkm.ReEvaluateWithSelected"),
            rejectCode: OperationCodes.RejectAmbiguousPayifyRecord,
            rejectNote: _localizer.Get("Reconciliation.Bkm.CloseAmbiguousManually"));
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

        var candidates = await _dbContext.Set<IngestionFileLine>()
            .AsNoTracking()
            .Where(x => x.CorrelationKey == "OceanTxnGuid"
                        && x.CorrelationValue == detail.OceanMainTxnGuid.ToString())
            .ToListAsync(cancellationToken);

        if (candidates.Count == 0)
        {
            return new OriginalTransactionResolution(OriginalTransactionStatus.Missing, null);
        }

        if (candidates.Count > 1)
        {
            return new OriginalTransactionResolution(OriginalTransactionStatus.Ambiguous, null);
        }

        var row = candidates[0];
        var parsed = DeserializeDetail(row.ParsedData);
        var emoneyTransactions = await _emoneyService.GetByCustomerTransactionIdAsync(
            (parsed?.OceanTxnGuid ?? detail.OceanMainTxnGuid).ToString(CultureInfo.InvariantCulture),
            cancellationToken);
        var latestEmoney = GetLatestEmoneyTransaction(emoneyTransactions.ToList());

        return new OriginalTransactionResolution(OriginalTransactionStatus.Found, latestEmoney);
    }

    private async Task<bool> HasAccPendingMatchAsync(
        CardBkmDetail detail,
        CancellationToken cancellationToken)
    {
        var txnGuid = detail.OceanTxnGuid.ToString(CultureInfo.InvariantCulture);

        return await _dbContext.Set<IngestionFileLine>()
            .AsNoTracking()
            .AnyAsync(x => x.CorrelationKey == "OceanTxnGuid"
                           && x.CorrelationValue == txnGuid
                           && x.ReconciliationStatus == Domain.Enums.FileIngestion.ReconciliationStatus.Processing,
                cancellationToken);
    }

    private static CardBkmDetail? GetRootCardDetail(BkmEvaluationContext context)
    {
        if (context.CardDetails.Count > 0)
        {
            return context.CardDetails[0];
        }

        return DeserializeDetail(context.RootRow.ParsedData);
    }

    private static CardBkmDetail? DeserializeDetail(string? parsedData)
    {
        if (string.IsNullOrWhiteSpace(parsedData))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<CardBkmDetail>(parsedData, JsonOptions);
        }
        catch
        {
            return null;
        }
    }

    private static EmoneyCustomerTransactionDto? GetLatestEmoneyTransaction(
        IReadOnlyList<EmoneyCustomerTransactionDto> transactions)
    {
        return transactions
            .OrderByDescending(x => x.TransactionDate)
            .ThenByDescending(x => x.Id)
            .FirstOrDefault();
    }

    private static PayifyStatus ResolvePayifyStatus(
        IReadOnlyList<EmoneyCustomerTransactionDto> transactions)
    {
        if (transactions.Count == 0) return PayifyStatus.Missing;
        if (transactions.Count > 1) return PayifyStatus.Ambiguous;

        var tx = transactions[0];
        return string.Equals(tx.TransactionStatus, "Failed", StringComparison.OrdinalIgnoreCase)
            ? PayifyStatus.Failed
            : PayifyStatus.Successful;
    }

    private static bool IsCancelOrReversal(CardBkmDetail detail)
        => detail.TxnStat is CardBkmTxnStat.Reverse or CardBkmTxnStat.Void;

    private static bool IsFailed(CardBkmDetail detail)
        => detail.IsSuccessfulTxn == CardBkmIsSuccessfulTxn.Unsuccessful;

    private static bool IsExpired(CardBkmDetail detail)
        => detail.TxnStat == CardBkmTxnStat.Expired;

    private static bool IsSuccessful(CardBkmDetail detail)
        => detail.IsSuccessfulTxn == CardBkmIsSuccessfulTxn.Successful;

    private static bool IsSettled(CardBkmDetail detail)
        => detail.IsTxnSettle == CardBkmIsTxnSettle.Settled;

    private static bool IsRefund(CardBkmDetail detail)
        => detail.TxnEffect == CardBkmTxnEffect.Credit;

    private static bool IsLinkedRefund(CardBkmDetail detail)
        => detail.OceanMainTxnGuid > 0 && detail.OceanMainTxnGuid != detail.OceanTxnGuid;

    private static bool AreAmountsEqual(decimal payifyAmount, decimal billingAmount)
        => decimal.Round(payifyAmount, 2, MidpointRounding.AwayFromZero)
           == decimal.Round(billingAmount, 2, MidpointRounding.AwayFromZero);

    private static bool IsTransactionAmountLessThanBilling(decimal payifyAmount, decimal billingAmount)
        => decimal.Round(payifyAmount, 2, MidpointRounding.AwayFromZero)
           < decimal.Round(billingAmount, 2, MidpointRounding.AwayFromZero);

    private static bool HasFileLengthValidationFailure(BkmEvaluationContext context)
        => !string.IsNullOrWhiteSpace(context.RootRow.Message)
           && context.RootRow.Message.Contains("length", StringComparison.OrdinalIgnoreCase);

    private static DuplicateStatus ResolveDuplicateStatus(IngestionFileLine row)
    {
        if (string.IsNullOrWhiteSpace(row.DuplicateStatus))
        {
            return DuplicateStatus.None;
        }

        if (Enum.TryParse<Domain.Enums.FileIngestion.DuplicateStatus>(row.DuplicateStatus, true, out var parsed))
        {
            return parsed switch
            {
                Domain.Enums.FileIngestion.DuplicateStatus.Conflict => DuplicateStatus.Conflict,
                Domain.Enums.FileIngestion.DuplicateStatus.Secondary => DuplicateStatus.Secondary,
                Domain.Enums.FileIngestion.DuplicateStatus.Primary => DuplicateStatus.Primary,
                _ => DuplicateStatus.None
            };
        }

        return DuplicateStatus.None;
    }

    private enum PayifyStatus { Missing, Successful, Failed, Ambiguous }
    private enum OriginalTransactionStatus { Missing, Found, Ambiguous }
    private enum DuplicateStatus { None, Primary, Secondary, Conflict }

    private sealed record OriginalTransactionResolution(
        OriginalTransactionStatus Status,
        EmoneyCustomerTransactionDto? Transaction);
}
