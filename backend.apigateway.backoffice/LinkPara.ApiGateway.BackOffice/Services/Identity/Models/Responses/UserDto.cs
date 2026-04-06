using LinkPara.ApiGateway.BackOffice.Services.Identity.Models.Enums;

namespace LinkPara.ApiGateway.BackOffice.Services.Identity.Models.Responses;

public class UserDto
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
    public UserLoginLastActivityDto LoginLastActivity { get; set; }
    public DateTime PasswordModifiedDate { get; set; }
    public string FullName
    {
        get
        {
            return FirstName + " " + LastName;
        }
    }
    public DateTime CreateDate { get; set; }
    public DateTime LockoutEnd { get; set; }
}

public class UserDtoWithRoles : UserDto
{
    public List<RoleDto> Roles { get; set; }
}