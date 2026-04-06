using LinkPara.Emoney.Application.Commons.Interfaces;
using LinkPara.SharedModels.BusModels.Commands.Scheduler;
using MassTransit;

namespace LinkPara.Emoney.Infrastructure.Consumers.CronJobs;

public class CheckVirtualIbanCountConsumer : IConsumer<CheckVirtualIbanCount>
{
    private readonly IVirtualIbanService _virtualIbanService;

    public CheckVirtualIbanCountConsumer(IVirtualIbanService virtualIbanService)
    {
        _virtualIbanService = virtualIbanService;
    }

    public async Task Consume(ConsumeContext<CheckVirtualIbanCount> context)
    {
        await _virtualIbanService.CheckAvailableCount();
    }
}
