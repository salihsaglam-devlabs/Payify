namespace LinkPara.Card.Application.Commons.Models.Reconciliation;

public class AlertOptions
{
    public const bool DefaultEnabled = true;
    public const string DefaultTemplateName = "ReconciliationAlertTemplate";
    public const int DefaultBatchSize = 10_000_000;
    public const bool DefaultIncludeFailed = true;

    public bool? Enabled { get; set; }
    public string TemplateName { get; set; }
    public string[] ToEmails { get; set; }
    public int? BatchSize { get; set; }
    public bool? IncludeFailed { get; set; }

    public void ValidateAndApplyDefaults()
    {
        Enabled ??= DefaultEnabled;
        TemplateName = string.IsNullOrWhiteSpace(TemplateName) ? DefaultTemplateName : TemplateName;
        ToEmails ??= Array.Empty<string>();
        BatchSize ??= DefaultBatchSize;
        IncludeFailed ??= DefaultIncludeFailed;

        if (BatchSize <= 0)
            throw new InvalidOperationException($"Reconciliation.Alert.BatchSize must be positive. Current: {BatchSize}");
    }

    [Obsolete("Use ValidateAndApplyDefaults() instead")]
    public void Validate() => ValidateAndApplyDefaults();
}