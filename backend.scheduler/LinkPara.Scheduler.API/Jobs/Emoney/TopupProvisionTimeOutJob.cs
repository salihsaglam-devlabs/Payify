using LinkPara.Scheduler.API.Commons.Entities;
using MassTransit;
using LinkPara.SharedModels.BusModels.Commands.Scheduler;
using LinkPara.Scheduler.API.Commons.Interfaces;

namespace LinkPara.Scheduler.API.Jobs.Emoney;

public class TopupProvisionTimeoutJob : IJobTrigger
{
    private readonly IBus _bus;

    public TopupProvisionTimeoutJob(IBus bus)
    {
        _bus = bus;
    }

    public async Task TriggerAsync(CronJob job)
    {
        var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        var endpoint = await _bus.GetSendEndpoint(new Uri("exchange:Emoney.TopupProvisionTimeout"));
        await endpoint.Send(new TopupProvisionTimeout(), tokenSource.Token);
    }
}
