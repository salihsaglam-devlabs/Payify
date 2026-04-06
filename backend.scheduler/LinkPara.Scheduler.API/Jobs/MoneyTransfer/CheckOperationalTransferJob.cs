using LinkPara.Scheduler.API.Commons.Entities;
using LinkPara.Scheduler.API.Commons.Interfaces;
using LinkPara.SharedModels.BusModels.Commands.Scheduler;
using MassTransit;

namespace LinkPara.Scheduler.API.Jobs.MoneyTransfer;

public class CheckOperationalTransferJob : IJobTrigger
{
    private readonly IBus _bus;
    private readonly ILogger<CheckOperationalBankAccountBalanceJob> _logger;

    public CheckOperationalTransferJob(IBus bus,
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
            var endpoint = await _bus.GetSendEndpoint(new Uri("exchange:MoneyTransfer.CheckOperationalTransfer"));
            await endpoint.Send(new CheckOperationalTransfer(), tokenSource.Token);
        }
        catch (Exception exception)
        {
            _logger.LogError("CheckOperationalTransferJob Trigger Error : {Exception}", exception);
        }
    }
}

