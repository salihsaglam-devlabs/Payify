using System.Transactions;
using LinkPara.PF.Application.Commons.Helpers;
using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Commons.Models.Posting;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;
using LinkPara.PF.Infrastructure.Persistence;
using LinkPara.SystemUser;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LinkPara.PF.Infrastructure.Posting;

public class PostingDeductionCalculationBatch : IPostingBatch<PostingDeductionCalculator>
{
    private readonly ILogger<PostingDeductionCalculationBatch> _logger;
    private readonly PfDbContext _pfDbContext;
    private readonly IApplicationUserService _applicationUserService;
    private readonly IPureSqlStore _pureSqlStore;

    public PostingDeductionCalculationBatch(
        ILogger<PostingDeductionCalculationBatch> logger, 
        PfDbContext pfDbContext, 
        IApplicationUserService applicationUserService, 
        IPureSqlStore pureSqlStore)
    {
        _logger = logger;
        _pfDbContext = pfDbContext;
        _applicationUserService = applicationUserService;
        _pureSqlStore = pureSqlStore;
    }

    public async Task ExecuteBatchAsync(PostingBatchStatus batchStatus)
    {
        try
        {
            var balances = await _pfDbContext
                .PostingBalance
                .Where(w =>
                    w.TotalPayingAmount > 0 &&
                    w.PaymentDate == DateTime.Today &&
                    w.MoneyTransferStatus == PostingMoneyTransferStatus.Pending
                )
                .OrderByDescending(w => w.TotalPayingAmount)
                .ThenBy(w => w.BlockageStatus)
                .ToListAsync();

            var distinctMerchants = balances
                .DistinctBy(d => d.MerchantId)
                .Select(s => s.MerchantId)
                .ToList();

            var deductionReservationId = await _pureSqlStore.ReserveMerchantDeductionsAsync(distinctMerchants);
            if (deductionReservationId is null)
            {
                batchStatus.BatchStatus = BatchStatus.Completed;
                batchStatus.IsCriticalError = false;
                batchStatus.BatchSummary = "PostingDeductionCalculationBatchJobCompletedSuccessfully";
                batchStatus.UpdateDate = DateTime.Now;
                _pfDbContext.PostingBatchStatus.Update(batchStatus);
                await _pfDbContext.SaveChangesAsync();
                return;
            }
            
            var merchantDeductions = await _pfDbContext
                .MerchantDeduction
                .Where(s =>
                    s.ProcessingId == deductionReservationId && 
                    s.DeductionStatus == DeductionStatus.Processing
                )
                .OrderBy(o => o.CreateDate)
                .ToListAsync();

            var merchantTransactionIds = merchantDeductions
                .Where(s => s.MerchantTransactionId != Guid.Empty)
                .Select(s => s.MerchantTransactionId)
                .Distinct().ToList();

            var deductionPostingTransactions = await _pfDbContext.PostingTransaction
                .Where(s => merchantTransactionIds.Contains(s.MerchantTransactionId)).ToListAsync();

            var strategy = _pfDbContext.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
                
                var result = DeductionHelper.Calculate(balances,merchantDeductions,deductionPostingTransactions,_applicationUserService.ApplicationUserId);
                _pfDbContext.DeductionTransaction.AddRange(result.DeductionTransactions);
                _pfDbContext.PostingAdditionalTransaction.AddRange(result.PostingAdditionalTransactions);
                _pfDbContext.MerchantDeduction.UpdateRange(result.MerchantDeductions);
                _pfDbContext.PostingBalance.UpdateRange(result.PostingBalances);
                
                batchStatus.BatchStatus = BatchStatus.Completed;
                batchStatus.IsCriticalError = false;
                batchStatus.BatchSummary = "PostingDeductionCalculationBatchJobCompletedSuccessfully";
                batchStatus.UpdateDate = DateTime.Now;
                _pfDbContext.PostingBatchStatus.Update(batchStatus);
                
                await _pfDbContext.SaveChangesAsync();
                scope.Complete();
            });
        }
        catch (Exception exception)
        {
            _logger.LogError($"PostingDeductionCalculationBatchJobError: {exception}");

            batchStatus.BatchStatus = BatchStatus.Error;
            batchStatus.IsCriticalError = true;
            batchStatus.BatchSummary = "PostingDeductionCalculationBatchJobError";
            batchStatus.UpdateDate = DateTime.Now;
            _pfDbContext.PostingBatchStatus.Update(batchStatus);
            await _pfDbContext.SaveChangesAsync();
        }
    }
}