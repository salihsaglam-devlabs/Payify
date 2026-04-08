using System.Net.Http.Json;
using System.Text.Json;
using LinkPara.Card.Application.Commons.Exceptions;
using LinkPara.Card.Domain.Entities;
using LinkPara.Card.Domain.Entities.Reconciliation;
using LinkPara.Card.Domain.Enums.FileIngestion;
using LinkPara.Card.Infrastructure.Persistence;
using LinkPara.Card.Infrastructure.Services.Reconciliation;
using LinkPara.Card.Infrastructure.Services.Audit;
using LinkPara.Card.Infrastructure.Services.Reconciliation.Integrations.Emoney;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace LinkPara.Card.Infrastructure.Services.Reconciliation.Execute;

internal sealed class OperationExecutor
{
    private readonly CardDbContext _dbContext;
    private readonly IEmoneyService _emoneyService;
    private readonly IAuditStampService _auditStampService;
    private readonly IStringLocalizer _localizer;

    public OperationExecutor(CardDbContext dbContext, IEmoneyService emoneyService, IAuditStampService auditStampService, Func<LinkPara.Card.Application.Commons.Localization.LocalizerResource, IStringLocalizer> localizerFactory)
    {
        _dbContext = dbContext;
        _emoneyService = emoneyService;
        _auditStampService = auditStampService;
        _localizer = localizerFactory(LinkPara.Card.Application.Commons.Localization.LocalizerResource.Messages);
    }

    public Task<OperationHandlerResult> ExecuteAsync(
        ReconciliationOperation operation,
        CancellationToken cancellationToken = default)
    {
        return operation.Code switch
        {
            OperationCodes.RaiseAlert => ExecuteRaiseAlertAsync(operation, cancellationToken),
            OperationCodes.CreateManualReview => ExecuteCreateManualReviewAsync(operation, cancellationToken),
            OperationCodes.MarkOriginalTransactionCancelled => ExecuteMarkOriginalTransactionCancelledAsync(operation, cancellationToken),
            OperationCodes.ReverseOriginalTransaction => ExecuteReverseOriginalTransactionAsync(operation, cancellationToken),
            OperationCodes.CorrectResponseCode => ExecuteCorrectResponseCodeAsync(operation, cancellationToken),
            OperationCodes.ConvertTransactionToFailed => ExecuteConvertTransactionToFailedAsync(operation, cancellationToken),
            OperationCodes.ConvertTransactionToSuccessful => ExecuteConvertTransactionToSuccessfulAsync(operation, cancellationToken),
            OperationCodes.ReverseByBalanceEffect => ExecuteReverseByBalanceEffectAsync(operation, cancellationToken),
            OperationCodes.MoveTransactionToExpired => ExecuteMoveTransactionToExpiredAsync(operation, cancellationToken),
            OperationCodes.CreateTransaction => ExecuteCreateTransactionAsync(operation, cancellationToken),
            OperationCodes.MoveCreatedTransactionToExpired => ExecuteMoveCreatedTransactionToExpiredAsync(operation, cancellationToken),
            OperationCodes.ApplyOriginalEffectOrRefund => ExecuteApplyOriginalEffectOrRefundAsync(operation, cancellationToken),
            OperationCodes.ApplyLinkedRefund => ExecuteApplyLinkedRefundAsync(operation, cancellationToken),
            OperationCodes.ApplyUnlinkedRefundEffect => ExecuteApplyUnlinkedRefundEffectAsync(operation, cancellationToken),
            OperationCodes.StartChargeback => ExecuteStartChargebackAsync(operation, cancellationToken),
            OperationCodes.InsertShadowBalanceEntry => ExecuteInsertShadowBalanceEntryAsync(operation, cancellationToken),
            OperationCodes.RunShadowBalanceProcess => ExecuteRunShadowBalanceProcessAsync(operation, cancellationToken),
            OperationCodes.RecoverMissingCardRow => ExecuteRecoverMissingCardRowAsync(operation, cancellationToken),
            OperationCodes.ApproveAmbiguousPayifyRecord => ExecuteApproveAmbiguousPayifyRecordAsync(operation, cancellationToken),
            OperationCodes.BindOriginalTransactionAndContinue => ExecuteBindOriginalTransactionAndContinueAsync(operation, cancellationToken),
            OperationCodes.DropMissingCardRow => ExecuteDropMissingCardRowAsync(operation, cancellationToken),
            OperationCodes.RejectAmbiguousPayifyRecord => ExecuteRejectAmbiguousPayifyRecordAsync(operation, cancellationToken),
            OperationCodes.ApproveUnmatchedFlow => ExecuteApproveUnmatchedFlowAsync(operation, cancellationToken),
            OperationCodes.RejectUnmatchedFlow => ExecuteRejectUnmatchedFlowAsync(operation, cancellationToken),
            OperationCodes.RejectReversalRecord => ExecuteRejectReversalRecordAsync(operation, cancellationToken),
            OperationCodes.ApprovePendingAccReview => ExecuteApprovePendingAccReviewAsync(operation, cancellationToken),
            OperationCodes.RejectPendingAccReview => ExecuteRejectPendingAccReviewAsync(operation, cancellationToken),
            OperationCodes.ApproveMissingPayifyTransaction => ExecuteApproveMissingPayifyTransactionAsync(operation, cancellationToken),
            OperationCodes.RejectMissingPayifyTransaction => ExecuteRejectMissingPayifyTransactionAsync(operation, cancellationToken),
            _ => throw new ReconciliationUnsupportedOperationException(ApiErrorCode.ReconciliationUnsupportedOperationCode, _localizer.Get("Reconciliation.UnsupportedOperationCode", operation.Code))
        };
    }

