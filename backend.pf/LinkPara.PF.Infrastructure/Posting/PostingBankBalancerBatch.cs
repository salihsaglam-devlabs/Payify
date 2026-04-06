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

public class PostingBankBalancerBatch : IPostingBatch<PostingBankBalancer>
{
    private readonly ILogger<PostingBankBalancerBatch> _logger;
    private readonly PfDbContext _dbContext;
    private readonly IApplicationUserService _applicationUserService;
    private readonly IGenericRepository<PostingBatchStatus> _postingBatchStatusRepository;

    public PostingBankBalancerBatch(
        IGenericRepository<PostingBatchStatus> postingBatchStatusRepository,
        ILogger<PostingBankBalancerBatch> logger,
        IApplicationUserService applicationUserService,
        PfDbContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
        _applicationUserService = applicationUserService;
        _postingBatchStatusRepository = postingBatchStatusRepository;
    }

    public async Task ExecuteBatchAsync(PostingBatchStatus batchStatus)
    {
        try
        {
            var bankBalances = await _dbContext
                .PostingTransaction
                .Where(t => 
                    (
                        (t.BatchStatus == BatchStatus.Pending && t.PostingDate == DateTime.Now.Date)
                        ||
                        t.BatchStatus == BatchStatus.Error
                    ) &&
                    t.RecordStatus == RecordStatus.Active
                 )
                .GroupBy(t => new { t.MerchantId, t.AcquireBankCode, t.TransactionDate, t.PaymentDate, t.Currency, t.BlockageStatus, t.ParentMerchantId })
                .Select(g => new PostingBankBalance
                {
                    MerchantId = g.Key.MerchantId,
                    AcquireBankCode = g.Key.AcquireBankCode,
                    PaymentDate = g.Key.PaymentDate,
                    OldPaymentDate = g.FirstOrDefault()!.OldPaymentDate,
                    PostingDate = DateTime.Now.Date,
                    TransactionDate = g.Key.TransactionDate,
                    Currency = g.Key.Currency,
                    ParentMerchantId = g.Key.ParentMerchantId,
                    TotalAmount = g.Where(w => w.TransactionType == TransactionType.Auth).Sum(s => s.Amount) +
                        g.Where(w => w.TransactionType == TransactionType.PostAuth).Sum(s => s.Amount) -
                        g.Where(w => w.TransactionType == TransactionType.Return).Sum(s => s.Amount),
                    TotalPointAmount = g.Where(w => w.TransactionType == TransactionType.Auth).Sum(s => s.PointAmount) +
                        g.Where(w => w.TransactionType == TransactionType.PostAuth).Sum(s => s.PointAmount) -
                        g.Where(w => w.TransactionType == TransactionType.Return).Sum(s => s.PointAmount),
                    TotalBankCommissionAmount = g.Where(w => w.TransactionType == TransactionType.Auth).Sum(s => s.BankCommissionAmount) +
                        g.Where(w => w.TransactionType == TransactionType.PostAuth).Sum(s => s.BankCommissionAmount) -
                        g.Where(w => w.TransactionType == TransactionType.Return).Sum(s => s.BankCommissionAmount),
                    TotalAmountWithoutBankCommission = g.Where(w => w.TransactionType == TransactionType.Auth).Sum(s => s.AmountWithoutBankCommission) +
                        g.Where(w => w.TransactionType == TransactionType.PostAuth).Sum(s => s.AmountWithoutBankCommission) -
                        g.Where(w => w.TransactionType == TransactionType.Return).Sum(s => s.AmountWithoutBankCommission),
                    TotalPfCommissionAmount = g.Where(w => w.TransactionType == TransactionType.Auth).Sum(s => s.PfCommissionAmount) +
                        g.Where(w => w.TransactionType == TransactionType.PostAuth).Sum(s => s.PfCommissionAmount) -
                        g.Where(w => w.TransactionType == TransactionType.Return).Sum(s => s.PfCommissionAmount),
                    TotalPfNetCommissionAmount = g.Where(w => w.TransactionType == TransactionType.Auth).Sum(s => s.PfNetCommissionAmount) +
                        g.Where(w => w.TransactionType == TransactionType.PostAuth).Sum(s => s.PfNetCommissionAmount) -
                        g.Where(w => w.TransactionType == TransactionType.Return).Sum(s => s.PfNetCommissionAmount),
                    TotalAmountWithoutCommissions = g.Where(w => w.TransactionType == TransactionType.Auth).Sum(s => s.AmountWithoutCommissions) +
                        g.Where(w => w.TransactionType == TransactionType.PostAuth).Sum(s => s.AmountWithoutCommissions) -
                        g.Where(w => w.TransactionType == TransactionType.Return).Sum(s => s.AmountWithoutCommissions),
                    TotalParentMerchantCommissionAmount = g.Where(w => w.TransactionType == TransactionType.Auth).Sum(s => s.ParentMerchantCommissionAmount) +
                        g.Where(w => w.TransactionType == TransactionType.PostAuth).Sum(s => s.ParentMerchantCommissionAmount) -
                        g.Where(w => w.TransactionType == TransactionType.Return).Sum(s => s.ParentMerchantCommissionAmount),
                    TotalReturnAmount = g.Where(w => w.TransactionType == TransactionType.Return).Sum(s => s.Amount),
                    TransactionCount = g.Count(),
                    BlockageStatus = g.Key.BlockageStatus,
                    BatchStatus = BatchStatus.Pending,
                    AccountingStatus = PostingAccountingStatus.PendingPosBlockage,
                    CreatedBy = _applicationUserService.ApplicationUserId.ToString()
                })
                .ToListAsync();

            bankBalances.ForEach(b => b.TotalPayingAmount = b.TotalAmount - b.TotalPfCommissionAmount - b.TotalParentMerchantCommissionAmount);

            var strategy = _dbContext.Database.CreateExecutionStrategy();

            await strategy.ExecuteAsync(async () =>
            {
                using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

                _dbContext.PostingBatchStatus.Attach(batchStatus);
                await _dbContext.AddRangeAsync(bankBalances);

                batchStatus.BatchSummary = "PostingBalancerBatchJobCompletedSuccessfully";
                batchStatus.BatchStatus = BatchStatus.Completed;
                batchStatus.UpdateDate = DateTime.Now;
                batchStatus.IsCriticalError = false;

                await _dbContext.SaveChangesAsync();

                scope.Complete();
            });
        }
        catch (Exception exception)
        {
            _logger.LogError($"PostingBalanceTransaction Error, {exception}");

            batchStatus.BatchSummary = "PostingBalanceTransaction";
            batchStatus.BatchStatus = BatchStatus.Error;
            batchStatus.UpdateDate = DateTime.Now;
            batchStatus.IsCriticalError = true;

            await _postingBatchStatusRepository.UpdateAsync(batchStatus);
        }
    }
}