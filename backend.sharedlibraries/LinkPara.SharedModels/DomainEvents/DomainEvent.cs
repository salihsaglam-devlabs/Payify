namespace LinkPara.SharedModels.DomainEvents;

public interface IHasDomainEvent
{
    public List<DomainEvent> DomainEvents { get; set; }
}

public abstract class DomainEvent
{
    protected DomainEvent()
    {
        DateOccurred = DateTime.Now;
    }

    public bool IsPublished { get; set; }
    public DateTime DateOccurred { get; }
}