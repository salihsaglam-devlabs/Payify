using LinkPara.Epin.Application.Commons.Interfaces;
using LinkPara.SharedModels.BusModels.Commands.Scheduler;
using MassTransit;

namespace LinkPara.Epin.Infrastructure.Consumers.CronJobs;

public class ReconciliationConsumer : IConsumer<CheckEpinOrders>
{
    private readonly IReconciliationService _reconciliationService;

    public ReconciliationConsumer(IReconciliationService reconciliationService)
    {
        _reconciliationService = reconciliationService;
    }

    public async Task Consume(ConsumeContext<CheckEpinOrders> context)
    {
        //EveryDay 04:00
        //Get 1 day ago
        var date = DateTime.Now.AddDays(-1).Date;
        await _reconciliationService.ReconciliationAsync(date);
    }
}
