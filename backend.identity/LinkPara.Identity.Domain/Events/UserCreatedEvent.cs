using LinkPara.SharedModels.DomainEvents;

namespace LinkPara.Identity.Domain.Events;

public class UserCreatedEvent : DomainEvent
{
    public UserCreatedEvent(User user, Dictionary<string, string> parameters = null)
    {
        User = user;
        Parameters = parameters;
    }

    public User User { get; }
    public Dictionary<string, string> Parameters { get; }
}