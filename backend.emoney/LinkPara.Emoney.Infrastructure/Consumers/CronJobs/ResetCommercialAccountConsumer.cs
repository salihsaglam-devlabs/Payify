using System.Transactions;
using LinkPara.Emoney.Application.Commons.Exceptions;
using LinkPara.SharedModels.BusModels.Commands.Scheduler;
using LinkPara.Emoney.Infrastructure.Persistence;
using LinkPara.SharedModels.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LinkPara.Emoney.Infrastructure.Consumers.CronJobs;

public class ResetCommercialAccountConsumer : IConsumer<ResetCommercialAccounts>
{
    private readonly IServiceScopeFactory _scopeFactory;

    public ResetCommercialAccountConsumer(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }
    public async Task Consume(ConsumeContext<ResetCommercialAccounts> context)
    {
        if (DateTime.Now.Day != 1)
        {
            throw new NotFirstDayOfMonthException();
        }
        
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<EmoneyDbContext>();

        var strategy = dbContext.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(async () =>
        {
            using var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
            var commercialAccounts = dbContext.Account
                .Where(a => 
                    a.IsCommercial == true 
                    && a.RecordStatus == RecordStatus.Active)
                .ToList();
            
            if (commercialAccounts.Any())
            {
                commercialAccounts.ForEach(a =>
                {
                    a.IsCommercial = false;
                    a.UpdateDate = DateTime.Now;
                    a.LastModifiedBy = $"BATCH";
                });
            
                dbContext.UpdateRange(commercialAccounts);

                await dbContext.SaveChangesAsync();
            }
            
            transactionScope.Complete();
        });
    }
}