using LinkPara.Card.Application.Commons.Models.Archive;
using Microsoft.Extensions.Options;

namespace LinkPara.Card.Infrastructure.Services.Archive;

internal sealed class ArchiveEligibilityEvaluator
{
    private readonly ArchiveOptions _options;

    public ArchiveEligibilityEvaluator(IOptions<ArchiveOptions> options)
    {
        _options = (options.Value ?? new ArchiveOptions()).Normalize();
    }

    public ArchiveEligibilityResult Evaluate(ArchiveAggregateSnapshot? snapshot, DateTime utcNow)
    {
        var result = new ArchiveEligibilityResult
        {
            AggregateId = snapshot?.AggregateId ?? Guid.Empty,
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

        if (snapshot.FileCreateDateUtc.HasValue &&
            snapshot.FileCreateDateUtc.Value > utcNow.AddDays(-_options.Rules.RetentionDays))
        {
            result.FailureReasons.Add("RETENTION_WINDOW_NOT_REACHED");
        }

        var lastActivityUtc = _options.Rules.UseAggregateLastActivityForMinAge
            ? snapshot.AggregateLastActivityUtc ?? snapshot.LastUpdateUtc
            : snapshot.LastUpdateUtc;
        if (lastActivityUtc.HasValue &&
            lastActivityUtc.Value > utcNow.AddHours(-_options.Rules.MinLastUpdateAgeHours))
        {
            result.FailureReasons.Add("MIN_LAST_UPDATE_AGE_NOT_REACHED");
        }

        foreach (var item in snapshot.ItemSnapshots)
        {
            item.IsCompleted = IsCompleted(item);
            item.IsEligible = !item.IsArchived && item.IsCompleted && !item.HasActiveLease && !item.HasScheduledRetryAttempt;
        }

        snapshot.AtomicItems.CompletedCount = snapshot.ItemSnapshots.Count(x => x.IsCompleted);
        snapshot.AtomicItems.EligibleCount = snapshot.ItemSnapshots.Count(x => x.IsEligible);
        snapshot.AtomicItems.ArchivedCount = snapshot.ItemSnapshots.Count(x => x.IsArchived);

        snapshot.Lifecycle.ArchiveRecordWritten = snapshot.Lifecycle.ArchiveRecordWritten || snapshot.ExistsInArchive;
        snapshot.Lifecycle.ChildrenFullyTransitioned =
            !_options.Rules.RequireAllAtomicItemsArchivedBeforeCleanup ||
            snapshot.AtomicItems.TotalCount == 0 ||
            snapshot.ItemSnapshots.All(x => x.IsArchived);
        snapshot.Lifecycle.CleanupEligible =
            (!_options.Rules.RequireFileArchiveRecordBeforeCleanup || snapshot.Lifecycle.ArchiveRecordWritten) &&
            snapshot.Lifecycle.ChildrenFullyTransitioned &&
            !snapshot.Lifecycle.CleanupCompleted &&
            !result.FailureReasons.Contains("RETENTION_WINDOW_NOT_REACHED") &&
            !result.FailureReasons.Contains("MIN_LAST_UPDATE_AGE_NOT_REACHED");

        if (snapshot.Lifecycle.CleanupCompleted)
        {
            result.FailureReasons.Add("ALREADY_CLEANED");
        }

        var requiresFileRecordMaterialization =
            !snapshot.Lifecycle.ArchiveRecordWritten &&
            (snapshot.AtomicItems.TotalCount == 0 ||
             snapshot.AtomicItems.EligibleCount > 0 ||
             snapshot.AtomicItems.ArchivedCount > 0);

        if (snapshot.AtomicItems.EligibleCount == 0 &&
            !snapshot.Lifecycle.CleanupEligible &&
            !requiresFileRecordMaterialization &&
            !snapshot.Lifecycle.CleanupCompleted)
        {
            result.FailureReasons.Add("NO_ELIGIBLE_ATOMIC_ITEMS");
        }

        result.IsEligible = result.FailureReasons.Count == 0;
        return result;
    }

    private bool IsCompleted(ArchiveAtomicItemSnapshot item)
    {
        return EnsureTerminal(item.FileLineStatuses, _options.Statuses.TerminalStatuses.IngestionFileLine) &&
               EnsureTerminal(item.FileLineReconciliationStatuses, _options.Statuses.TerminalStatuses.IngestionFileLineReconciliation) &&
               EnsureTerminal(item.EvaluationStatuses, _options.Statuses.TerminalStatuses.ReconciliationEvaluation) &&
               EnsureTerminal(item.OperationStatuses, _options.Statuses.TerminalStatuses.ReconciliationOperation) &&
               EnsureTerminal(item.ReviewStatuses, _options.Statuses.TerminalStatuses.ReconciliationReview) &&
               EnsureTerminal(item.OperationExecutionStatuses, _options.Statuses.TerminalStatuses.ReconciliationOperationExecution) &&
               EnsureTerminal(item.AlertStatuses, _options.Statuses.TerminalStatuses.ReconciliationAlert) &&
               EnsureBlockingAbsent(item.FileLineStatuses, _options.Statuses.NonTerminalBlockingStatuses.IngestionFileLine) &&
               EnsureBlockingAbsent(item.FileLineReconciliationStatuses, _options.Statuses.NonTerminalBlockingStatuses.IngestionFileLineReconciliation) &&
               EnsureBlockingAbsent(item.EvaluationStatuses, _options.Statuses.NonTerminalBlockingStatuses.ReconciliationEvaluation) &&
               EnsureBlockingAbsent(item.OperationStatuses, _options.Statuses.NonTerminalBlockingStatuses.ReconciliationOperation) &&
               EnsureBlockingAbsent(item.ReviewStatuses, _options.Statuses.NonTerminalBlockingStatuses.ReconciliationReview) &&
               EnsureBlockingAbsent(item.OperationExecutionStatuses, _options.Statuses.NonTerminalBlockingStatuses.ReconciliationOperationExecution) &&
               EnsureBlockingAbsent(item.AlertStatuses, _options.Statuses.NonTerminalBlockingStatuses.ReconciliationAlert) &&
               (!_options.Rules.RequireAllReviewsClosed ||
                EnsureBlockingAbsent(item.ReviewStatuses, _options.Statuses.ReviewPendingStatuses)) &&
               (!_options.Rules.RequireAllAlertsResolved ||
                EnsureBlockingAbsent(item.AlertStatuses, _options.Statuses.AlertPendingStatuses)) &&
               EnsureBlockingAbsent(item.OperationStatuses, _options.Statuses.RetryPendingOperationStatuses);
    }

    private static bool EnsureTerminal(
        IReadOnlyCollection<string> actualStatuses,
        IReadOnlyCollection<string> configuredStatuses)
    {
        if (actualStatuses.Count == 0)
        {
            return true;
        }

        var allowed = CreateSet(configuredStatuses);
        return allowed.Count > 0 && actualStatuses.All(allowed.Contains);
    }

    private static bool EnsureBlockingAbsent(
        IReadOnlyCollection<string> actualStatuses,
        IReadOnlyCollection<string> configuredBlockingStatuses)
    {
        var blocking = CreateSet(configuredBlockingStatuses);
        return blocking.Count == 0 || actualStatuses.All(x => !blocking.Contains(x));
    }

    private static HashSet<string> CreateSet(IEnumerable<string> values)
    {
        return new HashSet<string>(values.Where(x => !string.IsNullOrWhiteSpace(x)), StringComparer.OrdinalIgnoreCase);
    }
}
