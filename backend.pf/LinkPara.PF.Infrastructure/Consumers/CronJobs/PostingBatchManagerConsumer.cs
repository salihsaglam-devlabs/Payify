using LinkPara.Cache;
using LinkPara.HttpProviders.Notification;
using LinkPara.HttpProviders.Notification.Models;
using LinkPara.HttpProviders.Vault;
using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Commons.Models.Posting;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.BusModels.Commands.Notification;
using LinkPara.SharedModels.BusModels.Commands.Scheduler;
using LinkPara.SharedModels.Notification.NotificationModels.PF;
using LinkPara.SharedModels.Persistence;
using LinkPara.SystemUser;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LinkPara.PF.Infrastructure.Consumers.CronJobs
{
    public class PostingBatchManagerConsumer : IConsumer<PostingBatchManager>
    {
        private readonly ILogger<PostingBatchManagerConsumer> _logger;
        private readonly IGenericRepository<PostingBatchStatus> _postingBatchStatusRepository;
        private readonly IGenericRepository<PostingItem> _postingItemRepository;
        private readonly IPostingBatchFactory _postingBatchFactory;
        private readonly IApplicationUserService _applicationUserService;
        private readonly IBus _bus;
        private readonly IVaultClient _vaultClient;
        private readonly ICacheService _cacheService;
        private readonly IGenericRepository<Merchant> _merchantRepository;
        private readonly IGenericRepository<MerchantTransaction> _merchantTransactionRepository;
        private readonly INotificationService _notificationService;

        public PostingBatchManagerConsumer(ILogger<PostingBatchManagerConsumer> logger,
            IGenericRepository<PostingBatchStatus> postingBatchStatusRepository,
            IApplicationUserService applicationUserService,
            IPostingBatchFactory postingBatchFactory,
            IGenericRepository<PostingItem> postingItemRepository,
            IBus bus,
            IVaultClient vaultClient,
            ICacheService cacheService,
            IGenericRepository<Merchant> merchantRepository,
            IGenericRepository<MerchantTransaction> merchantTransactionRepository,
            INotificationService notificationService)
        {
            _logger = logger;
            _postingBatchStatusRepository = postingBatchStatusRepository;
            _postingBatchFactory = postingBatchFactory;
            _postingItemRepository = postingItemRepository;
            _applicationUserService = applicationUserService;
            _bus = bus;
            _vaultClient = vaultClient;
            _cacheService = cacheService;
            _merchantRepository = merchantRepository;
            _merchantTransactionRepository = merchantTransactionRepository;
            _notificationService = notificationService;
        }

        public async Task Consume(ConsumeContext<PostingBatchManager> context)
        {
            try
            {
                var currentLevel = PostingBatchLevel.BatchManager;
                var nextLevel = PostingBatchLevel.BatchManager;
                var currentLevelStatus = BatchStatus.Pending;
                var nextLevelStatus = BatchStatus.Pending;

                var batchStatus = await _postingBatchStatusRepository
                    .GetAll()
                    .OrderByDescending(o => o.BatchOrder)
                    .FirstOrDefaultAsync(s => s.PostingDate.Date == DateTime.Now.Date);

                if (batchStatus == null)
                {
                    currentLevelStatus = BatchStatus.Completed;
                    nextLevel = PostingBatchLevel.Transfer;
                    nextLevelStatus = BatchStatus.Pending;
                }
                else if (batchStatus.IsCriticalError && (batchStatus.PostingBatchLevel != PostingBatchLevel.Transfer || batchStatus.PostingBatchLevel != PostingBatchLevel.TransferValidation))
                {
                    await _postingBatchFactory.TriggerBatchAsync(batchStatus);

                    return;
                }
                else
                {
                    currentLevel = batchStatus.PostingBatchLevel;
                    currentLevelStatus = batchStatus.BatchStatus;

                    if (currentLevelStatus == BatchStatus.Queued && currentLevel is PostingBatchLevel.Transfer or PostingBatchLevel.TransferValidation)
                    {
                        var queuedPostingItems = await _postingItemRepository.GetAll()
                            .Where
                            (a =>
                                (a.BatchStatus == BatchStatus.Pending || a.BatchStatus == BatchStatus.Queued) &&
                                a.PostingDate.Date == DateTime.Now.Date &&
                                a.RecordStatus == RecordStatus.Active
                            ).ToListAsync();
                        if (queuedPostingItems.Count > 0)
                        {
                            if (await _merchantTransactionRepository.GetAll()
                                    .AnyAsync(s => 
                                        s.BatchStatus == BatchStatus.Pending && 
                                        queuedPostingItems.Select(a => a.Id).ToList().Contains(s.PostingItemId))
                                )
                            {
                                return;
                            }
                            
                            queuedPostingItems.ForEach(l =>
                            {
                                l.BatchStatus = BatchStatus.Completed;
                                l.UpdateDate = DateTime.Now;
                                l.LastModifiedBy = _applicationUserService.ApplicationUserId.ToString();
                            });
                            await _postingItemRepository.UpdateRangeAsync(queuedPostingItems);  
                        }
                        
                        currentLevelStatus = BatchStatus.Completed;
                        batchStatus.BatchStatus = BatchStatus.Completed;
                        batchStatus.IsCriticalError = false;
                        batchStatus.UpdateDate = DateTime.Now;
                        batchStatus.BatchSummary = currentLevel switch
                        {
                            PostingBatchLevel.Transfer => "PostingTransferJobFinishedSuccessfully",
                            PostingBatchLevel.TransferValidation => "PostingTransferValidationJobFinishedSuccessfully",
                            _ => "PostingJobFinishedSuccessfully"
                        };
                        await _postingBatchStatusRepository.UpdateAsync(batchStatus);
                    }

                    if (currentLevel == PostingBatchLevel.MerchantBlockage && currentLevelStatus == BatchStatus.Completed)
                    {
                        var items = await _postingItemRepository
                           .GetAll()
                           .Where(w => 
                                w.PostingDate.Date == DateTime.Now.Date 
                                && w.RecordStatus == RecordStatus.Active
                            )
                           .GroupBy(g => g.MerchantId)
                           .Select(s => new
                           {
                               MerchantId = s.Key,
                               TotalCount = s.Sum(w => w.TotalCount),
                               ErrorCount = s.Sum(w => w.ErrorCount)
                           })
                           .ToListAsync();
                        
                        var errorThreshold = await _cacheService.GetOrCreateAsync("PostingErrorThreshold",
                            async () =>
                            {
                                return await _vaultClient.GetSecretValueAsync<int>("PFSecrets", "PostingSettings", "ErrorThreshold");
                            });

                        foreach (var item in items)
                        {
                            var errorRate = Convert.ToDecimal(item.ErrorCount) > 0m ? 
                                Math.Round((Convert.ToDecimal(item.ErrorCount) / Convert.ToDecimal(item.TotalCount)) * 100m, 2) : 0m;

                            if (errorRate <= errorThreshold)
                            {
                                continue;
                            }

                            var merchant = await _merchantRepository.GetAll()
                                .FirstOrDefaultAsync(s => s.Id == item.MerchantId);
                            merchant.PaymentAllowed = false;
                            await _merchantRepository.UpdateAsync(merchant);
                            
                            var errorMessage = $"PostingTransferErrorRateIsHighForMerchant: Posting closed for merchant due to high error rate! MerchantId: {item.MerchantId}, ErrorRate: {errorRate}%";
                            await SendPostingFailedNotificationAsync(errorMessage);
                            _logger.LogCritical(errorMessage);
                        }
                    }

                    if (currentLevel == PostingBatchLevel.GrandBalancer && currentLevelStatus == BatchStatus.Completed)
                    {
                        using var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                        var endpoint = await _bus.GetSendEndpoint(new Uri("exchange:PF.PostingUpdateBalanceFields"));
                        await endpoint.Send(new PostingUpdateBalanceFields(), tokenSource.Token);
                    }
                }

                if (currentLevelStatus == BatchStatus.Completed)
                {
                    nextLevel = DetermineNextLevel(currentLevel);
                    nextLevelStatus = BatchStatus.Pending;
                    var nextLevelBatch = await GenerateNewBatchLevelAsync($"BatchManagerStartingNextLevel: {nextLevel}, Status: {nextLevelStatus}", nextLevelStatus, nextLevel);
                    await _postingBatchFactory.TriggerBatchAsync(nextLevelBatch);
                }
            }
            catch (Exception exception)
            {
                await SendPostingFailedNotificationAsync(exception.Message);
                _logger.LogError($"PostingBatchManagerError: {exception}");
            }
        }

        private async Task<PostingBatchStatus> GenerateNewBatchLevelAsync(string batchSummary, BatchStatus batchStatus, PostingBatchLevel batchLevel)
        {
            var postingBatchStatus = new PostingBatchStatus
            {
                PostingBatchLevel = batchLevel,
                BatchStatus = batchStatus,
                BatchSummary = batchSummary,
                IsCriticalError = false,
                StartTime = DateTime.Now,
                FinishTime = DateTime.Now,
                PostingDate = DateTime.Now.Date,
                UpdateDate = DateTime.Now,
                CreatedBy = _applicationUserService.ApplicationUserId.ToString(),
                BatchOrder = (int)batchLevel
            };

            await _postingBatchStatusRepository.AddAsync(postingBatchStatus);

            return postingBatchStatus;
        }

        private static PostingBatchLevel DetermineNextLevel(PostingBatchLevel currentLevel)
        {
            return currentLevel switch
            {
                PostingBatchLevel.BatchManager => PostingBatchLevel.Transfer,
                PostingBatchLevel.Transfer => PostingBatchLevel.TransferValidation,
                PostingBatchLevel.TransferValidation => PostingBatchLevel.MerchantBlockage,
                PostingBatchLevel.MerchantBlockage => PostingBatchLevel.BankBalancer,
                PostingBatchLevel.BankBalancer => PostingBatchLevel.GrandBalancer,
                PostingBatchLevel.GrandBalancer => PostingBatchLevel.DeductionBalancer,
                PostingBatchLevel.DeductionBalancer => PostingBatchLevel.ParentMerchantBalancer,
                PostingBatchLevel.ParentMerchantBalancer => PostingBatchLevel.DeductionTransfer,
                PostingBatchLevel.DeductionTransfer => PostingBatchLevel.DeductionCalculation,
                PostingBatchLevel.DeductionCalculation => PostingBatchLevel.PosBlockageAccounting,
                _ => throw new InvalidOperationException($"InvalidPostingBatchLevelDetected: {currentLevel}")
            };
        }

        private async Task SendPostingFailedNotificationAsync(string exceptionMessage)
        {
            try
            {
                if (exceptionMessage.Contains("InvalidPostingBatchLevelDetected"))
                {
                    return;
                }

                var message = "Merhaba,\r\nPosting sırasında sistemsel bir hata oluştu. Kontrol edilmesi gerekmektedir. <br><br>" + exceptionMessage;
                
                var notificationConfig =
                    _vaultClient.GetSecretValue<PostingFailedNotification>("PFSecrets", "PostingFailedNotification");
                
                var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
                
                if (notificationConfig.IsNotificationActiveForDevelopment)
                {
                    await _notificationService.SendAdvancedEmailNotificationAsync(new AdvancedEmailRequest
                    {
                        EventName = "PF.PostingFailed",
                        PreferredLanguage = "TR",
                        ReceiverId = Guid.Empty,
                        ToEmail = notificationConfig.Development.ToArray(),
                        TemplateParameters = new ()
                        {
                            {"Ortam", environment},
                            {"Hata", message}
                        }
                    });
                }
                
                await SendToBusAsync(environment, message);
            }
            catch (Exception exception)
            {
                _logger.LogError($"PostingFailedNotification Failed: {exception}");
            }
        }
        private async Task SendToBusAsync(string environment, string exceptionMessage)
        {
            await _bus.Publish(new PostingFailed
            {
                Environment = environment,
                Exception = exceptionMessage
            });
        }
    }
}