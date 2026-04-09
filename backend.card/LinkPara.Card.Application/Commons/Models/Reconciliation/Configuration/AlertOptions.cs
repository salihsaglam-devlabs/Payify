namespace LinkPara.Card.Application.Commons.Models.Reconciliation;

public class AlertOptions
{
    public bool Enabled { get; set; } = true;
    public string TemplateName { get; set; } = "ReconciliationAlertTemplate";
    public string[] ToEmails { get; set; } = Array.Empty<string>();
    public int BatchSize { get; set; } = 100;
    public bool IncludeFailed { get; set; } = true;
}