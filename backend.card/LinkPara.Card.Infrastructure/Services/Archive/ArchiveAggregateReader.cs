using LinkPara.Card.Application.Commons.Models.Archive;
using LinkPara.Card.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.Card.Infrastructure.Services.Archive;

internal sealed class ArchiveAggregateReader
{
    private readonly CardDbContext _dbContext;

    public ArchiveAggregateReader(CardDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<Guid>> ResolveCandidateFileIdsAsync(
        Guid[] fileIds,
        DateTime? beforeDate,
        int limit,
        CancellationToken cancellationToken)
    {
        var requestedIds = fileIds.Where(x => x != Guid.Empty).Distinct().ToArray();

        var query = _dbContext.IngestionFiles
            .AsNoTracking()
            .Where(x => requestedIds.Length == 0 || requestedIds.Contains(x.Id));

        if (beforeDate.HasValue)
        {
            query = query.Where(x => x.CreateDate <= beforeDate.Value);
        }

        return await query
            .OrderBy(x => x.UpdateDate ?? x.CreateDate)
            .ThenBy(x => x.Id)
            .Select(x => x.Id)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task<ArchiveAggregateSnapshot?> GetSnapshotAsync(Guid ingestionFileId, CancellationToken cancellationToken)
    {
        var file = await _dbContext.IngestionFiles
            .AsNoTracking()
            .Where(x => x.Id == ingestionFileId)
            .Select(x => new
            {
                x.Id,
                Status = x.Status.ToString(),
                x.CreateDate,
                LastUpdate = x.UpdateDate ?? x.CreateDate,
                x.ArchiveRecordWrittenAt,
                x.ArchiveChildrenTransitionedAt,
                x.ArchiveCleanupEligibleAt,
                x.ArchiveCleanupCompletedAt
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (file is null)
        {
            return null;
        }

        var snapshot = new ArchiveAggregateSnapshot
        {
            AggregateId = file.Id,
            FileCreateDateUtc = AsUtc(file.CreateDate),
            LastUpdateUtc = AsUtc(file.LastUpdate),
            AggregateLastActivityUtc = AsUtc(file.LastUpdate)
        };

        snapshot.Counts.IngestionFileCount = 1;
        snapshot.IngestionFileStatuses.Add(file.Status);

        snapshot.Lifecycle.ArchiveRecordWrittenAtUtc = AsUtc(file.ArchiveRecordWrittenAt);
        snapshot.Lifecycle.ChildrenTransitionedAtUtc = AsUtc(file.ArchiveChildrenTransitionedAt);
        snapshot.Lifecycle.CleanupEligibleAtUtc = AsUtc(file.ArchiveCleanupEligibleAt);
        snapshot.Lifecycle.CleanupCompletedAtUtc = AsUtc(file.ArchiveCleanupCompletedAt);
        snapshot.Lifecycle.ArchiveRecordWritten = file.ArchiveRecordWrittenAt.HasValue;
        snapshot.Lifecycle.ChildrenFullyTransitioned = file.ArchiveChildrenTransitionedAt.HasValue;
        snapshot.Lifecycle.CleanupEligible = file.ArchiveCleanupEligibleAt.HasValue;
        snapshot.Lifecycle.CleanupCompleted = file.ArchiveCleanupCompletedAt.HasValue;

        snapshot.Counts.IngestionFileLineCount = await _dbContext.IngestionFileLines
            .AsNoTracking()
            .Where(x => x.IngestionFileId == ingestionFileId)
            .CountAsync(cancellationToken);

        foreach (var status in await _dbContext.IngestionFileLines
                     .AsNoTracking()
                     .Where(x => x.IngestionFileId == ingestionFileId)
                     .Select(x => x.Status.ToString())
                     .Distinct()
                     .ToListAsync(cancellationToken))
        {
            snapshot.IngestionFileLineStatuses.Add(status);
        }

        foreach (var status in await _dbContext.IngestionFileLines
                     .AsNoTracking()
                     .Where(x => x.IngestionFileId == ingestionFileId && x.ReconciliationStatus != null)
                     .Select(x => x.ReconciliationStatus!.Value.ToString())
                     .Distinct()
                     .ToListAsync(cancellationToken))
        {
            snapshot.IngestionFileLineReconciliationStatuses.Add(status);
        }

        var liveLines = await _dbContext.IngestionFileLines
            .AsNoTracking()
            .Where(x => x.IngestionFileId == ingestionFileId && x.RecordType == "D")
            .Select(x => new
            {
                x.Id,
                Status = x.Status.ToString(),
                ReconciliationStatus = x.ReconciliationStatus != null ? x.ReconciliationStatus.Value.ToString() : null,
                x.ArchiveTransitionedAt,
                LastActivity = x.UpdateDate ?? x.CreateDate
            })
            .ToListAsync(cancellationToken);

        var fileLineIds = liveLines.Select(x => x.Id).ToList();

        if (fileLineIds.Count == 0)
        {
            snapshot.ExistsInArchive = await ExistsInArchiveAsync(ingestionFileId, cancellationToken);
            snapshot.Lifecycle.ArchiveRecordWritten |= snapshot.ExistsInArchive;
            if (snapshot.ExistsInArchive && snapshot.Lifecycle.ArchiveRecordWrittenAtUtc is null)
            {
                snapshot.Lifecycle.ArchiveRecordWrittenAtUtc = await GetArchiveFileArchivedAtAsync(ingestionFileId, cancellationToken);
            }

            return snapshot;
        }

        var evaluationRows = await _dbContext.ReconciliationEvaluations
            .AsNoTracking()
            .Where(x => fileLineIds.Contains(x.FileLineId))
            .Select(x => new { x.FileLineId, Status = x.Status.ToString(), LastActivity = x.UpdateDate ?? x.CreateDate })
            .ToListAsync(cancellationToken);

        var operationRows = await _dbContext.ReconciliationOperations
            .AsNoTracking()
            .Where(x => fileLineIds.Contains(x.FileLineId))
            .Select(x => new
            {
                x.FileLineId,
                Status = x.Status.ToString(),
                x.LeaseExpiresAt,
                x.NextAttemptAt,
                LastActivity = x.UpdateDate ?? x.CreateDate
            })
            .ToListAsync(cancellationToken);

        var reviewRows = await _dbContext.ReconciliationReviews
            .AsNoTracking()
            .Where(x => fileLineIds.Contains(x.FileLineId))
            .Select(x => new { x.FileLineId, Status = x.Decision.ToString(), LastActivity = x.UpdateDate ?? x.CreateDate })
            .ToListAsync(cancellationToken);

        var executionRows = await _dbContext.ReconciliationOperationExecutions
            .AsNoTracking()
            .Where(x => fileLineIds.Contains(x.FileLineId))
            .Select(x => new { x.FileLineId, Status = x.Status.ToString(), LastActivity = x.UpdateDate ?? x.CreateDate })
            .ToListAsync(cancellationToken);

        var alertRows = await _dbContext.ReconciliationAlerts
            .AsNoTracking()
            .Where(x => fileLineIds.Contains(x.FileLineId))
            .Select(x => new { x.FileLineId, Status = x.AlertStatus.ToString(), LastActivity = x.UpdateDate ?? x.CreateDate })
            .ToListAsync(cancellationToken);

        snapshot.Counts.ReconciliationEvaluationCount = evaluationRows.Count;
        snapshot.Counts.ReconciliationOperationCount = operationRows.Count;
        snapshot.Counts.ReconciliationReviewCount = reviewRows.Count;
        snapshot.Counts.ReconciliationOperationExecutionCount = executionRows.Count;
        snapshot.Counts.ReconciliationAlertCount = alertRows.Count;

        foreach (var status in evaluationRows.Select(x => x.Status).Distinct(StringComparer.OrdinalIgnoreCase))
        {
            snapshot.ReconciliationEvaluationStatuses.Add(status);
        }

        foreach (var row in operationRows)
        {
            snapshot.ReconciliationOperationStatuses.Add(row.Status);
            if (row.LeaseExpiresAt.HasValue && row.LeaseExpiresAt.Value > DateTime.UtcNow)
            {
                snapshot.HasAnyOperationLease = true;
            }

            if (row.NextAttemptAt.HasValue)
            {
                snapshot.HasScheduledRetryAttempt = true;
            }
        }

        foreach (var status in reviewRows.Select(x => x.Status).Distinct(StringComparer.OrdinalIgnoreCase))
        {
            snapshot.ReconciliationReviewStatuses.Add(status);
        }

        foreach (var status in executionRows.Select(x => x.Status).Distinct(StringComparer.OrdinalIgnoreCase))
        {
            snapshot.ReconciliationOperationExecutionStatuses.Add(status);
        }

        foreach (var status in alertRows.Select(x => x.Status).Distinct(StringComparer.OrdinalIgnoreCase))
        {
            snapshot.ReconciliationAlertStatuses.Add(status);
        }

        var lineArchiveIds = await _dbContext.ArchiveIngestionFileLines
            .AsNoTracking()
            .Where(x => x.IngestionFileId == ingestionFileId)
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        var archiveLineIdSet = lineArchiveIds.ToHashSet();

        var itemLookup = liveLines.ToDictionary(
            x => x.Id,
            x => new ArchiveAtomicItemSnapshot
            {
                FileLineId = x.Id,
                IsArchived = x.ArchiveTransitionedAt.HasValue || archiveLineIdSet.Contains(x.Id)
            });

        foreach (var line in liveLines)
        {
            itemLookup[line.Id].FileLineStatuses.Add(line.Status);
            if (!string.IsNullOrWhiteSpace(line.ReconciliationStatus))
            {
                itemLookup[line.Id].FileLineReconciliationStatuses.Add(line.ReconciliationStatus);
            }

            snapshot.AggregateLastActivityUtc = Max(snapshot.AggregateLastActivityUtc, AsUtc(line.LastActivity));
        }

        AddStatuses(evaluationRows, itemLookup, x => x.FileLineId, x => x.Status, x => x.LastActivity, snapshot, (item, status) => item.EvaluationStatuses.Add(status));
        AddStatuses(operationRows, itemLookup, x => x.FileLineId, x => x.Status, x => x.LastActivity, snapshot, (item, status) => item.OperationStatuses.Add(status));
        AddStatuses(reviewRows, itemLookup, x => x.FileLineId, x => x.Status, x => x.LastActivity, snapshot, (item, status) => item.ReviewStatuses.Add(status));
        AddStatuses(executionRows, itemLookup, x => x.FileLineId, x => x.Status, x => x.LastActivity, snapshot, (item, status) => item.OperationExecutionStatuses.Add(status));
        AddStatuses(alertRows, itemLookup, x => x.FileLineId, x => x.Status, x => x.LastActivity, snapshot, (item, status) => item.AlertStatuses.Add(status));

        foreach (var row in operationRows)
        {
            var item = itemLookup[row.FileLineId];
            if (row.LeaseExpiresAt.HasValue && row.LeaseExpiresAt.Value > DateTime.UtcNow)
            {
                item.HasActiveLease = true;
            }

            if (row.NextAttemptAt.HasValue)
            {
                item.HasScheduledRetryAttempt = true;
            }
        }

        snapshot.ExistsInArchive = await ExistsInArchiveAsync(ingestionFileId, cancellationToken);
        snapshot.Lifecycle.ArchiveRecordWritten |= snapshot.ExistsInArchive;
        if (snapshot.ExistsInArchive && snapshot.Lifecycle.ArchiveRecordWrittenAtUtc is null)
        {
            snapshot.Lifecycle.ArchiveRecordWrittenAtUtc = await GetArchiveFileArchivedAtAsync(ingestionFileId, cancellationToken);
        }

        snapshot.ItemSnapshots = itemLookup.Values
            .OrderBy(x => x.FileLineId)
            .ToList();

        snapshot.AtomicItems.TotalCount = snapshot.ItemSnapshots.Count;
        snapshot.AtomicItems.ArchivedCount = snapshot.ItemSnapshots.Count(x => x.IsArchived);

        return snapshot;
    }

    private async Task<bool> ExistsInArchiveAsync(Guid ingestionFileId, CancellationToken cancellationToken)
    {
        return await _dbContext.ArchiveIngestionFiles
            .AsNoTracking()
            .AnyAsync(x => x.Id == ingestionFileId, cancellationToken);
    }

    private async Task<DateTime?> GetArchiveFileArchivedAtAsync(Guid ingestionFileId, CancellationToken cancellationToken)
    {
        var archivedAt = await _dbContext.ArchiveIngestionFiles
            .AsNoTracking()
            .Where(x => x.Id == ingestionFileId)
            .Select(x => (DateTime?)x.ArchivedAt)
            .SingleOrDefaultAsync(cancellationToken);

        return AsUtc(archivedAt);
    }

    private static void AddStatuses<T>(
        IEnumerable<T> rows,
        IReadOnlyDictionary<Guid, ArchiveAtomicItemSnapshot> items,
        Func<T, Guid> idSelector,
        Func<T, string> statusSelector,
        Func<T, DateTime?> activitySelector,
        ArchiveAggregateSnapshot snapshot,
        Action<ArchiveAtomicItemSnapshot, string> apply)
    {
        foreach (var row in rows)
        {
            if (!items.TryGetValue(idSelector(row), out var item))
            {
                continue;
            }

            apply(item, statusSelector(row));
            snapshot.AggregateLastActivityUtc = Max(snapshot.AggregateLastActivityUtc, AsUtc(activitySelector(row)));
        }
    }

    private static DateTime? AsUtc(DateTime? value)
        => value.HasValue ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc) : null;

    private static DateTime? Max(DateTime? left, DateTime? right)
    {
        if (!left.HasValue)
        {
            return right;
        }

        if (!right.HasValue)
        {
            return left;
        }

        return left >= right ? left : right;
    }
}
