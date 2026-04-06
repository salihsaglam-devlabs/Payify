namespace LinkPara.AlertingSystem.Commons.ErrorQueueMonitor;

public class QueueInformation
{
    public string name { get; set; }
    public long messages { get; set; }
    public MessageDetails messages_details { get; set; }
    public long messages_ready { get; set; }
    public MessageDetails messages_ready_details { get; set; }
}