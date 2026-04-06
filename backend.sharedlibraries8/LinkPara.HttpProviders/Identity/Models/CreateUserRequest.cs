using LinkPara.HttpProviders.Identity.Models.Enums;

namespace LinkPara.HttpProviders.Identity.Models;

public class CreateUserRequest
{
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public string PhoneCode { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime BirthDate { get; set; }
    public UserType UserType { get; set; }
    public List<Guid> Roles { get; set; }
    public string UserName { get; set; }
    public string Password { get; set; }
    public bool IysPermission { get; set; }
    public string IdentityNumber { get; set; }
    public string AmlReferenceNumber { get; set; }
    public bool? IsBlacklistControl { get; set; }
}
