namespace LinkPara.Card.Application.Commons.Models.Reconciliation;

public class ReconciliationOptions
{
    public const string SectionName = "Reconciliation";

    public ConsumerOptions Consumer { get; set; }

    public AlertOptions Alert { get; set; }
    
    public EvaluateOptions Evaluate { get; set; }

    public ExecuteOptions Execute { get; set; }

    public void Validate()
    {
        if (Consumer is null)
            throw new InvalidOperationException("Vault configuration missing: Reconciliation.Consumer");
        if (Alert is null)
            throw new InvalidOperationException("Vault configuration missing: Reconciliation.Alert");
        if (Evaluate is null)
            throw new InvalidOperationException("Vault configuration missing: Reconciliation.Evaluate");
        if (Execute is null)
            throw new InvalidOperationException("Vault configuration missing: Reconciliation.Execute");

        Consumer.Validate();
        Alert.Validate();
        Evaluate.Validate();
        Execute.Validate();
    }
}

public class ConsumerOptions
{
    public bool? RespondToContext { get; set; }

    public void Validate()
    {
        if (RespondToContext is null)
            throw new InvalidOperationException("Vault configuration missing: Reconciliation.Consumer.RespondToContext");
    }
}
