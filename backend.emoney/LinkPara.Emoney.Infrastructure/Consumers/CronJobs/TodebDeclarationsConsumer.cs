using LinkPara.Emoney.Application.Commons.Interfaces;
using LinkPara.SharedModels.BusModels.Commands.Scheduler;
using MassTransit;

namespace LinkPara.Emoney.Infrastructure.Consumers.CronJobs;

public class TodebDeclarationsConsumer : IConsumer<TodebDeclarations>
{
    private readonly IAccountService _accountService;

    public TodebDeclarationsConsumer(IAccountService accountService)
    {
        _accountService = accountService;
    }

    public async Task Consume(ConsumeContext<TodebDeclarations> context)
    {
        await _accountService.DeclareAccountsAsync();
    }
}

