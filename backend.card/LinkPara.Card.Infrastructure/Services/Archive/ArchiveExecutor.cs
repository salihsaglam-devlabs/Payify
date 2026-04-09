using System.Text.Json;
using LinkPara.Card.Application.Commons.Models.Archive;
using LinkPara.Card.Domain.Entities.Archive;
using LinkPara.Card.Domain.Entities.FileIngestion;
using LinkPara.Card.Domain.Entities.Reconciliation;
using LinkPara.Card.Infrastructure.Persistence;
using LinkPara.Card.Infrastructure.Services.Audit;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.Card.Infrastructure.Services.Archive;

internal sealed class ArchiveExecutor
{
    private readonly CardDbContext _dbContext;
    private readonly ArchiveAggregateReader _reader;
    private readonly ArchiveEligibilityEvaluator _evaluator;
    private readonly IAuditStampService _auditStampService;

    public ArchiveExecutor(
        CardDbContext dbContext,
        ArchiveAggregateReader reader,
        ArchiveEligibilityEvaluator evaluator,
        ArchiveVerifier verifier,
        IArchiveSqlDialect sqlDialect,
        IAuditStampService auditStampService)
    {
        _dbContext = dbContext;
        _reader = reader;
        _evaluator = evaluator;
        _auditStampService = auditStampService;
    }

    public async Task<ArchiveRunItemResult> ExecuteAsync(Guid ingestionFileId, CancellationToken cancellationToken)
    {
        var auditStamp = _auditStampService.CreateStamp();
        var archiveRunId = Guid.NewGuid();

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var snapshot = await _reader.GetSnapshotAsync(ingestionFileId, cancellationToken);
            var eligibility = _evaluator.Evaluate(snapshot, auditStamp.Timestamp);
            if (!eligibility.IsEligible || snapshot is null)
            {
                await transaction.RollbackAsync(cancellationToken);
                return new ArchiveRunItemResult
                {
                    AggregateId = ingestionFileId,
                    Status = "Skipped",
                    Message = snapshot is null
                        ? "Snapshot not found for the given ingestion file."
                        : $"Eligibility check failed: {string.Join(", ", eligibility.FailureReasons)}",
                    FailureReasons = snapshot is null
                        ? new List<string> { "SNAPSHOT_NOT_FOUND" }
                        : eligibility.FailureReasons
                };
            }

            await BackfillLegacyArchiveStateAsync(snapshot, archiveRunId, auditStamp, cancellationToken);

            var eligibleItemIds = snapshot.ItemSnapshots
                .Where(x => x.IsEligible)
                .Select(x => x.FileLineId)
                .ToArray();

            var file = await _dbContext.IngestionFiles
                .AsTracking()
                .SingleAsync(x => x.Id == ingestionFileId, cancellationToken);

            var actions = new List<string>();

            var requiresArchiveFileRecord =
                !file.ArchiveRecordWrittenAt.HasValue &&
                (eligibleItemIds.Length > 0 ||
                 snapshot.ItemSnapshots.Any(x => x.IsArchived) ||
                 snapshot.AtomicItems.TotalCount == 0);

            if (requiresArchiveFileRecord)
            {
                file.ArchiveRecordWrittenAt = auditStamp.Timestamp;
                file.ArchiveRecordRunId = archiveRunId;
                await EnsureArchiveFileRecordAsync(file, auditStamp, archiveRunId, cancellationToken);
                actions.Add("FILE_ARCHIVE_RECORD_READY");
            }

            if (eligibleItemIds.Length > 0)
            {
                await ArchiveAtomicItemsAsync(file, eligibleItemIds, auditStamp, archiveRunId, cancellationToken);
                actions.Add($"ATOMIC_ITEMS_ARCHIVED:{eligibleItemIds.Length}");
            }

            await RefreshFileLifecycleAsync(file.Id, archiveRunId, auditStamp, cancellationToken);

            var refreshed = await _dbContext.IngestionFiles
                .AsNoTracking()
                .Where(x => x.Id == file.Id)
                .Select(x => new
                {
                    x.Id,
                    x.ArchiveRecordWrittenAt,
                    x.ArchiveChildrenTransitionedAt,
                    x.ArchiveCleanupEligibleAt
                })
                .SingleAsync(cancellationToken);

            if (refreshed.ArchiveCleanupEligibleAt.HasValue)
            {
                await CleanupLiveAggregateAsync(file.Id, auditStamp, archiveRunId, cancellationToken);
                actions.Add("LIVE_AGGREGATE_CLEANED");
            }

            await transaction.CommitAsync(cancellationToken);

            return new ArchiveRunItemResult
            {
                AggregateId = ingestionFileId,
                Status = actions.Count == 0 ? "Skipped" : "Archived",
                ArchiveRunId = archiveRunId,
                Message = actions.Count == 0 ? "No archive or cleanup action was required." : string.Join(", ", actions)
            };
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);

