using System.Text.Json;
using LinkPara.Card.Application.Commons.Models.Archive;
using LinkPara.Card.Domain.Entities.Archive;
using LinkPara.Card.Infrastructure.Persistence;
using LinkPara.Card.Infrastructure.Services.Audit;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.Card.Infrastructure.Services.Archive;

internal sealed class ArchiveExecutor
{
    private readonly CardDbContext _dbContext;
    private readonly ArchiveAggregateReader _reader;
    private readonly ArchiveEligibilityEvaluator _evaluator;
    private readonly ArchiveVerifier _verifier;
    private readonly IArchiveSqlDialect _sqlDialect;
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
        _verifier = verifier;
        _sqlDialect = sqlDialect;
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
            var eligibility = _evaluator.Evaluate(snapshot, DateTime.UtcNow);
            if (!eligibility.IsEligible)
            {
                await transaction.RollbackAsync(cancellationToken);
                return new ArchiveRunItemResult
                {
                    AggregateId = ingestionFileId,
                    Status = "Skipped",
                    FailureReasons = eligibility.FailureReasons
                };
            }

            var liveCounts = await _verifier.GetLiveCountsAsync(ingestionFileId, cancellationToken);

            await CopyAggregateAsync(ingestionFileId, auditStamp.Timestamp, cancellationToken);

            var archiveCounts = await _verifier.GetArchiveCountsAsync(ingestionFileId, cancellationToken);
            _verifier.EnsureArchiveCountsMatch(liveCounts, archiveCounts);

            await DeleteAggregateAsync(ingestionFileId, cancellationToken);

            var remainingLiveCounts = await _verifier.GetLiveCountsAsync(ingestionFileId, cancellationToken);
            _verifier.EnsureLiveCountsCleared(remainingLiveCounts);

            await transaction.CommitAsync(cancellationToken);

            return new ArchiveRunItemResult
            {
                AggregateId = ingestionFileId,
                Status = "Archived",
                ArchiveRunId = archiveRunId
            };
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            return new ArchiveRunItemResult
            {
                AggregateId = ingestionFileId,
                Status = "Failed",
                Message = ex.Message,
                FailureReasons = new List<string> { "ARCHIVE_EXECUTION_FAILED" }
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
            FailureReasonsJson = JsonSerializer.Serialize(item.FailureReasons),
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
        batch.Status = "Completed";
        batch.ProcessedCount = response.ProcessedCount;
        batch.ArchivedCount = response.ArchivedCount;
        batch.SkippedCount = response.SkippedCount;
        batch.FailedCount = response.FailedCount;
        _auditStampService.StampForUpdate(batch);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Copies the aggregate to archive tables using raw SQL.
    /// Parameters: {0}=archivedAt, {1}=ingestionFileId
    /// </summary>
    private async Task CopyAggregateAsync(Guid ingestionFileId, DateTime archivedAt, CancellationToken cancellationToken)
    {
        var copySqls = new[]
        {
            _sqlDialect.BuildCopyIngestionFileSql(),
            _sqlDialect.BuildCopyIngestionFileLineSql(),
            _sqlDialect.BuildCopyReconciliationEvaluationSql(),
            _sqlDialect.BuildCopyReconciliationOperationSql(),
            _sqlDialect.BuildCopyReconciliationReviewSql(),
            _sqlDialect.BuildCopyReconciliationOperationExecutionSql(),
            _sqlDialect.BuildCopyReconciliationAlertSql()
        };

        foreach (var sql in copySqls)
        {
            await _dbContext.Database.ExecuteSqlRawAsync(sql, new object[] { archivedAt, ingestionFileId }, cancellationToken);
        }
    }

    private async Task DeleteAggregateAsync(Guid ingestionFileId, CancellationToken cancellationToken)
    {
        var deleteSqls = new[]
        {
            _sqlDialect.BuildDeleteReconciliationAlertSql(),
            _sqlDialect.BuildDeleteReconciliationOperationExecutionSql(),
            _sqlDialect.BuildDeleteReconciliationReviewSql(),
            _sqlDialect.BuildDeleteReconciliationOperationSql(),
            _sqlDialect.BuildDeleteReconciliationEvaluationSql(),
            _sqlDialect.BuildDeleteIngestionFileLineSql(),
            _sqlDialect.BuildDeleteIngestionFileSql()
        };

        foreach (var sql in deleteSqls)
        {
            await _dbContext.Database.ExecuteSqlRawAsync(sql, new object[] { ingestionFileId }, cancellationToken);
        }
    }
}
