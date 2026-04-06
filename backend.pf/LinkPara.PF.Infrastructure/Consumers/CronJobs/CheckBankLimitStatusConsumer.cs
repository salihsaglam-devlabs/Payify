using LinkPara.PF.Domain.Entities;
using LinkPara.SharedModels.BusModels.Commands.Scheduler;
using LinkPara.SharedModels.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LinkPara.PF.Infrastructure.Consumers.CronJobs;

public class CheckBankLimitStatusConsumer : IConsumer<BankLimitStatus>
{
    private readonly ILogger<CheckBankLimitStatusConsumer> _logger;
    private readonly IGenericRepository<BankLimit> _repository;
    public CheckBankLimitStatusConsumer(ILogger<CheckBankLimitStatusConsumer> logger, IGenericRepository<BankLimit> repository)
    {
        _logger = logger;
        _repository = repository;
    }
    public async Task Consume(ConsumeContext<BankLimitStatus> context)
    {
        await CheckBankLimitStatusAsync();
    }

    private async Task CheckBankLimitStatusAsync()
    {
        try
        {
            var bankLimits = await _repository.GetAll().Where(b => b.RecordStatus == RecordStatus.Active && !b.IsExpired).ToListAsync();

            if (bankLimits.Any())
            {
                foreach (var item in bankLimits)
                {
                    if (item.LastValidDate < DateTime.Now)
                    {
                        item.IsExpired = true;
                    }
                }

                await _repository.UpdateRangeAsync(bankLimits);
            }
        }
        catch (Exception exception)
        {
            _logger.LogError($"BankLimitStatus Consumer Error {exception}");
        }
    }
}
