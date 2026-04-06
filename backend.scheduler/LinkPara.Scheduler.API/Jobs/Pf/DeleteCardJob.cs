using LinkPara.Scheduler.API.Commons.Entities;
using LinkPara.Scheduler.API.Commons.Interfaces;
using LinkPara.SharedModels.BusModels.Commands.Scheduler;
using LinkPara.SharedModels.Notification.NotificationModels.PF;
using MassTransit;

namespace LinkPara.Scheduler.API.Jobs.Pf;

public class DeleteCardJob : IJobTrigger
{
    private readonly IBus _bus;

    public DeleteCardJob(IBus bus)
    {
        _bus = bus;
    }
    public async Task TriggerAsync(CronJob job)
    {
        try
        {
            var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            var endpoint = await _bus.GetSendEndpoint(new Uri("exchange:PF.DeleteCard"));
            await endpoint.Send(new DeleteCard(), tokenSource.Token);
        }
        catch (Exception)
        {
            //Send email notification
           await _bus.Publish(new DeleteCardError());
        }
    }
}
