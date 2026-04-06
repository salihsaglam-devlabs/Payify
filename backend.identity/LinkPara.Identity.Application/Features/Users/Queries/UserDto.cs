using AutoMapper;
using LinkPara.HttpProviders.CustomerManagement.Models;
using LinkPara.Identity.Application.Common.Mappings;
using LinkPara.Identity.Domain.Enums;
using LinkPara.Identity.Domain.Entities;
using LinkPara.Identity.Application.Features.Roles.Queries;

namespace LinkPara.Identity.Application.Features.Users.Queries;

public class UserDto : IMapFrom<User>
{
    public Guid Id { get; set; }
    public string Email { get; set; }
    public string PhoneCode { get; set; }
    public string PhoneNumber { get; set; }
    public string UserName { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime BirthDate { get; set; }
    public UserType UserType { get; set; }
    public string IdentityNumber { get; set; }
    public UserStatus UserStatus { get; set; }
    public DateTime PasswordModifiedDate { get; set; }
    public UserLoginLastActivityDto LoginLastActivity { get; set; }
    public DateTime CreateDate { get; set; }
    public DateTime LockoutEnd { get; set; }
    public virtual void Mapping(Profile profile)
    {
        profile.CreateMap<User, UserDto>()
            .ForMember(dest => dest.LockoutEnd, opt => opt
            .MapFrom(src => src.LockoutEnd.HasValue ? src.LockoutEnd.Value.DateTime : DateTime.MinValue));
    }
}

public class UserDtoWithRoles : UserDto
{
    public List<RoleDto> Roles { get; set; }

    public override void Mapping(Profile profile)
    {
        profile.CreateMap<User, UserDtoWithRoles>()
            .ForMember(dest => dest.LockoutEnd, opt => opt
            .MapFrom(src => src.LockoutEnd.HasValue ? src.LockoutEnd.Value.DateTime : DateTime.MinValue));
    }
}

