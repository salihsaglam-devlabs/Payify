using LinkPara.Scheduler.API.Commons.Entities;
using LinkPara.Scheduler.API.Commons.Interfaces;
using MassTransit;

namespace LinkPara.Scheduler.API.Jobs.Card;

public class ImportCardTransactionsFromFtpJob : IJobTrigger
{
    private readonly IBus _bus;

    public ImportCardTransactionsFromFtpJob(IBus bus)
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
            ProcessType = (int)FileIngestionAndReconciliationJobType.ImportCardTransactionsFromFtp
        }, tokenSource.Token);
    }
}
