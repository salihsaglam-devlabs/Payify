using LinkPara.HttpProviders.BusinessParameter;
using LinkPara.SharedModels.BusModels.IntegrationEvents.Logging;
using LinkPara.SoftOtp.Application.Common.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace LinkPara.SoftOtp.Infrastructure.Services;

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
            var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            var endpoint = await _bus.GetSendEndpoint(new Uri("exchange:Log.IntegrationLog"));

            await endpoint.Send(log, cancellationToken.Token);
        }
        catch (Exception exception)
        {
            _logger.LogError($"ErrorPublishingIntegrationLog: {log.Name} - Exception {exception}");
        }
    }
}