using LinkPara.HttpProviders.MoneyTransfer;
using LinkPara.PF.Domain.Enums;
using LinkPara.PF.Infrastructure.Persistence;
using LinkPara.SharedModels.BusModels.Commands.Scheduler;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LinkPara.PF.Infrastructure.Consumers.CronJobs;

public class PostingMoneyTransferMonitorConsumer : IConsumer<PostingMoneyTransferMonitor>
{
    private readonly ILogger<PostingMoneyTransferMonitorConsumer> _logger;
    private readonly PfDbContext _dbContext;
    private readonly IMoneyTransferService _moneyTransferService;

    public PostingMoneyTransferMonitorConsumer(
        ILogger<PostingMoneyTransferMonitorConsumer> logger, 
        PfDbContext dbContext, 
        IMoneyTransferService moneyTransferService)
    {
        _logger = logger;
        _dbContext = dbContext;
        _moneyTransferService = moneyTransferService;
    }

    public async Task Consume(ConsumeContext<PostingMoneyTransferMonitor> context)
    {
        try
        {
            await ManageStuckBalancesAsync();
            await ManageStuckDeductionsAsync();
        }
        catch (Exception exception)
        {
            _logger.LogError($"PostingMoneyTransferMonitorJobError: {exception}");
        }
    }

    private async Task ManageStuckBalancesAsync()
    {
        var balances = await _dbContext.PostingBalance
            .Where(s => 
                s.BatchStatus == BatchStatus.MoneyTransferProcessing &&
                s.ProcessingStartedAt.AddHours(1) <= DateTime.Now)
            .GroupBy(s => s.TransactionSourceId)
            .ToListAsync();
        if (balances.Count == 0)
        {
            return;
        }

        foreach (var group in balances)
        {
            try
            {
                var groupedBalances = group.ToList();
                var checkTransactionExists = await _moneyTransferService.TransactionExistsAsync(group.Key);
                if (checkTransactionExists.IsExists)
                {
                    continue;
                }
                //balances stuck at processing but there is no record on moneyTransfer service
                groupedBalances.ForEach(s =>
                {
                    s.BatchStatus = BatchStatus.MoneyTransferError;
                    s.ProcessingId = null;
                    s.ProcessingStartedAt = DateTime.MinValue;
                    s.TransactionSourceId = Guid.Empty;
                    s.MoneyTransferStatus = PostingMoneyTransferStatus.PaymentError;
                });
                _dbContext.PostingBalance.UpdateRange(groupedBalances);
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception exception)
            {
                _logger.LogError($"PostingMoneyTransferMonitorJobError on Merchant Group: {exception}");
            }
        }
    }
    
    private async Task ManageStuckDeductionsAsync()
    {
        var deductions = await _dbContext.MerchantDeduction
            .Where(s => 
                s.DeductionStatus == DeductionStatus.Processing &&
                s.ProcessingStartedAt.AddHours(1) <= DateTime.Now)
            .ToListAsync();
        if (deductions.Count == 0)
        {
            return;
        }
        
        deductions.ForEach(s =>
        {
            s.DeductionStatus = DeductionStatus.Pending;
            s.ProcessingId = null;
            s.ProcessingStartedAt = DateTime.MinValue;
        });
        
        _dbContext.MerchantDeduction.UpdateRange(deductions);
        await _dbContext.SaveChangesAsync();
    }
}