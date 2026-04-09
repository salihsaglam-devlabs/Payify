using System.ComponentModel.DataAnnotations;

namespace LinkPara.Card.Application.Commons.Models.Archive;

public class ArchiveCandidateResult
{
    public Guid AggregateId { get; set; }

    public bool IsEligible { get; set; }

    [Required]
    public List<string> FailureReasons { get; set; } = new();

    public ArchiveAggregateCounts Counts { get; set; } = new();
}

public class ArchiveRunItemResult
{
    public Guid AggregateId { get; set; }

    [Required]
    [MaxLength(32)]
    public string Status { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Message { get; set; }

    [Required]
    public List<string> FailureReasons { get; set; } = new();

    public Guid? ArchiveRunId { get; set; }
}

public class ArchiveEligibilityResult
{
    public Guid AggregateId { get; set; }

    public bool IsEligible { get; set; }

    public List<string> FailureReasons { get; set; } = new();
}

public class ArchiveAggregateCounts
{
    public int IngestionFileCount { get; set; }

    public int IngestionFileLineCount { get; set; }

    public int ReconciliationEvaluationCount { get; set; }

    public int ReconciliationOperationCount { get; set; }

    public int ReconciliationReviewCount { get; set; }

    public int ReconciliationOperationExecutionCount { get; set; }

    public int ReconciliationAlertCount { get; set; }
}

public class ArchiveAggregateSnapshot
{
    public Guid AggregateId { get; set; }

    public DateTime? FileCreateDateUtc { get; set; }

    public DateTime? LastUpdateUtc { get; set; }

    public string? IngestionFileStatus { get; set; }

    public ArchiveAggregateCounts Counts { get; set; } = new();

    public HashSet<string> IngestionFileStatuses { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    public HashSet<string> IngestionFileLineStatuses { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    public HashSet<string> IngestionFileLineReconciliationStatuses { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    public HashSet<string> ReconciliationEvaluationStatuses { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    public HashSet<string> ReconciliationOperationStatuses { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    public HashSet<string> ReconciliationReviewStatuses { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    public HashSet<string> ReconciliationOperationExecutionStatuses { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    public HashSet<string> ReconciliationAlertStatuses { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    public bool HasActiveLease { get; set; }

    public bool HasAnyOperationLease { get; set; }

    public bool HasScheduledRetryAttempt { get; set; }

    public bool ExistsInArchive { get; set; }
}
