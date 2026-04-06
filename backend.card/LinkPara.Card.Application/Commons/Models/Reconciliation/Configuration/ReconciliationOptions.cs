namespace LinkPara.Card.Application.Commons.Models.Reconciliation;

public class ReconciliationOptions
{
    public const string SectionName = "Reconciliation";

    public AlertOptions Alert { get; set; } = new();
    
    public EvaluateOptions Evaluate { get; set; } = new();

    public ExecuteOptions Execute { get; set; } = new();
}
