using LinkPara.Scheduler.API.Commons.Entities;
using LinkPara.Scheduler.API.Commons.Interfaces;
using LinkPara.SharedModels.BusModels.Commands.Scheduler;
using MassTransit;

namespace LinkPara.Scheduler.API.Jobs.BTrans;

public class SendBTransDocumentsJob : IJobTrigger
{
    private readonly IBus _bus;

    public SendBTransDocumentsJob(IBus bus)
    {
        _bus = bus;
    }

    public async Task TriggerAsync(CronJob job)
    {
        var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        var endpoint = await _bus.GetSendEndpoint(new Uri("exchange:BTrans.SendBTransDocuments"));
        await endpoint.Send(new SendBTransDocuments(), tokenSource.Token);
    }
}