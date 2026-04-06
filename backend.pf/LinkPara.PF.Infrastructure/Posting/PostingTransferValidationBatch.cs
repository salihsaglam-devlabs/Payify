using System.Transactions;
using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Commons.Models.Posting;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;
using LinkPara.PF.Infrastructure.Persistence;
using LinkPara.SharedModels.Persistence;
using LinkPara.SystemUser;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LinkPara.PF.Infrastructure.Posting;

public class PostingTransferValidationBatch : IPostingBatch<PostingTransferValidation>
{
    private readonly ILogger<PostingTransferValidationBatch> _logger;
    private readonly IBus _bus;
    private readonly PfDbContext _dbContext;
    private readonly IGenericRepository<PostingBatchStatus> _postingBatchStatusRepository;
    private readonly IGenericRepository<PostingItem> _postingItemRepository;
    private readonly IApplicationUserService _applicationUserService;

    public PostingTransferValidationBatch(
        ILogger<PostingTransferValidationBatch> logger,
        IBus bus,
        PfDbContext dbContext,
        IGenericRepository<PostingBatchStatus> postingBatchStatusRepository,
        IGenericRepository<PostingItem> postingItemRepository,
        IApplicationUserService applicationUserService
        )
    {
        _logger = logger;
        _bus = bus;
        _applicationUserService = applicationUserService;
        _postingBatchStatusRepository = postingBatchStatusRepository;
        _postingItemRepository = postingItemRepository;
        _dbContext = dbContext;
    }

    public async Task ExecuteBatchAsync(PostingBatchStatus batchStatus)
    {
        var newPostingItemList = new List<PostingItem>();
        
        var failedPostingItems = await _dbContext.PostingItem
            .Where(s => 
                s.PostingDate == DateTime.Now.Date && 
                (s.ErrorCount > 0 || s.BatchStatus == BatchStatus.Error))
            .ToListAsync();

        foreach (var failedPostingItem in failedPostingItems)
        {
            var completedCount = await _dbContext.MerchantTransaction
                .Where(s =>
                    s.PostingItemId == failedPostingItem.Id && 
                    s.BatchStatus == BatchStatus.Completed)
                .CountAsync();
            
            var failedTransactions = await _dbContext.MerchantTransaction
                .Where(s =>
                    s.PostingItemId == failedPostingItem.Id &&
                    s.BatchStatus != BatchStatus.Completed)
                .ToListAsync();
            
            if (completedCount == failedPostingItem.TotalCount || !failedTransactions.Any())
            {
                failedPostingItem.BatchStatus = BatchStatus.Completed;
                failedPostingItem.ErrorCount = 0;
                _dbContext.Update(failedPostingItem);
                await _dbContext.SaveChangesAsync();
                continue;
            }
            
            var strategy = _dbContext.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

                failedPostingItem.TotalCount = completedCount;
                failedPostingItem.BatchStatus = BatchStatus.Completed;
                failedPostingItem.ErrorCount = 0;
                _dbContext.Update(failedPostingItem);
                
                var newPostingItem = new PostingItem
                {
                    MerchantId = failedPostingItem.MerchantId,
                    ErrorCount = 0,
                    TotalCount = failedTransactions.Count,
                    PostingDate = DateTime.Now.Date,
                    RecordStatus = RecordStatus.Active,
                    BatchStatus = BatchStatus.Pending,
                    CreateDate = DateTime.Now,
                    CreatedBy = _applicationUserService.ApplicationUserId.ToString()
                };

                await _dbContext.PostingItem.AddAsync(newPostingItem);
                
                failedTransactions.ForEach(t => t.PostingItemId = newPostingItem.Id);
                _dbContext.MerchantTransaction.UpdateRange(failedTransactions);
                
                newPostingItemList.Add(newPostingItem);
                
                await _dbContext.SaveChangesAsync();
                scope.Complete();
            });
        }

        foreach (var postingItem in newPostingItemList)
        {
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

            await _postingItemRepository.UpdateAsync(postingItem);
        }
        
        batchStatus.BatchSummary = "PostingTransferValidationBatchFinished";
        batchStatus.BatchStatus = BatchStatus.Queued;
        batchStatus.UpdateDate = DateTime.Now;
        batchStatus.IsCriticalError = false;

        await _postingBatchStatusRepository.UpdateAsync(batchStatus);
    }
}