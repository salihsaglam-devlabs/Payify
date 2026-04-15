using System.Data;
using System.Text.Json;
using LinkPara.Card.Application.Commons.Extensions;
using LinkPara.Card.Application.Commons.Helpers;
using LinkPara.Card.Application.Commons.Models.Archive.Contracts.Responses;
using LinkPara.Card.Domain.Entities.Archive;
using LinkPara.Card.Infrastructure.Persistence;
using LinkPara.Card.Infrastructure.Services.Audit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace LinkPara.Card.Infrastructure.Services.Archive;

internal sealed class ArchiveExecutor
{
    private readonly CardDbContext _dbContext;
    private readonly ArchiveAggregateReader _reader;
    private readonly ArchiveEligibilityEvaluator _evaluator;
    private readonly ArchiveVerifier _verifier;
    private readonly IArchiveSqlDialect _sqlDialect;
    private readonly IAuditStampService _auditStampService;
    private readonly IStringLocalizer _localizer;

    public ArchiveExecutor(
        CardDbContext dbContext,
        ArchiveAggregateReader reader,
        ArchiveEligibilityEvaluator evaluator,
        ArchiveVerifier verifier,
        IArchiveSqlDialect sqlDialect,
        IAuditStampService auditStampService,
        Func<LinkPara.Card.Application.Commons.Localization.LocalizerResource, IStringLocalizer> localizerFactory)
    {
        _dbContext = dbContext;
        _reader = reader;
        _evaluator = evaluator;
        _verifier = verifier;
        _sqlDialect = sqlDialect;
        _auditStampService = auditStampService;
        _localizer = localizerFactory(LinkPara.Card.Application.Commons.Localization.LocalizerResource.Messages);
    }

    public async Task<ArchiveRunItemResult> ExecuteAsync(Guid ingestionFileId, CancellationToken cancellationToken)
    {
        var stamp = _auditStampService.CreateStamp();
        var now = stamp.Timestamp;
        var currentStep = "INITIALIZATION";

        var strategy = _dbContext.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(IsolationLevel.RepeatableRead, cancellationToken);
            try
            {
                currentStep = "SNAPSHOT_LOAD";
                var snapshot = await _reader.GetSnapshotAsync(ingestionFileId, cancellationToken);
                if (snapshot is null)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return new ArchiveRunItemResult
                    {
                        IngestionFileId = ingestionFileId,
                        Status = "Skipped",
                        Message = _localizer.Get("Archive.SnapshotNotFound"),
                        FailureReasons = new List<string> { "SNAPSHOT_NOT_FOUND" }
                    };
                }

                currentStep = "ELIGIBILITY_CHECK";
                var eligibility = _evaluator.Evaluate(snapshot, now);
                if (!eligibility.IsEligible)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return new ArchiveRunItemResult
                    {
                        IngestionFileId = ingestionFileId,
                        Status = "Skipped",
                        Message = _localizer.Get("Archive.EligibilityCheckFailed", string.Join(", ", eligibility.FailureReasons)),
                        FailureReasons = eligibility.FailureReasons
                    };
                }

                currentStep = "LIVE_COUNT_SNAPSHOT";
                var liveCounts = await _verifier.GetLiveCountsAsync(ingestionFileId, cancellationToken);

                currentStep = "ARCHIVE_COPY";
                await CopyAggregateAsync(ingestionFileId, stamp, cancellationToken);

                currentStep = "ARCHIVE_COPY_VERIFICATION";
                var archiveCounts = await _verifier.GetArchiveCountsAsync(ingestionFileId, cancellationToken);
                _verifier.EnsureArchiveCountsMatch(liveCounts, archiveCounts);

                currentStep = "LIVE_DELETE";
                await DeleteAggregateAsync(ingestionFileId, cancellationToken);

                currentStep = "LIVE_DELETE_VERIFICATION";
                var remainingLiveCounts = await _verifier.GetLiveCountsAsync(ingestionFileId, cancellationToken);
                _verifier.EnsureLiveCountsCleared(remainingLiveCounts);

                await transaction.CommitAsync(cancellationToken);

                return new ArchiveRunItemResult
                {
                    IngestionFileId = ingestionFileId,
                    Status = "Archived"
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);

