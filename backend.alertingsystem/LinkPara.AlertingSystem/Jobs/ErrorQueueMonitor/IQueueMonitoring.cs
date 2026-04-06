namespace LinkPara.AlertingSystem.Commons.ErrorQueueMonitor;

public interface IQueueMonitoring
{
    public Task GetErrorQueueInfoAsync();
    public Task GetQueueInfosAsync();
}