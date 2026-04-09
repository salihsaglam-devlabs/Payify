namespace LinkPara.Card.Application.Commons.Models.Reconciliation;

public class ExecuteOptions
{
    public int? MaxEvaluations { get; set; }

    public int? LeaseSeconds { get; set; }

    public void Validate()
    {
        if (MaxEvaluations is null)
            throw new InvalidOperationException("Vault configuration missing: Reconciliation.Execute.MaxEvaluations");
        if (LeaseSeconds is null)
            throw new InvalidOperationException("Vault configuration missing: Reconciliation.Execute.LeaseSeconds");
        if (MaxEvaluations <= 0)
            throw new InvalidOperationException($"Reconciliation.Execute.MaxEvaluations must be positive. Current: {MaxEvaluations}");
        if (LeaseSeconds <= 0)
            throw new InvalidOperationException($"Reconciliation.Execute.LeaseSeconds must be positive. Current: {LeaseSeconds}");
    }
}
