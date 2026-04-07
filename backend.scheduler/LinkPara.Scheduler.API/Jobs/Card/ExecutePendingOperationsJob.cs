using LinkPara.Scheduler.API.Commons.Entities;
using LinkPara.Scheduler.API.Commons.Interfaces;
using MassTransit;

namespace LinkPara.Scheduler.API.Jobs.Card;

public class ExecutePendingOperationsJob : IJobTrigger
{
    private readonly IBus _bus;

    public ExecutePendingOperationsJob(IBus bus)
    {
        _bus = bus;
    }

    public async Task TriggerAsync(CronJob job)
    {
        var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        var endpoint = await _bus.GetSendEndpoint(new Uri($"exchange:{CronJobEndpointNames.SchedulerSerialCardJobs}"));
        await endpoint.Send(new
        {
            TriggerSource = "Scheduler",
            ProcessType = (int)FileIngestionAndReconciliationJobType.ExecutePendingOperations,
            Take = 100000
        }, tokenSource.Token);
    }
}
