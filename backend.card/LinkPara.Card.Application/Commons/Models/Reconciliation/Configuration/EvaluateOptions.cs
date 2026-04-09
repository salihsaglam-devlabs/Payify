namespace LinkPara.Card.Application.Commons.Models.Reconciliation;

public class EvaluateOptions
{
    public int? ChunkSize { get; set; }

    public int? ClaimTimeoutSeconds { get; set; }

    public int? ClaimRetryCount { get; set; }

    public void Validate()
    {
        if (ChunkSize is null)
            throw new InvalidOperationException("Vault configuration missing: Reconciliation.Evaluate.ChunkSize");
        if (ClaimTimeoutSeconds is null)
            throw new InvalidOperationException("Vault configuration missing: Reconciliation.Evaluate.ClaimTimeoutSeconds");
        if (ClaimRetryCount is null)
            throw new InvalidOperationException("Vault configuration missing: Reconciliation.Evaluate.ClaimRetryCount");
        if (ChunkSize <= 0)
            throw new InvalidOperationException($"Reconciliation.Evaluate.ChunkSize must be positive. Current: {ChunkSize}");
        if (ClaimTimeoutSeconds <= 0)
            throw new InvalidOperationException($"Reconciliation.Evaluate.ClaimTimeoutSeconds must be positive. Current: {ClaimTimeoutSeconds}");
        if (ClaimRetryCount <= 0)
            throw new InvalidOperationException($"Reconciliation.Evaluate.ClaimRetryCount must be positive. Current: {ClaimRetryCount}");
    }
}