            return new ArchiveRunItemResult
            {
                AggregateId = ingestionFileId,
                Status = "Failed",
                Message = BuildSafeMessage(ex),
                FailureReasons = new List<string> { "EXECUTION_ERROR" }
            };
        }
    }

    public async Task<Guid> CreateBatchAsync(ArchiveRunRequest? request, CancellationToken cancellationToken)
    {
        var stamp = _auditStampService.CreateStamp();
        var batch = new ArchiveBatch
        {
            Id = Guid.NewGuid(),
            RequestedAt = stamp.Timestamp,
            StartedAt = stamp.Timestamp,
            RequestedBy = stamp.UserId,
            FilterJson = JsonSerializer.Serialize(request ?? new ArchiveRunRequest()),
            Status = "Running",
            ProcessedCount = 0,
            ArchivedCount = 0,
            SkippedCount = 0,
            FailedCount = 0
        };

        _auditStampService.StampForCreate(batch);
        await _dbContext.ArchiveBatches.AddAsync(batch, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return batch.Id;
    }

    public async Task InsertBatchItemAsync(Guid batchId, ArchiveRunItemResult item, CancellationToken cancellationToken)
    {
        var batchItem = new ArchiveBatchItem
        {
            Id = Guid.NewGuid(),
            BatchId = batchId,
            IngestionFileId = item.AggregateId,
            ArchiveRunId = item.ArchiveRunId,
            Status = item.Status,
            Message = item.Message,
            FailureReasonsJson = JsonSerializer.Serialize(item.FailureReasons ?? new List<string>()),
            ProcessedAt = DateTime.UtcNow
        };

        _auditStampService.StampForCreate(batchItem);
        await _dbContext.ArchiveBatchItems.AddAsync(batchItem, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task CompleteBatchAsync(Guid batchId, ArchiveRunResponse response, CancellationToken cancellationToken)
    {
        var batch = await _dbContext.ArchiveBatches.SingleAsync(x => x.Id == batchId, cancellationToken);
        batch.CompletedAt = DateTime.UtcNow;
        batch.Status = DeriveBatchStatus(response);
        batch.ProcessedCount = response.ProcessedCount;
        batch.ArchivedCount = response.ArchivedCount;
        batch.SkippedCount = response.SkippedCount;
        batch.FailedCount = response.FailedCount;
        _auditStampService.StampForUpdate(batch);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static string DeriveBatchStatus(ArchiveRunResponse response)
    {
        if (response.ProcessedCount == 0)
            return "Completed";

        if (response.FailedCount == response.ProcessedCount)
            return "Failed";

        if (response.SkippedCount == response.ProcessedCount)
            return "Skipped";

        if (response.ArchivedCount == response.ProcessedCount)
            return "Completed";

        return "PartiallyCompleted";
    }

    private async Task BackfillLegacyArchiveStateAsync(
        ArchiveAggregateSnapshot snapshot,
        Guid archiveRunId,
        AuditStamp auditStamp,
        CancellationToken cancellationToken)
    {
        if (snapshot.ExistsInArchive && snapshot.Lifecycle.ArchiveRecordWrittenAtUtc is null)
        {
            await _dbContext.IngestionFiles
                .Where(x => x.Id == snapshot.AggregateId && x.ArchiveRecordWrittenAt == null)
                .ExecuteUpdateAsync(update => update
                    .SetProperty(x => x.ArchiveRecordWrittenAt, snapshot.Lifecycle.ArchiveRecordWrittenAtUtc ?? auditStamp.Timestamp)
                    .SetProperty(x => x.ArchiveRecordRunId, archiveRunId)
                    .SetProperty(x => x.UpdateDate, auditStamp.Timestamp)
                    .SetProperty(x => x.LastModifiedBy, auditStamp.UserId),
                    cancellationToken);
        }

        var archiveLineMetadata = await _dbContext.ArchiveIngestionFileLines
            .AsNoTracking()
            .Where(x => x.IngestionFileId == snapshot.AggregateId)
            .Select(x => new { x.Id, x.ArchiveTransitionedAt, x.ArchivedAt })
            .ToListAsync(cancellationToken);

        var archiveLineLookup = archiveLineMetadata.ToDictionary(x => x.Id, x => x.ArchiveTransitionedAt ?? x.ArchivedAt);
        var missingMarkers = snapshot.ItemSnapshots
            .Select(x => x.FileLineId)
            .Where(archiveLineLookup.ContainsKey)
            .ToArray();

        if (missingMarkers.Length == 0)
        {
            return;
        }

        var rows = await _dbContext.IngestionFileLines
            .Where(x => missingMarkers.Contains(x.Id) && x.ArchiveTransitionedAt == null)
            .ToListAsync(cancellationToken);

        foreach (var row in rows)
        {
            row.ArchiveTransitionedAt = archiveLineLookup[row.Id];
            row.ArchiveTransitionRunId ??= archiveRunId;
        }

        if (rows.Count > 0)
        {
            _auditStampService.StampForUpdate(rows);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task EnsureArchiveFileRecordAsync(
        IngestionFile file,
        AuditStamp auditStamp,
        Guid archiveRunId,
        CancellationToken cancellationToken)
    {
        var archiveFile = await _dbContext.ArchiveIngestionFiles
            .AsTracking()
            .SingleOrDefaultAsync(x => x.Id == file.Id, cancellationToken);

        if (archiveFile is null)
        {
            archiveFile = CloneArchiveFile(file, auditStamp, archiveRunId);
            await _dbContext.ArchiveIngestionFiles.AddAsync(archiveFile, cancellationToken);
        }
        else
        {
            archiveFile.ArchiveRecordWrittenAt = file.ArchiveRecordWrittenAt;
            archiveFile.ArchiveRecordRunId = file.ArchiveRecordRunId;
            archiveFile.ArchiveChildrenTransitionedAt = file.ArchiveChildrenTransitionedAt;
            archiveFile.ArchiveCleanupEligibleAt = file.ArchiveCleanupEligibleAt;
            archiveFile.ArchiveCleanupCompletedAt = file.ArchiveCleanupCompletedAt;
            archiveFile.UpdateDate = auditStamp.Timestamp;
            archiveFile.LastModifiedBy = auditStamp.UserId;
        }

        _auditStampService.StampForUpdate(file);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task ArchiveAtomicItemsAsync(
        IngestionFile file,
        IReadOnlyCollection<Guid> eligibleItemIds,
        AuditStamp auditStamp,
        Guid archiveRunId,
        CancellationToken cancellationToken)
    {
        var liveLines = await _dbContext.IngestionFileLines
            .AsTracking()
            .Where(x => eligibleItemIds.Contains(x.Id))
            .OrderBy(x => x.LineNumber)
            .ToListAsync(cancellationToken);

        foreach (var line in liveLines)
        {
            line.ArchiveTransitionedAt ??= auditStamp.Timestamp;
            line.ArchiveTransitionRunId ??= archiveRunId;
        }

        await UpsertArchiveLinesAsync(liveLines, auditStamp, archiveRunId, cancellationToken);
        await UpsertArchiveEvaluationsAsync(eligibleItemIds, auditStamp, archiveRunId, cancellationToken);
        await UpsertArchiveOperationsAsync(eligibleItemIds, auditStamp, archiveRunId, cancellationToken);
        await UpsertArchiveReviewsAsync(eligibleItemIds, auditStamp, archiveRunId, cancellationToken);
        await UpsertArchiveExecutionsAsync(eligibleItemIds, auditStamp, archiveRunId, cancellationToken);
        await UpsertArchiveAlertsAsync(eligibleItemIds, auditStamp, archiveRunId, cancellationToken);

        _auditStampService.StampForUpdate(liveLines);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await EnsureAtomicArchiveIntegrityAsync(file.Id, eligibleItemIds, cancellationToken);
    }

    private async Task RefreshFileLifecycleAsync(
        Guid ingestionFileId,
        Guid archiveRunId,
        AuditStamp auditStamp,
        CancellationToken cancellationToken)
    {
        var file = await _dbContext.IngestionFiles
            .AsTracking()
            .SingleAsync(x => x.Id == ingestionFileId, cancellationToken);

        var totalDetailCount = await _dbContext.IngestionFileLines
            .AsNoTracking()
            .Where(x => x.IngestionFileId == ingestionFileId && x.RecordType == "D")
            .CountAsync(cancellationToken);

        var transitionedCount = await _dbContext.IngestionFileLines
            .AsNoTracking()
            .Where(x => x.IngestionFileId == ingestionFileId && x.RecordType == "D" && x.ArchiveTransitionedAt != null)
            .CountAsync(cancellationToken);

        var fileRecordExists = await _dbContext.ArchiveIngestionFiles
            .AsNoTracking()
            .AnyAsync(x => x.Id == ingestionFileId, cancellationToken);

        if (fileRecordExists && !file.ArchiveRecordWrittenAt.HasValue)
        {
            file.ArchiveRecordWrittenAt = auditStamp.Timestamp;
            file.ArchiveRecordRunId = archiveRunId;
        }

        if (fileRecordExists && totalDetailCount == transitionedCount)
        {
            file.ArchiveChildrenTransitionedAt ??= auditStamp.Timestamp;
        }

        var snapshot = await _reader.GetSnapshotAsync(ingestionFileId, cancellationToken);
        var eligibility = _evaluator.Evaluate(snapshot, auditStamp.Timestamp);

        if (snapshot?.Lifecycle.CleanupEligible == true && eligibility.IsEligible)
        {
            file.ArchiveCleanupEligibleAt ??= auditStamp.Timestamp;
        }

        _auditStampService.StampForUpdate(file);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var archiveFile = await _dbContext.ArchiveIngestionFiles
            .AsTracking()
            .SingleOrDefaultAsync(x => x.Id == ingestionFileId, cancellationToken);

        if (archiveFile is not null)
        {
            archiveFile.ArchiveRecordWrittenAt = file.ArchiveRecordWrittenAt;
            archiveFile.ArchiveRecordRunId = file.ArchiveRecordRunId;
            archiveFile.ArchiveChildrenTransitionedAt = file.ArchiveChildrenTransitionedAt;
            archiveFile.ArchiveCleanupEligibleAt = file.ArchiveCleanupEligibleAt;
            archiveFile.UpdateDate = auditStamp.Timestamp;
            archiveFile.LastModifiedBy = auditStamp.UserId;
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task CleanupLiveAggregateAsync(
        Guid ingestionFileId,
        AuditStamp auditStamp,
        Guid archiveRunId,
        CancellationToken cancellationToken)
    {
        var archiveFile = await _dbContext.ArchiveIngestionFiles
            .AsTracking()
            .SingleAsync(x => x.Id == ingestionFileId, cancellationToken);

        archiveFile.ArchiveCleanupEligibleAt ??= auditStamp.Timestamp;
        archiveFile.ArchiveCleanupCompletedAt ??= auditStamp.Timestamp;
        archiveFile.ArchiveChildrenTransitionedAt ??= auditStamp.Timestamp;
        archiveFile.ArchiveRecordRunId ??= archiveRunId;
        archiveFile.UpdateDate = auditStamp.Timestamp;
        archiveFile.LastModifiedBy = auditStamp.UserId;

        await _dbContext.SaveChangesAsync(cancellationToken);

        var fileLineIds = await _dbContext.IngestionFileLines
            .AsNoTracking()
            .Where(x => x.IngestionFileId == ingestionFileId)
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        if (fileLineIds.Count > 0)
        {
            await _dbContext.ReconciliationAlerts
                .Where(x => fileLineIds.Contains(x.FileLineId))
                .ExecuteDeleteAsync(cancellationToken);
            await _dbContext.ReconciliationOperationExecutions
                .Where(x => fileLineIds.Contains(x.FileLineId))
                .ExecuteDeleteAsync(cancellationToken);
            await _dbContext.ReconciliationReviews
                .Where(x => fileLineIds.Contains(x.FileLineId))
                .ExecuteDeleteAsync(cancellationToken);
            await _dbContext.ReconciliationOperations
                .Where(x => fileLineIds.Contains(x.FileLineId))
                .ExecuteDeleteAsync(cancellationToken);
            await _dbContext.ReconciliationEvaluations
                .Where(x => fileLineIds.Contains(x.FileLineId))
                .ExecuteDeleteAsync(cancellationToken);
        }

        await _dbContext.IngestionFileLines
            .Where(x => x.IngestionFileId == ingestionFileId)
            .ExecuteDeleteAsync(cancellationToken);
        await _dbContext.IngestionFiles
            .Where(x => x.Id == ingestionFileId)
            .ExecuteDeleteAsync(cancellationToken);
    }

    private async Task EnsureAtomicArchiveIntegrityAsync(
        Guid ingestionFileId,
        IReadOnlyCollection<Guid> itemIds,
        CancellationToken cancellationToken)
    {
        var liveLineCount = await _dbContext.IngestionFileLines
            .AsNoTracking()
            .Where(x => itemIds.Contains(x.Id))
            .CountAsync(cancellationToken);
        var archiveLineCount = await _dbContext.ArchiveIngestionFileLines
            .AsNoTracking()
            .Where(x => itemIds.Contains(x.Id))
            .CountAsync(cancellationToken);

        if (liveLineCount != archiveLineCount)
        {
            throw new InvalidOperationException("Archive verification failed for archive.ingestion_file_line.");
        }

        await EnsureReconCountsMatchAsync<ReconciliationEvaluation, ArchiveReconciliationEvaluation>(itemIds, cancellationToken);
        await EnsureReconCountsMatchAsync<ReconciliationOperation, ArchiveReconciliationOperation>(itemIds, cancellationToken);
        await EnsureReconCountsMatchAsync<ReconciliationReview, ArchiveReconciliationReview>(itemIds, cancellationToken);
        await EnsureReconCountsMatchAsync<ReconciliationOperationExecution, ArchiveReconciliationOperationExecution>(itemIds, cancellationToken);
        await EnsureReconCountsMatchAsync<ReconciliationAlert, ArchiveReconciliationAlert>(itemIds, cancellationToken);

        var archiveFileExists = await _dbContext.ArchiveIngestionFiles
            .AsNoTracking()
            .AnyAsync(x => x.Id == ingestionFileId, cancellationToken);

        if (!archiveFileExists)
        {
            throw new InvalidOperationException("Archive verification failed for archive.ingestion_file.");
        }
    }

    private async Task EnsureReconCountsMatchAsync<TLive, TArchive>(
        IReadOnlyCollection<Guid> itemIds,
        CancellationToken cancellationToken)
        where TLive : class
        where TArchive : class
    {
        var liveCount = await _dbContext.Set<TLive>()
            .AsNoTracking()
            .CountAsync(CreateFileLinePredicate<TLive>(itemIds), cancellationToken);
        var archiveCount = await _dbContext.Set<TArchive>()
            .AsNoTracking()
            .CountAsync(CreateFileLinePredicate<TArchive>(itemIds), cancellationToken);

        if (liveCount != archiveCount)
        {
            throw new InvalidOperationException($"Archive verification failed for {typeof(TArchive).Name}.");
        }
    }

    private static System.Linq.Expressions.Expression<Func<TEntity, bool>> CreateFileLinePredicate<TEntity>(
        IReadOnlyCollection<Guid> itemIds)
    {
        var parameter = System.Linq.Expressions.Expression.Parameter(typeof(TEntity), "x");
        var property = System.Linq.Expressions.Expression.Property(parameter, nameof(ReconciliationEvaluation.FileLineId));
        var itemIdsConstant = System.Linq.Expressions.Expression.Constant(itemIds);
        var contains = System.Linq.Expressions.Expression.Call(
            typeof(Enumerable),
            nameof(Enumerable.Contains),
            new[] { typeof(Guid) },
            itemIdsConstant,
            property);

        return System.Linq.Expressions.Expression.Lambda<Func<TEntity, bool>>(contains, parameter);
    }

    private async Task UpsertArchiveLinesAsync(
        IReadOnlyCollection<IngestionFileLine> liveLines,
        AuditStamp auditStamp,
        Guid archiveRunId,
        CancellationToken cancellationToken)
    {
        var lineIds = liveLines.Select(x => x.Id).ToArray();
        var existingIds = await _dbContext.ArchiveIngestionFileLines
            .AsNoTracking()
            .Where(x => lineIds.Contains(x.Id))
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        var missing = liveLines
            .Where(x => !existingIds.Contains(x.Id))
            .Select(x => CloneArchiveLine(x, auditStamp, archiveRunId))
            .ToList();

        if (missing.Count > 0)
        {
            await _dbContext.ArchiveIngestionFileLines.AddRangeAsync(missing, cancellationToken);
        }
    }

    private async Task UpsertArchiveEvaluationsAsync(
        IReadOnlyCollection<Guid> itemIds,
        AuditStamp auditStamp,
        Guid archiveRunId,
        CancellationToken cancellationToken)
    {
        var live = await _dbContext.ReconciliationEvaluations
            .AsNoTracking()
            .Where(x => itemIds.Contains(x.FileLineId))
            .ToListAsync(cancellationToken);

        var existingIds = await _dbContext.ArchiveReconciliationEvaluations
            .AsNoTracking()
            .Where(x => itemIds.Contains(x.FileLineId))
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        var missing = live
            .Where(x => !existingIds.Contains(x.Id))
            .Select(x => CloneArchiveEvaluation(x, auditStamp, archiveRunId))
            .ToList();

        if (missing.Count > 0)
        {
            await _dbContext.ArchiveReconciliationEvaluations.AddRangeAsync(missing, cancellationToken);
        }
    }

    private async Task UpsertArchiveOperationsAsync(
        IReadOnlyCollection<Guid> itemIds,
        AuditStamp auditStamp,
        Guid archiveRunId,
        CancellationToken cancellationToken)
    {
        var live = await _dbContext.ReconciliationOperations
            .AsNoTracking()
            .Where(x => itemIds.Contains(x.FileLineId))
            .ToListAsync(cancellationToken);

        var existingIds = await _dbContext.ArchiveReconciliationOperations
            .AsNoTracking()
            .Where(x => itemIds.Contains(x.FileLineId))
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        var missing = live
            .Where(x => !existingIds.Contains(x.Id))
            .Select(x => CloneArchiveOperation(x, auditStamp, archiveRunId))
            .ToList();

        if (missing.Count > 0)
        {
            await _dbContext.ArchiveReconciliationOperations.AddRangeAsync(missing, cancellationToken);
        }
    }

    private async Task UpsertArchiveReviewsAsync(
        IReadOnlyCollection<Guid> itemIds,
        AuditStamp auditStamp,
        Guid archiveRunId,
        CancellationToken cancellationToken)
    {
        var live = await _dbContext.ReconciliationReviews
            .AsNoTracking()
            .Where(x => itemIds.Contains(x.FileLineId))
            .ToListAsync(cancellationToken);

        var existingIds = await _dbContext.ArchiveReconciliationReviews
            .AsNoTracking()
            .Where(x => itemIds.Contains(x.FileLineId))
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        var missing = live
            .Where(x => !existingIds.Contains(x.Id))
            .Select(x => CloneArchiveReview(x, auditStamp, archiveRunId))
            .ToList();

        if (missing.Count > 0)
        {
            await _dbContext.ArchiveReconciliationReviews.AddRangeAsync(missing, cancellationToken);
        }
    }

    private async Task UpsertArchiveExecutionsAsync(
        IReadOnlyCollection<Guid> itemIds,
        AuditStamp auditStamp,
        Guid archiveRunId,
        CancellationToken cancellationToken)
    {
        var live = await _dbContext.ReconciliationOperationExecutions
            .AsNoTracking()
            .Where(x => itemIds.Contains(x.FileLineId))
            .ToListAsync(cancellationToken);

        var existingIds = await _dbContext.ArchiveReconciliationOperationExecutions
            .AsNoTracking()
            .Where(x => itemIds.Contains(x.FileLineId))
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        var missing = live
            .Where(x => !existingIds.Contains(x.Id))
            .Select(x => CloneArchiveExecution(x, auditStamp, archiveRunId))
            .ToList();

        if (missing.Count > 0)
        {
            await _dbContext.ArchiveReconciliationOperationExecutions.AddRangeAsync(missing, cancellationToken);
        }
    }

    private async Task UpsertArchiveAlertsAsync(
        IReadOnlyCollection<Guid> itemIds,
        AuditStamp auditStamp,
        Guid archiveRunId,
        CancellationToken cancellationToken)
    {
        var live = await _dbContext.ReconciliationAlerts
            .AsNoTracking()
            .Where(x => itemIds.Contains(x.FileLineId))
            .ToListAsync(cancellationToken);

        var existingIds = await _dbContext.ArchiveReconciliationAlerts
            .AsNoTracking()
            .Where(x => itemIds.Contains(x.FileLineId))
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        var missing = live
            .Where(x => !existingIds.Contains(x.Id))
            .Select(x => CloneArchiveAlert(x, auditStamp, archiveRunId))
            .ToList();

        if (missing.Count > 0)
        {
            await _dbContext.ArchiveReconciliationAlerts.AddRangeAsync(missing, cancellationToken);
        }
    }

    private static string BuildSafeMessage(Exception ex)
    {
        var message = ex.Message;
        if (message.Length > 500)
        {
            message = message[..500];
        }

        return $"[EXECUTION_ERROR] {message}";
    }

    private static ArchiveIngestionFile CloneArchiveFile(IngestionFile source, AuditStamp auditStamp, Guid archiveRunId)
    {
        return new ArchiveIngestionFile
        {
            Id = source.Id,
            FileKey = source.FileKey,
            FileName = source.FileName,
            FullPath = source.FullPath,
            SourceType = source.SourceType,
            FileType = source.FileType,
            ContentType = source.ContentType,
            Status = source.Status,
            Message = source.Message,
            ExpectedCount = source.ExpectedCount,
            TotalCount = source.TotalCount,
            SuccessCount = source.SuccessCount,
            ErrorCount = source.ErrorCount,
            LastProcessedLineNumber = source.LastProcessedLineNumber,
            LastProcessedByteOffset = source.LastProcessedByteOffset,
            IsArchived = source.IsArchived,
            ArchiveRecordWrittenAt = source.ArchiveRecordWrittenAt,
            ArchiveRecordRunId = source.ArchiveRecordRunId,
            ArchiveChildrenTransitionedAt = source.ArchiveChildrenTransitionedAt,
            ArchiveCleanupEligibleAt = source.ArchiveCleanupEligibleAt,
            ArchiveCleanupCompletedAt = source.ArchiveCleanupCompletedAt,
            CreateDate = source.CreateDate,
            CreatedBy = source.CreatedBy,
            UpdateDate = source.UpdateDate,
            LastModifiedBy = source.LastModifiedBy,
            RecordStatus = source.RecordStatus,
            ArchivedAt = auditStamp.Timestamp,
            ArchivedBy = auditStamp.UserId,
            ArchiveRunId = archiveRunId
        };
    }

    private static ArchiveIngestionFileLine CloneArchiveLine(IngestionFileLine source, AuditStamp auditStamp, Guid archiveRunId)
    {
        return new ArchiveIngestionFileLine
        {
            Id = source.Id,
            IngestionFileId = source.IngestionFileId,
            LineNumber = source.LineNumber,
            ByteOffset = source.ByteOffset,
            ByteLength = source.ByteLength,
            RecordType = source.RecordType,
            RawData = source.RawData,
            ParsedData = source.ParsedData,
            Status = source.Status,
            Message = source.Message,
            RetryCount = source.RetryCount,
            CorrelationKey = source.CorrelationKey,
            CorrelationValue = source.CorrelationValue,
            DuplicateDetectionKey = source.DuplicateDetectionKey,
            DuplicateStatus = source.DuplicateStatus,
            DuplicateGroupId = source.DuplicateGroupId,
            ReconciliationStatus = source.ReconciliationStatus,
            ArchiveTransitionedAt = source.ArchiveTransitionedAt,
            ArchiveTransitionRunId = source.ArchiveTransitionRunId,
            CreateDate = source.CreateDate,
            CreatedBy = source.CreatedBy,
            UpdateDate = source.UpdateDate,
            LastModifiedBy = source.LastModifiedBy,
            RecordStatus = source.RecordStatus,
            ArchivedAt = auditStamp.Timestamp,
            ArchivedBy = auditStamp.UserId,
            ArchiveRunId = archiveRunId
        };
    }

    private static ArchiveReconciliationEvaluation CloneArchiveEvaluation(ReconciliationEvaluation source, AuditStamp auditStamp, Guid archiveRunId)
    {
        return new ArchiveReconciliationEvaluation
        {
            Id = source.Id,
            FileLineId = source.FileLineId,
            GroupId = source.GroupId,
            Status = source.Status,
            Message = source.Message,
            CreatedOperationCount = source.CreatedOperationCount,
            CreateDate = source.CreateDate,
            CreatedBy = source.CreatedBy,
            UpdateDate = source.UpdateDate,
            LastModifiedBy = source.LastModifiedBy,
            RecordStatus = source.RecordStatus,
            ArchivedAt = auditStamp.Timestamp,
            ArchivedBy = auditStamp.UserId,
            ArchiveRunId = archiveRunId
        };
    }

    private static ArchiveReconciliationOperation CloneArchiveOperation(ReconciliationOperation source, AuditStamp auditStamp, Guid archiveRunId)
    {
        return new ArchiveReconciliationOperation
        {
            Id = source.Id,
            FileLineId = source.FileLineId,
            EvaluationId = source.EvaluationId,
            GroupId = source.GroupId,
            SequenceIndex = source.SequenceIndex,
            ParentSequenceIndex = source.ParentSequenceIndex,
            Code = source.Code,
            Note = source.Note,
            Payload = source.Payload,
            IsManual = source.IsManual,
            Branch = source.Branch,
            Status = source.Status,
            LeaseOwner = source.LeaseOwner,
            LeaseExpiresAt = source.LeaseExpiresAt,
            RetryCount = source.RetryCount,
            MaxRetries = source.MaxRetries,
            NextAttemptAt = source.NextAttemptAt,
            IdempotencyKey = source.IdempotencyKey,
            LastError = source.LastError,
            CreateDate = source.CreateDate,
            CreatedBy = source.CreatedBy,
            UpdateDate = source.UpdateDate,
            LastModifiedBy = source.LastModifiedBy,
            RecordStatus = source.RecordStatus,
            ArchivedAt = auditStamp.Timestamp,
            ArchivedBy = auditStamp.UserId,
            ArchiveRunId = archiveRunId
        };
    }

    private static ArchiveReconciliationReview CloneArchiveReview(ReconciliationReview source, AuditStamp auditStamp, Guid archiveRunId)
    {
        return new ArchiveReconciliationReview
        {
            Id = source.Id,
            FileLineId = source.FileLineId,
            GroupId = source.GroupId,
            EvaluationId = source.EvaluationId,
            OperationId = source.OperationId,
            ReviewerId = source.ReviewerId,
            Decision = source.Decision,
            Comment = source.Comment,
            DecisionAt = source.DecisionAt,
            ExpiresAt = source.ExpiresAt,
            ExpirationAction = source.ExpirationAction,
            ExpirationFlowAction = source.ExpirationFlowAction,
            CreateDate = source.CreateDate,
            CreatedBy = source.CreatedBy,
            UpdateDate = source.UpdateDate,
            LastModifiedBy = source.LastModifiedBy,
            RecordStatus = source.RecordStatus,
            ArchivedAt = auditStamp.Timestamp,
            ArchivedBy = auditStamp.UserId,
            ArchiveRunId = archiveRunId
        };
    }

    private static ArchiveReconciliationOperationExecution CloneArchiveExecution(ReconciliationOperationExecution source, AuditStamp auditStamp, Guid archiveRunId)
    {
        return new ArchiveReconciliationOperationExecution
        {
            Id = source.Id,
            FileLineId = source.FileLineId,
            GroupId = source.GroupId,
            EvaluationId = source.EvaluationId,
            OperationId = source.OperationId,
            AttemptNumber = source.AttemptNumber,
            StartedAt = source.StartedAt,
            FinishedAt = source.FinishedAt,
            Status = source.Status,
            RequestPayload = source.RequestPayload,
            ResponsePayload = source.ResponsePayload,
            ResultCode = source.ResultCode,
            ResultMessage = source.ResultMessage,
            ErrorCode = source.ErrorCode,
            ErrorMessage = source.ErrorMessage,
            CreateDate = source.CreateDate,
            CreatedBy = source.CreatedBy,
            UpdateDate = source.UpdateDate,
            LastModifiedBy = source.LastModifiedBy,
            RecordStatus = source.RecordStatus,
            ArchivedAt = auditStamp.Timestamp,
            ArchivedBy = auditStamp.UserId,
            ArchiveRunId = archiveRunId
        };
    }

    private static ArchiveReconciliationAlert CloneArchiveAlert(ReconciliationAlert source, AuditStamp auditStamp, Guid archiveRunId)
    {
        return new ArchiveReconciliationAlert
        {
            Id = source.Id,
            FileLineId = source.FileLineId,
            GroupId = source.GroupId,
            EvaluationId = source.EvaluationId,
            OperationId = source.OperationId,
            Severity = source.Severity,
            AlertType = source.AlertType,
            Message = source.Message,
            AlertStatus = source.AlertStatus,
            CreateDate = source.CreateDate,
            CreatedBy = source.CreatedBy,
            UpdateDate = source.UpdateDate,
            LastModifiedBy = source.LastModifiedBy,
            RecordStatus = source.RecordStatus,
            ArchivedAt = auditStamp.Timestamp,
            ArchivedBy = auditStamp.UserId,
            ArchiveRunId = archiveRunId
        };
    }
}
