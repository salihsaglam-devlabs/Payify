using System.Data;
using System.Text.Json;
using LinkPara.Card.Application.Commons.Exceptions;
using LinkPara.Card.Application.Commons.Extensions;
using LinkPara.Card.Application.Commons.Interfaces;
using LinkPara.Card.Application.Commons.Interfaces.Reconciliation;
using LinkPara.Card.Application.Commons.Models.Reconciliation.Configuration;
using LinkPara.Card.Application.Commons.Models.Reconciliation.Contracts.Requests;
using LinkPara.Card.Application.Commons.Models.Reconciliation.Contracts.Responses;
using LinkPara.Card.Application.Commons.Models.Reconciliation.Shared;
using LinkPara.Card.Domain.Entities.FileIngestion.Persistence;
using LinkPara.Card.Domain.Entities.Reconciliation.Persistence;
using LinkPara.Card.Domain.Enums.FileIngestion;
using LinkPara.Card.Domain.Enums.Reconciliation;
using LinkPara.Card.Infrastructure.Persistence;
using LinkPara.Card.Infrastructure.Services.Audit;
using LinkPara.SharedModels.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;

namespace LinkPara.Card.Infrastructure.Services.Reconciliation.Evaluate.Core;

internal sealed class EvaluateService : IEvaluateService
{
    private const string ClaimMarkerPrefix = "EVAL_CLAIM:";
    private readonly CardDbContext _dbContext;
    private readonly IContextBuilder _contextBuilder;
    private readonly EvaluatorResolver _evaluatorResolver;
    private readonly IAuditStampService _auditStampService;
    private readonly IReconciliationErrorMapper _errorMapper;
    private readonly IStringLocalizer _localizer;
    private readonly ITimeProvider _timeProvider;
    private readonly ReconciliationOptions _options = new();

    public EvaluateService(
        CardDbContext dbContext,
        IContextBuilder contextBuilder,
        EvaluatorResolver evaluatorResolver,
        IAuditStampService auditStampService,
        ITimeProvider timeProvider,
        IOptions<ReconciliationOptions> options,
        IReconciliationErrorMapper errorMapper,
        Func<LinkPara.Card.Application.Commons.Localization.LocalizerResource, IStringLocalizer> localizerFactory)
    {
        _dbContext = dbContext;
        _contextBuilder = contextBuilder;
        _evaluatorResolver = evaluatorResolver;
        _auditStampService = auditStampService;
        _timeProvider = timeProvider;
        _options = options.Value;
        _errorMapper = errorMapper;
        _localizer = localizerFactory(LinkPara.Card.Application.Commons.Localization.LocalizerResource.Messages);
    }

    public async Task<EvaluateResponse> EvaluateAsync(
        EvaluateRequest request,
        List<ReconciliationErrorDetail>? errors = null,
        CancellationToken cancellationToken = default)
    {
        errors ??= new List<ReconciliationErrorDetail>();
        var runId = Guid.NewGuid();
        var groupId = runId;
        var chunkSize = ResolveChunkSize(request);
        var createdOperationCount = 0;
        var skippedCount = 0;
        var fileIds = await ResolveTargetFileIdsAsync(request, cancellationToken);

        foreach (var fileId in fileIds)
        {
            while (true)
            {
                var rows = await ClaimReadyChunkAsync(fileId, chunkSize, cancellationToken);
                if (rows.Count == 0) break;

                IReadOnlyDictionary<Guid, EvaluationContext> contextMap;
                try
                {
                    contextMap = await _contextBuilder.BuildManyAsync(rows, errors, cancellationToken);
                    await RefreshClaimAsync(rows.Select(x => x.Id).ToList(), cancellationToken);
                }
                catch (Exception ex)
                {
                    await MarkChunkFailedAsync(rows, ex, errors, groupId, cancellationToken);
                    continue;
                }

                var successfulEvaluations = new List<SuccessfulEvaluationPersistence>();

                foreach (var row in rows)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    try
                    {
                        await RefreshClaimAsync(new List<Guid> { row.Id }, cancellationToken);

                        if (!contextMap.TryGetValue(row.Id, out var context))
                        {
                            throw new ReconciliationContextException(ApiErrorCode.ReconciliationEvaluationContextNotBuilt, _localizer.Get("Reconciliation.EvaluationContextNotBuilt", row.Id));
                        }

                        var evaluator = _evaluatorResolver.Resolve(context.RootFile.ContentType);
                        var result = await evaluator.EvaluateAsync(context, cancellationToken);
                        successfulEvaluations.Add(new SuccessfulEvaluationPersistence(row, result));
                    }
                    catch (Exception ex)
                    {
                        errors.Add(_errorMapper.MapException(
                            ex, "EVALUATION_ROW", fileLineId: row.Id,
                            message: _localizer.Get("Reconciliation.EvaluationRowFailed", row.Id)));
                        await CreateFailedEvaluationAsync(row.Id, ex.Message, groupId, cancellationToken);
                        await MarkRowAsync(row.Id, ReconciliationStatus.Failed, ex.Message, cancellationToken);
                        skippedCount++;
                    }
                }

                createdOperationCount += await PersistSuccessfulEvaluationsAsync(successfulEvaluations, groupId, errors, cancellationToken);
            }
        }

