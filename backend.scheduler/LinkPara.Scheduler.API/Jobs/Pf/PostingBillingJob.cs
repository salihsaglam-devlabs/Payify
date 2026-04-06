using LinkPara.Scheduler.API.Commons.Entities;
using LinkPara.Scheduler.API.Commons.Interfaces;
using MassTransit;
using Models = LinkPara.SharedModels.BusModels.Commands.Scheduler;

namespace LinkPara.Scheduler.API.Jobs.Pf;

public class PostingBillingJob : IJobTrigger
{
    private readonly IBus _bus;

    public PostingBillingJob(IBus bus)
    {
        _bus = bus;
    }

    public async Task TriggerAsync(CronJob job)
    {
        var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        var endpoint = await _bus.GetSendEndpoint(new Uri("exchange:PF.PostingBilling"));
        await endpoint.Send(new Models.PostingBillingJob(), tokenSource.Token);
    }
}
