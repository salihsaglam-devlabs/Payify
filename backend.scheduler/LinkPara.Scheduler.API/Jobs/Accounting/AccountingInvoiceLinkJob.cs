using LinkPara.Scheduler.API.Commons.Entities;
using LinkPara.Scheduler.API.Commons.Interfaces;
using LinkPara.SharedModels.BusModels.Commands.Scheduler;
using MassTransit;

namespace LinkPara.Scheduler.API.Jobs.Accounting;

public class AccountingInvoiceLinkJob : IJobTrigger
{
    private readonly IBus _bus;

    public AccountingInvoiceLinkJob(IBus bus)
    {
        _bus = bus;
    }

    public async Task TriggerAsync(CronJob job)
    {
        var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        var endpoint = await _bus.GetSendEndpoint(new Uri("exchange:Accounting.InvoiceLinkJob"));
        await endpoint.Send(new AccountingInvoiceLink(), tokenSource.Token);
    }
}
