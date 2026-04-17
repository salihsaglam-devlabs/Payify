namespace LinkPara.Scheduler.API.Jobs.Card.FileIngestionAndReconciliation.Models;

public class EvaluateRequest
{
    public Guid[] IngestionFileIds { get; set; } = Array.Empty<Guid>();
    public EvaluateOptions Options { get; set; }
}