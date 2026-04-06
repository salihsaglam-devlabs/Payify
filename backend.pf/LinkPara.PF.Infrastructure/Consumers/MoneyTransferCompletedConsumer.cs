using LinkPara.Cache;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.BusModels.IntegrationEvents.Accounting.Enums;
using LinkPara.SharedModels.BusModels.IntegrationEvents.Accounting;
using LinkPara.SharedModels.BusModels.IntegrationEvents.MoneyTransfer;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using LinkPara.SystemUser;
using LinkPara.SharedModels.Persistence;
using LinkPara.HttpProviders.Vault;
using LinkPara.HttpProviders.BusinessParameter;
using LinkPara.HttpProviders.MoneyTransfer;
using LinkPara.HttpProviders.MoneyTransfer.Models;
using LinkPara.PF.Application.Commons.Helpers;
using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Commons.Models.Posting;
using LinkPara.SharedModels.Banking.Enums;
using LinkPara.SharedModels.BusModels.Commands.BTrans;
using LinkPara.SharedModels.Exceptions;
using TransactionSource = LinkPara.SharedModels.Banking.Enums.TransactionSource;

namespace LinkPara.PF.Infrastructure.Consumers;

public class MoneyTransferCompletedConsumer : IConsumer<TransferCompleted>
{
    private readonly IGenericRepository<PostingBankBalance> _bankBalanceRepository;
    private readonly IGenericRepository<PostingBalance> _balanceRepository;
    private readonly ILogger<MoneyTransferCompletedConsumer> _logger;
    private readonly IApplicationUserService _applicationUserService;
    private readonly IBus _bus;
    private readonly IVaultClient _vaultClient;
    private readonly IParameterService _parameterService;
    private readonly IGenericRepository<MerchantBankAccount> _merchantBankAccountRepository;
    private readonly IGenericRepository<Merchant> _merchantRepository;
    private readonly IAccountingService _accountingService;
    private readonly ISourceBankAccountService _sourceBankAccountService;
    private readonly ICacheService _cacheService;
    private readonly IGenericRepository<Currency> _currencyRepository;

    public MoneyTransferCompletedConsumer(
        ILogger<MoneyTransferCompletedConsumer> logger,
        IApplicationUserService applicationUserService,
        IBus bus,
        IGenericRepository<PostingBankBalance> bankBalanceRepository,
        IGenericRepository<PostingBalance> balanceRepository,
        IVaultClient vaultClient,
        IParameterService parameterService, 
        IGenericRepository<MerchantBankAccount> merchantBankAccountRepository, 
        IAccountingService accountingService, 
        ICacheService cacheService, 
        ISourceBankAccountService sourceBankAccountService, 
        IGenericRepository<Currency> currencyRepository, 
        IGenericRepository<Merchant> merchantRepository)
    {
        _logger = logger;
        _applicationUserService = applicationUserService;
        _bus = bus;
        _bankBalanceRepository = bankBalanceRepository;
        _balanceRepository = balanceRepository;
        _vaultClient = vaultClient;
        _parameterService = parameterService;
        _merchantBankAccountRepository = merchantBankAccountRepository;
        _accountingService = accountingService;
        _cacheService = cacheService;
        _sourceBankAccountService = sourceBankAccountService;
        _currencyRepository = currencyRepository;
        _merchantRepository = merchantRepository;
    }

