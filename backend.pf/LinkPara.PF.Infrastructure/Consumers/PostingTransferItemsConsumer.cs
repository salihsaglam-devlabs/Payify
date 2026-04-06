using System.Transactions;
using LinkPara.HttpProviders.Vault;
using LinkPara.PF.Application.Commons.Helpers;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;
using LinkPara.PF.Infrastructure.Persistence;
using LinkPara.SharedModels.Persistence;
using LinkPara.SystemUser;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LinkPara.PF.Infrastructure.Consumers;

public class PostingTransferItemsConsumer : IConsumer<PostingItem>
{
    private readonly ILogger<PostingTransferItemsConsumer> _logger;
    private readonly IGenericRepository<MerchantTransaction> _merchantTransactionRepository;
    private readonly IGenericRepository<Merchant> _merchantRepository;
    private readonly IGenericRepository<PostingItem> _postingItemRepository;
    private readonly IGenericRepository<MerchantInstallmentTransaction> _installmentTransactionRepository;
    private readonly PfDbContext _dbContext;
    private readonly IApplicationUserService _applicationUserService;
    private readonly IVaultClient _vaultClient;

    public PostingTransferItemsConsumer(
        ILogger<PostingTransferItemsConsumer> logger,
        IGenericRepository<MerchantTransaction> merchantTransactionRepository,
        IGenericRepository<Merchant> merchantRepository,
        PfDbContext dbContext,
        IGenericRepository<PostingItem> postingItemRepository,
        IApplicationUserService applicationUserService,
        IVaultClient vaultClient, IGenericRepository<MerchantInstallmentTransaction> installmentTransactionRepository)
    {
        _logger = logger;
        _merchantTransactionRepository = merchantTransactionRepository;
        _merchantRepository = merchantRepository;
        _dbContext = dbContext;
        _postingItemRepository = postingItemRepository;
        _applicationUserService = applicationUserService;
        _vaultClient = vaultClient;
        _installmentTransactionRepository = installmentTransactionRepository;
    }


    public async Task Consume(ConsumeContext<PostingItem> postingItem)
    {
        var message = postingItem.Message;
        try
        {
            await ProcessMerchantTransactionsAsync(message);
        }
        catch (Exception exception)
        {
            _logger.LogCritical(exception,
                "PostingTransferItemsErrorForMerchant: {MerchantId}", message.MerchantId);
            message.ErrorCount = message.TotalCount;
            message.BatchStatus = BatchStatus.Error;
        }

        await _postingItemRepository.UpdateAsync(message);
    }

