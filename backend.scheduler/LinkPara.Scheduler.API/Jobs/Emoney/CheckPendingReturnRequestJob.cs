using LinkPara.Scheduler.API.Commons.Entities;
using LinkPara.Scheduler.API.Commons.Interfaces;
using LinkPara.SharedModels.BusModels.Commands.Scheduler;
using MassTransit;

namespace LinkPara.Scheduler.API.Jobs.Emoney;

public class CheckPendingReturnRequestJob : IJobTrigger
{
    private readonly IBus _bus;

    public CheckPendingReturnRequestJob(IBus bus)
    {
        _bus = bus;
    }

    public async Task TriggerAsync(CronJob job)
    {
        var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        var endpoint = await _bus.GetSendEndpoint(new Uri("exchange:Emoney.CheckPendingReturnRequest"));
        await endpoint.Send(new CheckPendingReturnRequest(), tokenSource.Token);
    }
}
