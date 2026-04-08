using LinkPara.Card.Application.Commons.Models.Archive;
using Microsoft.Extensions.Options;

namespace LinkPara.Card.Infrastructure.Services.Archive;

internal sealed class ArchiveEligibilityEvaluator
{
    private readonly ArchiveOptions _options;

    public ArchiveEligibilityEvaluator(IOptions<ArchiveOptions> options)
    {
        _options = options.Value;
    }

    public ArchiveEligibilityResult Evaluate(ArchiveAggregateSnapshot? snapshot, DateTime utcNow)
    {
        var result = new ArchiveEligibilityResult
        {
            IngestionFileId = snapshot?.IngestionFileId ?? Guid.Empty,
            IsEligible = true
        };

        if (!_options.Enabled)
        {
            result.FailureReasons.Add("ARCHIVE_DISABLED");
            result.IsEligible = false;
            return result;
        }

        if (snapshot is null)
        {
            result.FailureReasons.Add("INGESTION_FILE_NOT_FOUND");
            result.IsEligible = false;
            return result;
        }

        if (snapshot.ExistsInArchive)
        {
            result.FailureReasons.Add("ALREADY_ARCHIVED");
        }

        if (snapshot.FileCreateDateUtc.HasValue &&
            snapshot.FileCreateDateUtc.Value > utcNow.AddDays(-Math.Abs(_options.Rules.RetentionDays)))
        {
            result.FailureReasons.Add("RETENTION_WINDOW_NOT_REACHED");
        }

        if (snapshot.LastUpdateUtc.HasValue &&
            snapshot.LastUpdateUtc.Value > utcNow.AddHours(-Math.Abs(_options.Rules.MinLastUpdateAgeHours)))
        {
            result.FailureReasons.Add("MIN_LAST_UPDATE_AGE_NOT_REACHED");
        }

        if (_options.Rules.ActiveLeaseEnabled && snapshot.HasAnyOperationLease)
        {
            result.FailureReasons.Add("ACTIVE_LEASE_EXISTS");
        }

        if (snapshot.HasScheduledRetryAttempt)
        {
            result.FailureReasons.Add("RETRY_SCHEDULE_EXISTS");
        }

        EnsureTerminal(snapshot.IngestionFileStatuses, _options.Statuses.TerminalStatuses.IngestionFile, "INGESTION_FILE_NOT_TERMINAL", result);
        EnsureTerminal(snapshot.IngestionFileLineStatuses, _options.Statuses.TerminalStatuses.IngestionFileLine, "INGESTION_FILE_LINE_NOT_TERMINAL", result);
        EnsureTerminal(snapshot.IngestionFileLineReconciliationStatuses, _options.Statuses.TerminalStatuses.IngestionFileLineReconciliation, "INGESTION_FILE_LINE_RECONCILIATION_NOT_TERMINAL", result);
        EnsureTerminal(snapshot.ReconciliationEvaluationStatuses, _options.Statuses.TerminalStatuses.ReconciliationEvaluation, "RECONCILIATION_EVALUATION_NOT_TERMINAL", result);
        EnsureTerminal(snapshot.ReconciliationOperationStatuses, _options.Statuses.TerminalStatuses.ReconciliationOperation, "RECONCILIATION_OPERATION_NOT_TERMINAL", result);
        EnsureTerminal(snapshot.ReconciliationReviewStatuses, _options.Statuses.TerminalStatuses.ReconciliationReview, "RECONCILIATION_REVIEW_NOT_TERMINAL", result);
        EnsureTerminal(snapshot.ReconciliationOperationExecutionStatuses, _options.Statuses.TerminalStatuses.ReconciliationOperationExecution, "RECONCILIATION_OPERATION_EXECUTION_NOT_TERMINAL", result);
        EnsureTerminal(snapshot.ReconciliationAlertStatuses, _options.Statuses.TerminalStatuses.ReconciliationAlert, "RECONCILIATION_ALERT_NOT_TERMINAL", result);

        EnsureBlockingAbsent(snapshot.IngestionFileStatuses, _options.Statuses.NonTerminalBlockingStatuses.IngestionFile, "INGESTION_FILE_BLOCKING_STATUS_EXISTS", result);
        EnsureBlockingAbsent(snapshot.IngestionFileLineStatuses, _options.Statuses.NonTerminalBlockingStatuses.IngestionFileLine, "INGESTION_FILE_LINE_BLOCKING_STATUS_EXISTS", result);
        EnsureBlockingAbsent(snapshot.IngestionFileLineReconciliationStatuses, _options.Statuses.NonTerminalBlockingStatuses.IngestionFileLineReconciliation, "INGESTION_FILE_LINE_RECON_BLOCKING_STATUS_EXISTS", result);
        EnsureBlockingAbsent(snapshot.ReconciliationEvaluationStatuses, _options.Statuses.NonTerminalBlockingStatuses.ReconciliationEvaluation, "RECONCILIATION_EVALUATION_BLOCKING_STATUS_EXISTS", result);
        EnsureBlockingAbsent(snapshot.ReconciliationOperationStatuses, _options.Statuses.NonTerminalBlockingStatuses.ReconciliationOperation, "RECONCILIATION_OPERATION_BLOCKING_STATUS_EXISTS", result);
        EnsureBlockingAbsent(snapshot.ReconciliationReviewStatuses, _options.Statuses.NonTerminalBlockingStatuses.ReconciliationReview, "RECONCILIATION_REVIEW_BLOCKING_STATUS_EXISTS", result);
        EnsureBlockingAbsent(snapshot.ReconciliationOperationExecutionStatuses, _options.Statuses.NonTerminalBlockingStatuses.ReconciliationOperationExecution, "RECONCILIATION_OPERATION_EXECUTION_BLOCKING_STATUS_EXISTS", result);
        EnsureBlockingAbsent(snapshot.ReconciliationAlertStatuses, _options.Statuses.NonTerminalBlockingStatuses.ReconciliationAlert, "RECONCILIATION_ALERT_BLOCKING_STATUS_EXISTS", result);

        if (_options.Rules.RequireAllReviewsClosed)
        {
            EnsureBlockingAbsent(snapshot.ReconciliationReviewStatuses, _options.Statuses.ReviewPendingStatuses, "REVIEW_PENDING_STATUS_EXISTS", result);
        }

        if (_options.Rules.RequireAllAlertsResolved)
        {
            EnsureBlockingAbsent(snapshot.ReconciliationAlertStatuses, _options.Statuses.AlertPendingStatuses, "ALERT_PENDING_STATUS_EXISTS", result);
        }

        EnsureBlockingAbsent(snapshot.ReconciliationOperationStatuses, _options.Statuses.RetryPendingOperationStatuses, "RETRY_PENDING_OPERATION_STATUS_EXISTS", result);

        result.IsEligible = result.FailureReasons.Count == 0;
        return result;
    }

    private static void EnsureTerminal(
        IReadOnlyCollection<string> actualStatuses,
        IReadOnlyCollection<string> configuredStatuses,
        string failureReason,
        ArchiveEligibilityResult result)
    {
        if (actualStatuses.Count == 0)
        {
            return;
        }

        var allowed = CreateSet(configuredStatuses);
        if (allowed.Count == 0 || actualStatuses.Any(x => !allowed.Contains(x)))
        {
            result.FailureReasons.Add(failureReason);
        }
    }

    private static void EnsureBlockingAbsent(
        IReadOnlyCollection<string> actualStatuses,
        IReadOnlyCollection<string> configuredBlockingStatuses,
        string failureReason,
        ArchiveEligibilityResult result)
    {
        var blocking = CreateSet(configuredBlockingStatuses);
        if (blocking.Count == 0)
        {
            return;
        }

        if (actualStatuses.Any(blocking.Contains))
        {
            result.FailureReasons.Add(failureReason);
        }
    }

    private static HashSet<string> CreateSet(IEnumerable<string> values)
    {
        return new HashSet<string>(values.Where(x => !string.IsNullOrWhiteSpace(x)), StringComparer.OrdinalIgnoreCase);
    }
}
