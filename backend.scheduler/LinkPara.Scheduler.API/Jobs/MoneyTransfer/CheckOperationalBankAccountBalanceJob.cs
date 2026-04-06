using LinkPara.Scheduler.API.Commons.Entities;
using LinkPara.Scheduler.API.Commons.Interfaces;
using LinkPara.SharedModels.BusModels.Commands.Scheduler;
using MassTransit;

namespace LinkPara.Scheduler.API.Jobs.MoneyTransfer;

public class CheckOperationalBankAccountBalanceJob : IJobTrigger
{
    private readonly IBus _bus;
    private readonly ILogger<CheckOperationalBankAccountBalanceJob> _logger;

    public CheckOperationalBankAccountBalanceJob(IBus bus, 
        ILogger<CheckOperationalBankAccountBalanceJob> logger)
    {
        _bus = bus;
        _logger = logger;
    }

    public async Task TriggerAsync(CronJob job)
    {
        try
        {
            var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            var endpoint = await _bus.GetSendEndpoint(new Uri("exchange:MoneyTransfer.CheckOperationalBankAccountBalance"));
            await endpoint.Send(new CheckOperationalBankAccountBalance(), tokenSource.Token);
        }
        catch (Exception exception)
        {
            _logger.LogError("CheckOperationalBankAccountBalanceJob Trigger Error : {Exception}", exception);
        }
    }
}
