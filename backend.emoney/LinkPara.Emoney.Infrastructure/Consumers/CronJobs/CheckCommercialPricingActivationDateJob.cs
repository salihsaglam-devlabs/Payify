using System.Transactions;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.Emoney.Domain.Enums;
using LinkPara.Emoney.Infrastructure.Persistence;
using LinkPara.SharedModels.BusModels.Commands.Scheduler;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LinkPara.Emoney.Infrastructure.Consumers.CronJobs;

public class CheckCommercialPricingActivationDateJob: IConsumer<CheckCommercialPricingActivationDate>
{
    private readonly IServiceScopeFactory _scopeFactory;

    public CheckCommercialPricingActivationDateJob(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }
    
    public async Task Consume(ConsumeContext<CheckCommercialPricingActivationDate> context)
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<EmoneyDbContext>();

        var strategy = dbContext.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(async () =>
        {
            using var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

            var activePricing = await dbContext.PricingCommercial
                .FirstOrDefaultAsync(s =>
                    s.ActivationDate <= DateTime.Now
                    && s.PricingCommercialStatus == PricingCommercialStatus.Waiting);
            
            if (activePricing != null)
            {
                IQueryable<PricingCommercial> currentPricing;
                if (activePricing.PricingCommercialType == PricingCommercialType.All)
                {
                    currentPricing = dbContext.PricingCommercial
                        .Where(s =>
                            s.PricingCommercialStatus == PricingCommercialStatus.InUse);
                }
                else
                {
                    currentPricing = dbContext.PricingCommercial
                        .Where(s =>
                            s.PricingCommercialStatus == PricingCommercialStatus.InUse
                            && (s.PricingCommercialType == PricingCommercialType.All
                                || s.PricingCommercialType == activePricing.PricingCommercialType));
                }
                
                if (currentPricing.Any())
                {
                    foreach (var pricing in currentPricing)
                    {
                        pricing.PricingCommercialStatus = PricingCommercialStatus.Finished;
                        pricing.LastModifiedBy = "CronJob";
                        pricing.UpdateDate = DateTime.Now;
                        dbContext.Update(pricing);
                    }
                }

                activePricing.PricingCommercialStatus = PricingCommercialStatus.InUse;
                activePricing.LastModifiedBy = "CronJob";
                activePricing.UpdateDate = DateTime.Now;
                dbContext.Update(activePricing);

                await dbContext.SaveChangesAsync();
            }
            
            transactionScope.Complete();
        });
    }
}