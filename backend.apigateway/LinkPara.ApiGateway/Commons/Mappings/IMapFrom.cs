namespace LinkPara.ApiGateway.Commons.Mappings;

public interface IMapFrom<T>
{
    public void Mapping(AutoMapper.Profile profile)
    {
        profile.CreateMap(typeof(T), GetType());
    }
}