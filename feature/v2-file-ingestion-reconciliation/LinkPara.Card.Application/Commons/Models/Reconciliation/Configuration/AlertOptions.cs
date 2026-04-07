namespace LinkPara.Card.Application.Commons.Models.Reconciliation;

public class AlertOptions
{
    public bool Enabled { get; set; } = true;
    public string TemplateNameTr { get; set; } = "ReconciliationAlertTemplate_TR";
    public string TemplateNameEn { get; set; } = "ReconciliationAlertTemplate_EN";
    public string DefaultLanguage { get; set; } = "tr";
    public string[] ToEmails { get; set; } = Array.Empty<string>();
    public int BatchSize { get; set; } = 100;
    public bool IncludeFailed { get; set; } = true;
}