        return CreateResponse(runId, fileIds.Length, createdOperationCount, skippedCount, errors);
    }

    private async Task<Guid[]> ResolveTargetFileIdsAsync(EvaluateRequest request, CancellationToken cancellationToken)
    {
        var requested = request.IngestionFileIds?.Where(x => x != Guid.Empty).Distinct().ToArray() ?? [];
        if (requested.Length > 0)
        {
            return requested;
        }

        return await _dbContext.IngestionFileLines
            .AsNoTracking()
            .Where(x => x.ReconciliationStatus == ReconciliationStatus.Ready)
            .Select(x => x.IngestionFileId)
            .Distinct()
            .ToArrayAsync(cancellationToken);
    }

    private async Task<List<IngestionFileLine>> ClaimReadyChunkAsync(
        Guid transactionFileId,
        int chunkSize,
        CancellationToken cancellationToken)
    {
        var maxAttempts = Math.Max(1, _options.Evaluate.ClaimRetryCount.Value);

        for (var attempt = 0; attempt < maxAttempts; attempt++)
        {
            try
            {
                return await ClaimReadyChunkCoreAsync(transactionFileId, chunkSize, cancellationToken);
            }
            catch (DbUpdateException) when (attempt < maxAttempts - 1)
            {
            }
            catch (InvalidOperationException) when (attempt < maxAttempts - 1)
            {
            }
        }

        return await ClaimReadyChunkCoreAsync(transactionFileId, chunkSize, cancellationToken);
    }

    private async Task<List<IngestionFileLine>> ClaimReadyChunkCoreAsync(
        Guid transactionFileId,
        int chunkSize,
        CancellationToken cancellationToken)
    {
        var auditStamp = _auditStampService.CreateStamp();
        var claimMarker = $"{ClaimMarkerPrefix}{Guid.NewGuid():N}";
        var claimTimeout = TimeSpan.FromSeconds(Math.Max(30, _options.Evaluate.ClaimTimeoutSeconds.Value));
        var staleCutoff = auditStamp.Timestamp.Add(-claimTimeout);

        var strategy = _dbContext.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);

            var candidateIds = await _dbContext.IngestionFileLines
                .AsNoTracking()
                .Where(x => x.IngestionFileId == transactionFileId)
                .Where(x => x.RecordType == "D")
                .Where(x =>
                    x.ReconciliationStatus == ReconciliationStatus.Ready ||
                    (x.ReconciliationStatus == ReconciliationStatus.Processing &&
                     x.UpdateDate.HasValue &&
                     x.UpdateDate.Value <= staleCutoff))
                .OrderBy(x => x.LineNumber)
                .ThenBy(x => x.Id)
                .Select(x => x.Id)
                .Take(chunkSize)
                .ToListAsync(cancellationToken);

            if (candidateIds.Count == 0)
            {
                await transaction.CommitAsync(cancellationToken);
                return [];
            }

            var claimedCount = await _dbContext.IngestionFileLines
                .Where(x => candidateIds.Contains(x.Id))
                .Where(x =>
                    x.ReconciliationStatus == ReconciliationStatus.Ready ||
                    (x.ReconciliationStatus == ReconciliationStatus.Processing &&
                     x.UpdateDate.HasValue &&
                     x.UpdateDate.Value <= staleCutoff))
                .ExecuteUpdateAsync(update => update
                    .SetProperty(x => x.ReconciliationStatus, ReconciliationStatus.Processing)
                    .SetProperty(x => x.Message, claimMarker)
                    .SetProperty(x => x.UpdateDate, auditStamp.Timestamp)
                    .SetProperty(x => x.LastModifiedBy, auditStamp.UserId),
                    cancellationToken);

            if (claimedCount == 0)
            {
                await transaction.CommitAsync(cancellationToken);
                return [];
            }

            var rows = await _dbContext.IngestionFileLines
                .AsNoTracking()
                .Include(x => x.IngestionFile)
                .Where(x => candidateIds.Contains(x.Id))
                .Where(x => x.ReconciliationStatus == ReconciliationStatus.Processing && x.Message == claimMarker)
                .OrderBy(x => x.LineNumber)
                .ThenBy(x => x.Id)
                .ToListAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);
            return rows;
        });
    }

    private async Task<int> PersistEvaluationAsync(
        IngestionFileLine row,
        EvaluationResult result,
        Guid groupId,
        CancellationToken cancellationToken)
    {
        var orderedOperations = result.Operations.OrderBy(x => x.Order).ToList();
        var evaluation = new ReconciliationEvaluation
        {
            Id = Guid.NewGuid(),
            FileLineId = row.Id,
            GroupId = groupId,
            Status = EvaluationStatus.Completed,
            Message = result.Note,
            CreatedOperationCount = orderedOperations.Count
        };
        
        var operations = BuildOperations(row.Id, evaluation.Id, groupId, orderedOperations);

        var reviews = BuildReviews(operations, orderedOperations, row.Id, evaluation.Id, groupId);
        _auditStampService.StampForCreate(evaluation);
        _auditStampService.StampForCreate(operations.Cast<AuditEntity>());
        _auditStampService.StampForCreate(reviews.Cast<AuditEntity>());

        await ExecuteInTransactionAsync(async () =>
        {
            await _dbContext.ReconciliationEvaluations.AddAsync(evaluation, cancellationToken);
            if (operations.Count > 0)
            {
                await _dbContext.ReconciliationOperations.AddRangeAsync(operations, cancellationToken);
            }

            if (reviews.Count > 0)
            {
                await _dbContext.ReconciliationReviews.AddRangeAsync(reviews, cancellationToken);
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
        }, cancellationToken);

        return operations.Count;
    }

    private async Task<int> PersistSuccessfulEvaluationsAsync(
        IReadOnlyList<SuccessfulEvaluationPersistence> items,
        Guid groupId,
        List<ReconciliationErrorDetail> errors,
        CancellationToken cancellationToken)
    {
        if (items.Count == 0) return 0;

        try
        {
            var batch = items.Select(item => CreatePersistedEvaluation(item.Row, item.Result, groupId)).ToList();
            await ExecuteInTransactionAsync(async () =>
            {
                await _dbContext.ReconciliationEvaluations.AddRangeAsync(batch.Select(x => x.Evaluation), cancellationToken);
                var operations = batch.SelectMany(x => x.Operations).ToList();
                if (operations.Count > 0)
                    await _dbContext.ReconciliationOperations.AddRangeAsync(operations, cancellationToken);
                var reviews = batch.SelectMany(x => x.Reviews).ToList();
                if (reviews.Count > 0)
                    await _dbContext.ReconciliationReviews.AddRangeAsync(reviews, cancellationToken);
                await _dbContext.SaveChangesAsync(cancellationToken);
            }, cancellationToken);

            foreach (var noteGroup in items.GroupBy(x => x.Result.Note))
            {
                var ids = noteGroup.Select(x => x.Row.Id).ToList();
                await MarkRowsBatchAsync(ids, ReconciliationStatus.Success, noteGroup.Key, cancellationToken);
            }

            return batch.Sum(x => x.Operations.Count);
        }
        catch
        {
            _dbContext.ChangeTracker.Clear();
            var createdOperationCount = 0;
            foreach (var item in items)
            {
                try
                {
                    var alreadyPersisted = await _dbContext.ReconciliationEvaluations
                        .AsNoTracking()
                        .AnyAsync(e => e.FileLineId == item.Row.Id && e.GroupId == groupId, cancellationToken);

                    if (alreadyPersisted)
                    {
                        await MarkRowAsync(item.Row.Id, ReconciliationStatus.Success, item.Result.Note, cancellationToken);
                        continue;
                    }

                    createdOperationCount += await PersistEvaluationAsync(item.Row, item.Result, groupId, cancellationToken);
                    await MarkRowAsync(item.Row.Id, ReconciliationStatus.Success, item.Result.Note, cancellationToken);
                }
                catch (Exception ex)
                {
                    errors.Add(_errorMapper.MapException(
                        ex, "EVALUATION_ROW", fileLineId: item.Row.Id,
                        message: _localizer.Get("Reconciliation.EvaluationRowFailed", item.Row.Id)));
                    await CreateFailedEvaluationAsync(item.Row.Id, ex.Message, groupId, cancellationToken);
                    await MarkRowAsync(item.Row.Id, ReconciliationStatus.Failed, ex.Message, cancellationToken);
                }
            }
            return createdOperationCount;
        }
    }

    private PersistedEvaluation CreatePersistedEvaluation(
        IngestionFileLine row,
        EvaluationResult result,
        Guid groupId)
    {
        var orderedOperations = result.Operations.OrderBy(x => x.Order).ToList();
        var evaluation = new ReconciliationEvaluation
        {
            Id = Guid.NewGuid(),
            FileLineId = row.Id,
            GroupId = groupId,
            Status = EvaluationStatus.Completed,
            Message = result.Note,
            CreatedOperationCount = orderedOperations.Count
        };

        var operations = BuildOperations(row.Id, evaluation.Id, groupId, orderedOperations);
        var reviews = BuildReviews(operations, orderedOperations, row.Id, evaluation.Id, groupId);

        _auditStampService.StampForCreate(evaluation);
        _auditStampService.StampForCreate(operations.Cast<AuditEntity>());
        _auditStampService.StampForCreate(reviews.Cast<AuditEntity>());

        return new PersistedEvaluation(evaluation, operations, reviews);
    }

    private List<ReconciliationReview> BuildReviews(
        IReadOnlyList<ReconciliationOperation> operations,
        IReadOnlyList<EvaluationOperation> orderedOperations,
        Guid fileLineId,
        Guid evaluationId,
        Guid groupId)
    {
        return operations
            .Where(x => x.IsManual)
            .Select(x =>
            {
                var definition = orderedOperations[x.SequenceIndex];
                return new ReconciliationReview
                {
                    Id = Guid.NewGuid(),
                    FileLineId = fileLineId,
                    GroupId = groupId,
                    EvaluationId = evaluationId,
                    OperationId = x.Id,
                    Decision = ReviewDecision.Pending,
                    ExpiresAt = ResolveReviewExpirationAt(definition),
                    ExpirationAction = ResolveExpirationAction(definition),
                    ExpirationFlowAction = ResolveExpirationFlowAction(definition)
                };
            })
            .ToList();
    }

    private List<ReconciliationOperation> BuildOperations(
        Guid transactionFileDataId,
        Guid evaluationId,
        Guid groupId,
        IReadOnlyList<EvaluationOperation> orderedOperations)
    {
        var operations = new List<ReconciliationOperation>();
        foreach (var operation in orderedOperations)
        {
            var sequence = operations.Count;
            var parentSequenceIndex = ResolveParentSequenceIndex(operation, operations);
            var status = sequence == 0 ? OperationStatus.Planned : OperationStatus.Blocked;

            operations.Add(new ReconciliationOperation
            {
                Id = Guid.NewGuid(),
                FileLineId = transactionFileDataId,
                EvaluationId = evaluationId,
                GroupId = groupId,
                SequenceIndex = sequence,
                ParentSequenceIndex = parentSequenceIndex,
                Code = operation.Code,
                Note = operation.Note,
                Payload = JsonSerializer.Serialize(operation.Payload),
                IsManual = operation.IsManual,
                Branch = operation.Branch,
                Status = status,
                RetryCount = 0,
                MaxRetries = 5,
                IdempotencyKey = ResolveIdempotencyKey(transactionFileDataId, sequence, operation)
            });
        }

        return operations;
    }
    

    private static string ResolveIdempotencyKey(
        Guid fileLineId,
        int sequenceIndex,
        EvaluationOperation operation)
    {
        var prefix = $"{operation.Code}:{fileLineId:N}:{sequenceIndex}";
        var correlationValue = GetPayloadValue(operation.Payload, operation.Code, "correlationValue");
        if (!string.IsNullOrWhiteSpace(correlationValue))
        {
            var differenceAmount = GetPayloadValue(operation.Payload, operation.Code, "differenceAmount");
            if (!string.IsNullOrWhiteSpace(differenceAmount))
            {
                return $"{prefix}:{correlationValue}:{differenceAmount}";
            }

            var originalTransactionId = GetPayloadValue(operation.Payload, operation.Code, "originalTransactionId");
            if (!string.IsNullOrWhiteSpace(originalTransactionId))
            {
                return $"{prefix}:{originalTransactionId}";
            }

            return $"{prefix}:{correlationValue}:{operation.Branch}";
        }

        return $"{prefix}:{operation.Branch}";
    }

    private int? ResolveParentSequenceIndex(
        EvaluationOperation operation,
        IReadOnlyList<ReconciliationOperation> operations)
    {
        if (operations.Count == 0)
        {
            return null;
        }

        if (operation.Branch is Branches.Approve or Branches.Reject)
        {
            return FindRequiredManualGateSequence(operations);
        }

        return operations[^1].SequenceIndex;
    }

    private int FindRequiredManualGateSequence(IReadOnlyList<ReconciliationOperation> operations)
    {
        var manualGateSequence = operations.LastOrDefault(x => x.IsManual)?.SequenceIndex;
        if (manualGateSequence.HasValue) return manualGateSequence.Value;
        throw new ReconciliationBusinessRuleException(ApiErrorCode.ReconciliationBranchRequiresManualGate, _localizer.Get("Reconciliation.BranchRequiresManualGate"));
    }

    private DateTime? ResolveReviewExpirationAt(EvaluationOperation operation)
    {
        return operation.ReviewTimeout.HasValue
            ? _timeProvider.Now.Add(operation.ReviewTimeout.Value)
            : null;
    }

    private static ReviewExpirationAction ResolveExpirationAction(EvaluationOperation operation)
    {
        return operation.ExpirationAction ?? ReviewExpirationAction.Cancel;
    }

    private static ReviewExpirationFlowAction ResolveExpirationFlowAction(EvaluationOperation operation)
    {
        return operation.ExpirationFlowAction ?? ReviewExpirationFlowAction.Continue;
    }

    private static string? GetPayloadValue(
        Dictionary<string, List<OperationPayloadItem>> payload,
        string group,
        string key)
    {
        if (!payload.TryGetValue(group, out var items))
        {
            return null;
        }

        return items.FirstOrDefault(x => x.Key == key)?.Value;
    }

    private async Task CreateFailedEvaluationAsync(Guid rowId, string error, Guid groupId, CancellationToken cancellationToken)
    {
        var evaluation = new ReconciliationEvaluation
        {
            Id = Guid.NewGuid(),
            GroupId = groupId,
            FileLineId = rowId,
            Status = EvaluationStatus.Failed,
            Message = error,
            CreatedOperationCount = 0
        };

        var alert = new ReconciliationAlert
        {
            Id = Guid.NewGuid(),
            GroupId = groupId,
            FileLineId = rowId,
            EvaluationId = evaluation.Id,
            OperationId = Guid.Empty,
            Severity = "Error",
            AlertType = "EvaluationFailed",
            Message = error
        };
        _auditStampService.StampForCreate(new AuditEntity[] { evaluation, alert });
        
        await ExecuteInTransactionAsync(async () =>
        {
            await _dbContext.ReconciliationEvaluations.AddAsync(evaluation, cancellationToken);
            await _dbContext.ReconciliationAlerts.AddAsync(alert, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }, cancellationToken);
    }

    private async Task MarkChunkFailedAsync(
        IReadOnlyCollection<IngestionFileLine> rows,
        Exception ex,
        List<ReconciliationErrorDetail> errors,
        Guid groupId,
        CancellationToken cancellationToken)
    {
        var evaluations = new List<ReconciliationEvaluation>();
        var alerts = new List<ReconciliationAlert>();

        foreach (var row in rows)
        {
            errors.Add(_errorMapper.MapException(
                ex, "EVALUATION_CHUNK", fileLineId: row.Id,
                message: _localizer.Get("Reconciliation.EvaluationChunkFailed", row.Id)));

            var evaluation = new ReconciliationEvaluation
            {
                Id = Guid.NewGuid(),
                GroupId = groupId,
                FileLineId = row.Id,
                Status = EvaluationStatus.Failed,
                Message = ex.Message,
                CreatedOperationCount = 0
            };

            alerts.Add(new ReconciliationAlert
            {
                Id = Guid.NewGuid(),
                GroupId = groupId,
                FileLineId = row.Id,
                EvaluationId = evaluation.Id,
                OperationId = Guid.Empty,
                Severity = "Error",
                AlertType = "EvaluationFailed",
                Message = ex.Message
            });

            evaluations.Add(evaluation);
        }

        _auditStampService.StampForCreate(evaluations.Cast<AuditEntity>().Concat(alerts.Cast<AuditEntity>()));
        await ExecuteInTransactionAsync(async () =>
        {
            await _dbContext.ReconciliationEvaluations.AddRangeAsync(evaluations, cancellationToken);
            await _dbContext.ReconciliationAlerts.AddRangeAsync(alerts, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }, cancellationToken);

        var rowIds = rows.Select(x => x.Id).ToList();
        await MarkRowsBatchAsync(rowIds, ReconciliationStatus.Failed, ex.Message, cancellationToken);
    }

    private async Task MarkRowAsync(
        Guid rowId,
        ReconciliationStatus status,
        string? message,
        CancellationToken cancellationToken)
    {
        var auditStamp = _auditStampService.CreateStamp();
        await _dbContext.IngestionFileLines
            .Where(x => x.Id == rowId)
            .ExecuteUpdateAsync(update => update
                .SetProperty(x => x.ReconciliationStatus, status)
                .SetProperty(x => x.Message, message)
                .SetProperty(x => x.UpdateDate, auditStamp.Timestamp)
                .SetProperty(x => x.LastModifiedBy, auditStamp.UserId),
                cancellationToken);
    }

    private async Task MarkRowsBatchAsync(
        List<Guid> rowIds,
        ReconciliationStatus status,
        string? message,
        CancellationToken cancellationToken)
    {
        if (rowIds.Count == 0)
            return;

        var auditStamp = _auditStampService.CreateStamp();
        await _dbContext.IngestionFileLines
            .Where(x => rowIds.Contains(x.Id))
            .ExecuteUpdateAsync(update => update
                .SetProperty(x => x.ReconciliationStatus, status)
                .SetProperty(x => x.Message, message)
                .SetProperty(x => x.UpdateDate, auditStamp.Timestamp)
                .SetProperty(x => x.LastModifiedBy, auditStamp.UserId),
                cancellationToken);
    }

    private async Task RefreshClaimAsync(
        List<Guid> rowIds,
        CancellationToken cancellationToken)
    {
        if (rowIds.Count == 0)
            return;

        var auditStamp = _auditStampService.CreateStamp();
        await _dbContext.IngestionFileLines
            .Where(x => rowIds.Contains(x.Id))
            .Where(x => x.ReconciliationStatus == ReconciliationStatus.Processing)
            .Where(x => x.Message != null && x.Message.StartsWith(ClaimMarkerPrefix))
            .ExecuteUpdateAsync(update => update
                .SetProperty(x => x.UpdateDate, auditStamp.Timestamp)
                .SetProperty(x => x.LastModifiedBy, auditStamp.UserId),
                cancellationToken);
    }

    private int ResolveChunkSize(EvaluateRequest request)
    {
        return Math.Clamp(request.Options?.ChunkSize ?? _options.Evaluate.ChunkSize.Value, 100, 10_000);
    }

    private EvaluateResponse CreateResponse(
        Guid runId, int fileCount, int createdOperationCount, int skippedCount,
        List<ReconciliationErrorDetail> errors)
    {
        return new EvaluateResponse
        {
            EvaluationRunId = runId,
            CreatedOperationsCount = createdOperationCount,
            SkippedCount = skippedCount,
            Message = errors.Count == 0
                ? _localizer.Get("Reconciliation.EvaluationCompletedSuccess", fileCount)
                : _localizer.Get("Reconciliation.EvaluationCompletedWithErrors", errors.Count, fileCount),
            ErrorCount = errors.Count,
            Errors = errors
        };
    }

    private async Task ExecuteInTransactionAsync(
        Func<Task> action,
        CancellationToken cancellationToken)
    {
        var strategy = _dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                await action();
                await transaction.CommitAsync(cancellationToken);
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        });
    }

    private sealed record SuccessfulEvaluationPersistence(IngestionFileLine Row, EvaluationResult Result);

    private sealed record PersistedEvaluation(
        ReconciliationEvaluation Evaluation,
        List<ReconciliationOperation> Operations,
        List<ReconciliationReview> Reviews);
}
