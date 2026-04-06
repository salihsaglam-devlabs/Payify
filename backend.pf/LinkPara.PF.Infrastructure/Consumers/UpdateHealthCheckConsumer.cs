using LinkPara.HttpProviders.Vault;
using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Infrastructure.Services;
using LinkPara.SharedModels.BusModels.Commands.PF;
using LinkPara.SharedModels.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.PF.Infrastructure.Consumers;

public class UpdateHealthCheckConsumer : IConsumer<IncrementLimits>
{
    private readonly IBankHealthCheckService _bankHealthCheckService;
    private readonly IGenericRepository<MerchantTransaction> _merchantTransactionRepository;
    private readonly IGenericRepository<AcquireBank> _acquireBankRepository;
    private readonly IBankHealthCheckTransactionService _bankHealthCheckTransactionService;
    private readonly IVaultClient _vaultClient;

    public UpdateHealthCheckConsumer(
        IGenericRepository<MerchantTransaction> merchantTransactionRepository,
        IBankHealthCheckService bankHealthCheckService,
        IGenericRepository<AcquireBank> acquireBankRepository,
        IBankHealthCheckTransactionService bankHealthCheckTransactionService,
        IVaultClient vaultClient)
    {
        _merchantTransactionRepository = merchantTransactionRepository;
        _bankHealthCheckService = bankHealthCheckService;
        _acquireBankRepository = acquireBankRepository;
        _bankHealthCheckTransactionService = bankHealthCheckTransactionService;
        _vaultClient = vaultClient;
    }

    public async Task Consume(ConsumeContext<IncrementLimits> context)
    {
        var isHealthCheckEnabled = _vaultClient
              .GetSecretValue<bool>("SharedSecrets", "ServiceState", "HealthCheckEnabled");

        if (!isHealthCheckEnabled)
        {
            return;
        }

        var merchantTransaction = await _merchantTransactionRepository
                                 .GetAll()
        .Where(b => b.Id == context.Message.MerchantTransactionId)
        .FirstOrDefaultAsync();

        await _bankHealthCheckTransactionService.SaveAsync(merchantTransaction);

        var acquireBank = await _acquireBankRepository
                         .GetAll()
                         .Where(b => b.BankCode == merchantTransaction.AcquireBankCode && b.RecordStatus == RecordStatus.Active)
                         .FirstOrDefaultAsync();

        await _bankHealthCheckService.UpdateHealthCheckAsync(acquireBank.Id);
    }
}

