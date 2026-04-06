namespace LinkPara.SharedModels.DomainEvents.Interfaces;

public interface IDomainEventService
{
    Task PublishAsync(DomainEvent domainEvent);
}
