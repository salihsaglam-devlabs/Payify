using LinkPara.Emoney.Application.Commons.Interfaces;
using LinkPara.SharedModels.BusModels.Commands.Scheduler;
using MassTransit;

namespace LinkPara.Emoney.Infrastructure.Consumers.CronJobs;

public class AssignVirtualIbanConsumer : IConsumer<AssignVirtualIban>
{
    private readonly IVirtualIbanService _virtualIbanService;

    public AssignVirtualIbanConsumer(IVirtualIbanService virtualIbanService)
    {
        _virtualIbanService = virtualIbanService;
    }

    public async Task Consume(ConsumeContext<AssignVirtualIban> context)
    {
        await _virtualIbanService.AssignToAccountsAsync();
    }
}
