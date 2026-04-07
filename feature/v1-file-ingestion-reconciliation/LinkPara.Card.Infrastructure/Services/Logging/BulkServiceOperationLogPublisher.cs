using LinkPara.Card.Application.Commons.Interfaces;
using LinkPara.Card.Application.Commons.Models.Logging;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace LinkPara.Card.Infrastructure.Services.Logging;

public class BulkServiceOperationLogPublisher : IBulkServiceOperationLogPublisher
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<BulkServiceOperationLogPublisher> _logger;

    public BulkServiceOperationLogPublisher(
        IPublishEndpoint publishEndpoint,
        ILogger<BulkServiceOperationLogPublisher> logger)
    {
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task PublishAsync(BulkServiceOperationLog log, CancellationToken cancellationToken = default)
    {
        try
        {
            await _publishEndpoint.Publish(log, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Bulk service operation log publish failed. Service={ServiceName}, Endpoint={EndpointName}, CorrelationId={CorrelationId}",
                log.ServiceName, log.EndpointName, log.CorrelationId);
        }
    }
}
