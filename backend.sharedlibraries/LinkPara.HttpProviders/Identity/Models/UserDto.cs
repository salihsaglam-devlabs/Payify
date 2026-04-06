using LinkPara.HttpProviders.Identity.Models.Enums;

namespace LinkPara.HttpProviders.Identity.Models;

public class UserDto
{
    public Guid Id { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public string UserName { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime BirthDate { get; set; }
    public UserType UserType { get; set; }
    public UserStatus UserStatus { get; set; }
    public string IdentityNumber { get; set; }
}
