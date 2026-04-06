using LinkPara.HttpProviders.Vault;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.BusModels.Commands.Scheduler;
using LinkPara.SharedModels.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Globalization;
using LinkPara.SharedModels.Notification.NotificationModels.PF;

namespace LinkPara.PF.Infrastructure.Consumers.CronJobs;

public class SendPostingSummaryMailConsumer : IConsumer<PostingDailyControl>
{
    private readonly ILogger<SendPostingSummaryMailConsumer> _logger;
    private readonly IGenericRepository<PostingBalance> _postingBalanceRepository;
    private readonly IGenericRepository<PostingTransaction> _postingTransactionRepository;
    private readonly IGenericRepository<PostingBatchStatus> _postingBatchStatusRepository;
    private readonly IGenericRepository<MerchantTransaction> _merchantTransactionRepository;
    private readonly IBus _bus;

    public SendPostingSummaryMailConsumer(ILogger<SendPostingSummaryMailConsumer> logger,
        IGenericRepository<PostingBatchStatus> postingBatchStatusRepository,
        IBus bus,
        IGenericRepository<PostingBalance> postingBalanceRepository,
        IGenericRepository<MerchantTransaction> merchantTransactionRepository,
        IGenericRepository<PostingTransaction> postingTransactionRepository)
    {
        _logger = logger;
        _postingBatchStatusRepository = postingBatchStatusRepository;
        _bus = bus;
        _postingBalanceRepository = postingBalanceRepository;
        _merchantTransactionRepository = merchantTransactionRepository;
        _postingTransactionRepository = postingTransactionRepository;
    }

    public async Task Consume(ConsumeContext<PostingDailyControl> context)
    {
        try
        {
            DateTime startTime = DateTime.Now;
            DateTime finishTime = DateTime.Now;
            double totalMinute = 0;
            var batchStatus = await _postingBatchStatusRepository.GetAll().Where(b => b.PostingDate == DateTime.Now.Date).OrderBy(c => c.StartTime).ToListAsync();
            if (batchStatus.Any())
            {
                startTime = batchStatus.FirstOrDefault()?.StartTime ?? DateTime.MinValue;
                finishTime = batchStatus.LastOrDefault()?.FinishTime ?? DateTime.MinValue;
                DateTime start = new(1, 1, 1, startTime.Hour, startTime.Minute, 0);
                DateTime finish = new(1, 1, 1, finishTime.Hour, finishTime.Minute, 0);
                TimeSpan timeDifference = finish - start;
                totalMinute = timeDifference.TotalMinutes;
            }

            var postingBalances = await _postingBalanceRepository
                .GetAll()
                .Where(b => b.PostingDate == DateTime.Now.Date)
                .ToListAsync();

            if (postingBalances.Any())
            {
                var paymentBalances = postingBalances.Where(b => b.PostingBalanceType == PostingBalanceType.Payment);

                var totalTransactionCount = paymentBalances.Sum(c => c.TransactionCount);
                var failCount = await _merchantTransactionRepository.GetAll().Include(m => m.Merchant).Where(t => (((t.BatchStatus == BatchStatus.Pending) && ((t.TransactionType == TransactionType.Return && t.TransactionStatus == TransactionStatus.Success) || ((t.TransactionType == TransactionType.Auth || t.TransactionType == TransactionType.PostAuth) && (t.TransactionStatus == TransactionStatus.Success || t.TransactionStatus == TransactionStatus.PartiallyReturned || t.TransactionStatus == TransactionStatus.Returned))) && t.TransactionDate < DateTime.Now.Date) || t.BatchStatus == BatchStatus.Error) && !t.IsChargeback && !t.IsSuspecious && t.RecordStatus == RecordStatus.Active && t.Merchant.PaymentAllowed).CountAsync() + await _postingTransactionRepository.GetAll().Where(b => b.BatchStatus != BatchStatus.Completed && b.PostingDate == DateTime.Now.Date).CountAsync();
                var totalTransactionAmount = paymentBalances.Sum(c => c.TotalAmount);
                var totalCommissionAmount = paymentBalances.Sum(c => c.TotalPfCommissionAmount);
                var totalBankCommissionAmount = paymentBalances.Sum(c => c.TotalBankCommissionAmount);
                var totalPfNetCommissionAmount = paymentBalances.Sum(c => c.TotalPfNetCommissionAmount);
                var totalPayingAmount = paymentBalances.Sum(c => c.TotalPayingAmount);
                var totalDueAmount = paymentBalances.Sum(c => c.TotalDueAmount);
                var totalExcessAmount = paymentBalances.Sum(c => c.TotalExcessReturnAmount);
                var totalChargebackAmount = paymentBalances.Sum(c => c.TotalChargebackAmount);
                var totalSuspiciousAmount = paymentBalances.Sum(c => c.TotalSuspiciousAmount);

                var totalChargebackCancelAmount = postingBalances
                    .Where(b => b.PostingBalanceType == PostingBalanceType.ChargebackReturn)
                    .Sum(c => c.TotalPayingAmount);

                var totalSuspiciousCancelAmount = postingBalances
                    .Where(b => b.PostingBalanceType == PostingBalanceType.SuspiciousReturn)
                    .Sum(c => c.TotalPayingAmount);

                var totalFutureAmount = postingBalances
                    .Where(b => b.PaymentDate > DateTime.Now.Date)
                    .Sum(c => c.TotalPayingAmount);

                var totalAmount = totalDueAmount + totalExcessAmount + totalChargebackAmount + totalSuspiciousAmount - totalChargebackCancelAmount - totalSuspiciousCancelAmount;

                await _bus.Publish(new PostingSummary
                {
                    Date = DateTime.Now.ToString("dd/MM/yyyy"),
                    StartHour = startTime.ToShortTimeString(),
                    FinishHour = finishTime.ToShortTimeString(),
                    Duration = totalMinute.ToString(),
                    TotalCount = totalTransactionCount.ToString(),
                    TotalFailCount = failCount.ToString(),
                    TotalTransactionAmount = FormatAmount(totalTransactionAmount),
                    TotalCommissionAmount = FormatAmount(totalCommissionAmount),
                    TotalBankCommissionAmount = FormatAmount(totalBankCommissionAmount),
                    TotalPfNetCommissionAmount = FormatAmount(totalPfNetCommissionAmount),
                    TotalPayingAmount = FormatAmount(totalPayingAmount),
                    TotalFuturePayingAmount = FormatAmount(totalFutureAmount),
                    TotalDueAmount = FormatAmount(totalDueAmount),
                    TotalExcessReturnAmount = FormatAmount(totalExcessAmount),
                    TotalChargebackAmount = FormatAmount(totalChargebackAmount),
                    TotalSuspiciousAmount = FormatAmount(totalSuspiciousAmount),
                    TotalChargebackCancelAmount = FormatAmount(totalChargebackCancelAmount),
                    TotalSuspiciousCancelAmount = FormatAmount(totalSuspiciousCancelAmount),
                    TotalAmount = FormatAmount(totalAmount)
                });
            }
        }
        catch (Exception exception)
        {
            _logger.LogError($"SendPostingSummaryMailConsumerError. Exception: {exception}");
        }
    }
    private string FormatAmount(decimal amount)
    {
        return decimal.Round(amount, 2, MidpointRounding.AwayFromZero)
        .ToString(new CultureInfo("en-US"));
    }
}
