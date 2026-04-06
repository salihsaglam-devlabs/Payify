using LinkPara.Emoney.Application.Commons.Interfaces;
using LinkPara.SharedModels.BusModels.Commands.Scheduler;
using MassTransit;

namespace LinkPara.Emoney.Infrastructure.Consumers.CronJobs;

public class CheckChangedBalanceConsumer : IConsumer<CheckChangedBalance>
{
    private readonly IOpenBankingService _openBankingService;

    public CheckChangedBalanceConsumer(IOpenBankingService openBankingService)
    {
        _openBankingService = openBankingService;
    }

    public async Task Consume(ConsumeContext<CheckChangedBalance> context)
    {
        await _openBankingService.CheckChangedBalanceAsync();
    }
}
