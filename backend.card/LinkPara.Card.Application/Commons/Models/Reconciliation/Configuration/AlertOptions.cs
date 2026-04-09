namespace LinkPara.Card.Application.Commons.Models.Reconciliation;

public class AlertOptions
{
    public bool? Enabled { get; set; }
    public string TemplateName { get; set; }
    public string[] ToEmails { get; set; }
    public int? BatchSize { get; set; }
    public bool? IncludeFailed { get; set; }

    public void Validate()
    {
        if (Enabled is null)
            throw new InvalidOperationException("Vault configuration missing: Reconciliation.Alert.Enabled");
        if (string.IsNullOrWhiteSpace(TemplateName))
            throw new InvalidOperationException("Vault configuration missing: Reconciliation.Alert.TemplateName");
        if (ToEmails is null)
            throw new InvalidOperationException("Vault configuration missing: Reconciliation.Alert.ToEmails");
        if (BatchSize is null)
            throw new InvalidOperationException("Vault configuration missing: Reconciliation.Alert.BatchSize");
        if (IncludeFailed is null)
            throw new InvalidOperationException("Vault configuration missing: Reconciliation.Alert.IncludeFailed");
        if (BatchSize <= 0)
            throw new InvalidOperationException($"Reconciliation.Alert.BatchSize must be positive. Current: {BatchSize}");
    }
}