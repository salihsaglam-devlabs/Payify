using LinkPara.Scheduler.API.Commons.Entities;
using LinkPara.Scheduler.API.Commons.Interfaces;
using LinkPara.SharedModels.Banking.Enums;
using LinkPara.SharedModels.BusModels.Commands.Scheduler;
using MassTransit;

namespace LinkPara.Scheduler.API.Jobs.MoneyTransfer;

public class CheckDepositJob : IJobTrigger
{
    private readonly IBus _bus;
    private readonly ILogger<CheckDepositJob> _logger;

    public CheckDepositJob(
        IBus bus,
        ILoggerFactory logger)
    {
        _bus = bus;
        _logger = logger.CreateLogger<CheckDepositJob>();
    }

    public async Task TriggerAsync(CronJob job)
    {
        var banks = new List<BankCode>
        {
            BankCode.VakifKatilim,
            BankCode.Denizbank,
            BankCode.SekerBank,
            BankCode.VakifBank,
            BankCode.Garanti,
            BankCode.KuveytTurk,
            BankCode.Akbank,
            BankCode.IsBank,
            BankCode.TestBank,
            BankCode.Ziraat,
            BankCode.Albaraka,
            BankCode.TurkiyeFinansKatilim
        };

        foreach (var item in banks)
        {
            try
            {
                var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                var endpoint = await _bus.GetSendEndpoint(new Uri("exchange:MoneyTransfer.CheckDeposit"));
                await endpoint.Send(new CheckDeposit { BankCode = Convert.ToInt32(item) }, tokenSource.Token);
            }
            catch (Exception exception)
            {
                _logger.LogError("CheckDepositJob Trigger Error : BankCode( {Item} ) - {Exception}", Convert.ToInt32(item), exception);
            }
        }
    }
}
