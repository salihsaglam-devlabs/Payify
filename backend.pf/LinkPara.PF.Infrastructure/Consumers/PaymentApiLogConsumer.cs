using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Domain.Entities;
using LinkPara.SharedModels.BusModels.Commands.Scheduler;
using LinkPara.SharedModels.Persistence;
using LinkPara.SystemUser;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace LinkPara.PF.Infrastructure.Consumers;

public class PaymentApiLogConsumer : IConsumer<PaymentApiLog>
{
    private readonly IApplicationUserService _applicationUserService;
    private readonly IGenericRepository<MerchantApiLog> _repository;
    private readonly ILogger<PaymentApiLogConsumer> _logger;

    public PaymentApiLogConsumer(IApplicationUserService applicationUserService,
            IGenericRepository<MerchantApiLog> repository,
            ILogger<PaymentApiLogConsumer> logger)
    {
        _applicationUserService = applicationUserService;
        _repository = repository;
        _logger = logger;
    }
    public async Task Consume(ConsumeContext<PaymentApiLog> context)
    {
        var message = context.Message;

        await LogConsumer(message);
    }

    private async Task LogConsumer(PaymentApiLog apiLog)
    {
        try
        {
            var log = new MerchantApiLog
            {
                MerchantId = apiLog.MerchantId,
                PaymentType = apiLog.PaymentType,
                Request = apiLog.Request,
                Response = apiLog.Response,
                ErrorCode = apiLog.ErrorCode ?? "0000",
                ErrorMessage = apiLog.ErrorMessage ?? "Unknown",
                CreatedBy = _applicationUserService.ApplicationUserId.ToString()
            };

            await _repository.AddAsync(log);
        }
        catch (Exception exception)
        {
            _logger.LogError($"PaymentApiLogConsumer detail: \n{exception}");
            throw;
        }        
    }
}