    private async Task<OperationHandlerResult> ExecuteRaiseAlertAsync(ReconciliationOperation operation, CancellationToken cancellationToken)
    {
        var existingAlert = await _dbContext.ReconciliationAlerts
            .AsTracking()
            .FirstOrDefaultAsync(
                x => x.OperationId == operation.Id &&
                     x.AlertType == operation.Code &&
                     x.Message == operation.Note,
                cancellationToken);



        if (existingAlert is not null)
        {
            return Skipped("SKIPPED_ALERT_ALREADY_EXISTS", _localizer.Get("Reconciliation.AlertAlreadyExists"), operation, existingAlert);
        }

        var alert = new ReconciliationAlert
        {
            Id = Guid.NewGuid(),
            FileLineId = operation.FileLineId,
            GroupId = operation.GroupId,
            OperationId = operation.Id,
            EvaluationId = operation.EvaluationId,
            Severity = "Error",
            AlertType = operation.Code,
            Message = operation.Note
        };
        _auditStampService.StampForCreate(alert);
        await _dbContext.ReconciliationAlerts.AddAsync(alert, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Success("SUCCESS_ALERT_CREATED", _localizer.Get("Reconciliation.AlertCreated"), operation, new { alertId = alert.Id });
    }

    private async Task<OperationHandlerResult> ExecuteCreateManualReviewAsync(ReconciliationOperation operation, CancellationToken cancellationToken)
    {
        _ = cancellationToken;
        return Skipped("SKIPPED_MANUAL_REVIEW_GATE", _localizer.Get("Reconciliation.ManualReviewGateHandled"), operation, null);
    }

    private Task<OperationHandlerResult> ExecuteRecoverMissingCardRowAsync(ReconciliationOperation operation, CancellationToken cancellationToken)
        => ExecuteManualRequeueAsync(operation, _localizer.Get("Reconciliation.CaseRequeued", "RecoverMissingCardRow"), cancellationToken);

    private Task<OperationHandlerResult> ExecuteApproveAmbiguousPayifyRecordAsync(ReconciliationOperation operation, CancellationToken cancellationToken)
        => ExecuteManualRequeueAsync(operation, _localizer.Get("Reconciliation.CaseRequeued", "ApproveAmbiguousPayifyRecord"), cancellationToken);

    private Task<OperationHandlerResult> ExecuteBindOriginalTransactionAndContinueAsync(ReconciliationOperation operation, CancellationToken cancellationToken)
        => ExecuteManualRequeueAsync(operation, _localizer.Get("Reconciliation.CaseRequeued", "BindOriginalTransactionAndContinue"), cancellationToken);

    private Task<OperationHandlerResult> ExecuteDropMissingCardRowAsync(ReconciliationOperation operation, CancellationToken cancellationToken)
        => ExecuteManualCloseAsync(operation, _localizer.Get("Reconciliation.ManualBranchCompleted", "DropMissingCardRow"), cancellationToken);

    private Task<OperationHandlerResult> ExecuteRejectAmbiguousPayifyRecordAsync(ReconciliationOperation operation, CancellationToken cancellationToken)
        => ExecuteManualCloseAsync(operation, _localizer.Get("Reconciliation.ManualBranchCompleted", "RejectAmbiguousPayifyRecord"), cancellationToken);

    private Task<OperationHandlerResult> ExecuteApproveUnmatchedFlowAsync(ReconciliationOperation operation, CancellationToken cancellationToken)
        => ExecuteManualCloseAsync(operation, _localizer.Get("Reconciliation.ManualBranchCompleted", "ApproveUnmatchedFlow"), cancellationToken);

    private Task<OperationHandlerResult> ExecuteRejectUnmatchedFlowAsync(ReconciliationOperation operation, CancellationToken cancellationToken)
        => ExecuteManualCloseAsync(operation, _localizer.Get("Reconciliation.ManualBranchCompleted", "RejectUnmatchedFlow"), cancellationToken);

    private Task<OperationHandlerResult> ExecuteRejectReversalRecordAsync(ReconciliationOperation operation, CancellationToken cancellationToken)
        => ExecuteManualCloseAsync(operation, _localizer.Get("Reconciliation.ManualBranchCompleted", "RejectReversalRecord"), cancellationToken);

    private Task<OperationHandlerResult> ExecuteApprovePendingAccReviewAsync(ReconciliationOperation operation, CancellationToken cancellationToken)
        => ExecuteManualCloseAsync(operation, _localizer.Get("Reconciliation.ManualBranchCompleted", "ApprovePendingAccReview"), cancellationToken);

    private Task<OperationHandlerResult> ExecuteRejectPendingAccReviewAsync(ReconciliationOperation operation, CancellationToken cancellationToken)
        => ExecuteManualCloseAsync(operation, _localizer.Get("Reconciliation.ManualBranchCompleted", "RejectPendingAccReview"), cancellationToken);

    private Task<OperationHandlerResult> ExecuteApproveMissingPayifyTransactionAsync(ReconciliationOperation operation, CancellationToken cancellationToken)
        => ExecuteManualCloseAsync(operation, _localizer.Get("Reconciliation.ManualBranchCompleted", "ApproveMissingPayifyTransaction"), cancellationToken);

    private Task<OperationHandlerResult> ExecuteRejectMissingPayifyTransactionAsync(ReconciliationOperation operation, CancellationToken cancellationToken)
        => ExecuteManualCloseAsync(operation, _localizer.Get("Reconciliation.ManualBranchCompleted", "RejectMissingPayifyTransaction"), cancellationToken);

    private async Task<OperationHandlerResult> ExecuteManualRequeueAsync(
        ReconciliationOperation operation,
        string resultMessage,
        CancellationToken cancellationToken)
    {
        var row = await _dbContext.IngestionFileLines
            .AsTracking()
            .SingleAsync(x => x.Id == operation.FileLineId, cancellationToken);

        row.ReconciliationStatus = ReconciliationStatus.Ready;
        row.Message = operation.Note;

        await AddInfoAlertAsync(operation, cancellationToken);

        _auditStampService.StampForUpdate(row);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Success("SUCCESS_REQUEUED", resultMessage, operation, new { requeued = true });
    }

    private async Task<OperationHandlerResult> ExecuteManualCloseAsync(
        ReconciliationOperation operation,
        string resultMessage,
        CancellationToken cancellationToken)
    {
        await AddInfoAlertAsync(operation, cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Success("SUCCESS_MANUAL_OUTCOME_COMPLETED", resultMessage, operation, new { requeued = false });
    }

    private async Task<OperationHandlerResult> ExecuteMarkOriginalTransactionCancelledAsync(ReconciliationOperation operation, CancellationToken cancellationToken)
    {
        var payload = new OperationPayloadAccessor(operation.Payload, _localizer);
        var request = new
        {
            customerTransactionId = GetRequired<long>(payload, operation, "referenceTransactionId").ToString(),
            targetStatus = "Rejected",
            isCancelled = true,
            idempotencyKey = operation.IdempotencyKey,
            note = operation.Note
        };

        var response = await _emoneyService.UpdateTransactionStatusAsync(request, cancellationToken);
        return FromExternalResult("SUCCESS_MARK_ORIGINAL_TRANSACTION_CANCELLED", "MARK_ORIGINAL_TRANSACTION_CANCELLED_FAILED", request, response);
    }

    private async Task<OperationHandlerResult> ExecuteReverseOriginalTransactionAsync(ReconciliationOperation operation, CancellationToken cancellationToken)
    {
        var payload = new OperationPayloadAccessor(operation.Payload, _localizer);
        var request = new
        {
            customerTransactionId = GetRequired<long>(payload, operation, "referenceTransactionId").ToString(),
            referenceCustomerTransactionId = GetRequired<long>(payload, operation, "currentTransactionId").ToString(),
            txnEffect = GetRequired<string>(payload, operation, "txnEffect"),
            amount = GetRequired<decimal>(payload, operation, "billingAmount"),
            currencyCode = GetRequired<string>(payload, operation, "currencyCode"),
            idempotencyKey = operation.IdempotencyKey,
            note = operation.Note
        };

        var response = await _emoneyService.ReverseBalanceEffectAsync(request, cancellationToken);
        return FromExternalResult("SUCCESS_REVERSE_ORIGINAL_TRANSACTION", "REVERSE_ORIGINAL_TRANSACTION_FAILED", request, response);
    }

    private async Task<OperationHandlerResult> ExecuteCorrectResponseCodeAsync(ReconciliationOperation operation, CancellationToken cancellationToken)
    {
        var payload = new OperationPayloadAccessor(operation.Payload, _localizer);
        var responseCode = GetRequired<string>(payload, operation, "responseCode");
        var request = new
        {
            customerTransactionId = GetRequired<long>(payload, operation, "currentTransactionId").ToString(),
            responseCode,
            isSuccessful = string.Equals(responseCode, "00", StringComparison.OrdinalIgnoreCase),
            idempotencyKey = operation.IdempotencyKey,
            note = operation.Note
        };

        var response = await _emoneyService.CorrectResponseCodeAsync(request, cancellationToken);
        return FromExternalResult("SUCCESS_CORRECT_RESPONSE_CODE", "CORRECT_RESPONSE_CODE_FAILED", request, response);
    }

    private async Task<OperationHandlerResult> ExecuteConvertTransactionToFailedAsync(ReconciliationOperation operation, CancellationToken cancellationToken)
        => await ExecuteConvertTransactionStatusAsync(operation, "Failed", "SUCCESS_CONVERT_TRANSACTION_TO_FAILED", "CONVERT_TRANSACTION_TO_FAILED_FAILED", cancellationToken);

    private async Task<OperationHandlerResult> ExecuteConvertTransactionToSuccessfulAsync(ReconciliationOperation operation, CancellationToken cancellationToken)
        => await ExecuteConvertTransactionStatusAsync(operation, "Completed", "SUCCESS_CONVERT_TRANSACTION_TO_SUCCESSFUL", "CONVERT_TRANSACTION_TO_SUCCESSFUL_FAILED", cancellationToken);

    private async Task<OperationHandlerResult> ExecuteConvertTransactionStatusAsync(ReconciliationOperation operation, string targetStatus, string successCode, string failureCode, CancellationToken cancellationToken)
    {
        var payload = new OperationPayloadAccessor(operation.Payload, _localizer);
        var request = new
        {
            customerTransactionId = GetRequired<long>(payload, operation, "currentTransactionId").ToString(),
            targetStatus,
            isSettlementReceived = GetOptional<bool>(payload, operation, "isSettlementReceived"),
            idempotencyKey = operation.IdempotencyKey,
            note = operation.Note
        };

        var response = await _emoneyService.UpdateTransactionStatusAsync(request, cancellationToken);
        return FromExternalResult(successCode, failureCode, request, response);
    }

    private async Task<OperationHandlerResult> ExecuteReverseByBalanceEffectAsync(ReconciliationOperation operation, CancellationToken cancellationToken)
    {
        var payload = new OperationPayloadAccessor(operation.Payload, _localizer);
        var currentTransactionId = GetRequired<long>(payload, operation, "currentTransactionId");
        var request = new
        {
            customerTransactionId = currentTransactionId.ToString(),
            referenceCustomerTransactionId = GetOptional<long?>(payload, operation, "referenceTransactionId")?.ToString() ?? currentTransactionId.ToString(),
            txnEffect = GetRequired<string>(payload, operation, "txnEffect"),
            amount = GetRequired<decimal>(payload, operation, "billingAmount"),
            currencyCode = GetRequired<string>(payload, operation, "currencyCode"),
            idempotencyKey = operation.IdempotencyKey,
            note = operation.Note
        };

        var response = await _emoneyService.ReverseBalanceEffectAsync(request, cancellationToken);
        return FromExternalResult("SUCCESS_REVERSE_BY_BALANCE_EFFECT", "REVERSE_BY_BALANCE_EFFECT_FAILED", request, response);
    }

    private async Task<OperationHandlerResult> ExecuteMoveTransactionToExpiredAsync(ReconciliationOperation operation, CancellationToken cancellationToken)
    {
        var payload = new OperationPayloadAccessor(operation.Payload, _localizer);
        var request = new
        {
            customerTransactionId = GetRequired<long>(payload, operation, "currentTransactionId").ToString(),
            idempotencyKey = operation.IdempotencyKey,
            note = operation.Note
        };

        var response = await _emoneyService.ExpireTransactionAsync(request, cancellationToken);
        return FromExternalResult("SUCCESS_MOVE_TRANSACTION_TO_EXPIRED", "MOVE_TRANSACTION_TO_EXPIRED_FAILED", request, response);
    }

    private async Task<OperationHandlerResult> ExecuteCreateTransactionAsync(ReconciliationOperation operation, CancellationToken cancellationToken)
    {
        var payload = new OperationPayloadAccessor(operation.Payload, _localizer);
        var cardNo = GetRequired<string>(payload, operation, "cardNo");
        var walletBinding = await ResolveWalletBindingAsync(cardNo, cancellationToken);

        if (walletBinding is null)
        {
            var msg = _localizer.Get("Reconciliation.WalletBindingMissing");
            return Failed("CREATE_TRANSACTION_FAILED", msg, operation, null, "WALLET_BINDING_MISSING", msg);
        }

        var currentTransactionId = GetRequired<long>(payload, operation, "currentTransactionId");
        var request = new
        {
            customerTransactionId = currentTransactionId.ToString(),
            referenceCustomerTransactionId = GetOptional<long?>(payload, operation, "referenceTransactionId")?.ToString() ?? currentTransactionId.ToString(),
            walletNumber = walletBinding.WalletNumber,
            amount = GetRequired<decimal>(payload, operation, "billingAmount"),
            currencyCode = GetRequired<string>(payload, operation, "currencyCode"),
            externalReferenceId = GetOptional<string>(payload, operation, "externalReferenceId") ?? string.Empty,
            merchantId = GetOptional<string>(payload, operation, "merchantId") ?? string.Empty,
            description = GetOptional<string>(payload, operation, "description") ?? string.Empty,
            idempotencyKey = operation.IdempotencyKey
        };

        var response = await _emoneyService.CreateTransactionAsync(request, cancellationToken);
        return FromExternalResult("SUCCESS_CREATE_TRANSACTION", "CREATE_TRANSACTION_FAILED", request, response);
    }

    private async Task<OperationHandlerResult> ExecuteMoveCreatedTransactionToExpiredAsync(ReconciliationOperation operation, CancellationToken cancellationToken)
        => await ExecuteMoveTransactionToExpiredAsync(operation, cancellationToken);

    private async Task<OperationHandlerResult> ExecuteApplyOriginalEffectOrRefundAsync(ReconciliationOperation operation, CancellationToken cancellationToken)
        => await ExecuteApplyRefundInternalAsync(operation, "SUCCESS_APPLY_ORIGINAL_EFFECT_OR_REFUND", "APPLY_ORIGINAL_EFFECT_OR_REFUND_FAILED", cancellationToken);

    private async Task<OperationHandlerResult> ExecuteApplyLinkedRefundAsync(ReconciliationOperation operation, CancellationToken cancellationToken)
        => await ExecuteApplyRefundInternalAsync(operation, "SUCCESS_APPLY_LINKED_REFUND", "APPLY_LINKED_REFUND_FAILED", cancellationToken);

    private async Task<OperationHandlerResult> ExecuteApplyUnlinkedRefundEffectAsync(ReconciliationOperation operation, CancellationToken cancellationToken)
        => await ExecuteApplyRefundInternalAsync(operation, "SUCCESS_APPLY_UNLINKED_REFUND_EFFECT", "APPLY_UNLINKED_REFUND_EFFECT_FAILED", cancellationToken);

    private async Task<OperationHandlerResult> ExecuteApplyRefundAsync(ReconciliationOperation operation, string successCode, string failureCode, CancellationToken cancellationToken)
    {
        var payload = new OperationPayloadAccessor(operation.Payload, _localizer);
        var currentTransactionId = GetRequired<long>(payload, operation, "currentTransactionId");
        var request = new
        {
            customerTransactionId = currentTransactionId.ToString(),
            referenceCustomerTransactionId = GetOptional<long?>(payload, operation, "referenceTransactionId")?.ToString() ?? currentTransactionId.ToString(),
            amount = GetRequired<decimal>(payload, operation, "billingAmount"),
            currencyCode = GetRequired<string>(payload, operation, "currencyCode"),
            idempotencyKey = operation.IdempotencyKey,
            note = operation.Note
        };

        var response = await _emoneyService.RefundTransactionAsync(request, cancellationToken);
        return FromExternalResult(successCode, failureCode, request, response);
    }

    private Task<OperationHandlerResult> ExecuteApplyRefundInternalAsync(ReconciliationOperation operation, string successCode, string failureCode, CancellationToken cancellationToken)
        => ExecuteApplyRefundAsync(operation, successCode, failureCode, cancellationToken);

    private async Task<OperationHandlerResult> ExecuteStartChargebackAsync(ReconciliationOperation operation, CancellationToken cancellationToken)
    {
        var payload = new OperationPayloadAccessor(operation.Payload, _localizer);
        var cardNo = GetRequired<string>(payload, operation, "cardNo");
        var walletBinding = await ResolveWalletBindingAsync(cardNo, cancellationToken);
        var payifyTransactionId = GetOptional<Guid?>(payload, operation, "payifyTransactionId");

        if (!payifyTransactionId.HasValue || payifyTransactionId == Guid.Empty || walletBinding is null)
        {
            var msg = _localizer.Get("Reconciliation.ChargebackContextInvalid");
            return Failed("START_CHARGEBACK_FAILED", msg, operation, null, "CHARGEBACK_CONTEXT_INVALID", msg);
        }

        var currentTxnId = GetRequired<long>(payload, operation, "currentTransactionId");
        var chargebackDesc = _localizer.Get("Reconciliation.ChargebackDescription", currentTxnId);

        var initRequest = new
        {
            transactionId = payifyTransactionId.Value,
            walletNumber = walletBinding.WalletNumber,
            description = chargebackDesc,
            merchantId = GetOptional<string>(payload, operation, "merchantId") ?? string.Empty
        };

        var initResponse = await _emoneyService.InitChargebackAsync(initRequest, cancellationToken);
        if (!initResponse.IsSuccessful)
            return FromExternalResult("SUCCESS_START_CHARGEBACK", "START_CHARGEBACK_FAILED", initRequest, initResponse);

        var approveRequest = new
        {
            transactionId = payifyTransactionId.Value,
            status = "Approved",
            description = chargebackDesc
        };
        var approveResponse = await _emoneyService.ApproveChargebackAsync(approveRequest, cancellationToken);
        return FromExternalResult("SUCCESS_START_CHARGEBACK", "START_CHARGEBACK_FAILED", approveRequest, approveResponse);
    }

    private async Task<OperationHandlerResult> ExecuteInsertShadowBalanceEntryAsync(ReconciliationOperation operation, CancellationToken cancellationToken)
    {
        var payload = new OperationPayloadAccessor(operation.Payload, _localizer);
        var currentTransactionId = GetRequired<long>(payload, operation, "currentTransactionId");
        var request = new
        {
            customerTransactionId = currentTransactionId.ToString(),
            referenceCustomerTransactionId = GetOptional<long?>(payload, operation, "referenceTransactionId")?.ToString() ?? currentTransactionId.ToString(),
            amount = GetOptional<decimal?>(payload, operation, "differenceAmount") ?? 0m,
            currencyCode = GetRequired<string>(payload, operation, "currencyCode"),
            direction = ResolveDirection(GetRequired<string>(payload, operation, "txnEffect")),
            idempotencyKey = operation.IdempotencyKey
        };

        var response = await _emoneyService.CreateShadowBalanceDebtCreditAsync(request, cancellationToken);
        return FromExternalResult("SUCCESS_INSERT_SHADOW_BALANCE_ENTRY", "INSERT_SHADOW_BALANCE_ENTRY_FAILED", request, response);
    }

    private async Task<OperationHandlerResult> ExecuteRunShadowBalanceProcessAsync(ReconciliationOperation operation, CancellationToken cancellationToken)
    {
        var payload = new OperationPayloadAccessor(operation.Payload, _localizer);
        var request = new
        {
            customerTransactionId = GetRequired<long>(payload, operation, "currentTransactionId").ToString(),
            amount = GetOptional<decimal?>(payload, operation, "differenceAmount") ?? 0m,
            currencyCode = GetRequired<string>(payload, operation, "currencyCode"),
            direction = ResolveDirection(GetRequired<string>(payload, operation, "txnEffect")),
            idempotencyKey = operation.IdempotencyKey
        };

        var response = await _emoneyService.RunShadowBalanceProcessAsync(request, cancellationToken);
        return FromExternalResult("SUCCESS_RUN_SHADOW_BALANCE_PROCESS", "RUN_SHADOW_BALANCE_PROCESS_FAILED", request, response);
    }

    private async Task<CustomerWalletCard?> ResolveWalletBindingAsync(string cardNo, CancellationToken cancellationToken)
    {
        return await _dbContext.CustomerWalletCard
            .AsNoTracking()
            .Where(x => x.CardNumber == cardNo && x.IsActive)
            .OrderByDescending(x => x.CreateDate)
            .FirstOrDefaultAsync(cancellationToken);
    }

    private Task AddInfoAlertAsync(ReconciliationOperation operation, CancellationToken cancellationToken)
    {
        var alert = new ReconciliationAlert
        {
            Id = Guid.NewGuid(),
            FileLineId = operation.FileLineId,
            GroupId = operation.GroupId,
            OperationId = operation.Id,
            EvaluationId = operation.EvaluationId,
            Severity = "Info",
            AlertType = operation.Code,
            Message = operation.Note
        };
        _auditStampService.StampForCreate(alert);
        return _dbContext.ReconciliationAlerts.AddAsync(alert, cancellationToken).AsTask();
    }

    private static string ResolveDirection(string txnEffect)
    {
        return txnEffect switch
        {
            "Debit" => "Credit",
            "Credit" => "Debit",
            _ => "Credit"
        };
    }

    private OperationHandlerResult Success(string resultCode, string resultMessage, object? requestPayload, object? responsePayload)
    {
        return new OperationHandlerResult
        {
            ResultCode = resultCode,
            ResultMessage = resultMessage,
            IsSuccessful = true,
            IsSkipped = false,
            RequestPayload = requestPayload,
            ResponsePayload = responsePayload
        };
    }

    private OperationHandlerResult Skipped(string resultCode, string resultMessage, object? requestPayload, object? responsePayload)
    {
        return new OperationHandlerResult
        {
            ResultCode = resultCode,
            ResultMessage = resultMessage,
            IsSuccessful = true,
            IsSkipped = true,
            RequestPayload = requestPayload,
            ResponsePayload = responsePayload
        };
    }

    private OperationHandlerResult Failed(string resultCode, string resultMessage, object? requestPayload, object? responsePayload, string? errorCode = null, string? errorMessage = null)
    {
        return new OperationHandlerResult
        {
            ResultCode = resultCode,
            ResultMessage = resultMessage,
            IsSuccessful = false,
            IsSkipped = false,
            RequestPayload = requestPayload,
            ResponsePayload = responsePayload,
            ErrorCode = errorCode ?? resultCode,
            ErrorMessage = errorMessage ?? resultMessage
        };
    }

    private OperationHandlerResult FromExternalResult(string successCode, string failureCode, object request, EmoneyCommandResult response)
    {
        return response.IsSuccessful
            ? Success(successCode, _localizer.Get("Reconciliation.OperationCompletedSuccessfully"), request, response.ResponseBody)
            : Failed(failureCode, response.ErrorMessage ?? _localizer.Get("Reconciliation.RequestFailed"), request, response.ResponseBody, response.ErrorCode, response.ErrorMessage);
    }

    private static T GetRequired<T>(OperationPayloadAccessor payload, ReconciliationOperation operation, string key)
        => payload.GetRequiredValue<T>(operation.Code, key);

    private static T GetOptional<T>(OperationPayloadAccessor payload, ReconciliationOperation operation, string key)
        => payload.GetOptionalValue<T>(operation.Code, key);
}
