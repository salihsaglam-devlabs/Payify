using LinkPara.HttpProviders.BusinessParameter;
using LinkPara.HttpProviders.Vault;
using LinkPara.PF.Application.Commons.Helpers;
using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Commons.Models.Posting;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;
using LinkPara.PF.Infrastructure.Persistence;
using LinkPara.SharedModels.BusModels.IntegrationEvents.Accounting;
using LinkPara.SharedModels.BusModels.IntegrationEvents.Accounting.Enums;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using LinkPara.SystemUser;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LinkPara.PF.Infrastructure.Posting;

public class PostingPosBlockageAccountingBatch : IPostingBatch<PostingPosBlockage>
{
    private readonly ILogger<PostingBankBalancerBatch> _logger;
    private readonly PfDbContext _dbContext;
    private readonly IApplicationUserService _applicationUserService;
    private readonly IGenericRepository<PostingBatchStatus> _postingBatchStatusRepository;
    private readonly IBus _bus;
    private readonly IVaultClient _vaultClient;
    private readonly IParameterService _parameterService;
    private readonly IAccountingService _accountingService;

    public PostingPosBlockageAccountingBatch(
        IGenericRepository<PostingBatchStatus> postingBatchStatusRepository,
        ILogger<PostingBankBalancerBatch> logger,
        IApplicationUserService applicationUserService,
        PfDbContext dbContext,
        IBus bus,
        IVaultClient vaultClient,
        IParameterService parameterService, 
        IAccountingService accountingService)
    {
        _logger = logger;
        _dbContext = dbContext;
        _applicationUserService = applicationUserService;
        _postingBatchStatusRepository = postingBatchStatusRepository;
        _bus = bus;
        _vaultClient = vaultClient;
        _parameterService = parameterService;
        _accountingService = accountingService;
    }

    public async Task ExecuteBatchAsync(PostingBatchStatus batchStatus)
    {
        bool isActivePosBlockage;
        try
        {
            isActivePosBlockage = await _vaultClient.GetSecretValueAsync<bool>("PFSecrets", "PostingSettings", "IsActivePosBlockageAccounting");
        }
        catch (Exception exception)
        {
            _logger.LogError($"ErrorOnGetSecretValue Error: {exception}");
            isActivePosBlockage = false;
        }
        
        if (!isActivePosBlockage)
        {
            batchStatus.BatchSummary = "PostingAccountingIsNotActive";
            batchStatus.BatchStatus = BatchStatus.Completed;
            batchStatus.UpdateDate = DateTime.Now;
            batchStatus.IsCriticalError = false;

            await _postingBatchStatusRepository.UpdateAsync(batchStatus);
            return;
        }
        
        var accountingCustomerInitial = await _accountingService.GetCustomerCodeInitialAsync();

        try
        {
            var bankBalances = await _dbContext
                .PostingBankBalance
                .Where(t => t.AccountingStatus == PostingAccountingStatus.PendingPosBlockage)
                .ToListAsync();

            await PublishPosTransactionAccountingAsync(bankBalances,accountingCustomerInitial);

            _dbContext.PostingBankBalance.AttachRange(bankBalances);
            bankBalances.ForEach(s => s.AccountingStatus = PostingAccountingStatus.Completed);
            await _dbContext.SaveChangesAsync();
        }
        catch (Exception exception)
        {
            _logger.LogCritical($"ErrorPostingPosBlockageAccountingBatch\n Error {exception}");
        }
        
        batchStatus.BatchSummary = "PostingAccountingFinished";
        batchStatus.BatchStatus = BatchStatus.Completed;
        batchStatus.UpdateDate = DateTime.Now;
        batchStatus.IsCriticalError = false;

        await _postingBatchStatusRepository.UpdateAsync(batchStatus);
    }

    private async Task PublishPosTransactionAccountingAsync(List<PostingBankBalance> bankBalances, string accountingCustomerInitial)
    {
        var merchantIds = bankBalances.Select(x => x.MerchantId).Distinct().ToList();
        
        var merchants = await _dbContext.Merchant
            .Where(s => merchantIds.Contains(s.Id)).ToListAsync();
        
        foreach (var bankBalance in bankBalances)
        {
            try
            {
                var merchant = merchants.FirstOrDefault(s => s.Id == bankBalance.MerchantId);

                if (merchant is null)
                {
                    throw new NotFoundException(nameof(Merchant), bankBalance.MerchantId);
                }
                
                var bsmvAmount = await BsmvAmountCalculateHelper.CalculateBsmvAmount(bankBalance.TotalPfNetCommissionAmount, _parameterService);
                var accountingPayment = new AccountingPayment
                {
                    AccountingCustomerType = AccountingCustomerType.PF,
                    AccountingTransactionType = AccountingTransactionType.PfBankBalance,
                    Amount = bankBalance.TotalAmount + bankBalance.TotalReturnAmount, //TotalAuth + TotalPostAuth
                    ReturnAmount = bankBalance.TotalReturnAmount,//TotalReturn
                    CommissionAmount = bankBalance.TotalPfCommissionAmount,//Total Brüt Komisyon
                    BankCommissionAmount = bankBalance.TotalBankCommissionAmount,//Total Banka Komisyon
                    BsmvAmount = bsmvAmount,//Profit BSMV
                    BankCode = bankBalance.AcquireBankCode,
                    HasCommission = false,
                    Source = string.Empty,
                    Destination = $"{accountingCustomerInitial}{merchant.Number}",
                    CurrencyCode = "TRY",
                    OperationType = OperationType.PfPosBlockage,
                    UserId = _applicationUserService.ApplicationUserId,
                    TransactionDate = bankBalance.TransactionDate
                };

                using var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                var endpoint = await _bus.GetSendEndpoint(new Uri("exchange:Accounting.SavePayment"));
                await endpoint.Send(accountingPayment, tokenSource.Token);
            }
            catch (Exception exception)
            {
                _logger.LogCritical($"ErrorSavingPosBlockageAccountingMerchantId: {bankBalance.MerchantId} BankCode: {bankBalance.AcquireBankCode} ,\n Error {exception}");
            }
        }
    }
}
