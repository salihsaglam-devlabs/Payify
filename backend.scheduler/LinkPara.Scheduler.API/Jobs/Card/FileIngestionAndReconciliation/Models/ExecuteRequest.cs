namespace LinkPara.Scheduler.API.Jobs.Card.FileIngestionAndReconciliation.Models;

public class ExecuteRequest
{
    public Guid[] GroupIds { get; set; } = Array.Empty<Guid>();
    public Guid[] EvaluationIds { get; set; } = Array.Empty<Guid>();
    public Guid[] OperationIds { get; set; } = Array.Empty<Guid>();
    public ExecuteOptions Options { get; set; }
}