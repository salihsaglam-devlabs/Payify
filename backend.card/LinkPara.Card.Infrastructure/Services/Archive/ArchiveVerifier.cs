using LinkPara.Card.Application.Commons.Models.Archive;
using LinkPara.Card.Infrastructure.Persistence;
using LinkPara.Card.Infrastructure.Persistence.ArchiveEntities;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.Card.Infrastructure.Services.Archive;

internal sealed class ArchiveVerifier
{
    private readonly CardDbContext _dbContext;

    public ArchiveVerifier(CardDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ArchiveAggregateCounts> GetLiveCountsAsync(Guid ingestionFileId, CancellationToken cancellationToken)
    {
        var counts = new ArchiveAggregateCounts
        {
            IngestionFileCount = await _dbContext.IngestionFiles.CountAsync(x => x.Id == ingestionFileId, cancellationToken),
            IngestionFileLineCount = await _dbContext.IngestionFileLines.CountAsync(x => x.IngestionFileId == ingestionFileId, cancellationToken)
        };

        var fileLineIds = await _dbContext.IngestionFileLines
            .AsNoTracking()
            .Where(x => x.IngestionFileId == ingestionFileId)
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        if (fileLineIds.Count == 0)
        {
            return counts;
        }

        counts.ReconciliationEvaluationCount = await _dbContext.ReconciliationEvaluations.CountAsync(x => fileLineIds.Contains(x.FileLineId), cancellationToken);
        counts.ReconciliationOperationCount = await _dbContext.ReconciliationOperations.CountAsync(x => fileLineIds.Contains(x.FileLineId), cancellationToken);
        counts.ReconciliationReviewCount = await _dbContext.ReconciliationReviews.CountAsync(x => fileLineIds.Contains(x.FileLineId), cancellationToken);
        counts.ReconciliationOperationExecutionCount = await _dbContext.ReconciliationOperationExecutions.CountAsync(x => fileLineIds.Contains(x.FileLineId), cancellationToken);
        counts.ReconciliationAlertCount = await _dbContext.ReconciliationAlerts.CountAsync(x => fileLineIds.Contains(x.FileLineId), cancellationToken);
        return counts;
    }

    public async Task<ArchiveAggregateCounts> GetArchiveCountsAsync(Guid archiveRunId, CancellationToken cancellationToken)
    {
        return new ArchiveAggregateCounts
        {
            IngestionFileCount = await CountArchiveAsync(_dbContext.ArchiveIngestionFiles, archiveRunId, cancellationToken),
            IngestionFileLineCount = await CountArchiveAsync(_dbContext.ArchiveIngestionFileLines, archiveRunId, cancellationToken),
            ReconciliationEvaluationCount = await CountArchiveAsync(_dbContext.ArchiveReconciliationEvaluations, archiveRunId, cancellationToken),
            ReconciliationOperationCount = await CountArchiveAsync(_dbContext.ArchiveReconciliationOperations, archiveRunId, cancellationToken),
            ReconciliationReviewCount = await CountArchiveAsync(_dbContext.ArchiveReconciliationReviews, archiveRunId, cancellationToken),
            ReconciliationOperationExecutionCount = await CountArchiveAsync(_dbContext.ArchiveReconciliationOperationExecutions, archiveRunId, cancellationToken),
            ReconciliationAlertCount = await CountArchiveAsync(_dbContext.ArchiveReconciliationAlerts, archiveRunId, cancellationToken)
        };
    }

    public void EnsureArchiveCountsMatch(ArchiveAggregateCounts liveCounts, ArchiveAggregateCounts archiveCounts)
    {
        EnsureEqual(liveCounts.IngestionFileCount, archiveCounts.IngestionFileCount, "archive.ingestion_file");
        EnsureEqual(liveCounts.IngestionFileLineCount, archiveCounts.IngestionFileLineCount, "archive.ingestion_file_line");
        EnsureEqual(liveCounts.ReconciliationEvaluationCount, archiveCounts.ReconciliationEvaluationCount, "archive.reconciliation_evaluation");
        EnsureEqual(liveCounts.ReconciliationOperationCount, archiveCounts.ReconciliationOperationCount, "archive.reconciliation_operation");
        EnsureEqual(liveCounts.ReconciliationReviewCount, archiveCounts.ReconciliationReviewCount, "archive.reconciliation_review");
        EnsureEqual(liveCounts.ReconciliationOperationExecutionCount, archiveCounts.ReconciliationOperationExecutionCount, "archive.reconciliation_operation_execution");
        EnsureEqual(liveCounts.ReconciliationAlertCount, archiveCounts.ReconciliationAlertCount, "archive.reconciliation_alert");
    }

    public void EnsureLiveCountsCleared(ArchiveAggregateCounts counts)
    {
        if (counts.IngestionFileCount != 0 ||
            counts.IngestionFileLineCount != 0 ||
            counts.ReconciliationEvaluationCount != 0 ||
            counts.ReconciliationOperationCount != 0 ||
            counts.ReconciliationReviewCount != 0 ||
            counts.ReconciliationOperationExecutionCount != 0 ||
            counts.ReconciliationAlertCount != 0)
        {
            throw new InvalidOperationException("Live aggregate rows remain after archive delete.");
        }
    }

    private static Task<int> CountArchiveAsync<TArchive>(
        IQueryable<TArchive> query,
        Guid archiveRunId,
        CancellationToken cancellationToken)
        where TArchive : class
    {
        return query.CountAsync(
            x => EF.Property<Guid>(x, nameof(ArchiveIngestionFile.ArchiveRunId)) == archiveRunId,
            cancellationToken);
    }

    private static void EnsureEqual(int expected, int actual, string table)
    {
        if (expected != actual)
        {
            throw new InvalidOperationException($"Archive verification failed for {table}. Expected={expected}, Actual={actual}.");
        }
    }
}
