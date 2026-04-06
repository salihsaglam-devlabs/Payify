using LinkPara.Scheduler.API.Commons.Entities;
using LinkPara.Scheduler.API.Commons.Interfaces;

namespace LinkPara.Scheduler.API.Jobs.AlertingSystem;

public class CheckErrorQueuesJob : IJobTrigger
{
    private readonly IJobHttpInvoker _invoker;
    public CheckErrorQueuesJob(IJobHttpInvoker invoker)
    {
        _invoker = invoker;
    }
    public async Task TriggerAsync(CronJob job)
    {
        await _invoker.InvokeAsync(job);
    }
}