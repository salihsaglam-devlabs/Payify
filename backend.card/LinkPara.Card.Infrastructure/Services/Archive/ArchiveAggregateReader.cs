using LinkPara.Card.Application.Commons.Models.Archive.Contracts.Responses;
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
                LastUpdate = x.UpdateDate ?? x.CreateDate
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (file is null)
        {
            return null;
        }

        var snapshot = new ArchiveAggregateSnapshot
        {
            IngestionFileId = file.Id,
            FileCreateDate = file.CreateDate,
            LastUpdate = file.LastUpdate
        };

        snapshot.Counts.IngestionFileCount = 1;
        snapshot.IngestionFileStatuses.Add(file.Status);

        snapshot.Counts.IngestionFileLineCount = await _dbContext.IngestionFileLines
            .AsNoTracking()
            .Where(x => x.FileId == ingestionFileId)
            .CountAsync(cancellationToken);

        foreach (var status in await _dbContext.IngestionFileLines
                     .AsNoTracking()
                     .Where(x => x.FileId == ingestionFileId)
                     .Select(x => x.Status.ToString())
                     .Distinct()
                     .ToListAsync(cancellationToken))
        {
            snapshot.IngestionFileLineStatuses.Add(status);
        }

        foreach (var status in await _dbContext.IngestionFileLines
                     .AsNoTracking()
                     .Where(x => x.FileId == ingestionFileId && x.ReconciliationStatus != null)
                     .Select(x => x.ReconciliationStatus!.Value.ToString())
                     .Distinct()
                     .ToListAsync(cancellationToken))
        {
            snapshot.IngestionFileLineReconciliationStatuses.Add(status);
        }

        var fileLineIds = await _dbContext.IngestionFileLines
            .AsNoTracking()
            .Where(x => x.FileId == ingestionFileId)
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        if (fileLineIds.Count == 0)
        {
            snapshot.ExistsInArchive = await ExistsInArchiveAsync(ingestionFileId, cancellationToken);
            return snapshot;
        }

        snapshot.Counts.IngestionCardVisaDetailCount = await _dbContext.IngestionCardVisaDetails
            .AsNoTracking()
            .Where(x => fileLineIds.Contains(x.FileLineId))
            .CountAsync(cancellationToken);

        snapshot.Counts.IngestionCardMscDetailCount = await _dbContext.IngestionCardMscDetails
            .AsNoTracking()
            .Where(x => fileLineIds.Contains(x.FileLineId))
            .CountAsync(cancellationToken);

        snapshot.Counts.IngestionCardBkmDetailCount = await _dbContext.IngestionCardBkmDetails
            .AsNoTracking()
            .Where(x => fileLineIds.Contains(x.FileLineId))
            .CountAsync(cancellationToken);

        snapshot.Counts.IngestionClearingVisaDetailCount = await _dbContext.IngestionClearingVisaDetails
            .AsNoTracking()
            .Where(x => fileLineIds.Contains(x.FileLineId))
            .CountAsync(cancellationToken);

        snapshot.Counts.IngestionClearingMscDetailCount = await _dbContext.IngestionClearingMscDetails
            .AsNoTracking()
            .Where(x => fileLineIds.Contains(x.FileLineId))
            .CountAsync(cancellationToken);

        snapshot.Counts.IngestionClearingBkmDetailCount = await _dbContext.IngestionClearingBkmDetails
            .AsNoTracking()
            .Where(x => fileLineIds.Contains(x.FileLineId))
            .CountAsync(cancellationToken);

        snapshot.Counts.ReconciliationEvaluationCount = await _dbContext.ReconciliationEvaluations
            .AsNoTracking()
            .Where(x => fileLineIds.Contains(x.FileLineId))
            .CountAsync(cancellationToken);

        foreach (var status in await _dbContext.ReconciliationEvaluations
                     .AsNoTracking()
                     .Where(x => fileLineIds.Contains(x.FileLineId))
                     .Select(x => x.Status.ToString())
                     .Distinct()
                     .ToListAsync(cancellationToken))
        {
            snapshot.ReconciliationEvaluationStatuses.Add(status);
        }

        snapshot.Counts.ReconciliationOperationCount = await _dbContext.ReconciliationOperations
            .AsNoTracking()
            .Where(x => fileLineIds.Contains(x.FileLineId))
            .CountAsync(cancellationToken);

        foreach (var status in await _dbContext.ReconciliationOperations
                     .AsNoTracking()
                     .Where(x => fileLineIds.Contains(x.FileLineId))
                     .Select(x => x.Status.ToString())
                     .Distinct()
                     .ToListAsync(cancellationToken))
        {
            snapshot.ReconciliationOperationStatuses.Add(status);
        }

        snapshot.Counts.ReconciliationReviewCount = await _dbContext.ReconciliationReviews
            .AsNoTracking()
            .Where(x => fileLineIds.Contains(x.FileLineId))
            .CountAsync(cancellationToken);

        foreach (var status in await _dbContext.ReconciliationReviews
                     .AsNoTracking()
                     .Where(x => fileLineIds.Contains(x.FileLineId))
                     .Select(x => x.Decision.ToString())
                     .Distinct()
                     .ToListAsync(cancellationToken))
        {
            snapshot.ReconciliationReviewStatuses.Add(status);
        }

        snapshot.Counts.ReconciliationOperationExecutionCount = await _dbContext.ReconciliationOperationExecutions
            .AsNoTracking()
            .Where(x => fileLineIds.Contains(x.FileLineId))
            .CountAsync(cancellationToken);

        foreach (var status in await _dbContext.ReconciliationOperationExecutions
                     .AsNoTracking()
                     .Where(x => fileLineIds.Contains(x.FileLineId))
                     .Select(x => x.Status.ToString())
                     .Distinct()
                     .ToListAsync(cancellationToken))
        {
            snapshot.ReconciliationOperationExecutionStatuses.Add(status);
        }

        snapshot.Counts.ReconciliationAlertCount = await _dbContext.ReconciliationAlerts
            .AsNoTracking()
            .Where(x => fileLineIds.Contains(x.FileLineId))
            .CountAsync(cancellationToken);

        foreach (var status in await _dbContext.ReconciliationAlerts
                     .AsNoTracking()
                     .Where(x => fileLineIds.Contains(x.FileLineId))
                     .Select(x => x.AlertStatus.ToString())
                     .Distinct()
                     .ToListAsync(cancellationToken))
        {
            snapshot.ReconciliationAlertStatuses.Add(status);
        }

        snapshot.LastUpdate = await ResolveAggregateLastUpdateAsync(
            ingestionFileId,
            fileLineIds,
            snapshot.LastUpdate,
            cancellationToken);

        snapshot.ExistsInArchive = await ExistsInArchiveAsync(ingestionFileId, cancellationToken);

        return snapshot;
    }

    private async Task<DateTime?> ResolveAggregateLastUpdateAsync(
        Guid ingestionFileId,
        IReadOnlyCollection<Guid> fileLineIds,
        DateTime? currentLastUpdate,
        CancellationToken cancellationToken)
    {
        DateTime? maxLastUpdate = currentLastUpdate;

        DateTime? Combine(DateTime? a, DateTime? b)
        {
            if (!a.HasValue) return b;
            if (!b.HasValue) return a;
            return a.Value >= b.Value ? a : b;
        }

        var fileLineMax = await _dbContext.IngestionFileLines
            .AsNoTracking()
            .Where(x => x.FileId == ingestionFileId)
            .MaxAsync(x => (DateTime?)(x.UpdateDate ?? x.CreateDate), cancellationToken);
        maxLastUpdate = Combine(maxLastUpdate, fileLineMax);

        if (fileLineIds.Count == 0)
        {
            return maxLastUpdate;
        }

        var evalMax = await _dbContext.ReconciliationEvaluations
            .AsNoTracking()
            .Where(x => fileLineIds.Contains(x.FileLineId))
            .MaxAsync(x => (DateTime?)(x.UpdateDate ?? x.CreateDate), cancellationToken);
        maxLastUpdate = Combine(maxLastUpdate, evalMax);

        var opMax = await _dbContext.ReconciliationOperations
            .AsNoTracking()
            .Where(x => fileLineIds.Contains(x.FileLineId))
            .MaxAsync(x => (DateTime?)(x.UpdateDate ?? x.CreateDate), cancellationToken);
        maxLastUpdate = Combine(maxLastUpdate, opMax);

        var reviewMax = await _dbContext.ReconciliationReviews
            .AsNoTracking()
            .Where(x => fileLineIds.Contains(x.FileLineId))
            .MaxAsync(x => (DateTime?)(x.UpdateDate ?? x.CreateDate), cancellationToken);
        maxLastUpdate = Combine(maxLastUpdate, reviewMax);

        var execMax = await _dbContext.ReconciliationOperationExecutions
            .AsNoTracking()
            .Where(x => fileLineIds.Contains(x.FileLineId))
            .MaxAsync(x => (DateTime?)(x.UpdateDate ?? x.CreateDate), cancellationToken);
        maxLastUpdate = Combine(maxLastUpdate, execMax);

        var alertMax = await _dbContext.ReconciliationAlerts
            .AsNoTracking()
            .Where(x => fileLineIds.Contains(x.FileLineId))
            .MaxAsync(x => (DateTime?)(x.UpdateDate ?? x.CreateDate), cancellationToken);
        maxLastUpdate = Combine(maxLastUpdate, alertMax);

        return maxLastUpdate;
    }

    private async Task<bool> ExistsInArchiveAsync(Guid ingestionFileId, CancellationToken cancellationToken)
    {
        return await _dbContext.ArchiveIngestionFiles
            .AsNoTracking()
            .AnyAsync(x => x.Id == ingestionFileId, cancellationToken);
    }
}
