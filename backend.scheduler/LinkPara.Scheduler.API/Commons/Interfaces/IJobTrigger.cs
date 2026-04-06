using LinkPara.Scheduler.API.Commons.Entities;

namespace LinkPara.Scheduler.API.Commons.Interfaces;

public interface IJobTrigger
{
    Task TriggerAsync(CronJob job);
}