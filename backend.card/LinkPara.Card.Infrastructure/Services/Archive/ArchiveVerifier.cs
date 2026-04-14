using LinkPara.Card.Application.Commons.Exceptions;
using LinkPara.Card.Application.Commons.Extensions;
using LinkPara.Card.Application.Commons.Models.Archive.Contracts.Responses;
using LinkPara.Card.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace LinkPara.Card.Infrastructure.Services.Archive;

internal sealed class ArchiveVerifier
{
    private readonly CardDbContext _dbContext;
    private readonly IStringLocalizer _localizer;

    public ArchiveVerifier(
        CardDbContext dbContext,
        Func<LinkPara.Card.Application.Commons.Localization.LocalizerResource, IStringLocalizer> localizerFactory)
    {
        _dbContext = dbContext;
        _localizer = localizerFactory(LinkPara.Card.Application.Commons.Localization.LocalizerResource.Messages);
    }

    public async Task<ArchiveAggregateCounts> GetLiveCountsAsync(Guid ingestionFileId, CancellationToken cancellationToken)
    {
        var fileLineIdQuery = _dbContext.IngestionFileLines
            .AsNoTracking()
            .Where(x => x.IngestionFileId == ingestionFileId)
            .Select(x => x.Id);

        var counts = new ArchiveAggregateCounts
        {
            IngestionFileCount = await _dbContext.IngestionFiles.CountAsync(x => x.Id == ingestionFileId, cancellationToken),
            IngestionFileLineCount = await _dbContext.IngestionFileLines.CountAsync(x => x.IngestionFileId == ingestionFileId, cancellationToken)
        };

        if (counts.IngestionFileLineCount == 0)
        {
            return counts;
        }

        counts.ReconciliationEvaluationCount = await _dbContext.ReconciliationEvaluations.CountAsync(x => fileLineIdQuery.Contains(x.FileLineId), cancellationToken);
        counts.ReconciliationOperationCount = await _dbContext.ReconciliationOperations.CountAsync(x => fileLineIdQuery.Contains(x.FileLineId), cancellationToken);
        counts.ReconciliationReviewCount = await _dbContext.ReconciliationReviews.CountAsync(x => fileLineIdQuery.Contains(x.FileLineId), cancellationToken);
        counts.ReconciliationOperationExecutionCount = await _dbContext.ReconciliationOperationExecutions.CountAsync(x => fileLineIdQuery.Contains(x.FileLineId), cancellationToken);
        counts.ReconciliationAlertCount = await _dbContext.ReconciliationAlerts.CountAsync(x => fileLineIdQuery.Contains(x.FileLineId), cancellationToken);
        return counts;
    }
    
    public async Task<ArchiveAggregateCounts> GetArchiveCountsAsync(Guid ingestionFileId, CancellationToken cancellationToken)
    {
        var archiveFileLineIdQuery = _dbContext.ArchiveIngestionFileLines
            .AsNoTracking()
            .Where(x => x.IngestionFileId == ingestionFileId)
            .Select(x => x.Id);

        var counts = new ArchiveAggregateCounts
        {
            IngestionFileCount = await _dbContext.ArchiveIngestionFiles
                .CountAsync(x => x.Id == ingestionFileId, cancellationToken),
            IngestionFileLineCount = await _dbContext.ArchiveIngestionFileLines
                .CountAsync(x => x.IngestionFileId == ingestionFileId, cancellationToken)
        };

        if (counts.IngestionFileLineCount == 0)
        {
            return counts;
        }

        counts.ReconciliationEvaluationCount = await _dbContext.ArchiveReconciliationEvaluations
            .CountAsync(x => archiveFileLineIdQuery.Contains(x.FileLineId), cancellationToken);
        counts.ReconciliationOperationCount = await _dbContext.ArchiveReconciliationOperations
            .CountAsync(x => archiveFileLineIdQuery.Contains(x.FileLineId), cancellationToken);
        counts.ReconciliationReviewCount = await _dbContext.ArchiveReconciliationReviews
            .CountAsync(x => archiveFileLineIdQuery.Contains(x.FileLineId), cancellationToken);
        counts.ReconciliationOperationExecutionCount = await _dbContext.ArchiveReconciliationOperationExecutions
            .CountAsync(x => archiveFileLineIdQuery.Contains(x.FileLineId), cancellationToken);
        counts.ReconciliationAlertCount = await _dbContext.ArchiveReconciliationAlerts
            .CountAsync(x => archiveFileLineIdQuery.Contains(x.FileLineId), cancellationToken);

        return counts;
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
            throw new ArchiveLiveCountsNotClearedException( _localizer.Get("Archive.LiveCountsNotCleared"));
        }
    }

    private void EnsureEqual(int expected, int actual, string table)
    {
        if (expected != actual)
        {
            throw new ArchiveVerificationCountMismatchException(_localizer.Get("Archive.VerificationFailed", table, expected, actual));
        }
    }
}
