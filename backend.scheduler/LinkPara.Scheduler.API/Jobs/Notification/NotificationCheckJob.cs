using LinkPara.Scheduler.API.Commons.Entities;
using LinkPara.Scheduler.API.Commons.Interfaces;
using LinkPara.SharedModels.BusModels.Commands.Notification;
using MassTransit;

namespace LinkPara.Scheduler.API.Jobs.Notification;

public class NotificationCheckJob : IJobTrigger
{
    private readonly IBus _bus;

    public NotificationCheckJob(IBus bus)
    {
        _bus = bus;
    }
    public async Task TriggerAsync(CronJob job)
    {
        var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        var endpoint = await _bus.GetSendEndpoint(new Uri("exchange:Notification.NotificationCheck"));
        await endpoint.Send(new NotificationCheck(), tokenSource.Token);
    }
}