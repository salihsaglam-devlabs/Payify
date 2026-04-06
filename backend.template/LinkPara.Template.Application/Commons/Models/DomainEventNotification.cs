using LinkPara.Template.Domain.Commons;
using MediatR;

namespace LinkPara.Template.Application.Commons.Models;

public class DomainEventNotification<TDomainEvent> : INotification where TDomainEvent : DomainEvent
{
    public DomainEventNotification(TDomainEvent domainEvent)
    {
        DomainEvent = domainEvent;
    }

    public TDomainEvent DomainEvent { get; }
}