namespace LinkPara.Card.Application.Commons.Models.Reconciliation;

public class EvaluateOptions
{
    public const int DefaultChunkSize = 5_000;
    public const int DefaultClaimTimeoutSeconds = 600;
    public const int DefaultClaimRetryCount = 5;

    public int? ChunkSize { get; set; }

    public int? ClaimTimeoutSeconds { get; set; }

    public int? ClaimRetryCount { get; set; }

    public void ValidateAndApplyDefaults()
    {
        ChunkSize ??= DefaultChunkSize;
        ClaimTimeoutSeconds ??= DefaultClaimTimeoutSeconds;
        ClaimRetryCount ??= DefaultClaimRetryCount;

        if (ChunkSize <= 0)
            throw new InvalidOperationException($"Reconciliation.Evaluate.ChunkSize must be positive. Current: {ChunkSize}");
        if (ClaimTimeoutSeconds <= 0)
            throw new InvalidOperationException($"Reconciliation.Evaluate.ClaimTimeoutSeconds must be positive. Current: {ClaimTimeoutSeconds}");
        if (ClaimRetryCount <= 0)
            throw new InvalidOperationException($"Reconciliation.Evaluate.ClaimRetryCount must be positive. Current: {ClaimRetryCount}");
    }

    [Obsolete("Use ValidateAndApplyDefaults() instead")]
    public void Validate() => ValidateAndApplyDefaults();
}
