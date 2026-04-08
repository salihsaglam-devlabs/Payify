using LinkPara.Card.Application.Commons.Models.Archive;
using LinkPara.Card.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.Card.Infrastructure.Services.Archive;

internal sealed class ArchiveVerifier
{
    private readonly CardDbContext _dbContext;
    private readonly bool _isSqlServer;

    public ArchiveVerifier(CardDbContext dbContext)
    {
        _dbContext = dbContext;
        _isSqlServer = (_dbContext.Database.ProviderName ?? string.Empty)
            .Contains("SqlServer", StringComparison.OrdinalIgnoreCase);
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

    /// <summary>
    /// Counts archive rows by ingestionFileId using raw SQL.
    /// Archive business tables mirror live tables and have no duplicate EF entity types.
    /// </summary>
    public async Task<ArchiveAggregateCounts> GetArchiveCountsAsync(Guid ingestionFileId, CancellationToken cancellationToken)
    {
        var a = ArchiveSchema;
        var fileLineSub = $"SELECT id FROM {a}.{Q("ingestion_file_line")} WHERE file_id = {{0}}";

        return new ArchiveAggregateCounts
        {
            IngestionFileCount = await CountAsync(
                $"SELECT CAST(COUNT(*) AS INTEGER) FROM {a}.{Q("ingestion_file")} WHERE id = {{0}}",
                ingestionFileId, cancellationToken),
            IngestionFileLineCount = await CountAsync(
                $"SELECT CAST(COUNT(*) AS INTEGER) FROM {a}.{Q("ingestion_file_line")} WHERE file_id = {{0}}",
                ingestionFileId, cancellationToken),
            ReconciliationEvaluationCount = await CountAsync(
                $"SELECT CAST(COUNT(*) AS INTEGER) FROM {a}.{Q("reconciliation_evaluation")} WHERE file_line_id IN ({fileLineSub})",
                ingestionFileId, cancellationToken),
            ReconciliationOperationCount = await CountAsync(
                $"SELECT CAST(COUNT(*) AS INTEGER) FROM {a}.{Q("reconciliation_operation")} WHERE file_line_id IN ({fileLineSub})",
                ingestionFileId, cancellationToken),
            ReconciliationReviewCount = await CountAsync(
                $"SELECT CAST(COUNT(*) AS INTEGER) FROM {a}.{Q("reconciliation_review")} WHERE file_line_id IN ({fileLineSub})",
                ingestionFileId, cancellationToken),
            ReconciliationOperationExecutionCount = await CountAsync(
                $"SELECT CAST(COUNT(*) AS INTEGER) FROM {a}.{Q("reconciliation_operation_execution")} WHERE file_line_id IN ({fileLineSub})",
                ingestionFileId, cancellationToken),
            ReconciliationAlertCount = await CountAsync(
                $"SELECT CAST(COUNT(*) AS INTEGER) FROM {a}.{Q("reconciliation_alert")} WHERE file_line_id IN ({fileLineSub})",
                ingestionFileId, cancellationToken)
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

    private string ArchiveSchema => Q("archive");

    private async Task<int> CountAsync(string sql, Guid param, CancellationToken cancellationToken)
    {
        return await _dbContext.Database
            .SqlQueryRaw<int>(sql, param)
            .SingleAsync(cancellationToken);
    }

    private static void EnsureEqual(int expected, int actual, string table)
    {
        if (expected != actual)
        {
            throw new InvalidOperationException($"Archive verification failed for {table}. Expected={expected}, Actual={actual}.");
        }
    }

    private string Q(string name) => _isSqlServer ? $"[{name}]" : $"\"{name}\"";
}
