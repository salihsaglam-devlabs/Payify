using LinkPara.HttpProviders.Vault;
using LinkPara.PF.Application.Commons.Models.Posting;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;
using LinkPara.PF.Infrastructure.Persistence;
using LinkPara.SharedModels.BusModels.IntegrationEvents.Accounting;
using LinkPara.SharedModels.BusModels.IntegrationEvents.Accounting.Enums;
using LinkPara.SharedModels.Exceptions;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LinkPara.PF.Infrastructure.Consumers;

public class PostingUpdateBalanceItemsConsumer : IConsumer<PostingUpdateBalanceItems>
{
    private readonly ILogger<PostingUpdateBalanceItemsConsumer> _logger;
    private readonly PfDbContext _dbContext;
    
    public PostingUpdateBalanceItemsConsumer(
        ILogger<PostingUpdateBalanceItemsConsumer> logger,
        PfDbContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }
    
    public async Task Consume(ConsumeContext<PostingUpdateBalanceItems> updateBalanceItems)
    {
        try
        {
            var chunks = ChunkBy(updateBalanceItems.Message.PostingTransactionIds, 100);

            foreach (var chunk in chunks)
            {
                await _dbContext
                    .PostingTransaction
                    .Where(a => chunk.Contains(a.Id))
                    .ExecuteUpdateAsync(u => u
                            .SetProperty(p => p.BatchStatus, BatchStatus.Completed)
                            .SetProperty(p => p.PostingBankBalanceId, updateBalanceItems.Message.PostingBankBalanceId)
                            .SetProperty(p => p.PostingBalanceId, updateBalanceItems.Message.PostingBalanceId)
                        );
            }
            
        }
        catch (Exception exception)
        {
            _logger.LogCritical($"PostingUpdateBalanceItemsError:{exception}");
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