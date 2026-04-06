using System.ComponentModel.DataAnnotations;

namespace LinkPara.Card.Application.Commons.Models.Reconciliation;

public class EvaluateOptions
{
    [Range(1, 10000)]
    public int ChunkSize { get; set; } = 500;

    [Range(30, 3600)]
    public int ClaimTimeoutSeconds { get; set; } = 300;

    [Range(1, 10)]
    public int ClaimRetryCount { get; set; } = 3;
}
