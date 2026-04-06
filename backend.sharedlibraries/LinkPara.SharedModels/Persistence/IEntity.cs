namespace LinkPara.SharedModels.Persistence
{
    public interface IEntity<TKey>
    {
        TKey Id { get; set; }
    }
}