using LinkPara.PF.Application.Commons.Models.Posting;
using LinkPara.PF.Domain.Enums;
using LinkPara.PF.Infrastructure.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LinkPara.PF.Infrastructure.Consumers;

public class PostingPublishTransferItemsConsumer : IConsumer<PublishPostingItem>
{
    private readonly ILogger<PostingPublishTransferItemsConsumer> _logger;
    private readonly PfDbContext _dbContext;
    private readonly IBus _bus;
    
    public PostingPublishTransferItemsConsumer(
        ILogger<PostingPublishTransferItemsConsumer> logger,
        PfDbContext dbContext,
        IBus bus)
    {
        _logger = logger;
        _dbContext = dbContext;
        _bus = bus;
    }
    
    public async Task Consume(ConsumeContext<PublishPostingItem> publishPostingItem)
    {
        try
        {
            var postingItem = publishPostingItem.Message.PostingItem;
            var chunks = ChunkBy(publishPostingItem.Message.MerchantTransactionIds, 100);

            foreach (var chunk in chunks)
            {
                await _dbContext
                    .MerchantTransaction
                    .Where(a => chunk.Contains(a.Id))
                    .ExecuteUpdateAsync(u => u.SetProperty(p => p.PostingItemId, postingItem.Id));
            }

            try
            {
                using var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                var busEndpoint = await _bus.GetSendEndpoint(new Uri("exchange:PF.PostingTransferItems"));
                await busEndpoint.Send(postingItem, cancellationToken.Token);

                postingItem.BatchStatus = BatchStatus.Queued;
            }
            catch (Exception exception)
            {
                _logger.LogCritical($"PostingTransferErrorPublishing. MerchantId:{postingItem.MerchantId} PostingItemId:{postingItem.Id}, Exception: {exception}");

                postingItem.ErrorCount = postingItem.TotalCount;
                postingItem.BatchStatus = BatchStatus.Error;
            }

            _dbContext.PostingItem.Update(postingItem);
            await _dbContext.SaveChangesAsync();
        }
        catch (Exception exception)
        {
            _logger.LogCritical($"PostingPublishTransferItemsError:{exception}");
        }
    }

    private static IEnumerable<List<T>> ChunkBy<T>(List<T> source, int chunkSize)
    {
        for (var i = 0; i < source.Count; i += chunkSize)
        {
            yield return source.GetRange(i, Math.Min(chunkSize, source.Count - i));
        }
    }
}