namespace LinkPara.Card.Application.Commons.Models.Scheduler;

public class FileIngestionAndReconciliationJob
{
    public int ProcessType { get; set; }
    public int? Take { get; set; }
    public string TriggerSource { get; set; } = string.Empty;
}
