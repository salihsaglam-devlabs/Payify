using LinkPara.HttpProviders.BusinessParameter;
using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.BusModels.Commands.Scheduler;
using LinkPara.SharedModels.BusModels.IntegrationEvents.Accounting;
using LinkPara.SharedModels.BusModels.IntegrationEvents.Accounting.Enums;
using LinkPara.SharedModels.Persistence;
using LinkPara.SystemUser;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LinkPara.PF.Infrastructure.Consumers.CronJobs;

public class PostingBillingConsumer : IConsumer<PostingBillingJob>
{
    private readonly IGenericRepository<PostingBill> _postingBillRepository;
    private readonly IGenericRepository<PostingBalance> _postingBalanceRepository;
    private readonly IGenericRepository<Merchant> _merchantRepository;
    private readonly IApplicationUserService _applicationUserService;
    private readonly ILogger<PostingBillingConsumer> _logger;
    private readonly IBus _bus;
    private readonly IParameterService _parameterService;
    private readonly IAccountingService _accountingService;
    public PostingBillingConsumer(
        IGenericRepository<PostingBill> postingBillRepository,
        IApplicationUserService applicationUserService,
        IGenericRepository<PostingBalance> postingBalanceRepository,
        IGenericRepository<Merchant> merchantRepository,
        ILogger<PostingBillingConsumer> logger,
        IBus bus,
        IParameterService parameterService, IAccountingService accountingService)
    {
        _postingBillRepository = postingBillRepository;
        _applicationUserService = applicationUserService;
        _postingBalanceRepository = postingBalanceRepository;
        _merchantRepository = merchantRepository;
        _logger = logger;
        _bus = bus;
        _parameterService = parameterService;
        _accountingService = accountingService;
    }

    public async Task Consume(ConsumeContext<PostingBillingJob> context)
    {
        try
        {
            var billMonth = DateTime.Now.Month;
            var bills = await _postingBalanceRepository
                .GetAll()
                .Include(i => i.Merchant)
                .Where(w =>
                    w.PaymentDate.Month == billMonth
                    && w.BatchStatus == BatchStatus.Completed
                    && w.MoneyTransferStatus == PostingMoneyTransferStatus.PaymentSucceeded
                )
                .GroupBy(b => new { b.MerchantId, b.Currency })
                .Select(g => new PostingBill
                {
                    MerchantId = g.Key.MerchantId,
                    Currency = g.Key.Currency,
                    BillMonth = billMonth,
                    BillYear = DateTime.Now.Year,
                    BillDate = DateTime.Now.Date,
                    TotalAmount = g.Sum(s => s.TotalAmount),
                    TotalDueAmount = g.Sum(s => s.TotalDueAmount),
                    TotalPayingAmount = g.Sum(s => s.TotalPayingAmount),
                    TotalPfCommissionAmount = g.Sum(s => s.TotalPfCommissionAmount),
                    TotalBankCommissionAmount = g.Sum(s => s.TotalBankCommissionAmount),
                    CreatedBy = _applicationUserService.ApplicationUserId.ToString(),
                    RecordStatus = RecordStatus.Active
                })
                .ToListAsync();

            decimal bsmvRate = 5.0m;
            try
            {
                var bsmvRateParameter = await _parameterService.GetParameterAsync("Comission", "BsmvRate");
                if (bsmvRateParameter is null || !decimal.TryParse(bsmvRateParameter.ParameterValue, out bsmvRate))
                {
                    bsmvRate = 5.0m;
                }
            }
            catch (Exception)
            {
                bsmvRate = 5.0m;
            }

            foreach (var bill in bills)
            {
                try
                {
                    decimal bsmvAmount = 0.00m;
                    var merchant = await _merchantRepository.GetByIdAsync(bill.MerchantId);

                    bill.ClientReferenceId = Guid.NewGuid();

                    await _postingBillRepository.AddAsync(bill);

                    var profit = bill.TotalPfCommissionAmount - bill.TotalBankCommissionAmount;

                    if (profit > 0)
                    {
                        var bsmvCommision = (bsmvRate / 100.0m) + 1.0m;

                        bsmvAmount =  profit / bsmvCommision* (bsmvRate / 100.0m);
                    }

                    var customerCodeInitial = await _accountingService.GetCustomerCodeInitialAsync();

                    var accountingPayment = new AccountingPayment
                    {
                        AccountingCustomerType = AccountingCustomerType.PF,
                        Amount = bill.TotalPfCommissionAmount,
                        HasCommission = false,
                        Source = $"{customerCodeInitial}{merchant.Number}",
                        Destination = "",
                        CurrencyCode = "TRY",
                        OperationType = OperationType.PfCustomerInvoice,
                        UserId = _applicationUserService.ApplicationUserId,
                        TransactionDate = DateTime.Now,
                        ClientReferenceId = bill.ClientReferenceId,
                        BsmvAmount = bsmvAmount,
                    };

                    using var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                    var endpoint = await _bus.GetSendEndpoint(new Uri("exchange:Accounting.SavePayment"));
                    await endpoint.Send(accountingPayment, tokenSource.Token);
                }
                catch (Exception exception)
                {
                    _logger.LogCritical($"ErrorSavingMerchantBillForMerchantId: {bill.MerchantId}" +
                        $"Error: {exception}");
                }
            }
        }
        catch (Exception exception)
        {
            _logger.LogCritical($"ErrorGeneratingMonthlyPostingBill: {exception}");
        }
    }
}