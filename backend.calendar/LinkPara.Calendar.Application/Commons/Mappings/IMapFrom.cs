using AutoMapper;

namespace LinkPara.Calendar.Application.Commons.Mappings;

public interface IMapFrom<T>
{
    void Mapping(Profile profile)
    {
        profile.CreateMap(typeof(T), GetType());
    }
}