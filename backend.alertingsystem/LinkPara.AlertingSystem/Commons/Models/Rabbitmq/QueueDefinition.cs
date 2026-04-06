namespace LinkPara.AlertingSystem.Commons.ErrorQueueMonitor;

public class QueueDefinition
{
    public string name { get; set; }
    public string vhost { get; set; }
    public bool durable { get; set; }
}