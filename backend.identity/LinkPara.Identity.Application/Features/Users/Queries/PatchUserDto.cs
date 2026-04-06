using LinkPara.Identity.Application.Common.Mappings;
using LinkPara.Identity.Domain.Enums;
using LinkPara.Identity.Domain.Entities;
using AutoMapper;

namespace LinkPara.Identity.Application.Features.Users.Queries;

public class PatchUserDto : IMapFrom<User>
{
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public string PhoneCode { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public UserStatus UserStatus { get; set; }
    public List<Guid> Roles { get; set; }
    public DateTime BirthDate { get; set; }
    public string IdentityNumber { get; set; }
    
    public void Mapping(Profile profile)
    {
        profile.CreateMap<User, PatchUserDto>()
            .ForMember(
                destination => destination.Roles,
                options => options.MapFrom(
                    source => source.Roles.Select(detail => detail.Id).ToList())
            ).ReverseMap();
    }
}