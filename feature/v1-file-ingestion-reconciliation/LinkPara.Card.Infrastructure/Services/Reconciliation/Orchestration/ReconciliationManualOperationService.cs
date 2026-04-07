using LinkPara.Card.Application.Commons.Interfaces;
using LinkPara.Card.Application.Commons.Constants;
using LinkPara.Card.Application.Commons.Models.Logging;
using LinkPara.Card.Application.Commons.Models.Reconciliation;
using LinkPara.Card.Domain.Constants;
using LinkPara.Card.Domain.Entities.FileIngestion;
using LinkPara.Card.Domain.Entities.Reconciliation;
using LinkPara.Card.Domain.Enums;
using LinkPara.Card.Infrastructure.Persistence;
using LinkPara.SharedModels.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Text.Json;

namespace LinkPara.Card.Infrastructure.Services.Reconciliation.Orchestration;

public class ReconciliationManualOperationService : IReconciliationManualOperationService
{
    private readonly CardDbContext _dbContext;
    private readonly IReconciliationOperationExecutor _operationExecutor;
    private readonly IReconciliationAlarmService _alarmService;
    private readonly IBulkServiceOperationLogPublisher _bulkLogPublisher;
    private readonly ILogger<ReconciliationManualOperationService> _logger;

    public ReconciliationManualOperationService(
        CardDbContext dbContext,
        IReconciliationOperationExecutor operationExecutor,
        IReconciliationAlarmService alarmService,
        IBulkServiceOperationLogPublisher bulkLogPublisher,
        ILogger<ReconciliationManualOperationService> logger)
    {
        _dbContext = dbContext;
        _operationExecutor = operationExecutor;
        _alarmService = alarmService;
        _bulkLogPublisher = bulkLogPublisher;
        _logger = logger;
    }

