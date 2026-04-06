namespace LinkPara.Template.Domain.Commons;

public interface IEntity<TKey>
{
    TKey Id { get; set; }
}
