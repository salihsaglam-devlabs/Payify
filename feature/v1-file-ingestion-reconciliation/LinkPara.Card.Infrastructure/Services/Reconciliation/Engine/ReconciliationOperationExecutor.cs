using System.Globalization;
using LinkPara.Card.Application.Commons.Interfaces;
using LinkPara.Card.Application.Commons.Constants;
using LinkPara.Card.Application.Commons.Models.Reconciliation;
using LinkPara.Card.Domain.Constants;
using LinkPara.Card.Domain.Entities.FileIngestion;
using LinkPara.Card.Domain.Entities.Reconciliation;
using LinkPara.Card.Domain.Enums;
using LinkPara.Card.Infrastructure.Persistence;
using LinkPara.SharedModels.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace LinkPara.Card.Infrastructure.Services.Reconciliation.Engine;

public class ReconciliationOperationExecutor : IReconciliationOperationExecutor
{
    private readonly CardDbContext _dbContext;
    private readonly IReadOnlyDictionary<string, IReconciliationOperationHandler> _operationHandlers;
    private readonly ILogger<ReconciliationOperationExecutor> _logger;

    public ReconciliationOperationExecutor(
        CardDbContext dbContext,
        IEnumerable<IReconciliationOperationHandler> operationHandlers,
        ILogger<ReconciliationOperationExecutor> logger)
    {
        _dbContext = dbContext;
        _operationHandlers = operationHandlers.ToDictionary(x => x.OperationName, StringComparer.OrdinalIgnoreCase);
        _logger = logger;
    }

    public async Task<ReconciliationOperationPlan> PrepareAsync(
        ReconciliationOperationScope scope,
        string actor,
        CancellationToken cancellationToken = default)
    {
        var card = await _dbContext.CardTransactionRecords
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == scope.CardTransactionRecordId, cancellationToken);
        if (card is null)
        {
            return null;
        }

        var plannedOperations = await _dbContext.ReconciliationOperations
            .AsNoTracking()
            .Where(x => x.CardTransactionRecordId == scope.CardTransactionRecordId && x.RunId == scope.RunId)
            .OrderBy(x => x.OperationIndex)
            .Select(x => x.OperationCode)
            .ToArrayAsync(cancellationToken);
        if (plannedOperations.Length == 0)
        {
            return null;
        }

        var plan = new ReconciliationOperationPlan
        {
            RunId = scope.RunId,
            CardTransactionRecordId = scope.CardTransactionRecordId,
            ClearingRecordId = scope.ClearingRecordId,
            OceanTxnGuid = card.OceanTxnGuid,
            OceanMainTxnGuid = card.OceanMainTxnGuid,
            CardNo = card.CardNo,
            Rrn = card.Rrn,
            Arn = card.Arn,
            ProvisionCode = card.ProvisionCode,
            Mcc = card.Mcc,
            TxnStat = card.TxnStat,
            ResponseCode = card.ResponseCode,
            IsSuccessfulTxn = card.IsSuccessfulTxn,
            IsTxnSettle = card.IsTxnSettle,
            TxnEffect = card.TxnEffect,
            CardHolderBillingAmount = card.CardHolderBillingAmount,
            CardHolderBillingCurrency = card.CardHolderBillingCurrency,
            PlannedOperations = plannedOperations
        };

