namespace LinkPara.ApiGateway.Card.Commons.Mappings;

public interface IMapFrom<T>
{
    public void Mapping(AutoMapper.Profile profile)
    {
        profile.CreateMap(typeof(T), GetType());
    }
}