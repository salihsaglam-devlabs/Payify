namespace LinkPara.Card.Application.Commons.Models.Archive.Configuration;

public class ArchiveOptions
{
    public const string SectionName = "Archive";

    public const bool DefaultEnabled = true;

    public bool? Enabled { get; set; }

    public ArchiveDefaultOptions Defaults { get; set; }

    public ArchiveRuleOptions Rules { get; set; }

    public ArchiveTerminalStatusOptions TerminalStatuses { get; set; }

    public void ValidateAndApplyDefaults()
    {
        Enabled ??= DefaultEnabled;
        Defaults ??= new ArchiveDefaultOptions();
        Rules ??= new ArchiveRuleOptions();
        TerminalStatuses ??= new ArchiveTerminalStatusOptions();

        Defaults.ValidateAndApplyDefaults();
        Rules.ValidateAndApplyDefaults();
        TerminalStatuses.ValidateAndApplyDefaults();
    }

    [Obsolete("Use ValidateAndApplyDefaults() instead")]
    public void Validate() => ValidateAndApplyDefaults();
}

public class ArchiveDefaultOptions
{
    public const int DefaultPreviewLimit = 1_000;
    public const int DefaultMaxRunCount = 5_000;
    public const bool DefaultContinueOnError = false;
    public const bool DefaultUseConfiguredBeforeDateOnly = false;
    public const string DefaultDefaultBeforeDateStrategy = "RetentionDays";

    public int? PreviewLimit { get; set; }

    public int? MaxRunCount { get; set; }

    public bool? ContinueOnError { get; set; }

    public bool? UseConfiguredBeforeDateOnly { get; set; }

    public string DefaultBeforeDateStrategy { get; set; }

    public void ValidateAndApplyDefaults()
    {
        PreviewLimit ??= DefaultPreviewLimit;
        MaxRunCount ??= DefaultMaxRunCount;
        ContinueOnError ??= DefaultContinueOnError;
        UseConfiguredBeforeDateOnly ??= DefaultUseConfiguredBeforeDateOnly;
        DefaultBeforeDateStrategy = string.IsNullOrWhiteSpace(DefaultBeforeDateStrategy)
            ? DefaultDefaultBeforeDateStrategy
            : DefaultBeforeDateStrategy;

        if (PreviewLimit <= 0)
            throw new InvalidOperationException($"Archive.Defaults.PreviewLimit must be positive. Current: {PreviewLimit}");
        if (MaxRunCount <= 0)
            throw new InvalidOperationException($"Archive.Defaults.MaxRunCount must be positive. Current: {MaxRunCount}");
    }

    [Obsolete("Use ValidateAndApplyDefaults() instead")]
    public void Validate() => ValidateAndApplyDefaults();
}

public class ArchiveRuleOptions
{
    public const int DefaultRetentionDays = 90;
    public const int DefaultMinLastUpdateAgeHours = 72;
    public const bool DefaultRetentionOnlyMode = false;

    public int? RetentionDays { get; set; }

    public int? MinLastUpdateAgeHours { get; set; }

    public bool? RetentionOnlyMode { get; set; }

    public void ValidateAndApplyDefaults()
    {
        RetentionDays ??= DefaultRetentionDays;
        MinLastUpdateAgeHours ??= DefaultMinLastUpdateAgeHours;
        RetentionOnlyMode ??= DefaultRetentionOnlyMode;

        if (RetentionDays < 0)
            throw new InvalidOperationException($"Archive.Rules.RetentionDays must be non-negative. Current: {RetentionDays}");
        if (MinLastUpdateAgeHours < 0)
            throw new InvalidOperationException($"Archive.Rules.MinLastUpdateAgeHours must be non-negative. Current: {MinLastUpdateAgeHours}");
    }

    [Obsolete("Use ValidateAndApplyDefaults() instead")]
    public void Validate() => ValidateAndApplyDefaults();
}

public class ArchiveTerminalStatusOptions
{
    private static readonly string[] DefaultTerminalStatuses = new[] { "Success", "Failed" };
    private static readonly string[] DefaultOperationStatuses = new[] { "Completed", "Failed", "Cancelled" };
    private static readonly string[] DefaultReviewStatuses = new[] { "Approved", "Rejected", "Cancelled" };
    private static readonly string[] DefaultExecutionStatuses = new[] { "Completed", "Failed", "Skipped" };
    private static readonly string[] DefaultAlertStatuses = new[] { "Consumed", "Failed", "Ignored" };

    public string[] IngestionFile { get; set; }

    public string[] IngestionFileLine { get; set; }

    public string[] IngestionFileLineReconciliation { get; set; }

    public string[] ReconciliationEvaluation { get; set; }

    public string[] ReconciliationOperation { get; set; }

    public string[] ReconciliationReview { get; set; }

    public string[] ReconciliationOperationExecution { get; set; }

    public string[] ReconciliationAlert { get; set; }

    public void ValidateAndApplyDefaults()
    {
        IngestionFile ??= DefaultTerminalStatuses;
        IngestionFileLine ??= DefaultTerminalStatuses;
        IngestionFileLineReconciliation ??= DefaultTerminalStatuses;
        ReconciliationEvaluation ??= new[] { "Completed", "Failed" };
        ReconciliationOperation ??= DefaultOperationStatuses;
        ReconciliationReview ??= DefaultReviewStatuses;
        ReconciliationOperationExecution ??= DefaultExecutionStatuses;
        ReconciliationAlert ??= DefaultAlertStatuses;
    }

    [Obsolete("Use ValidateAndApplyDefaults() instead")]
    public void Validate() => ValidateAndApplyDefaults();
}
