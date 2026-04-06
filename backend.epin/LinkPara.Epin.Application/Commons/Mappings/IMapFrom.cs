namespace LinkPara.Epin.Application.Commons.Mappings;

public interface IMapFrom<T>
{
    public void Mapping(AutoMapper.Profile profile)
    {
        profile.CreateMap(typeof(T), GetType());
    }
}