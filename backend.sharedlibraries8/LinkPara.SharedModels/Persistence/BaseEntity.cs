namespace LinkPara.SharedModels.Persistence;

public class BaseEntity : IEntity<Guid>
{
    public Guid Id { get; set; } = SequentialGuid.NewSequentialGuid();
}