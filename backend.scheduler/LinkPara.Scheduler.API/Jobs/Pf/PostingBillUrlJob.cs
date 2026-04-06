using LinkPara.Scheduler.API.Commons.Entities;
using LinkPara.Scheduler.API.Commons.Interfaces;
using MassTransit;
using Models = LinkPara.SharedModels.BusModels.Commands.Scheduler;

namespace LinkPara.Scheduler.API.Jobs.Pf;

public class PostingBillUrlJob : IJobTrigger
{
    private readonly IBus _bus;

    public PostingBillUrlJob(IBus bus)
    {
        _bus = bus;
    }

    public async Task TriggerAsync(CronJob job)
    {
        var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        var endpoint = await _bus.GetSendEndpoint(new Uri("exchange:PF.PostingBillUrl"));
        await endpoint.Send(new Models.PostingBillUrlJob(), tokenSource.Token);
    }
}