using System.Data;
using System.Text.Json;
using LinkPara.Card.Application.Commons.Exceptions;
using LinkPara.Card.Application.Commons.Extensions;
using LinkPara.Card.Application.Commons.Helpers;
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
                            throw new ReconciliationEvaluationContextNotBuiltException( _localizer.Get("Reconciliation.EvaluationContextNotBuilt", row.Id));
                        }

                        var evaluator = _evaluatorResolver.Resolve(context.RootFile.ContentType);
                        var result = await evaluator.EvaluateAsync(context, cancellationToken);
                        successfulEvaluations.Add(new SuccessfulEvaluationPersistence(row, result));
                    }
                    catch (Exception ex)
                    {
                        var exceptionDetail = ExceptionDetailHelper.BuildDetailMessage(ex, 2000);
                        errors.Add(_errorMapper.MapException(
                            ex, "EVALUATION_ROW", fileLineId: row.Id,
                            message: _localizer.Get("Reconciliation.EvaluationRowFailed", row.Id)));
                        await CreateFailedEvaluationAsync(row.Id, exceptionDetail, groupId, cancellationToken);
                        await MarkRowAsync(row.Id, ReconciliationStatus.Failed, exceptionDetail, cancellationToken);
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
            .Select(x => x.FileId)
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
                .Where(x => x.FileId == transactionFileId)
                .Where(x => x.LineType == "D")
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
            OperationCount = orderedOperations.Count
        };
        
        var operations = BuildOperations(row, evaluation.Id, groupId, orderedOperations);

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

            foreach (var statusGroup in items.GroupBy(x => x.Result.IsAwaitingClearing))
            {

                IReadOnlySet<Guid> redirectToReadyRowIds = new HashSet<Guid>();
                if (statusGroup.Key)
                {
                    redirectToReadyRowIds = await ResolveRowsWithLateArrivingClearingAsync(
                        statusGroup.Select(x => x.Row).ToList(),
                        cancellationToken);
                }

                foreach (var noteGroup in statusGroup.GroupBy(x => x.Result.Note))
                {
                    if (statusGroup.Key && redirectToReadyRowIds.Count > 0)
                    {
                        var redirectIds = noteGroup
                            .Where(x => redirectToReadyRowIds.Contains(x.Row.Id))
                            .Select(x => x.Row.Id).ToList();
                        var stayIds = noteGroup
                            .Where(x => !redirectToReadyRowIds.Contains(x.Row.Id))
                            .Select(x => x.Row.Id).ToList();

                        if (redirectIds.Count > 0)
                            await MarkRowsBatchAsync(redirectIds, ReconciliationStatus.Ready,
                                _localizer.Get("Reconciliation.LateClearingRequeuedAtFinalize"), cancellationToken);
                        if (stayIds.Count > 0)
                            await MarkRowsBatchAsync(stayIds, ReconciliationStatus.AwaitingClearing, noteGroup.Key, cancellationToken);
                    }
                    else
                    {
                        var targetStatus = statusGroup.Key
                            ? ReconciliationStatus.AwaitingClearing
                            : ReconciliationStatus.Success;
                        var ids = noteGroup.Select(x => x.Row.Id).ToList();
                        await MarkRowsBatchAsync(ids, targetStatus, noteGroup.Key, cancellationToken);
                    }
                }
            }

            return batch.Sum(x => x.Operations.Count);
        }
        catch
        {
            _dbContext.ChangeTracker.Clear();
            var createdOperationCount = 0;
            
            var awaitingItems = items.Where(x => x.Result.IsAwaitingClearing).ToList();
            IReadOnlySet<Guid> redirectToReadyRowIds = awaitingItems.Count > 0
                ? await ResolveRowsWithLateArrivingClearingAsync(
                    awaitingItems.Select(x => x.Row).ToList(),
                    cancellationToken)
                : new HashSet<Guid>();

            foreach (var item in items)
            {
                try
                {
                    var alreadyPersisted = await _dbContext.ReconciliationEvaluations
                        .AsNoTracking()
                        .AnyAsync(e => e.FileLineId == item.Row.Id && e.GroupId == groupId, cancellationToken);

                    var (targetStatus, targetMessage) = ResolveFallbackTargetStatus(item, redirectToReadyRowIds);

                    if (alreadyPersisted)
                    {
                        await MarkRowAsync(item.Row.Id, targetStatus, targetMessage, cancellationToken);
                        continue;
                    }

                    createdOperationCount += await PersistEvaluationAsync(item.Row, item.Result, groupId, cancellationToken);
                    await MarkRowAsync(item.Row.Id, targetStatus, targetMessage, cancellationToken);
                }
                catch (Exception ex)
                {
                    var exceptionDetail = ExceptionDetailHelper.BuildDetailMessage(ex, 2000);
                    errors.Add(_errorMapper.MapException(
                        ex, "EVALUATION_ROW", fileLineId: item.Row.Id,
                        message: _localizer.Get("Reconciliation.EvaluationRowFailed", item.Row.Id)));
                    await CreateFailedEvaluationAsync(item.Row.Id, exceptionDetail, groupId, cancellationToken);
                    await MarkRowAsync(item.Row.Id, ReconciliationStatus.Failed, exceptionDetail, cancellationToken);
                }
            }
            return createdOperationCount;
        }
    }

    private (ReconciliationStatus Status, string Message) ResolveFallbackTargetStatus(
        SuccessfulEvaluationPersistence item,
        IReadOnlySet<Guid> redirectToReadyRowIds)
    {
        if (item.Result.IsAwaitingClearing && redirectToReadyRowIds.Contains(item.Row.Id))
        {
            return (ReconciliationStatus.Ready,
                _localizer.Get("Reconciliation.LateClearingRequeuedAtFinalize"));
        }

        return (ResolveTargetStatus(item.Result), item.Result.Note);
    }

    private async Task<IReadOnlySet<Guid>> ResolveRowsWithLateArrivingClearingAsync(
        IReadOnlyList<IngestionFileLine> rows,
        CancellationToken cancellationToken)
    {
        var correlations = rows
            .Where(r => !string.IsNullOrWhiteSpace(r.CorrelationKey) && !string.IsNullOrWhiteSpace(r.CorrelationValue))
            .GroupBy(r => r.CorrelationKey!)
            .ToDictionary(g => g.Key, g => g.Select(r => r.CorrelationValue!).Distinct().ToArray());

        if (correlations.Count == 0)
        {
            return new HashSet<Guid>();
        }

        var matchedValues = new HashSet<(string Key, string Value)>();
        foreach (var pair in correlations)
        {
            var key = pair.Key;
            var values = pair.Value;
            var found = await _dbContext.IngestionFileLines
                .AsNoTracking()
                .Where(x => x.LineType == "D"
                            && x.CorrelationKey == key
                            && values.Contains(x.CorrelationValue)
                            && x.IngestionFile.FileType == FileType.Clearing
                            && x.IngestionFile.Status == FileStatus.Success)
                .Select(x => x.CorrelationValue!)
                .Distinct()
                .ToListAsync(cancellationToken);

            foreach (var v in found)
            {
                matchedValues.Add((key, v));
            }
        }

        if (matchedValues.Count == 0)
        {
            return new HashSet<Guid>();
        }

        return rows
            .Where(r => r.CorrelationKey is not null
                        && r.CorrelationValue is not null
                        && matchedValues.Contains((r.CorrelationKey!, r.CorrelationValue!)))
            .Select(r => r.Id)
            .ToHashSet();
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
            OperationCount = orderedOperations.Count
        };

        var operations = BuildOperations(row, evaluation.Id, groupId, orderedOperations);
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
                var definition = orderedOperations[x.SequenceNumber];
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
        IngestionFileLine row,
        Guid evaluationId,
        Guid groupId,
        IReadOnlyList<EvaluationOperation> orderedOperations)
    {
        var operations = new List<ReconciliationOperation>();
        foreach (var operation in orderedOperations)
        {
            var sequence = operations.Count;
            var parentSequenceNumber = ResolveParentSequenceIndex(operation, operations);
            var status = sequence == 0 ? OperationStatus.Planned : OperationStatus.Blocked;

            operations.Add(new ReconciliationOperation
            {
                Id = Guid.NewGuid(),
                FileLineId = row.Id,
                EvaluationId = evaluationId,
                GroupId = groupId,
                SequenceNumber = sequence,
                ParentSequenceNumber = parentSequenceNumber,
                Code = operation.Code,
                Note = operation.Note,
                Payload = JsonSerializer.Serialize(operation.Payload),
                IsManual = operation.IsManual,
                Branch = operation.Branch,
                Status = status,
                RetryCount = 0,
                MaxRetries = _options.Evaluate.OperationMaxRetries!.Value,
                IdempotencyKey = ResolveIdempotencyKey(row, operation)
            });
        }

        return operations;
    }


    private static string ResolveIdempotencyKey(
        IngestionFileLine row,
        EvaluationOperation operation)
    {
        var code = operation.Code;
        var branch = operation.Branch;
        var payload = operation.Payload;

        var transactionKey =
            GetPayloadValue(payload, code, "currentTransactionId")
            ?? GetPayloadValue(payload, code, "correlationValue")
            ?? row.CorrelationValue?.Trim();

        if (string.IsNullOrWhiteSpace(transactionKey))
        {
            transactionKey = $"LINE:{row.Id:N}";
        }

        var parts = new List<string> { "TXN", code, branch, transactionKey };

        var referenceTransactionId = GetPayloadValue(payload, code, "referenceTransactionId");
        if (!string.IsNullOrWhiteSpace(referenceTransactionId)
            && !string.Equals(referenceTransactionId, transactionKey, StringComparison.Ordinal))
        {
            parts.Add($"ref:{referenceTransactionId}");
        }

        var differenceAmount = GetPayloadValue(payload, code, "differenceAmount");
        if (!string.IsNullOrWhiteSpace(differenceAmount))
        {
            parts.Add($"diff:{differenceAmount}");
        }

        var originalTransactionId = GetPayloadValue(payload, code, "originalTransactionId");
        if (!string.IsNullOrWhiteSpace(originalTransactionId)
            && !string.Equals(originalTransactionId, transactionKey, StringComparison.Ordinal)
            && !string.Equals(originalTransactionId, referenceTransactionId, StringComparison.Ordinal))
        {
            parts.Add($"orig:{originalTransactionId}");
        }

        return string.Join(":", parts);
    }

    private static ReconciliationStatus ResolveTargetStatus(EvaluationResult result)
        => result.IsAwaitingClearing
            ? ReconciliationStatus.AwaitingClearing
            : ReconciliationStatus.Success;

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

        return operations[^1].SequenceNumber;
    }

    private int FindRequiredManualGateSequence(IReadOnlyList<ReconciliationOperation> operations)
    {
        var manualGateSequence = operations.LastOrDefault(x => x.IsManual)?.SequenceNumber;
        if (manualGateSequence.HasValue) return manualGateSequence.Value;
        throw new ReconciliationBranchRequiresManualGateException(_localizer.Get("Reconciliation.BranchRequiresManualGate"));
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
            OperationCount = 0
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

            var exceptionDetail = ExceptionDetailHelper.BuildDetailMessage(ex, 2000);

            var evaluation = new ReconciliationEvaluation
            {
                Id = Guid.NewGuid(),
                GroupId = groupId,
                FileLineId = row.Id,
                Status = EvaluationStatus.Failed,
                Message = exceptionDetail,
                OperationCount = 0
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
                Message = exceptionDetail
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