    private async Task ProcessMerchantTransactionsAsync(PostingItem postingItem)
    {
        var merchantTransactions = await _merchantTransactionRepository.GetAll()
            .Where(t => t.PostingItemId == postingItem.Id)
            .OrderByDescending(s => s.Amount)
            .ToListAsync();

        if (!merchantTransactions.Any())
        {
            _logger.LogError(
                "NoItemsFoundForMerchant: PostingItem({PostingItemId}), Merchant({MerchantId})",
                postingItem.Id, postingItem.MerchantId);
            postingItem.BatchStatus = BatchStatus.Error;
            postingItem.ErrorCount = postingItem.TotalCount;
            await _postingItemRepository.UpdateAsync(postingItem);
            return;
        }

        if (merchantTransactions.Count != postingItem.TotalCount)
        {
            _logger.LogError(
                "TotalCountIsNotMatched: PostingItem({PostingItemId}), Merchant({MerchantId})",
                postingItem.Id, postingItem.MerchantId);
            postingItem.BatchStatus = BatchStatus.Error;
            postingItem.ErrorCount = postingItem.TotalCount;
            await _postingItemRepository.UpdateAsync(postingItem);
            return;
        }

        var merchant = await _merchantRepository
            .GetAll()
            .SingleOrDefaultAsync(w => w.Id == postingItem.MerchantId);

        if (merchant == null)
        {
            _logger.LogError(
                "MerchantIsNullForMerchantId: PostingItem({PostingItemId}), Merchant({MerchantId})",
                postingItem.Id, postingItem.MerchantId);
            postingItem.BatchStatus = BatchStatus.Error;
            postingItem.ErrorCount = postingItem.TotalCount;
            await _postingItemRepository.UpdateAsync(postingItem);
            return;
        }

        postingItem.BatchStatus = BatchStatus.Pending;
        await _postingItemRepository.UpdateAsync(postingItem);

        var perInstallmentTransactionIds =
            merchantTransactions.Where(s => s.IsPerInstallment).Select(s => s.Id).ToList();
        var installmentTransactions = await _installmentTransactionRepository.GetAll()
            .Where(t => perInstallmentTransactionIds.Contains(t.MerchantTransactionId))
            .ToListAsync();

        var isActivePfNetCommissionNegativeAmount = await _vaultClient.GetSecretValueAsync<bool>("PFSecrets",
            "PostingSettings", "IsActivePfNetCommissionNegativeAmount");
        if (!isActivePfNetCommissionNegativeAmount)
        {
            var merchantTransactionsAtLoss = merchantTransactions
                .Where(s => s.PfNetCommissionAmount < 0).ToList();

            if (merchantTransactionsAtLoss.Count > 0)
            {
                var strategy = _dbContext.Database.CreateExecutionStrategy();
                await strategy.ExecuteAsync(async () =>
                {
                    using var scope = new TransactionScope(TransactionScopeOption.RequiresNew,
                        new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
                        TransactionScopeAsyncFlowOption.Enabled);

                    var now = DateTime.Now.Date;
                    var createdBy = _applicationUserService.ApplicationUserId.ToString();
                    var lossTransactionIds = merchantTransactionsAtLoss.Select(s => s.Id).ToList();

                    await _dbContext.PostingTransferError
                        .Where(w => lossTransactionIds.Contains(w.MerchantTransactionId)
                                    && w.RecordStatus == RecordStatus.Active)
                        .ExecuteUpdateAsync(u => u.SetProperty(p => p.RecordStatus, RecordStatus.Passive));

                    var errorRecords = merchantTransactionsAtLoss.Select(mt => new PostingTransferError
                    {
                        MerchantId = merchant.Id,
                        MerchantTransactionId = mt.Id,
                        TransferErrorCategory = TransferErrorCategory.NegativePfNetCommissionAmount,
                        ErrorMessage = $"PfNetCommissionAmountIsNegative: {mt.PfNetCommissionAmount}",
                        PostingDate = now,
                        CreatedBy = createdBy
                    }).ToList();
                    await _dbContext.PostingTransferError.AddRangeAsync(errorRecords);

                    merchantTransactionsAtLoss.ForEach(mt => mt.BatchStatus = BatchStatus.Error);
                    _dbContext.MerchantTransaction.UpdateRange(merchantTransactionsAtLoss);

                    var lossInstallments = installmentTransactions
                        .Where(s => lossTransactionIds.Contains(s.MerchantTransactionId))
                        .ToList();
                    lossInstallments.ForEach(s => s.BatchStatus = BatchStatus.Error);
                    _dbContext.MerchantInstallmentTransaction.UpdateRange(lossInstallments);

                    postingItem.ErrorCount += merchantTransactionsAtLoss.Count;

                    await _dbContext.SaveChangesAsync();
                    scope.Complete();
                });

                merchantTransactions = merchantTransactions.Except(merchantTransactionsAtLoss).ToList();
            }
        }
        
        var merchantTransactionIds = merchantTransactions.Select(s => s.Id).ToList();
        var existingTransactions =
            await _dbContext.PostingTransaction.Where(s => merchantTransactionIds.Contains(s.MerchantTransactionId))
                .ToListAsync();

        try
        {
            var strategy = _dbContext.Database.CreateExecutionStrategy();

            await strategy.ExecuteAsync(async () =>
            {
                using var scope = new TransactionScope(TransactionScopeOption.RequiresNew,
                    new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
                    TransactionScopeAsyncFlowOption.Enabled);

                var postingTransactionsToAdd = new List<PostingTransaction>();
                var installmentsToMarkCompleted = new List<MerchantInstallmentTransaction>();

                var existingMerchantTransactionIds = existingTransactions
                    .Select(s => s.MerchantTransactionId)
                    .ToHashSet();

                foreach (var merchantTransaction in merchantTransactions)
                {
                    if (!existingMerchantTransactionIds.Contains(merchantTransaction.Id))
                    {
                        if (merchantTransaction.IsPerInstallment)
                        {
                            var merchantInstallmentTransactions = installmentTransactions
                                .Where(s => s.MerchantTransactionId == merchantTransaction.Id)
                                .ToList();

                            postingTransactionsToAdd.AddRange(
                                merchantInstallmentTransactions.Select(s =>
                                    MapToPostingTransaction(merchantTransaction, merchant, s)));

                            installmentsToMarkCompleted.AddRange(merchantInstallmentTransactions);
                        }
                        else
                        {
                            postingTransactionsToAdd.Add(
                                MapToPostingTransaction(merchantTransaction, merchant));
                        }
                    }

                    merchantTransaction.BatchStatus = BatchStatus.Completed;
                    _dbContext.MerchantTransaction.Update(merchantTransaction);
                }

                if (postingTransactionsToAdd.Count > 0)
                    await _dbContext.PostingTransaction.AddRangeAsync(postingTransactionsToAdd);

                if (installmentsToMarkCompleted.Count > 0)
                {
                    installmentsToMarkCompleted.ForEach(s => s.BatchStatus = BatchStatus.Completed);
                    _dbContext.MerchantInstallmentTransaction.UpdateRange(installmentsToMarkCompleted);
                }
                
                await _dbContext.PostingTransferError
                    .Where(w => merchantTransactionIds.Contains(w.MerchantTransactionId)
                                && w.RecordStatus == RecordStatus.Active)
                    .ExecuteUpdateAsync(u => u.SetProperty(p => p.RecordStatus, RecordStatus.Passive));

                await _dbContext.SaveChangesAsync();
                scope.Complete();
            });
        }
        catch (Exception exception)
        {
            _logger.LogCritical(exception,
                "PostingBatchTransactionError for PostingItem {PostingItemId}", postingItem.Id);
            _dbContext.ChangeTracker.Clear();

            var strategy = _dbContext.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                using var scope = new TransactionScope(TransactionScopeOption.RequiresNew,
                    new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
                    TransactionScopeAsyncFlowOption.Enabled);

                var now = DateTime.Now.Date;
                var createdBy = _applicationUserService.ApplicationUserId.ToString();
                const int maxStackTraceLength = 2000;
                var truncatedStackTrace = exception.StackTrace.Truncate(maxStackTraceLength);

                await _dbContext.PostingTransferError
                    .Where(w => merchantTransactionIds.Contains(w.MerchantTransactionId)
                                && w.RecordStatus == RecordStatus.Active)
                    .ExecuteUpdateAsync(u => u.SetProperty(p => p.RecordStatus, RecordStatus.Passive));

                var errorRecords = merchantTransactions.Select(mt => new PostingTransferError
                {
                    MerchantId = merchant.Id,
                    MerchantTransactionId = mt.Id,
                    TransferErrorCategory = TransferErrorCategory.TransferSystemError,
                    ErrorMessage = $"PostingBatchTransactionError: {exception.Message}",
                    StackTrace = truncatedStackTrace,
                    PostingDate = now,
                    CreatedBy = createdBy
                }).ToList();
                await _dbContext.PostingTransferError.AddRangeAsync(errorRecords);

                merchantTransactions.ForEach(mt => mt.BatchStatus = BatchStatus.Error);
                _dbContext.MerchantTransaction.UpdateRange(merchantTransactions);

                var failedInstallments = installmentTransactions
                    .Where(s => merchantTransactionIds.Contains(s.MerchantTransactionId))
                    .ToList();
                failedInstallments.ForEach(s => s.BatchStatus = BatchStatus.Error);
                _dbContext.MerchantInstallmentTransaction.UpdateRange(failedInstallments);

                await _dbContext.SaveChangesAsync();
                scope.Complete();
            });

