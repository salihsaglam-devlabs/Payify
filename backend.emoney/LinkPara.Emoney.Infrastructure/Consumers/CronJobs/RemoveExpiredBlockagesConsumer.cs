using LinkPara.Emoney.Application.Commons.Interfaces;
using LinkPara.SharedModels.BusModels.Commands.Scheduler;
using MassTransit;

namespace LinkPara.Emoney.Infrastructure.Consumers.CronJobs;

public class RemoveExpiredBlockagesConsumer : IConsumer<RemoveExpiredBlockages>
{
    private readonly IWalletBlockageService _walletBlockageService;

    public RemoveExpiredBlockagesConsumer(IWalletBlockageService walletBlockageService)
    {
        _walletBlockageService = walletBlockageService;
    }

    public async Task Consume(ConsumeContext<RemoveExpiredBlockages> context)
    {
        await _walletBlockageService.RemoveExpiredBlockagesAsync();
    }
}

