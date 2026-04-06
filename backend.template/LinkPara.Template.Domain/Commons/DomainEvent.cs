namespace LinkPara.Template.Domain.Commons;

public interface IHasDomainEvent
{
    public List<DomainEvent> DomainEvents { get; set; }
}

public abstract class DomainEvent
{
    protected DomainEvent()
    {
        DateOccurred = DateTime.UtcNow;
    }
    
    public bool IsPublished { get; set; }
    public DateTimeOffset DateOccurred { get; }
}