    public async Task Consume(ConsumeContext<TransferCompleted> context)
    {
        var paymentCompletedDate = DateTime.Now;
        var transferResponse = context.Message;
        bool isActivePosUnblockage = false;

        try
        {
            isActivePosUnblockage = await _vaultClient.GetSecretValueAsync<bool>("PFSecrets", "PostingSettings", "IsActivePosUnblockage");
        }
        catch (Exception exception)
        {
            _logger.LogError($"ErrorOnGetScretValue Error:{exception}");
            isActivePosUnblockage = false;
        }
        
        var accountingCustomerInitial = await _accountingService.GetCustomerCodeInitialAsync();

        try
        {
            var balancesQuery = _balanceRepository
                .GetAll()
                .Include(i => i.Merchant)
                .Include(i => i.PostingBankBalances)
                .ThenInclude(i => i.Merchant)
                .ThenInclude(m => m.MerchantBankAccounts)
                .Where(w => w.TransactionSourceId == transferResponse.TransactionSourceReferenceId );

            if (!transferResponse.IncomingTransferCompleted)
            {
                balancesQuery = balancesQuery.Where(s => s.PostingPaymentChannel != PostingPaymentChannel.Wallet);
            }
            
            var balances = await balancesQuery.ToListAsync();

            if (!balances.Any())
            {
                _logger.LogCritical($"MoneyTransferCompletedButReferenceNotFound: {transferResponse.TransactionSourceReferenceId}");

                return;
            }
            
            var distinctMerchantIds = balances.Select(s => s.MerchantId).Distinct().ToList();
            var merchantBankAccounts = await _merchantBankAccountRepository.GetAll()
                .Include(s => s.Bank)
                .Where(s => distinctMerchantIds.Contains(s.MerchantId)).ToListAsync();
            var merchants = await _merchantRepository.GetAll().Where(s =>  distinctMerchantIds.Contains(s.Id)).ToListAsync();
            
            var balanceIds = balances.Select(s => s.Id).ToList();
            var groupedMerchantBalances = balances.Where(s => balanceIds.Contains(s.Id)).GroupBy(s => new {s.MerchantId, s.Iban, s.WalletNumber, s.MoneyTransferBankCode, s.Currency});
            
            var sourceBankAccounts = await _cacheService.GetOrCreateAsync("BTransSourceBankAccounts",
                async () => await _sourceBankAccountService.GetAllSourceBankAccountsAsync(
                    new GetSourceBankAccountsRequest
                    {
                        Source = TransactionSource.PF,
                        AccountType = BankAccountType.UsageAccount,
                        RecordStatus = RecordStatus.Active
                    }));
            
            var currencyNumbers = balances.Select(a => a.Currency).Distinct().ToList();
            var currencyDict = await _currencyRepository
                .GetAll()
                .Where(s => currencyNumbers.Contains(s.Number))
                .ToDictionaryAsync(s => s.Number);
            
            foreach (var group in groupedMerchantBalances)
            {
                var currency = currencyDict[group.Key.Currency];
                
                var sourceBank = sourceBankAccounts.Items
                    .FirstOrDefault(a =>
                        a.BankCode == group.Key.MoneyTransferBankCode && a.CurrencyCode == currency?.Code);
                
                var bankAccount = merchantBankAccounts.FirstOrDefault(t => t.Iban == group.Key.Iban && t.MerchantId == group.Key.MerchantId) ?? 
                                  merchantBankAccounts.FirstOrDefault(t => t.MerchantId == group.Key.MerchantId && t.RecordStatus == RecordStatus.Active);
                    
                using var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                var endpoint = await _bus.GetSendEndpoint(new Uri("exchange:BTrans.UpdatePosInformationPaymentDate"));
                await endpoint.Send(new UpdatePosInformationPaymentDate
                {
                    RelatedBalanceIds = group.Select(s => s.Id).ToList(),
                    PaymentDate = paymentCompletedDate,
                    ReceiverIban = bankAccount?.Iban,
                    ReceiverBankName = bankAccount?.Bank?.Name,
                    ReceiverBankCode = bankAccount?.BankCode.ToString(),
                    ReceiverWalletNumber = group.Key.WalletNumber,
                    SenderBankName = group.Select(s => s.MoneyTransferBankName).FirstOrDefault(s => !string.IsNullOrEmpty(s)),
                    SenderBankCode = group.Key.MoneyTransferBankCode.ToString(),
                    SenderAccountNumber = sourceBank?.AccountNumber,
                    SenderIbanNumber = sourceBank?.IBANNumber,
                }, tokenSource.Token);
            }

            foreach (var bankBalance in balances.Select(b => b.PostingBankBalances))
            {
                if (isActivePosUnblockage)
                {
                    await PublishPosUnblockageAccountingAsync(bankBalance,paymentCompletedDate,accountingCustomerInitial);
                }
            }
            await PublishBalanceAccountingAsync(balances,paymentCompletedDate,accountingCustomerInitial,merchants);
            await PublishUpdatePaymentDateAsync(balances.Select(s => s.Id).ToList(), paymentCompletedDate);
        }
        catch (Exception exception)
        {
            _logger.LogCritical($"ErrorConsumingMoneyTransferCompletedEvent: {exception}");
        }
    }

