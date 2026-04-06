using LinkPara.Calendar.Application.Commons.Models;
using LinkPara.SharedModels.DomainEvents;
using LinkPara.SharedModels.DomainEvents.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LinkPara.Calendar.Infrastructure.Services;

public class DomainEventService : IDomainEventService
{
    private readonly ILogger<DomainEventService> _logger;
    private readonly IPublisher _mediator;

    public DomainEventService(ILogger<DomainEventService> logger, IPublisher mediator)
    {
        _logger = logger;
        _mediator = mediator;
    }

    public async Task PublishAsync(DomainEvent domainEvent)
    {
        _logger.LogInformation("Publishing domain event. Event - {Event}", domainEvent.GetType().Name);

        var notification = (INotification)Activator.CreateInstance(typeof(DomainEventNotification<>)
            .MakeGenericType(domainEvent.GetType()), domainEvent)!;

        await _mediator.Publish(notification);
    }
}