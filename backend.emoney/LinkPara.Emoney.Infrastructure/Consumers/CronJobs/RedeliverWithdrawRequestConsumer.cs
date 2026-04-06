using LinkPara.Emoney.Domain.Enums;
using LinkPara.Emoney.Infrastructure.Persistence;
using LinkPara.SharedModels.BusModels.Commands.Scheduler;
using LinkPara.SharedModels.Persistence;
using LinkPara.SystemUser;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LinkPara.Emoney.Infrastructure.Consumers.CronJobs;

public class RedeliverWithdrawRequestConsumer : IConsumer<RedeliverWithdrawRequest>
{
    private readonly ILogger<RedeliverWithdrawRequestConsumer> _logger;
    private readonly IApplicationUserService _applicationUserService;
    private readonly IServiceScopeFactory _scopeFactory;

    public RedeliverWithdrawRequestConsumer(
        ILogger<RedeliverWithdrawRequestConsumer> logger,
        IApplicationUserService applicationUserService,
        IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _applicationUserService = applicationUserService;
        _scopeFactory = scopeFactory;
    }

    public async Task Consume(ConsumeContext<RedeliverWithdrawRequest> context)
    {
        using var scope = _scopeFactory.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<EmoneyDbContext>();

        var list = await dbContext.WithdrawRequest
            .Where(s =>
                s.RecordStatus == RecordStatus.Active &&
                s.WithdrawStatus == WithdrawStatus.NotDelivered)
            .ToListAsync();

        foreach (var item in list)
        {
            try
            {
                item.WithdrawStatus = WithdrawStatus.Pending;
                item.UpdateDate = DateTime.Now;
                item.LastModifiedBy = _applicationUserService.ApplicationUserId.ToString();

                dbContext.Update(item); 

                await dbContext.SaveChangesAsync();
            }
            catch (Exception exception)
            {
                _logger.LogError($"RedeliverWithdrawRequest Error : {exception}");
            }
        }
    }
}