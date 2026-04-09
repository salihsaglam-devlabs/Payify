namespace LinkPara.Card.Application.Commons.Models.Archive;

public class ArchiveOptions
{
    public const string SectionName = "Archive";

    public bool Enabled { get; set; } = true;

    public ArchiveDefaultOptions Defaults { get; set; } = new();

    public ArchiveRuleOptions Rules { get; set; } = new();

    public ArchiveStatusOptions Statuses { get; set; } = new();

    public ArchiveOptions Normalize()
    {
        Defaults ??= new ArchiveDefaultOptions();
        Rules ??= new ArchiveRuleOptions();
        Statuses ??= new ArchiveStatusOptions();

        Statuses.TerminalStatuses ??= new ArchiveEntityStatusOptions();
        Statuses.NonTerminalBlockingStatuses ??= new ArchiveEntityStatusOptions();
        Statuses.ReviewPendingStatuses ??= [];
        Statuses.AlertPendingStatuses ??= [];
        Statuses.RetryPendingOperationStatuses ??= [];

        Statuses.TerminalStatuses.Normalize();
        Statuses.NonTerminalBlockingStatuses.Normalize();

        Defaults.DefaultBeforeDateStrategy ??= "RetentionDays";

        return this;
    }
}

public class ArchiveDefaultOptions
{
    public int PreviewLimit { get; set; } = 100;

    public int MaxRunCount { get; set; } = 100;

    public bool ContinueOnError { get; set; } = false;

    public bool UseConfiguredBeforeDateOnly { get; set; }

    public string DefaultBeforeDateStrategy { get; set; } = "RetentionDays";
}

public class ArchiveRuleOptions
{
    public int RetentionDays { get; set; } = 30;

    public int MinLastUpdateAgeHours { get; set; } = 24;

    public bool UseAggregateLastActivityForMinAge { get; set; } = true;

    public bool RequireAllReviewsClosed { get; set; } = true;

    public bool RequireAllAlertsResolved { get; set; } = true;

    public bool ActiveLeaseEnabled { get; set; } = true;

    public bool RequireFileArchiveRecordBeforeCleanup { get; set; } = true;

    public bool RequireAllAtomicItemsArchivedBeforeCleanup { get; set; } = true;
}

public class ArchiveStatusOptions
{
    public ArchiveEntityStatusOptions TerminalStatuses { get; set; } = new();

    public ArchiveEntityStatusOptions NonTerminalBlockingStatuses { get; set; } = new();

    public string[] ReviewPendingStatuses { get; set; } = [];

    public string[] AlertPendingStatuses { get; set; } = [];

    public string[] RetryPendingOperationStatuses { get; set; } = [];
}

public class ArchiveEntityStatusOptions
{
    public string[] IngestionFile { get; set; } = [];

    public string[] IngestionFileLine { get; set; } = [];

    public string[] IngestionFileLineReconciliation { get; set; } = [];

    public string[] ReconciliationEvaluation { get; set; } = [];

    public string[] ReconciliationOperation { get; set; } = [];

    public string[] ReconciliationReview { get; set; } = [];

    public string[] ReconciliationOperationExecution { get; set; } = [];

    public string[] ReconciliationAlert { get; set; } = [];

    public void Normalize()
    {
        IngestionFile ??= [];
        IngestionFileLine ??= [];
        IngestionFileLineReconciliation ??= [];
        ReconciliationEvaluation ??= [];
        ReconciliationOperation ??= [];
        ReconciliationReview ??= [];
        ReconciliationOperationExecution ??= [];
        ReconciliationAlert ??= [];
    }
}