    public async Task<IReadOnlyCollection<ReconciliationRunListItem>> GetPendingManualReviewsAsync(int take = 100, CancellationToken cancellationToken = default)
    {
        return await RunWithBulkLogAsync(
            endpointName: nameof(GetPendingManualReviewsAsync),
            actor: AuditUsers.CardFileIngestion,
            metadata: new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["take"] = take.ToString()
            },
            run: async () =>
            {
                if (take <= 0)
                {
                    take = 100;
                }

                var runs = await (
                        from manual in _dbContext.ReconciliationManualReviewItems.AsNoTracking()
                        join operation in _dbContext.ReconciliationOperations.AsNoTracking()
                            on manual.ReconciliationOperationId equals operation.Id
                        where manual.ReviewStatus == ManualReviewStatus.Pending
                        group operation by new { operation.CardTransactionRecordId, operation.RunId, operation.ExecutionGroupId }
                        into grouped
                        orderby grouped.Min(x => x.CreateDate) descending
                        select new
                        {
                            grouped.Key.CardTransactionRecordId,
                            grouped.Key.RunId,
                            grouped.Key.ExecutionGroupId,
                            CreateDate = grouped.Min(x => x.CreateDate)
                        })
                    .Take(take)
                    .ToListAsync(cancellationToken);

                if (runs.Count == 0)
                {
                    return [];
                }

                var runIds = runs.Select(x => x.RunId).Distinct().ToArray();
                var operations = await _dbContext.ReconciliationOperations
                    .AsNoTracking()
                    .Where(x => runIds.Contains(x.RunId))
                    .OrderBy(x => x.OperationIndex)
                    .ToListAsync(cancellationToken);

                var operationIds = operations.Select(x => x.Id).ToArray();
                var manualMap = operationIds.Length == 0
                    ? new Dictionary<Guid, ReconciliationManualReviewItem>()
                    : await _dbContext.ReconciliationManualReviewItems
                        .AsNoTracking()
                        .Where(x => x.ReviewStatus == ManualReviewStatus.Pending && operationIds.Contains(x.ReconciliationOperationId))
                        .ToDictionaryAsync(x => x.ReconciliationOperationId, x => x, cancellationToken);

                var runOperationsLookup = operations
                    .GroupBy(x => new { x.CardTransactionRecordId, x.RunId })
                    .ToDictionary(
                        x => (x.Key.CardTransactionRecordId, x.Key.RunId),
                        x => x.OrderBy(v => v.OperationIndex).ToArray());

                return runs.OrderByDescending(x => x.CreateDate).Select(run =>
                {
                    if (!runOperationsLookup.TryGetValue((run.CardTransactionRecordId, run.RunId), out var runOps))
                    {
                        runOps = [];
                    }

                    var manuals = runOps.Where(x => manualMap.TryGetValue(x.Id, out var m) && m.ReviewStatus == ManualReviewStatus.Pending).Select(x => manualMap[x.Id]).ToArray();
                    return new ReconciliationRunListItem
                    {
                        CardTransactionRecordId = run.CardTransactionRecordId,
                        ClearingRecordId = runOps.FirstOrDefault()?.ClearingRecordId,
                        RunId = run.RunId,
                        ExecutionGroupId = run.ExecutionGroupId,
                        CreateDate = run.CreateDate,
                        OperationMode = runOps.Any(x => x.Mode == ReconciliationOperationMode.Manual) ? ReconciliationOperationMode.Manual : ReconciliationOperationMode.Auto,
                        RunStatus = ResolveRunStatus(runOps),
                        PlannedOperations = runOps.Select(x => x.OperationCode).ToArray(),
                        Operations = runOps.Select(x => new ReconciliationRunOperationListItem
                        {
                            Id = x.Id,
                            OperationIndex = x.OperationIndex,
                            OperationShortCode = $"O{x.OperationIndex:00}",
                            OperationCode = x.OperationCode,
                            OperationText = x.OperationCode,
                            OperationHandlerName = $"{x.OperationCode}OperationHandler",
                            Mode = x.Mode,
                            Status = x.Status
                        }).ToArray(),
                        OperationPreview = string.Join(Environment.NewLine, runOps.Select(x => $"{x.OperationIndex}. {x.OperationCode} - {x.Status}")),
                        ManualReviewItemIds = manuals.Select(x => x.Id).ToArray(),
                        ManualReviews = manuals.Select(x => new ManualReviewListItem
                        {
                            Id = x.Id,
                            ReviewStatus = x.ReviewStatus,
                            ReviewNote = x.ReviewNote
                        }).ToArray()
                    };
                }).ToArray();
            },
            isSuccess: result => true,
            cancellationToken);
    }

    public async Task<ManualReviewExecutionPreview> GetManualReviewDecisionPreviewAsync(Guid manualReviewItemId, CancellationToken cancellationToken = default)
    {
        return await RunWithBulkLogAsync(
            endpointName: nameof(GetManualReviewDecisionPreviewAsync),
            actor: AuditUsers.CardFileIngestion,
            metadata: new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["manualReviewItemId"] = manualReviewItemId.ToString()
            },
            run: async () =>
            {
                var manual = await _dbContext.ReconciliationManualReviewItems.AsNoTracking().FirstOrDefaultAsync(x => x.Id == manualReviewItemId, cancellationToken);
                if (manual is null)
                {
                    return null;
                }

                var manualOperation = await _dbContext.ReconciliationOperations.AsNoTracking().FirstOrDefaultAsync(x => x.Id == manual.ReconciliationOperationId, cancellationToken);
                if (manualOperation is null)
                {
                    return null;
                }

                var runOps = await _dbContext.ReconciliationOperations
                    .AsNoTracking()
                    .Where(x => x.CardTransactionRecordId == manualOperation.CardTransactionRecordId && x.RunId == manualOperation.RunId)
                    .OrderBy(x => x.OperationIndex)
                    .ToListAsync(cancellationToken);
                var manualIndex = manualOperation.OperationIndex;

                return new ManualReviewExecutionPreview
                {
                    ManualReviewItemId = manualReviewItemId,
                    ReconciliationOperationId = manualOperation.Id,
                    CardTransactionRecordId = manualOperation.CardTransactionRecordId,
                    RunId = manualOperation.RunId,
                    PlannedOperations = runOps.Select(x => x.OperationCode).ToArray(),
                    ApproveOperationPath = runOps
                        .Where(x => x.OperationIndex >= manualIndex &&
                                    (x.Status is ReconciliationOperationStatus.Pending or ReconciliationOperationStatus.Blocked or ReconciliationOperationStatus.Processing))
                        .Select(x => $"{x.OperationIndex}:{x.OperationCode}")
                        .ToArray(),
                    RejectOperationPath = runOps
                        .Where(x => x.OperationIndex > manualIndex &&
                                    (x.Status is ReconciliationOperationStatus.Pending or ReconciliationOperationStatus.Blocked or ReconciliationOperationStatus.Processing))
                        .Select(x => $"{x.OperationIndex}:{x.OperationCode}")
                        .ToArray(),
                    OperationPreview = string.Join(Environment.NewLine, runOps.Select(x => $"{x.OperationIndex}. {x.OperationCode} - {x.Status}")),
                    LastExecutionLogs = runOps.Select(x => new ManualReviewExecutionLogItem
                    {
                        OperationOrder = x.OperationIndex,
                        OperationName = x.OperationCode,
                        Status = x.Status.ToString().ToUpperInvariant(),
                        StartedAt = x.StartedAt,
                        EndedAt = x.CompletedAt,
                        ErrorMessage = x.ErrorText
                    }).ToArray()
                };
            },
            isSuccess: _ => true,
            cancellationToken);
    }

    public async Task<ManualReviewOperationResult> ApproveManualReviewAsync(Guid manualReviewItemId, string note, string actor, CancellationToken cancellationToken = default)
    {
        return await RunWithBulkLogAsync(
            endpointName: nameof(ApproveManualReviewAsync),
            actor: actor,
            metadata: new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["manualReviewItemId"] = manualReviewItemId.ToString()
            },
            run: async () =>
            {
                var strategy = _dbContext.Database.CreateExecutionStrategy();
                return await strategy.ExecuteAsync(async () =>
                {
                    await using var tx = await _dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
                    try
                    {
                        var manualOperationId = await _dbContext.ReconciliationManualReviewItems
                            .AsNoTracking()
                            .Where(x => x.Id == manualReviewItemId)
                            .Select(x => (Guid?)x.ReconciliationOperationId)
                            .FirstOrDefaultAsync(cancellationToken);

                        if (!manualOperationId.HasValue)
                        {
                            await tx.RollbackAsync(cancellationToken);
                            return ManualReviewOperationResult.NotFound("Manual review item not found.");
                        }

                        var now = DateTime.Now;
                        var updatedRows = await _dbContext.ReconciliationManualReviewItems
                            .Where(x => x.Id == manualReviewItemId && x.ReviewStatus == ManualReviewStatus.Pending)
                            .ExecuteUpdateAsync(setters => setters
                                .SetProperty(x => x.ReviewStatus, ManualReviewStatus.Approved)
                                .SetProperty(x => x.ReviewNote, note ?? string.Empty)
                                .SetProperty(x => x.ReviewedBy, actor)
                                .SetProperty(x => x.ReviewedAt, now)
                                .SetProperty(x => x.UpdateDate, now)
                                .SetProperty(x => x.LastModifiedBy, actor), cancellationToken);

                        if (updatedRows == 0)
                        {
                            await tx.RollbackAsync(cancellationToken);
                            return ManualReviewOperationResult.AlreadyReviewed("Manual review item is already processed.");
                        }

                        var operation = await _dbContext.ReconciliationOperations
                            .AsTracking()
                            .FirstOrDefaultAsync(x => x.Id == manualOperationId.Value, cancellationToken);
                        if (operation is null)
                        {
                            await tx.RollbackAsync(cancellationToken);
                            return ManualReviewOperationResult.OperationExecutionFailed("Reconciliation operation not found for manual review item.");
                        }

                        operation.Status = ReconciliationOperationStatus.Pending;
                        operation.CompletedAt = null;
                        operation.StartedAt = null;
                        operation.ErrorCode = null;
                        operation.ErrorText = null;
                        operation.UpdateDate = now;
                        operation.LastModifiedBy = actor;

                        foreach (var dependent in await _dbContext.ReconciliationOperations
                                     .AsTracking()
                                     .Where(x => x.CardTransactionRecordId == operation.CardTransactionRecordId && x.RunId == operation.RunId && x.DependsOnIndex == operation.OperationIndex && x.Status == ReconciliationOperationStatus.Blocked)
                                     .ToListAsync(cancellationToken))
                        {
                            dependent.Status = ReconciliationOperationStatus.Pending;
                            dependent.UpdateDate = now;
                            dependent.LastModifiedBy = actor;
                        }

                        var scope = new ReconciliationOperationScope
                        {
                            CardTransactionRecordId = operation.CardTransactionRecordId,
                            ClearingRecordId = operation.ClearingRecordId,
                            RunId = operation.RunId
                        };
                        var plan = await _operationExecutor.PrepareAsync(scope, actor, cancellationToken);
                        if (plan is null)
                        {
                            await tx.RollbackAsync(cancellationToken);
                            return ManualReviewOperationResult.OperationExecutionFailed("Operation plan not found for manual review run.");
                        }

                        plan.DerivedFields[ReconciliationDerivedFieldKeys.ApprovedManualOperationId] = operation.Id.ToString();
                        var executed = await _operationExecutor.ExecuteAsync(plan, scope, actor, cancellationToken);
                        await UpdateCardStateFromRunAsync(operation.CardTransactionRecordId, operation.RunId, actor, cancellationToken);

                        await _dbContext.SaveChangesAsync(cancellationToken);
                        await tx.CommitAsync(cancellationToken);

                        _logger.LogInformation("Manual review approved. ManualReviewItemId={ManualReviewItemId}, RunId={RunId}, Executed={Executed}",
                            manualReviewItemId,
                            operation.RunId,
                            executed);
                        return executed ? ManualReviewOperationResult.Success() : ManualReviewOperationResult.OperationExecutionFailed("Operation execution failed after manual approval.");
                    }
                    catch
                    {
                        await tx.RollbackAsync(cancellationToken);
                        throw;
                    }
                });
            },
            isSuccess: result => result?.Status == ManualReviewOperationStatus.Success,
            cancellationToken);
    }

    public async Task<ManualReviewOperationResult> RejectManualReviewAsync(Guid manualReviewItemId, string note, string actor, CancellationToken cancellationToken = default)
    {
        return await RunWithBulkLogAsync(
            endpointName: nameof(RejectManualReviewAsync),
            actor: actor,
            metadata: new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["manualReviewItemId"] = manualReviewItemId.ToString()
            },
            run: async () =>
            {
                var strategy = _dbContext.Database.CreateExecutionStrategy();
                return await strategy.ExecuteAsync(async () =>
                {
                    await using var tx = await _dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
                    try
                    {
                        var manualOperationId = await _dbContext.ReconciliationManualReviewItems
                            .AsNoTracking()
                            .Where(x => x.Id == manualReviewItemId)
                            .Select(x => (Guid?)x.ReconciliationOperationId)
                            .FirstOrDefaultAsync(cancellationToken);

                        if (!manualOperationId.HasValue)
                        {
                            await tx.RollbackAsync(cancellationToken);
                            return ManualReviewOperationResult.NotFound("Manual review item not found.");
                        }

                        var now = DateTime.Now;
                        var updatedRows = await _dbContext.ReconciliationManualReviewItems
                            .Where(x => x.Id == manualReviewItemId && x.ReviewStatus == ManualReviewStatus.Pending)
                            .ExecuteUpdateAsync(setters => setters
                                .SetProperty(x => x.ReviewStatus, ManualReviewStatus.Rejected)
                                .SetProperty(x => x.ReviewNote, note ?? string.Empty)
                                .SetProperty(x => x.ReviewedBy, actor)
                                .SetProperty(x => x.ReviewedAt, now)
                                .SetProperty(x => x.UpdateDate, now)
                                .SetProperty(x => x.LastModifiedBy, actor), cancellationToken);

                        if (updatedRows == 0)
                        {
                            await tx.RollbackAsync(cancellationToken);
                            return ManualReviewOperationResult.AlreadyReviewed("Manual review item is already processed.");
                        }

                        var operation = await _dbContext.ReconciliationOperations
                            .AsTracking()
                            .FirstOrDefaultAsync(x => x.Id == manualOperationId.Value, cancellationToken);
                        if (operation is null)
                        {
                            await tx.RollbackAsync(cancellationToken);
                            return ManualReviewOperationResult.OperationExecutionFailed("Reconciliation operation not found for manual review item.");
                        }

                        operation.Status = ReconciliationOperationStatus.Rejected;
                        operation.ErrorCode = ReconciliationErrorCodes.ManualRejected;
                        operation.ErrorText = note ?? "Manual review rejected.";
                        operation.CompletedAt = now;
                        operation.UpdateDate = now;
                        operation.LastModifiedBy = actor;
                        await CreateExecutionRecordAsync(
                            operation,
                            ReconciliationOperationExecutionOutcome.Failed,
                            actor,
                            ReconciliationErrorCodes.ManualRejected,
                            note ?? "Manual review rejected.");

                        var downstream = await _dbContext.ReconciliationOperations
                            .AsTracking()
                            .Where(x => x.CardTransactionRecordId == operation.CardTransactionRecordId && x.RunId == operation.RunId && x.OperationIndex > operation.OperationIndex &&
                                        (x.Status == ReconciliationOperationStatus.Pending || x.Status == ReconciliationOperationStatus.Blocked))
                            .ToListAsync(cancellationToken);
                        foreach (var item in downstream)
                        {
                            item.Status = ReconciliationOperationStatus.Skipped;
                            item.ErrorCode = ReconciliationErrorCodes.UpstreamRejected;
                            item.ErrorText = "Skipped because upstream manual operation was rejected.";
                            item.CompletedAt = now;
                            item.UpdateDate = now;
                            item.LastModifiedBy = actor;
                            await CreateExecutionRecordAsync(
                                item,
                                ReconciliationOperationExecutionOutcome.Skipped,
                                actor,
                                ReconciliationErrorCodes.UpstreamRejected,
                                "Skipped because upstream manual operation was rejected.");
                        }
        await UpdateCardStateAsync(
            operation.CardTransactionRecordId,
            CardReconciliationState.ReconcileFailed,
                            ReconciliationErrorCodes.ManualRejected,
            operation.RunId,
            operation.ExecutionGroupId,
            actor,
            now,
            cancellationToken);

                        await _dbContext.SaveChangesAsync(cancellationToken);
                        await tx.CommitAsync(cancellationToken);

                        await RaiseAlarmSafeAsync(
                            ReconciliationAlarmCodes.ReconciliationManualRejected,
                            "Manual reconciliation operation rejected.",
                            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                            {
                                ["manualReviewItemId"] = manualReviewItemId.ToString(),
                                ["cardTransactionRecordId"] = operation.CardTransactionRecordId.ToString(),
                                ["runId"] = operation.RunId.ToString(),
                                ["operationCode"] = operation.OperationCode
                            },
                            cancellationToken);

                        _logger.LogWarning("Manual review rejected. ManualReviewItemId={ManualReviewItemId}, RunId={RunId}, OperationCode={OperationCode}",
                            manualReviewItemId,
                            operation.RunId,
                            operation.OperationCode);
                        return ManualReviewOperationResult.Success();
                    }
                    catch
                    {
                        await tx.RollbackAsync(cancellationToken);
                        throw;
                    }
                });
            },
            isSuccess: result => result?.Status == ManualReviewOperationStatus.Success,
            cancellationToken);
    }

    private async Task RaiseAlarmSafeAsync(
        string alarmCode,
        string summary,
        IReadOnlyDictionary<string, string> metadata,
        CancellationToken cancellationToken)
    {
        try
        {
            await _alarmService.RaiseAsync(alarmCode, summary, metadata, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Manual operation alarm raise failed. AlarmCode={AlarmCode}", alarmCode);
        }
    }

    private async Task<T> RunWithBulkLogAsync<T>(
        string endpointName,
        string actor,
        Dictionary<string, string> metadata,
        Func<Task<T>> run,
        Func<T, bool> isSuccess,
        CancellationToken cancellationToken)
    {
        var startedAt = DateTime.Now;
        var steps = new List<string>();
        var success = false;
        T result = default;
        try
        {
            result = await run();
            success = isSuccess?.Invoke(result) ?? true;
            steps.Add($"ResultType={typeof(T).Name}");
            return result;
        }
        catch (Exception ex)
        {
            steps.Add($"Exception={ex.Message}");
            _logger.LogError(ex, "Manual operation endpoint failed. Endpoint={EndpointName}", endpointName);
            throw;
        }
        finally
        {
            var endedAt = DateTime.Now;
            await _bulkLogPublisher.PublishAsync(new BulkServiceOperationLog
            {
                ServiceName = nameof(ReconciliationManualOperationService),
                EndpointName = endpointName,
                Actor = string.IsNullOrWhiteSpace(actor) ? AuditUsers.CardFileIngestion : actor,
                StartedAt = startedAt,
                EndedAt = endedAt,
                DurationMs = (long)(endedAt - startedAt).TotalMilliseconds,
                IsSuccess = success,
                Summary = success ? "Manual operation completed." : "Manual operation failed.",
                Metadata = metadata ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase),
                Logs = steps
            }, cancellationToken);
        }
    }

    private static ReconciliationOperationStatus ResolveRunStatus(IReadOnlyCollection<ReconciliationOperation> operations)
    {
        if (operations.Any(x => x.Status == ReconciliationOperationStatus.Processing))
        {
            return ReconciliationOperationStatus.Processing;
        }

        if (operations.Any(x => x.Status == ReconciliationOperationStatus.Failed))
        {
            return ReconciliationOperationStatus.Failed;
        }

        if (operations.Any(x => x.Status == ReconciliationOperationStatus.Rejected))
        {
            return ReconciliationOperationStatus.Rejected;
        }

        if (operations.Any(x => x.Status is ReconciliationOperationStatus.Pending or ReconciliationOperationStatus.Blocked))
        {
            return ReconciliationOperationStatus.Pending;
        }

        if (operations.Any(x => x.Status == ReconciliationOperationStatus.Skipped))
        {
            return ReconciliationOperationStatus.Skipped;
        }

        return ReconciliationOperationStatus.Done;
    }

    private async Task UpdateCardStateFromRunAsync(
        Guid cardTransactionRecordId,
        Guid runId,
        string actor,
        CancellationToken cancellationToken)
    {
        var operations = await _dbContext.ReconciliationOperations
            .AsNoTracking()
            .Where(x => x.CardTransactionRecordId == cardTransactionRecordId && x.RunId == runId)
            .ToListAsync(cancellationToken);
        var executionGroupId = operations.Select(x => (Guid?)x.ExecutionGroupId).FirstOrDefault();
        var operationIds = operations.Select(x => x.Id).ToArray();
        var hasPendingManualReview = operationIds.Length > 0 && await _dbContext.ReconciliationManualReviewItems
            .AsNoTracking()
            .AnyAsync(x => operationIds.Contains(x.ReconciliationOperationId) && x.ReviewStatus == ManualReviewStatus.Pending, cancellationToken);

        var (state, reason) = CardReconciliationStateResolver.Resolve(operations, hasPendingManualReview);
        await UpdateCardStateAsync(cardTransactionRecordId, state, reason, runId, executionGroupId, actor, DateTime.Now, cancellationToken);
    }

    private async Task UpdateCardStateAsync(
        Guid cardTransactionRecordId,
        CardReconciliationState state,
        string reason,
        Guid? runId,
        Guid? executionGroupId,
        string actor,
        DateTime now,
        CancellationToken cancellationToken)
    {
        await _dbContext.CardTransactionRecords
            .Where(x => x.Id == cardTransactionRecordId)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(x => x.ReconciliationState, state)
                .SetProperty(x => x.ReconciliationStateReason, reason)
                .SetProperty(x => x.LastReconciliationRunId, runId)
                .SetProperty(x => x.LastReconciliationExecutionGroupId, executionGroupId)
                .SetProperty(x => x.ReconciliationStateUpdatedAt, now)
                .SetProperty(x => x.UpdateDate, now)
                .SetProperty(x => x.LastModifiedBy, actor), cancellationToken);
    }

    private async Task CreateExecutionRecordAsync(
        ReconciliationOperation operation,
        ReconciliationOperationExecutionOutcome outcome,
        string actor,
        string errorCode = null,
        string errorText = null)
    {
        var maxPersistedAttemptNo = await _dbContext.ReconciliationOperationExecutions
            .AsNoTracking()
            .Where(x => x.ReconciliationOperationId == operation.Id)
            .Select(x => (int?)x.AttemptNo)
            .MaxAsync() ?? 0;

        var maxTrackedAttemptNo = _dbContext.ChangeTracker.Entries<ReconciliationOperationExecution>()
            .Where(x => x.Entity.ReconciliationOperationId == operation.Id &&
                        x.State is EntityState.Added or EntityState.Modified or EntityState.Unchanged)
            .Select(x => (int?)x.Entity.AttemptNo)
            .DefaultIfEmpty(0)
            .Max() ?? 0;

        var now = DateTime.Now;
        _dbContext.ReconciliationOperationExecutions.Add(new ReconciliationOperationExecution
        {
            Id = Guid.NewGuid(),
            ReconciliationOperationId = operation.Id,
            ExecutionGroupId = operation.ExecutionGroupId,
            AttemptNo = Math.Max(maxPersistedAttemptNo, maxTrackedAttemptNo) + 1,
            StartedAt = operation.StartedAt ?? now,
            EndedAt = operation.CompletedAt ?? now,
            Outcome = outcome,
            ErrorCode = errorCode,
            ErrorText = errorText,
            RequestPayload = JsonSerializer.Serialize(new { operationCode = operation.OperationCode, source = "ReconciliationManualOperationService" }),
            ResponsePayload = JsonSerializer.Serialize("Operation state changed by manual review action."),
            CreateDate = now,
            CreatedBy = actor,
            UpdateDate = now,
            LastModifiedBy = actor,
            RecordStatus = RecordStatus.Active
        });
    }
}
