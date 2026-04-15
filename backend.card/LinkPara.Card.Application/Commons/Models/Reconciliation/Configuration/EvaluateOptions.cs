using LinkPara.Card.Application.Commons.Exceptions;

namespace LinkPara.Card.Application.Commons.Models.Reconciliation.Configuration;

public class EvaluateOptions
{
    public const int DefaultChunkSize = 50_000;
    public const int DefaultClaimTimeoutSeconds = 1_800;
    public const int DefaultClaimRetryCount = 5;
    public const int DefaultOperationMaxRetries = 5;

    public int? ChunkSize { get; set; }

    public int? ClaimTimeoutSeconds { get; set; }

    public int? ClaimRetryCount { get; set; }

    public int? OperationMaxRetries { get; set; }

    public void ValidateAndApplyDefaults()
    {
        ChunkSize ??= DefaultChunkSize;
        ClaimTimeoutSeconds ??= DefaultClaimTimeoutSeconds;
        ClaimRetryCount ??= DefaultClaimRetryCount;
        OperationMaxRetries ??= DefaultOperationMaxRetries;

        if (ChunkSize <= 0)
            throw new ReconciliationEvaluateChunkSizeInvalidException($"Reconciliation.Evaluate.ChunkSize must be positive. Current: {ChunkSize}");
        if (ClaimTimeoutSeconds <= 0)
            throw new ReconciliationEvaluateClaimTimeoutInvalidException($"Reconciliation.Evaluate.ClaimTimeoutSeconds must be positive. Current: {ClaimTimeoutSeconds}");
        if (ClaimRetryCount <= 0)
            throw new ReconciliationEvaluateClaimRetryInvalidException($"Reconciliation.Evaluate.ClaimRetryCount must be positive. Current: {ClaimRetryCount}");
        if (OperationMaxRetries < 0)
            throw new ReconciliationEvaluateOperationMaxRetriesInvalidException($"Reconciliation.Evaluate.OperationMaxRetries must be non-negative. Current: {OperationMaxRetries}");
    }

    [Obsolete("Use ValidateAndApplyDefaults() instead")]
    public void Validate() => ValidateAndApplyDefaults();
}