                var failureReason = MapStepToFailureReason(currentStep);
                var safeMessage = BuildSafeMessage(failureReason, ex);

                return new ArchiveRunItemResult
                {
                    IngestionFileId = ingestionFileId,
                    Status = "Failed",
                    Message = safeMessage,
                    FailureReasons = new List<string> { failureReason }
                };
            }
        });
    }

    public async Task InsertArchiveLogAsync(ArchiveRunItemResult item, string? filterJson, CancellationToken cancellationToken)
    {
        var log = new ArchiveLog
        {
            Id = Guid.NewGuid(),
            IngestionFileId = item.IngestionFileId,
            Status = item.Status,
            Message = item.Message,
            FailureReasonsJson = JsonSerializer.Serialize(item.FailureReasons ?? new List<string>()),
            FilterJson = filterJson
        };

        _auditStampService.StampForCreate(log);
        await _dbContext.ArchiveLogs.AddAsync(log, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task CopyAggregateAsync(
        Guid ingestionFileId,
        AuditStamp stamp,
        CancellationToken cancellationToken)
    {
        var copySqls = new[]
        {
            _sqlDialect.BuildCopyIngestionFileSql(),
            _sqlDialect.BuildCopyIngestionFileLineSql(),
            _sqlDialect.BuildCopyIngestionCardVisaDetailSql(),
            _sqlDialect.BuildCopyIngestionCardMscDetailSql(),
            _sqlDialect.BuildCopyIngestionCardBkmDetailSql(),
            _sqlDialect.BuildCopyIngestionClearingVisaDetailSql(),
            _sqlDialect.BuildCopyIngestionClearingMscDetailSql(),
            _sqlDialect.BuildCopyIngestionClearingBkmDetailSql(),
            _sqlDialect.BuildCopyReconciliationEvaluationSql(),
            _sqlDialect.BuildCopyReconciliationOperationSql(),
            _sqlDialect.BuildCopyReconciliationReviewSql(),
            _sqlDialect.BuildCopyReconciliationOperationExecutionSql(),
            _sqlDialect.BuildCopyReconciliationAlertSql()
        };

        foreach (var sql in copySqls)
        {
            await _dbContext.Database.ExecuteSqlRawAsync(
                sql,
                new object[] { stamp.Timestamp, stamp.UserId, ingestionFileId },
                cancellationToken);
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
            _sqlDialect.BuildDeleteIngestionCardVisaDetailSql(),
            _sqlDialect.BuildDeleteIngestionCardMscDetailSql(),
            _sqlDialect.BuildDeleteIngestionCardBkmDetailSql(),
            _sqlDialect.BuildDeleteIngestionClearingVisaDetailSql(),
            _sqlDialect.BuildDeleteIngestionClearingMscDetailSql(),
            _sqlDialect.BuildDeleteIngestionClearingBkmDetailSql(),
            _sqlDialect.BuildDeleteIngestionFileLineSql(),
            _sqlDialect.BuildDeleteIngestionFileSql()
        };

        foreach (var sql in deleteSqls)
        {
            await _dbContext.Database.ExecuteSqlRawAsync(sql, new object[] { ingestionFileId }, cancellationToken);
        }
    }

    private static string MapStepToFailureReason(string step)
    {
        return step switch
        {
            "SNAPSHOT_LOAD" => "SNAPSHOT_NOT_FOUND",
            "ELIGIBILITY_CHECK" => "ELIGIBILITY_FAILED",
            "ARCHIVE_COPY" => "SQL_GENERATION_FAILED",
            "ARCHIVE_COPY_VERIFICATION" => "ARCHIVE_COPY_COUNT_MISMATCH",
            "LIVE_DELETE" => "LIVE_DELETE_NOT_CLEARED",
            "LIVE_DELETE_VERIFICATION" => "LIVE_DELETE_NOT_CLEARED",
            _ => "EXECUTION_ERROR"
        };
    }

    private static string BuildSafeMessage(string failureReason, Exception ex)
    {
        var baseMessage = ExceptionDetailHelper.BuildDetailMessage(ex, 500);
        return $"[{failureReason}] {baseMessage}";
    }
}
