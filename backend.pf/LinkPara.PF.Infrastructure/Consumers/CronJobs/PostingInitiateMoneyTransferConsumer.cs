using System.Transactions;
using LinkPara.HttpProviders.BusinessParameter;
using LinkPara.HttpProviders.Calendar;
using LinkPara.HttpProviders.MoneyTransfer.Models;
using LinkPara.PF.Application.Commons.Helpers;
using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Commons.Models.MerchantDeductions;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;
using LinkPara.PF.Infrastructure.Persistence;
using LinkPara.SharedModels.Banking.Enums;
using LinkPara.SharedModels.BusModels.Commands.Scheduler;
using LinkPara.SharedModels.Persistence;
using LinkPara.SystemUser;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LinkPara.PF.Infrastructure.Consumers.CronJobs;
public class PostingInitiateMoneyTransferConsumer : IConsumer<PostingInitiateMoneyTransfer>
{
    private readonly ILogger<PostingInitiateMoneyTransferConsumer> _logger;
    private readonly PfDbContext _dbContext;
    private readonly IApplicationUserService _applicationUserService;
    private readonly IBus _bus;
    private readonly ICalendarService _calendarService;
    private readonly IParameterService _parameterService;
    private readonly IPureSqlStore _pureSqlStore;
    private static string Tenant => Environment.GetEnvironmentVariable("Tenant");
    private const int MoneyTransferPreventionHour = 6;

    public PostingInitiateMoneyTransferConsumer(
        ILogger<PostingInitiateMoneyTransferConsumer> logger,
        IApplicationUserService applicationUserService,
        IBus bus,
        ICalendarService calendarService,
        IParameterService parameterService, 
        PfDbContext dbContext, 
        IPureSqlStore pureSqlStore)
    {
        _logger = logger;
        _applicationUserService = applicationUserService;
        _bus = bus;
        _calendarService = calendarService;
        _parameterService = parameterService;
        _dbContext = dbContext;
        _pureSqlStore = pureSqlStore;
    }

