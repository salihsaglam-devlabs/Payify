namespace LinkPara.Scheduler.API.Jobs.Card.FileIngestionAndReconciliation.Models;

public class ExecuteOptions
{
    public int? MaxEvaluations { get; set; }
    public int? LeaseSeconds { get; set; }
}