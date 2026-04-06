using LinkPara.SharedModels.BusModels.Commands.Scheduler;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace LinkPara.Backend.Emoney.PaymentProvider.Commons;

public interface IPaymentApiLog
{
    Task SaveApiLogAsync(PaymentApiLog model);
}

public class PfPaymentApiLog : IPaymentApiLog
{
    private readonly IBus _eventBus;
    private readonly ILogger<PfPaymentApiLog> _logger;

    public PfPaymentApiLog(IBus eventBus,
        ILogger<PfPaymentApiLog> logger)
    {
        _eventBus = eventBus;
        _logger = logger;
    }
    public async Task SaveApiLogAsync(PaymentApiLog model)
    {
        try
        {
            CancellationTokenSource token = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            await _eventBus.Publish<PaymentApiLog>(new
            {
                MerchantId = model.MerchantId,
                Request = model.Request,
                Response = model.Response,
                ErrorCode = model.ErrorCode,
                ErrorMessage = model.ErrorMessage,
                PaymentType = model.PaymentType
            }, token.Token);
        }
        catch (Exception exception)
        {
            _logger.LogError($"PaymentRequestResponseLog detail: \n{exception}");
            throw;
        }
    }
}
