using AutoMapper;
using LinkPara.Accounting.Application.Commons.Interfaces;
using LinkPara.Accounting.Domain.Entities;
using LinkPara.SharedModels.BusModels.Commands.Scheduler;
using LinkPara.SharedModels.BusModels.IntegrationEvents.Accounting;
using LinkPara.SharedModels.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace LinkPara.Accounting.Infrastructure.Consumers;

public class RetryFailedPaymentConsumer : IConsumer<RetryFailedPayment>
{
    private readonly IAccountingService _accountingService;
    private readonly IGenericRepository<Payment> _paymentRepository;
    private readonly IConfiguration _configuration;
    private readonly ILogger<RetryFailedPaymentConsumer> _logger;
    public RetryFailedPaymentConsumer(
        IConfiguration configuration,
        ILogger<RetryFailedPaymentConsumer> logger,
        IGenericRepository<Payment> paymentRepository,
        IMapper mapper,
        IServiceProvider serviceProvider,
        IAccountingService accountingService)
    {
        _configuration = configuration;
        _logger = logger;
        _paymentRepository = paymentRepository;
        _accountingService = accountingService;
    }
    public async Task Consume(ConsumeContext<RetryFailedPayment> context)
    {
        var failedCount = _configuration.GetValue<int>("MaxFailedPaymentRetry");

        var payments = await _paymentRepository
            .GetAll()
            .Where(x => !x.IsSuccess &&
            x.FailedPaymentRetryCount < failedCount &&
            x.RecordStatus == RecordStatus.Active)
            .ToListAsync();

        foreach (var payment in payments)
        {
            try
            {
                await _accountingService.PostPaymentAsync(new AccountingPayment
                {
                    ReferenceId = payment.ReferenceId
                });
            }
            catch (Exception exception)
            {
                _logger.LogError("RetryFailedPaymentConsumer Error: {exception}", exception);
            }
        }
    }
}