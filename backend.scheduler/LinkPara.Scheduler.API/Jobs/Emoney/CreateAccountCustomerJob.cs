using LinkPara.Scheduler.API.Commons.Entities;
using LinkPara.Scheduler.API.Commons.Interfaces;
using MassTransit;
using LinkPara.SharedModels.BusModels.Commands.Scheduler;

namespace LinkPara.Scheduler.API.Jobs.Emoney;

public class CreateAccountCustomerJob : IJobTrigger
{
    private readonly IBus _bus;

    public CreateAccountCustomerJob(IBus bus)
    {
        _bus = bus;
    }

    public async Task TriggerAsync(CronJob job)
    {
        var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        var endpoint = await _bus.GetSendEndpoint(new Uri("exchange:Emoney.CreateAccountCustomer"));
        await endpoint.Send(new CreateAccountCustomer(), tokenSource.Token);
    }
}