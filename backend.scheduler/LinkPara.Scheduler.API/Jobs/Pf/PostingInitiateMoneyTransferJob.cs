using LinkPara.Scheduler.API.Commons.Entities;
using LinkPara.Scheduler.API.Commons.Interfaces;
using LinkPara.SharedModels.BusModels.Commands.Scheduler;
using MassTransit;

namespace LinkPara.Scheduler.API.Jobs.Pf;

public class PostingInitiateMoneyTransferJob : IJobTrigger
{
    private readonly IBus _bus;

    public PostingInitiateMoneyTransferJob(IBus bus)
    {
        _bus = bus;
    }

    public async Task TriggerAsync(CronJob job)
    {
        var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        var endpoint = await _bus.GetSendEndpoint(new Uri("exchange:PF.PostingInitiateMoneyTransfer"));
        await endpoint.Send(new PostingInitiateMoneyTransfer(), tokenSource.Token);
    }
}