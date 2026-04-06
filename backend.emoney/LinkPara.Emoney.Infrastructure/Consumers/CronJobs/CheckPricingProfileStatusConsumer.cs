using LinkPara.Emoney.Application.Commons.Interfaces;
using LinkPara.SharedModels.BusModels.Commands.Scheduler;
using MassTransit;

namespace LinkPara.Emoney.Infrastructure.Consumers.CronJobs;

public class CheckPricingProfileStatusConsumer : IConsumer<CheckPricingProfileStatus>
{
    private readonly IPricingProfileService _service;

    public CheckPricingProfileStatusConsumer(IPricingProfileService service)
    {
        _service = service;
    }

    public async Task Consume(ConsumeContext<CheckPricingProfileStatus> context)
    {
        await _service.CheckProfileStatus();
    }
}