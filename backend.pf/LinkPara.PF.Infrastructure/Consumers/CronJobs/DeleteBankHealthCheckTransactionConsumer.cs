using LinkPara.HttpProviders.Vault;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Infrastructure.Persistence;
using LinkPara.SharedModels.BusModels.Commands.Scheduler;
using LinkPara.SharedModels.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.PF.Infrastructure.Consumers.CronJobs;

public class DeleteBankHealthCheckTransactionConsumer : IConsumer<DeleteBankHealthCheckTransaction>
{
    private readonly IGenericRepository<BankHealthCheckTransaction> _bankHealthCheckTransactionRepository;
    private readonly PfDbContext _pfDbContext;
    private readonly IVaultClient _vaultClient;

    public DeleteBankHealthCheckTransactionConsumer(
        IGenericRepository<BankHealthCheckTransaction> bankHealthCheckTransactionRepository,
        IVaultClient vaultClient,
        PfDbContext pfDbContext)
    {
        _bankHealthCheckTransactionRepository = bankHealthCheckTransactionRepository;
        _vaultClient = vaultClient;
        _pfDbContext = pfDbContext;
    }

    public async Task Consume(ConsumeContext<DeleteBankHealthCheckTransaction> context)
    {
        var doubleMinute = Convert.ToDouble(_vaultClient.GetSecretValue<string>("PFSecrets", "BankHealthCheckInfo", "RangeMinute"));
        var date = DateTime.Now.AddMinutes(-doubleMinute);

        var bankHealthCheckTransactions = await _bankHealthCheckTransactionRepository
                          .GetAll()
                          .Where(b => b.BankTransactionDate <= date).ToListAsync();

        if (bankHealthCheckTransactions.Count > 0)
        {
            _pfDbContext.RemoveRange(bankHealthCheckTransactions);
            await _pfDbContext.SaveChangesAsync();
        }
    }
}
