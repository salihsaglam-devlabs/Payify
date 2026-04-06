using LinkPara.Emoney.Application.Commons.Interfaces;
using LinkPara.SharedModels.BusModels.Commands.Scheduler;
using MassTransit;

namespace LinkPara.Emoney.Infrastructure.Consumers.CronJobs;

public class TransformCustodyAccountsConsumer : IConsumer<TransformCustodyAccounts>
{
    private readonly IAccountService _accountService;

    public TransformCustodyAccountsConsumer(IAccountService accountService)
    {
        _accountService = accountService;
    }

    public async Task Consume(ConsumeContext<TransformCustodyAccounts> context)
    {
        await _accountService.TransformCustodyAccountsAsync();
    }
}