        plan.DerivedFields[ReconciliationDerivedFieldKeys.ResponseCodeTransition] = ResolveResponseCodeTransition(card, plannedOperations);
        plan.DerivedFields[ReconciliationDerivedFieldKeys.ShouldSetExpire] = plannedOperations.Contains(ReconciliationOperationCode.SetExpireStatus.ToString(), StringComparer.OrdinalIgnoreCase).ToString();
        plan.DerivedFields[ReconciliationDerivedFieldKeys.ShouldCreateTransaction] = plannedOperations.Contains(ReconciliationOperationCode.CreateTransaction.ToString(), StringComparer.OrdinalIgnoreCase).ToString();
        plan.DerivedFields[ReconciliationDerivedFieldKeys.ShouldMarkCancelled] = plannedOperations.Contains(ReconciliationOperationCode.MarkOriginalCancelled.ToString(), StringComparer.OrdinalIgnoreCase).ToString();
        plan.DerivedFields[ReconciliationDerivedFieldKeys.ReferenceTxnGuid] = string.IsNullOrWhiteSpace(card.OceanMainTxnGuid) ? card.OceanTxnGuid ?? string.Empty : card.OceanMainTxnGuid;
        plan.DerivedFields[ReconciliationDerivedFieldKeys.BalanceEffectHint] = ResolveBalanceEffectHint(card.TxnEffect, plannedOperations);
        plan.DerivedFields[ReconciliationDerivedFieldKeys.CardBillingAmount] = card.CardHolderBillingAmount?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
        plan.DerivedFields[ReconciliationDerivedFieldKeys.CardBillingCurrency] = card.CardHolderBillingCurrency ?? string.Empty;
        _logger.LogInformation("Reconciliation operation plan prepared. CardId={CardId}, RunId={RunId}, OperationCount={OperationCount}",
            scope.CardTransactionRecordId,
            scope.RunId,
            plannedOperations.Length);
        return plan;
    }

    public async Task<bool> ExecuteAsync(
        ReconciliationOperationPlan plan,
        ReconciliationOperationScope scope,
        string actor,
        CancellationToken cancellationToken = default)
    {
        var operations = await _dbContext.ReconciliationOperations
            .AsTracking()
            .Where(x => x.CardTransactionRecordId == scope.CardTransactionRecordId && x.RunId == scope.RunId)
            .OrderBy(x => x.OperationIndex)
            .ToListAsync(cancellationToken);
        if (operations.Count == 0)
        {
            _logger.LogWarning("No reconciliation operations found for scope. CardId={CardId}, RunId={RunId}", scope.CardTransactionRecordId, scope.RunId);
            return false;
        }

        var fingerprints = operations
            .Select(x => x.Fingerprint)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        var alreadyApplied = fingerprints.Length == 0
            ? new HashSet<string>(StringComparer.Ordinal)
            : (await _dbContext.ReconciliationOperations
                .AsNoTracking()
                .Where(x =>
                    x.CardTransactionRecordId == scope.CardTransactionRecordId &&
                    x.RunId != scope.RunId &&
                    x.Status == ReconciliationOperationStatus.Done &&
                    x.Fingerprint != null &&
                    fingerprints.Contains(x.Fingerprint))
                .Select(x => x.Fingerprint!)
                .ToListAsync(cancellationToken))
            .ToHashSet(StringComparer.Ordinal);

        var now = DateTime.Now;
        var approvedManualOperationId = TryResolveApprovedManualOperationId(plan);
        _logger.LogInformation("Reconciliation operation execution started. CardId={CardId}, RunId={RunId}, OperationCount={OperationCount}",
            scope.CardTransactionRecordId,
            scope.RunId,
            operations.Count);

        while (true)
        {
            var propagated = await PropagateUpstreamTerminalStatusesAsync(operations, actor);
            if (propagated)
            {
                now = DateTime.Now;
            }

            var invalidDependencyFound = await FailOperationsWithMissingDependencyAsync(operations, actor);
            if (invalidDependencyFound)
            {
                return false;
            }

            var next = operations
                .Where(x => x.Status is ReconciliationOperationStatus.Pending or ReconciliationOperationStatus.Blocked)
                .OrderBy(x => x.OperationIndex)
                .FirstOrDefault(x => IsDependencyResolved(x, operations, now));
            if (next is null)
            {
                if (operations.Any(x => x.Status is ReconciliationOperationStatus.Pending or ReconciliationOperationStatus.Blocked))
                {
                    _logger.LogInformation(
                        "Reconciliation operation execution postponed. CardId={CardId}, RunId={RunId}, Reason=NoRunnableOperation",
                        scope.CardTransactionRecordId,
                        scope.RunId);
                    return false;
                }

                break;
            }

            if (next.Mode == ReconciliationOperationMode.Manual)
            {
                var isApprovedManualOperation =
                    approvedManualOperationId.HasValue && approvedManualOperationId.Value == next.Id;

                if (!isApprovedManualOperation)
                {
                    if (next.Status == ReconciliationOperationStatus.Blocked)
                    {
                        await SetStatusAsync(next, ReconciliationOperationStatus.Pending, actor);
                    }

                    _logger.LogInformation("Execution paused at manual operation gate. CardId={CardId}, RunId={RunId}, OperationCode={OperationCode}, OperationIndex={OperationIndex}",
                        scope.CardTransactionRecordId,
                        scope.RunId,
                        next.OperationCode,
                        next.OperationIndex);
                    break;
                }

                if (next.Status == ReconciliationOperationStatus.Blocked)
                {
                    await SetStatusAsync(next, ReconciliationOperationStatus.Pending, actor);
                }
            }

            if (!string.IsNullOrWhiteSpace(next.Fingerprint) && alreadyApplied.Contains(next.Fingerprint))
            {
                await SetStatusAsync(next, ReconciliationOperationStatus.Skipped, actor, endedAt: DateTime.Now, errorCode: ReconciliationErrorCodes.AlreadyAppliedEffect, errorText: "Equivalent business effect already exists.");
                await CreateExecutionAsync(next, ReconciliationOperationExecutionOutcome.Skipped, actor, ReconciliationErrorCodes.AlreadyAppliedEffect, "Equivalent business effect already exists.", "Skipped by idempotency.");
                _logger.LogInformation("Operation skipped by idempotency. CardId={CardId}, RunId={RunId}, OperationCode={OperationCode}, OperationIndex={OperationIndex}",
                    scope.CardTransactionRecordId,
                    scope.RunId,
                    next.OperationCode,
                    next.OperationIndex);
                continue;
            }

            if (next.Status == ReconciliationOperationStatus.Blocked)
            {
                await SetStatusAsync(next, ReconciliationOperationStatus.Pending, actor);
            }

            await SetStatusAsync(next, ReconciliationOperationStatus.Processing, actor, startedAt: DateTime.Now);

            if (!_operationHandlers.TryGetValue(next.OperationCode, out var handler))
            {
                await SetStatusAsync(next, ReconciliationOperationStatus.Failed, actor, endedAt: DateTime.Now, errorCode: ReconciliationErrorCodes.HandlerNotFound, errorText: $"Handler not found: {next.OperationCode}");
                await CreateExecutionAsync(next, ReconciliationOperationExecutionOutcome.Failed, actor, ReconciliationErrorCodes.HandlerNotFound, $"Handler not found: {next.OperationCode}", "Handler lookup failed.");
                await SkipDownstreamAsync(operations, next.OperationIndex, actor, ReconciliationErrorCodes.UpstreamFailed, $"Skipped because upstream operation {next.OperationCode} failed.");
                _logger.LogError("Operation handler not found. CardId={CardId}, RunId={RunId}, OperationCode={OperationCode}",
                    scope.CardTransactionRecordId,
                    scope.RunId,
                    next.OperationCode);
                return false;
            }

            var execution = await StartExecutionAsync(next, actor, cancellationToken);
            bool isHandlerExecutionSuccessful;
            try
            {
                plan.DerivedFields[ReconciliationDerivedFieldKeys.CurrentOperationId] = next.Id.ToString();
                isHandlerExecutionSuccessful = await handler.ExecuteAsync(plan, scope, actor, cancellationToken);
            }
            catch (Exception ex)
            {
                await SetStatusAsync(next, ReconciliationOperationStatus.Failed, actor, endedAt: DateTime.Now, errorCode: ReconciliationErrorCodes.HandlerException, errorText: ex.Message);
                await CompleteExecutionAsync(execution, ReconciliationOperationExecutionOutcome.Failed, actor, ReconciliationErrorCodes.HandlerException, ex.Message, "Operation handler threw exception.");
                _logger.LogError(ex, "Operation handler exception. CardId={CardId}, RunId={RunId}, OperationCode={OperationCode}",
                    scope.CardTransactionRecordId,
                    scope.RunId,
                    next.OperationCode);
                await SkipDownstreamAsync(operations, next.OperationIndex, actor, ReconciliationErrorCodes.UpstreamFailed, $"Skipped because upstream operation {next.OperationCode} failed.");
                return false;
            }
            finally
            {
                plan.DerivedFields.Remove(ReconciliationDerivedFieldKeys.CurrentOperationId);
            }

            if (!isHandlerExecutionSuccessful)
            {
                await SetStatusAsync(next, ReconciliationOperationStatus.Failed, actor, endedAt: DateTime.Now, errorCode: ReconciliationErrorCodes.HandlerFailed, errorText: "Operation returned false.");
                await CompleteExecutionAsync(execution, ReconciliationOperationExecutionOutcome.Failed, actor, ReconciliationErrorCodes.HandlerFailed, "Operation returned false.", "Operation returned false.");
                _logger.LogWarning("Operation handler returned failure. CardId={CardId}, RunId={RunId}, OperationCode={OperationCode}",
                    scope.CardTransactionRecordId,
                    scope.RunId,
                    next.OperationCode);
                await SkipDownstreamAsync(operations, next.OperationIndex, actor, ReconciliationErrorCodes.UpstreamFailed, $"Skipped because upstream operation {next.OperationCode} failed.");
                return false;
            }

            await SetStatusAsync(next, ReconciliationOperationStatus.Done, actor, endedAt: DateTime.Now);
            await CompleteExecutionAsync(execution, ReconciliationOperationExecutionOutcome.Done, actor, responsePayload: "Operation completed successfully.");
            if (!string.IsNullOrWhiteSpace(next.Fingerprint))
            {
                alreadyApplied.Add(next.Fingerprint);
            }

            foreach (var dependent in operations.Where(x => x.Status == ReconciliationOperationStatus.Blocked && x.DependsOnIndex == next.OperationIndex))
            {
                await SetStatusAsync(dependent, ReconciliationOperationStatus.Pending, actor);
            }
        }

        _logger.LogInformation("Reconciliation operation execution finished. CardId={CardId}, RunId={RunId}", scope.CardTransactionRecordId, scope.RunId);
        return true;
    }

    private static Guid? TryResolveApprovedManualOperationId(ReconciliationOperationPlan plan)
    {
        if (!plan.DerivedFields.TryGetValue(ReconciliationDerivedFieldKeys.ApprovedManualOperationId, out var raw) ||
            string.IsNullOrWhiteSpace(raw))
        {
            return null;
        }

        return Guid.TryParse(raw, out var parsed) ? parsed : null;
    }

    private static bool IsDependencyResolved(ReconciliationOperation op, IReadOnlyCollection<ReconciliationOperation> all, DateTime now)
    {
        if (!op.DependsOnIndex.HasValue)
        {
            return true;
        }

        var dependency = all.FirstOrDefault(x => x.OperationIndex == op.DependsOnIndex.Value);
        if (dependency is null)
        {
            return false;
        }

        if (dependency.Status == ReconciliationOperationStatus.Skipped &&
            string.Equals(dependency.ErrorCode, ReconciliationErrorCodes.UpstreamRejected, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (dependency.Status == ReconciliationOperationStatus.Skipped &&
            string.Equals(dependency.ErrorCode, ReconciliationErrorCodes.UpstreamFailed, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return dependency.Status is ReconciliationOperationStatus.Done or ReconciliationOperationStatus.Skipped;
    }

    private async Task<bool> PropagateUpstreamTerminalStatusesAsync(
        IReadOnlyCollection<ReconciliationOperation> operations,
        string actor)
    {
        var changed = false;
        var keepIterating = true;
        while (keepIterating)
        {
            keepIterating = false;
            foreach (var op in operations
                         .Where(x => x.Status is ReconciliationOperationStatus.Pending or ReconciliationOperationStatus.Blocked)
                         .OrderBy(x => x.OperationIndex))
            {
                if (!op.DependsOnIndex.HasValue)
                {
                    continue;
                }

                var dependency = operations.FirstOrDefault(x => x.OperationIndex == op.DependsOnIndex.Value);
                if (dependency is null)
                {
                    continue;
                }

                if (dependency.Status == ReconciliationOperationStatus.Rejected)
                {
                    await SetStatusAsync(op, ReconciliationOperationStatus.Skipped, actor, endedAt: DateTime.Now, errorCode: ReconciliationErrorCodes.UpstreamRejected, errorText: "Skipped because upstream operation was rejected.");
                    await CreateExecutionAsync(op, ReconciliationOperationExecutionOutcome.Skipped, actor, ReconciliationErrorCodes.UpstreamRejected, "Skipped because upstream operation was rejected.", "Skipped by upstream rejected dependency.");
                    changed = true;
                    keepIterating = true;
                    continue;
                }

                if (dependency.Status == ReconciliationOperationStatus.Failed)
                {
                    await SetStatusAsync(op, ReconciliationOperationStatus.Skipped, actor, endedAt: DateTime.Now, errorCode: ReconciliationErrorCodes.UpstreamFailed, errorText: "Skipped because upstream operation failed.");
                    await CreateExecutionAsync(op, ReconciliationOperationExecutionOutcome.Skipped, actor, ReconciliationErrorCodes.UpstreamFailed, "Skipped because upstream operation failed.", "Skipped by upstream failed dependency.");
                    changed = true;
                    keepIterating = true;
                    continue;
                }

                if (dependency.Status == ReconciliationOperationStatus.Skipped &&
                    string.Equals(dependency.ErrorCode, ReconciliationErrorCodes.UpstreamRejected, StringComparison.OrdinalIgnoreCase))
                {
                    await SetStatusAsync(op, ReconciliationOperationStatus.Skipped, actor, endedAt: DateTime.Now, errorCode: ReconciliationErrorCodes.UpstreamRejected, errorText: "Skipped because upstream operation was rejected.");
                    await CreateExecutionAsync(op, ReconciliationOperationExecutionOutcome.Skipped, actor, ReconciliationErrorCodes.UpstreamRejected, "Skipped because upstream operation was rejected.", "Skipped by upstream rejected dependency.");
                    changed = true;
                    keepIterating = true;
                    continue;
                }

                if (dependency.Status == ReconciliationOperationStatus.Skipped &&
                    string.Equals(dependency.ErrorCode, ReconciliationErrorCodes.UpstreamFailed, StringComparison.OrdinalIgnoreCase))
                {
                    await SetStatusAsync(op, ReconciliationOperationStatus.Skipped, actor, endedAt: DateTime.Now, errorCode: ReconciliationErrorCodes.UpstreamFailed, errorText: "Skipped because upstream operation failed.");
                    await CreateExecutionAsync(op, ReconciliationOperationExecutionOutcome.Skipped, actor, ReconciliationErrorCodes.UpstreamFailed, "Skipped because upstream operation failed.", "Skipped by upstream failed dependency.");
                    changed = true;
                    keepIterating = true;
                }
            }
        }

        return changed;
    }

    private async Task<bool> FailOperationsWithMissingDependencyAsync(
        IReadOnlyCollection<ReconciliationOperation> operations,
        string actor)
    {
        var invalids = operations
            .Where(x => x.Status is ReconciliationOperationStatus.Pending or ReconciliationOperationStatus.Blocked)
            .Where(x => x.DependsOnIndex.HasValue && operations.All(o => o.OperationIndex != x.DependsOnIndex.Value))
            .ToArray();
        if (invalids.Length == 0)
        {
            return false;
        }

        foreach (var op in invalids)
        {
            await SetStatusAsync(op, ReconciliationOperationStatus.Failed, actor, endedAt: DateTime.Now, errorCode: ReconciliationErrorCodes.DependencyNotFound, errorText: $"Dependency operation index not found: {op.DependsOnIndex}");
            await CreateExecutionAsync(op, ReconciliationOperationExecutionOutcome.Failed, actor, ReconciliationErrorCodes.DependencyNotFound, $"Dependency operation index not found: {op.DependsOnIndex}", "Operation failed due to missing dependency.");
        }

        return true;
    }

    private async Task SkipDownstreamAsync(
        IReadOnlyCollection<ReconciliationOperation> operations,
        int upstreamOperationIndex,
        string actor,
        string errorCode,
        string errorText)
    {
        foreach (var op in operations.Where(x =>
                     x.OperationIndex > upstreamOperationIndex &&
                     x.Status is ReconciliationOperationStatus.Pending or ReconciliationOperationStatus.Blocked))
        {
            await SetStatusAsync(op, ReconciliationOperationStatus.Skipped, actor, endedAt: DateTime.Now, errorCode: errorCode, errorText: errorText);
            await CreateExecutionAsync(op, ReconciliationOperationExecutionOutcome.Skipped, actor, errorCode, errorText, "Skipped by upstream terminal status.");
        }
    }

    private Task SetStatusAsync(
        ReconciliationOperation op,
        ReconciliationOperationStatus status,
        string actor,
        DateTime? startedAt = null,
        DateTime? endedAt = null,
        string errorCode = null,
        string errorText = null)
    {
        op.Status = status;
        if (startedAt.HasValue)
        {
            op.StartedAt = startedAt.Value;
            op.CompletedAt = null;
            op.ErrorCode = null;
            op.ErrorText = null;
        }

        if (endedAt.HasValue)
        {
            op.CompletedAt = endedAt.Value;
        }

        if (!string.IsNullOrWhiteSpace(errorCode))
        {
            op.ErrorCode = errorCode;
        }

        if (!string.IsNullOrWhiteSpace(errorText))
        {
            op.ErrorText = errorText.Length > 2000 ? errorText[..2000] : errorText;
        }
        else if (status == ReconciliationOperationStatus.Done)
        {
            op.ErrorCode = null;
            op.ErrorText = null;
        }

        op.UpdateDate = DateTime.Now;
        op.LastModifiedBy = actor;
        return Task.CompletedTask;
    }

    private async Task<ReconciliationOperationExecution> StartExecutionAsync(
        ReconciliationOperation reconciliationOperation,
        string actor,
        CancellationToken cancellationToken)
    {
        var maxPersistedAttemptNumber = (await _dbContext.ReconciliationOperationExecutions
            .AsNoTracking()
            .Where(executionRecord => executionRecord.ReconciliationOperationId == reconciliationOperation.Id)
            .Select(executionRecord => (int?)executionRecord.AttemptNo)
            .MaxAsync(cancellationToken) ?? 0);

        var maxTrackedAttemptNumber = _dbContext.ChangeTracker.Entries<ReconciliationOperationExecution>()
            .Where(trackedExecutionEntry =>
                trackedExecutionEntry.Entity.ReconciliationOperationId == reconciliationOperation.Id &&
                trackedExecutionEntry.State is EntityState.Added or EntityState.Modified or EntityState.Unchanged)
            .Select(trackedExecutionEntry => (int?)trackedExecutionEntry.Entity.AttemptNo)
            .DefaultIfEmpty(0)
            .Max() ?? 0;

        var nextAttemptNumber = Math.Max(maxPersistedAttemptNumber, maxTrackedAttemptNumber) + 1;

        var execution = new ReconciliationOperationExecution
        {
            Id = Guid.NewGuid(),
            ReconciliationOperationId = reconciliationOperation.Id,
            ExecutionGroupId = reconciliationOperation.ExecutionGroupId,
            AttemptNo = nextAttemptNumber,
            StartedAt = DateTime.Now,
            Outcome = ReconciliationOperationExecutionOutcome.Processing,
            RequestPayload = JsonSerializer.Serialize(new { operationCode = reconciliationOperation.OperationCode }),
            CreateDate = DateTime.Now,
            CreatedBy = actor,
            UpdateDate = DateTime.Now,
            LastModifiedBy = actor,
            RecordStatus = RecordStatus.Active
        };

        _dbContext.ReconciliationOperationExecutions.Add(execution);
        return execution;
    }

    private async Task CreateExecutionAsync(
        ReconciliationOperation op,
        ReconciliationOperationExecutionOutcome outcome,
        string actor,
        string errorCode = null,
        string errorText = null,
        string responsePayload = null)
    {
        var execution = await StartExecutionAsync(op, actor, CancellationToken.None);
        await CompleteExecutionAsync(execution, outcome, actor, errorCode, errorText, responsePayload);
    }

    private Task CompleteExecutionAsync(
        ReconciliationOperationExecution execution,
        ReconciliationOperationExecutionOutcome outcome,
        string actor,
        string errorCode = null,
        string errorText = null,
        string responsePayload = null)
    {
        execution.Outcome = outcome;
        execution.EndedAt = DateTime.Now;
        execution.ErrorCode = errorCode;
        execution.ErrorText = errorText;
        execution.ResponsePayload = NormalizeJsonPayload(responsePayload);
        execution.UpdateDate = DateTime.Now;
        execution.LastModifiedBy = actor;
        return Task.CompletedTask;
    }

    private static string NormalizeJsonPayload(string payload)
    {
        if (string.IsNullOrWhiteSpace(payload))
        {
            return null;
        }

        try
        {
            using var _ = JsonDocument.Parse(payload);
            return payload;
        }
        catch (JsonException)
        {
            return JsonSerializer.Serialize(payload);
        }
    }

    private static string ResolveResponseCodeTransition(CardTransactionRecord card, IReadOnlyCollection<string> plannedOperations)
    {
        if (!plannedOperations.Contains(ReconciliationOperationCode.AdjustResponseCode.ToString(), StringComparer.OrdinalIgnoreCase))
        {
            return ReconciliationDerivedFieldValues.NoChange;
        }

        var isCardSuccessful = string.Equals((card.IsSuccessfulTxn ?? string.Empty).Trim(), ReconciliationFieldValues.FlagYes, StringComparison.OrdinalIgnoreCase) &&
                               string.Equals((card.ResponseCode ?? string.Empty).Trim(), ReconciliationFieldValues.ResponseCodeApproved, StringComparison.OrdinalIgnoreCase);
        return isCardSuccessful ? ReconciliationDerivedFieldValues.SuccessToFailed : ReconciliationDerivedFieldValues.FailedToSuccess;
    }

    private static string ResolveBalanceEffectHint(string txnEffect, IReadOnlyCollection<string> plannedOperations)
    {
        if (plannedOperations.Contains(ReconciliationOperationCode.ApplyRefundToOriginal.ToString(), StringComparer.OrdinalIgnoreCase) ||
            plannedOperations.Contains(ReconciliationOperationCode.ApplyManualRefundIfApproved.ToString(), StringComparer.OrdinalIgnoreCase))
        {
            return ReconciliationDerivedFieldValues.RefundEffect;
        }

        return string.Equals((txnEffect ?? string.Empty).Trim(), "R", StringComparison.OrdinalIgnoreCase)
            ? ReconciliationDerivedFieldValues.RefundEffect
            : ReconciliationDerivedFieldValues.OriginalEffect;
    }
}