    private async Task PublishPosUnblockageAccountingAsync(List<PostingBankBalance> bankBalances, DateTime paymentCompletedDate, string accountingCustomerInitial)
    {
        foreach (var bankBalance in bankBalances)
        {
            try
            {
                var ibanNumber = bankBalance.Merchant?.MerchantBankAccounts?
                    .FirstOrDefault(w => w.RecordStatus == RecordStatus.Active)?.Iban;

                var accountingPayment = new AccountingPayment
                {
                    AccountingCustomerType = AccountingCustomerType.PF,
                    AccountingTransactionType = AccountingTransactionType.PfBankBalance,
                    Amount = bankBalance.TotalAmount,
                    BankCode = bankBalance.AcquireBankCode,
                    HasCommission = false,
                    Source = string.Empty,
                    Destination = $"{accountingCustomerInitial}{bankBalance.Merchant.Number}",
                    CurrencyCode = "TRY",
                    OperationType = OperationType.PfPosUnBlockage,
                    UserId = _applicationUserService.ApplicationUserId,
                    TransactionDate = DateTime.Now,
                    IbanNumber = ibanNumber,
                    MerchantId = bankBalance.MerchantId
                };

                using var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                var endpoint = await _bus.GetSendEndpoint(new Uri("exchange:Accounting.SavePayment"));
                await endpoint.Send(accountingPayment, tokenSource.Token);

                bankBalance.AccountingStatus = PostingAccountingStatus.Completed;
                bankBalance.OldPaymentDate = bankBalance.PaymentDate;
                bankBalance.PaymentDate = paymentCompletedDate;

                await _bankBalanceRepository.UpdateAsync(bankBalance);
            }
            catch (Exception exception)
            {
                _logger.LogCritical($"ErrorSavingBankBalancePaymentForId: {bankBalance.Id}, Error {exception}");

                bankBalance.AccountingStatus = PostingAccountingStatus.Error;

                await _bankBalanceRepository.UpdateAsync(bankBalance);
            }
        }
    }

    private async Task PublishBalanceAccountingAsync(List<PostingBalance> balances, DateTime paymentCompletedDate, string accountingCustomerInitial,List<Merchant> merchants)
    {
        try
        {
            var profit = balances.Sum(x => x.TotalPfNetCommissionAmount);

            decimal bsmvAmount = await BsmvAmountCalculateHelper.CalculateBsmvAmount(profit, _parameterService);


            var accountingPayment = new AccountingPayment
            {
                AccountingCustomerType = AccountingCustomerType.PF,
                AccountingTransactionType = AccountingTransactionType.PfBalance,
                Amount = balances.Sum(b => b.TotalPayingAmount),
                BankCode = balances.FirstOrDefault()?.MoneyTransferBankCode ?? -1,
                HasCommission = false,
                Source = $"{accountingCustomerInitial}{balances.FirstOrDefault()?.Merchant.Number}",
                Destination = string.Empty,
                CurrencyCode = "TRY",
                OperationType = OperationType.PfBalance,
                UserId = _applicationUserService.ApplicationUserId,
                TransactionDate = DateTime.Now,
                CommissionAmount = balances.Sum(b => b.TotalPfCommissionAmount),
                BsmvAmount = bsmvAmount,
                MerchantId = balances.FirstOrDefault()?.MerchantId
            };

            using var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            var endpoint = await _bus.GetSendEndpoint(new Uri("exchange:Accounting.SavePayment"));

            await endpoint.Send(accountingPayment, tokenSource.Token);
            
            bool isActiveDeductionBalance;
            try
            {
                isActiveDeductionBalance = await _vaultClient.GetSecretValueAsync<bool>("PFSecrets", "PostingSettings", "IsActiveDeductionBalanceAccounting");
            }
            catch (Exception exception)
            {
                _logger.LogError($"ErrorOnGetSecretValue Error: {exception}");
                isActiveDeductionBalance = false;
            }
            
            if (isActiveDeductionBalance)
            {
                await PublishDeductionBalanceAccountingAsync(balances,accountingCustomerInitial,merchants);
            }

            balances.ForEach(balance =>
            {
                balance.MoneyTransferStatus = PostingMoneyTransferStatus.PaymentSucceeded;
                balance.AccountingStatus = PostingAccountingStatus.Completed;
                balance.OldPaymentDate = balance.PaymentDate;
                balance.PaymentDate = paymentCompletedDate;
                balance.ProcessingId = null;
            });

            await _balanceRepository.UpdateRangeAsync(balances);
        }
        catch (Exception exception)
        {
            _logger.LogCritical($"ErrorSavingGrandBalancePaymentForTransferReferenceId: {balances.FirstOrDefault()?.MoneyTransferReferenceId}, Error {exception}");

            balances.ForEach(balance =>
            {
                balance.MoneyTransferStatus = PostingMoneyTransferStatus.PaymentSucceeded;
                balance.AccountingStatus = PostingAccountingStatus.Error;
                balance.OldPaymentDate = balance.PaymentDate;
                balance.PaymentDate = paymentCompletedDate;
            });

            await _balanceRepository.UpdateRangeAsync(balances);
        }
    }

