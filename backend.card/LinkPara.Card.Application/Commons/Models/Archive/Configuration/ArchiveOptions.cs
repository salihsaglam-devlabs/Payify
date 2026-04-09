namespace LinkPara.Card.Application.Commons.Models.Archive;

public class ArchiveOptions
{
    public const string SectionName = "Archive";

    public bool? Enabled { get; set; }

    public ArchiveDefaultOptions Defaults { get; set; }

    public ArchiveRuleOptions Rules { get; set; }

    public ArchiveTerminalStatusOptions TerminalStatuses { get; set; }

    public void Validate()
    {
        if (Enabled is null)
            throw new InvalidOperationException("Vault configuration missing: Archive.Enabled");
        if (Defaults is null)
            throw new InvalidOperationException("Vault configuration missing: Archive.Defaults");
        if (Rules is null)
            throw new InvalidOperationException("Vault configuration missing: Archive.Rules");
        if (TerminalStatuses is null)
            throw new InvalidOperationException("Vault configuration missing: Archive.TerminalStatuses");

        Defaults.Validate();
        Rules.Validate();
        TerminalStatuses.Validate();
    }
}

public class ArchiveDefaultOptions
{
    public int? PreviewLimit { get; set; }

    public int? MaxRunCount { get; set; }

    public bool? ContinueOnError { get; set; }

    public bool? UseConfiguredBeforeDateOnly { get; set; }

    public string DefaultBeforeDateStrategy { get; set; }

    public void Validate()
    {
        if (PreviewLimit is null)
            throw new InvalidOperationException("Vault configuration missing: Archive.Defaults.PreviewLimit");
        if (MaxRunCount is null)
            throw new InvalidOperationException("Vault configuration missing: Archive.Defaults.MaxRunCount");
        if (ContinueOnError is null)
            throw new InvalidOperationException("Vault configuration missing: Archive.Defaults.ContinueOnError");
        if (UseConfiguredBeforeDateOnly is null)
            throw new InvalidOperationException("Vault configuration missing: Archive.Defaults.UseConfiguredBeforeDateOnly");
        if (string.IsNullOrWhiteSpace(DefaultBeforeDateStrategy))
            throw new InvalidOperationException("Vault configuration missing: Archive.Defaults.DefaultBeforeDateStrategy");
        if (PreviewLimit <= 0)
            throw new InvalidOperationException($"Archive.Defaults.PreviewLimit must be positive. Current: {PreviewLimit}");
        if (MaxRunCount <= 0)
            throw new InvalidOperationException($"Archive.Defaults.MaxRunCount must be positive. Current: {MaxRunCount}");
    }
}

public class ArchiveRuleOptions
{
    public int? RetentionDays { get; set; }

    public int? MinLastUpdateAgeHours { get; set; }

    public bool? RetentionOnlyMode { get; set; }

    public void Validate()
    {
        if (RetentionDays is null)
            throw new InvalidOperationException("Vault configuration missing: Archive.Rules.RetentionDays");
        if (MinLastUpdateAgeHours is null)
            throw new InvalidOperationException("Vault configuration missing: Archive.Rules.MinLastUpdateAgeHours");
        if (RetentionOnlyMode is null)
            throw new InvalidOperationException("Vault configuration missing: Archive.Rules.RetentionOnlyMode");
        if (RetentionDays < 0)
            throw new InvalidOperationException($"Archive.Rules.RetentionDays must be non-negative. Current: {RetentionDays}");
        if (MinLastUpdateAgeHours < 0)
            throw new InvalidOperationException($"Archive.Rules.MinLastUpdateAgeHours must be non-negative. Current: {MinLastUpdateAgeHours}");
    }
}

public class ArchiveTerminalStatusOptions
{
    public string[] IngestionFile { get; set; }

    public string[] IngestionFileLine { get; set; }

    public string[] IngestionFileLineReconciliation { get; set; }

    public string[] ReconciliationEvaluation { get; set; }

    public string[] ReconciliationOperation { get; set; }

    public string[] ReconciliationReview { get; set; }

    public string[] ReconciliationOperationExecution { get; set; }

    public string[] ReconciliationAlert { get; set; }

    public void Validate()
    {
        if (IngestionFile is null)
            throw new InvalidOperationException("Vault configuration missing: Archive.TerminalStatuses.IngestionFile");
        if (IngestionFileLine is null)
            throw new InvalidOperationException("Vault configuration missing: Archive.TerminalStatuses.IngestionFileLine");
        if (IngestionFileLineReconciliation is null)
            throw new InvalidOperationException("Vault configuration missing: Archive.TerminalStatuses.IngestionFileLineReconciliation");
        if (ReconciliationEvaluation is null)
            throw new InvalidOperationException("Vault configuration missing: Archive.TerminalStatuses.ReconciliationEvaluation");
        if (ReconciliationOperation is null)
            throw new InvalidOperationException("Vault configuration missing: Archive.TerminalStatuses.ReconciliationOperation");
        if (ReconciliationReview is null)
            throw new InvalidOperationException("Vault configuration missing: Archive.TerminalStatuses.ReconciliationReview");
        if (ReconciliationOperationExecution is null)
            throw new InvalidOperationException("Vault configuration missing: Archive.TerminalStatuses.ReconciliationOperationExecution");
        if (ReconciliationAlert is null)
            throw new InvalidOperationException("Vault configuration missing: Archive.TerminalStatuses.ReconciliationAlert");
    }
}
