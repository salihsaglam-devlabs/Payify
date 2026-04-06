using LinkPara.HttpProviders.Vault;
using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Commons.Models.BankLimits;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.BusModels.Commands.PF;
using LinkPara.SharedModels.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.PF.Infrastructure.Consumers;

public class IncrementLimitsConsumer : IConsumer<IncrementLimits>
{
    private readonly ILimitService _limitService;
    private readonly IBankLimitService _bankLimitService;
    private readonly IBankHealthCheckService _bankHealthCheckService;
    private readonly IBankHealthCheckTransactionService _bankHealthCheckTransactionService;
    private readonly IGenericRepository<MerchantTransaction> _merchantTransactionRepository;
    private readonly IGenericRepository<AcquireBank> _acquireBankRepository;
    private readonly IVaultClient _vaultClient;
    public IncrementLimitsConsumer(ILimitService limitService,
        IGenericRepository<MerchantTransaction> merchantTransactionRepository,
        IBankLimitService bankLimitService,
        IBankHealthCheckService bankHealthCheckService,
        IGenericRepository<AcquireBank> acquireBankRepository,
        IVaultClient vaultClient,
        IBankHealthCheckTransactionService bankHealthCheckTransactionService)
    {
        _limitService = limitService;
        _merchantTransactionRepository = merchantTransactionRepository;
        _bankLimitService = bankLimitService;
        _bankHealthCheckService = bankHealthCheckService;
        _acquireBankRepository = acquireBankRepository;
        _vaultClient = vaultClient;
        _bankHealthCheckTransactionService = bankHealthCheckTransactionService;
    }
    public async Task Consume(ConsumeContext<IncrementLimits> context)
    {
        var merchantTransaction = await _merchantTransactionRepository
                                  .GetAll()
                                  .Where(b => b.Id == context.Message.MerchantTransactionId)
                                  .FirstOrDefaultAsync();

        await _limitService.IncrementMerchantDailyUsageAsync(merchantTransaction);
        await _limitService.IncrementMerchantMonthlyUsageAsync(merchantTransaction);

        if (merchantTransaction.SubMerchantId is not null)
        {
            await _limitService.IncrementSubMerchantDailyUsageAsync(merchantTransaction);
            await _limitService.IncrementSubMerchantMonthlyUsageAsync(merchantTransaction);
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

        if (merchantTransaction.TransactionType == TransactionType.Return)
        {
            await _bankLimitService.DecrementLimitAsync(bankLimitRequest);
        }

        await _bankLimitService.IncrementLimitAsync(bankLimitRequest);

        await _bankHealthCheckService.UpdateHealthCheckAsync(acquireBank.Id);


    }
}