    private async Task PublishUpdatePaymentDateAsync(List<Guid> balanceIds, DateTime paymentCompletedDate)
    {
        using var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        var endpoint = await _bus.GetSendEndpoint(new Uri("exchange:PF.PublishTransactionPaymentDate"));
        await endpoint.Send(new PublishTransactionPaymentDate
        {
            PostingBalanceIds = balanceIds,
            PaymentDate = paymentCompletedDate,
        }, tokenSource.Token);
    }
    
    private async Task PublishDeductionBalanceAccountingAsync(List<PostingBalance> postingBalances, string accountingCustomerInitial, List<Merchant> merchants)
    {
        var postingBalanceGroup = postingBalances.GroupBy(x => x.MerchantId);
        
        foreach (var merchantPostingBalances in postingBalanceGroup)
        {
            try
            {
                var merchant = merchants.FirstOrDefault(s => s.Id == merchantPostingBalances.Key);

                if (merchant is null)
                {
                    throw new NotFoundException(nameof(Merchant), merchantPostingBalances.Key);
                }

                var chargebackReturnBalances = merchantPostingBalances.Where(s =>
                        s.PostingBalanceType == PostingBalanceType.ChargebackReturn)
                    .ToList();
                
                var suspiciousReturnBalances = merchantPostingBalances.Where(s =>
                        s.PostingBalanceType == PostingBalanceType.SuspiciousReturn)
                    .ToList();
                
                var chargebackAmount = merchantPostingBalances.Sum(b => b.TotalChargebackAmount + b.TotalChargebackCommissionAmount + b.TotalChargebackTransferAmount);
                var suspiciousAmount = merchantPostingBalances.Sum(b => b.TotalSuspiciousAmount + b.TotalSuspiciousCommissionAmount + b.TotalSuspiciousTransferAmount);
                
                var accountingPayment = new AccountingPayment
                {
                    AccountingCustomerType = AccountingCustomerType.PF,
                    AccountingTransactionType = AccountingTransactionType.PfDeductionBalance,
                    Amount = chargebackAmount + suspiciousAmount,//TotalDeductionAmount
                    HasCommission = false,
                    Source = $"{accountingCustomerInitial}{merchant.Number}",
                    Destination = string.Empty,
                    CurrencyCode = "TRY",
                    OperationType = OperationType.PfDeductionBalance,
                    UserId = _applicationUserService.ApplicationUserId,
                    TransactionDate = DateTime.Now,
                    MerchantId = merchant.Id,
                    ChargebackAmount = chargebackAmount,
                    SuspiciousAmount = suspiciousAmount,
                    DueAmount = merchantPostingBalances.Sum(b => b.TotalDueAmount + b.TotalDueTransferAmount),
                    ChargebackReturnAmount = chargebackReturnBalances.Sum(b => b.TotalAmountWithoutCommissions + b.TotalDueAmount + b.TotalDueTransferAmount + b.TotalExcessReturnAmount + b.TotalExcessReturnOnCommissionAmount + b.TotalExcessReturnTransferAmount),
                    SuspiciousReturnAmount = suspiciousReturnBalances.Sum(b => b.TotalAmountWithoutCommissions + b.TotalDueAmount + b.TotalDueTransferAmount + b.TotalExcessReturnAmount + b.TotalExcessReturnOnCommissionAmount + b.TotalExcessReturnTransferAmount)
                };

                using var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                var endpoint = await _bus.GetSendEndpoint(new Uri("exchange:Accounting.SavePayment"));
                await endpoint.Send(accountingPayment, tokenSource.Token);
            }
            catch (Exception exception)
            {
                _logger.LogCritical($"ErrorSavingGrossBalanceAccountingMerchantId: {merchantPostingBalances.Key} ,\n Error {exception}");
            }
        }
    }
}