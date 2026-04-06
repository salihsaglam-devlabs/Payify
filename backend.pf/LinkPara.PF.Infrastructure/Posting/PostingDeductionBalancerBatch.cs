using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Commons.Models.Posting;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;
using LinkPara.PF.Infrastructure.Persistence;
using LinkPara.SharedModels.Persistence;
using LinkPara.SystemUser;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Transactions;

namespace LinkPara.PF.Infrastructure.Posting;

public class PostingDeductionBalancerBatch : IPostingBatch<PostingDeductionBalancer>
{
    private readonly ILogger<PostingDeductionBalancer> _logger;
    private readonly IGenericRepository<PostingBatchStatus> _postingBatchStatusRepository;
    private readonly IApplicationUserService _applicationUserService;
    private readonly PfDbContext _pfDbContext;

    public PostingDeductionBalancerBatch(ILogger<PostingDeductionBalancer> logger,
        IGenericRepository<PostingBatchStatus> postingBatchStatusRepository,
        IApplicationUserService applicationUserService,
        PfDbContext pfDbContext)
    {
        _logger = logger;
        _postingBatchStatusRepository = postingBatchStatusRepository;
        _applicationUserService = applicationUserService;
        _pfDbContext = pfDbContext;
    }

    public async Task ExecuteBatchAsync(PostingBatchStatus batchStatus)
    {
        try
        {
            var validDeductionTypes = new List<DeductionType>
            {
                DeductionType.RejectedChargeback,
                DeductionType.RejectedChargebackCommission,
                DeductionType.RejectedChargebackTransfer,
                DeductionType.RejectedSuspicious,
                DeductionType.RejectedSuspiciousCommission,
                DeductionType.RejectedSuspiciousTransfer
            };
            
            var merchantDeductions = await (from merchantDeduction in _pfDbContext.MerchantDeduction
                join merchantTransaction in _pfDbContext.MerchantTransaction on merchantDeduction.MerchantTransactionId
                    equals merchantTransaction.Id
                where merchantDeduction.DeductionStatus == DeductionStatus.Pending
                      && merchantDeduction.ExecutionDate <= DateTime.Now
                      && validDeductionTypes.Contains(merchantDeduction.DeductionType)
                orderby merchantDeduction.CreateDate
                select new
                {
                    MerchantDeduction = merchantDeduction,
                    TransactionDate = merchantTransaction.TransactionDate,
                    MerchantId = merchantDeduction.MerchantId
                }).ToListAsync();

            var merchantIds = merchantDeductions.Select(s => s.MerchantId).Distinct().ToList();

            var paymentNotAllowedMerchants = await _pfDbContext.Merchant
                .Where(t =>
                    merchantIds.Contains(t.Id) &&
                    !t.PaymentAllowed
                )
                .Select(t => t.Id)
                .ToListAsync();

            var deductionGroups = merchantDeductions
                .GroupBy(x => new
                {
                    x.MerchantId,
                    GroupKey = GetGroupKey(x.MerchantDeduction.DeductionType),
                    Currency = x.MerchantDeduction.Currency,
                    TransactionDate = x.TransactionDate.Date
                });

            var strategy = _pfDbContext.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

                foreach (var deductionGroup in deductionGroups)
                {
                    var deductionBalance = new PostingBalance
                    {
                        MerchantId = deductionGroup.Key.MerchantId,
                        PostingDate = DateTime.Today.Date,
                        PaymentDate = DateTime.Today.Date,
                        OldPaymentDate = DateTime.Today.Date,
                        Currency = deductionGroup.Key.Currency,
                        TransactionDate = deductionGroup.Key.TransactionDate,
                        TotalAmount = deductionGroup.Sum(s => s.MerchantDeduction.RemainingDeductionAmount),
                        TotalPointAmount = 0,
                        TotalBankCommissionAmount = 0,
                        TotalAmountWithoutBankCommission =
                            deductionGroup.Sum(s => s.MerchantDeduction.RemainingDeductionAmount),
                        TotalPfCommissionAmount = 0,
                        TotalPfNetCommissionAmount = 0,
                        TotalAmountWithoutCommissions =
                            deductionGroup.Sum(s => s.MerchantDeduction.RemainingDeductionAmount),
                        TotalDueAmount = 0,
                        TotalDueTransferAmount = 0,
                        TotalPayingAmount = deductionGroup.Sum(s => s.MerchantDeduction.RemainingDeductionAmount),
                        TotalChargebackAmount = 0,
                        TotalChargebackCommissionAmount = 0,
                        TotalChargebackTransferAmount = 0,
                        TotalSuspiciousAmount = 0,
                        TotalSuspiciousCommissionAmount = 0,
                        TotalSuspiciousTransferAmount = 0,
                        TotalExcessReturnAmount = 0,
                        TotalExcessReturnTransferAmount = 0,
                        TotalExcessReturnOnCommissionAmount = 0,
                        TotalNegativeBalanceAmount = 0,
                        MoneyTransferStatus = paymentNotAllowedMerchants.Contains(deductionGroup.Key.MerchantId) ? PostingMoneyTransferStatus.PaymentBlocked : PostingMoneyTransferStatus.Pending,
                        PostingPaymentChannel = PostingPaymentChannel.Unknown,
                        RetryCount = 0,
                        TransactionCount = deductionGroup.Count(),
                        BatchStatus = BatchStatus.Pending,
                        BlockageStatus = BlockageStatus.None,
                        BTransStatus = PostingBTransStatus.Completed,
                        AccountingStatus = PostingAccountingStatus.PendingDeductionBalance,
                        CreatedBy = _applicationUserService.ApplicationUserId.ToString(),
                        PostingBalanceType = deductionGroup.Key.GroupKey is "ChargebackGroup"
                            ? PostingBalanceType.ChargebackReturn
                            : PostingBalanceType.SuspiciousReturn
                    };

                    await _pfDbContext.PostingBalance.AddAsync(deductionBalance);

                    foreach (var deduction in deductionGroup)
                    {
                        var merchantDeduction = deduction.MerchantDeduction;
                        _pfDbContext.Entry(merchantDeduction).State = EntityState.Modified;
                        merchantDeduction.DeductionStatus = DeductionStatus.Completed;
                        merchantDeduction.RemainingDeductionAmount = 0;

                        var deductionTransaction = new DeductionTransaction
                        {
                            MerchantId = merchantDeduction.MerchantId,
                            PostingBalanceId = deductionBalance.Id,
                            MerchantDeductionId = merchantDeduction.Id,
                            DeductionType = merchantDeduction.DeductionType,
                            Amount = merchantDeduction.TotalDeductionAmount,
                            Currency = merchantDeduction.Currency,
                            CreatedBy = _applicationUserService.ApplicationUserId.ToString()
                        };

                        var originalPostingTransaction =
                            await _pfDbContext.PostingTransaction.FirstOrDefaultAsync(s =>
                                s.MerchantTransactionId == merchantDeduction.MerchantTransactionId);
                        if (originalPostingTransaction == null)
                        {
                            throw new InvalidOperationException(
                                $"PostingTransaction not found for MerchantTransactionId {merchantDeduction.MerchantTransactionId}");
                        }

                        var postingAdditionalTransaction = new PostingAdditionalTransaction
                        {
                            MerchantId = merchantDeduction.MerchantId,
                            TransactionType = deductionGroup.Key.GroupKey is "ChargebackGroup"
                                ? TransactionType.RejectedChargeback
                                : TransactionType.RejectedSuspicious,
                            TransactionDate = originalPostingTransaction.TransactionDate,
                            PostingDate = deductionBalance.PostingDate,
                            PaymentDate = deductionBalance.PaymentDate,
                            OldPaymentDate = originalPostingTransaction.PaymentDate,
                            CardNumber = originalPostingTransaction.CardNumber,
                            OrderId = originalPostingTransaction.OrderId,
                            InstallmentCount = originalPostingTransaction.InstallmentCount,
                            Currency = originalPostingTransaction.Currency,
                            PointAmount = 0,
                            PricingProfileNumber = originalPostingTransaction.PricingProfileNumber,
                            BatchStatus = BatchStatus.Completed,
                            BlockageStatus = BlockageStatus.None,
                            MerchantTransactionId = originalPostingTransaction.MerchantTransactionId,
                            PostingBankBalanceId = Guid.Empty,
                            PostingBalanceId = deductionBalance.Id,
                            PricingProfileItemId = originalPostingTransaction.PricingProfileItemId,
                            VposId = originalPostingTransaction.VposId,
                            AcquireBankCode = originalPostingTransaction.AcquireBankCode,
                            TransactionStartDate = originalPostingTransaction.TransactionStartDate,
                            TransactionEndDate = originalPostingTransaction.TransactionEndDate,
                            BTransStatus = PostingBTransStatus.Completed,
                            ConversationId = originalPostingTransaction.ConversationId,
                            CreatedBy = originalPostingTransaction.CreatedBy,
                            Amount = merchantDeduction.TotalDeductionAmount,
                            BankCommissionRate = 0,
                            BankCommissionAmount = 0,
                            AmountWithoutBankCommission = merchantDeduction.TotalDeductionAmount,
                            PfCommissionRate = 0,
                            PfPerTransactionFee = 0,
                            ParentMerchantCommissionAmount = 0,
                            ParentMerchantCommissionRate = 0,
                            AmountWithoutParentMerchantCommission = merchantDeduction.TotalDeductionAmount,
                            PfCommissionAmount = 0,
                            PfNetCommissionAmount = 0,
                            AmountWithoutCommissions = merchantDeduction.TotalDeductionAmount,
                            MerchantDeductionId = merchantDeduction.Id,
                            RelatedPostingBalanceId = Guid.Empty,
                            SubMerchantId = Guid.Empty,
                            SubMerchantName = string.Empty,
                            SubMerchantNumber = string.Empty,
                            EasySubMerchantName = string.Empty,
                            EasySubMerchantNumber = string.Empty,
                            PfTransactionSource = PfTransactionSource.VirtualPos,
                            MerchantPhysicalPosId = Guid.Empty,
                            InstallmentSequence = 0,
                            IsPerInstallment = false,
                            MerchantInstallmentTransactionId = Guid.Empty
                        };

                        await _pfDbContext.DeductionTransaction.AddAsync(deductionTransaction);
                        await _pfDbContext.PostingAdditionalTransaction.AddAsync(postingAdditionalTransaction);
                    }
                }

                _pfDbContext.Entry(batchStatus).State = EntityState.Modified;
                batchStatus.BatchStatus = BatchStatus.Completed;
                batchStatus.IsCriticalError = false;
                batchStatus.BatchSummary = "PostingDeductionBalancerJobFinishedSuccessfully";
                batchStatus.UpdateDate = DateTime.Now;

                await _pfDbContext.SaveChangesAsync();
                scope.Complete();
            });
        }
        catch (Exception exception)
        {
            _logger.LogError($"PostingDeductionBalancerError, {exception}");

            batchStatus.BatchSummary = "PostingDeductionBalancerError";
            batchStatus.BatchStatus = BatchStatus.Error;
            batchStatus.UpdateDate = DateTime.Now;
            batchStatus.IsCriticalError = true;

            await _postingBatchStatusRepository.UpdateAsync(batchStatus);
        }
    }

    private static string GetGroupKey(DeductionType type)
    {
        return type switch
        {
            DeductionType.RejectedChargeback => "ChargebackGroup",
            DeductionType.RejectedChargebackCommission => "ChargebackGroup",
            DeductionType.RejectedChargebackTransfer => "ChargebackGroup",

            DeductionType.RejectedSuspicious => "SuspiciousGroup",
            DeductionType.RejectedSuspiciousCommission => "SuspiciousGroup",
            DeductionType.RejectedSuspiciousTransfer => "SuspiciousGroup",

            _ => type.ToString()
        };
    }
}