using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.BusModels.Commands.PF;
using LinkPara.SharedModels.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LinkPara.PF.Infrastructure.Consumers.CronJobs;

public class CheckBlacklistControlConsumer : IConsumer<MerchantBlacklistControl>
{
    private readonly IGenericRepository<Merchant> _repository;
    private readonly ILogger<CheckBlacklistControlConsumer> _logger;
    private readonly IBus _bus;
    public CheckBlacklistControlConsumer(IGenericRepository<Merchant> repository,
        ILogger<CheckBlacklistControlConsumer> logger,
        IBus bus)
    {
        _repository = repository;
        _logger = logger;
        _bus = bus;
    }

    public async Task Consume(ConsumeContext<MerchantBlacklistControl> context)
    {
        var activeMerchants = await _repository
            .GetAll()
            .Where(b =>
                   b.MerchantStatus == MerchantStatus.Active &&
                   b.RecordStatus == RecordStatus.Active)
            .ToListAsync();

        foreach (var item in activeMerchants)
        {
            try
            {
                using var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                var endpoint = await _bus.GetSendEndpoint(new Uri("exchange:PF.MerchantBlacklistControl"));
                await endpoint.Send(new MerchantBlacklistControl { MerchantId = item.Id }, tokenSource.Token);
            }
            catch (Exception exception)
            {
                _logger.LogError($"CheckBlacklistControlConsumer - MerchantId({item.Id}) Error : {exception}", item.Id);
            }
        }
    }
}
