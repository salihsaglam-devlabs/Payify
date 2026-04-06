using System.Transactions;
using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Commons.Models.Posting;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;
using LinkPara.PF.Infrastructure.Persistence;
using LinkPara.SharedModels.Persistence;
using LinkPara.SystemUser;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LinkPara.PF.Infrastructure.Posting;

public class PostingParentMerchantBalancerBatch : IPostingBatch<PostingParentMerchantBalancer>
{
    private readonly ILogger<PostingParentMerchantBalancer> _logger;
    private readonly IGenericRepository<PostingBatchStatus> _postingBatchStatusRepository;
    private readonly IApplicationUserService _applicationUserService;
    private readonly PfDbContext _pfDbContext;

    public PostingParentMerchantBalancerBatch(ILogger<PostingParentMerchantBalancer> logger,
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
            var postingBalances = await _pfDbContext
                .PostingBalance
                .Include(s => s.Merchant)
                .Where(w => 
                    ((w.PostingDate == DateTime.Now.Date && w.BatchStatus == BatchStatus.Pending)
                    ||
                    (w.PostingDate == DateTime.Now.Date  && w.MoneyTransferStatus == PostingMoneyTransferStatus.PaymentNotRequired)
                    ||
                    w.BatchStatus == BatchStatus.Error) &&
                    w.ParentMerchantId != Guid.Empty
                 )
                .ToListAsync();
            
            var postingBalanceIdSet = new HashSet<Guid>(postingBalances.Select(s => s.Id));
            
            var alreadyCommissionedBalanceIds = await _pfDbContext.PostingAdditionalTransaction
                .Where(s => 
                    postingBalanceIdSet.Contains(s.RelatedPostingBalanceId) &&
                    s.TransactionType == TransactionType.ParentMerchantCommission)
                .Select(s => s.RelatedPostingBalanceId)
                .ToListAsync();

            var commissionBalances = postingBalances.Where(s => !alreadyCommissionedBalanceIds.Contains(s.Id)).ToList();
            
            var subMerchantIds = commissionBalances.Select(s => s.MerchantId).Distinct().ToList();
            var subMerchants = await _pfDbContext.Merchant.Where(s => subMerchantIds.Contains(s.Id)).ToListAsync();
            
            var newBalances = commissionBalances
                .GroupBy(g => new { g.ParentMerchantId, g.PaymentDate, g.PostingDate, g.Currency, g.BlockageStatus, g.TransactionDate })
                .Select(g => new PostingBalance
                {
                    MerchantId = g.Key.ParentMerchantId,
                    PostingDate = g.Key.PostingDate,
                    PaymentDate = g.Key.PaymentDate,
                    OldPaymentDate = g.FirstOrDefault()!.OldPaymentDate,
                    Currency = g.Key.Currency,
                    TransactionDate = g.Key.TransactionDate,
                    TotalAmount = g.Sum(s => s.TotalParentMerchantCommissionAmount),
                    TotalPointAmount = 0,
                    TotalBankCommissionAmount = 0,
                    TotalAmountWithoutBankCommission = g.Sum(s => s.TotalParentMerchantCommissionAmount),
                    TotalPfCommissionAmount = 0,
                    TotalPfNetCommissionAmount = 0,
                    TotalAmountWithoutCommissions = g.Sum(s => s.TotalParentMerchantCommissionAmount),
                    TotalParentMerchantCommissionAmount = 0,
                    TotalDueAmount = 0,
                    TotalDueTransferAmount = 0,
                    TotalChargebackAmount = 0,
                    TotalChargebackCommissionAmount = 0,
                    TotalChargebackTransferAmount = 0,
                    TotalSuspiciousAmount = 0,
                    TotalSuspiciousCommissionAmount = 0,
                    TotalSuspiciousTransferAmount = 0,
                    TotalExcessReturnAmount = 0,
                    TotalExcessReturnTransferAmount = 0,
                    TotalExcessReturnOnCommissionAmount = 0,
                    TotalNegativeBalanceAmount = g.Sum(s => s.TotalParentMerchantCommissionAmount) < 0 ? -1 * g.Sum(s => s.TotalParentMerchantCommissionAmount) : 0,
                    TotalPayingAmount = g.Sum(s => s.TotalParentMerchantCommissionAmount) > 0 ? g.Sum(s => s.TotalParentMerchantCommissionAmount) : 0,
                    MoneyTransferStatus = g.Sum(s => s.TotalParentMerchantCommissionAmount) <= 0
                        ? PostingMoneyTransferStatus.PaymentNotRequired
                        : (g.FirstOrDefault()!.Merchant.PaymentAllowed
                            ? PostingMoneyTransferStatus.Pending
                            : PostingMoneyTransferStatus.PaymentBlocked),
                    RetryCount = 0,
                    TransactionCount = g.Count(),
                    BatchStatus = g.Sum(s => s.TotalParentMerchantCommissionAmount) <= 0 ? BatchStatus.Completed : BatchStatus.Pending,
                    BlockageStatus = g.Key.BlockageStatus,
                    CreatedBy = _applicationUserService.ApplicationUserId.ToString(),
                    AccountingStatus = PostingAccountingStatus.PendingDeductionBalance,
                    PostingBalanceType = PostingBalanceType.ParentMerchantCommission,
                    PostingPaymentChannel = PostingPaymentChannel.Unknown,
                    BTransStatus = PostingBTransStatus.Completed,
                    ParentMerchantId = Guid.Empty
                })
                .ToList();
            
            var excessReturnDeductions = newBalances.Where(s => s.TotalNegativeBalanceAmount > 0)
                .Select(excessReturnBalance => new MerchantDeduction
                {
                    TotalDeductionAmount = excessReturnBalance.TotalNegativeBalanceAmount,
                    RemainingDeductionAmount = excessReturnBalance.TotalNegativeBalanceAmount,
                    Currency = excessReturnBalance.Currency,
                    DeductionType = DeductionType.ExcessReturnOnCommission,
                    DeductionStatus = DeductionStatus.Pending,
                    ExecutionDate = DateTime.Now,
                    MerchantId = excessReturnBalance.MerchantId,
                    MerchantTransactionId = Guid.Empty,
                    MerchantDueId = Guid.Empty,
                    RecordStatus = RecordStatus.Active,
                    CreatedBy = _applicationUserService.ApplicationUserId.ToString(),
                    PostingBalanceId = excessReturnBalance.Id,
                    ConversationId = string.Empty,
                    DeductionAmountWithCommission = excessReturnBalance.TotalNegativeBalanceAmount,
                    SubMerchantDeductionId = Guid.Empty
                })
                .ToList();

            var strategy = _pfDbContext.Database.CreateExecutionStrategy();

            await strategy.ExecuteAsync(async () =>
            {
                using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

                await _pfDbContext.PostingBalance.AddRangeAsync(newBalances);
                await _pfDbContext.MerchantDeduction.AddRangeAsync(excessReturnDeductions);
                
                var additionalTransactions = new List<PostingAdditionalTransaction>();
                
                foreach (var newBalance in newBalances)
                {
                    var relatedBalances = commissionBalances.Where(pb =>
                        pb.ParentMerchantId == newBalance.MerchantId &&
                        pb.PaymentDate == newBalance.PaymentDate &&
                        pb.PostingDate == newBalance.PostingDate &&
                        pb.Currency == newBalance.Currency &&
                        pb.BlockageStatus == newBalance.BlockageStatus &&
                        pb.TransactionDate == newBalance.TransactionDate
                    ).ToList();

                    foreach (var relatedBalance in relatedBalances)
                    {
                        var relatedMerchant = subMerchants.FirstOrDefault(s => s.Id == relatedBalance.MerchantId);
                        
                        additionalTransactions.Add(new PostingAdditionalTransaction
                        {
                            PostingBalanceId = newBalance.Id,
                            RelatedPostingBalanceId = relatedBalance.Id,
                            MerchantId = newBalance.MerchantId,
                            TransactionType = TransactionType.ParentMerchantCommission,
                            TransactionDate = newBalance.TransactionDate,
                            PostingDate = newBalance.PostingDate,
                            PaymentDate = newBalance.PaymentDate,
                            OldPaymentDate = newBalance.PaymentDate,
                            CardNumber = string.Empty,
                            OrderId = string.Empty,
                            InstallmentCount = 0,
                            Currency = newBalance.Currency,
                            PointAmount = 0,
                            PricingProfileNumber = string.Empty,
                            BatchStatus = BatchStatus.Completed,
                            BlockageStatus = BlockageStatus.None,
                            MerchantTransactionId = Guid.Empty,
                            PostingBankBalanceId = Guid.Empty,
                            PricingProfileItemId = Guid.Empty,
                            VposId = Guid.Empty,
                            AcquireBankCode = 0,
                            TransactionStartDate = DateTime.Now,
                            TransactionEndDate = DateTime.Now,
                            BTransStatus = PostingBTransStatus.Completed,
                            ConversationId = string.Empty,
                            CreatedBy = newBalance.CreatedBy,
                            Amount = relatedBalance.TotalAmount,
                            BankCommissionRate = 0,
                            BankCommissionAmount = 0,
                            AmountWithoutBankCommission = relatedBalance.TotalParentMerchantCommissionAmount,
                            PfCommissionRate = 0,
                            PfPerTransactionFee = 0,
                            ParentMerchantCommissionAmount = 0,
                            ParentMerchantCommissionRate = 0,
                            AmountWithoutParentMerchantCommission = relatedBalance.TotalParentMerchantCommissionAmount,
                            PfCommissionAmount = 0,
                            PfNetCommissionAmount = 0,
                            AmountWithoutCommissions = relatedBalance.TotalParentMerchantCommissionAmount,
                            MerchantDeductionId = Guid.Empty,
                            SubMerchantName = relatedMerchant?.Name,
                            SubMerchantId = relatedBalance.MerchantId,
                            SubMerchantNumber = relatedMerchant?.Number,
                            EasySubMerchantName = string.Empty,
                            EasySubMerchantNumber = string.Empty,
                            PfTransactionSource = PfTransactionSource.VirtualPos,
                            MerchantPhysicalPosId = Guid.Empty,
                            InstallmentSequence = 0,
                            IsPerInstallment = false,
                            MerchantInstallmentTransactionId = Guid.Empty
                        });

                        relatedBalance.BatchStatus = BatchStatus.ParentMerchantCommissionCalculated;
                        relatedBalance.UpdateDate = DateTime.Now;
                        _pfDbContext.PostingBalance.Update(relatedBalance);
                    }

                    await _pfDbContext.PostingAdditionalTransaction.AddRangeAsync(additionalTransactions);
                }
                
                _pfDbContext.PostingBatchStatus.Attach(batchStatus);
                batchStatus.BatchStatus = BatchStatus.Completed;
                batchStatus.IsCriticalError = false;
                batchStatus.BatchSummary = "PostingParentMerchantBalancerJobFinishedSuccessfully";
                batchStatus.UpdateDate = DateTime.Now;
                
                await _pfDbContext.SaveChangesAsync();
                
                scope.Complete();
            });
        }
        catch (Exception exception)
        {
            _logger.LogError($"PostingParentMerchantBalancerError, {exception}");

            batchStatus.BatchSummary = "PostingParentMerchantBalancerError";
            batchStatus.BatchStatus = BatchStatus.Error;
            batchStatus.UpdateDate = DateTime.Now;
            batchStatus.IsCriticalError = true;

            await _postingBatchStatusRepository.UpdateAsync(batchStatus);
        }
    }
}