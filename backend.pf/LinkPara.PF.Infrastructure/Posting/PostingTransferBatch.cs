using LinkPara.HttpProviders.Vault;
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

namespace LinkPara.PF.Infrastructure.Posting
{
    public class PostingTransferBatch : IPostingBatch<PostingTransfer>
    {
        private readonly ILogger<PostingTransferBatch> _logger;
        private readonly IGenericRepository<MerchantTransaction> _merchantTransactionRepository;
        private readonly IGenericRepository<PostingItem> _postingItemRepository;
        private readonly IBus _bus;
        private readonly IApplicationUserService _applicationUserService;
        private readonly PfDbContext _dbContext;
        private readonly IGenericRepository<PostingBatchStatus> _postingBatchStatusRepository;
        private readonly IVaultClient _vaultClient;
        private const int DefaultTransactionPerBatch = 1000;

        public PostingTransferBatch(
            IGenericRepository<PostingBatchStatus> postingBatchStatusRepository,
            ILogger<PostingTransferBatch> logger,
            IBus bus,
            IApplicationUserService applicationUserService,
            IGenericRepository<MerchantTransaction> merchantTransactionRepository,
            IGenericRepository<PostingItem> postingItemRepository,
            PfDbContext dbContext,
            IVaultClient vaultClient)
        {
            _logger = logger;
            _merchantTransactionRepository = merchantTransactionRepository;
            _postingItemRepository = postingItemRepository;
            _bus = bus;
            _applicationUserService = applicationUserService;
            _postingBatchStatusRepository = postingBatchStatusRepository;
            _dbContext = dbContext;
            _vaultClient = vaultClient;
        }

        public async Task ExecuteBatchAsync(PostingBatchStatus batchStatus)
        {
            try
            {
                var transactionPerBatch = DefaultTransactionPerBatch;
                try
                {
                    transactionPerBatch = await _vaultClient.GetSecretValueAsync<int>("PFSecrets", "PostingSettings", "TransactionPerBatch");
                }
                catch (Exception exception)
                {
                    _logger.LogError($"ErrorOnGetSecretValue Error:{exception}");
                }
                
                var transactionQuery = _merchantTransactionRepository
                    .GetAll()
                    .Include(m => m.Merchant)
                    .Where(t =>
                        (((t.BatchStatus == BatchStatus.Pending)
                          &&
                          (
                              (t.TransactionType == TransactionType.Return &&
                               t.TransactionStatus == TransactionStatus.Success)
                              ||
                              (
                                  (t.TransactionType == TransactionType.Auth ||
                                   t.TransactionType == TransactionType.PostAuth)
                                  &&
                                  (
                                      t.TransactionStatus == TransactionStatus.Success
                                      || t.TransactionStatus == TransactionStatus.PartiallyReturned
                                      || t.TransactionStatus == TransactionStatus.Returned
                                  )
                              )
                          )
                         && t.TransactionDate < DateTime.Now.Date
                         )
                         ||
                         t.BatchStatus == BatchStatus.Error)
                        && !t.IsChargeback
                        && !t.IsSuspecious
                        && t.RecordStatus == RecordStatus.Active);

                var merchantIds = await transactionQuery
                    .Select(s => s.MerchantId)
                    .Distinct()
                    .ToListAsync();

                foreach (var merchantId in merchantIds)
                {
                    await _dbContext
                        .PostingItem
                        .Where(i =>
                            i.PostingDate == DateTime.Now.Date
                            && i.MerchantId == merchantId
                            && i.RecordStatus == RecordStatus.Active
                        )
                        .ExecuteUpdateAsync(u => u.SetProperty(p => p.RecordStatus, RecordStatus.Passive));
                    
                    var currentPage = 0;
                    
                    var transactions = await transactionQuery
                        .Where(s => s.MerchantId == merchantId)
                        .OrderBy(x => x.Id)
                        .Select(s => s.Id)
                        .ToListAsync();
                    
                    while (true)
                    {
                        var pageResults = transactions
                            .Skip(currentPage * transactionPerBatch)
                            .Take(transactionPerBatch)
                            .ToList();

                        if (pageResults.Count == 0)
                        {
                            break;
                        }

                        var postingItem = new PostingItem
                        {
                            MerchantId = merchantId,
                            ErrorCount = 0,
                            TotalCount = pageResults.Count,
                            PostingDate = DateTime.Now.Date,
                            RecordStatus = RecordStatus.Active,
                            BatchStatus = BatchStatus.Pending,
                            CreateDate = DateTime.Now,
                            CreatedBy = _applicationUserService.ApplicationUserId.ToString()
                        };

                        await _postingItemRepository.AddAsync(postingItem);
                        
                        try
                        {
                            using var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                            var busEndpoint = await _bus.GetSendEndpoint(new Uri("exchange:PF.PublishTransferItems"));
                            await busEndpoint.Send(new PublishPostingItem
                            {
                                PostingItem = postingItem,
                                MerchantTransactionIds = pageResults
                            }, cancellationToken.Token);
                        }
                        catch (Exception exception)
                        {
                            _logger.LogCritical($"PostingTransferErrorPublishing. Exception: {exception}");
                            postingItem.BatchStatus = BatchStatus.Error;
                            postingItem.ErrorCount = postingItem.TotalCount;
                            await _postingItemRepository.UpdateAsync(postingItem);
                        }

                        currentPage++;
                    }
                }

                batchStatus.BatchSummary = "PostingTransferBatchFinished";
                batchStatus.BatchStatus = BatchStatus.Queued;
                batchStatus.UpdateDate = DateTime.Now;
                batchStatus.IsCriticalError = false;

                await _postingBatchStatusRepository.UpdateAsync(batchStatus);
            }
            catch (Exception exception)
            {
                _logger.LogError($"PostingTransferBatchError {exception}");

                batchStatus.BatchSummary = "PostingTransferBatchError";
                batchStatus.BatchStatus = BatchStatus.Error;
                batchStatus.UpdateDate = DateTime.Now;
                batchStatus.IsCriticalError = true;

                await _postingBatchStatusRepository.UpdateAsync(batchStatus);
            }
        }
    }
}