    public async Task Consume(ConsumeContext<PostingInitiateMoneyTransfer> context)
    {
        try
        {
            if (DateTime.Now.Hour < MoneyTransferPreventionHour)
            {
                return;
            }
                
            var defaultTransferHour = await MoneyTransferHourHelper.GetMoneyTransferHourAsync(_parameterService, _logger);
            
            var processingId = await _pureSqlStore.ReservePostingBalancesForMoneyTransferAsync(defaultTransferHour);
            if (processingId == null || processingId == Guid.Empty)
            {
                return;
            }
            
            var balances = await _dbContext.PostingBalance
                .Include(s => s.Merchant)
                .Include(s => s.Merchant.MerchantBankAccounts)
                .Include(s => s.Merchant.MerchantWallets)
                .Where(s => 
                    s.ProcessingId ==  processingId && 
                    s.BatchStatus == BatchStatus.MoneyTransferProcessing)
                .ToListAsync();
            if (balances.Count == 0)
            {
                return;
            }
            
            var merchantIds = balances
                .Select(i => i.MerchantId)
                .Distinct()
                .ToList();
            
            var deductionReservationId = await _pureSqlStore.ReserveMerchantDeductionsAsync(merchantIds);
            var merchantDeductions = new List<MerchantDeduction>();
            var deductionPostingTransactions = new List<PostingTransaction>();
            if (deductionReservationId is not null)
            {
                merchantDeductions = await _dbContext
                    .MerchantDeduction
                    .Where(s =>
                        s.ProcessingId ==  deductionReservationId && 
                        s.DeductionStatus == DeductionStatus.Processing
                    )
                    .OrderBy(o => o.CreateDate)
                    .ToListAsync();

                var merchantTransactionIds = merchantDeductions
                    .Where(s => s.MerchantTransactionId != Guid.Empty)
                    .Select(s => s.MerchantTransactionId)
                    .Distinct().ToList();

                deductionPostingTransactions = await _dbContext.PostingTransaction
                    .Where(s => merchantTransactionIds.Contains(s.MerchantTransactionId)).ToListAsync();
            }

            var deductionResult = DeductionHelper.Calculate(balances, merchantDeductions, deductionPostingTransactions, _applicationUserService.ApplicationUserId);

            var holidayTransferAmountThreshold =
                await MoneyTransferHourHelper.GetMoneyHolidayTransferAmountThresholdAsync(_parameterService, _logger);

            foreach (var merchantId in merchantIds)
            {
                var merchantResult = new DeductionResult
                {
                    DeductionTransactions = deductionResult.DeductionTransactions.Where(b => b.MerchantId == merchantId).ToList(),
                    PostingAdditionalTransactions = deductionResult.PostingAdditionalTransactions.Where(b => b.MerchantId == merchantId).ToList(),
                    MerchantDeductions = deductionResult.MerchantDeductions.Where(b => b.MerchantId == merchantId).ToList(),
                    PostingBalances = deductionResult.PostingBalances.Where(b => b.MerchantId == merchantId).ToList()
                };
                
                try
                {
                    var merchant = merchantResult.PostingBalances.FirstOrDefault()?.Merchant;
                    
                    var today = DateTime.Now.Date;
                    if (
                        (
                            today.DayOfWeek == DayOfWeek.Saturday ||
                            today.DayOfWeek == DayOfWeek.Sunday ||
                            await _calendarService.IsHolidayAsync(today, "TUR") 
                        )
                        &&
                        merchantResult.PostingBalances.Sum(b => b.TotalPayingAmount) > holidayTransferAmountThreshold)
                    {
                        var nextWorkDay = (await _calendarService.NextWorkDayAsync(today, "TUR")).Date;
                        
                        merchantResult.PostingBalances.ForEach(b =>
                        {
                            b.PaymentDate = nextWorkDay;
                            b.ProcessingId = null;
                            b.ProcessingStartedAt = DateTime.MinValue;
                            b.TransactionSourceId = Guid.Empty;
                            b.BatchStatus = BatchStatus.MoneyTransferPostponed;
                        });

                        await SaveChangesWithScopeAsync(merchantResult);
                        continue;
                    }
                    
                    var workingDate = today
                        .AddHours(merchant?.MoneyTransferStartHour ?? defaultTransferHour.Hours)
                        .AddMinutes(merchant?.MoneyTransferStartMinute ?? defaultTransferHour.Minutes);
                    
                    await PublishMoneyTransferAsync(merchant, merchantResult, workingDate);
                }
                catch (Exception exception)
                {
                    _logger.LogCritical($"ErrorSendingMoneyTransferForMerchantWithId: {merchantId}" +
                                        $"Error: {exception}");
                    //no update on balances, this records will temporarily unavailable until money transfer monitor job picked up
                }
            }
        }
        catch (Exception exception)
        {
            _logger.LogError($"PostingMoneyTransferJobError: {exception}");
        }
    }
    private async Task PublishMoneyTransferAsync(Merchant merchant, DeductionResult deductionResult, DateTime workingDate)
    {
        var referenceId = deductionResult.PostingBalances.First().TransactionSourceId;
        var merchantBankAccount = merchant
            .MerchantBankAccounts?
            .FirstOrDefault(w => w.RecordStatus == RecordStatus.Active);

        var merchantWallet = merchant.MerchantWallets?.FirstOrDefault(w => w.RecordStatus == RecordStatus.Active);

        var totalBalance = deductionResult.PostingBalances.Sum(b => b.TotalPayingAmount);

        if (totalBalance <= 0)
        {
            _logger.LogError($"PostingMoneyTransferBatch - TotalBalance below zero: {merchant.Id}");
            deductionResult.PostingBalances.ForEach(b =>
            {
                b.BatchStatus = BatchStatus.Completed;
                b.ProcessingId = null;
                b.ProcessingStartedAt = DateTime.MinValue;
                b.TransactionSourceId = Guid.Empty;
                b.MoneyTransferStatus = PostingMoneyTransferStatus.PaymentNotRequired;
            });
            await SaveChangesWithScopeAsync(deductionResult);
            return;
        }

        var transferRequest = new SaveTransferRequest
        {
            UserId = _applicationUserService.ApplicationUserId,
            Amount = RoundDecimal(totalBalance, 2),
            CurrencyCode = "TRY",
            ReceiverName = merchant.Name,
            Source = TransactionSource.PF,
            TransactionSourceReferenceId = referenceId,
            IsReturnPayment = false,
            WorkingDate = workingDate,
            Description = $"{Tenant} - {workingDate.Date}"
        };

        if (merchant.PostingPaymentChannel == PostingPaymentChannel.Wallet)
        {
            if (merchantWallet is null)
            {
                _logger.LogError($"MerchantWalletNotFoundForMerchant: {merchant.Id}");
                deductionResult.PostingBalances.ForEach(b =>
                {
                    b.BatchStatus = BatchStatus.MoneyTransferError;
                    b.ProcessingId = null;
                    b.ProcessingStartedAt = DateTime.MinValue;
                    b.TransactionSourceId = Guid.Empty;
                    b.MoneyTransferStatus = PostingMoneyTransferStatus.PaymentError;
                    b.MoneyTransferPaymentDate = workingDate;
                });
                await SaveChangesWithScopeAsync(deductionResult);
                return;
            }

            transferRequest.WalletNumber = merchantWallet.WalletNumber;
        }
        else
        {
            if (merchantBankAccount is null)
            {
                _logger.LogError($"MerchantBankAccountNotFoundForMerchant: {merchant.Id}");
                deductionResult.PostingBalances.ForEach(b =>
                {
                    b.BatchStatus = BatchStatus.MoneyTransferError;
                    b.ProcessingId = null;
                    b.ProcessingStartedAt = DateTime.MinValue;
                    b.TransactionSourceId = Guid.Empty;
                    b.MoneyTransferStatus = PostingMoneyTransferStatus.PaymentError;
                    b.MoneyTransferPaymentDate = workingDate;
                });
                await SaveChangesWithScopeAsync(deductionResult);
                return;
            }

            transferRequest.ReceiverIBAN = merchantBankAccount.Iban;
        }

        deductionResult.PostingBalances.ForEach(b =>
        {
            b.TotalPayingAmount = RoundDecimal(b.TotalPayingAmount, 2);
            b.BatchStatus = BatchStatus.Completed;
            b.MoneyTransferStatus = PostingMoneyTransferStatus.PaymentInitiated;
            b.MoneyTransferPaymentDate = workingDate;
            b.PostingPaymentChannel = merchant.PostingPaymentChannel;
            b.WalletNumber = merchantWallet?.WalletNumber;
            b.Iban = merchantBankAccount?.Iban;
            b.ProcessingId = null;
        });

        await SaveChangesWithScopeAsync(deductionResult);

        using var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        var endpoint = await _bus.GetSendEndpoint(new Uri("exchange:MoneyTransfer.SaveTransfer"));
        await endpoint.Send(transferRequest, cancellationToken.Token);

        var busEndpoint = await _bus.GetSendEndpoint(new Uri("exchange:PF.PostingPfProfit"));
        await busEndpoint.Send(new PostingPfProfitControl(), cancellationToken.Token);
    }

    private static decimal RoundDecimal(decimal value, int decimalPlaces)
    {
        var multiplier = (decimal)Math.Pow(10, decimalPlaces);
        return Math.Round(value * multiplier) / multiplier;
    }

    private async Task SaveChangesWithScopeAsync(DeductionResult deductionResult)
    {
        var strategy = _dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
            _dbContext.DeductionTransaction.AddRange(deductionResult.DeductionTransactions);
            _dbContext.PostingAdditionalTransaction.AddRange(deductionResult.PostingAdditionalTransactions);
            _dbContext.MerchantDeduction.UpdateRange(deductionResult.MerchantDeductions);
            _dbContext.PostingBalance.UpdateRange(deductionResult.PostingBalances);
            await _dbContext.SaveChangesAsync();
            scope.Complete();
        });
    }
}