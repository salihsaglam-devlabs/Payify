using LinkPara.Template.Domain.Commons;

namespace LinkPara.Template.Application.Commons.Interfaces;

public interface IDomainEventService
{
    Task Publish(DomainEvent domainEvent);
}
