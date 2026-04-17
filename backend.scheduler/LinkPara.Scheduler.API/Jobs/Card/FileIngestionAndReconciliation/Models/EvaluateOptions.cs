namespace LinkPara.Scheduler.API.Jobs.Card.FileIngestionAndReconciliation.Models;

public class EvaluateOptions
{
    public int? ChunkSize { get; set; }
    public int? ClaimTimeoutSeconds { get; set; }
    public int? ClaimRetryCount { get; set; }
    public int? OperationMaxRetries { get; set; }
}