using LinkPara.Billing.Application.Commons.Interfaces;
using LinkPara.HttpProviders.BusinessParameter;
using LinkPara.SharedModels.BusModels.IntegrationEvents.Logging;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace LinkPara.Billing.Infrastructure.Services.IntegrationLoggingServices;

public class IntegrationLoggerService : IIntegrationLogger
{
    private readonly IBus _bus;
    private readonly ILogger<IntegrationLoggerService> _logger;
    private readonly IParameterService _parameterService;

    private readonly string _loggerParameterGroup = "IntegrationLoggerState";

    public IntegrationLoggerService(IBus bus,
        ILogger<IntegrationLoggerService> logger,
        IParameterService parameterService)
    {
        _bus = bus;
        _logger = logger;
        _parameterService = parameterService;
    }

    public async Task QueueLogAsync(IntegrationLog log)
    {
        try
        {
            var isLogEnabled = await _parameterService.GetParameterAsync(_loggerParameterGroup, log.Name);

            if (isLogEnabled.ParameterValue == "True")
            {
                using var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                var endpoint = await _bus.GetSendEndpoint(new Uri("exchange:Log.IntegrationLog"));
                await endpoint.Send(log, cancellationToken.Token);

            }
        }
        catch (Exception exception)
        {
            _logger.LogError("ErrorPublishingIntegrationLog: {Name} - Exception {exception}", log.Name, exception);
        }
    }
}