            postingItem.ErrorCount = postingItem.TotalCount;
            postingItem.BatchStatus = BatchStatus.Error;
            await _postingItemRepository.UpdateAsync(postingItem);
            return;
        }

        postingItem.BatchStatus = BatchStatus.Completed;
        await _postingItemRepository.UpdateAsync(postingItem);
    }

    private static PostingTransaction MapToPostingTransaction(
        MerchantTransaction merchantTransaction,
        Merchant merchant,
        MerchantInstallmentTransaction installment = null)
    {
        var source = installment is not null;
        return new PostingTransaction
        {
            MerchantId = source ? installment!.MerchantId : merchantTransaction.MerchantId,
            OrderId = source ? installment!.OrderId : merchantTransaction.OrderId,
            MerchantTransactionId = source ? installment!.Id : merchantTransaction.Id,
            TransactionType = source ? installment!.TransactionType : merchantTransaction.TransactionType,
            TransactionDate = source ? installment!.TransactionDate : merchantTransaction.TransactionDate,
            PostingDate = DateTime.Now.Date,
            PaymentDate = source ? installment!.PfPaymentDate : merchantTransaction.PfPaymentDate,
            BankPaymentDate = source ? installment!.BankPaymentDate : merchantTransaction.BankPaymentDate,
            OldPaymentDate = source ? installment!.PfPaymentDate : merchantTransaction.PfPaymentDate,
            CardNumber = source ? installment!.CardNumber : merchantTransaction.CardNumber,
            InstallmentCount = merchantTransaction.InstallmentCount,
            InstallmentSequence = source ? installment!.InstallmentCount : 1,
            Currency = source ? installment!.Currency : merchantTransaction.Currency,
            Amount = source ? installment!.Amount : merchantTransaction.Amount,
            PointAmount = source ? installment!.PointAmount : merchantTransaction.PointAmount,
            BankCommissionRate = source ? installment!.BankCommissionRate : merchantTransaction.BankCommissionRate,
            BankCommissionAmount =
                source ? installment!.BankCommissionAmount : merchantTransaction.BankCommissionAmount,
            AmountWithoutBankCommission = source
                ? installment!.AmountWithoutBankCommission
                : merchantTransaction.AmountWithoutBankCommission,
            PfCommissionRate = source ? installment!.PfCommissionRate : merchantTransaction.PfCommissionRate,
            PfPerTransactionFee = source ? installment!.PfPerTransactionFee : merchantTransaction.PfPerTransactionFee,
            PfCommissionAmount = source ? installment!.PfCommissionAmount : merchantTransaction.PfCommissionAmount,
            ParentMerchantCommissionAmount = source
                ? installment!.ParentMerchantCommissionAmount
                : merchantTransaction.ParentMerchantCommissionAmount,
            ParentMerchantCommissionRate = source
                ? installment!.ParentMerchantCommissionRate
                : merchantTransaction.ParentMerchantCommissionRate,
            AmountWithoutParentMerchantCommission = source
                ? installment!.AmountWithoutParentMerchantCommission
                : merchantTransaction.AmountWithoutParentMerchantCommission,
            BatchStatus = BatchStatus.Pending,
            BlockageStatus = BlockageStatus.None,
            CreatedBy = source ? installment!.CreatedBy : merchantTransaction.CreatedBy,
            PricingProfileNumber = merchant.PricingProfileNumber,
            AcquireBankCode = source ? installment!.AcquireBankCode : merchantTransaction.AcquireBankCode,
            PricingProfileItemId =
                source ? installment!.PricingProfileItemId : merchantTransaction.PricingProfileItemId,
            VposId = source ? installment!.VposId : merchantTransaction.VposId,
            ParentMerchantId = merchant.ParentMerchantId ?? Guid.Empty,
            PfNetCommissionAmount =
                source ? installment!.PfNetCommissionAmount : merchantTransaction.PfNetCommissionAmount,
            AmountWithoutCommissions =
                source ? installment!.AmountWithoutCommissions : merchantTransaction.AmountWithoutCommissions,
            TransactionStartDate =
                source ? installment!.TransactionStartDate : merchantTransaction.TransactionStartDate,
            TransactionEndDate = source ? installment!.TransactionEndDate : merchantTransaction.TransactionEndDate,
            BTransStatus = PostingBTransStatus.Pending,
            ConversationId = source ? installment!.ConversationId : merchantTransaction.ConversationId,
            MerchantDeductionId = Guid.Empty,
            RelatedPostingBalanceId = Guid.Empty,
            SubMerchantId = Guid.Empty,
            SubMerchantName = string.Empty,
            SubMerchantNumber = string.Empty,
            EasySubMerchantName = source ? installment!.SubMerchantName : merchantTransaction.SubMerchantName,
            EasySubMerchantNumber = source ? installment!.SubMerchantNumber : merchantTransaction.SubMerchantNumber,
            PfTransactionSource = source ? installment!.PfTransactionSource : merchantTransaction.PfTransactionSource,
            MerchantPhysicalPosId =
                source ? installment!.MerchantPhysicalPosId : merchantTransaction.MerchantPhysicalPosId,
            IsPerInstallment = merchantTransaction.IsPerInstallment,
            MerchantInstallmentTransactionId = source ? installment!.Id : Guid.Empty
        };
    }
}