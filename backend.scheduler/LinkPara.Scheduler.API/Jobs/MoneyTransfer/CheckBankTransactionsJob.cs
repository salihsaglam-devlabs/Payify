using LinkPara.Scheduler.API.Commons.Entities;
using LinkPara.Scheduler.API.Commons.Interfaces;
using LinkPara.SharedModels.Banking.Enums;
using LinkPara.SharedModels.BusModels.Commands.Scheduler;
using MassTransit;

namespace LinkPara.Scheduler.API.Jobs.MoneyTransfer;

public class CheckBankTransactionsJob : IJobTrigger
{
    private readonly IBus _bus;
    private readonly ILogger<CheckBankTransactionsJob> _logger;

    public CheckBankTransactionsJob(
        IBus bus,
        ILoggerFactory logger)
    {
        _bus = bus;
        _logger = logger.CreateLogger<CheckBankTransactionsJob>();
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
            BankCode.Ziraat,
            BankCode.Albaraka,
            BankCode.TurkiyeFinansKatilim
        };

        foreach (var item in banks)
        {
            try
            {
                var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                var endpoint = await _bus.GetSendEndpoint(new Uri("exchange:MoneyTransfer.CheckBankTransactions"));
                await endpoint.Send(new CheckBankTransactions
                {
                    BankCode = Convert.ToInt32(item),
                    QueryDate = DateTime.Now.AddDays(-1)
                }, tokenSource.Token);
            }
            catch (Exception exception)
            {
                _logger.LogError("CheckBankTransactionsJob Trigger Error : BankCode( {Item} ) - {Exception}", Convert.ToInt32(item), exception);
            }
        }
    }
}
