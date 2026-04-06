using LinkPara.Scheduler.API.Commons.Entities;

namespace LinkPara.Scheduler.API.Commons.Interfaces;

public interface IJobHttpInvoker
{
    Task InvokeAsync(CronJob cronJob);
}