using LinkPara.Billing.Application.Commons.Interfaces;
using LinkPara.Billing.Application.Features.Reconciliations.Commands.ReconciliationJob;
using LinkPara.Billing.Domain.Entities;
using LinkPara.SharedModels.BusModels.Commands.Scheduler;
using LinkPara.SharedModels.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LinkPara.Billing.Infrastructure.Consumers.CronJobs;

public class ReconciliationConsumer : IConsumer<BillingReconcilition>
{
    private readonly IGenericRepository<Vendor> _vendorRepository;
    private readonly ILogger<ReconciliationConsumer> _logger;
    private readonly IBillingService _billingService;

    public ReconciliationConsumer(
        IGenericRepository<Vendor> vendorRepository,
        ILogger<ReconciliationConsumer> logger,
        IBillingService billingService)
    {
        _vendorRepository = vendorRepository;
        _logger = logger;
        _billingService = billingService;
    }

    public async Task Consume(ConsumeContext<BillingReconcilition> context)
    {
        try
        {
            var vendorIds = await _vendorRepository.GetAll()
                .Where(v => v.RecordStatus == RecordStatus.Active)
                .Select(v => v.Id)
                .ToListAsync();

            foreach (var vendorId in vendorIds)
            {
                var reconciliationDate = DateTime.Now.AddDays(-1).Date;
                var request = new ReconciliationJobCommand
                {
                    VendorId = vendorId,
                    ReconciliationDate = reconciliationDate
                };

                await _billingService.DoReconciliationAsync(request);
            }
        }
        catch (Exception exception)
        {
            _logger.LogError("ErrorDuringReconciliationJob: {exception}", exception);
        }
    }
}