using LinkPara.Card.Application.Commons.Models.Archive;
using Microsoft.Extensions.Options;

namespace LinkPara.Card.Infrastructure.Services.Archive;

internal sealed class ArchiveEligibilityEvaluator
{
    private readonly ArchiveOptions _options;

    public ArchiveEligibilityEvaluator(IOptions<ArchiveOptions> options)
    {
        _options = options.Value ?? throw new InvalidOperationException("ArchiveOptions is not configured.");
    }

    public ArchiveEligibilityResult Evaluate(ArchiveAggregateSnapshot? snapshot, DateTime now)
    {
        var result = new ArchiveEligibilityResult
        {
            IngestionFileId = snapshot?.IngestionFileId ?? Guid.Empty,
            IsEligible = true
        };

        if (_options.Enabled != true)
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

        if (snapshot.FileCreateDate.HasValue &&
            snapshot.FileCreateDate.Value > now.AddDays(-_options.Rules.RetentionDays.Value))
        {
            result.FailureReasons.Add("RETENTION_WINDOW_NOT_REACHED");
        }

        if (snapshot.LastUpdate.HasValue &&
            snapshot.LastUpdate.Value > now.AddHours(-_options.Rules.MinLastUpdateAgeHours.Value))
        {
            result.FailureReasons.Add("MIN_LAST_UPDATE_AGE_NOT_REACHED");
        }

        if (_options.Rules.RetentionOnlyMode != true)
        {
            var t = _options.TerminalStatuses;

            EnsureAllTerminal(snapshot.IngestionFileStatuses, t.IngestionFile, "INGESTION_FILE_NOT_TERMINAL", result);
            EnsureAllTerminal(snapshot.IngestionFileLineStatuses, t.IngestionFileLine, "INGESTION_FILE_LINE_NOT_TERMINAL", result);
            EnsureAllTerminal(snapshot.IngestionFileLineReconciliationStatuses, t.IngestionFileLineReconciliation, "INGESTION_FILE_LINE_RECONCILIATION_NOT_TERMINAL", result);
            EnsureAllTerminal(snapshot.ReconciliationEvaluationStatuses, t.ReconciliationEvaluation, "RECONCILIATION_EVALUATION_NOT_TERMINAL", result);
            EnsureAllTerminal(snapshot.ReconciliationOperationStatuses, t.ReconciliationOperation, "RECONCILIATION_OPERATION_NOT_TERMINAL", result);
            EnsureAllTerminal(snapshot.ReconciliationReviewStatuses, t.ReconciliationReview, "RECONCILIATION_REVIEW_NOT_TERMINAL", result);
            EnsureAllTerminal(snapshot.ReconciliationOperationExecutionStatuses, t.ReconciliationOperationExecution, "RECONCILIATION_OPERATION_EXECUTION_NOT_TERMINAL", result);
            EnsureAllTerminal(snapshot.ReconciliationAlertStatuses, t.ReconciliationAlert, "RECONCILIATION_ALERT_NOT_TERMINAL", result);
        }

        result.IsEligible = result.FailureReasons.Count == 0;
        return result;
    }

    private static void EnsureAllTerminal(
        IReadOnlyCollection<string> actualStatuses,
        IReadOnlyCollection<string> terminalStatuses,
        string failureReason,
        ArchiveEligibilityResult result)
    {
        if (actualStatuses.Count == 0)
        {
            return;
        }

        var allowed = new HashSet<string>(
            terminalStatuses.Where(x => !string.IsNullOrWhiteSpace(x)),
            StringComparer.OrdinalIgnoreCase);

        if (allowed.Count == 0 || actualStatuses.Any(x => !allowed.Contains(x)))
        {
            result.FailureReasons.Add(failureReason);
        }
    }
}
