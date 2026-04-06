using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Commons.Models.Posting;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;
using LinkPara.SystemUser;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MassTransit;
using LinkPara.PF.Infrastructure.Persistence;
using System.Transactions;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.PF.Infrastructure.Posting;

public class PostingGrandBalancerBatch : IPostingBatch<PostingGrandBalancer>
{
    private readonly ILogger<PostingGrandBalancerBatch> _logger;
    private readonly IGenericRepository<PostingBatchStatus> _postingBatchStatusRepository;
    private readonly IApplicationUserService _applicationUserService;
    private readonly PfDbContext _dbContext;

    public PostingGrandBalancerBatch(
        IGenericRepository<PostingBatchStatus> postingBatchStatusRepository,
        ILogger<PostingGrandBalancerBatch> logger,
        IApplicationUserService applicationUserService,
        PfDbContext dbContext)
    {
        _logger = logger;
        _applicationUserService = applicationUserService;
        _dbContext = dbContext;
        _postingBatchStatusRepository = postingBatchStatusRepository;
    }
    public async Task ExecuteBatchAsync(PostingBatchStatus batchStatus)
    {
        try
        {
            var bankBalances = await _dbContext
                .PostingBankBalance
                .Where(w => 
                    (w.PostingDate == DateTime.Now.Date && w.BatchStatus == BatchStatus.Pending)
                    ||
                    w.BatchStatus == BatchStatus.Error
                 )
                .ToListAsync();
            
            var merchantIds = bankBalances.Select(s => s.MerchantId).Distinct().ToList();
            var merchants = await _dbContext.Merchant.Where(s =>  merchantIds.Contains(s.Id)).ToListAsync();
            
            var balances = bankBalances
                .GroupBy(g => new { g.MerchantId, g.PaymentDate, g.PostingDate, g.Currency, g.BlockageStatus, g.TransactionDate, g.ParentMerchantId })
                .Select(g => new PostingBalance
                {
                    MerchantId = g.Key.MerchantId,
                    PostingDate = g.Key.PostingDate,
                    PaymentDate = g.Key.PaymentDate,
                    OldPaymentDate = g.FirstOrDefault()!.OldPaymentDate,
                    Currency = g.Key.Currency,
                    TransactionDate = g.Key.TransactionDate,
                    TotalAmount = g.Sum(s => s.TotalAmount),
                    TotalPointAmount = g.Sum(s => s.TotalPointAmount),
                    TotalBankCommissionAmount = g.Sum(s => s.TotalBankCommissionAmount),
                    TotalAmountWithoutBankCommission = g.Sum(s => s.TotalAmountWithoutBankCommission),
                    TotalPfCommissionAmount = g.Sum(s => s.TotalPfCommissionAmount),
                    TotalPfNetCommissionAmount = g.Sum(s => s.TotalPfNetCommissionAmount),
                    TotalAmountWithoutCommissions = g.Sum(s => s.TotalAmountWithoutCommissions),
                    TotalParentMerchantCommissionAmount = g.Sum(s => s.TotalParentMerchantCommissionAmount),
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
                    TotalNegativeBalanceAmount = 0,
                    TotalPayingAmount = g.Sum(s => s.TotalPayingAmount),
                    MoneyTransferStatus = merchants.FirstOrDefault(s => s.Id == g.Key.MerchantId)!.PaymentAllowed ? PostingMoneyTransferStatus.Pending : PostingMoneyTransferStatus.PaymentBlocked,
                    RetryCount = 0,
                    TransactionCount = g.Sum(s => s.TransactionCount),
                    BatchStatus = BatchStatus.Pending,
                    BlockageStatus = g.Key.BlockageStatus,
                    CreatedBy = _applicationUserService.ApplicationUserId.ToString(),
                    AccountingStatus = PostingAccountingStatus.PendingDeductionBalance,
                    PostingBalanceType = PostingBalanceType.Payment,
                    PostingPaymentChannel = PostingPaymentChannel.Unknown,
                    BTransStatus = PostingBTransStatus.Pending,
                    ParentMerchantId = g.Key.ParentMerchantId
                })
                .ToList();

            var excessReturnDeductions = new List<MerchantDeduction>();

            foreach (var excessReturnBalance in balances.Where(s => s.TotalPayingAmount <= 0))
            {
                if (excessReturnBalance.TotalPayingAmount < 0)
                {
                    var excessReturnAmount = -1 * excessReturnBalance.TotalPayingAmount;
                    excessReturnDeductions.Add(
                        new MerchantDeduction
                        {
                            TotalDeductionAmount = excessReturnAmount,
                            RemainingDeductionAmount = excessReturnAmount,
                            Currency = excessReturnBalance.Currency,
                            DeductionType = DeductionType.ExcessReturn,
                            DeductionStatus = DeductionStatus.Pending,
                            ExecutionDate = DateTime.Now,
                            MerchantId = excessReturnBalance.MerchantId,
                            MerchantTransactionId = Guid.Empty,
                            MerchantDueId = Guid.Empty,
                            RecordStatus = RecordStatus.Active,
                            CreatedBy = _applicationUserService.ApplicationUserId.ToString(),
                            PostingBalanceId = excessReturnBalance.Id,
                            ConversationId = string.Empty,
                            DeductionAmountWithCommission = excessReturnAmount,
                            SubMerchantDeductionId = Guid.Empty
                        });
                    excessReturnBalance.TotalNegativeBalanceAmount = excessReturnAmount;
                    excessReturnBalance.TotalPayingAmount = 0;
                }
                excessReturnBalance.MoneyTransferStatus = PostingMoneyTransferStatus.PaymentNotRequired;
                excessReturnBalance.BatchStatus = BatchStatus.Completed;
            }

            var strategy = _dbContext.Database.CreateExecutionStrategy();

            await strategy.ExecuteAsync(async () =>
            {
                using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

                _dbContext.PostingBankBalance.UpdateRange(bankBalances);
                _dbContext.PostingBatchStatus.Attach(batchStatus);

                await _dbContext.PostingBalance.AddRangeAsync(balances);
                await _dbContext.MerchantDeduction.AddRangeAsync(excessReturnDeductions);

                bankBalances.ForEach(bankBalance =>
                {
                    bankBalance.BatchStatus = BatchStatus.Completed;
                    bankBalance.PostingBalanceId = balances
                        .FirstOrDefault(b =>
                            b.TransactionDate == bankBalance.TransactionDate
                            && b.MerchantId == bankBalance.MerchantId
                            && b.PaymentDate == bankBalance.PaymentDate
                            && b.Currency == bankBalance.Currency
                            && b.BlockageStatus == bankBalance.BlockageStatus
                            && b.PostingDate == bankBalance.PostingDate
                            && b.ParentMerchantId == bankBalance.ParentMerchantId
                        )?
                        .Id;
                });

                batchStatus.BatchStatus = BatchStatus.Completed;
                batchStatus.IsCriticalError = false;
                batchStatus.BatchSummary = "PostingGrandBalancerJobFinishedSuccessfully";
                batchStatus.UpdateDate = DateTime.Now;

                await _dbContext.SaveChangesAsync();

                scope.Complete();
            });
        }
        catch (Exception exception)
        {
            _logger.LogCritical($"PostingGrandBalancerJobConsumeError: {exception}");

            batchStatus.BatchStatus = BatchStatus.Error;
            batchStatus.IsCriticalError = true;
            batchStatus.BatchSummary = "PostingGrandBalancerJobConsumeError";
            batchStatus.UpdateDate = DateTime.Now;

            await _postingBatchStatusRepository.UpdateAsync(batchStatus);
        }
    }
}