using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.BusModels.Commands.Scheduler;
using LinkPara.SharedModels.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LinkPara.PF.Infrastructure.Consumers.CronJobs;

public class ProcessTimeoutConsumer : IConsumer<ProcessTimeout>
{
    private readonly ILogger<ProcessTimeoutConsumer> _logger;
    private readonly IGenericRepository<TimeoutTransaction> _repository;
    private readonly IBus _bus;
    public const int MaxRetryCount = 5; 

    public ProcessTimeoutConsumer(ILogger<ProcessTimeoutConsumer> logger,
        IGenericRepository<TimeoutTransaction> repository,
        IBus bus)
    {
        _logger = logger;
        _repository = repository;
        _bus = bus;
    }
    public async Task Consume(ConsumeContext<ProcessTimeout> context)
    {
        await ProcessTimeoutTransaction();
    }

    private async Task ProcessTimeoutTransaction()
    {
        try
        {
            var timeoutTransactions = await _repository.GetAll()
               .Where(b => (b.TimeoutTransactionStatus == TimeoutTransactionStatus.Pending
                    || b.TimeoutTransactionStatus == TimeoutTransactionStatus.Fail
                    || b.TimeoutTransactionStatus == TimeoutTransactionStatus.Queued)
                    && b.RetryCount <= MaxRetryCount
                    && (b.NextTryTime < DateTime.Now || b.NextTryTime == null))
                .ToListAsync();

            if (timeoutTransactions.Any())
            {
                foreach (var item in timeoutTransactions)
                {
                    try
                    {
                        item.TimeoutTransactionStatus = TimeoutTransactionStatus.Queued;

                        await _repository.UpdateAsync(item);

                        using var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                        var busEndpoint = await _bus.GetSendEndpoint(new Uri("exchange:PF.ProcessTimeoutItems"));
                        await busEndpoint.Send(item, cancellationToken.Token);
                    }
                    catch (Exception exception)
                    {
                        _logger.LogError($"ProcessTimeoutTransactionItem Consumer Error {exception}");
                    }
                }
            }
        }
        catch (Exception exception)
        {
            _logger.LogError($"ProcessTimeoutTransaction Consumer Error {exception}");
        }
    }
}
