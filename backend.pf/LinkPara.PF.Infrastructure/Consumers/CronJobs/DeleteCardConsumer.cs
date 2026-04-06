using LinkPara.PF.Infrastructure.Persistence;
using LinkPara.SharedModels.BusModels.Commands.Scheduler;
using LinkPara.SharedModels.Notification.NotificationModels.PF;
using LinkPara.SharedModels.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LinkPara.PF.Infrastructure.Consumers.CronJobs;

public class DeleteCardConsumer : IConsumer<DeleteCard>
{
    private readonly ILogger<DeleteCardConsumer> _logger;
    private readonly PfDbContext _dbContext;
    private readonly IBus _bus;

    public DeleteCardConsumer(ILogger<DeleteCardConsumer> logger, 
        PfDbContext dbContext,
        IBus bus)
    {
        _logger = logger;
        _dbContext = dbContext;
        _bus = bus;
    }
    public async Task Consume(ConsumeContext<DeleteCard> context)
    {
        await DeleteCardAsync();
    }

    private async Task DeleteCardAsync()
    {
        var cardTokens = await _dbContext.CardToken
            .Where(b => b.ExpiryDate < DateTime.Now && b.RecordStatus == RecordStatus.Active)
            .ToListAsync();

        if (cardTokens.Any())
        {
            try
            {
                _dbContext.RemoveRange(cardTokens); 

                await _dbContext.SaveChangesAsync();
            }
            catch (Exception exception)
            {
                _logger.LogError($"DeleteCard Consumer Error {exception}");
                
                //Send notification email
                await _bus.Publish(new DeleteCardError());
            }
        }
    }
}
