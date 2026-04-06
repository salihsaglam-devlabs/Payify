using LinkPara.HttpProviders.BusinessParameter;
using LinkPara.PF.Application.Commons.Models.BTrans;
using LinkPara.PF.Domain.Enums;
using LinkPara.PF.Infrastructure.Persistence;
using LinkPara.SharedModels.BusModels.Commands.Scheduler;
using LinkPara.SharedModels.Notification.NotificationModels.PF;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LinkPara.PF.Infrastructure.Consumers.CronJobs;

public class SendPosInformationConsumer : IConsumer<SendPosInformation>
{
    private readonly ILogger<SendPosInformationConsumer> _logger;
    private readonly IBus _bus;
    private readonly PfDbContext _dbContext;
    private readonly IParameterService _parameterService;
    private int _transactionPerBatch;
    private const int TransactionPerBatch = 1000;

    public SendPosInformationConsumer(
        ILogger<SendPosInformationConsumer> logger,
        IBus bus,
        PfDbContext dbContext,
        IParameterService parameterService
    )
    {
        _logger = logger;
        _bus = bus;
        _dbContext = dbContext;
        _parameterService = parameterService;
    }

    public async Task Consume(ConsumeContext<SendPosInformation> context)
    {
        var errorMessage = $"ErrorGettingPostingBTransParamsContinueWithDefaultValues!";
        try
        {
            var postingParameters = await _parameterService.GetParametersAsync("PostingParams");
            var parse = int.TryParse(postingParameters
                    .FirstOrDefault(p => p.ParameterCode == "BTransTransactionPerBatch")
                    ?.ParameterValue,
                out _transactionPerBatch);

            if (!parse)
            {
                _logger.LogError(errorMessage);
                _transactionPerBatch = TransactionPerBatch;
            }
        }
        catch (Exception)
        {
            _transactionPerBatch = TransactionPerBatch;
            _logger.LogError(errorMessage);
        }

        try
        {
            var postingBalanceIdDeductionPairs = await
                _dbContext.PostingBalance
                    .Where(b =>
                        b.PostingBalanceType == PostingBalanceType.Payment &&
                        b.BTransStatus == PostingBTransStatus.Pending
                    )
                    .OrderBy(t => t.CreateDate)
                    .Select(s => new { Id = s.Id, TotalDueAmount = s.TotalDueAmount })
                    .ToListAsync();

            var postingBalancesWithDueAmount = postingBalanceIdDeductionPairs.Where(s => s.TotalDueAmount > 0).ToList();

            var processingBalanceIdSet = new HashSet<Guid>(postingBalanceIdDeductionPairs.Select(s => s.Id).ToList());

            var activeTransactionBalanceIds = await _dbContext.PostingTransaction
                .Where(t => t.BTransStatus != PostingBTransStatus.Completed &&
                            processingBalanceIdSet.Contains(t.PostingBalanceId))
                .Select(t => t.PostingBalanceId)
                .Distinct()
                .ToListAsync();

            var activeBalanceIdSet = new HashSet<Guid>(activeTransactionBalanceIds);
            var balancesToCompleteIdSet = new HashSet<Guid>(processingBalanceIdSet.Except(activeBalanceIdSet));

            var now = DateTime.Now;
            await _dbContext.PostingBalance
                .Where(b => balancesToCompleteIdSet.Contains(b.Id))
                .ExecuteUpdateAsync(s => s
                    .SetProperty(b => b.BTransStatus, _ => PostingBTransStatus.Completed)
                    .SetProperty(b => b.UpdateDate, _ => now));

            var excludedPostingTransactionIds = new HashSet<Guid>();

            //Raporlanacak iade kayıtlarının ana satış merchantTransactionId listesi
            var returnInfoList = await (
                    from returnPt in _dbContext.PostingTransaction
                    where activeBalanceIdSet.Contains(returnPt.PostingBalanceId)
                          && returnPt.BTransStatus != PostingBTransStatus.Completed
                          && returnPt.TransactionType == TransactionType.Return
                    join returnMt in _dbContext.MerchantTransaction
                        on returnPt.MerchantTransactionId equals returnMt.Id
                    where !string.IsNullOrEmpty(returnMt.ReturnedTransactionId)
                    select returnMt.ReturnedTransactionId
                )
                .Distinct()
                .ToListAsync();

            var masterMerchantTransactionIds = new HashSet<Guid>(returnInfoList.Select(Guid.Parse));

            //Raporlanacak iade kayıtlarının ana satış kaydına bağlı tüm postingTransaction kayıtları
            var masterPtLookup = await _dbContext.PostingTransaction
                .Where(s => masterMerchantTransactionIds.Contains(s.MerchantTransactionId)
                            && s.BTransStatus == PostingBTransStatus.Completed)
                .Select(s => new
                {
                    s.MerchantTransactionId,
                    s.InstallmentSequence,
                    PtId = s.Id
                })
                .ToDictionaryAsync(
                    x => (x.MerchantTransactionId, x.InstallmentSequence),
                    x => x.PtId);

            var masterMtIdStrSet = masterPtLookup.Keys
                .Select(k => k.MerchantTransactionId.ToString())
                .Distinct()
                .ToList();

            //ana satış kaydına bağlı tüm iade merchantTransactionlarının postingTransaction listesi
            var returnTransactionList = await (
                from returnMt in _dbContext.MerchantTransaction
                where masterMtIdStrSet.Contains(returnMt.ReturnedTransactionId)
                      && returnMt.TransactionStatus == TransactionStatus.Success
                join returnPt in _dbContext.PostingTransaction
                    on returnMt.Id equals returnPt.MerchantTransactionId
                select new
                {
                    ReturnPtId = returnPt.Id,
                    MasterMtIdStr = returnMt.ReturnedTransactionId,
                    returnPt.InstallmentSequence
                }
            ).ToListAsync();

            var returnBTransReports = returnTransactionList
                .Select(x => new
                {
                    MasterMtId = Guid.TryParse(x.MasterMtIdStr, out var guid) ? (Guid?)guid : null,
                    x.ReturnPtId,
                    x.InstallmentSequence
                })
                .Where(x => x.MasterMtId.HasValue)
                .GroupBy(x => (x.MasterMtId.Value, x.InstallmentSequence))
                .Where(g => masterPtLookup.ContainsKey(g.Key))
                .Select(g => new ReturnBTransReport
                {
                    ReferencePostingTransactionId = masterPtLookup[g.Key],
                    ReturnPostingTransactionIds = g.Select(y => y.ReturnPtId).Distinct().ToList()
                })
                .ToList();

            excludedPostingTransactionIds.UnionWith(
                returnBTransReports.Select(s => s.ReferencePostingTransactionId));

            var returnBTransReportEndpoint = await _bus.GetSendEndpoint(
                new Uri("exchange:PF.ReturnBTransReport"));

            foreach (var returnBTransReport in returnBTransReports)
            {
                using var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                await returnBTransReportEndpoint.Send(returnBTransReport, tokenSource.Token);
            }

            var authPostAuthBTransReportEndpoint =
                await _bus.GetSendEndpoint(new Uri("exchange:PF.AuthPostAuthBTransReport"));

            foreach (var postingBalanceWithDueAmount in postingBalancesWithDueAmount)
            {
                var totalDueAmount = postingBalanceWithDueAmount.TotalDueAmount;
                decimal deductedDueAmount = 0;
                var deductionTransactionIds = new List<Guid>();
                var page = 0;

                while (true)
                {
                    var balanceTransactions = await _dbContext.PostingTransaction
                        .Where(t => t.PostingBalanceId == postingBalanceWithDueAmount.Id &&
                                    !excludedPostingTransactionIds.Contains(t.Id))
                        .OrderByDescending(t => t.AmountWithoutCommissions)
                        .Select(s => new { Id = s.Id, Amount = s.AmountWithoutCommissions })
                        .Skip(page * 1000)
                        .Take(1000)
                        .ToListAsync();

                    if (balanceTransactions.Count == 0)
                        break;

                    foreach (var balanceTransaction in balanceTransactions)
                    {
                        deductionTransactionIds.Add(balanceTransaction.Id);
                        deductedDueAmount += balanceTransaction.Amount;
                        excludedPostingTransactionIds.Add(balanceTransaction.Id);
                        if (deductedDueAmount >= totalDueAmount) break;
                    }

                    page++;

                    if (deductedDueAmount >= totalDueAmount) break;
                }

                if (deductionTransactionIds.Count > 0)
                {
                    await authPostAuthBTransReportEndpoint.Send(
                        new AuthPostAuthBTransReport
                            { PostingTransactionIds = deductionTransactionIds, DeductionAmount = totalDueAmount },
                        CancellationToken.None);
                }
            }

            var batch = new List<Guid>(capacity: _transactionPerBatch);

            await foreach (var id in _dbContext.PostingTransaction
                               .AsNoTracking()
                               .Where(t =>
                                   activeBalanceIdSet.Contains(t.PostingBalanceId) &&
                                   !excludedPostingTransactionIds.Contains(t.Id) &&
                                   t.TransactionType != TransactionType.Return &&
                                   t.TransactionType != TransactionType.Chargeback &&
                                   t.TransactionType != TransactionType.Suspicious &&
                                   t.BTransStatus != PostingBTransStatus.Completed)
                               .OrderBy(t => t.CreateDate)
                               .Select(t => t.Id)
                               .AsAsyncEnumerable())
            {
                batch.Add(id);

                if (batch.Count >= _transactionPerBatch)
                {
                    using var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                    await authPostAuthBTransReportEndpoint.Send(
                        new AuthPostAuthBTransReport { PostingTransactionIds = batch.ToList(), DeductionAmount = 0 },
                        cancellationToken.Token);
                    batch.Clear();
                }
            }

            if (batch.Count > 0)
            {
                using var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                await authPostAuthBTransReportEndpoint.Send(
                    new AuthPostAuthBTransReport { PostingTransactionIds = batch, DeductionAmount = 0 },
                    cancellationToken.Token);
            }
        }
        catch (Exception exception)
        {
            _logger.LogError($"BTransReportError. Exception: {exception}");
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            await _bus.Publish(new SendBTransFailed
            {
                Environment = environment,
                Exception = "Merhaba,\r\n BTrans veri aktarımı sırasında sistemsel bir hata oluştu. Kontrol edilmesi gerekmektedir."
            });
        }
    }
}