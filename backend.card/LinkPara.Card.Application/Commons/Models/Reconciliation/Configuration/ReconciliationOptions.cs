namespace LinkPara.Card.Application.Commons.Models.Reconciliation;

public class ReconciliationOptions
{
    public const string SectionName = "Reconciliation";

    public ConsumerOptions Consumer { get; set; } = new();

    public AlertOptions Alert { get; set; } = new();
    
    public EvaluateOptions Evaluate { get; set; } = new();

    public ExecuteOptions Execute { get; set; } = new();
}

public class ConsumerOptions
{
    public bool RespondToContext { get; set; }
}
