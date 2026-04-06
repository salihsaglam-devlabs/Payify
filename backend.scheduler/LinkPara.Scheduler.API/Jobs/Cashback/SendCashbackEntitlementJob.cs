using LinkPara.Scheduler.API.Commons.Entities;
using LinkPara.Scheduler.API.Commons.Interfaces;
using LinkPara.SharedModels.BusModels.Commands.Scheduler;
using MassTransit;

namespace LinkPara.Scheduler.API.Jobs.Cashback;

public class SendCashbackEntitlementJob : IJobTrigger
{
    private readonly IBus _bus;

    public SendCashbackEntitlementJob(IBus bus)
    {
        _bus = bus;
    }

    public async Task TriggerAsync(CronJob job)
    {
        var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        var endpoint = await _bus.GetSendEndpoint(new Uri("exchange:Cashback.SendCashbackEntitlementRequest"));
        await endpoint.Send(new SendCashbackEntitlementRequest(), tokenSource.Token);
    }
}
