using LinkPara.HttpProviders.Vault;
using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Commons.Models.BankLimits;
using LinkPara.PF.Application.Commons.Models.Limit;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.BusModels.Commands.PF;
using LinkPara.SharedModels.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;


namespace LinkPara.PF.Infrastructure.Consumers;

public class DecrementLimitsConsumer : IConsumer<IncrementLimits>
{
    private readonly ILimitService _limitService;
    private readonly IBankLimitService _bankLimitService;
    private readonly IBankHealthCheckService _bankHealthCheckService;
    private readonly IGenericRepository<MerchantTransaction> _merchantTransactionRepository;
    private readonly IGenericRepository<AcquireBank> _acquireBankRepository;
    private readonly IBankHealthCheckTransactionService _bankHealthCheckTransactionService;
    private readonly IVaultClient _vaultClient;
    public DecrementLimitsConsumer(ILimitService limitService,
        IGenericRepository<MerchantTransaction> merchantTransactionRepository,
        IBankLimitService bankLimitService,
        IBankHealthCheckService bankHealthCheckService,
        IGenericRepository<AcquireBank> acquireBankRepository,
        IBankHealthCheckTransactionService bankHealthCheckTransactionService,
        IVaultClient vaultClient)
    {
        _limitService = limitService;
        _merchantTransactionRepository = merchantTransactionRepository;
        _bankLimitService = bankLimitService;
        _bankHealthCheckService = bankHealthCheckService;
        _acquireBankRepository = acquireBankRepository;
        _bankHealthCheckTransactionService = bankHealthCheckTransactionService;
        _vaultClient = vaultClient;
    }
    public async Task Consume(ConsumeContext<IncrementLimits> context)
    {
        var merchantTransaction = await _merchantTransactionRepository
                                 .GetAll()
                                 .Where(b => b.Id == context.Message.MerchantTransactionId)
                                 .FirstOrDefaultAsync();

        var decreaseMerchantLimitRequest = new DecreaseMerchantLimitRequest
        {
            Amount = merchantTransaction.Amount,
            MerchantId = merchantTransaction.MerchantId,
            TransactionDate = merchantTransaction.TransactionDate,
            TransactionType = merchantTransaction.TransactionType,
            SubMerchantId = merchantTransaction.SubMerchantId
        };

        await _limitService.DecrementMerchantDailyUsageAsync(decreaseMerchantLimitRequest);
        await _limitService.DecrementMerchantMonthlyUsageAsync(decreaseMerchantLimitRequest);

        if (merchantTransaction.SubMerchantId is not null)
        {
            await _limitService.DecrementSubMerchantDailyUsageAsync(decreaseMerchantLimitRequest);
            await _limitService.DecrementSubMerchantMonthlyUsageAsync(decreaseMerchantLimitRequest);
        }

        var isHealthCheckEnabled = _vaultClient
         .GetSecretValue<bool>("SharedSecrets", "ServiceState", "HealthCheckEnabled");

        if (!isHealthCheckEnabled)
        {
            return;
        }

        var acquireBank = await _acquireBankRepository
                        .GetAll()
                        .Where(b => b.BankCode == merchantTransaction.AcquireBankCode && b.RecordStatus == RecordStatus.Active)
                        .FirstOrDefaultAsync();

        await _bankHealthCheckTransactionService.SaveAsync(merchantTransaction);

        //bank limits

        var bankLimitType = BankLimitType.AllTransaction;
        if (merchantTransaction.InstallmentCount > 1)
        {
            bankLimitType = BankLimitType.Installment;
        }
        else if (merchantTransaction.IsInternational == true)
        {
            bankLimitType = BankLimitType.International;
        }
        else if (merchantTransaction.CardTransactionType == CardTransactionType.OnUs)
        {
            bankLimitType = BankLimitType.OnUs;
        }
        else if (merchantTransaction.CardTransactionType == CardTransactionType.NotOnUs)
        {
            bankLimitType = BankLimitType.NotOnUs;
        }

        var bankLimitRequest = new UpdateBankLimitRequest
        {
            AcquireBankId = acquireBank.Id,
            BankLimitType = bankLimitType,
            Amount = merchantTransaction.Amount
        };

        await _bankLimitService.DecrementLimitAsync(bankLimitRequest);

        await _bankHealthCheckService.UpdateHealthCheckAsync(acquireBank.Id);
    }
}
